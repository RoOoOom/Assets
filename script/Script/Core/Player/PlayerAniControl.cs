using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 玩家动画控制组件
/// </summary>
public class PlayerAniControl : MonoBehaviour 
{
    public PlayerAniConifg.directionStatus direction = PlayerAniConifg.directionStatus.SOUTH;

    public PlayerAniConifg.actionStatus action = PlayerAniConifg.actionStatus.IDEL;

    public Dictionary<string, tk2dSpriteAnimator> animators = new Dictionary<string, tk2dSpriteAnimator>();
    public Dictionary<string, tk2dSprite> colorTk2dSprite = new Dictionary<string, tk2dSprite>();

    public bool isJoystickMove = false;
    public Action doSkill = null;

    public int mountHeight = 0;
    public bool isRiding = false;

    public byte DirectionNumber()
    {
        return (byte)direction;
    }

    public byte ActionNumber()
    {
        return (byte)action;
    }

    public Transform headPart = null;

    public bool SetDirectionAndAction(int dir, int act)
    {
        bool changeAct = SetAction(act);
        bool chagneDir = SetDirection(dir);
        return changeAct || chagneDir;
    }

    public bool SetAction(int act)
    {
        if (action == PlayerAniConifg.actionStatus.DIE) return false;

        bool changed = false;
        PlayerAniConifg.actionStatus newAct = action;
        if ((PlayerAniConifg.actionStatus)act == PlayerAniConifg.actionStatus.HURT)
        {
            if (action == PlayerAniConifg.actionStatus.RIDEIDEL || action == PlayerAniConifg.actionStatus.IDEL)
            {
                newAct = (PlayerAniConifg.actionStatus)act;
            }
        }
        else
        {
            newAct = (PlayerAniConifg.actionStatus)act;
        }
        
        changed = newAct != action;
        action = newAct;
        return changed;
    }

    public bool ForceSetAction(int act)
    {
        bool changed = false;
        PlayerAniConifg.actionStatus newAct = (PlayerAniConifg.actionStatus)act;
        changed = newAct != action;
        action = newAct;
        return changed;
    }

    public bool SetDirection(int dir)
    {
        if (action == PlayerAniConifg.actionStatus.DIE) return false;

        bool changed = false;
        PlayerAniConifg.directionStatus newDir = (PlayerAniConifg.directionStatus)dir;
        changed = newDir != direction;
        direction = newDir;
        return changed;
    }

    /// <summary>
    /// 更新所有序列帧
    /// </summary>
    public void playAllAnimation()
    {
        string clipName = GetClipName();
        Dictionary<string, tk2dSpriteAnimator>.Enumerator etor = animators.GetEnumerator();
        while (etor.MoveNext())
        {
            tk2dSpriteAnimator animator = etor.Current.Value;
            if (!animator.gameObject.activeSelf)
            {
                continue;
            }

            tk2dSpriteAnimationClip clip = animator.GetClipByName(clipName);

            if (null == clip)
            {
                clipName = GetClipName(PlayerAniConifg.directionStatus.SOUTHEAST, action, isRiding);
                clip = animator.GetClipByName(clipName);
            }

            if (null == clip)
            {
                JZLog.LogWarning(etor.Current.Key + " 没有该动画数据： " + clipName);
                continue;
            }

            switch (action)
            {
                case PlayerAniConifg.actionStatus.DIE:
                case PlayerAniConifg.actionStatus.ATTACK:
                case PlayerAniConifg.actionStatus.HURT:
                    clip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                    break;
                default:
                    clip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
                    break;
            }

            clip.fps = 30f;
            animator.DefaultClipId = 0;
            animator.PlayFromFrame(clip, 0);

            CheckRotation(etor.Current.Key, direction, animator);
            CheckLayer(etor.Current.Key, animator.gameObject.transform);
        }
    }

