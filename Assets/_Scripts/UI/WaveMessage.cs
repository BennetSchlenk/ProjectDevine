using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveMessage : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI waveMessageText;

    #region Unity Callbacks
        
    private void Awake()
    {
        
    }

    private void Start()
    {
        if (waveMessageText == null)
            waveMessageText = GetComponentInChildren<TMPro.TextMeshProUGUI>();

        GlobalData.OnChangeWaveMessage += ChangeWaveMessage;
    }

    private void OnDestroy()
    {
        GlobalData.OnChangeWaveMessage -= ChangeWaveMessage;
    }

    #endregion

    private void ChangeWaveMessage(string message)
    {
        waveMessageText.text = message;
    }
}
