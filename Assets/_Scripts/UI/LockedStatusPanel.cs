using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockedStatusPanel : MonoBehaviour
{
    [SerializeField] private Image frame;
    [SerializeField] private Image icon;

    public void SetStatus(Sprite frame, Sprite icon)
    {
        this.frame.sprite = frame;
        this.icon.sprite = icon;
        if (frame == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}
