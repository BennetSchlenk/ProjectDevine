using UnityEngine;
using UnityEngine.Serialization;

public class RefHolder : MonoBehaviour
{
    [HideInInspector]
    public string Path;
    [HideInInspector]
    public ScriptableObject scriptableData;
}
