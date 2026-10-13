using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class RequirementInfoPanel : UIWindow
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Not Requireable")] // Все здесь может быть пустым
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI additionalText;
    [SerializeField] private RectTransform layoutGroupContainer;

    [Header("Costs")]
    [SerializeField] private Transform costContainer;
    [SerializeField] private ResourceUICost resourceCostPrefab;

    [Header("Options")]
    [SerializeField] private bool offIfNull;

    private List<ResourceUICost> _spawnedCosts = new();

    // sprite и description является опциональным параметром
    public void Show(string title, ItemStack[] costs, InventoryModel inventory, Sprite sprite = null, string description = "", string additionalInfo = "")
    {
        if (titleText != null)
            titleText.text = title;

        // Если описание передали - показываем, если нет - скрываем объект
        if (descriptionText != null)
        {
            if (!string.IsNullOrEmpty(description))
            {
                descriptionText.text = description;
                descriptionText.gameObject.SetActive(true);
            }
            else
                descriptionText.gameObject.SetActive(false);
        }

        // Если спрайт передали - показываем, если нет - скрываем объект
        if (icon != null)
        {
            if (sprite != null)
            {
                icon.sprite = sprite;
                icon.gameObject.SetActive(true);
            }
            else
                icon.gameObject.SetActive(false);
        }

        if (additionalText != null)
        {
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                additionalText.text = additionalInfo;
                additionalText.gameObject.SetActive(true);
            }
            else
                additionalText.gameObject.SetActive(false);
        }

        int activeCostIndex = 0;

        foreach (ItemStack cost in costs)
        {
            int playerHas = inventory != null ? inventory.GetItemAmount(cost.Item) : 0;

            if (activeCostIndex < _spawnedCosts.Count)
            {
                _spawnedCosts[activeCostIndex].gameObject.SetActive(true);
                _spawnedCosts[activeCostIndex].Setup(cost.Item.Icon, playerHas, cost.Amount);
            }
            else
            {
                ResourceUICost costUI = Instantiate(resourceCostPrefab, costContainer);
                costUI.Setup(cost.Item.Icon, playerHas, cost.Amount);
                _spawnedCosts.Add(costUI);
            }
            activeCostIndex++;
        }

        // Скрываем оставшиемся иконки
        for (int i = activeCostIndex; i < _spawnedCosts.Count; i++)
            _spawnedCosts[i].gameObject.SetActive(false);

        if (layoutGroupContainer != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroupContainer);

        if (!IsOpened)
            Open();
    }

    public void Hide()
    {
        if (offIfNull)
            Close();
        else
        {
            titleText.text = "-";

            if (icon != null) icon.gameObject.SetActive(false);
            if (descriptionText != null) descriptionText.gameObject.SetActive(false);
            if (additionalText != null) additionalText.gameObject.SetActive(false);

            for (int i = 0; i < _spawnedCosts.Count; i++)
                _spawnedCosts[i].gameObject.SetActive(false);
        }
    }
}