using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAnim", menuName = "Project/Animation Config")]
public class AnimationConfig : ScriptableObject
{
    [System.Serializable]
    public struct Frame
    {
        public Sprite Sprite;
        public float Duration;
        public bool ActivateEvent;
    }

    [Header("Animation Data")]
    [field: SerializeField] public string SpecialEventId;
    [field: SerializeField] public Frame[] Frames { get; private set; }

    [field: SerializeField] public bool Loop { get; private set; } = true;
    [field: SerializeField] public bool WaitUntilEnd { get; private set; } = false;

#if UNITY_EDITOR
    [BoxGroup("Setup Tool")]
    [SerializeField] private Sprite[] SpritesToImport;
    [BoxGroup("Setup Tool")]
    [SerializeField] private float DefaultDuration = 0.1f;

    [Button("Convert Sprites to Frames")]
    public void ConvertSprites()
    {
        int lenght = SpritesToImport.Length;
        if (lenght == 0) return;

        Frames = new Frame[lenght];
        for (int i = 0; i < lenght; i++)
        {
            Frames[i] = new()
            {
                Sprite = SpritesToImport[i],
                Duration = DefaultDuration
            };
        }

        SpritesToImport = null;
        Debug.Log($"Успешно добавлено {Frames.Length} кадров!");
    }

    [Button("Set Deafault Duration")]
    public void SetDeafaultDuration()
    {
        int lenght = Frames.Length;
        if (lenght == 0) return;

        for (int i = 0; i < lenght; i++)
            Frames[i].Duration = DefaultDuration;

        Debug.Log("Duration был установлен!");
    }

    [Button("Display All Duration")]
    public void DisplayAnimationDuration()
    {
        int lenght = Frames.Length;
        if (lenght == 0) return;

        float dur = 0;

        for (int i = 0; i < lenght; i++)
            dur += Frames[i].Duration;

        Debug.Log("Длительность всей анимации: " + dur);
    }
#endif
}