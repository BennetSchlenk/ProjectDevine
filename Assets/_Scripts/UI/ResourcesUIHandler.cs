using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesUIHandler : MonoBehaviour
{
    private PlayerDataManager playerDataManager;
    [SerializeField]
    private TMP_Text essenceText;
    [SerializeField]
    private TMP_Text percentHPText;
    [SerializeField]
    private Image hpImage;
    

    #region Unity Callbacks
        
    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }
    
    private void OnEnable()
    {
        playerDataManager = ServiceLocator.Instance.GetService<PlayerDataManager>();
        var initValues = playerDataManager.GetInitialValues();
        UpdateHPDisplay(initValues.Item1, initValues.Item1);
        UpdateEssenceDisplay(initValues.Item2);
        
        
        playerDataManager.OnHPChanged += UpdateHPDisplay;
        playerDataManager.OnEssenceChanged += UpdateEssenceDisplay;
    }
    private void OnDisable()
    {
        playerDataManager.OnHPChanged -= UpdateHPDisplay;
        playerDataManager.OnEssenceChanged -= UpdateEssenceDisplay;
    }
    #endregion
    
    private void UpdateHPDisplay(int currHp, int maxHp)
    {
        float percentageZeroOneRange = 0;
        float percentage = 0;

        if (currHp > 0)
        {
            percentageZeroOneRange = (float)currHp / maxHp;
            percentage = percentageZeroOneRange * 100;
        }
        else
        {
            percentageZeroOneRange = 0;
            percentage = 0;
        }


        hpImage.fillAmount = percentageZeroOneRange;
        percentHPText.text = percentage + "%";
    }
    private void UpdateEssenceDisplay(int newEssence)
    {
        essenceText.text = newEssence.ToString();
    }

}
