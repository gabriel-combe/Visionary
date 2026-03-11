using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterRunner : MonoBehaviour
{
    [SerializeField] private float _speed = 0.05f;

    private TMP_Text _target;
    private bool _isTyping;
    private bool _skipRequested;
    private bool _awaitingClick;
    private bool _clickReceived;
    private System.Action _onSegmentEnd;

    public bool IsTyping => _isTyping;
    public float Speed { get => _speed; set => _speed = value; }

    /// <summary>
    /// Display the text segment by segment (separator = pageBreak).
    /// Click 1 : reveals full segment instantly (if typing)
    /// Click 2 : advances to next segment (or ends)
    /// </summary>
    public IEnumerator PlayAndWait(TMP_Text target, string text, char pageBreak = '$',
    System.Action onSegmentStart = null, System.Action onSegmentEnd = null)
    {
        _target = target;
        _onSegmentEnd = onSegmentEnd;
        string[] segments = text.Split(pageBreak);

        foreach (string segment in segments)
        {
            target.text = segment.Trim('\n');
            target.ForceMeshUpdate();

            onSegmentStart?.Invoke();

            _isTyping = true;
            _skipRequested = false;

            yield return StartCoroutine(TypewritterEffect(target));

            _isTyping = false;
            _onSegmentEnd?.Invoke();

            _awaitingClick = true;
            _clickReceived = false;
            yield return new WaitUntil(() => _clickReceived);
            _awaitingClick = false;
        }

        _onSegmentEnd = null;
        _target = null;
    }

    /// <summary>
    /// Reveals characters one by one. Exits early if _skipRequested is set.
    /// </summary>
    private IEnumerator TypewritterEffect(TMP_Text target)
    {
        int total = target.textInfo.characterCount;
        target.maxVisibleCharacters = 0;

        for (int i = 0; i <= total; i++)
        {
            if (_skipRequested)
            {
                target.maxVisibleCharacters = int.MaxValue;
                yield break;
            }

            target.maxVisibleCharacters = i;
            yield return new WaitForSeconds(_speed);
        }
    }

    /// <summary>
    /// Called from the Update of the parent MonoBehaviour.
    /// Returns true if the click was consumed (so it is not propagated).
    /// </summary>
    public bool HandleClick()
    {
        if (_isTyping)
        {
            Skip();
            return true;
        }
        if (_awaitingClick)
        {
            _clickReceived = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Reveals the current segment instantly without breaking the coroutine flow.
    /// The coroutine will then reach _awaitingClick and wait for the next click normally.
    /// </summary>
    public void Skip()
    {
        if (!_isTyping) return;
        _skipRequested = true;
    }

    /// <summary>
    /// Fully stops the typewriter (e.g. when forcibly hiding the panel).
    /// </summary>
    public void Stop()
    {
        StopAllCoroutines();
        _isTyping = false;
        _skipRequested = false;
        _awaitingClick = false;
        _clickReceived = false;
        _onSegmentEnd = null;
        _target = null;
    }
}