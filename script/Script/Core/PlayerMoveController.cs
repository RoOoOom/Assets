using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

namespace JZWL
{

    public class PlayerMoveController : MonoBehaviour
    {
        private float m_distance = 0f;
        private Vector3 m_speed = Vector3.zero;
        private Vector3 m_lastToNode = Vector3.zero;
        private Vector3 m_toNode = Vector3.zero;

        private PlayerAniControl m_aniCtrl = null;
        public PlayerAniControl aniCtrl
        {
            get
            {
                if (null == m_aniCtrl) m_aniCtrl = GetComponent<PlayerAniControl>();
                return m_aniCtrl;
            }
        }

        public float _speed;

        public LuaFunction doSkill;
        public bool canMove = true;
        public bool isMoving = false;

        private bool _inited = false;

        public void Init(float speed)
        {
            _speed = speed;
            _inited = true;
        }

        void OnDisable()
        {
            _inited = false;
        }

        void Update()
        {
            if (!_inited || aniCtrl.action == PlayerAniConifg.actionStatus.DIE || !canMove)
            {
                return;
            }

            if (m_distance > 0)
            {
                Vector3 moveSegment = m_speed * Time.deltaTime;
                transform.position += moveSegment;
                m_distance -= moveSegment.magnitude;

                if (m_distance <= 0)
                {
                    isMoving = false;
                    //aniCtrl.SetAction((int)PlayerAniConifg.actionStatus.IDEL);
                    //aniCtrl.playAllAnimation();

                    if (null != doSkill)
                    {

                        aniCtrl.SetAction((int)PlayerAniConifg.actionStatus.IDEL);
                        aniCtrl.playAllAnimation();
                        doSkill.Call();
                        doSkill = null;
                    }
                    else if (!M_Check())
                    {
                        aniCtrl.SetAction((int)PlayerAniConifg.actionStatus.IDEL);
                        aniCtrl.playAllAnimation();
                    }
                }
            }
            else
            {
                M_Check();
            }
        }

        private bool M_Check()
        {
            if (isMoving)
            {
                return false;
            }

            if (m_lastToNode != m_toNode)
            {
                Vector3 v = transform.position;
                v.z = 0;

                Vector3 dirVector = m_toNode - v;
                m_distance = dirVector.magnitude;
                m_lastToNode = m_toNode;
                if (!GameUtils.EqualZero(m_distance) && m_distance > 0)
                {
                    isMoving = true;
                    PlayerAniConifg.directionStatus nowDirection = PlayerAniConifg.getDirection(dirVector.x, dirVector.y);
                    bool isChange = false;
                    isChange |= aniCtrl.SetDirection((int)nowDirection);
                    isChange |= aniCtrl.SetAction((int)PlayerAniConifg.actionStatus.WALK);
                    if (isChange) aniCtrl.playAllAnimation();

                    m_speed = _speed * dirVector.normalized;
                    switch (nowDirection)
                    {
                        case PlayerAniConifg.directionStatus.EAST:
                        case PlayerAniConifg.directionStatus.WEST:
                            m_speed *= AstarPath.active.astarData.gridGraph.aspectRatio;
                            break;
                        case PlayerAniConifg.directionStatus.NORTHEAST:
                        case PlayerAniConifg.directionStatus.NORTHWEST:
                        case PlayerAniConifg.directionStatus.SOUTHEAST:
                        case PlayerAniConifg.directionStatus.SOUTHWEST:
                            m_speed *= Mathf.Sqrt(AstarPath.active.astarData.gridGraph.aspectRatio * AstarPath.active.astarData.gridGraph.aspectRatio + 1f);
                            break;
                        default:
                            break;
                    }
                    return true;
                }
                else
                {
                    m_distance = 0;
                }
            }
            return false;
        }

        public void MoveTo(int x, int y)
        {
            m_toNode = (Vector3)AstarPath.active.astarData.gridGraph.GraphPointToWorld(x, y, 0);
        }

        public void CancelMove()
        {
            m_distance = 0;
            m_lastToNode = m_toNode;
            isMoving = false;
            if (aniCtrl.action == PlayerAniConifg.actionStatus.WALK || aniCtrl.action == PlayerAniConifg.actionStatus.RIDEWALK)
            {
                aniCtrl.ForceSetAction((int)PlayerAniConifg.actionStatus.IDEL);
                aniCtrl.playAllAnimation();
            }
        }

        void OnDestroy()
        {
            if (null != doSkill)
            {
                doSkill.Dispose();
                doSkill = null;
            }
        }
    }
}
