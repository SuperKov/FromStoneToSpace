using UnityEngine;

public class BuildingPreview : MonoBehaviour
{
    [SerializeField] private SpriteRenderer previewRenderer;

    public void SetData(Sprite sprite, Vector2Int gridSize)
    {
        Vector3 size = new(gridSize.x, gridSize.y, 1);
        previewRenderer.transform.localScale = size;
        previewRenderer.sprite = sprite;
        previewRenderer.gameObject.SetActive(true);
    }

    public void Hide()
    {
        previewRenderer.gameObject.SetActive(false);
    }

    public void UpdateVisuals(Vector2 position, bool canBuild, Color validColor, Color invalidColor)
    {
        transform.position = position;
        previewRenderer.color = canBuild ? validColor : invalidColor;
    }
}