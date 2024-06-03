using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioStore", menuName = "Project Divine/Audio/AudioStoreSO")]
public class AudioStoreSO : ScriptableObject
{
    [SerializeField] private List<AudioObject> effects;
    [SerializeField] private List<AudioObject> musics;

    public List<AudioObject> Effects => effects;
    public List<AudioObject> Musics => musics;
}
