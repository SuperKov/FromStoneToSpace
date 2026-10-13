using UnityEngine;
using System;

[RequireComponent(typeof(InventoryModel))]
public class ChestInteractable : MonoBehaviour, IInteractable
{
    public static event Action<InventoryModel> ChestOpened;

    public Transform Transform => transform;

    private InventoryModel _chestInventory;

    private void Awake()
    {
        _chestInventory = GetComponent<InventoryModel>();
    }

    public string GetInteractionID() => "[E] Открыть сундук";

    public bool Interact(PlayerInteractor interactor)
    {
        ChestOpened?.Invoke(_chestInventory);

        return true;
    }

    public void SetHighlight(bool isHighlighted)
    {
        // подсвечиваем
    }
}