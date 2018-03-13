/************************************************************************************************************
 
    IPointerEnterHandler – OnPointerEnter – Called when a pointer enters the object
    IPointerExitHandler – OnPointerExit – Called when a pointer exits the object
    IPointerDownHandler – OnPointerDown – Called when a pointer is pressed on the object
    IPointerUpHandler – OnPointerUp – Called when a pointer is released (called on the original the pressed object)
    IPointerClickHandler – OnPointerClick – Called when a pointer is pressed and released on the same object
    IBeginDragHandler – OnBeginDrag – Called on the drag object when dragging is about to begin
    IDragHandler – OnDrag – Called on the drag object when a drag is happening
    IEndDragHandler – OnEndDrag – Called on the drag object when a drag finishes
    IDropHandler – OnDrop – Called on the object where a drag finishes
    IScrollHandler – OnScroll – Called when a mouse wheel scrolls
    IUpdateSelectedHandler – OnUpdateSelected – Called on the selected object each tick
    ISelectHandler – OnSelect – Called when the object becomes the selected object
    IDeselectHandler – OnDeselect – Called on the selected object becomes deselected
    IMoveHandler – OnMove – Called when a move event occurs (left, right, up, down, ect)
    ISubmitHandler – OnSubmit – Called when the submit button is pressed
    ICancelHandler – OnCancel – Called when the cancel button is pressed
  
**********************************************************************************************************/

using System;
using UnityEngine;
using LuaInterface;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIEventListener : EventTrigger
{
    public delegate void VoidDelegate(GameObject go);
    public delegate void VoidDelegate1(GameObject go, bool value);
    public VoidDelegate onClick;
    public VoidDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;
    public VoidDelegate onSelect;
    public VoidDelegate onUpdateSelect;
    public VoidDelegate1 onPress;
    public VoidDelegate onBeginDrag;
    public VoidDelegate onDrag;
    public VoidDelegate onEndDrag;

    private static float durationThreshold = 0.8f;
    private bool isPointerDown = false;
    private bool longPressTriggered = false;
    private float timePressStarted;

    private void Update()
    {
        if (isPointerDown && !longPressTriggered)
        {
            if (Time.time - timePressStarted >= durationThreshold)
            {
                longPressTriggered = true;
                if (null != onPress) onPress(gameObject, true);
            }
        }
    }

    static public UIEventListener Get(GameObject go)
    {
        UIEventListener listener = go.GetComponent<UIEventListener>();
        if (null == listener) listener = go.AddComponent<UIEventListener>();
        return listener;
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        //if (!longPressTriggered)
        //{
        //    isPointerDown = false;
        //    if (null != onClick) onClick(gameObject);
        //}
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        timePressStarted = Time.time;
        isPointerDown = true;
        longPressTriggered = false;

        if (null != onDown) onDown(gameObject);
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (null != onEnter) onEnter(gameObject);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {

        if (null != onExit) onExit(gameObject);

        isPointerDown = false;

        if (longPressTriggered)
        {
            longPressTriggered = false;
            if (null != onPress) onPress(gameObject, false);
        }

        //this.OnPress(gameObject);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {

        if (null != onUp) onUp(gameObject);

        //this.OnPress(gameObject);

        if (isPointerDown)
        {
            isPointerDown = false;

            if (longPressTriggered)
            {
                longPressTriggered = false;
                if (null != onPress) onPress(gameObject, false);
            }
            else
            {
                if (null != onClick) onClick(gameObject);
            }
        }
    }

    private void OnPress(GameObject gameObject)
    {
        if (longPressTriggered)
        {
            if (null != onPress) onPress(gameObject, false);
        }
    }

    public override void OnSelect(BaseEventData eventData)
    {
        if (null != onSelect) onSelect(gameObject);
    }
    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if (null != onUpdateSelect) onUpdateSelect(gameObject);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (null != onBeginDrag) onBeginDrag(gameObject);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (null != onDrag) onDrag(gameObject);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (null != onEndDrag) onEndDrag(gameObject);
    }
}