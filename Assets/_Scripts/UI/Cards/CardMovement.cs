using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardMovement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [SerializeField] private float scaleAnimationSpeed = 10f;
    [SerializeField] private Vector3 positionOffset;

    private bool _isMoving = false;
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private Quaternion _startRotation;
    private Quaternion _targetRotation;
    private float _targetTime;
    private float _currentMovementTime;

    [SerializeField] private Vector3 mouseOverLocalScale;
    [SerializeField] private Transform[] arrowControlPoints;

    private Vector3 _initialLocalScale;
    private Button button;
    private Vector3 targetScale;
    private Vector3 targetPosition;
    private Vector3 targetRotation;
    private int currentSiblingIndex;
    private Transform hoverPositionTransform;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {

        _initialLocalScale = transform.localScale;
        targetScale = _initialLocalScale;
        targetPosition = _targetPosition;
        targetRotation = _targetRotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {

        if (_isMoving)
        {
            //Debug.LogFormat("{0} - {1} / {2} - {3} / {4}", transform.gameObject.name, _currentMovementTime, _targetTime, transform.position, _targetPosition);
            _currentMovementTime += Time.deltaTime / _targetTime;
            transform.position = Vector3.Lerp(_startPosition, _targetPosition, _currentMovementTime / _targetTime);
            transform.rotation = Quaternion.Lerp(_startRotation, _targetRotation, _currentMovementTime / _targetTime);

            if (_currentMovementTime >= _targetTime) _isMoving = false;

        }
        else
        {
            HandleCardScaleAnimation();
        }
        
    }

    public void MoveToPosition(Vector3 position, Quaternion rotation, float time)
    {
        var t = transform;
        _startPosition = t.position;
        _startRotation = t.rotation;

        _targetPosition = position;
        _targetRotation = rotation;
        targetPosition = _targetPosition;
        targetRotation = _targetRotation.eulerAngles;
        _targetTime = time;
        _currentMovementTime = 0f;

        _isMoving = true;
    }

    public void SetHoveredPositionTransform(Transform gameObjectTransform)
    {
        hoverPositionTransform = gameObjectTransform;
    }

    public bool CanBeUsed()
    {
        return true;
    }

    public bool IsChooseTargetCard()
    {
        return true;
    }

    private void OnMouseDown()
    {
        if (!CanBeUsed()) return;

        if (IsChooseTargetCard())
        {
            Vector3[] points = new Vector3[arrowControlPoints.Length];
            for (int i = 0; i < arrowControlPoints.Length; i++)
            {
                points[i] = arrowControlPoints[i].position;
                Debug.LogFormat("{0}:{1}", i, points[i]);
            }
            Debug.Log("Finished");
        }
        else
        {

        }
    }

    private void OnMouseUp()
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter");
        targetScale = mouseOverLocalScale;

        // Take the global Y position from hoverPositionTransform
        var hoverPositionTransformY = hoverPositionTransform.position.y;

        targetPosition = new Vector3(_targetPosition.x, hoverPositionTransformY, _targetPosition.z);
        targetRotation = new Vector3(_targetRotation.x, _targetRotation.y, 0f);
        // Get the current sibling index
        currentSiblingIndex = transform.GetSiblingIndex();
        // Make this gameobject show on top of other gameobjects
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = _initialLocalScale;
        targetPosition = _targetPosition;
        targetRotation = _targetRotation.eulerAngles;
        // Return the gameobject to its original sibling index
        transform.SetSiblingIndex(currentSiblingIndex);
    }

    private void HandleCardScaleAnimation()
    {
        if (transform.localScale != targetScale)
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleAnimationSpeed);
        if (transform.position != targetPosition)
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * scaleAnimationSpeed);
        if (transform.rotation.eulerAngles != targetRotation)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime * scaleAnimationSpeed);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        //transform.localScale = mouseOverLocalScale;
    }
}
