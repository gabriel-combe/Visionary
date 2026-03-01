using UnityEngine;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class TextColorChange : MonoBehaviour
{
    [SerializeField] private Color _colorText = Color.green;

    public void ChangeTextColor()
    {
        var textMeshProUGUI = GetComponent<TMPro.TextMeshProUGUI>();
        textMeshProUGUI.color = _colorText;
    }
}
