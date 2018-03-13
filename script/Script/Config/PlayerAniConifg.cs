using UnityEngine;
using System.Collections;
using System;

public class PlayerAniConifg
{
    public enum directionStatus
    {
        None,
        NORTH = 1,
        NORTHEAST = 2,
        EAST = 3,
        SOUTHEAST = 4,
        SOUTH = 5,
        SOUTHWEST = 6,
        WEST = 7,
        NORTHWEST = 8
    };

    public enum actionStatus
    {
        None,
        IDEL = 1,
        WALK = 2,
        DIE = 3,
        ATTACK = 4,
        ATTACKWAIT = 5,
        SKILL = 6,
        RIDEIDEL = 7,
        RIDEWALK = 8,
        HURT = 9
    };

    public enum parts
    {
        None,
        BODY = 1,
        WEAPON = 2,
        MOUNT = 3,
        WING = 4,
    };

    public static string getPartsToString(parts part)
    {
        string partStr = "";
        switch (part)
        {
            case parts.None:
                partStr = "None";
                break;
            case parts.BODY:
                partStr = "BODY";
                break;
            case parts.WEAPON:
                partStr = "WEAPON";
                break;
            case parts.MOUNT:
                partStr = "MOUNT";
                break;
            case parts.WING:
                partStr = "WING";
                break;
            default:
                break;
        }
        return partStr;
    }


    public static string getDirectionStatusToString(directionStatus status)
    {
        string statusStr = "";
        switch (status)
        {
            case directionStatus.None:
                statusStr = "None";
                break;
            case directionStatus.NORTH:
                statusStr = "NORTH";
                break;
            case directionStatus.NORTHEAST:
                statusStr = "NORTHEAST";
                break;
            case directionStatus.EAST:
                statusStr = "EAST";
                break;
            case directionStatus.SOUTHEAST:
                statusStr = "SOUTHEAST";
                break;
            case directionStatus.SOUTH:
                statusStr = "SOUTH";
                break;
            case directionStatus.SOUTHWEST:
                statusStr = "SOUTHWEST";
                break;
            case directionStatus.WEST:
                statusStr = "WEST";
                break;
            case directionStatus.NORTHWEST:
                statusStr = "NORTHWEST";
                break;
            default:
                break;

        }
        return statusStr;
    }


    public static string getActionStatusToString(actionStatus status)
    {
        string statusStr = "";
        switch (status)
        {
            case actionStatus.None:
                statusStr = "None";
                break;
            case actionStatus.IDEL:
                statusStr = "IDEL";
                break;
            case actionStatus.WALK:
                statusStr = "WALK";
                break;
            case actionStatus.DIE:
                statusStr = "DIE";
                break;
            case actionStatus.ATTACK:
                statusStr = "ATTACK";
                break;
            case actionStatus.ATTACKWAIT:
                statusStr = "ATTACKWAIT";
                break;
            case actionStatus.SKILL:
                statusStr = "SKILL";
                break;
            case actionStatus.RIDEIDEL:
                statusStr = "RIDEIDEL";
                break;
            case actionStatus.RIDEWALK:
                statusStr = "RIDEWALK";
                break;
            case actionStatus.HURT:
                statusStr = "HURT";
                break;
            default:
                break;

        }
        return statusStr;
    }

    public static directionStatus getRealDirectionStatus(directionStatus direction)
    {
        if (direction == directionStatus.NORTHEAST || direction == directionStatus.NORTHWEST)
        {
            direction = directionStatus.NORTHEAST;
        }


        return direction;
    }


    public static string getSceneDirectionRes(directionStatus direction)
    {
        string dir = getDirectionStatusToString(direction);
        if (direction == directionStatus.NORTHWEST)
        {
            dir = getDirectionStatusToString(directionStatus.NORTHEAST);
        }
        else if (direction == directionStatus.SOUTHWEST)
        {
            dir = getDirectionStatusToString(directionStatus.SOUTHEAST);
        }
        else if (direction == directionStatus.WEST)
        {
            dir = getDirectionStatusToString(directionStatus.EAST);
        }
        return dir;
    }

