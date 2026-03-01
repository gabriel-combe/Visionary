using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CreateNewCard", order = 1)]
public class Card : ScriptableObject
{
    public string name;
    public string text;
    public Sprite sprite;
}