using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class BezierCurve : MonoBehaviour
{
    public AnimationCurve curve;
    public Quaternion tstQuat;
    [SerializeField]
    private GameObject testCard;

    [SerializeField]
    private bool alwaysSameRotation = false;

    [SerializeField]
    private float controlPointsSize;

    [Range(0, 1)]
    [SerializeField]
    private float tTest = 0;

    [SerializeField]
    private Transform[] controlPoints = new Transform[4];

    public Transform[] ControlPoints => controlPoints;

    Vector3 GetPos(int i) => controlPoints[i].position;


    private void Update()
    {
        testCard.transform.position = GetBezierPoint(tTest);
        testCard.transform.rotation = GetBezierOrientation(tTest) * Quaternion.Euler(Vector3.up * 90);
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        for (int i = 0; i < controlPoints.Length; i++)
        {
            Gizmos.DrawSphere(GetPos(i), controlPointsSize);
        }

        Handles.DrawBezier(GetPos(0), GetPos(3), GetPos(1), GetPos(2), Color.white, EditorGUIUtility.whiteTexture, 1f);


        Gizmos.color = Color.red;

        Vector3 testPoint = GetBezierPoint(tTest);
        Quaternion testOrientation = GetBezierOrientation(tTest);

        Gizmos.DrawSphere(testPoint, controlPointsSize);

        //Handles.PositionHandle(testPoint, testOrientation);

        Gizmos.color = Color.white;

#endif
    }

    public Quaternion GetBezierOrientation(float t)
    {
        Vector3 tangent = GetBezierTangent(t);

        if (!alwaysSameRotation)
            return Quaternion.LookRotation(tangent) * new Quaternion(0, 180, 0, 0);
        else
            return Quaternion.identity * tstQuat;
    }

    public Vector3 GetBezierPoint(float t)
    {
        Vector3 p0 = GetPos(0);
        Vector3 p1 = GetPos(1);
        Vector3 p2 = GetPos(2);
        Vector3 p3 = GetPos(3);

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        return Vector3.Lerp(d, e, t);
    }

    Vector3 GetBezierTangent(float t)
    {
        Vector3 p0 = GetPos(0);
        Vector3 p1 = GetPos(1);
        Vector3 p2 = GetPos(2);
        Vector3 p3 = GetPos(3);

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        return (e - d).normalized;
    }

    public Quaternion GetCardOrientation(float t)
    {
        return GetBezierOrientation(t) * Quaternion.Euler(Vector3.up * 90);
    }
}