    public static bool isNativeDirection(directionStatus direction)
    {
        bool isNative = false;
        if (direction == directionStatus.NORTHWEST || direction == directionStatus.SOUTHWEST || direction == directionStatus.WEST)
        {
            isNative = true;
        }
        return isNative;
    }



    //private static float SouthMaxAngle = 22.5f;
    //private static float SouthEastMaxAngle = 45 + 22.5f;
    //private static float EastMaxAngle = 90 + 22.5f;
    //private static float NorthEastMaxAngle = 135 + 22.5f;
    //private static float NorthMaxAngle = 180 + 22.5f;
    //private static float NorthWestMaxAngle = 225 + 22.5f;
    //private static float WestMaxAngle = 270 + 22.5f;
    //private static float SouthWestMaxAngle = 315 + 22.5f;

    private static float SouthMaxAngle = 315 + 22.5f;
    private static float SouthEastMaxAngle = 270 + 22.5f;
    private static float EastMaxAngle = 225 + 22.5f;
    private static float NorthEastMaxAngle = 180 + 22.5f;
    private static float NorthMaxAngle = 135 + 22.5f;
    private static float NorthWestMaxAngle = 90 + 22.5f;
    private static float WestMaxAngle = 45 + 22.5f;
    private static float SouthWestMaxAngle = 22.5f;


    public static directionStatus getDirection(float x, float y)
    {
        directionStatus status = directionStatus.None;

        float angle = Mathf.Atan2(x, y) * Mathf.Rad2Deg + 180;
        if ((angle >= 0 && angle < SouthWestMaxAngle) || angle >= SouthMaxAngle)
        {
            status = directionStatus.SOUTH;
        }
        else if (angle >= SouthWestMaxAngle && angle < WestMaxAngle)
        {
            status = directionStatus.SOUTHWEST;
        }
        else if (angle >= WestMaxAngle && angle < NorthWestMaxAngle)
        {
            status = directionStatus.WEST;
        }
        else if (angle >= NorthWestMaxAngle && angle < NorthMaxAngle)
        {
            status = directionStatus.NORTHWEST;
        }
        else if (angle >= NorthMaxAngle && angle < NorthEastMaxAngle)
        {
            status = directionStatus.NORTH;
        }
        else if (angle >= NorthEastMaxAngle && angle < EastMaxAngle)
        {
            status = directionStatus.NORTHEAST;
        }
        else if (angle >= EastMaxAngle && angle < SouthEastMaxAngle)
        {
            status = directionStatus.EAST;
        }
        else if (angle >= SouthEastMaxAngle && angle < SouthMaxAngle)
        {
            status = directionStatus.SOUTHEAST;
        }
        else
        {
            status = directionStatus.SOUTH;
        }


        //if ((angle >= 0 && angle < SouthMaxAngle) || angle >= SouthWestMaxAngle)
        //{
        //    status = directionStatus.SOUTH;
        //}
        //else if (angle >= SouthMaxAngle && angle < SouthEastMaxAngle)
        //{
        //    status = directionStatus.SOUTHEAST;
        //}
        //else if (angle >= SouthEastMaxAngle && angle < EastMaxAngle)
        //{
        //    status = directionStatus.EAST;
        //}
        //else if (angle >= EastMaxAngle && angle < NorthEastMaxAngle)
        //{
        //    status = directionStatus.NORTHEAST;
        //}
        //else if (angle >= NorthEastMaxAngle && angle < NorthMaxAngle)
        //{
        //    status = directionStatus.NORTH;
        //}
        //else if (angle >= NorthMaxAngle && angle < NorthWestMaxAngle)
        //{
        //    status = directionStatus.NORTHWEST;
        //}
        //else if (angle >= NorthWestMaxAngle && angle < WestMaxAngle)
        //{
        //    status = directionStatus.WEST;
        //}
        //else if (angle >= WestMaxAngle && angle < SouthWestMaxAngle)
        //{
        //    status = directionStatus.SOUTHWEST;
        //}
        //else
        //{
        //    status = directionStatus.SOUTH;
        //}
        return status;
    }

}
