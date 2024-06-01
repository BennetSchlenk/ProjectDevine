using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModifierUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI modifierName;
    [SerializeField] private Image modifierIcon;
    [SerializeField] private Transform modifierAttributesContainer;
    [SerializeField] private GameObject modifierAttributePrefab;

    [SerializeField] private Sprite attrDamageIcon;
    [SerializeField] private Sprite attrDamageOverTimeIcon;
    [SerializeField] private Sprite attrInfectionIcon;

    public void SetUp(DamageData modifier)
    {
        // Destroy previous attributes
        foreach (Transform child in modifierAttributesContainer)
        {
            Destroy(child.gameObject);
        }


        modifierName.text = modifier.DamageType.DamageTypeName;
        modifierIcon.sprite = modifier.DamageType.Sprite;

        // Create modifier attributes

        GameObject attrDamage = Instantiate(modifierAttributePrefab, modifierAttributesContainer);
        attrDamage.GetComponent<ModifierAttributeUI>().SetUp(modifier.Damage, attrDamageIcon);

        GameObject attrDamageOverTime = Instantiate(modifierAttributePrefab, modifierAttributesContainer);
        attrDamageOverTime.GetComponent<ModifierAttributeUI>().SetUp(modifier.DamageOverTime, modifier.DamageType.Sprite);

        GameObject attrDamageOverTimeDuration = Instantiate(modifierAttributePrefab, modifierAttributesContainer);
        attrDamageOverTimeDuration.GetComponent<ModifierAttributeUI>().SetUp(modifier.DamageOverTimeDuration, modifier.DamageType.Sprite);

        if (modifier.DamageType.IsInfection)
        {
            GameObject attrIsInfection = Instantiate(modifierAttributePrefab, modifierAttributesContainer);
            attrIsInfection.GetComponent<ModifierAttributeUI>().SetUp(0f, modifier.DamageType.Sprite, false);
        }
    }
}
