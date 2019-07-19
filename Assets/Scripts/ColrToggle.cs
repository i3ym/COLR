using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(TextMeshProUGUI))]
public class ColrToggle : MonoBehaviour
{
    [HideInInspector]
    public Button Button;
    TextMeshProUGUI Text;

    [SerializeField]
    Color OffColor;
    [SerializeField]
    public bool IsOn = true;

    void Awake()
    {
        Button = GetComponent<Button>();
        Text = GetComponent<TextMeshProUGUI>();

        UpdateTextColor();

        Button.onClick.AddListener(() =>
        {
            IsOn = !IsOn;
            UpdateTextColor();
        });
    }

    void UpdateTextColor() => Text.color = IsOn ? Color.white : OffColor;
}