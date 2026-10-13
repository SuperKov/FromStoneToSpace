using UnityEngine.InputSystem;
using UnityEngine;
using System;

public class UIInputRouter : MonoBehaviour
{
    [System.Serializable]
    private struct WindowHotkey
    {
        public string id; // Для эдитора
        public InputActionReference Key;
        public UIWindow Window;

        [Header("Tab Settings (Optional)")]
        public UITabController TabController; // Если это окно с вкладками, кидаем ссылку сюда
        public int TargetTabIndex;            // Индекс вкладки (например, Инвентарь = 0)
    }

    [Header("General Hotkeys")]
    [SerializeField] private WindowHotkey[] windows;

    [Header("Escape Key Behavior")]
    [SerializeField] private InputActionReference backKey;
    [Tooltip("Что открывать по Esc, если все окна закрыты (Настройки)")]
    [SerializeField] private WindowHotkey settingsHotkey;

    // Массив для сохранения ссылок на лямбды
    private Action<InputAction.CallbackContext>[] _hotkeyCallbacks;

    private void OnEnable()
    {
        if (backKey != null) backKey.action.performed += OnBack;

        if (windows != null)
        {
            _hotkeyCallbacks = new Action<InputAction.CallbackContext>[windows.Length];

            for (int i = 0; i < windows.Length; i++)
            {
                var hotkey = windows[i]; // Локальная копия для замыкания
                if (hotkey.Key == null) continue;

                // 1. Создаем лямбду и сохраняем ее в массив
                _hotkeyCallbacks[i] = (ctx) => OnHotkey(hotkey);

                // 2. Подписываем именно сохраненную ссылку
                hotkey.Key.action.performed += _hotkeyCallbacks[i];
            }
        }
    }

    private void OnDisable()
    {
        if (backKey != null) backKey.action.performed -= OnBack;

        if (windows != null && _hotkeyCallbacks != null)
        {
            for (int i = 0; i < windows.Length; i++)
            {
                if (windows[i].Key == null || _hotkeyCallbacks[i] == null) continue;

                // Правильная отписка! Теперь событие не будет дублироваться при загрузке
                windows[i].Key.action.performed -= _hotkeyCallbacks[i];
            }
        }
    }

    // ЛОГИКА КЛАВИШИ ESC
    private void OnBack(InputAction.CallbackContext ctx)
    {
        if (UIManager.Instance == null) return;

        if (UIManager.Instance.CurrentWindow != null)
            // СЦЕНАРИЙ 1: Что-то открыто -> Закрываем верхнее окно
            UIManager.Instance.CloseAll();
        else if (settingsHotkey.Window != null)
            // СЦЕНАРИЙ 2: Всё закрыто -> Открываем Настройки
            OnHotkey(settingsHotkey);
    }

    // ЛОГИКА ГОРЯЧИХ КЛАВИШ
    private void OnHotkey(WindowHotkey hotkey)
    {
        if (UIManager.Instance == null) return;

        bool isWindowOpen = hotkey.Window.IsOpened;
        bool isCorrectTab = hotkey.TabController == null || hotkey.TabController.CurrentTabIndex == hotkey.TargetTabIndex;

        // Если окно УЖЕ открыто, И мы находимся на НУЖНОЙ вкладке -> Закрываем окно!
        if (isWindowOpen && isCorrectTab)
            UIManager.Instance.CloseAll();
        else
        {
            // В противном случае -> Открываем окно (если закрыто) и переключаем вкладку
            if (UIManager.Instance.CurrentWindow == null)
                UIManager.Instance.OpenWindow(hotkey.Window);
            else if (UIManager.Instance.CurrentWindow != hotkey.Window)
                UIManager.Instance.CloseAll();

            if (hotkey.TabController != null)
            {
                hotkey.TabController.OpenTab(hotkey.TargetTabIndex);
            }
        }
    }
}