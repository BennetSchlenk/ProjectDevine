using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Core : MonoBehaviour, IDamagable
{
    [SerializeField] float initialHealth = 500f;
    [SerializeField] GameObject hitEffect;
    private AudioManager audioManager;

    public float Health { get; set; }

    #region Unity Callbacks

    private void Awake()
    {
        
    }

    private void Start()
    {
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();
        Health = initialHealth;
    }

    #endregion

    public float InstantKill()
    {
        DestroySelf();
        return Health;
    }

    public float TakeDamage(float incomingAmount)
    {
        float damageTaken;

        Health = Mathf.Clamp(Health - incomingAmount, 0f, Health);

        damageTaken = Health - (Health - incomingAmount);

        if (damageTaken > 0f)
        {
            // Handle the damage taken, animations, effects, etc
            Debug.Log($"<color=#E60000>The Core took {damageTaken} damage</color>");
            audioManager.PlayerLooseHPSound(this.transform.position);
            if (hitEffect != null )
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            // trigger death of enemy
            if (Health <= 0f)
            {
                // Trigger GameOver!
                Debug.Log("<color=#00FF00><b>---=== GAME OVER, YOU ARE DONE :D ===---</b></color>");
                
                //DestroySelf();
            }
        }
        return damageTaken;

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
