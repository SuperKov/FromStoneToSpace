using System.Collections.Generic;
using UnityEngine;
using TMPro;

public struct SavedChunkBuyerData
{
    public Vector2IntData GridIndex;
    public List<ItemStackData> Cost;
}

public class ChunkBuyerInteractable : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private TextMeshProUGUI content;

    [Header("Content Following")]
    [SerializeField] private float panelMaxOffset = 1.5f;
    [SerializeField] private float stopFollowingDistance = 3.5f;
    [SerializeField] private float followingSpeed = 1.5f;

    public Transform Transform => transform;

    public Vector2Int TargetGridIndex { get; private set; }
    public ItemStack[] ItemsToUnlock { get; private set; }

    private bool _isActive;
    private Transform _player;

    private Vector3 _anchorPos;

    private void Update()
    {
        if (!_isActive || _player == null) return;

        float distance = Vector2.Distance(_player.position, _anchorPos);
        if (distance > stopFollowingDistance || distance < 0.2f) return;

        Vector3 offset = _player.transform.position -_anchorPos;
        offset = Vector2.ClampMagnitude(offset, panelMaxOffset);

        contentPanel.transform.position = Vector2.Lerp(contentPanel.transform.position, _anchorPos + offset, Time.deltaTime * followingSpeed);
    }

    public void Init(Vector2Int gridIndex, ItemStack[] unlockItems)
    {
        TargetGridIndex = gridIndex;
        ItemsToUnlock = unlockItems;

        string costText = string.Empty;
        foreach (var item in ItemsToUnlock)
            costText += $"\n{item.Item.LocalizedName.GetLocalizedString()}: x{item.Amount}";
        content.text = "Требуется для открытия:" + costText;

        _player = PlayerEntity.Instance.transform;
        _anchorPos = transform.position;
        _isActive = true;
    }

    public string GetInteractionID()
    {
        // Здесь в будущем подключится логика цен из ProgressionStage

        return "[E] Купить новую землю";
    }

    public bool Interact(PlayerInteractor interactor)
    {
        // После проверки и списания ресурсов из инвентаря игрока:
        if (ItemsToUnlock.Length > 0)
            if (!PlayerEntity.Instance.Inventory.TryRemoveItems(ItemsToUnlock))
                return false;

        _isActive = false;
        ChunkManager.Instance.UnlockChunk(TargetGridIndex);
        return true;
    }

    public void SetHighlight(bool isHighlighted)
    {

    }
}