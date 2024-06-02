using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField] private bool destroyObjectIfNotPlaced = true;
    [SerializeField] private CardDataSO testInitialCardData;
    [SerializeField] private Grid grid;

    [Header("Towers Settings")]
    [SerializeField] private GameObject towerRootPrefab;
    [SerializeField] private GameObject modifierPlaceholderPrefab;

    private CardDataSO cardToUse;
    private GameObject instantiatedObject;
    private IPlaceable instantiatedObjectPlaceable;
    private bool isPlacing;
    private Camera mainCam;
    private Transform towersContainerTransform;
    private ISelectable lastSelected;
    private bool placementCalledFromClick; // Used to differentiate between click and drag placement
    
    // Parameters passed from the card to the object placer
    private Action onPlacingSuccess;
    private Action onPlacingFail;
    private bool lastIsClick;
    private AudioManager audioManager;

    #region Unity Callbacks
        
    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Start()
    {
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();

        if (testInitialCardData != null)
        {
            SetCardToUse(testInitialCardData);
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
                {
                    switch (cardToUse.Type)
                    {
                        case CardType.Tower:

                            // Check if a tower is not placed in the node
                            if (node.TowerObj == null)
                            {
                                TryPlaceTowerInEmptyNode(node);
                            }
                            else
                            {
                                // Trying to place a tower in a node with a tower

                                var tower = node.TowerObj.GetComponent<Tower>();
                                // Upgrade tower if possible
                                bool upgraded = tower.ApplyUpgrade(cardToUse);
                                Debug.Log("Tower upgraded: " + upgraded);
                                if (upgraded)
                                {
                                    StopPlacing(true);
                                    onPlacingSuccess?.Invoke();
                                }
                                else
                                {
                                    if (destroyObjectIfNotPlaced)
                                        StopPlacing(true);
                                    onPlacingFail?.Invoke();
                                    PlayPlaceFailSFX(false);
                                }
                            }
                            break;
                        case CardType.Modifier:
                            if (node.TowerObj != null)
                            {
                                // Get tower
                                var tower = node.TowerObj.GetComponent<Tower>();

                                // Apply modifier to tower if possible
                                bool isApplied = tower.ApplyModifier(cardToUse);

                                if (isApplied)
                                {
                                    StopPlacing(true);
                                    onPlacingSuccess?.Invoke();
                                }
                                else
                                {
                                    if (destroyObjectIfNotPlaced)
                                        StopPlacing(true);
                                    onPlacingFail?.Invoke();
                                    PlayPlaceFailSFX(false);
                                }
                            }
                            else
                            {
                                // Can't apply modifier to an empty node
                                if (destroyObjectIfNotPlaced)
                                    StopPlacing(true);
                                onPlacingFail?.Invoke();
                                PlayPlaceFailSFX(false);
                            }
                            break;
                    }

                }

                // Stop placing if right mouse button is pressed
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    StopPlacing(true);
                    onPlacingFail?.Invoke();
                    PlayPlaceFailSFX(true);
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
                            selectable.Select(true);
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
       SetCardToUse(testInitialCardData);
       StartPlacing();
    }

    /// <summary>
    /// Set up the object to place and the actions to be called when placing is successful or fails.
    /// Example: Call this method from a card click to set the object to place and the actions to be called when placing is successful or fails.
    /// </summary>
    /// <param name="cardData">Prefab to instantiate</param>
    /// <param name="onSuccess"></param>
    /// <param name="onFail"></param>
    /// <param name="isClick">If the player clicked a card (true) or dragged (false) to call this method</param>
    public void UseCard(CardDataSO cardData, Action onSuccess, Action onFail, bool isClick)
    {
        //Debug.Log("Setting up card: " + cardData.Name + " for placing.", cardData);
        SetCardToUse(cardData);
        onPlacingSuccess = onSuccess;
        onPlacingFail = onFail;
        lastIsClick = isClick;
        StartPlacing(cardData, isClick);
    }

    private void SetCardToUse(CardDataSO cardData)
    {
        if (this.cardToUse != null && cardData != null)
            StopPlacing(true);

        this.cardToUse = cardData;
    }

    public void StartPlacing(CardDataSO cardData = null, bool isClick = false)
    {
        if (cardData != null) SetCardToUse(cardData);

        // TODO: Implement different behavior for different card types
        placementCalledFromClick = isClick;
        isPlacing = true;
        // Tower card
        
        switch (cardData.Type)
        {
            case CardType.Tower:
                // Instantiate tower root prefab and call OnPlacing
                instantiatedObject = Instantiate(towerRootPrefab);
                instantiatedObjectPlaceable = instantiatedObject.GetComponent<IPlaceable>();
                

                // Instantiate tower model
                GameObject model = Instantiate(cardToUse.TowerPrefab);
                
                Tower tower = instantiatedObject.GetComponentInChildren<Tower>();
                tower.SetUp(model, cardData, cardData.TowerTier);

                // Call OnPlacing after SetUp so the tower model is already set up and the range is updated
                instantiatedObjectPlaceable.OnPlacing();

                break;
            case CardType.Modifier:
                instantiatedObject = Instantiate(modifierPlaceholderPrefab);
                instantiatedObjectPlaceable = instantiatedObject.GetComponent<IPlaceable>();
                instantiatedObjectPlaceable.OnPlacing();
                break;
        }
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

    private bool TryPlaceTowerInEmptyNode(GridNode node)
    {
        if (node == null)
        {
            onPlacingFail?.Invoke();
            PlayPlaceFailSFX(false);
            return false;
        }

        //Debug.Log("Trying to place object in node: " + node.GridX + " / " + node.GridY);

        if (node.Buildable)
        {
            // Place object
            //Debug.Log("Placing object in node: " + node.GridX + " / " + node.GridY);
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
            //Debug.Log("Node is not buildable!");
            if (destroyObjectIfNotPlaced)
                StopPlacing(true);

            onPlacingFail?.Invoke();
            PlayPlaceFailSFX(false);
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

    private void PlayPlaceFailSFX(bool isCancelled)
    {
        string sfxName = isCancelled ? "cardClick" : "cardPlaceWrong";
        audioManager.PlaySFXOneShotAtPosition(sfxName, transform.position);

    }
}
