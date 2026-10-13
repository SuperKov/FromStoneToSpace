using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interacteRadius = 5f;

    private CircleCollider2D _collider;

    private List<IInteractable> _interactablesInRange = new();
    private IInteractable _closestInteractable;

    private void Start()
    {
        _collider = GetComponent<CircleCollider2D>();
        _collider.isTrigger = true;
        _collider.radius = interacteRadius;
    }

    private void OnEnable()
    {
        InputManager.Instance.Actions.Player.Interact.canceled += OnInteract;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.Actions.Player.Interact.canceled -= OnInteract;

        // При отключении игрока/сцены обязательно снимаем фокус
        ChangeTarget(null);
    }

    private void Update()
    {
        // Постоянно пересчитываем дистанцию, так как игрок двигается
        UpdateClosestInteractable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            // Защита от двойного добавления
            if (!_interactablesInRange.Contains(interactable))
                _interactablesInRange.Add(interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            _interactablesInRange.Remove(interactable);
        }
    }

    private void UpdateClosestInteractable()
    {
        // 1. ОЧИСТКА ПАМЯТИ: Удаляем из списка все уничтоженные объекты (решает NullReferenceException).
        // Проверка (i as MonoBehaviour) нужна из-за специфики "фейкового null" в Unity для интерфейсов.
        _interactablesInRange.RemoveAll(i => i == null || (i as MonoBehaviour) == null || i.Transform == null);

        if (_interactablesInRange.Count == 0)
        {
            ChangeTarget(null);
            return;
        }

        IInteractable closest = null;
        float minDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        // 2. ПОИСК БЛИЖАЙШЕГО
        foreach (IInteractable interactable in _interactablesInRange)
        {
            Vector3 directionToTarget = interactable.Transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < minDistanceSqr)
            {
                minDistanceSqr = dSqrToTarget;
                closest = interactable;
            }
        }

        ChangeTarget(closest);
    }

    private void ChangeTarget(IInteractable newTarget)
    {
        if (_closestInteractable == newTarget)
        {
            if (_closestInteractable != null)
            {
                _closestInteractable.SetHighlight(true);

                if (InteractionUI.Instance != null)
                    InteractionUI.Instance.Show(_closestInteractable.GetInteractionID(), _closestInteractable.Transform.position);
            }
            else if (InteractionUI.Instance != null)
                InteractionUI.Instance.Hide();

            return;
        }
        // Безопасно снимаем выделение со старого предмета
        if (_closestInteractable != null && (_closestInteractable as MonoBehaviour) != null)
            _closestInteractable.SetHighlight(false);

        _closestInteractable = newTarget;
    }

    private void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // Проверяем, что предмет все еще существует
        if (_closestInteractable != null && (_closestInteractable as MonoBehaviour) != null)
            _closestInteractable.Interact(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interacteRadius);
    }
}