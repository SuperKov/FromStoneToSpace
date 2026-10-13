using UnityEngine;

public class PlayerCombatController : MonoBehaviour, IWeaponEquipper
{
    [SerializeField] private PlayerEntity playerEntity;

    [Header("Sub-Controllers")]
    [SerializeField] private MeleeCombatController meleeController;
    // [SerializeField] private BowCombatController bowController;
    // Сюда можно добавить MagicCombatController и т.д.

    // Базовый интерфейс для всех контроллеров атаки
    private ICombatController _activeWeaponController;
    private bool _blockAttack;

    private void OnEnable()
    {
        playerEntity.SelectedSlotChanged += OnSelectedSlotChanged;
    }

    private void OnDisable()
    {
        playerEntity.SelectedSlotChanged -= OnSelectedSlotChanged;
    }

    // ЭТОТ МЕТОД ВЫЗЫВАЕТ PLAYERCONTROLLER ПРИ КЛИКЕ
    public void StartAttack()
    {
        if (_blockAttack) return;

        _activeWeaponController?.StartAttack();
    }

    public void StopAttack()
    {
        if (_blockAttack) return;

        _activeWeaponController?.StopAttack();
    }

    public void SwitchAttackBlock(bool mode)
    {
        _blockAttack = mode;
    }

    private void OnSelectedSlotChanged(int slotIndex)
    {
        ItemStack itemStack = playerEntity.Inventory.Slots[slotIndex];

        if (!itemStack.IsEmpty && itemStack.Item is WeaponItemData weapon)
        {
            // Используем паттерн Двойная диспетчеризация
            // Оружие само вызывает нужный метод (EquipMelee или EquipBow)
            weapon.EquipTo(this);
        }
        else
            // Взяли в руки камень или ничего - отключаем боевку
            DisableAllWeapons();
    }

    public void EquipMelee(MeleeWeaponData data)
    {
        if (meleeController == null) return;

        DisableAllWeapons();

        meleeController.StopAttack();
        meleeController.SetData(data);
        meleeController.enabled = true;
        _activeWeaponController = meleeController; // IWeaponController
    }

    public void EquipBow(BowData data)
    {
        DisableAllWeapons();

        //meleeController.SetData(data);
        //meleeController.enabled = true;
        //_activeWeaponController = meleeController;
    }

    private void DisableAllWeapons()
    {
        meleeController.enabled = false;
        // bowController.enabled = false;
        _activeWeaponController = null;
    }
}