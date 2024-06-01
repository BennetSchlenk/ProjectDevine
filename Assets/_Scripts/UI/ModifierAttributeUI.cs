using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModifierAttributeUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI attributeValue;
    [SerializeField] private Image attributeIcon;

    internal void SetUp(float value, Sprite attrDamageIcon, bool showValue = true)
    {
        attributeValue.text = value.ToString();
        attributeIcon.sprite = attrDamageIcon;
        attributeValue.enabled = showValue;
    }

    #region Unity Callbacks

    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }

    #endregion

}
