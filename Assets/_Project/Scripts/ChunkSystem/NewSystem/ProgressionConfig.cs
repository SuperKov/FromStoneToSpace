using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProgressionStage
{
    public string ID = "0: New Stage";

    [Header("Requirements")]
    public int RequiredChunks = 4;
    public int RequiredPlayerLevel = 0;
    public ItemData[] RequiredItems;

    [Header("Costs")]
    public float CostMuiltiplier = 1.5f;
    public ItemStack[] AllowedCurrencyItems;

    public ItemStack[] GetRandomItems(int multipliers)
    {
        if (AllowedCurrencyItems == null || AllowedCurrencyItems.Length == 0) return new ItemStack[0];

        float chance;
        List<ItemStack> items = new();

        foreach (var item in AllowedCurrencyItems)
        {
            chance = Random.value;

            if (chance > 0.5f)
            {
                int amount = multipliers > 0 ? Mathf.RoundToInt(item.Amount * (multipliers * CostMuiltiplier)) : item.Amount;
                items.Add(new ItemStack(item.Item, amount));
            }
        }

        if (items.Count < 1)
        {
            int amount = multipliers > 0 ? Mathf.RoundToInt(AllowedCurrencyItems[0].Amount * (multipliers * CostMuiltiplier)) : AllowedCurrencyItems[0].Amount;
            items.Add(new ItemStack(AllowedCurrencyItems[0].Item, amount));
        }

        return items.ToArray();
    }
}

[CreateAssetMenu(fileName = "New Progression Config", menuName = "Project/World/Progression Config")]
public class ProgressionConfig : ScriptableObject
{
    [field: SerializeField] public ProgressionStage[] Stages { get; private set; }

    public ProgressionStage GetCurrentStage(int unlockedChunksCount, int playerLevel, HashSet<ItemData> unlockedItems)
    {
        foreach (var stage in Stages)
        {
            if (unlockedChunksCount < stage.RequiredChunks) continue;
            if (playerLevel < stage.RequiredPlayerLevel) continue;

            bool hasAllItems = true;
            foreach (var item in stage.RequiredItems)
            {
                if (!unlockedItems.Contains(item))
                {
                    hasAllItems = false;
                    break;
                }
            }

            if (!hasAllItems) continue;

            return stage;
        }

        // Возвращаем первую стадию по умолчанию, если ничего не подошло
        return Stages.Length > 0 ? Stages[0] : null;
    }
}