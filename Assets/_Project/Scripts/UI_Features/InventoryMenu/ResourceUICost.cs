using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ResourceUICost : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;

    [Header("Colors")]
    [SerializeField] private Color enoughColor = Color.white;
    [SerializeField] private Color notEnoughColor = Color.gray;

    public void Setup(Sprite itemIcon, int playerAmount, int requiredAmount)
    {
        icon.sprite = itemIcon;
        amountText.text = $"{playerAmount}/{requiredAmount}";

        // Если ресурсов не хватает, красим текст в красный
        amountText.color = (playerAmount >= requiredAmount) ? enoughColor : notEnoughColor;
    }
}