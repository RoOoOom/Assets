using UnityEngine;
using System.Collections;
using System.Reflection; 
using System.Collections.Generic;

public class SDK : MonoBehaviour {

	#if UNITY_ANDROID
    public const int LOGIN_SUCCESS_CODE = 1;
    public const int CHANGE_ACCOUNT_CODE = 2;
    public const int LOGINOUT_CODE = 3;
    public const int VOICE_CODE = 4; 
    public const int NETWORK_SINGLE_CODE = 6;
    public const int ELECTRICITY_CODE = 7;
    public const int RESUME_CODE = 8;


    public const int SUBMIT_ROLE_CREATE = 1;
    public const int SUBMIT_ROLE_LOGIN = 2;
    public const int SUBMIT_ROLE_LEVELUP = 3;
    public const int SUBMIT_ROLE_EXIT = 4;

    private static SDKUtil sdkUtil = new SDKUtil();
    private static Dictionary<string, object> sdkConfigDic = new Dictionary<string, object>();

    private static SDK sdkInstance;

    private int power;

    public static SDK GetInstance()
    {
        if (sdkInstance == null)
        {
            if (GameWorld.instance == null) return null;
            GameObject go = GameWorld.instance.gameObject;
            sdkInstance = GameUtils.GetScript<SDK>(go);
        }
        return sdkInstance;
    }


    public void Init()
    {
        this.LoadSdkConifg();
        string sdkClass = GetSdkJavaClass();
        if (sdkClass != null && !sdkClass.Trim().Equals(""))
        {
            sdkUtil.ClassName = sdkClass;
            SendToSDK("SetClassName", sdkClass);
        } 
        SendToSDK("SDKInit", NewParam());
        this.SetCallBack();
        //this.PlayVoice("/20161115/10/10010010_1479176047337.amr", 1);
    }

    public void SetCallBack()
    {
        var jsonParams = NewParam();
        jsonParams.Add("CallbackObject", "GameWorld");
        jsonParams.Add("CallbackMethod", "Message");
        SendToSDK("SetCallback", jsonParams);
    }

    public void InitJpush()
    { 
        SendToSDK("InitJpush", "GameWorld");
    }

    public void SetJpushInfo(long playerId, string jpushTag)
    {
        SendToSDK("SetJpushInfo", "GameWorld", playerId.ToString(), jpushTag);
    }


    public void OpenLoginView()
    {
        var jsonParams = NewParam(); 
        SendToSDK("OpenLoginWindow", jsonParams);
    }

    public void OpenPay(PayParam param)
    {
        SendToSDK("OpenPay", JsonFx.Json.JsonWriter.Serialize(param));
    }

    public void OnExit(RoleInfo roleInfo)
    {
        if (roleInfo.roleID != 0)
        {
            SubmitRoleInfo(roleInfo);
        } 
        SendToSDK("Exit");
    }

    public void SubmitRoleInfo(RoleInfo roleInfo)
    {
        SendToSDK("SubmitRoleInfo", JsonFx.Json.JsonWriter.Serialize(roleInfo));
    }

    public void StartVoice()
    { 
        SendToSDK("StartVoice");
    }

    public void StopVoice(long playerId)
    { 
        SendToSDK("StopVoice", playerId.ToString()); 
    }

    public void PlayVoice(string filePath, float soundValue)
    { 
        SendToSDK("PlayVoice", filePath, soundValue.ToString());
    }

    public void StopPlayVoice()
    { 
        SendToSDK("StopPlayVoice");
    }

    public string GetDeviceId()
    {
        string device = sdkUtil.CallAPI<string>("GetDeviceId");
        if (device == null)
        {
            device = "";
        }
        return device;
    }

    public string GetMacAddress()
    {
        string mac = sdkUtil.CallAPI<string>("GetMacAddress");
        if (mac == null)
        {
            mac = "";
        }
        return mac;
    }

    public string GetPhoneBrand()
    {
        string brand = sdkUtil.CallAPI<string>("GetPhoneBrand");
        if (brand == null)
        {
            brand = "";
        }
        return brand;
    }

