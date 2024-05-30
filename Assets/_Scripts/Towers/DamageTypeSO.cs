using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DamageType", menuName = "Project Divine/Towers/Damage Type")]
public class DamageTypeSO : ScriptableObject
{
    public string DamageTypeName;
    public Sprite Sprite;
    public GameObject HitEffect;
    public GameObject TrailEffect;
    public GameObject DamageOverTimeEffect;
    public bool IsInfection;
}
