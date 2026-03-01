using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SO_DialogNPC;

/// <summary>
/// Dialog box UI with typewriter effect, choice buttons and controlled progression.
/// </summary>
public class DialogBoxUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Image spriteChara;
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private float typewriterSpeed = 0.05f;

    // Choice UI
    [SerializeField] private GameObject choicesContainer; // parent for buttons
    [SerializeField] private Button choiceButtonPrefab; // prefab containing Button + TextMeshProUGUI

    private bool isTyping = false;
    private Coroutine typewriterCoroutine;
    private string currentFullText = "";
    private SO_DialogNPC _currentDialog;

    // Interaction control
    private bool _awaitingClick = false;
    private bool _clickReceived = false;

    // Choice result
    private int _selectedChoice = -1;
    public int LastSelectedChoice => _selectedChoice;

    // Gibberish text characters list
    string gibberishChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;':\",./<>?!@#$%^&*()_+-=[]{}|;':\",./<>?!@#$%^&*()_+-=[]{}|;':\",./<>?!@#$%^&*()_+-=[]{}|;':\",./<>?";

    /// <summary>
    /// Prepare dialog panel visuals (name, portrait) without starting a text flow.
    /// </summary>
    public void ShowDialog(SO_DialogNPC dialog)
    {
        _currentDialog = dialog;

        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        if (speakerNameText != null)
        {
            speakerNameText.text = dialog.Name;
            speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(dialog.Name));
        }

        if (spriteChara != null)
        {
            spriteChara.sprite = dialog.Portrait;
            spriteChara.gameObject.SetActive(dialog.Portrait != null);
        }
    }

    /// <summary>
    /// Hide dialog box
    /// </summary>
    public void HideDialog()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;
        _awaitingClick = false;
        _clickReceived = false;

        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        ClearChoices();
    }

    /// <summary>
    /// Typewriter coroutine to display text letter by letter
    /// </summary>
    private IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogText.text = "";
        bool italicOpen = false;

        foreach (char c in text)
        {
            if (c == '$')
            {
                // Now wait for a click to proceed
                _awaitingClick = true;
                _clickReceived = false;
                yield return new WaitUntil(() => _clickReceived);
                _awaitingClick = false;
                _clickReceived = false;
                dialogText.text = ""; // Clear text for next segment
                continue; // Skip adding this character to the text
            }

            if (c == '/' && !italicOpen)
            {
                dialogText.text += "<i>";
                continue;
            }
            else if (c == '/')
            {
                dialogText.text += "</i>";
                continue;
            }
            
            dialogText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        italicOpen = false;
        typewriterCoroutine = null;
    }

    /// <summary>
    /// Show a single text line with typewriter and wait until the player clicks to continue.
    /// This will return only after the player has clicked (skip handled).
    /// </summary>
    public IEnumerator ShowTextAndWaitForClick(string text)
    {
        dialogPanel.SetActive(true);

        currentFullText = text;

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterEffect(text));

        // Wait until typewriter finished
        yield return new WaitUntil(() => !isTyping);

        // Now wait for a click to proceed
        _awaitingClick = true;
        _clickReceived = false;
        yield return new WaitUntil(() => _clickReceived);
        _awaitingClick = false;
        _clickReceived = false;
    }

    /// <summary>
    /// Show a set of choice strings (buttons) and wait until the player selects one.
    /// The chosen index will be available in LastSelectedChoice.
    /// </summary>
    public IEnumerator ShowChoicesAndWait(List<(DialogEntry, bool, int)> options)
    {
        if (choicesContainer == null || choiceButtonPrefab == null)
        {
            Debug.LogWarning("Choices UI not configured.");
            yield break;
        }

        ClearChoices();
        _selectedChoice = -1;

        for (int i = 0; i < options.Count; i++)
        {
            var btnObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            btnObj.gameObject.SetActive(true);
            var text = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            text.richText = true; // Enable rich text for italics

            text.text = "???"; // Start with empty text
            //for (int j = 0; j < 30; j++)
            //    text.text += gibberishChars[Random.Range(0, gibberishChars.Length)].ToString(); // Random gibberish text for visual effect

            if (text != null && options[i].Item2)
            {
                text.text = options[i].Item1.playerText;
                text.text = options[i].Item1.playerText.Replace("/", "<i>");
                int place = options[i].Item1.playerText.LastIndexOf("<i>");

                if (place >= 0)
                    text.text = text.text.Remove(place).Insert(place, "</i>");
            }
            int index = options[i].Item3; // Capture index for the listener
            Debug.Log($"Created choice button {index} with text: {text.text} (enabled: {options[i].Item2})");

            btnObj.interactable = options[i].Item2;
            btnObj.onClick.RemoveAllListeners();
            btnObj.onClick.AddListener(() =>
            {
                _selectedChoice = index;
            });
        }

        choicesContainer.SetActive(true);

        yield return new WaitUntil(() => _selectedChoice >= 0);
    }

    public void ClearChoices()
    {
        if (choicesContainer == null) return;

        for (int i = choicesContainer.transform.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.transform.GetChild(i).gameObject);

        choicesContainer.SetActive(false);
    }

    /// <summary>
    /// Skip typewriter and show full text instantly
    /// </summary>
    public void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;
        dialogText.text = currentFullText;
    }

    /// <summary>
    /// Handle input for advancing dialog
    /// </summary>
    private void Update()
    {
        if (dialogPanel == null || !dialogPanel.activeSelf)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
                SkipTypewriter();
            else if (_awaitingClick)
                _clickReceived = true;
        }
    }
}