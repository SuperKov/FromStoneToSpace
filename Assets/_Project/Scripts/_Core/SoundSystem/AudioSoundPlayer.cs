using System.Collections.Generic;
using UnityEngine;

public class AudioSoundPlayer : MonoBehaviour
{
    [SerializeField] private int _poolSize = 4;

    private List<AudioSource> _sources = new();
    private int _currentIndex = 0;

    private void Awake()
    {
        // Создаем несколько источников про запас
        for (int i = 0; i < _poolSize; i++)
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            // Устанавливаем настройки по умолчанию для 3D
            s.spatialBlend = 1f;
            _sources.Add(s);
        }
    }

    public AudioSource GetAvailableSource()
    {
        AudioSource source = _sources[_currentIndex];

        // Сдвигаем индекс по кругу: 0, 1, 2, 0, 1...
        _currentIndex = (_currentIndex + 1) % _sources.Count;

        return source;
    }
}