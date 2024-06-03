using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControls : MonoBehaviour
{
    private CameraControlActions cameraActions;
    private InputAction movement;
    private Camera camera;
    private Transform cameraTransform;

    //Horizontal
    [SerializeField] private float panMaxSpeed = 15f;
    private float speed;
    [SerializeField] private float panAcceleration = 10f;
    [SerializeField] private float panDampening = 15f;

    //Vertical
    [SerializeField] private float zoomStepSize = 2f;
    [SerializeField] private float zoomDampening = 7.5f;
    [SerializeField] private float zoomMinHeight = 13f;
    [SerializeField] private float zoomMaxHeight = 40f;
    [SerializeField] private float ZoomSpeed = 3f;

    //Rotation
    [SerializeField] private float rotationMaxSpeed = 5f;

    //For updating CameraRig Base object
    private Vector3 targetPosition;
    private float zoomHeight;

    private Vector3 horizontalVelocity;
    private Vector3 lastPosition;

    private Vector3 startDragMotion;

    private void Awake()
    {
        cameraActions = new CameraControlActions();
        camera = this.GetComponentInChildren<Camera>();
        cameraTransform = camera.transform;
    }

    private void OnEnable()
    {
        lastPosition = this.transform.position;
        zoomHeight = cameraTransform.localPosition.y;
        cameraTransform.LookAt(this.transform);
        movement = cameraActions.Camera.Movement;
        cameraActions.Camera.Rotate.performed += RotateCamera;
        cameraActions.Camera.Zoom.performed += ZoomCamera;
        cameraActions.Camera.Enable();
    }

    private void OnDisable()
    {
        cameraActions.Camera.Rotate.performed -= RotateCamera;
        cameraActions.Camera.Zoom.performed -= ZoomCamera;
        cameraActions.Camera.Disable();
    }

    void Update()
    {
        GetKeyboardMovement();
        DragMoveCamera();
        UpdateCamRigVelocity();
        UpdateCameraPosition();
        UpdateCamRigBase();
    }

    private void UpdateCamRigVelocity()
    {
        var position = this.transform.position;
        if (Time.deltaTime != 0)
        {
            horizontalVelocity = (position - lastPosition) / Time.deltaTime;
        }
        else
        {
            horizontalVelocity = Vector3.zero; // Set horizontalVelocity to zero when paused
        }
        horizontalVelocity.y = 0;


        lastPosition = position;
    }

    private void GetKeyboardMovement()
    {
        Vector3 inputValue = movement.ReadValue<Vector2>().x * GetCameraRight() +
                             movement.ReadValue<Vector2>().y * GetCameraForward();

        inputValue = inputValue.normalized;
        targetPosition += inputValue;
    }

    private Vector3 GetCameraForward()
    {
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        return cameraForward;
    }

    private Vector3 GetCameraRight()
    {
        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0;
        return cameraRight;
    }

    private void UpdateCamRigBase()
    {
        if (targetPosition.sqrMagnitude > 0.1f)
        {
            speed = Mathf.Lerp(speed, panMaxSpeed, Time.deltaTime * panAcceleration);
            this.transform.position += targetPosition * (speed * Time.deltaTime);
        }
        else
        {
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.deltaTime * panDampening);
            this.transform.position += horizontalVelocity * Time.deltaTime;
        }

        targetPosition = Vector3.zero;
    }

    private void RotateCamera(InputAction.CallbackContext inputVal)
    {
        if (!Mouse.current.middleButton.isPressed) return;

        float val = inputVal.ReadValue<Vector2>().x;
        transform.rotation = Quaternion.Euler(0f,
            transform.rotation.eulerAngles.y + (val * rotationMaxSpeed) * Time.deltaTime, 0f);
    }

    private void ZoomCamera(InputAction.CallbackContext inputVal)
    {
        float val = -inputVal.ReadValue<Vector2>().y / 100f;

        if (Mathf.Abs(val) > 0.1f)
        {
            zoomHeight = cameraTransform.localPosition.y + val * zoomStepSize;
            if (zoomHeight < zoomMinHeight)
            {
                zoomHeight = zoomMinHeight;
            }
            else if (zoomHeight > zoomMaxHeight)
            {
                zoomHeight = zoomMaxHeight;
            }
        }
    }

    private void UpdateCameraPosition()
    {
        var localCameraPosition = cameraTransform.localPosition;
        Vector3 zoomTarget = new Vector3(localCameraPosition.x, zoomHeight, localCameraPosition.z);
        zoomTarget -= ZoomSpeed * (zoomHeight - localCameraPosition.y) * Vector3.forward;

        cameraTransform.localPosition =
            Vector3.Lerp(cameraTransform.localPosition, zoomTarget, zoomDampening * Time.deltaTime);

        cameraTransform.LookAt(this.transform);
    }


    private void DragMoveCamera()
    {
        if (!Mouse.current.rightButton.isPressed)
            return;

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (plane.Raycast(ray, out float distance))
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                startDragMotion = ray.GetPoint(distance);
            }
            else
            {
                targetPosition += startDragMotion - ray.GetPoint(distance);
            }
        }
    }
}