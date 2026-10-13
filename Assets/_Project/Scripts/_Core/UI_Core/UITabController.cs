using UnityEngine.UI;
using UnityEngine;

public class UITabController : UIWindow
{
    [System.Serializable]
    public struct Tab
    {
        public Button TabButton;
        public GameObject ContentPanel;
    }

    [SerializeField] private Tab[] tabs;

    // Свойство, чтобы Роутер мог узнать, какая вкладка сейчас открыта
    public int CurrentTabIndex { get; private set; } = -1;

    private void OnEnable()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            if (tabs[i].TabButton != null)
                tabs[i].TabButton.onClick.AddListener(() => OpenTab(index));
        }
    }

    private void OnDisable()
    {
        foreach (var tab in tabs)
            if (tab.TabButton != null)
                tab.TabButton.onClick.RemoveAllListeners();
    }

    public void OpenTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabs.Length) return;

        CurrentTabIndex = tabIndex;

        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActive = (i == tabIndex);
            tabs[i].ContentPanel.SetActive(isActive);

            if (tabs[i].TabButton != null)
                tabs[i].TabButton.interactable = !isActive; // Отключаем кнопку активной вкладки
        }
    }
}