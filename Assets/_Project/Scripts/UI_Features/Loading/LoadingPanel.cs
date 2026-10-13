using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class LoadingPanel : MonoBehaviour
{
    [SerializeField] private GameObject LoadingScreen;

    [Header("UI")]
    [SerializeField] private Image ProgressBar;

    private Coroutine _currentLoading;

    private void Start()
    {
        LoadingScreen.SetActive(false);
    }

    // Вызываем этот метод отовсюду
    public void LoadScene(string sceneName)
    {
        if (_currentLoading != null) return;

        _currentLoading = StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 1. Показываем экран загрузки
        LoadingScreen.SetActive(true);

        // 2. Начинаем асинхронную загрузку сцены
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // Запрещаем юнити автоматически включать сцену, пока она не загрузится на 100%
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // Обновляем ползунок (Unity считает загрузку завершенной на 0.9f)
            if (ProgressBar != null)
                ProgressBar.fillAmount = Mathf.Clamp01(operation.progress / 0.9f);

            if (operation.progress >= 0.9f)
            {
                // Можно добавить искусственную задержку yield return new WaitForSeconds(0.5f);
                // чтобы игрок успел прочитать подсказку на экране.

                // Разрешаем активацию сцены
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // 3. Сцена загрузилась. Прячем экран загрузки.
        LoadingScreen.SetActive(false);

        _currentLoading = null;
    }
}