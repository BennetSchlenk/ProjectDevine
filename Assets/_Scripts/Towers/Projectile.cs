using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public void MoveTowardsTarget(Transform target, bool follow, float speed, List<GameObject> effects, List<GameObject> hitEffects, System.Action<Vector3> onHit)
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

        if (follow)
            StartCoroutine(MoveTowardsTargetEnumerator(target, speed, hitEffects, onHit));
        else
            StartCoroutine(MoveTowardsPositionEnumerator(target.position, speed, hitEffects, onHit));
    }

    private void HitTarget(Transform target, List<GameObject> hitEffects, System.Action<Vector3> onHit)
    {
        if (target == null)
        {
            Debug.Log("Target is null!!!!!!!!!!!!!");
            onHit(transform.position);
            Return();
            return;
        }

        foreach (GameObject hitEffect in hitEffects)
        {
            GameObject newHitEffect = Instantiate(hitEffect, target.transform.position, Quaternion.identity);
            newHitEffect.transform.SetParent(target.transform);
        }

        onHit(target.position);

        Return();
    }

    private IEnumerator MoveTowardsTargetEnumerator(Transform target, float speed, List<GameObject> hitEffects, System.Action<Vector3> onHit)
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

        Return();
    }

    private IEnumerator MoveTowardsPositionEnumerator(Vector3 targetPosition, float speed, List<GameObject> hitEffects, System.Action<Vector3> onHit)
    {
        Debug.Log("Moving towards position");
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            Vector3 direction = targetPosition - transform.position;
            float distanceThisFrame = speed * Time.deltaTime;

            //Debug.Log(direction.magnitude + ", " + distanceThisFrame);
            if (direction.magnitude <= distanceThisFrame)
            {
                HitTarget(null, hitEffects, onHit);
                yield break;
            }

            transform.Translate(direction.normalized * distanceThisFrame, Space.World);
            yield return null;
        }

        Return();
    }

    private void Return()
    {
        Destroy(gameObject);
    }
}
