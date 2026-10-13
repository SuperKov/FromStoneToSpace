public class CraftingUISlot : BaseUISlot<CraftingRecipe>
{
    protected override void UpdateVisuals()
    {
        icon.sprite = _data.Result.Item.Icon;
        label.text = _data.Result.Item.name;
    }
}