using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public struct ResourceSpawnChance
{
    public ChunkResource Prefab;

    [Tooltip("Чем больше билетов, тем выше шанс выпадения по сравнению с другими ресурсами.")]
    [Min(1)] public int Tickets;

    [Header("Percent")] [SerializeField] private string chancePercent;

    public void SetInfo(string info) => chancePercent = info;
}

public abstract class BaseChunkData : ScriptableObject
{
    [field: Header("General Info")]
    [field: SerializeField] public string ChunkID { get; private set; } = "New_Base";

    [field: Header("Dynamic Resource Spawns")]
    [field: SerializeField] public ResourceSpawnChance[] Resources { get; private set; }
    [field: SerializeField] public float SpawnInterval { get; private set; } = 5f;
    [Tooltip("Максимальный процент заполнения чанка ресурсами (0.0 - 1.0)")]
    [field: SerializeField, Range(0f, 1f)] public float MaxResourceDensity { get; private set; } = 0.5f;

    // Срабатывает каждый раз, когда ты меняешь значения в Инспекторе
    private void OnValidate()
    {
        if (Resources == null || Resources.Length == 0) return;

        // 1. Считаем общее количество билетов в пуле
        int totalTickets = 0;
        for (int i = 0; i < Resources.Length; i++)
        {
            if (Resources[i].Tickets < 1) Resources[i].Tickets = 1; // Защита от нуля и отрицательных чисел
            totalTickets += Resources[i].Tickets;
        }

        // 2. Рассчитываем и записываем процент для отображения в Инспекторе
        for (int i = 0; i < Resources.Length; i++)
        {
            float chance = ((float)Resources[i].Tickets / totalTickets) * 100f;

            // Так как это структура, переназначаем элемент массива целиком
            var res = Resources[i];
            res.SetInfo($"{chance:F1}%");
            Resources[i] = res;
        }
    }
}