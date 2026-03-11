using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PoemUi : MonoBehaviour
{
    [SerializeField] private TypewriterRunner _typewriter;
    [SerializeField] private TextMeshProUGUI poemText;
    [SerializeField] private GameObject poemPanel;

    [Header("Poem Parts")]
    [TextArea(2, 5)]
    [SerializeField] private string poemA;
    [TextArea(2, 5)]
    [SerializeField] private string poemB;
    [TextArea(2, 5)]
    [SerializeField] private string poemC;

    private struct PoemSegment
    {
        public bool canBeGibberish;
        public string text;
    }

    private struct PoemData
    {
        public List<PoemSegment> segments;
        public int gibberishCount;
    }

    private PoemData _poemA, _poemB, _poemC;

    private const string GibberishChars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" +
        "!@#%^&*()_+-=[]{}|;':\",./<>?";

    void Awake()
    {
        if (poemText != null) poemText.text = "";
        _poemA = ParsePoem(poemA);
        _poemB = ParsePoem(poemB);
        _poemC = ParsePoem(poemC);
    }

    #region Parsing

    private static PoemData ParsePoem(string raw)
    {
        var data = new PoemData { segments = new List<PoemSegment>() };
        string[] parts = raw.Split('|');
        for (int i = 0; i < parts.Length; i++)
        {
            bool isGibberish = i % 2 == 1;
            data.segments.Add(new PoemSegment { canBeGibberish = isGibberish, text = parts[i] });
            if (isGibberish) data.gibberishCount++;
        }
        return data;
    }
    #endregion

    #region Public API

    public void ShowPoem()
    {
        if (poemPanel != null) poemPanel.SetActive(true);
    }

    public void HidePoem()
    {
        _typewriter.Stop();
        if (poemPanel != null) poemPanel.SetActive(false);
    }

    public void ResetPoems()
    {
        if (poemText != null) poemText.text = "";
    }

    [ContextMenu("Write Poem A")]
    public void WritePoemA() => StartCoroutine(ShowPoemAAndWaitForClick(0));

    [ContextMenu("Write Poem B")]
    public void WritePoemB() => StartCoroutine(ShowPoemBAndWaitForClick(0));

    public IEnumerator ShowPoemAAndWaitForClick(int amountOfWordsToShow)
        => ShowPoemAndWaitForClick(_poemA, amountOfWordsToShow, leadingNewline: false);

    public IEnumerator ShowPoemBAndWaitForClick(int amountOfWordsToShow)
        => ShowPoemAndWaitForClick(_poemB, amountOfWordsToShow, leadingNewline: true);

    public IEnumerator ShowPoemCAndWaitForClick(int amountOfWordsToShow)
        => ShowPoemAndWaitForClick(_poemC, amountOfWordsToShow, leadingNewline: true);
    #endregion

    #region Core logic

    private IEnumerator ShowPoemAndWaitForClick(PoemData poem, int amountOfWordsToShow, bool leadingNewline)
    {
        ResetPoems();

        // Build the reveal mask
        bool[] reveal = new bool[poem.gibberishCount];
        int revealCount = Mathf.Min(amountOfWordsToShow, poem.gibberishCount);
        for (int i = 0; i < revealCount; i++) reveal[i] = true;
        FisherYates(reveal);

        // Build the display string — $ = newline in poem texts
        string text = leadingNewline ? "\n" : "";
        int gi = 0;
        foreach (var seg in poem.segments)
        {
            if (seg.canBeGibberish)
            {
                text += reveal[gi] ? seg.text : GenerateGibberish(seg.text.Length);
                gi++;
            }
            else
            {
                text += seg.text;
            }
        }


        poemPanel.SetActive(true);

        // $ = newline in poem texts, no pagination
        string processed = text.Replace('$', '\n');
        yield return StartCoroutine(_typewriter.PlayAndWait(poemText, processed, '\0'));
    }
    #endregion

    #region Helpers

    private string GenerateGibberish(int length)
    {
        var sb = new System.Text.StringBuilder(length);
        for (int i = 0; i < length; i++)
            sb.Append('?');
            //sb.Append(GibberishChars[Random.Range(0, GibberishChars.Length)]);
        return sb.ToString();
    }

    private static void FisherYates(bool[] array)
    {
        var rng = new System.Random();
        for (int n = array.Length - 1; n > 0; n--)
        {
            int k = rng.Next(n + 1);
            (array[k], array[n]) = (array[n], array[k]);
        }
    }
    #endregion

    private void Update()
    {
        if (poemPanel == null || !poemPanel.activeSelf) return;
        if (Input.GetMouseButtonDown(0))
            _typewriter.HandleClick();
    }
}