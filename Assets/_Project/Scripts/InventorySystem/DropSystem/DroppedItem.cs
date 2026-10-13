using UnityEngine;
using System;
using DG.Tweening; // Подключаем DOTween

[Serializable]
public struct SavedDroppedItemData
{
    public string ItemID;
    public int Amount;
    public Vector2Data Position;

    public SavedDroppedItemData(string id, int amount, Vector2Data pos)
    {
        ItemID = id;
        Amount = amount;
        Position = pos;
    }
}

public class DroppedItem : MonoBehaviour
{
    public ItemStack Data { get; private set; }

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float pickupRadius = 0.2f;

    [Header("Juice (Spawn Animation)")]
    [SerializeField] private float dropDuration = 0.5f; // Сколько длится прыжок
    [SerializeField] private float jumpPower = 1f;      // Высота прыжка
    [SerializeField] private float dropRadius = 1.5f;   // На какое расстояние отлетает предмет

    [Header("Visual")]
    [SerializeField] private SpriteRenderer model;
    [SerializeField] private LayerSorter layerSorter;

    private event Action<DroppedItem> _releaseCallback;
    private InventoryModel _targetInventory;
    private Transform _target;
    private float _currentSpeed = 0f;

    // Блокировщик подбора: true, если предмет сейчас в анимации появления
    private bool _isSpawning;

    // Свойство для Магнита: Готов ли предмет к притягиванию?
    public bool IsReady => !_isSpawning && _target == null;

    private void Update()
    {
        if (_target == null || _isSpawning) return;

        if (_currentSpeed < maxSpeed)
            _currentSpeed += acceleration * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, _target.position, _currentSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, _target.position) < pickupRadius)
            Collect();
    }

    public void Init(ItemStack item, Vector2 position, Action<DroppedItem> releaseCallback, bool fastSpawn = false)
    {
        Data = item;
        model.sprite = item.Item.Icon;
        transform.position = position;
        layerSorter.UpdateLayer();

        ResetFollow();

        _releaseCallback = releaseCallback;

        // Запускаем анимацию выпадания
        if (!fastSpawn)
            PlayDropAnimation(position);
    }

    private void PlayDropAnimation(Vector2 startPos)
    {
        _isSpawning = true;

        // Начальные значения (предмет вылетает из точки, будучи маленьким)
        transform.localScale = Vector3.zero;

        // Вычисляем случайную точку падения вокруг места добычи
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * dropRadius;
        Vector2 endPos = startPos + randomOffset;

        // Создаем секвенцию (цепочку анимаций)
        Sequence dropSeq = DOTween.Sequence();

        // 1. Прыжок по дуге (использует встроенный DOJump из DOTween)
        dropSeq.Append(transform.DOJump(endPos, jumpPower, 1, dropDuration));

        // 2. Одновременно с прыжком увеличиваем масштаб с эффектом отдачи (OutBack)
        dropSeq.Join(transform.DOScale(Vector3.one, dropDuration).SetEase(Ease.OutBack));

        // 3. Когда анимация закончилась — разрешаем подбор
        dropSeq.OnComplete(() => _isSpawning = false);
    }

    public void StartAttraction(Transform targetPlayer, InventoryModel inventory)
    {
        _target = targetPlayer;
        _targetInventory = inventory;

        // Можно добавить эффект сужения (squash) при старте притягивания
        transform.DOScale(new Vector3(0.5f, 1.5f, 1f), 0.2f);
    }

    private void Collect()
    {
        if (_targetInventory == null) return;

        int rest = _targetInventory.AddItem(Data);
        Data = new ItemStack(Data.Item, rest);

        ResetFollow();

        if (Data.IsEmpty)
            _releaseCallback?.Invoke(this);
    }

    private void ResetFollow()
    {
        // КРИТИЧЕСКИ ВАЖНО: Убиваем все анимации DOTween на этом объекте перед сбросом
        transform.DOKill();

        _target = null;
        _targetInventory = null;
        _currentSpeed = 0;
        _isSpawning = false;

        // Возвращаем масштаб в норму, если прервали анимацию
        transform.localScale = Vector3.one;
    }
}