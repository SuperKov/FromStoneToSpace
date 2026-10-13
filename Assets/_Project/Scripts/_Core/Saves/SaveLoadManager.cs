using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using System;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInit() => _ = Instance;

    public void SaveGame(WorldSaveHeader worldHeader)
    {
        GameSaveData finalSave = new();
        worldHeader.LastSavedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        finalSave.WorldHeader = worldHeader;

        var allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        IEnumerable<ISaveable> saveables = allBehaviours.OfType<ISaveable>();

        foreach (ISaveable saveable in saveables)
            finalSave.EntitiesData[saveable.SaveID] = saveable.SaveState();

        //Сериализуем объект finalSave в отформатированную JSON-строку
        string jsonSaveData = JsonConvert.SerializeObject(finalSave, Formatting.Indented);

        // Передаем готовую строку
        SaveSystem.SaveToFile(worldHeader.Name, jsonSaveData);
    }

    public void LoadGame(string worldName)
    {
        GameSaveData loadedSave = SaveSystem.LoadFromFile(worldName);
        if (loadedSave == null) return;

        // Рассылаем данные всем ISaveable
        var allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        IEnumerable<ISaveable> saveables = allBehaviours.OfType<ISaveable>();

        foreach (ISaveable saveable in saveables)
            if (loadedSave.EntitiesData.TryGetValue(saveable.SaveID, out string savedStateJson))
                saveable.LoadState(savedStateJson);

        Debug.Log($"[SaveLoadManager] Данные мира '{worldName}' успешно загружены!");
    }
}