// Этот интерфейс вешается на контроллеры (или модели в MVP), которые хотят сохраняться
public interface ISaveable
{
    string SaveID { get; } // Уникальный ID (GUID)
    string SaveState(); // Упаковать себя в строку (JSON)
    void LoadState(string stateJson); // Распаковать себя из строки
}