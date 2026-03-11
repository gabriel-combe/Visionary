using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all UI panels and navigation.
/// Singleton pattern.
/// </summary>
public class UIDirector : SingletonMono<UIDirector>
{
    [Header("Game Context")]
    [SerializeField] private GameContext _ctx;

    [Header("Cursor")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    [Header("UI Panels")]
    [SerializeField] private GameObject _startMenu;
    [SerializeField] private GameObject _debutText;
    [SerializeField] private GameObject _endMenu;
    [SerializeField] private GameObject _dayScreen;
    //[SerializeField] private PauseMenuUI _pauseMenu;
    [SerializeField] private GameObject _transitionScreen;

    [Header("Camera")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _halfwidth = 1.135f;
    [SerializeField] private float _halfheight = 0.64f;

    [Header("Environment Navigation")]
    [SerializeField] private Button _upArrow;
    [SerializeField] private Button _rightArrow;
    [SerializeField] private Button _downArrow;
    [SerializeField] private Button _leftArrow;

    [Header("Manager")]
    [SerializeField] private throwableManager _throwManager;
    [SerializeField] private PoemUi _poemManager;
    [SerializeField] private CardDeck _cardDeckManager;
    [SerializeField] private DialogBoxUI _dialogBox;
    [SerializeField] private DialogRunner _dialogRunner;


    // State
    private int _currentEnvironmentIndex = 0;
    private bool _ossicleView = true;
    private bool _enableInteraction = false;
    private bool _isInEncounter = false;

    // Click control (for StartMenu, DebutText, etc.)
    private bool _awaitingClick = false;
    private bool _clickReceived = false;

    // UI state
    private Tween _cameraTweenMove;
    private Tween _cameraTweenRotate;

    public bool EnableInteraction
    {
        get => _enableInteraction;
        set => _enableInteraction = value;
    }

    public bool IsInEncounter => _isInEncounter;

    protected override void Awake()
    {
        base.Awake();
        Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
    }

    private void Start()
    {
        // Setup arrow button listeners
        _upArrow.onClick.AddListener(() => OffsetEnvironmentIndex(-2));
        _rightArrow.onClick.AddListener(() => OffsetEnvironmentIndex(1));
        _downArrow.onClick.AddListener(() => OffsetEnvironmentIndex(2));
        _leftArrow.onClick.AddListener(() => OffsetEnvironmentIndex(-1));

        GameStateManager.Instance.Init(_ctx);
        StartCoroutine(GameStateManager.Instance.TransitionTo(GameStateId.StartMenu));
    }

    private void OnDestroy()
    {
        _upArrow.onClick.RemoveAllListeners();
        _rightArrow.onClick.RemoveAllListeners();
        _downArrow.onClick.RemoveAllListeners();
        _leftArrow.onClick.RemoveAllListeners();
    }

    #region Sequence UI

    public IEnumerator ShowStartMenuAndWait()
    {
        _startMenu.SetActive(true);
        _startMenu.GetComponent<CanvasGroup>().alpha = 1f;
        _awaitingClick = true;
        _clickReceived = false;
        yield return new WaitUntil(() => _clickReceived);
        _awaitingClick = false;
        yield return _startMenu.GetComponent<CanvasGroup>().DOFade(0, 0.5f).WaitForCompletion();
        _startMenu.SetActive(false);
    }

    public IEnumerator ShowDebutTextAndWait()
    {
        _debutText.SetActive(true);
        _debutText.GetComponent<CanvasGroup>().alpha = 1f;
        _awaitingClick = true;
        _clickReceived = false;
        yield return new WaitUntil(() => _clickReceived);
        _awaitingClick = false;
        yield return _debutText.GetComponent<CanvasGroup>().DOFade(0, 0.5f).WaitForCompletion();
        _debutText.SetActive(false);
    }

    public IEnumerator ShowDayScreen(string text)
    {
        _dayScreen.SetActive(true);
        _dayScreen.GetComponentInChildren<TextMeshProUGUI>().text = text;
        yield return _dayScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).From(0f).WaitForCompletion();
        yield return new WaitForSeconds(1f);
        yield return _dayScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).WaitForCompletion();
        _dayScreen.SetActive(false);
    }

    public IEnumerator RunOssiclePhase(int phase)
    {
        yield return new WaitForSeconds(0.5f);
        int ossicleResult = -1;
        Action<OssicleResultEvent> onOssicle = e => ossicleResult = e.UpCount;
        EventBus.Subscribe<OssicleResultEvent>(onOssicle);
        _throwManager.ThrowOssicles();
        yield return new WaitUntil(() => ossicleResult >= 0);
        EventBus.Unsubscribe<OssicleResultEvent>(onOssicle);

        yield return new WaitForSeconds(0.5f);
        _poemManager.ShowPoem();

        if (phase == 1) yield return StartCoroutine(_poemManager.ShowPoemAAndWaitForClick(ossicleResult));
        else if (phase == 2) yield return StartCoroutine(_poemManager.ShowPoemBAndWaitForClick(ossicleResult));
        else yield return StartCoroutine(_poemManager.ShowPoemCAndWaitForClick(ossicleResult));

        _poemManager.HidePoem();
        _throwManager.ClearBoard();
    }

