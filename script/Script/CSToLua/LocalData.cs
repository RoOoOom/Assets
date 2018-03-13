using UnityEngine;
using System.Text;
using System;

public class LocalData
{
    public static void SetInt(string key, int value) { PlayerPrefs.SetInt(key, value); }
    public static int GetInt(string key) { return PlayerPrefs.GetInt(key, 0); }

    public static bool HasString(string key) { return PlayerPrefs.HasKey(key) && !string.IsNullOrEmpty(PlayerPrefs.GetString(key)); }

    public static bool HasKey(string key)
    {
        if (key == null) return false;
        return PlayerPrefs.HasKey(key);
    }
    public static void SetString(string key, string value) 
    {
        byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(value.ToString());
        PlayerPrefs.SetString(key, Convert.ToBase64String(toEncryptArray, 0, toEncryptArray.Length));
    }
    public static string GetString(string key)
    {
        byte[] toEncryptArray = Convert.FromBase64String(PlayerPrefs.GetString(key, ""));
        return UTF8Encoding.UTF8.GetString(toEncryptArray);
    }

    public static void SetFloat(string key, float value) { PlayerPrefs.SetFloat(key, value); }
    public static float GetFloat(string key) { return PlayerPrefs.GetFloat(key, 0f); }

    public static void Delete(string key) { PlayerPrefs.DeleteKey(key); }
    public static void DeleteAll() { PlayerPrefs.DeleteAll(); } //！！！注意，在不确定的情况下不调用DeleteAll
}