    public string GetPhoneModel()
    {
        string model = sdkUtil.CallAPI<string>("GetPhoneModel");
        if (model == null)
        {
            model = "";
        }
        return model;
    }

    public string GetDeviceUniqueIdentifier()
    {
        string unique = SystemInfo.deviceUniqueIdentifier;
        if (unique == null)
        {
            unique = "";
        }
        return unique;
    }

    public void Message(string data)
    {
        JsonMessage jsonMessage = JsonFx.Json.JsonReader.Deserialize<JsonMessage>(data);
        switch (jsonMessage.code)
        {
            case LOGIN_SUCCESS_CODE:
                Login(jsonMessage.data);
                break;
            case CHANGE_ACCOUNT_CODE:
                ChangeAccount();
                break;
            case LOGINOUT_CODE:
                break;
            case VOICE_CODE:
                VoiceResult(jsonMessage.data);
                break; 
            case NETWORK_SINGLE_CODE:
                NetworkChange(jsonMessage.data);
                break;
            case ELECTRICITY_CODE:
                ElectricityChange(jsonMessage.data);
                break;
            case RESUME_CODE:
                sdkUtil.SetActivityIndicatorStyle();
                sdkUtil.StartActivityIndicator();
                break;
            default: 
                break;
        }
    }

    private void Login(string data)
    {
        LoginUser user = JsonFx.Json.JsonReader.Deserialize<LoginUser>(data);
        if (user.Platform == null || user.Platform.Equals(""))
        {
            user.Platform = ServerInfoConfig.GetPlatform();
        }
        if (user.AgentId == null || user.AgentId.Equals(""))
        {
            user.AgentId = ServerInfoConfig.GetAgentID();
        }
        if (user.Channel == null || user.Channel.Equals(""))
        {
            user.Channel = user.Platform;
        }
        LuaManager.instance.CallFunction("LoginManager.OnSdkLoginSuccess", user);
        LuaManager.instance.CallFunction("LoginManager.ElectricityChange", power);
    }

    private void ChangeAccount()
    {
        LuaManager.instance.CallFunction("LoginManager.ChangeAccount");
    }


    private void NetworkChange(string data)
    {
        JzwlNetworkInfo info = JsonFx.Json.JsonReader.Deserialize<JzwlNetworkInfo>(data);
        LuaManager.instance.CallFunction("Network.ChangeInfo", info);
    }

    private void ElectricityChange(string data)
    {
        JzwlElectricity info = JsonFx.Json.JsonReader.Deserialize<JzwlElectricity>(data);
        power = info.power;
        LuaManager.instance.CallFunction("LoginManager.ElectricityChange", info.power);
    }

    private void VoiceResult(string data)
    {
        VoiceInfo info = JsonFx.Json.JsonReader.Deserialize<VoiceInfo>(data);
        LuaManager.instance.CallFunction("ChatManager.CallbackVoice", info);
    }

    public string GetBundleID()
    {
        string bid = sdkUtil.CallAPI<string>("GetBundleID");
        if (bid == null)
        {
            bid = "com.kingjoy.jzwl";
        }
        return bid;
    }


    /**
     *===============================================
     * 获取sdk配置信息
     *===============================================
    */
    protected void LoadSdkConifg()
    { 
        string config = GetSdkConfig();
        if(config == null)
        {
            return;
        } 
        SDKConfig result = JsonFx.Json.JsonReader.Deserialize<SDKConfig>(config);
        ServerInfoConfig.AddConfig(result);
    }

    protected string GetSdkConfig()
    {
        sdkUtil.ClassName = GetSdkConfigClass();
        return sdkUtil.CallAPI<string>("GetConfig");
    }

    protected string GetSdkConfigClass()
    {
        return "com.sz.jzwl.sdk.PlatfromSDKConfig";
    }

    protected string GetSdkJavaClass()
    {
        return ServerInfoConfig.GetSdkClass();
    }


    protected Dictionary<string, object> NewParam()
    {
        return new Dictionary<string, object>();
    }

	/**
	*===============================================
	* Unity调用android方法
	*===============================================
	*/
	protected void SendToSDK(string method, Dictionary<string, object> dict)
	{
	string result = JsonFx.Json.JsonWriter.Serialize(dict);
	SendToSDK(method, result);
	}

