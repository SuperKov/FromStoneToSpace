using UnityEngine;

public class PlayerArmorView : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private EquipmentModel armor; // Ссылка на модель брони игрока

    [Header("Renderers")]
    [SerializeField] private SpriteRenderer helmetRenderer;
    [SerializeField] private SpriteRenderer chestRenderer;
    [SerializeField] private SpriteRenderer pantsRenderer;

    private void OnEnable()
    {
        armor.OnSlotUpdated += HandleArmorChanged;
    }

    private void OnDisable()
    {
        armor.OnSlotUpdated -= HandleArmorChanged;
    }

    private void Start()
    {
        // Принудительно отрисовываем броню при старте (если она уже надета)
        for (int i = 0; i < armor.Slots.Length; i++)
            HandleArmorChanged(i);
    }

    private void HandleArmorChanged(int slotIndex)
    {
        ItemStack armorStack = this.armor.Slots[slotIndex];
        Sprite armorSprite = null;

        // Если слот не пустой и в нем лежит броня — достаем её спрайт
        if (!armorStack.IsEmpty && armorStack.Item is ArmorItemData armor)
        {
            armorSprite = armor.View;
        }

        // Включаем или выключаем нужный рендерер в зависимости от индекса слота
        switch (slotIndex)
        {
            case 0: // Шлем
                helmetRenderer.sprite = armorSprite;
                break;
            case 1: // Нагрудник
                chestRenderer.sprite = armorSprite;
                break;
            case 2: // Штаны
                pantsRenderer.sprite = armorSprite;
                break;
        }
    }
}