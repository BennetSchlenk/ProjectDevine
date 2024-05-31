using UnityEngine;

[System.Serializable]
public class AudioObject
{
    public string Name;
    public AudioClip Clip;
    [Range(0f, 1f)] public float Volume = 1f;
    [Range(-3f, 3f)] public float Pitch = 1f;
}
