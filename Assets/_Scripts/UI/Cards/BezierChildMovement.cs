using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierChildMovement : MonoBehaviour
{
    [SerializeField]
    private BezierCurve _bezierCurve;

    [SerializeField] private float _speed;

    public float T;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (_bezierCurve == null) return;

        T += Time.deltaTime * _speed;
        if (T > 1f) T = 0f;

        transform.position = _bezierCurve.GetBezierPoint(T);
        transform.rotation = _bezierCurve.GetCardOrientation(T);
    }

    public void SetBezierCurve(BezierCurve bezierCurve)
    {
        _bezierCurve = bezierCurve;
    }
}
