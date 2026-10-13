using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private FrameAnimator legsAnimator;
    [SerializeField] private string legsIdleAnimationId;
    [SerializeField] private string legsWalkAnimationId;

    [Header("Player")]
    [SerializeField] private MoveController moveController;

    private void Update()
    {
        CheckMove();
    }

    private void CheckMove()
    {
        if (legsAnimator == null ||  moveController == null) return;

        if (Mathf.Abs(moveController.Velocity.sqrMagnitude) > 0.1f)
            legsAnimator.Play(legsWalkAnimationId);
        else
            legsAnimator.Play(legsIdleAnimationId);

    }
}