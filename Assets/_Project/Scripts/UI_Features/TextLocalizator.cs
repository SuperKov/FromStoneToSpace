using UnityEngine.Localization;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextLocalizator : MonoBehaviour
{
    [SerializeField] private LocalizedString Key;

    private TextMeshProUGUI _text;

    private void Awake()
    {
        if (Key == null)
        {
            enabled = false;
            return;
        }

        _text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable() => Key.StringChanged += OnLocaleChanged;

    private void OnDisable() => Key.StringChanged -= OnLocaleChanged;

    private void OnLocaleChanged(string value) => _text.text = value;
}