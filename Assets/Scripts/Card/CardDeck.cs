using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDeck : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float deckHeight = 0.1f;
    [SerializeField] private int cardsPerDeck = 48;

    [SerializeField] private TextMeshProUGUI cardText;
    [SerializeField] private TextMeshProUGUI cardTitle;
    [SerializeField] private GameObject cardPanel;
    [SerializeField] private float typewriterSpeed = 0.05f;

    // Interaction control
    private bool _awaitingClick = false;
    private bool _clickReceived = false;

    private bool isTyping = false;
    private Coroutine typewriterCoroutine;
    private string currentFullText = "";

    private AudioManager audioManager;

    void Awake()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    private void fisherYates(List<int> array)
    {
        System.Random rng = new System.Random();
        int n = array.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
    }

    public IEnumerator DealCard(Card card)
    {
        // --- Snapshot current transform state (no extra fields) ---
        Transform savedParent = transform.parent;
        Vector3 savedLocalPos = transform.localPosition;
        Quaternion savedLocalRot = transform.localRotation;
        Vector3 savedLocalScale = transform.localScale;

        currentFullText = card.text;
        cardText.text = "";

        // Always start from a clean transform to avoid accumulating rotations/scales between deals.
        transform.localScale = savedLocalScale; // keep scene-authored scale
        transform.rotation = cameraTransform.rotation;
        List<GameObject> cardList = new List<GameObject>();
        for (int i = 0; i < cardsPerDeck; i++)
        {
            float step = deckHeight / (float)cardsPerDeck;

            GameObject newcard = Instantiate(cardPrefab, transform);
            newcard.transform.localPosition = Vector3.forward * (step * i);
            newcard.transform.localRotation = Quaternion.identity;

            newcard.transform.localScale = cardPrefab.transform.localScale;

            cardList.Add(newcard);
        }

        Vector3 centerPos = cameraTransform.position + cameraTransform.forward * 0.2f;
        transform.position = centerPos;
        transform.rotation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);

        for (int i = 0; i < 150; i++)
        {
            float parm = Mathf.SmoothStep(1.0f, 0.0f, Mathf.Pow(Mathf.InverseLerp(150, 0, i), 1.5f));
            transform.position = Vector3.Lerp(centerPos + cameraTransform.up * .3f, centerPos, parm);
            transform.rotation = cameraTransform.rotation * Quaternion.Slerp(Quaternion.LookRotation(cameraTransform.up, cameraTransform.right), Quaternion.Slerp(Quaternion.identity, Quaternion.LookRotation(cameraTransform.right, cameraTransform.up), 0.3f), parm);
            yield return new WaitForSeconds(.01f);
        }

        List<int> rightPile = new List<int>();
        for (int i = 0; i < cardsPerDeck; i++)
        {
            if (UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f)
            {
                rightPile.Add(1);
            }
            else
            {
                rightPile.Add(0);
            }
        }

        for (int k = 0; k < 4; k++)
        {
            audioManager.SfxSource.PlayOneShot(audioManager.SfxCards[k % 3], 1.0f);
            List<int> newids = new List<int>();
            for (int i = 0; i < cardsPerDeck; i++)
            {
                newids.Add(i);
            }
            fisherYates(newids);
            for (int i = 0; i < 31; i++)
            {
                float parm = Mathf.SmoothStep(0.0f, 1.0f, Mathf.InverseLerp(30, 0, i) * Mathf.InverseLerp(0, 30, i) * 4.0f);
                for (int j = 0; j < cardsPerDeck; j++)
                {
                    float step = deckHeight / (float)cardsPerDeck;

                    Vector3 startpos = Vector3.forward * (step * j);
                    Vector3 endpos = Vector3.forward * (step * newids[j]);
                    Vector3 pos = Vector3.Lerp(startpos, endpos, parm);

                    Vector3 side = Vector3.right * 2.5f;

                    if (rightPile[j] == 1)
                        cardList[j].transform.localPosition = Vector3.Lerp(pos, pos + side, parm);
                    else
                        cardList[j].transform.localPosition = Vector3.Lerp(pos, pos - side, parm);
                }
                yield return new WaitForSeconds(.01f);
            }
        }


        for (int i = 0; i < 50; i++)
        {
            float parm = Mathf.SmoothStep(1.0f, 0.0f, Mathf.Pow(Mathf.InverseLerp(49, 0, i), 1.5f));
            transform.rotation = cameraTransform.rotation * Quaternion.Slerp(Quaternion.Slerp(Quaternion.identity, Quaternion.LookRotation(cameraTransform.right, cameraTransform.up), 0.3f), Quaternion.identity, parm);

            yield return new WaitForSeconds(.01f);
        }

        cardList[0].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = card.sprite;
        for (int i = 0; i < 100; i++)
        {
            float parm = Mathf.SmoothStep(1.0f, 0.0f, Mathf.Pow(Mathf.InverseLerp(99, 0, i), 1.5f));
            cardList[0].transform.rotation = Quaternion.Slerp(
                Quaternion.LookRotation(-cameraTransform.forward, cameraTransform.up),
                transform.rotation,
                parm);
            for (int j = 1; j < cardsPerDeck; j++)
            {
                cardList[j].transform.localPosition = Vector3.Lerp(Vector3.zero, Vector3.up * 20.0f, parm);
            }
            yield return new WaitForSeconds(.01f);
        }

        for (int i = 0; i < 100; i++)
        {
            float parm = Mathf.SmoothStep(1.0f, 0.0f, Mathf.InverseLerp(99, 0, i));
            cardList[0].transform.localPosition = Vector3.Lerp(Vector3.zero, Vector3.right * -5.0f, parm);
            yield return new WaitForSeconds(.01f);
        }

        cardPanel.SetActive(true);
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        cardTitle.text = card.Name;
        typewriterCoroutine = StartCoroutine(TypewriterEffect(card.text));

        yield return new WaitUntil(() => !isTyping);

        _awaitingClick = true;
        _clickReceived = false;
        yield return new WaitUntil(() => _clickReceived);
        _awaitingClick = false;
        _clickReceived = false;


        for (int i = 0; i < cardsPerDeck; i++)
        {
            Destroy(cardList[i]);
        }

        cardPanel.SetActive(false);

        // Restore the transform so the next deal starts from the same authored state.
        transform.SetParent(savedParent, true);
        transform.localPosition = savedLocalPos;
        transform.localRotation = savedLocalRot;
        transform.localScale = savedLocalScale;
    }

    public void ShowCardText()
    {
        if (cardPanel != null)
            cardPanel.SetActive(true);
    }

    public void HideCardText()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;
        _awaitingClick = false;
        _clickReceived = false;

        if (cardPanel != null)
            cardPanel.SetActive(false);
    }

    private IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;

        foreach (char c in text)
        {
            if (c == '$')
            {
                cardText.text += '\n';
                continue; // Skip adding this character to the text
            }

            cardText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        typewriterCoroutine = null;
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

        cardText.text = currentFullText.Replace('$', '\n');
    }

    /// <summary>
    /// Handle input for advancing dialog
    /// </summary>
    private void Update()
    {
        if (cardPanel == null || !cardPanel.activeSelf)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
                SkipTypewriter();
            else if (_awaitingClick)
                _clickReceived = true;
        }
    }

    [ContextMenu("Test Deal")]
    public void TestDeal()
    {
        Card test = new Card();
        StartCoroutine(DealCard(test));
    }
}