using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField] private bool destroyObjectIfNotPlaced = true;
    [SerializeField] private GameObject testInitialObject;
    [SerializeField] private Grid grid;

    private GameObject objectToPlace;
    private GameObject instantiatedObject;
    private IPlaceable instantiatedObjectPlaceable;
    private bool isPlacing;
    private Camera mainCam;
    private float gridCellSize;
    private Transform towersContainerTransform;
    private ISelectable lastSelected;
    private bool placementCalledFromClick; // Used to differentiate between click and drag placement
    
    // Parameters passed from the card to the object placer
    private Action onPlacingSuccess;
    private Action onPlacingFail;
    private bool lastIsClick;

    #region Unity Callbacks
        
    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Start()
    {
        if (testInitialObject != null)
        {
            SetObjectToPlace(testInitialObject);
        }

        if (grid  == null)
            grid = GameObject.FindObjectOfType<Grid>();

        var gridTowersTr = grid.transform.Find("Towers");
        if (gridTowersTr != null)
        {
            towersContainerTransform = gridTowersTr.transform;
        }
        else
        {
            towersContainerTransform = new GameObject("Towers").transform;
            towersContainerTransform.SetParent(grid.transform);
        }

        gridCellSize = GlobalData.GridNodeSize;
    }

    private void Update()
    {
        // Detect every frame if the mouse is over a grid node
        GridNode node = GetGridNodeFromMousePosition();

        // If placing, check if the object can be placed in the node
        if (isPlacing)
        {
            if (instantiatedObject != null)
            {
                // Snap object to grid
                if (node != null)
                    instantiatedObject.transform.position = node.Position;

                // Try to place object in node
                if (GetWasPlaceButtonTriggered() && node != null)
                    TryPlaceInNode(node);

                // Stop placing if right mouse button is pressed
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    StopPlacing(true);
                    onPlacingFail?.Invoke();
                }
            }
        }
        else
        {
            // If not placing, select the current ISelectable object from the node
            if (GetWasPlaceButtonTriggered())
            {
                if (node == null)
                {
                    lastSelected?.Deselect();
                }
                else
                {
                    if (node.TowerObj == null)
                    {
                        lastSelected?.Deselect();
                    }
                    else
                    {
                        if (lastSelected != null)
                            lastSelected.Deselect();

                        var selectable = node.TowerObj.GetComponent<ISelectable>();
                        if (selectable != null)
                        {
                            selectable.Select();
                            lastSelected = selectable;
                        }
                    }
                }
            }
        }
    }

    private bool GetWasPlaceButtonTriggered()
    {
        if (placementCalledFromClick)
        {
                return Mouse.current.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject();
        }
        else
            return placementCalledFromClick ? Mouse.current.leftButton.wasPressedThisFrame : Mouse.current.leftButton.wasReleasedThisFrame;
    }

    #endregion

    public void StartPlacingTestTower()
    {
       SetObjectToPlace(testInitialObject);
       StartPlacing();
    }

    /// <summary>
    /// Set up the object to place and the actions to be called when placing is successful or fails.
    /// Example: Call this method from a card click to set the object to place and the actions to be called when placing is successful or fails.
    /// </summary>
    /// <param name="objectToPlace">Prefab to instantiate</param>
    /// <param name="onSuccess"></param>
    /// <param name="onFail"></param>
    /// <param name="isClick">If the player clicked a card (true) or dragged (false) to call this method</param>
    public void SetUpPlacing(GameObject objectToPlace, Action onSuccess, Action onFail, bool isClick)
    {
        SetObjectToPlace(objectToPlace);
        onPlacingSuccess = onSuccess;
        onPlacingFail = onFail;
        lastIsClick = isClick;
        StartPlacing(objectToPlace, isClick);
    }

    private void SetObjectToPlace(GameObject objectToPlace)
    {
        if (this.objectToPlace != null && objectToPlace != null)
            StopPlacing(true);

        this.objectToPlace = objectToPlace;
    }

    public void StartPlacing(GameObject obj = null, bool isClick = false)
    {
        if (obj != null) SetObjectToPlace(obj);

        placementCalledFromClick = isClick;
        isPlacing = true;
        instantiatedObject = Instantiate(objectToPlace);
        instantiatedObjectPlaceable = instantiatedObject.GetComponent<IPlaceable>();
        instantiatedObjectPlaceable.OnPlacing();
    }

    public void StopPlacing(bool destroy = false)
    {
        isPlacing = false;

        if (destroy)
            Destroy(instantiatedObject);

        if (instantiatedObject != null)
            instantiatedObject = null;

        if (instantiatedObjectPlaceable != null)
            instantiatedObjectPlaceable = null;
    }

    private bool TryPlaceInNode(GridNode node)
    {
        if (node == null)
        {
            onPlacingFail?.Invoke();
            return false;
        }

        Debug.Log("Trying to place object in node: " + node.GridX + " / " + node.GridY);

        if (node.Buildable)
        {
            // Place object
            Debug.Log("Placing object in node: " + node.GridX + " / " + node.GridY);
            instantiatedObject.transform.position = node.Position;
            instantiatedObjectPlaceable.OnPlaced();
            node.Buildable = false;
            node.TowerObj = instantiatedObject;
            instantiatedObject.transform.SetParent(towersContainerTransform);
            StopPlacing();
            onPlacingSuccess?.Invoke();
            return true;
        }
        else
        {
            Debug.Log("Node is not buildable!");
            if (destroyObjectIfNotPlaced)
                StopPlacing(true);

            onPlacingFail?.Invoke();
            return false;
        }
    }

    private GridNode GetGridNodeFromMousePosition()
    {
        // if (!Mouse.current.leftButton.wasPressedThisFrame) return null;

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());


        if (plane.Raycast(ray, out float distance))
        {
            var hitPoint = ray.GetPoint(distance);
            
            if (hitPoint.x < -grid.GridSize.x || hitPoint.x > grid.GridSize.x || hitPoint.z < -grid.GridSize.y || hitPoint.z > grid.GridSize.y)
            {
                return null;
            }
            else
            {
                //Node can be null if clicked outside of grid area
                var node = grid.NodeFromWorldPosition(hitPoint);
                //Debug.Log("NODE: " + node.GridX + " / " + node.GridY + "  was clicked!");
                return node;
            }            
        }

        return null;
    }
}
