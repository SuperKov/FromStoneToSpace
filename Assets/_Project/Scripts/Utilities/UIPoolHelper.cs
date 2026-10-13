using System.Collections.Generic;
using UnityEngine;
using System;

public static class UIPoolHelper
{
    // TData - это BuildingData или CraftingRecipe
    // TSlot - это BuildingUISlot или CraftingUISlot
    public static void UpdatePool<TData, TSlot>(
        IList<TData> dataList,       // Массив данных
        List<TSlot> pool,            // Наш список спавненных слотов
        TSlot prefab,                // Префаб слота
        Transform container,         // Куда спавнить
        Action<TSlot, TData> setup)  // Метод, который скажет, как инициализировать слот
        where TSlot : MonoBehaviour
    {
        for (int i = 0; i < dataList.Count; i++)
            if (i < pool.Count)
            {
                setup(pool[i], dataList[i]);
                pool[i].gameObject.SetActive(true);
            }
            else
            {
                TSlot newSlot = UnityEngine.Object.Instantiate(prefab, container);
                setup(newSlot, dataList[i]);
                pool.Add(newSlot);
            }

        // Отключаем лишние
        for (int i = dataList.Count; i < pool.Count; i++)
            pool[i].gameObject.SetActive(false);
    }
}