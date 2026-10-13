public static class GameStateManager
{
    // Если true - генерируем новый мир. Если false - грузим из файла.
    public static bool IsNewWorld { get; set; }

    public static SaveFileDescriptor TargetWorld { get; set; }

}