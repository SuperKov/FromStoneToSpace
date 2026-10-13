using System.Collections.Generic;
using UnityEngine;

public class UIManager: MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Безопасная проверка: если список пуст, возвращаем null, иначе последнее окно
    public UIWindow CurrentWindow => _openedWindows.Count > 0 ? _openedWindows[^1] : null;

    private List<UIWindow> _openedWindows = new();

    private void Awake()
    {
        Instance = this;
    }

    public void OpenWindow(UIWindow window)
    {
        if (window == null) return;

        // Закрываем ВСЕ прошлые окна и очищаем список
        CloseAll();

        // Добавляем новое окно и открываем его
        _openedWindows.Add(window);
        window.Open();

        // Если у тебя есть InputManager, здесь логично заблокировать управление персонажем
        InputManager.Instance.LockPlayerActions();
    }

    public void CloseAll()
    {
        // Обязательно идем с конца списка (обратный цикл), чтобы корректно закрывать верхние окна
        for (int i = _openedWindows.Count - 1; i >= 0; i--)
        {
            _openedWindows[i].Close();
        }

        _openedWindows.Clear();

        // Разблокируем управление персонажем
        InputManager.Instance.UnlockPlayerActions(fullUnlock: true  );
    }

    public void AddWindow(UIWindow window, bool closePrevious = true)
    {
        if (window == null) return;
        if (_openedWindows.Contains(window)) return; // Защита от случайного двойного клика

        // Если в стеке уже есть окна, работаем с предыдущим
        if (_openedWindows.Count > 0)
        {
            UIWindow previousWindow = _openedWindows[^1];

            // Если флаг true, "усыпляем" прошлое окно, чтобы оно не мешало кликам и рендеру
            if (closePrevious)
            {
                previousWindow.Close();
            }
        }
        else
        {
            // Если стек был пуст, значит это первое открытое окно в игре (лочим инпут)
            InputManager.Instance.LockPlayerActions();
        }

        _openedWindows.Add(window);
        window.Open();
    }

    public void BackWindow()
    {
        if (_openedWindows.Count == 0) return;

        // 1. Берем самое верхнее окно, закрываем его и выкидываем из списка
        UIWindow topWindow = _openedWindows[^1];
        topWindow.Close();
        _openedWindows.RemoveAt(_openedWindows.Count - 1);

        // 2. Проверяем, осталось ли что-то под ним
        if (_openedWindows.Count > 0)
        {
            UIWindow previousWindow = _openedWindows[^1];

            // Если мы прятали прошлое окно через closePrevious = true, оно сейчас IsOpened == false.
            // Значит его нужно снова "разбудить" и показать игроку!
            if (!previousWindow.IsOpened)
            {
                previousWindow.Open();
            }
        }
        else
        {
            // 3. Если мы закрыли последнее окно и стек опустел — возвращаем игрока в игру
            InputManager.Instance.UnlockPlayerActions();
        }
    }
}