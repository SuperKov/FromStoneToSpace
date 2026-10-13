using System.Collections.Generic;
using UnityEngine;
using System;

// Главный класс, который пойдет в файл save.json
[Serializable]
public class GameSaveData
{
    // Метаданные (Шапка для Главного Меню)
    public WorldSaveHeader WorldHeader;

    // Ключ - это SaveID объекта, Значение - его запакованные данные (CustomState)
    public Dictionary<string, string> EntitiesData = new();
}

// Описание мира
[Serializable]
public struct WorldSaveHeader
{
    public string Name;
    public string LastSavedDate;
    public int Seed;
    // Сюда можно добавить Seed, Difficulty, время в игре и т.д.
}

[Serializable]
public struct ItemStackData
{
    public string ItemID;
    public int Amount;

    public ItemStackData(string itemID, int amount)
    {
        ItemID = itemID;
        Amount = amount;
    }
}

// Структуры для работы с векторами в json (обычный от юньки не подходит)
[Serializable]
public struct Vector2Data
{
    public float x;
    public float y;

    // Конструктор, чтобы удобно создавать из обычного вектора Unity
    public Vector2Data(Vector2 vector)
    {
        x = vector.x;
        y = vector.y;
    }

    // Метод, чтобы удобно превращать обратно в вектор Unity
    public Vector2 ToVector2()
    {
        return new(x, y);
    }
}

[Serializable]
public struct Vector2IntData
{
    public int x;
    public int y;

    // Конструктор, чтобы удобно создавать из обычного вектора Unity
    public Vector2IntData(Vector2Int vector)
    {
        x = vector.x;
        y = vector.y;
    }

    // Метод, чтобы удобно превращать обратно в вектор Unity
    public Vector2Int ToVector2Int()
    {
        return new(x, y);
    }
}