	protected void SendToSDK(string method, params string[] jsonParam)
	{
	sdkUtil.CallAPI(method, jsonParam);
	}
	#elif UNITY_IOS 
	public const int LOGIN_SUCCESS_CODE = 1;
	public const int CHANGE_ACCOUNT_CODE = 2;
	public const int LOGINOUT_CODE = 3;
	public const int VOICE_CODE = 4; 
	public const int NETWORK_SINGLE_CODE = 6;
	public const int ELECTRICITY_CODE = 7;
	public const int RESUME_CODE = 8;


	public const int SUBMIT_ROLE_CREATE = 1;
	public const int SUBMIT_ROLE_LOGIN = 2;
	public const int SUBMIT_ROLE_LEVELUP = 3;
	public const int SUBMIT_ROLE_EXIT = 4;

	private static SDKUtil sdkUtil = new SDKUtil();
	private static Dictionary<string, object> sdkConfigDic = new Dictionary<string, object>();

	private static SDK sdkInstance;

	public static SDK GetInstance()
	{
		if (sdkInstance == null)
		{
			if (GameWorld.instance == null) {
				return null;
			}
			GameObject go = GameWorld.instance.gameObject;
			sdkInstance = GameUtils.GetScript<SDK>(go);
		}
		return sdkInstance;
	}

	public void Init()
	{
		this.LoadSdkConifg();
		IosManager.SDKInit ();
		this.SetCallBack ();
	}

	public void SetCallBack()
	{
		IosManager.SetCallback ("GameWorld");
	}

	public void InitJpush()
	{ 
		//SendToSDK("InitJpush", "GameWorld");
	}

	public void SetJpushInfo(long playerId, string jpushTag)
	{
		//SendToSDK("SetJpushInfo", "GameWorld", playerId.ToString(), jpushTag);
	}


	public void OpenLoginView()
	{
		IosManager.OpenLoginWindow ();
	}

	public void OpenPay(PayParam param)
	{
		IosManager.OpenPay (JsonFx.Json.JsonWriter.Serialize (param));
	}

	public void OnExit(RoleInfo roleInfo)
	{
		if (roleInfo.roleID != 0)
		{
			SubmitRoleInfo(roleInfo);
		} 
		//SendToSDK("Exit");
		IosManager.Exit();
	}

	public void SubmitRoleInfo(RoleInfo roleInfo)
	{
		//SendToSDK("SubmitRoleInfo", JsonFx.Json.JsonWriter.Serialize(roleInfo));
		IosManager.SumbitRoleInfo(JsonFx.Json.JsonWriter.Serialize (roleInfo));
	}

	public void StartVoice()
	{ 
		//("StartVoice");
		IosManager.StartVoice();
	}

	public void StopVoice(long playerId)
	{ 
		//SendToSDK("StopVoice", playerId.ToString()); 
		IosManager.StopVoice(playerId);
	}

	public void PlayVoice(string filePath, float soundValue)
	{ 
		//SendToSDK("PlayVoice", filePath, soundValue.ToString());
		IosManager.PlayVoice(filePath , soundValue.ToString());
	}

	public void StopPlayVoice()
	{ 
		//SendToSDK("StopPlayVoice");
		IosManager.StopPlayVoice();
	}

	public string GetDeviceId()
	{
		//string device = sdkUtil.CallAPI<string>("GetDeviceId");
		string device = IosManager.GetDeviceId();
		if (device == null)
		{
			device = "";
		}
		return device;
	}

	public string GetMacAddress()
	{
		//string mac = sdkUtil.CallAPI<string>("GetMacAddress");
		string mac = IosManager.GetMacAdress();
		if (mac == null)
		{
			mac = "";
		}
		return mac;
	}

	public string GetPhoneBrand()
	{
		//string brand = sdkUtil.CallAPI<string>("GetPhoneBrand");
		string brand = IosManager.GetPhoneBrand();
		if (brand == null)
		{
			brand = "";
		}
		return brand;
	}

