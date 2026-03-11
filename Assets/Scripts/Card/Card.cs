using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CreateNewCard", order = 1)]
public class Card : ScriptableObject
{
    [Header("Card Name")]
    public string Name;

    [Header("Card Text")]
    [TextArea(2, 8)]
    public string text;

    [Header("Card Sprite")]
    public Sprite sprite;
}