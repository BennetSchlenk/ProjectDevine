using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleShake : MonoBehaviour
{
    [Tooltip("The target object to shake. If empty, use this .")]
    [SerializeField] private bool makeParentTarget = true;
    [SerializeField] private bool triggerOnStart = true;
    [SerializeField] private Transform target;

    [SerializeField] private Vector3 targetScale = new Vector3(1.1f, 1.1f, 1.1f);
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Ease scaleEase = Ease.InOutCubic;
    [SerializeField] private int loops = 2;
    [SerializeField] private LoopType loopType = LoopType.Yoyo;

    private void Start()
    {
        if (makeParentTarget)
            target = transform.parent.transform;
        
        if (triggerOnStart)
            Trigger();
    }

    public void Trigger()
    {

        if (target == null)
        {
            target = gameObject.transform;
        }

        target.DOScale(targetScale, duration).SetEase(scaleEase).SetLoops(loops, loopType);
    }

}
