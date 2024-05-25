using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourcePool : PollingPool<AudioSource>
{
    public AudioSourcePool(AudioSource prefab, Transform transform, int prewarmSize) : base(prefab, transform, prewarmSize)
    {
    }

    protected override bool IsActive(AudioSource component)
    {
        return component.isPlaying;
    }

    public SoundEmitter Request()
    {
        var source = Get();
        return source.GetComponent<SoundEmitter>();
    }
}
