using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class HealthBarView : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image deltaBar;

    [Header("Settings")]
    [SerializeField] private float deltaBarTime = 0.4f;
    [SerializeField] private Ease deltaBarEase = Ease.InExpo;

    private bool _isActive;
    private float _currentDelta;
    private Transform _targetPos;
    private Tweener _currentTweener;
    private HealthBarManager _manager;

    private void LateUpdate()
    {
        if (!_isActive) return;

        if (_targetPos == null || !_targetPos.gameObject.activeSelf)
        {
            Hide();
            return;
        }

        transform.position = _targetPos.position;
    }

    public void Init(Transform targetPos, float health, float maxHealth, HealthBarManager manager)
    {
        _targetPos = targetPos;
        _manager = manager;

        SetHealth(health, maxHealth);

        _isActive = true;
    }

    public void SetHealth(float newHealth, float maxHealth)
    {
        if (maxHealth <= 0) return;

        float _reqHealth = newHealth / maxHealth;

        healthBar.fillAmount = _reqHealth;

        if (_currentDelta <= 0)
            _currentDelta = _reqHealth;

        if (_currentTweener != null)
            _currentTweener.Kill();

        _currentTweener = DOVirtual.Float(_currentDelta, _reqHealth, deltaBarTime, (f) =>
        {
            _currentDelta = f;
            deltaBar.fillAmount = f;
        })
            .SetEase(deltaBarEase);
    }

    public void Hide()
    {
        Debug.Log("health bar has been hiden!");
        _isActive = false;

        if (_manager != null)
            _manager.ReturnToPool(this);
    }
}