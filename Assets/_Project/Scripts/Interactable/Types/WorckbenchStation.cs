using UnityEngine;
using System;

public class WorckbenchStation : MonoBehaviour, IInteractable
{
    public static event Action<CraftingRecipe[]> CraftStationOpened;

    public Transform Transform => transform;

    [Header("Workbench Recipes")]
    [SerializeField] private CraftingRecipe[] availableRecipes;

    public string GetInteractionID() => "[E] Использовать верстак";

    public bool Interact(PlayerInteractor interactor)
    {
        CraftStationOpened?.Invoke(availableRecipes);

        return true;
    }

    public void SetHighlight(bool isHighlighted)
    {
        // Подсветка объекта
    }
}