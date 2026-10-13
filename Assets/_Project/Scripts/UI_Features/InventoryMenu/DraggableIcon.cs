using UnityEngine.UI;
using UnityEngine;

public class DraggableIcon : MonoBehaviour
{
    public static DraggableIcon Instance { get; private set; }

    [SerializeField] private Image icon;

    private void Awake()
    {
        Instance = this;
        icon.gameObject.SetActive(false);
    }

    // Включаем иконку, задаем ей картинку и переносим к мыши
    public void StartDragging(Sprite itemSprite, Vector2 startMousePosition)
    {
        icon.gameObject.SetActive(true);
        icon.sprite = itemSprite;
        icon.transform.position = startMousePosition;
    }

    // Двигаем иконку за курсором
    public void UpdateDragPosition(Vector2 mousePosition)
    {
        icon.transform.position = mousePosition;
    }

    // Прячем иконку
    public void StopDragging()
    {
        icon.gameObject.SetActive(false);
    }
}