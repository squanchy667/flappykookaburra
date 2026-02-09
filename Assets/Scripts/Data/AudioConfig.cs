using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "FlappyKookaburra/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    [Header("Sound Effects")]
    public AudioClip flapSound;
    public AudioClip scoreSound;
    public AudioClip collisionSound;

    [Header("Music")]
    public AudioClip ambientLoop;

    [Header("Volume")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
}
