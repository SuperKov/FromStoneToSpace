public interface IWeaponEquipper
{
    // Описываем все возможные методы экипировки
    void EquipMelee(MeleeWeaponData data);
    void EquipBow(BowData data);
    // void EquipMagic(MagicWeaponItemData data);
}