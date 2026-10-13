using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance { get; private set; }  

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Vector3 offset = new(0, 1f, 0);

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        panel.SetActive(false);
    }

    public void Show(string prompt, Vector3 worldPos)
    {
        transform.position = worldPos + offset;
        promptText.text = prompt;

        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}