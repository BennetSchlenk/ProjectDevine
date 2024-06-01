using System;
using UnityEngine;

public class UIFollowGameObject : MonoBehaviour
{
    public GameObject target;  // The gameobject to follow
    public RectTransform uiElement;  // The UI element that will follow the target

    [SerializeField] private Vector3 offset;

   

    void Update()
    {
        if (target == null || uiElement == null)
        {
            return;
        }

        // Convert the target's world position to screen space
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);

        // Set the position of the UI element to the screen position
        uiElement.position = screenPos + offset;
    }
}
