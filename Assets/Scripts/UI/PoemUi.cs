using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI poemText;
    [SerializeField] private GameObject poemPanel;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private string poemA;
    [SerializeField] private string poemB;

    private struct substrpoem
    {
        public bool can_be_gibberish;
        public string text;
    }

    private List<substrpoem> secPoemA = new List<substrpoem>();
    private List<substrpoem> secPoemB = new List<substrpoem>();

    int poemAGibberishCount;
    int poemBGibberishCount;

    // Interaction control
    private bool _awaitingClick = false;
    private bool _clickReceived = false;

    private bool isTyping = false;
    private Coroutine typewriterCoroutine;
    private string currentFullText = "";

    // Gibberish text characters list
    string gibberishChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;':\",./<>?!@#$%^&*()_+-=[]{}|;':\",./<>?!@#$%^&*()_+-=[]{}|;':\",./<>?!@#$%^&*()_+-=[]{}|;':\",./<>?";

    void Awake()
    {    
        if (poemText != null)
        {
            poemText.text = "";
        }
        poemAGibberishCount=0;
        poemBGibberishCount=0;
        string[] splitA = poemA.Split("#");
        for(int i = 0;i < splitA.Length; i++)
        {
            substrpoem q;
            if(i%2==1)
            {
                q.can_be_gibberish=true;
                poemAGibberishCount++;
            }
            else
            {
                q.can_be_gibberish=false;
            }
            q.text = splitA[i];
            secPoemA.Add(q);
        }

        string[] splitB = poemB.Split("#");
        for(int i = 0;i < splitB.Length; i++)
        {
            substrpoem q;
            if(i%2==1)
            {
                q.can_be_gibberish=true;
                poemBGibberishCount++;
            }
            else
            {
                q.can_be_gibberish=false;
            }
            q.text = splitB[i];
            secPoemB.Add(q);
        }
    }
    
    public void ShowPoem()
    {
        if (poemPanel != null)
            poemPanel.SetActive(true);
    }
    
    public void HidePoem()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;
        _awaitingClick = false;
        _clickReceived = false;

        if (poemPanel != null)
            poemPanel.SetActive(false);
    }

    private IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;

        foreach (char c in text)
        {
            poemText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        typewriterCoroutine = null;
    }

    public void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;
        poemText.text = currentFullText;
    }

    void fisherYates(bool[] array)
    {
        System.Random rng = new System.Random();
        int n = array.Length;
        
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            bool value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
    }

    [ContextMenu("Write Poem A")] 
    public void WritePoemA()
    {
        StartCoroutine(ShowPoemAAndWaitForClick(0));
    }

    [ContextMenu("Write Poem B")] 
    public void WritePoemB()
    {
        StartCoroutine(ShowPoemBAndWaitForClick(0));
    }

    public IEnumerator ShowPoemAAndWaitForClick(int amountOfWordsToShow)
    {
        bool[] boolArray = new bool[poemAGibberishCount];
        for (int i = 0; i < Mathf.Min(amountOfWordsToShow,poemAGibberishCount); i++)
        {
            boolArray[i] = true;
        }
        fisherYates(boolArray);

        string text = "";
        int j = 0;
        for(int i = 0; i < secPoemA.Count; i++)
        {
            if(secPoemA[i].can_be_gibberish)
            {
                if(!boolArray[j])
                {
                    for(int k = 0; k < secPoemA[i].text.Length; k++)
                    {
                        text += gibberishChars[Random.Range(0, gibberishChars.Length)].ToString();
                    }
                }
                else
                {
                    text += secPoemA[i].text;
                }
                j++;
            }
            else
            {
                text += secPoemA[i].text;
            }
        }
        Debug.Log(text);
        currentFullText=poemText.text+text;

        poemPanel.SetActive(true);
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterEffect(text));

        yield return new WaitUntil(() => !isTyping);

        _awaitingClick = true;
        _clickReceived = false;
        yield return new WaitUntil(() => _clickReceived);
        _awaitingClick = false;
        _clickReceived = false;
    }

    public IEnumerator ShowPoemBAndWaitForClick(int amountOfWordsToShow)
    {
        bool[] boolArray = new bool[poemBGibberishCount];
        for (int i = 0; i < Mathf.Min(amountOfWordsToShow,poemBGibberishCount); i++)
        {
            boolArray[i] = true;
        }
        fisherYates(boolArray);

        string text = "";
        int j = 0;
        for(int i = 0; i < secPoemB.Count; i++)
        {
            if(secPoemB[i].can_be_gibberish)
            {
                if(!boolArray[j])
                {
                    for(int k = 0; k < secPoemB[i].text.Length; k++)
                    {
                        text += gibberishChars[Random.Range(0, gibberishChars.Length)].ToString();
                    }
                }
                else
                {
                    text += secPoemB[i].text;
                }
                j++;
            }
            else
            {
                text += secPoemB[i].text;
            }
        }
        Debug.Log(text);
        currentFullText=poemText.text+text;

        poemPanel.SetActive(true);
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterEffect(text));

        yield return new WaitUntil(() => !isTyping);

        _awaitingClick = true;
        _clickReceived = false;
        yield return new WaitUntil(() => _clickReceived);
        _awaitingClick = false;
        _clickReceived = false;
    }

    private void Update()
    {
        if (poemPanel == null || !poemPanel.activeSelf)
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
