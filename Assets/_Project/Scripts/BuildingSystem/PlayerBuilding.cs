using UnityEngine;

public class PlayerBuilding : DestructibleObject
{
    [Header("BuildingData")]
    [SerializeField] private BuildingData buildingData;
    [SerializeField] private bool dropAllItems = true;

    public string InstanceID { get; private set; }

    // Геттер, чтобы менеджер мог прочитать ID конфига
    public BuildingData Data => buildingData;

    private void Start()
    {
        if (dropAllItems && buildingData != null)
            DroppedItems = buildingData.BuildCost;
    }

    public void Init(string instanceId)
    {
        InstanceID = instanceId;
    }

    public virtual string GetCustomState() => "";
    public virtual void LoadCustomState(string stateJson) { }

    public override void Die()
    {
        if (WorldSaveManager.Instance != null)
            WorldSaveManager.Instance.UnregisterBuilding(InstanceID);

        base.Die();
    }
}