using UnityEngine;
using System.Collections;

public class JsonMessage {
    public int code;
    public string data;
}


public class LoginUser
{ 
    public string AccountName;
    public string Password;
    public string LoginKey;
    public string Platform;
    public string AgentId;
    public string Channel; 
}

public class PayParam
{
    public int index;
    public string productKey;
    public int money;
    public string productname;
    public long roleId;
    public string roleName;
    public int serverNo;
    public string orderId;
    public int scale;
    public int gameMoney;
}

public class JzwlNetworkInfo
{
    public const int TYPE_NONE = 0;
    public const int TYPE_WIFI = 1;
    public const int TYPE_MOBILE = 2; 

    public int type; //类型，wifi or 移动网络 0未链接, 1为wifi，2为移动网络,
    public int singnal; //信号强度
}

public class JzwlElectricity
{
    public int power; // 电量百分比
}

public class VoiceInfo
{
    public const int STATUS_FAIL = 0;
    public const int STATUS_SUCCESS = 1;
    public const int STATUS_ERROR = 2;

    public int status; 
    public string text; 
    public string voiceUrl;
}


public class RoleInfo
{
    public long roleID;
    public string roleName;
    public int roleLevel;
    public int serverNo;
    public string serverName;
    public int type;
    public int vip;
    public string guildName;
    public int createTime; //unix 时间戳
    public int money;
}
