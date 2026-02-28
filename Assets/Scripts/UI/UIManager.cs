using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static SO_DialogNPC;

/// <summary>
/// Manages all UI panels and navigation.
/// Singleton pattern.
/// </summary>
public class UIManager : SingletonMono<UIManager>
{
    // UI Panel references
    //[SerializeField] private StartMenuUI _startMenu;
    //[SerializeField] private PauseMenuUI _pauseMenu;
    [SerializeField] private DialogBoxUI _dialogBox;
    //[SerializeField] private EndingUI _endingUI;
    [SerializeField] private Camera _camera;
    [SerializeField] private float _halfwidth = 113.5f;
    [SerializeField] private float _halfheight = 64f;
    [SerializeField] private Button _upArrow;
    [SerializeField] private Button _rightArrow;
    [SerializeField] private Button _downArrow;
    [SerializeField] private Button _leftArrow;

    private bool _isInEncounter = false;
    private int _diceResult = -1;
    private int _currentEnvironmentIndex = 0;
    private Tween _cameraTween;

    public int CurrentEnvironmentIndex => _currentEnvironmentIndex;

    private void Start()
    {
        //throwablemanager.diceThrowFinished += OnDiceThrowFinished;
        _upArrow.onClick.AddListener(OnUpArrowClicked);
        _rightArrow.onClick.AddListener(OnRightArrowClicked);
        _downArrow.onClick.AddListener(OnDownArrowClicked);
        _leftArrow.onClick.AddListener(OnLeftArrowClicked);

        _currentEnvironmentIndex = 0;
        UpdateCamera();
        UpdateArrowVisibility();
    }

    public void OffsetEnvironmentIndex(int offset)
    {
        _currentEnvironmentIndex += offset;
        Debug.Log($"Environment index changed to {_currentEnvironmentIndex}");
        UpdateCamera();
        UpdateArrowVisibility();
    }

    public void UpdateArrowVisibility()
    {
        _upArrow.gameObject.SetActive(_currentEnvironmentIndex >= 2);
        _rightArrow.gameObject.SetActive(_currentEnvironmentIndex%2 == 0);
        _downArrow.gameObject.SetActive(_currentEnvironmentIndex < 2);
        _leftArrow.gameObject.SetActive(_currentEnvironmentIndex%2 == 1);
    }

    public void OnUpArrowClicked()
    {
        OffsetEnvironmentIndex(-2);
    }

    public void OnRightArrowClicked()
    {
        OffsetEnvironmentIndex(1);
    }

    public void OnDownArrowClicked()
    {
        OffsetEnvironmentIndex(2);
    }

    public void OnLeftArrowClicked()
    {
        OffsetEnvironmentIndex(-1);
    }

