using UnityEngine;
using TMPro;

public class TextHelper : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textUI;

    // use with animation events
    public void AnimateText(string newText) => textUI.text = newText;
}