	public string GetPhoneModel()
	{
		//string model = sdkUtil.CallAPI<string>("GetPhoneModel");
		string model = IosManager.GetPhoneModel();
		if (model == null)
		{
			model = "";
		}
		return model;
	}

	public string GetDeviceUniqueIdentifier()
	{
		string unique = SystemInfo.deviceUniqueIdentifier;
		if (unique == null)
		{
			unique = "";
		}
		return unique;
	}

	public void Message(string data)
	{
		JsonMessage jsonMessage = JsonFx.Json.JsonReader.Deserialize<JsonMessage>(data);
		switch (jsonMessage.code)
		{
		case LOGIN_SUCCESS_CODE:
			Login(jsonMessage.data);
			break;
		case CHANGE_ACCOUNT_CODE:
			ChangeAccount();
			break;
		case LOGINOUT_CODE:
			break;
		case VOICE_CODE:
			VoiceResult(jsonMessage.data);
			break; 
		case NETWORK_SINGLE_CODE:
			NetworkChange(jsonMessage.data);
			break;
		case ELECTRICITY_CODE:
			ElectricityChange(jsonMessage.data);
			break;
		case RESUME_CODE:
			sdkUtil.SetActivityIndicatorStyle();
			sdkUtil.StartActivityIndicator();
			break;
		default: 
			break;
		}
	}

	private void Login(string data)
	{
		LoginUser user = JsonFx.Json.JsonReader.Deserialize<LoginUser>(data);
		if (user.Platform == null || user.Platform.Equals(""))
		{
			user.Platform = ServerInfoConfig.GetPlatform();
		}
		if (user.AgentId == null || user.AgentId.Equals(""))
		{
			user.AgentId = ServerInfoConfig.GetAgentID();
		}
		if (user.Channel == null || user.Channel.Equals(""))
		{
			user.Channel = user.Platform;
		}
		LuaManager.instance.CallFunction("LoginManager.OnSdkLoginSuccess", user);       
	}

	private void ChangeAccount()
	{
		LuaManager.instance.CallFunction("LoginManager.ChangeAccount");
	}


	private void NetworkChange(string data)
	{
		JzwlNetworkInfo info = JsonFx.Json.JsonReader.Deserialize<JzwlNetworkInfo>(data);
		LuaManager.instance.CallFunction("Network.ChangeInfo", info);
	}

	private void ElectricityChange(string data)
	{
		JzwlElectricity info = JsonFx.Json.JsonReader.Deserialize<JzwlElectricity>(data);
		LuaManager.instance.CallFunction("LoginManager.ElectricityChange", info.power);
	}

	private void VoiceResult(string data)
	{
		VoiceInfo info = JsonFx.Json.JsonReader.Deserialize<VoiceInfo>(data);
		LuaManager.instance.CallFunction("ChatManager.CallbackVoice", info);
	}


	/**
     *===============================================
     * 获取配置信息
     *===============================================
    */
	protected void LoadSdkConifg()
	{ 
		string str = GetSdkConfig ();
		SDKConfig result = JsonFx.Json.JsonReader.Deserialize<SDKConfig>(str);
		ServerInfoConfig.AddConfig(result);
	}

	protected string GetSdkConfig()
	{
		sdkUtil.ClassName = GetSdkConfigClass();
		//return sdkUtil.CallAPI<string>("GetConfig");
		return IosManager.GetConfig();
	}

	protected string GetSdkConfigClass()
	{
		return "com.sz.jzwl.sdk.PlatfromSDKConfig";
	}

	protected string GetSdkJavaClass()
	{
		return ServerInfoConfig.GetSdkClass();
	}


	protected Dictionary<string, object> NewParam()
	{
		return new Dictionary<string, object>();
	}

    public string GetBundleID()
    {
        return "";
    }

	/**
     *===============================================
     * Unity调用android方法
     *===============================================
    */
	protected void SendToSDK(string method, Dictionary<string, object> dict)
	{
		string result = JsonFx.Json.JsonWriter.Serialize(dict);
		SendToSDK(method, result);
	}

	protected void SendToSDK(string method, params string[] jsonParam)
	{
		sdkUtil.CallAPI(method, jsonParam);
	}
#endif
}
