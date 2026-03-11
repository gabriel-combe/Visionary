using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Dialog box UI with typewriter effect, choice buttons and controlled progression.
/// </summary>
public class DialogBoxUI : MonoBehaviour
{
    [SerializeField] private TypewriterRunner _typewriter;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Image spriteChara;
    [SerializeField] private GameObject dialogPanel;

    // Choice UI
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;

    private Speaker _currentSpeaker;

    // Choice result
    private int _selectedChoice = -1;
    public int LastSelectedChoice => _selectedChoice;
    public GameObject ChoicesContainer => choicesContainer;

    #region Speaker SFX
    private void playSfx()
    {
        if (_currentSpeaker == null) return;
        var clip = _currentSpeaker.VoiceClip;
        if (clip != null) AudioManager.Instance.SfxSource.PlayOneShot(clip, 1f);
    }
    #endregion

    #region Panel Control
    /// <summary>
    /// Open the dialog panel without setting any speaker yet.
    /// Called once at the start of a conversation.
    /// </summary>
    public void OpenDialog()
    {
        if (dialogPanel != null) dialogPanel.SetActive(true);
    }

    /// <summary>
    /// Update the speaker portrait and name for the current node.
    /// Called by DialogRunner at each node transition.
    /// </summary>
    public void SetSpeaker(Speaker speaker, SpeakerSide side = SpeakerSide.Left)
    {
        _currentSpeaker = speaker;
        if (speaker == null) return;

        if (speakerNameText != null)
        {
            speakerNameText.text = speaker.NpcId;
            speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(speaker.NpcId));
        }

        if (spriteChara != null)
        {
            spriteChara.sprite = speaker.Portrait;
            spriteChara.gameObject.SetActive(speaker.Portrait != null);

            // Flip portrait horizontally when speaker is on the right
            var scale = spriteChara.rectTransform.localScale;
            scale.x = side == SpeakerSide.Right ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            spriteChara.rectTransform.localScale = scale;
        }
    }

    /// <summary>
    /// Hide dialog box
    /// </summary>
    public void HideDialog()
    {
        _typewriter.Stop();
        if (dialogPanel != null) dialogPanel.SetActive(false);
        ClearChoices();
    }
    #endregion

    #region Text
    /// <summary>
    /// Show text with typewriter, paginated by '$', with voice SFX per segment.
    /// </summary>
    public IEnumerator ShowTextAndWaitForClick(string text)
    {
        dialogPanel.SetActive(true);

        yield return StartCoroutine(_typewriter.PlayAndWait(
            dialogText,
            text,
            '$',
            onSegmentStart: () => playSfx(),
            onSegmentEnd: () => AudioManager.Instance.SfxSource.Stop()
        ));
    }
    #endregion

    #region Choices
    /// <summary>
    /// Show a set of choice buttons and wait until the player selects one.
    /// The chosen index will be available in LastSelectedChoice.
    /// </summary>
    public IEnumerator ShowChoicesAndWait(List<(PlayerChoice choice, bool revealed)> options)
    {
        if (choicesContainer == null || choiceButtonPrefab == null) yield break;

        ClearChoices();
        _selectedChoice = -1;

        for (int i = 0; i < options.Count; i++)
        {
            var btnObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            btnObj.gameObject.SetActive(true);
            var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            tmp.richText = true;
            tmp.text = options[i].revealed ? options[i].choice.playerText : "???";

            int index = i;
            btnObj.interactable = options[i].revealed;
            btnObj.onClick.RemoveAllListeners();
            btnObj.onClick.AddListener(() => _selectedChoice = index);
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
    #endregion

    #region Input
    private void Update()
    {
        if (dialogPanel == null || !dialogPanel.activeSelf) return;
        if (Input.GetMouseButtonDown(0))
            _typewriter.HandleClick();
    }
    #endregion
}