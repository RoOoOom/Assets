using System;

public class IosManager
{
	public static void SDKInit()
	{
		_SDKInit ();
	}

	public static void SetCallback(string callback)
	{
		_SetCallback (callback);
	}

	public static void OpenLoginWindow()
	{
		_OpenLoginWindow ();
	}

	public static void OpenPay(string data)
	{
		_OpenPay (data);
	}

	public static void Exit()
	{
	}

	public static void SumbitRoleInfo(string roleInfo)
	{
		_SumbitRoleInfo (roleInfo);
	}

	public static void StartVoice()
	{
		_StartRecord();
	}

	public static void StopVoice(long playerId)
	{
		_StopRecord (playerId.ToString());
	}

	public static void PlayVoice(string filePath , string soundValue)
	{
		_PlayRecord (filePath, soundValue);
	}

	public static void StopPlayVoice()
	{
		_StopPlayRecord ();
	}

	public static string GetDeviceId()
	{
		//return _GetDeviceId ();
		return "";
	}

	public static string GetMacAdress()
	{
		//return _GetMacAdress ();
		return "";
	}

	public static string GetPhoneBrand()
	{
		//return  _GetPhoneBrand ();
		return "";
	}

	public static string GetPhoneModel()
	{
		//return _GetPhoneModel ();
		return "";
	}

	public static string GetConfig()
	{
		return _GetConfig ();
		//return "";
	}

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern void _SDKInit ();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern void _SetCallback (string callback);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern void  _OpenLoginWindow();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern void _OpenPay (string data);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern void _SumbitRoleInfo (string RoleInfo);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern string _GetConfig ();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern void _StartRecord ();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern void _StopRecord (string playerId);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern void _PlayRecord ( string filePath , string soundValue );

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern void _StopPlayRecord ();
	/*
	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern string _GetDeviceId();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern string _GetMacAdress ( );

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern string _GetPhoneBrand ();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern string _GetPhoneModel ();

   */

}

