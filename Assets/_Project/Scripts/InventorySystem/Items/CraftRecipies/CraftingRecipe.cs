using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Project/Crafts/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [field: SerializeField] public ItemStack Result { get; private set; }
    [field: SerializeField] public ItemStack[] Ingredients { get; private set; }
    [field: SerializeField] public float CraftTime { get; private set; } = 1f;
}