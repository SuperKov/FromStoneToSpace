using UnityEngine;

public class PickupableItem : MonoBehaviour, IInteractable
{
    public Transform Transform => transform;

    [SerializeField] private ItemStack item;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private MaterialPropertyBlock _mpb;

    private static readonly int OutlineProperty = Shader.PropertyToID("Outline");

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
    }

    public string GetInteractionID()
    {
        return $"[E] {item.Item.LocalizedName.GetLocalizedString()} x{item.Amount}";
    }

    public bool Interact(PlayerInteractor interactor)
    {
        DroppedItemsManager.Instance.SpawnDroppedItem(item, transform.position);

        // Обязательно снимаем фокус перед скрытием, чтобы UI не завис
        SetHighlight(false);

        if (InteractionUI.Instance != null)
            InteractionUI.Instance.Hide();

        gameObject.SetActive(false);
        return true;
    }

    public void SetHighlight(bool isHighlighted)
    {
        // Включаем или выключаем шейдер обводки через PropertyBlock
        spriteRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(OutlineProperty, isHighlighted ? 1f : 0f);
        spriteRenderer.SetPropertyBlock(_mpb);
    }
}