using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform), typeof(Button))]
public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform Line;

    [Header("Settings")]
    [SerializeField] private float ScaleMultiplier = 1.1f;
    [SerializeField] private float TargetLineScale = 1;
    [SerializeField] private float MinLineScale = 0;

    [Header("Animation")]
    [SerializeField] private float EnterDuration = 0.2f;
    [SerializeField] private Ease EnterEase = Ease.OutBack;
    [SerializeField] private float ExitDuration = 0.3f;
    [SerializeField] private Ease ExitEase = Ease.OutQuart;

    private Button _button;
    private Vector3 _defaultScale;
    private RectTransform _rectTransform;

    private Sequence _currentAnimation;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _button = GetComponent<Button>();
        _defaultScale = _rectTransform.localScale;

        ResetView();
    }

    private void OnEnable()
    {
        ResetView();
        _button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        if (_currentAnimation != null)
            _currentAnimation.Kill();

        _button.onClick.RemoveListener(OnClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_rectTransform == null || Line == null) return;

        if (_currentAnimation != null)
            _currentAnimation.Kill();

        _currentAnimation = DOTween.Sequence().Append(_rectTransform.DOScale(ScaleMultiplier, EnterDuration).SetEase(EnterEase)).
            Join(Line.DOScaleX(TargetLineScale, EnterDuration).SetEase(EnterEase)).
            SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_rectTransform == null || Line == null) return;

        if (_currentAnimation != null)
            _currentAnimation.Kill();

        _currentAnimation = DOTween.Sequence().Append(_rectTransform.DOScale(_defaultScale, ExitDuration).SetEase(ExitEase)).
            Join(Line.DOScaleX(MinLineScale, ExitDuration).SetEase(ExitEase)).
            SetUpdate(true);
    }

    private void ResetView()
    {
        if (_rectTransform == null || Line == null) return;

        Vector2 s = Line.localScale;
        s.x = MinLineScale;
        Line.localScale = s;
        _rectTransform.localScale = _defaultScale;
    }

    private void OnClick()
    {
        if (_rectTransform == null) return;

        if (_currentAnimation != null)
            _currentAnimation.Kill();

        _currentAnimation = DOTween.Sequence().Append(_rectTransform.DOPunchScale(new Vector3(-0.1f, -0.1f, 0), 0.2f, 10, 1f));
    }
}