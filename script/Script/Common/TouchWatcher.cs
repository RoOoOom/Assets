using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TouchWatcher : MonoBehaviour
{
    private enum JoystickTouch
    {
        None = -1,
        South = 0,
        East = 1,
        Nouth = 2,
        West = 3,
        Southeast = 4,
        Noutheast = 5,
        Nouthwest = 6,
        Southwest = 7,
    }

    public int rayDistance = 100;
    private int _UILayerMask { get { return LayerMask.NameToLayer("UI"); } }
    private int _PlayerLayerMask { get { return LayerMask.NameToLayer("Player"); } }
    private int _MapLayerMask { get { return LayerMask.NameToLayer("Ground"); } }
    private int _ItemLayerMask { get { return LayerMask.NameToLayer("Item"); } }
    private int _NpcLayerMask { get { return LayerMask.NameToLayer("NPC"); } }


    private bool _joystickStarted = false;
    private bool _joystickEnded = false;
    private Vector2 _moveDir = Vector2.zero;
    private float _distance;
    private Vector3 _speed;
    private Vector3 _dirVector;
    private Pathfinding.GridNode _toNode;


    private GraphicRaycaster m_canvasJoystick = null;
    private GraphicRaycaster _canvasUI = null;
    private PointerEventData _eventData;
    private EventSystem _eventSystem;
    private ETCJoystick _joystick;

    private static float tan23 = 0.414f; //tan22.5
    private static float tan67 = 2.414f; //tan67.5


    private float _fDelayEnableTouchTime = 0f;

    private int TOUCH_UI = 1;
    private int TOUCH_PLAYER = 2;
    private int TOUCH_GROUND = 3;
    private int TOUCH_ITEM = 4;
    private int TOUCH_E_TOUCH = 5;

    private static Dictionary<JoystickTouch, List<JoystickTouch>> m_refindList = new Dictionary<JoystickTouch, List<JoystickTouch>>();

    static List<JoystickTouch> M_BuildRefindDir(params JoystickTouch[] dir)
    {
        List<JoystickTouch> ret = new List<JoystickTouch>();
        for (int i = 0; i < dir.Length; i++)
        {
            ret.Add(dir[i]);
        }
        return ret;
    }

    static TouchWatcher()
    {
        m_refindList.Add(JoystickTouch.South, M_BuildRefindDir(JoystickTouch.South, JoystickTouch.Southeast, JoystickTouch.Southwest));
        m_refindList.Add(JoystickTouch.Nouth, M_BuildRefindDir(JoystickTouch.Nouth, JoystickTouch.Noutheast, JoystickTouch.Nouthwest));
        m_refindList.Add(JoystickTouch.East, M_BuildRefindDir(JoystickTouch.East, JoystickTouch.Southeast, JoystickTouch.Noutheast));
        m_refindList.Add(JoystickTouch.West, M_BuildRefindDir(JoystickTouch.West, JoystickTouch.Southwest, JoystickTouch.Nouthwest));
        m_refindList.Add(JoystickTouch.Southeast, M_BuildRefindDir(JoystickTouch.Southeast, JoystickTouch.East, JoystickTouch.South));
        m_refindList.Add(JoystickTouch.Southwest, M_BuildRefindDir(JoystickTouch.Southwest, JoystickTouch.West, JoystickTouch.South));
        m_refindList.Add(JoystickTouch.Noutheast, M_BuildRefindDir(JoystickTouch.Noutheast, JoystickTouch.East, JoystickTouch.Nouth));
        m_refindList.Add(JoystickTouch.Nouthwest, M_BuildRefindDir(JoystickTouch.Nouthwest, JoystickTouch.West, JoystickTouch.Nouth));
    }

    void Awake()
    {
        Input.multiTouchEnabled = true;
        m_canvasJoystick = GameObject.Find("CanvasEasyTouch").GetComponent<UnityEngine.UI.GraphicRaycaster>();
        _canvasUI = GameObject.Find("Canvas").GetComponent<UnityEngine.UI.GraphicRaycaster>();
        _eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        _eventData = new PointerEventData(_eventSystem);
        _joystick = GameObject.Find("/CanvasEasyTouch/Joystick").GetComponent<ETCJoystick>();
    }

    private JoystickTouch _vectorToDir(Vector3 vector)
    {
        if (vector.x == 0)
        {
            if (vector.y > 0) return JoystickTouch.Nouth;
            else if (vector.y < 0) return JoystickTouch.South;
        }

        float tanV = vector.y / vector.x;
        if (vector.x > 0)
        {
            if (tanV < -tan67) return JoystickTouch.South;
            else if (tanV < -tan23) return JoystickTouch.Southeast;
            else if (tanV < tan23) return JoystickTouch.East;
            else if (tanV < tan67) return JoystickTouch.Noutheast;
            else return JoystickTouch.Nouth;
        }
        else
        {
            if (tanV < -tan67) return JoystickTouch.Nouth;
            else if (tanV < -tan23) return JoystickTouch.Nouthwest;
            else if (tanV < tan23) return JoystickTouch.West;
            else if (tanV < tan67) return JoystickTouch.Southwest;
            else return JoystickTouch.South;
        }
    }

    private void JoystickTouchToDir(JoystickTouch touchDir, ref Vector3 dir)
    {
        switch (touchDir)
        {
            case JoystickTouch.West: dir.y = 0f; dir.x = -AstarPath.active.astarData.gridGraph.aspectRatio; break;
            case JoystickTouch.East: dir.y = 0f; dir.x = AstarPath.active.astarData.gridGraph.aspectRatio; break;
            case JoystickTouch.Nouth: dir.y = 1f; dir.x = 0f; break;
            case JoystickTouch.South: dir.y = -1f; dir.x = 0f; break;
            case JoystickTouch.Noutheast: dir.y = 1f; dir.x = AstarPath.active.astarData.gridGraph.aspectRatio; break;
            case JoystickTouch.Nouthwest: dir.y = 1f; dir.x = -AstarPath.active.astarData.gridGraph.aspectRatio; break;
            case JoystickTouch.Southwest: dir.y = -1f; dir.x = -AstarPath.active.astarData.gridGraph.aspectRatio; break;
            case JoystickTouch.Southeast: dir.y = -1f; dir.x = AstarPath.active.astarData.gridGraph.aspectRatio; break;
        }
    }

    private void _Stop()
    {
        if (null != AILerp.me)
        {
            if (AILerp.me.playerAniControl.action != PlayerAniConifg.actionStatus.ATTACK)
            {
                AILerp.me.playerAniControl.SetAction((int)PlayerAniConifg.actionStatus.IDEL);
                AILerp.me.playerAniControl.playAllAnimation();
            }

            if (null != _toNode)
            {
                //Vector3 pos = (Vector3)_toNode.position;
                //pos.z = AILerp.me.transform.position.z;
                //AILerp.me.transform.position = pos;
                AILerp.me.ArriveCallBack(_toNode.Cell);
                _toNode = null;
            }

            AILerp.me.playerAniControl.isJoystickMove = false;
        }
    }

    void Update()
    {
        if (MapSceneDataManage.Instance().isChangingScene)
        {
            _toNode = null;
            _distance = 0;
            return;
        }

        if (_distance > 0)
        {
            Vector3 moveSegment = _speed * Time.deltaTime;
            AILerp.me.transform.position += moveSegment;
            _distance -= moveSegment.magnitude;
            if (_distance <= 0)
            {
                if (null != AILerp.me.playerAniControl.doSkill)
                {
                    AILerp.me.playerAniControl.doSkill();
                    AILerp.me.playerAniControl.doSkill = null;
                }

                if (!_joystickStarted)
                {
                    _Stop();
                }
            }

        }
        else if (_joystickEnded)
        {
            _Stop();
            _joystickEnded = false;
        }

        if (AILerp.me && AILerp.me.canMove && (CheckKey() || _joystickStarted))
        {
         //   if (_moveDir.magnitude < 0.2f || _distance > 0) return;
            if (_distance <= 0)
            {

                dispatch(TOUCH_E_TOUCH, null, Vector3.zero);
                JoystickTouch touchDir = _vectorToDir(_moveDir);
                Vector3 vec = AILerp.me.transform.position;
                vec.z = 0f;
                Pathfinding.NNInfo nodeInfo = AstarPath.active.GetNearest(vec);

                _toNode = null;
                for (int i = 0; i < m_refindList[touchDir].Count; i++)
                {
                    Pathfinding.GridNode node = AstarPath.active.astarData.gridGraph.GetNodeConnection((Pathfinding.GridNode)nodeInfo, (int)(m_refindList[touchDir][i]));
                    if (node != null && node.Walkable)
                    {
                        _toNode = node;
                        touchDir = m_refindList[touchDir][i];
                        break;
                    }
                }

                JoystickTouchToDir(touchDir, ref _dirVector);
                float angle = Mathf.Atan2(_dirVector.x, -_dirVector.y) * Mathf.Rad2Deg + 180;
                PlayerAniConifg.directionStatus nowDirection = PlayerAniConifg.getDirection(_dirVector.x, _dirVector.y);
                if (null != AILerp.me.playerAniControl)
                {
                    bool isChanged = false;
                    isChanged |= AILerp.me.playerAniControl.SetDirection((int)nowDirection);
                    if (isChanged)
                    {
                        AILerp.me.directionStatus = nowDirection;
                        if (AILerp.me.moveDirectionCallBack != null)
                        {
                            AILerp.me.moveDirectionCallBack.Call(AILerp.me.gameObject, angle);
                        }
                    }

                    isChanged |= AILerp.me.playerAniControl.SetAction((int)PlayerAniConifg.actionStatus.WALK);
                    if (isChanged) AILerp.me.playerAniControl.playAllAnimation();
                }

                //_toNode = AstarPath.active.astarData.gridGraph.GetNodeConnection((Pathfinding.GridNode)nodeInfo, (int)touchDir);
                if (null != _toNode && _toNode.Walkable)
                {
                    AILerp.me.StepCallBak(((Pathfinding.GridNode)_toNode).Cell);

                    Vector3 toPos = (Vector3)_toNode.position;
                    toPos.z = 0f;

                    _distance = Vector3.Distance(toPos, vec);
                    if (touchDir == JoystickTouch.South || touchDir == JoystickTouch.Nouth)
                    {
                        _speed = AILerp.me.speed * _dirVector.normalized;
                    }
                    else if (touchDir == JoystickTouch.East || touchDir == JoystickTouch.West)
                    {
                        _speed = AILerp.me.speed * _dirVector.normalized * AstarPath.active.astarData.gridGraph.aspectRatio;
                    }
                    else
                    {
                        _speed = AILerp.me.speed * _dirVector.normalized * Mathf.Sqrt(AstarPath.active.astarData.gridGraph.aspectRatio * AstarPath.active.astarData.gridGraph.aspectRatio + 1f);
                    }

                    _speed.z = 0f;
                }
                else
                {
                    _toNode = null;
                }
            }
        }

        multiTouchChecker();

        //if (CheckGuiRaycastObjects(_canvasJoystick)) return;

        //if (_fDelayEnableTouchTime < 0f)
        //{
        //    // 做个优化 最多锁屏几秒钟 防止各种情况下锁死
        //    _fDelayEnableTouchTime = Mathf.Min(0f, _fDelayEnableTouchTime + Time.deltaTime);
        //    if (_fDelayEnableTouchTime == 0f)
        //    {
        //        //UIUtil.SetUICameraTouchEnable(true);
        //    }
        //    return;
        //}
        //if (_fDelayEnableTouchTime > 0f)
        //{
        //    _fDelayEnableTouchTime = Mathf.Max(0f, _fDelayEnableTouchTime - Time.deltaTime);
        //    // Debug.LogWarning("Wait for lock ......");
        //    return;
        //}
        
        // 进行判定
        //{
        //    Vector3 touchPos = Vector3.zero;
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        touchPos = Input.mousePosition;
        //        touchChecker(touchPos);
        //    }
        //    //else if (Input.multiTouchEnabled)
        //    //{
        //    //    multiTouchChecker();
        //    //}
        //}
    }

    bool CheckGuiRaycastObjects(GraphicRaycaster canvas)
    {
        if (Input.GetMouseButtonDown(0) && null != canvas)
        {
            _eventData.pressPosition = Input.mousePosition;
            _eventData.position = Input.mousePosition;
            List<RaycastResult> list = new List<RaycastResult>();
            canvas.Raycast(_eventData, list);
            return list.Count > 0;
        }
        return false;
    }

    bool IsPointerOverUIObject()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (null == EventSystem.current) return true;

            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
        return false;
    }

    /*********************************
     * 函数说明: 是否启用UICamera
     * 返 回 值: void
     * 参数说明: args
     * 注意事项: 无
     *********************************/
    void OnTouchEnable(params object[] args)
    {
        if (args == null)
        {
            return;
        }
        if ((bool)args[0] == false)
        {
            _fDelayEnableTouchTime = -3f;
        }
        else
        {
            _fDelayEnableTouchTime = 1.0f;
        }
    }

    private List<RaycastResult> _list = new List<RaycastResult>();

    private bool M_TouchJoystick(Vector3 vec)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(_joystick.userArea, vec, m_canvasJoystick.eventCamera);

        //if (null != m_canvasJoystick)
        //{
        //    _list.Clear();
        //    _eventData.pressPosition = vec;
        //    _eventData.position = vec;
        //    m_canvasJoystick.Raycast(_eventData, _list);
        //    return _list.Count > 0;
        //}

        //return false;
    }

    private bool M_TouchUI(Vector3 vec)
    {
        _list.Clear();
        //PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        //eventDataCurrentPosition.position = new Vector2(vec.x, vec.y);
        //EventSystem.current.RaycastAll(eventDataCurrentPosition, _list);
        //return _list.Count > 0;
        _eventData.position = new Vector2(vec.x, vec.y);
        _eventSystem.RaycastAll(_eventData, _list);
        return _list.Count > 0;
    }

    private GameObject M_FindTouchOBJ(Vector3 touchPos, int layer)
    {
        Ray ray;
        RaycastHit hit;
        ray = Camera.main.ScreenPointToRay(touchPos);
        Physics.Raycast(ray, out hit, rayDistance, 1 << layer);
        if (hit.collider != null)
        {
            return hit.collider.gameObject;
        }

        return null;
    }

    private List<Vector3> m_listVec = new List<Vector3>();
    bool touchJoyStick = false;
    GameObject findPlayer = null;
    Vector3 playerPos = Vector3.zero;
    GameObject findGround = null;
    Vector3 groundPos = Vector3.zero;

    private void multiTouchChecker()
    {
        if (null == EventSystem.current) return;

        m_listVec.Clear();

        if (UnityEngine.Application.platform != RuntimePlatform.Android && UnityEngine.Application.platform != RuntimePlatform.IPhonePlayer)
        {
            if (!Input.GetMouseButtonDown(0)) return;

            m_listVec.Add(Input.mousePosition);
        }
        else
        {
            // 仅允许双点触控
            if (Input.touches == null || Input.touches.Length <= 0 || Input.touches.Length > 2) return;

            for (int i = 0; i < Input.touches.Length; i++)
            {
                if (Input.touches[i].phase == TouchPhase.Began)
                {
                    m_listVec.Add(Input.GetTouch(i).position);
                }
            }
        }

        touchJoyStick = false;
        findPlayer = null;
        findGround = null;

        for (int i = 0; i < m_listVec.Count; i++ )
        {
            Vector3 touchPos = m_listVec[i];

            if (M_TouchUI(touchPos))
            {
                dispatch(TOUCH_UI, null, Vector3.zero);
                return;
            }

            if (!touchJoyStick && M_TouchJoystick(touchPos))
            {
                touchJoyStick = true;
                return;
            }

            if (!MapSceneDataManage.Instance().onlyCheckMap && null == findPlayer)
            {
                findPlayer = M_FindTouchOBJ(touchPos, _NpcLayerMask);
                playerPos = touchPos;
            }

            if (!MapSceneDataManage.Instance().onlyCheckMap && null == findPlayer)
            {
                findPlayer = M_FindTouchOBJ(touchPos, _PlayerLayerMask);
                playerPos = touchPos;
            }

            if (null == findPlayer && null == findGround)
            {
                findGround = M_FindTouchOBJ(touchPos, _MapLayerMask);
                groundPos = touchPos;
            }
        }

        if (null != findPlayer)
        {
            dispatch(TOUCH_PLAYER, findPlayer, playerPos);
        }
        else if (null != findGround)
        {
            dispatch(TOUCH_GROUND, findGround, groundPos); 
        }
    }

    private void touchChecker(Vector3 touchPos)
    {
        if (!touchPos.Equals(Vector3.zero))
        {
            if (MapSceneDataManage.Instance().onlyCheckMap)
            {
                FindTouchObj(touchPos, _MapLayerMask); 
            }
            else
            {
                bool isFinded = FindTouchObj(touchPos, _PlayerLayerMask);
                if (!isFinded) isFinded = FindTouchObj(touchPos, _ItemLayerMask);
                if (!isFinded) isFinded = FindTouchObj(touchPos, _MapLayerMask); 
            }
        }
    }

    private bool FindTouchObj(Vector3 touchPos,int layer)
    {
        Ray ray;
        RaycastHit hit;
        int eventType = 0;
        ray = Camera.main.ScreenPointToRay(touchPos);
        Physics.Raycast(ray, out hit, rayDistance, 1<<layer);
        if (hit.collider != null)
        {
            if (layer == _PlayerLayerMask) eventType = TOUCH_PLAYER;
            else if (layer == _ItemLayerMask) eventType = TOUCH_ITEM;
            else if (layer == _MapLayerMask) eventType = TOUCH_GROUND;
        }
        if (eventType != 0)
        {
            //JZLog.LogError("---------eventType----------->" + eventType + " => " + (hit.collider.gameObject.transform.parent != null ? hit.collider.gameObject.transform.parent.name : "")); 
            
            dispatch(eventType, hit.collider.gameObject, touchPos); 
        }
        return eventType == 0 ? false : true;
    }

    private void dispatch(int type, GameObject go, Vector3 position)
    {
        LuaManager.instance.CallFunction("TouchListener.touch", type, go, position.x, position.y, position.z);
    }

    public void JoystickStart()
    {
        if(AILerp.me.playerAniControl != null && AILerp.me.playerAniControl.isRiding)
        { 
            AudioPlay2D.Play("audio", AudioPlay2D.AudioType.Audio, "mic_horse_walk", (float)LocalData.GetInt("SoundValue") / 100, true);
        } else
        {
            AudioPlay2D.Play("audio", AudioPlay2D.AudioType.Audio, "mic_walk", (float)LocalData.GetInt("SoundValue") / 100, true);
        }
       
        _joystickStarted = true;
        _joystickEnded = false;
        AILerp.me.playerAniControl.isJoystickMove = true;
        if (null != AILerp.me) AILerp.me.ReleasePath();
    }

    public void JoystickEnd()
    {
        if (AILerp.me.playerAniControl != null && AILerp.me.playerAniControl.isRiding)
        {
            AudioPlay2D.Stop("audio", "mic_horse_walk");
        }
        else
        {
            AudioPlay2D.Stop("audio", "mic_walk");
        }
       
        _joystickStarted = false;
        _joystickEnded = true;
    }

    public void Move(Vector2 dir)
    {
        _moveDir = dir;
       // Debug.LogError(_moveDir.magnitude);
    }

    bool lastCheck = false;
    private bool CheckKey()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) return false;

        bool check = false;
        float deltaX = 0f, deltaY = 0f;
        if (Input.GetKey(KeyCode.W))
        {
            check = true;
            deltaY += 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            check = true;
            deltaX += 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            check = true;
            deltaX -= 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            check = true;
            deltaY -= 1;
        }

        if (check)
        {
            lastCheck = check;
            JoystickStart();
            _moveDir = new Vector2(deltaX, deltaY);
        }
        else if (lastCheck)
        {
            lastCheck = false;
            JoystickEnd();
        }

        return check;
#else
        return false;
#endif
       
    }
}


public class TouchedEx
{
    public GameObject downObject;
    public GameObject upObject;
    public Vector3 posDown;
    public Vector3 posUp;

}
public class TouchedObject
{
    public GameObject touchTarget;
    public Vector3 touchPosition;
}
