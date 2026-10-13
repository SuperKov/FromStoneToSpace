using UnityEngine;


[CreateAssetMenu(fileName = "NewItem", menuName = "Project/Items/Base Item")]
public class StackableItemData : ItemData
{
    [Header("Stack Settings")]
    [SerializeField] private int maxStackSize = 1000;

    // Переопределяем свойство базового класса, отдавая значение из инспектора
    public override int MaxStackSize => maxStackSize;
}