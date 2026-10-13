using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public InputSystem_Actions Actions {  get; private set; }

    public bool IsPlayerBlocked => _playerActionsBlock > 0;

    private int _playerActionsBlock = 0;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInit() => _ = Instance;

    protected override void Awake()
    {
        base.Awake();
        Actions = new InputSystem_Actions();
        Actions.Enable();
    }

    public void UnlockPlayerActions(bool fullUnlock = false)
    {
        if (!fullUnlock)
        {
            _playerActionsBlock--;
            if (_playerActionsBlock < 0)
                _playerActionsBlock = 0;
        }
        else
            _playerActionsBlock = 0;

        CheckPlayerActionsBlock();
    }

    public void LockPlayerActions()
    {
        _playerActionsBlock++;
        CheckPlayerActionsBlock();
    }

    private void CheckPlayerActionsBlock()
    {
        if (IsPlayerBlocked)
            Actions.Player.Disable();
        else
            Actions.Player.Enable();
    }
}