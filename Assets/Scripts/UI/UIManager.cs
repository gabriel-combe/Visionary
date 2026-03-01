using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static SO_DialogNPC;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

/// <summary>
/// Manages all UI panels and navigation.
/// Singleton pattern.
/// </summary>
public class UIManager : SingletonMono<UIManager>
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _startMenu;
    //[SerializeField] private PauseMenuUI _pauseMenu;
    [SerializeField] private DialogBoxUI _dialogBox;
    //[SerializeField] private EndingUI _endingUI;

    [Header("Camera")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _halfwidth = 113.5f;
    [SerializeField] private float _halfheight = 64f;

    [Header("Environment Navigation")]
    [SerializeField] private Button _upArrow;
    [SerializeField] private Button _rightArrow;
    [SerializeField] private Button _downArrow;
    [SerializeField] private Button _leftArrow;

    [Header("Manager")]
    [SerializeField] private throwableManager throwManager;
    [SerializeField] private PoemUi poemManager;

    [Header("Dialog Data")]
    [SerializeField] private NPCData _handOfTheKingData;
    [SerializeField] private List<NPCData> _blackListChoice;
    [SerializeField] private List<NPCData> _mainNPC;
    [SerializeField] private Card _wheelOfFortune;
    [SerializeField] private Card _wheelOfFortuneReversed;

    // logic state
    private bool _isInEncounter = false;
    private int _diceResult = -1;
    private int _ossicleResult = -1;
    private int _currentEnvironmentIndex = 0;
    private bool _ossicleView = true;
    private int _phase = 1; // 1 = first throw, 2 = second throw, 3 = ending
    private List<NPCData> _remainingdMainNPCEncounter;
    private Card _card1 = null;
    private Card _card2 = null;

    // UI state
    private Tween _cameraTweenMove;
    private Tween _cameraTweenRotate;

    // Interaction control
    private bool _awaitingClick = false;
    private bool _clickReceived = false;

    public int CurrentEnvironmentIndex => _currentEnvironmentIndex;
    public bool OssicleView => _ossicleView;

    private void Start()
    {
        throwableManager.diceThrowFinished += OnDiceThrowFinished; // Subscribe to dice throw results
        throwableManager.ossicleThrowFinished += OnOssicleThrowFinished; // Subscribe to ossicle throw results (if needed, can be handled separately)

        // Setup arrow button listeners
        _upArrow.onClick.AddListener(OnUpArrowClicked);
        _rightArrow.onClick.AddListener(OnRightArrowClicked);
        _downArrow.onClick.AddListener(OnDownArrowClicked);
        _leftArrow.onClick.AddListener(OnLeftArrowClicked);

        ResetGame();

        // Start the main sequence of the game
        StartCoroutine(MainSequence());
    }

    #region Environment Navigation
    /// <summary>
    /// Change the current environment index by the specified offset and update camera and arrow visibility accordingly.
    /// </summary>
    /// <param name="offset"></param>
    public void OffsetEnvironmentIndex(int offset)
    {
        _currentEnvironmentIndex += offset;
        Debug.Log($"Environment index changed to {_currentEnvironmentIndex}");
        UpdateCamera();
        UpdateArrowVisibility();
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

    #region UI Panels
    /// <summary>
    /// Show start menu
    /// </summary>
    public void ShowStartMenu()
    {
        HideAllPanels();
        if (_startMenu != null)
        {
            _startMenu.SetActive(true);
        }
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
    #endregion

    /// <summary>
    /// Reset the game state to initial values. This can be called when restarting the game after an ending.
    /// </summary>
    public void ResetGame()
    {
        _currentEnvironmentIndex = 0;
        UpdateCamera();
        UpdateArrowVisibility();
        _remainingdMainNPCEncounter = new List<NPCData>(_mainNPC);
        _card1 = null;
        _card2 = null;
        _diceResult = -1;
        _isInEncounter = false;
        _phase = 1;
        ResetEncounter();
    }

    public void ResetEncounter()
    {
        _diceResult = -1;
        _isInEncounter = false;

        foreach(var npc in FindObjectsByType<NPCData>(FindObjectsSortMode.None))
            npc.ResetEncounters();
    }

    /// <summary>
    /// Toggle the camera view to focus on the ossicle throwing area. When toggle is true, transition to close view; when false, transition back to normal view.
    /// </summary>
    /// <param name="toggle"></param>
    /// <returns></returns>
    IEnumerator ToggleOssicleView(bool toggle)
    {
        _ossicleView = toggle;
        ToggleArrow(!toggle);
        // transition camera to ossicle view close

        _cameraTweenMove?.Kill();
        _cameraTweenRotate?.Kill();

        _cameraTweenMove = _mainCamera.transform.DOMove(toggle ? new Vector3(0, 0, 1) : new Vector3(0, 0, -1), 0.5f).SetEase(Ease.InOutSine);
        _cameraTweenRotate = _mainCamera.transform.DORotate(toggle ? new Vector3(60, 0, 0) : Vector3.zero, 0.5f).SetEase(Ease.InOutSine);

        yield return _cameraTweenMove.WaitForCompletion();
        yield return _cameraTweenRotate.WaitForCompletion();

        // transition camera back to normal view open

        yield break;
    }

    IEnumerator StartMenu()
    {
        // Show start menu and wait for player to click "Start"
        // For simplicity, we just wait for 2 seconds here
        _awaitingClick = true;
        _clickReceived = false;
        yield return new WaitUntil(() => _clickReceived);
        _awaitingClick = false;
        _clickReceived = false;

        Tween fadeIn = _startMenu.GetComponent<CanvasGroup>().DOFade(0, 0.5f).From(1f);

        yield return fadeIn.WaitForCompletion();
    }

    IEnumerator ThrowOssicle(int phase)
    {
        // Show throw UI and wait for player to throw the ossicle
        // For simplicity, we just wait for 2 seconds here
        throwManager.ThrowOssicles();
        yield return new WaitUntil(() => _ossicleResult >= 0);
        poemManager.ShowPoem();
        if (phase == 1)
            yield return StartCoroutine(poemManager.ShowPoemAAndWaitForClick(_ossicleResult));
        else if (phase == 2)
            yield return StartCoroutine(poemManager.ShowPoemBAndWaitForClick(_ossicleResult));
    }

    IEnumerator DeckAnimation(Card card)
    {
        // Show deck animation and wait for it to finish
        // For simplicity, we just wait for 2 seconds here
        yield return new WaitForSeconds(2f);
    }

    IEnumerator EndGame(Card card1, Card card2)
    {
        // Show ending based on the two cards and wait for player to click "Restart"
        // For simplicity, we just wait for 2 seconds here
        yield return new WaitForSeconds(2f);
    }

    /// <summary>
    /// Main sequence of the game. This coroutine orchestrates the flow of the game from start menu, through encounters, to endings, and back to start menu.
    /// It uses a series of yield statements to wait for player interactions and animations to complete before proceeding to the next step.
    /// </summary>
    /// <returns></returns>
    IEnumerator MainSequence()
    {
        // Start Menu
        ToggleArrow(false);
        yield return StartCoroutine(ToggleOssicleView(true));
        yield return StartCoroutine(StartMenu()); // Show start menu and wait for player to start the game

        // Phase 1
        _remainingdMainNPCEncounter = new List<NPCData>(_mainNPC); // Reset the list of main NPCs to encounter
        yield return StartCoroutine(ThrowOssicle(1)); // Show throw UI and wait for player to throw the ossicle
        yield return StartCoroutine(ToggleOssicleView(false));
        _currentEnvironmentIndex = 0;
        OffsetEnvironmentIndex(0); // Ensure we are in the initial environment
        yield return StartCoroutine(SequenceDialog(_handOfTheKingData)); // Start the dialog sequence with the Hand of the King NPC
        yield return new WaitUntil(() => _card1 != null && !_isInEncounter); // Wait until the first card is set (this would be done in the environment where the player can speak with NPCs)
        yield return StartCoroutine(ToggleOssicleView(true));
        yield return StartCoroutine(DeckAnimation(_card1)); // Show deck animation for the first card and wait for it to finish
        ResetEncounter();

        // Phase 2
        _phase = 2;
        _remainingdMainNPCEncounter = new List<NPCData>(_mainNPC); // Reset the list of main NPCs to encounter
        yield return StartCoroutine(ThrowOssicle(2)); // Show throw UI again for the second throw and wait for player to throw the ossicle
        yield return StartCoroutine(ToggleOssicleView(false));
        _currentEnvironmentIndex = 0;
        OffsetEnvironmentIndex(0); // Ensure we are in the initial environment
        yield return StartCoroutine(SequenceDialog(_handOfTheKingData)); // Start the dialog sequence again with the Hand of the King NPC
        yield return new WaitUntil(() => _card2 != null && !_isInEncounter); // Wait until the second card is set (this would be done in the environment where the player can speak with NPCs)
        yield return StartCoroutine(ToggleOssicleView(true));
        yield return StartCoroutine(DeckAnimation(_card2)); // Show deck animation for the second card and wait for it to finish
        ResetEncounter();

        // Ending
        _phase = 3;
        yield return StartCoroutine(ThrowOssicle(3)); // Show throw UI again for the final throw and wait for player to throw the ossicle
        yield return StartCoroutine(ToggleOssicleView(false));
        _currentEnvironmentIndex = 0;
        OffsetEnvironmentIndex(0); // Ensure we are in the initial environment
        yield return StartCoroutine(EndGame(_card1, _card2)); // Start the end of the game based on the combination of the two cards

        // Wait before reseting the game and starting over
        yield return new WaitForSeconds(2f);
        ResetGame();
        ShowStartMenu();
    }

    /*
     PSEUDOCODE / PLAN (detailed):
     - Each dialog path's entries are structured: first come "choice" entries (they have playerText),
       then come "response" entries (they have empty playerText and contain npcText). Responses are at the end.
     - We must show an initial NPC prompt (we take the first response entry if present) before revealing choices.
     - When there are no player choices, immediately show the response chosen according to current phase:
         phase 1 -> choose the last response
         phase 2 -> choose the second-last response
         phase 3 -> choose the third-last response
       If there are fewer responses than needed, clamp to available indices.
     - When there are choices:
         - Start a dice throw and wait for the result.
         - Map the dice (1..20) to a revealCount in 1..N (N = number of choices).
         - Shuffle the choice list, mark the first revealCount as revealed.
         - Ask the dialog box to show choices and wait for player selection.
         - If the player selects a valid choice, show the NPC response chosen according to phase
           (same selection rule as above: last/second-last/third-last).
     - Always clear choices and hide dialog at the end. Reset _isInEncounter flag.
    */

    IEnumerator SequenceDialog(NPCData npcData)
    {
        if (npcData == null || npcData.DialogData == null)
            yield break;

        _isInEncounter = true;
        _diceResult = -1;

        var dialog = npcData.DialogData;

        var path = dialog.DialogPaths[_blackListChoice.Contains(npcData) ? 0 : _phase-1];
        var defaultPath = dialog.DialogPaths[Mathf.Clamp(dialog.DialogPaths.Count - _phase, 0, dialog.DialogPaths.Count)];

        // Show base NPC line (first entry without playerText)
        bool isChoice = false;
        var playerChoices = new List<DialogEntry>();
        for (int i = 0; i < path.entries.Count; i++)
        {
            if (!string.IsNullOrEmpty(path.entries[i].playerText))
            {
                isChoice = true;
                playerChoices.Add(path.entries[i]);
            }
        }

        // Prepare dialog visuals (name, portrait)
        ShowDialog(dialog);

        // Clear previous choices if any
        _dialogBox.ClearChoices();

        Debug.Log($"Has Encontered the {dialog.Name}? -> {npcData.HasEncountered}");
        if (npcData.HasEncountered || !isChoice || _phase == 3)
        {
            yield return StartCoroutine(_dialogBox.ShowTextAndWaitForClick(defaultPath.entries[Random.Range(0, defaultPath.entries.Count)].npcText));

            // Finally hide dialog
            HideDialog();

            _isInEncounter = false;
            yield break;
        }
        else
        {
            npcData.Encountered();
            yield return StartCoroutine(_dialogBox.ShowTextAndWaitForClick(path.entries[0].npcText));
        }

        // Roll a d20 to decide how many among the N choices will be revealed (map 1..20 -> 1..N)
        throwManager.ThrowDice();
        yield return new WaitUntil(() => _diceResult >= 0);
        int revealCount = Mathf.Clamp(Mathf.CeilToInt(_diceResult / 4f), 1, playerChoices.Count);
        Debug.Log($"Rolled a {_diceResult} to reveal {revealCount} choices.");

        var revealedChoices = new List<(DialogEntry, bool, int)>();
        for (int i = 0; i < playerChoices.Count; i++)
        {
            var entry = playerChoices[i];
            if (i == revealCount - 1)
                revealedChoices.Shuffle();
            revealedChoices.Add((entry, i < revealCount, i));
        }

        // Show choices and wait for selection
        yield return StartCoroutine(_dialogBox.ShowChoicesAndWait(revealedChoices));

        int choice = _dialogBox.LastSelectedChoice;
        if (choice < 0 || choice >= revealedChoices.Count)
        {
            HideDialog();
            _isInEncounter = false;
            yield break;
        }

        var selectedEntry = playerChoices[choice];
        Debug.Log($"Player selected choice {choice} (\"{selectedEntry.playerText}\").");

        if (npcData == _handOfTheKingData && choice <= 1)
        {
            if (_phase == 1)
                _card1 = npcData.DialogData.Cards[0];
            else if (_phase == 2)
                _card2 = npcData.DialogData.Cards[1];
        } 
        else if (npcData != _handOfTheKingData && choice >= 3)
        {
            if (_phase == 1)
                _card1 = npcData.DialogData.Cards[0];
            else if (_phase == 2)
                _card2 = npcData.DialogData.Cards[npcData.DialogData.Cards.Count-1];
        }

        // Show NPC response for the chosen line and wait for click to hide
        yield return StartCoroutine(_dialogBox.ShowTextAndWaitForClick(selectedEntry.npcText));

        // Finally hide dialog
        HideDialog();
        _dialogBox.ClearChoices();

        _isInEncounter = false;
    }

    private void Update()
    {
        if (_remainingdMainNPCEncounter != null && _remainingdMainNPCEncounter.Count == 0)
        {
            if (_phase == 1)
                _card1 = _wheelOfFortune;
            else if (_phase == 2)
                _card2 = _wheelOfFortuneReversed;
        }

        if (Input.GetMouseButtonDown(0))
            if (_awaitingClick)
                _clickReceived = true;

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
                _remainingdMainNPCEncounter.Remove(npcData);
            }
        }
    }

    private void OnDiceThrowFinished(int result)
    {
        Debug.Log($"Received dice throw result: {result}");
        _diceResult = result;
    }

    private void OnOssicleThrowFinished(int result)
    {
        Debug.Log($"Received ossicle throw result: {result}");
        _ossicleResult = result;
    }
}

public static class IListExtensions
{
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}