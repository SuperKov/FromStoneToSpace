using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

public class TakeDamageFeedback : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Entity targetEntity;
    [SerializeField] private Transform visualsContainer; // Объект, который будем сжимать
    [SerializeField] private SpriteRenderer spriteRenderer; // Обычно тот же самый объект

    [Header("Squash & Stretch")]
    [SerializeField] private Ease punchEase = Ease.Linear;
    [SerializeField] private Vector3 punchScale = new(0.2f, -0.2f, 0f); // Растягиваем по X, сжимаем по Y
    [SerializeField] private float punchDuration = 0.2f;

    [Header("White Flash")]
    [SerializeField] private float flashDuration = 0.15f;

    [Header("HealthBar")]
    [SerializeField] private bool useHealthBar;
    [ShowIf(nameof(useHealthBar))]
    [SerializeField] private Transform barPosition;

    private HealthBarView _currentHealthBar;
    private Vector3 _originalScale;

    // Ссылки на текущие анимации, чтобы можно было их прервать
    private Tween _scaleTween;
    private Tween _flashTween;
    private float _lastHealth;

    // Инструменты для работы с шейдером (без создания дубликатов материалов)
    private MaterialPropertyBlock _mpb;

    private static Material flashMaterial;
    private static readonly int FlashAmountProperty = Shader.PropertyToID("_FlashAmount");

    private void Start()
    {
        if (flashMaterial == null)
        {
            GameConfig config = Resources.Load<GameConfig>("GameConfig");

            if (config == null || config.FlashMaterial == null)
            {
                Debug.Log("[TakeDamageFeedback] Ошибка загрузки GameConfig или Отстутсвует FlashMeterial!");
                return;
            }

            // Кэшируем материал, чтобы каждый раз не выгружать его
            flashMaterial = config.FlashMaterial;
        }

        // Задаем материал для мигания
        if (spriteRenderer != null)
            spriteRenderer.material = flashMaterial;

        // Запоминаем оригинальный размер
        if (visualsContainer != null)
            _originalScale = visualsContainer.localScale;

        // Запоминаем последнее / стартовое здоровье
        if (targetEntity != null)
            _lastHealth = targetEntity.Health;

        _mpb = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        if (targetEntity != null)
            targetEntity.HealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        if (targetEntity != null)
            targetEntity.HealthChanged -= OnHealthChanged;

        // Важно: убиваем твины при отключении объекта, чтобы избежать ошибок MissingReferenceException
        _scaleTween?.Kill();
        _flashTween?.Kill();

        // Возвращаем исходные значения
        if (visualsContainer != null)
            visualsContainer.localScale = _originalScale;

        if (_currentHealthBar != null)
        {
            _currentHealthBar.Hide();
            _currentHealthBar = null;
        }

        SetFlashAmount(0f);
    }

    private void OnHealthChanged(float health, float maxHealth)
    {
        // Если объект погиб
        if (health <= 0)
        {
            OnDie();
            return;
        }

        // Проверяем, что здоровье именно уменьшилось
        if (health < _lastHealth)
        {
            // Меняем хелс бар если он есть
            if (useHealthBar && HealthBarManager.Instance != null)
            {
                if (_currentHealthBar == null)
                    _currentHealthBar = HealthBarManager.Instance.GetHealthBar(barPosition, health, maxHealth);
                else
                    _currentHealthBar.SetHealth(health, maxHealth);
            }

            PlayFeedback();
        }

        _lastHealth = health;
    }

    private void OnDie()
    {
        if (_currentHealthBar != null)
        {
            _currentHealthBar.Hide();
            _currentHealthBar = null;
        }
    }    

    private void PlayFeedback()
    {
        // 1. Анимация сжатия (PunchScale отлично подходит для ударов)
        if (visualsContainer != null)
        {
            // Убиваем предыдущую анимацию, если по нам ударили дважды очень быстро
            _scaleTween?.Kill(true);

            // 1. Мгновенно сплющиваем модель от удара
            visualsContainer.localScale = _originalScale + punchScale;

            // 2. Анимируем возвращение в исходный размер с нужным Ease
            _scaleTween = visualsContainer.DOScale(_originalScale, punchDuration)
                .SetEase(punchEase);
        }

        // 2. Белая вспышка (Shader Property)
        if (spriteRenderer != null)
        {
            _flashTween?.Kill();

            // Сразу делаем спрайт полностью белым (1f)
            SetFlashAmount(1f);

            // Используем "виртуальный" твин DOTween, чтобы плавно опустить значение от 1 до 0
            _flashTween = DOVirtual.Float(1f, 0f, flashDuration, SetFlashAmount);
        }
    }

    // Метод для передачи значения прямо в видеокарту
    private void SetFlashAmount(float amount)
    {
        if (spriteRenderer == null) return;

        // Берем текущий блок параметров спрайта, меняем значение и возвращаем обратно
        spriteRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(FlashAmountProperty, amount);
        spriteRenderer.SetPropertyBlock(_mpb);
    }
}