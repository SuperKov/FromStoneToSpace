using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ToolTipManager : MonoBehaviour
{
    public static ToolTipManager Instance { get; private set; }

    [SerializeField] private RectTransform tooltipRect; // Ссылка на саму панель
    [SerializeField] private RectTransform canvasRect;  // Ссылка на Canvas
    [SerializeField] private VerticalLayoutGroup verticalLayout;

    [Header("Settings")]
    [SerializeField] private Vector2 cursorOffset = new(15f, -15f);

    [Header("UI Info")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private GameObject _currentPanel;

    private void Awake() => Instance = this;

    private void Start() => Hide();

    private void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (_currentPanel == null) return;

        if (!_currentPanel.activeInHierarchy)
        {
            _currentPanel = null;
            Hide();
        }

        // 1. Базовая позиция: курсор + смещение
        Vector2 finalPos = CameraManager.Instance.GetMouseScreenPosition() + cursorOffset;

        // 2. Учитываем масштаб UI (если в Canvas Scaler стоит Scale With Screen Size)
        float scaleX = canvasRect.localScale.x;
        float scaleY = canvasRect.localScale.y;

        // Физический размер панели на экране
        float tooltipWidth = tooltipRect.rect.width * scaleX;
        float tooltipHeight = tooltipRect.rect.height * scaleY;

        // 3. Защита от выхода за правый край
        if (finalPos.x + tooltipWidth > Screen.width)
            finalPos.x = Screen.width - tooltipWidth;

        // 4. Защита от выхода за нижний край (Y идет от 0 снизу до Screen.height сверху)
        if (finalPos.y - tooltipHeight < 0)
            finalPos.y = tooltipHeight;

        tooltipRect.position = finalPos;
    }

    public void Show(ItemData item, GameObject panel)
    {
        _currentPanel = panel;

        titleText.text = item.GetTranslatedName();
        descriptionText.text = item.GetTranslatedDescription();

        typeText.text = item.GetTranslatedType();
        statsText.text = item.GetTranslatedStats();

        // Принудительное обновление UI, чтобы размеры пересчитались до позиционирования
        verticalLayout.CalculateLayoutInputVertical();

        // Если статов нет (камень/ветка), отключаем лишние отступы/текст
        statsText.gameObject.SetActive(!string.IsNullOrEmpty(statsText.text));
        // Если описания нет
        descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(descriptionText.text));

        UpdatePosition(); // Ставим на место сразу при показе
        tooltipRect.gameObject.SetActive(true);
    }

    public void Hide()
    {
        tooltipRect.gameObject.SetActive(false);
    }
}
