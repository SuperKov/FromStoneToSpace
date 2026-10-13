public class BuildingUISlot : BaseUISlot<BuildingData>
{
    protected override void UpdateVisuals()
    {
        icon.sprite = _data.Icon;
        label.text = _data.ID;
    }
}