using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class CreateWorldPanel : MonoBehaviour
{
    [SerializeField] private Button CreateButton;
    [SerializeField] private TMP_InputField NameInput;
    [SerializeField] private TMP_InputField SeedInput;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameWorld"; // Имя сцены с игрой

    private void OnEnable()
    {
        CreateButton.onClick.AddListener(CreateWorld);
    }

    private void OnDisable()
    {
        CreateButton.onClick.RemoveListener(CreateWorld);
    }

    public void CreateWorld()
    {
        // 1. Получаем имя (если поле пустое, ставим заглушку)
        string worldName = string.IsNullOrWhiteSpace(NameInput.text) ? "New World" : NameInput.text;

        // 2. Генерируем или парсим сид
        int seed;
        if (string.IsNullOrWhiteSpace(SeedInput.text))
            // Если поле пустое - случайный сид
            seed = Random.Range(int.MinValue, int.MaxValue);
        else
        {
            // Пытаемся распарсить как число. 
            // Если игрок ввел текст (например "abracadabra"), получаем числовой хэш из строки
            if (!int.TryParse(SeedInput.text, out seed))
                seed = SeedInput.text.GetHashCode();
        }

        // 3. Упаковываем данные в статический класс-посредник
        GameStateManager.IsNewWorld = true;
        GameStateManager.TargetWorld = new()
        {
            Header = new()
            {
                Name = worldName,
                Seed = seed
            }
        };

        // 4. Запускаем загрузчик
        SceneLoader.Instance.LoadScene(gameSceneName);
    }
}