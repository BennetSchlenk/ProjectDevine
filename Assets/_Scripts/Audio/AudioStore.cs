using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioStore : MonoBehaviour
{
    [SerializeField] private List<AudioObject> effects;
    [SerializeField] private List<AudioObject> musics;

    public List<AudioObject> Effects => effects;
    public List<AudioObject> Musics => musics;
}
