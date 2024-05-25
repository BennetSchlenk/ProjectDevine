using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public void MoveTowardsTarget(Transform target, float speed, List<GameObject> effects, List<GameObject> hitEffects, System.Action onHit)
    {
        foreach (GameObject effect in effects)
        {
            // Spawn effect. TODO: Pooling
            GameObject newEffect = Instantiate(effect, transform.position, Quaternion.identity);
            newEffect.transform.SetParent(transform);
        }

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(MoveTowardsTargetEnumerator(target, speed, hitEffects, onHit));
    }

    private void HitTarget(Transform target, List<GameObject> hitEffects, System.Action onHit)
    {
        if (target == null) return;
        
        foreach (GameObject hitEffect in hitEffects)
        {
            GameObject newHitEffect = Instantiate(hitEffect, target.transform.position, Quaternion.identity);
            newHitEffect.transform.SetParent(target.transform);
        }

        onHit();

        Return();
    }

    private IEnumerator MoveTowardsTargetEnumerator(Transform target, float speed, List<GameObject> hitEffects, System.Action onHit)
    {
        while (target != null)
        {
            Vector3 direction = target.position - transform.position;
            float distanceThisFrame = speed * Time.deltaTime;

            if (direction.magnitude <= distanceThisFrame)
            {
                HitTarget(target, hitEffects, onHit);
                yield break;
            }

            transform.Translate(direction.normalized * distanceThisFrame, Space.World);
            yield return null;
        }

        Return ();
    }

    private void Return()
    {
        Destroy(gameObject);
    }
}
