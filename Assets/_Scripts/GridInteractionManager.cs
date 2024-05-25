using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class GridInteractionManager : MonoBehaviour
{
    [SerializeField]
    private Grid grid;
    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        GetGridNodeFromRaycast();
    }

    private void GetGridNodeFromRaycast()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());


        if (plane.Raycast(ray, out float distance))
        {
            var hitPoint = ray.GetPoint(distance);
            var nodeHit = grid.NodeFromWorldPosition(hitPoint);
            Debug.Log("NODE: " + nodeHit.GridX + " / " + nodeHit.GridY + "  was clicked!");
        }
    }
}