    public void ShowEndMenu()
    {
        _endMenu.SetActive(true);
        _endMenu.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).From(0f);
    }

    /// <summary>
    /// Show pause menu
    /// </summary>
    //public void ShowPauseMenu()
    //{
    //    if (pauseMenu != null)
    //    {
    //        pauseMenu.gameObject.SetActive(true);
    //        currentActivePanel = pauseMenu.gameObject;
    //    }
    //}

    /// <summary>
    /// Hide pause menu
    /// </summary>
    //public void HidePauseMenu()
    //{
    //    if (pauseMenu != null)
    //    {
    //        pauseMenu.gameObject.SetActive(false);
    //        if (currentActivePanel == pauseMenu.gameObject)
    //            currentActivePanel = null;
    //    }
    //}
    #endregion

    #region NPC Click

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (_awaitingClick)
        {
            _clickReceived = true;
            return;
        }

        if (!_enableInteraction || _isInEncounter) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        if (!hit.collider.CompareTag("Interactable")) return;

        var npc = hit.collider.GetComponent<NPCData>();
        if (npc == null || npc.DialogGraph == null) return;

        StartCoroutine(HandleNPCDialog(npc));
    }

    public IEnumerator HandleNPCDialog(NPCData npc)
    {
        _isInEncounter = true;
        ToggleArrow(false);
        _dialogBox.gameObject.SetActive(true);

        yield return StartCoroutine(
            _dialogRunner.RunDialog(npc.DialogGraph, _dialogBox, _throwManager)
        );

        _dialogBox.gameObject.SetActive(false);
        ToggleArrow(true);
        UpdateArrowVisibility();
        _isInEncounter = false;
    }
    #endregion

    #region Environment Navigation
    /// <summary>
    /// Change the current environment index by the specified offset and update camera and arrow visibility accordingly.
    /// </summary>
    /// <param name="offset"></param>
    public void OffsetEnvironmentIndex(int offset)
    {
        _currentEnvironmentIndex += offset;
        EventBus.Emit(new EnvironmentChangedEvent(_currentEnvironmentIndex));
        UpdateCamera();
        UpdateArrowVisibility();
    }

    public void ResetEnvironment()
    {
        _currentEnvironmentIndex = 0;
        OffsetEnvironmentIndex(0);
    }

    /// <summary>
    /// Update the visibility of navigation arrows based on the current environment index.
    /// </summary>
    public void UpdateArrowVisibility()
    {
        _upArrow.gameObject.SetActive(_currentEnvironmentIndex >= 2);
        _rightArrow.gameObject.SetActive(_currentEnvironmentIndex%2 == 0);
        _downArrow.gameObject.SetActive(_currentEnvironmentIndex < 2);
        _leftArrow.gameObject.SetActive(_currentEnvironmentIndex%2 == 1);
    }

    public void ToggleArrow(bool toggle)
    {
        _upArrow.gameObject.SetActive(toggle);
        _rightArrow.gameObject.SetActive(toggle);
        _downArrow.gameObject.SetActive(toggle);
        _leftArrow.gameObject.SetActive(toggle);
    }

    /// <summary>
    /// Update the camera position based on the current environment index. The camera will smoothly transition to the new position.
    /// </summary>
    public void UpdateCamera() 
    { 
        Vector3 newPosition = _mainCamera.transform.position;
        newPosition.x = (_currentEnvironmentIndex % 2) * 2 * _halfwidth - _halfwidth;
        newPosition.y = -(_currentEnvironmentIndex / 2) * 2 * _halfheight + _halfheight;
        _cameraTweenMove?.Kill();
        _cameraTweenMove = _mainCamera.transform.DOMove(newPosition, 1f).SetEase(Ease.InOutSine);
    }
    #endregion

    public IEnumerator ToggleOssicleView(bool toggle)
    {
        _ossicleView = toggle;
        if (toggle) ToggleArrow(false);
        EventBus.Emit(new OssicleViewChangedEvent(toggle));

        _transitionScreen.SetActive(true);
        yield return _transitionScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).From(0f).WaitForCompletion();

        _cameraTweenMove?.Kill();
        _cameraTweenRotate?.Kill();
        _cameraTweenMove = _mainCamera.transform.DOMove(toggle ? new Vector3(0, 0, 1) : new Vector3(-_halfwidth, _halfheight, -1), 1f).SetEase(Ease.InOutSine);
        _cameraTweenRotate = _mainCamera.transform.DORotate(toggle ? new Vector3(60, 0, 0) : Vector3.zero, 1f).SetEase(Ease.InOutSine);
        yield return _cameraTweenMove.WaitForCompletion();

        yield return _transitionScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).WaitForCompletion();
        _transitionScreen.SetActive(false);

        if (!toggle)
        {
            ResetEnvironment();
            ToggleArrow(true);
            UpdateArrowVisibility();
        }
    }

    public void ApplicationQuit()
    {
        // Ensure all tweens are killed when the application quits to prevent errors
        _cameraTweenMove?.Kill();
        _cameraTweenRotate?.Kill();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
        Application.OpenURL("https://ullcarion.itch.io/threads-of-destiny");
#else
        Application.Quit();
#endif
    }
}