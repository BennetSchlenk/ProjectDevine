using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            Debug.LogError("Grid is not assigned to ObjectPlacer!");

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
                if (Mouse.current.leftButton.wasPressedThisFrame && node != null)
                    TryPlaceInNode(node);

                // Stop placing if right mouse button is pressed
                if (Mouse.current.rightButton.wasPressedThisFrame)
                    StopPlacing(true);
            }
        }
        else
        {
            // If not placing, select the current ISelectable object from the node
            if (Mouse.current.leftButton.wasPressedThisFrame)
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

    #endregion

    public void StartPlacingTestTower()
    {
       SetObjectToPlace(testInitialObject);
       StartPlacing();
    }

    public void SetObjectToPlace(GameObject objectToPlace)
    {
        this.objectToPlace = objectToPlace;
    }

    public void StartPlacing()
    {
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

    private void TryPlaceInNode(GridNode node)
    {
        if (node.Buildable)
        {
            // Place object
            instantiatedObject.transform.position = node.Position;
            instantiatedObjectPlaceable.OnPlaced();
            node.Buildable = false;
            node.TowerObj = instantiatedObject;
            instantiatedObject.transform.SetParent(towersContainerTransform);
            StopPlacing();
        }
        else
        {
            Debug.Log("Node is not buildable!");
            if (destroyObjectIfNotPlaced)
                StopPlacing(true);
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
