using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoveController : MonoBehaviour
{
    public bool BlockMoving;

    [Header("Movement")]
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected float acceleration = 50f;

    [Header("Rotation")]
    [SerializeField] protected Transform rotationTarget;

    protected Vector2 _moveDirection;
    protected float _lookDirectionX;
    protected Rigidbody2D _rb;

    public Vector2 Velocity => _rb.linearVelocity;
    public float SpeedModifier { get; private set; } = 1f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        _rb.gravityScale = 0f;

        _lookDirectionX = Mathf.Sign(rotationTarget.localScale.x);
    }

    private void FixedUpdate()
    {
        HandleMove();
    }

    // В Top-Down "движение назад" обычно актуально только если персонаж целится в одну сторону, 
    // а идет в другую. Оставил проверку по оси X, как в оригинале.
    public bool IsMovingBack()
    {
        if (Mathf.Abs(Velocity.x) < 0.1f)
            return false;

        return Mathf.Sign(Velocity.x) != Mathf.Sign(_lookDirectionX);
    }

    public void SetDirection(Vector2 direction)
    {
        // Нормализуем, чтобы движение по диагонали не было быстрее
        _moveDirection = direction.normalized;
    }

    public void SetSpeedModifier(float multiplier) => SpeedModifier = multiplier;

    public virtual void HandleMove()
    {
        Vector2 targetVelocity;

        if (BlockMoving)
            targetVelocity = Vector2.zero;
        else
            targetVelocity = _moveDirection * speed * SpeedModifier;

        // Используем Vector2.MoveTowards для плавного ускорения/торможения по обеим осям
        _rb.linearVelocity = Vector2.MoveTowards(
            _rb.linearVelocity,
            targetVelocity,
            acceleration * SpeedModifier * Time.fixedDeltaTime
        );

        if (_rb.linearVelocity.sqrMagnitude < 0.01f)
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    // Флип персонажа влево/вправо в зависимости от направления движения по X
    public void HandleFlip(float direction)
    {
        // Если нет движения по X, оставляем текущее направление
        if (Mathf.Abs(direction) < 0.01f)
            return;

        float targetScaleX = direction > 0 ? 1f : -1f;

        if (targetScaleX != _lookDirectionX)
        {
            _lookDirectionX = targetScaleX;

            // Классический 2D флип через локальный масштаб
            Vector3 newScale = rotationTarget.localScale;
            newScale.x = Mathf.Abs(newScale.x) * targetScaleX;
            rotationTarget.localScale = newScale;
        }
    }
}