using UnityEngine.Events;
using NaughtyAttributes;
using DG.Tweening;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
    [SerializeField] private CanvasGroup menu;

    [BoxGroup("Animation")]
    [SerializeField] private bool animate = true;
    [Header("Offset")]
    [ShowIf(nameof(animate))][BoxGroup("Animation")][SerializeField] private Vector2 moveOffset = new(0, 20f);
    [ShowIf(nameof(animate))][BoxGroup("Animation")][SerializeField] private float moveDuration = 0.2f;
    [ShowIf(nameof(animate))][BoxGroup("Animation")][SerializeField] private Ease moveEase = Ease.InQuad;
    [Header("Alpha")]
    [ShowIf(nameof(animate))][BoxGroup("Animation")][SerializeField] private float alphaDuration = 0.2f;
    [ShowIf(nameof(animate))][BoxGroup("Animation")][SerializeField] private Ease alphaEase = Ease.InQuad;

    [Header("Events")]
    public UnityEvent OnWindowOpened;
    public UnityEvent OnWindowClosed;

    public bool IsOpened { get; protected set; }

    protected Sequence _currentAnimation;
    protected Vector2 _menuDefaultPos;

    protected virtual void Awake()
    {
        _menuDefaultPos = menu.transform.localPosition;
        OnCloseInternal(); // Прячем при старте
    }

    protected virtual void OnDestroy()
    {
        _currentAnimation.Kill();
    }

    public virtual void Open()
    {
        if (IsOpened) return; // Защита от двойного открытия
        IsOpened = true;

        if (animate)
        {
            _currentAnimation?.Complete();

            menu.blocksRaycasts = true;
            menu.interactable = true;

            Tween floatAnim = DOVirtual.Float(menu.alpha, 1, alphaDuration, (f) => menu.alpha = f).SetEase(alphaEase);
            menu.transform.localPosition = _menuDefaultPos + moveOffset;
            Tween moveAnim = menu.transform.DOLocalMove(_menuDefaultPos, moveDuration).SetEase(moveEase);

            _currentAnimation = DOTween.Sequence().Append(floatAnim).Join(moveAnim)
                .OnComplete(() => OnWindowOpened?.Invoke());
        }
        else
        {
            menu.alpha = 1;
            menu.interactable = true;
            menu.blocksRaycasts = true;
            OnWindowOpened?.Invoke();
        }
    }

    public virtual void Close()
    {
        if (!IsOpened) return;
        IsOpened = false;

        if (animate)
        {
            _currentAnimation?.Kill();

            menu.blocksRaycasts = false;
            menu.interactable = false;

            Tween floatAnim = DOVirtual.Float(menu.alpha, 0, alphaDuration, (f) => menu.alpha = f).SetEase(alphaEase);
            Tween moveAnim = menu.transform.DOLocalMove(_menuDefaultPos + moveOffset, moveDuration).SetEase(moveEase);

            _currentAnimation = DOTween.Sequence().Append(floatAnim).Join(moveAnim)
                .OnComplete(() => OnWindowClosed?.Invoke());
        }
        else
        {
            OnCloseInternal();
            OnWindowClosed?.Invoke();
        }
    }

    private void OnCloseInternal()
    {
        menu.alpha = 0;
        menu.interactable = false;
        menu.blocksRaycasts = false;
    }

#if UNITY_EDITOR
    [Button]
    private void ForceOpen()
    {
        if (menu == null) return;

        if (Application.isPlaying)
            Open();
        else
            menu.alpha = 1;
    }

    [Button]
    private void ForceClose()
    {
        if (menu == null) return;

        if (Application.isPlaying)
            Close();
        else
            menu.alpha = 0;
    }
#endif
}