    public void UpdateCamera() 
    { 
        Vector3 newPosition = _camera.transform.position;
        newPosition.x = (_currentEnvironmentIndex % 2) * 2 * _halfwidth - _halfwidth;
        newPosition.y = -(_currentEnvironmentIndex / 2) * 2 * _halfheight + _halfheight;
        _cameraTween?.Kill();
        _cameraTween = _camera.transform.DOMove(newPosition, 1f).SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// Show start menu
    /// </summary>
    //public void ShowStartMenu()
    //{
    //    HideAllPanels();
    //    if (startMenu != null)
    //    {
    //        startMenu.gameObject.SetActive(true);
    //        currentActivePanel = startMenu.gameObject;
    //    }
    //}

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

    /// <summary>
    /// Show dialog box with text
    /// </summary>
    /// <param name="text">Dialog text</param>
    /// <param name="speakerName">Name of speaker (optional)</param>
    public void ShowDialog(SO_DialogNPC dialog)
    {
        if (_dialogBox == null) return;
        _dialogBox.gameObject.SetActive(true);
        _dialogBox.ShowDialog(dialog);
    }

    /// <summary>
    /// Hide dialog box
    /// </summary>
    public void HideDialog()
    {
        if (_dialogBox == null) return;

        _dialogBox.HideDialog();
        _dialogBox.gameObject.SetActive(false);
    }

    /// <summary>
    /// Show ending screen
    /// </summary>
    /// <param name="endingData">Data for the ending to display</param>
    //public void ShowEnding(EndingData endingData)
    //{
    //    if (endingUI != null)
    //    {
    //        endingUI.ShowEnding(endingData);
    //        currentActivePanel = endingUI.gameObject;
    //    }
    //}

    /// <summary>
    /// Hide all UI panels
    /// </summary>
    public void HideAllPanels()
    {
        //if (startMenu != null) startMenu.gameObject.SetActive(false);
        //if (pauseMenu != null) pauseMenu.gameObject.SetActive(false);
        if (_dialogBox != null) _dialogBox.HideDialog();
        //if (endingUI != null) endingUI.gameObject.SetActive(false);
    }

    IEnumerator SequenceDialog(NPCData npcData)
    {
        if (npcData == null || npcData.DialogData == null)
            yield break;

        _isInEncounter = true;

        var dialog = npcData.DialogData;

        // Determine which dialog path to use:
        int currentEncounter = npcData.NumberOfEncounters;
        int lastIndex = dialog.DialogPaths.Count - 1;
        int pathIndex = lastIndex;
        if (currentEncounter < lastIndex)
        {
            pathIndex = npcData.NumberOfEncounters;
            npcData.IncrementEncounters();
        }

        var path = dialog.DialogPaths[pathIndex];

        // Show base NPC line (first entry without playerText)
        DialogEntry firstNpcLine = null;
        var playerChoices = new List<DialogEntry>();
        for (int i = 0; i < path.entries.Count; i++)
        {
            var e = path.entries[i];
            if (string.IsNullOrEmpty(e.playerText))
                firstNpcLine ??= e;
            else
                playerChoices.Add(e);
        }

        // Prepare dialog visuals (name, portrait)
        ShowDialog(dialog);

        if (firstNpcLine != null)
            yield return StartCoroutine(_dialogBox.ShowTextAndWaitForClick(firstNpcLine.npcText));

        // Roll a d20 to decide how many among the 5 choices will be revealed (map 1..20 -> 1..5)
        // Call throw dice
        //yield return new WaitUntil(() => _diceResult >= 0);
        int roll = Random.Range(1, 21); // 1 to 20 inclusive
        int revealCount = Mathf.Clamp(Mathf.CeilToInt(roll / 4f), 1, 5);
        Debug.Log($"Rolled a {roll} to reveal {revealCount} choices.");

        var revealedChoices = new List<(string, bool)>();
        var revealedEntries = new List<DialogEntry>();
        for (int i = 0; i < playerChoices.Count; i++)
        {
            var entry = playerChoices[i];
            revealedChoices.Add((entry.playerText, i < revealCount));
            revealedEntries.Add(entry);
        }

        // Show choices and wait for selection
        if (currentEncounter < lastIndex)
            yield return StartCoroutine(_dialogBox.ShowChoicesAndWait(revealedChoices));
        int choice = _dialogBox.LastSelectedChoice;
        if (choice < 0 || choice >= revealedEntries.Count)
        {
            HideDialog();
            _isInEncounter = false;
            yield break;
        }

        var selectedEntry = revealedEntries[choice];

        // Show NPC response for the chosen line and wait for click to hide
        yield return StartCoroutine(_dialogBox.ShowTextAndWaitForClick(selectedEntry.npcText));

        // Finally hide dialog
        HideDialog();

        _isInEncounter = false;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || _isInEncounter) return;

        Debug.Log("Mouse click detected, checking for interactable...");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"Raycast hit: {hit.collider.name}");
            if (hit.collider.CompareTag("Interactable"))
            {
                Debug.Log($"Clicked on interactable: {hit.collider.name}");
                NPCData npcData = hit.collider.GetComponent<NPCData>();
                StartCoroutine(SequenceDialog(npcData));
            }
        }
    }

    private void OnDiceThrowFinished(int result)
    {
        Debug.Log($"Received dice throw result: {result}");
        _diceResult = result;
    }
}