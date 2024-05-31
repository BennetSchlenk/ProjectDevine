using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardMovement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public event Action<CardMovement> OnCardClicked = delegate { };
    public event Action<CardMovement> OnCardDragged = delegate { };
    public event Action<CardMovement> OnCardDropped = delegate { };
    public event Action<CardMovement> OnCardRemove = delegate { };

    [SerializeField] private float scaleAnimationSpeed = 10f;
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private AnimationCurve movementCurve;

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

    #region Unity Callbacks

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void Start()
    {
        _initialLocalScale = transform.localScale;
        SetTargetScale(_initialLocalScale);
        targetPosition = _targetPosition;
        targetRotation = _targetRotation.eulerAngles;
    }

    void Update()
    {

        if (_isMoving)
        {
            _currentMovementTime += Time.deltaTime / _targetTime;
            float curveProgress = movementCurve.Evaluate(_currentMovementTime / _targetTime);
            transform.position = Vector3.Lerp(_startPosition, _targetPosition, curveProgress);
            transform.rotation = Quaternion.Lerp(_startRotation, _targetRotation, curveProgress);

            if (_currentMovementTime >= _targetTime) _isMoving = false;

        }
        else
        {
            HandleCardScaleAnimation();
        }
        
    }

    private void OnDisable()
    {
        OnCardRemove(this);
    }

    private void OnDestroy()
    {
        OnCardRemove(this);
        button.onClick.RemoveListener(OnClick);
    }

    #endregion

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

    public void SetHoveredPositionTransform(Transform gameObjectTransform) => hoverPositionTransform = gameObjectTransform;

    public bool CanBeUsed() => true;

    public bool IsChooseTargetCard() => true;
    private void SetTargetPositionWithHoverPositionY()
    {
        // Take the global Y position from hoverPositionTransform
        var hoverPositionTransformY = hoverPositionTransform.position.y;

        targetPosition = new Vector3(_targetPosition.x, hoverPositionTransformY, _targetPosition.z);
        targetRotation = new Vector3(_targetRotation.x, _targetRotation.y, 0f);
    }

    private void SetTargetScale(Vector3 scale)
    {
        targetScale = scale;
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

    #region Event Handlers

    public void OnMouseDown()
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
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetTargetScale(mouseOverLocalScale);
        SetTargetPositionWithHoverPositionY();

        // Get the current sibling index
        currentSiblingIndex = transform.GetSiblingIndex();

        // Make this gameobject show on top of other gameobjects
        transform.SetAsLastSibling();
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        SetTargetScale(_initialLocalScale);
        targetPosition = _targetPosition;
        targetRotation = _targetRotation.eulerAngles;

        // Return the gameobject to its original sibling index
        transform.SetSiblingIndex(currentSiblingIndex);
    }

    public void OnBeginDrag(PointerEventData eventData) => OnCardDragged(this);

    public void OnEndDrag(PointerEventData eventData) => OnCardDropped(this);

    public void OnDrag(PointerEventData eventData)
    {
    }

    private void OnClick() => OnCardClicked(this);

    #endregion
}
