using UnityEngine.Audio;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSound", menuName = "Project/Sound Config")]
public class SoundData : ScriptableObject
{
    [Header("Sound")]
    [SerializeField] private AudioClip[] Clips;
    [SerializeField] private AudioMixerGroup Mixer;

    [Header("Space Settings. 2D / 3D")]
    [SerializeField] [Range(0, 1)] private float SpatialBlend  = 1f; // 2D / 3D
    [SerializeField] private float MinDistance = 1f;  // Расстояние, на котором звук максимально громкий
    [SerializeField] private float MaxDistance = 20f;

    [Header("Properties")]
    [SerializeField] private float Volume = 1f;
    [Range(0, 1f)]
    [SerializeField] private float DefaultPitch = 1f;
    [SerializeField] private float PitchRandomness = 0.1f;

    public void Play(AudioSoundPlayer player)
    {
        if (Clips.Length == 0) return;

        AudioSource source = player.GetAvailableSource();

        source.outputAudioMixerGroup = Mixer;
        source.spatialBlend = SpatialBlend;

        // Настройка дистанции для 3D
        source.minDistance = MinDistance;
        source.maxDistance = MaxDistance;
        source.rolloffMode = AudioRolloffMode.Linear; // Линейное затухание проще в балансе

        source.pitch = DefaultPitch + Random.Range(-PitchRandomness, PitchRandomness);
        source.PlayOneShot(Clips[Random.Range(0, Clips.Length)], Volume);
    }
}