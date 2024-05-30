using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Core : MonoBehaviour
{
    [SerializeField] GameObject hitEffect;
    private AudioManager audioManager;
    private PlayerDataManager playerDataManager;
    

    #region Unity Callbacks

    private void Awake()
    {
        
    }

    private void Start()
    {
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();
        playerDataManager = ServiceLocator.Instance.GetService<PlayerDataManager>();

    }

    #endregion
    

    public float DamageCore(int incomingAmount)
    {
        
        if (incomingAmount > 0f)
        {
            playerDataManager.RemoveHP(incomingAmount);
            // Handle the damage taken, animations, effects, etc
            //Debug.Log($"<color=#E60000>The Core took {incomingAmount} damage</color>");
            audioManager.PlayerLooseHPSound(this.transform.position);
            if (hitEffect != null )
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
        }
        return incomingAmount;

    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.ReachedCore(this);
        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