    public void PlayAnimations(tk2dSpriteAnimationClip.WrapMode mode, float fps)
    {
        string clipName = GetClipName();
        Dictionary<string, tk2dSpriteAnimator>.Enumerator etor = animators.GetEnumerator();
        while (etor.MoveNext())
        {
            tk2dSpriteAnimator animator = etor.Current.Value;
            if (!animator.gameObject.activeInHierarchy)
            {
                continue;
            }

            tk2dSpriteAnimationClip clip = animator.GetClipByName(clipName);
            if (null == clip)
            {
                clipName = GetClipName(PlayerAniConifg.directionStatus.SOUTHEAST, action, isRiding);
                clip = animator.GetClipByName(clipName);
            }

            if (null == clip)
            {
                JZLog.LogWarning(etor.Current.Key + " 没有该动画数据： " + clipName);
                continue;
            }

            clip.wrapMode = mode;
            clip.fps = fps;
            animator.DefaultClipId = 0;
            animator.PlayFromFrame(clip, 0);

            CheckRotation(etor.Current.Key, direction, animator);
            CheckLayer(etor.Current.Key, animator.gameObject.transform);
        }
    }

    void CheckRotation(string name, PlayerAniConifg.directionStatus direction, tk2dSpriteAnimator animator)
    {
        Vector3 scale = animator.Sprite.scale;
        bool isNegative = PlayerAniConifg.isNativeDirection(direction);
        if ((scale.x < 0 && !isNegative) || (scale.x > 0 && isNegative))
        {
            scale.x = -scale.x;
            animator.Sprite.scale = scale;
        }
    }

    Vector3 tmpPos = Vector3.zero;
    // 人物默认层级
    static float defaultPartZ = 10f;
    static float layerL1 = 10.0001f;
    static float layerL2 = 10.0002f;
    static float layerL3 = 10.0003f;
    static float layerH1 = 9.9996f;
    static float layerH2 = 9.9995f;

    static float minDistance = 0.0001f;

    void CheckLayer(string name, Transform transform)
    {
        tmpPos = transform.localPosition;
        if (name == "mount")
        {
            tmpPos.z = layerL3;
            transform.localPosition = tmpPos;
            return;
        }

        tmpPos.z = defaultPartZ;
        switch(direction)
        {
            case PlayerAniConifg.directionStatus.EAST:
                if (name == "wing") tmpPos.z = layerH2;
                else if (name == "weapon") tmpPos.z = layerH1;
                break;
            case PlayerAniConifg.directionStatus.NORTH:
                if (name == "wing") tmpPos.z = layerH2;
                else if (name == "weapon")
                {
                    if (action == PlayerAniConifg.actionStatus.IDEL || action == PlayerAniConifg.actionStatus.WALK)
                    {
                        tmpPos.z = layerL1;
                    }
                    else
                    {
                        tmpPos.z = layerH1;
                    }
                }
                break;
            case PlayerAniConifg.directionStatus.NORTHEAST:
                if (name == "wing") tmpPos.z = layerH2;
                else if (name == "weapon") tmpPos.z = layerH1;
                break;
            case PlayerAniConifg.directionStatus.NORTHWEST:
                if (name == "wing") tmpPos.z = layerH2;
                else if (name == "weapon") tmpPos.z = layerH1;
                break;
            case PlayerAniConifg.directionStatus.SOUTH:
                if (name == "wing") tmpPos.z = layerL2;
                else if (name == "weapon") tmpPos.z = layerH1;
                break;
            case PlayerAniConifg.directionStatus.SOUTHEAST:
                if (name == "wing") tmpPos.z = layerL1;
                else if (name == "weapon") tmpPos.z = layerH1;
                break;
            case PlayerAniConifg.directionStatus.SOUTHWEST:
                if (name == "wing") tmpPos.z = layerL1;
                else if (name == "weapon") tmpPos.z = layerH1;
                break;
            case PlayerAniConifg.directionStatus.WEST:
                if (name == "wing") tmpPos.z = layerH2;
                else if (name == "weapon") tmpPos.z = layerH1;
                break;
            case PlayerAniConifg.directionStatus.None:
            default:
                break;
        }
        transform.localPosition = tmpPos;
    }

    public void StopAndReset()
    {
        Dictionary<string, tk2dSpriteAnimator>.Enumerator etor = animators.GetEnumerator();
        while(etor.MoveNext())
        {
            tk2dSpriteAnimator animator = etor.Current.Value;
            tk2dSpriteAnimationClip clip = animator.CurrentClip;
            if (null != clip && clip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Once)
            {
                animator.StopAndResetFrame();
            }
        }
    }

    public void Resume()
    {
        Dictionary<string, tk2dSpriteAnimator>.Enumerator etor = animators.GetEnumerator();
        while (etor.MoveNext())
        {
            tk2dSpriteAnimator animator = etor.Current.Value;
            tk2dSpriteAnimationClip clip = animator.CurrentClip;
            if (null != clip && clip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Once)
            {
                animator.Play();
            }
        }
    }

