using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;

public static class SaveSystem
{
    // Application.persistentDataPath - это специальная папка, 
    // к которой игра гарантированно имеет доступ на чтение и запись на любой ОС.
    public static string SavesDirectory => Application.persistentDataPath + "/Saves";

    private static string GetSavePath(string worldName) => $"{SavesDirectory}/{worldName}.json";

    public static void SaveToFile(string worldName, string jsonSaveData)
    {
        if (!Directory.Exists(SavesDirectory))
            Directory.CreateDirectory(SavesDirectory);

        File.WriteAllText(GetSavePath(worldName), jsonSaveData);
        Debug.Log($"[SaveSystem] Мир {worldName} успешно сохранен на диск!");
    }

    public static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"Сохранение удалено: {filePath}");
        }
    }

    public static GameSaveData LoadFromFile(string worldName)
    {
        string path = GetSavePath(worldName);

        if (!File.Exists(path))
            return null; // Возвращаем null, чтобы менеджер знал, что файла нет

        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<GameSaveData>(json);
    }

    // Для Главного Меню: читаем только шапки всех файлов
    public static SaveFileDescriptor[] GetAllSavesDescriptors()
    {
        if (!Directory.Exists(SavesDirectory))
            return null;

        List<SaveFileDescriptor> savesList = new();

        string[] saveFiles = Directory.GetFiles(SavesDirectory, "*.json");

        foreach (string path in saveFiles)
        {
            string rawJson = File.ReadAllText(path);
            try
            {
                // Десериализуем ТОЛЬКО поля из WorldSaveHeader (WorldName, LastSavedDate)
                // Остальной тяжелый JSON (чанки, здания) проигнорируется, что спасет от лагов!
                WorldSaveHeader header = JsonConvert.DeserializeObject<GameSaveData>(rawJson).WorldHeader;

                savesList.Add(new SaveFileDescriptor
                {
                    Header = header,
                    RawJson = rawJson,
                    FilePath = path
                });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] Ошибка чтения файла {path}: {e.Message}");
            }
        }

        return savesList.ToArray();
    }
}