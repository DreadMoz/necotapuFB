using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour
{
    public Text textInstance;

    public void UpdateText(string newText)
    {
        textInstance.text = newText;
    }
}