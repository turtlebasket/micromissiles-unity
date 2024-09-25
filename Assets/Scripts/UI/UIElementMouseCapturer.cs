using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIElementMouseCapturer : EventTrigger
{
    private bool _hasPointerEntered = false;
    public override void OnPointerEnter(PointerEventData eventData)
    {
        InputManager.Instance.mouseActive = false;
        _hasPointerEntered = true;
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        InputManager.Instance.mouseActive = true;
        _hasPointerEntered = false;
        base.OnPointerExit(eventData);
    }

    public void OnDisable()
    {
        OnPointerExit(null);
    }

}