    public void StopFrame()
    {
        Dictionary<string, tk2dSpriteAnimator>.Enumerator etor = animators.GetEnumerator();
        while (etor.MoveNext())
        {
            etor.Current.Value.enabled = false;
        }
    }

    public void ResumeFrame()
    {
        Dictionary<string, tk2dSpriteAnimator>.Enumerator etor = animators.GetEnumerator();
        while (etor.MoveNext())
        {
            etor.Current.Value.enabled = true;
        }
    }

    public tk2dSpriteAnimator GetAnimator(string name)
    {
        if (animators.ContainsKey(name)) return animators[name];

        return null;
    }

    public void AddAnimator(string name, tk2dSpriteAnimator animator)
    {
        if (string.IsNullOrEmpty(name) || animator == null)
        {
            return;
        }

        if (animators.ContainsKey(name))
        {
            animators[name] = animator;
        } else
        {
            animators.Add(name, animator);
        }

        playAllAnimation();
    }

    public void RemoveAnimator(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        if (animators.ContainsKey(name))
        {
            animators.Remove(name);
        }
    }

    public string GetClipName()
    {
        return GetClipName(direction, action, isRiding);
    }

    string GetClipName(PlayerAniConifg.directionStatus direction, PlayerAniConifg.actionStatus act, bool isRiding)
    {
        string dirName = PlayerAniConifg.getSceneDirectionRes(direction);
        if (act == PlayerAniConifg.actionStatus.IDEL && isRiding)
        {
            act = PlayerAniConifg.actionStatus.RIDEIDEL;
        }
        else if (act == PlayerAniConifg.actionStatus.WALK && isRiding)
        {
            act = PlayerAniConifg.actionStatus.RIDEWALK;
        }

        string actName = PlayerAniConifg.getActionStatusToString(act);
        string clipName = dirName + "_" + actName;
        return clipName;
    }


    public void AddSprite(string name, tk2dSprite sprite)
    {
        if (string.IsNullOrEmpty(name) || null == sprite)
        {
            return;
        }

        if (colorTk2dSprite.ContainsKey(name))
	    {
            colorTk2dSprite[name] = sprite;
        }
        else
        {
            colorTk2dSprite.Add(name, sprite);
        }
    }

    public void RemoveSprite(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        if (colorTk2dSprite.ContainsKey(name))
        {
            colorTk2dSprite.Remove(name);
        } 
    }

    public void SetColorRGBA(float r, float g, float b, float a)
    {
        Color color = new Color(r, g, b, a);
        Dictionary<string, tk2dSprite>.Enumerator etor = colorTk2dSprite.GetEnumerator();
        while (etor.MoveNext())
        {
            if (null != etor.Current.Value)
            {
                etor.Current.Value.color = color;
            }
        }
    }

    public void SetColor(Color color)
    {
        Dictionary<string, tk2dSprite>.Enumerator etor = colorTk2dSprite.GetEnumerator();
        while(etor.MoveNext())
        {
            if (null != etor.Current.Value)
            {
                etor.Current.Value.color = color;
            }
        }
    }

    public void SetAlpha(float alpha)
    {
        Dictionary<string, tk2dSprite>.Enumerator etor = colorTk2dSprite.GetEnumerator();
        while (etor.MoveNext())
        {
            if (null != etor.Current.Value)
            {
                Color color = etor.Current.Value.color;
                color.a = alpha;
                etor.Current.Value.color = color;
            }
        }
    }

    public void SetCover(bool cover)
    {
        float alpha = 1f;
        if (cover)
        {
            alpha = 0.6f;
        }
        SetAlpha(alpha);
    }

    Vector3 lastPos = Vector3.zero;
    int count = 0;
    void Update()
    {
        count++;
        if (count >= 3 && lastPos != transform.position)
        {
            count = 0;
            SetCover(MapSceneDataManage.Instance().IsCover(transform.position));
            lastPos = transform.position;
            //Pathfinding.NNInfo nodeInfo = AstarPath.active.GetNearest(lastPos);
            //if (null != nodeInfo.node)
            //{
            //    SetCover(((Pathfinding.GridNode)nodeInfo).IsCover);
            //}
        }
    }

    public void Clear()
    {
        animators.Clear();
        colorTk2dSprite.Clear();
    }
}
