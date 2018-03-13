using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Waiting : MonoBehaviour 
{
    public float speed;
    public Transform imgRing;
    public bool enableClick;
    EventSystem _eventSystem;
    ETCJoystick _joystick;
    
    void Awake()
    {
        _eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        GameObject canvasJoystick = GameObject.Find("/CanvasEasyTouch/Joystick");
        if (canvasJoystick)
        {
            _joystick = canvasJoystick.GetComponent<ETCJoystick>();
        }
    }

    private void _SetClickStatus(bool active)
    {
        if (null != _eventSystem)
        {
            _eventSystem.enabled = active;
        }

        if (null != _joystick)
        {
            _joystick.enabled = active;
        }
    }

    void OnEnable()
    {
        if (!enableClick)
        {
            _SetClickStatus(false);
        }
    }

    void OnDisable()
    {
        if (!enableClick)
        {
            _SetClickStatus(true);
        }
    }

	// Update is called once per frame
	void Update () 
    {
        imgRing.Rotate(Vector3.forward, -Time.fixedDeltaTime * speed);
	}
}
