using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class WorldUISlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;

    private SaveFileDescriptor _worldInfo;

    private Action _onDeleteCallback;

    public void Init(SaveFileDescriptor saveFile, Action onDeleteCallback)
    {
        _worldInfo = saveFile;

        _onDeleteCallback = onDeleteCallback;

        nameText.text = _worldInfo.Header.Name;
        dateText.text = $"Последняя игра: {saveFile.Header.LastSavedDate}";

        // Настройка кнопки загрузки
        loadButton.onClick.RemoveAllListeners();
        loadButton.onClick.AddListener(LoadWorld);

        // Настройка кнопки удаления
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(DeleteWorld);
        }
    }

    private void LoadWorld()
    {
        GameStateManager.IsNewWorld = false;
        GameStateManager.TargetWorld = _worldInfo; // Передаем только имя

        SceneLoader.Instance.LoadScene("GameWorld");
    }

    private void DeleteWorld()
    {
        SaveSystem.DeleteFile(_worldInfo.FilePath);

        // Говорим панели: "Я удалился, перечитай папку и обнови интерфейс!"
        _onDeleteCallback?.Invoke();
    }
}