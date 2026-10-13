using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public abstract class BaseUISlot<T> : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] protected Image icon;
    [SerializeField] protected TextMeshProUGUI label;

    protected Action<T> _onSelected;
    protected Action<T> _onClick;
    protected T _data;

    // Делаем коллбэки опциональными (null по умолчанию), 
    // чтобы крафт мог передать только клик, а стройка — и клик, и наведение
    public virtual void Init(T data, Action<T> onHover = null, Action<T> onClick = null)
    {
        _data = data;
        _onSelected = onHover;
        _onClick = onClick;

        UpdateVisuals();
    }

    // Абстрактный метод: каждый сам решает, откуда брать картинку и текст
    protected abstract void UpdateVisuals();

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        _onSelected?.Invoke(_data);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _onClick?.Invoke(_data);
        }
    }
}