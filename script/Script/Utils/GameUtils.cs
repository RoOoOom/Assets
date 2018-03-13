
// ***************************************************************
//  FileName: GameUtils.cs
//  Version : 1.0
//  Date    : 2016/6/22
//  Author  : cjzhanying 
//  Copyright (C) 2016 - Digital Technology Co.,Ltd. All rights reserved.	版权申明
//  --------------------------------------------------------------
//  Description: 游戏公共接口方法
//  -------------------------------------------------------------
//  History:
//  -------------------------------------------------------------
// ***************************************************************
// #define USER_GRAPHIC_DESIGNER
using UnityEngine;
using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

public class GameUtils : MonoBehaviour
{
    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /** @brief: -------------------------------------------变量声明------------------------------------------------------- */
    /* ====================================================================================================================*/

    #region 声明代理函数
    //声明void返回值委托
    public delegate void VoidDelegate0();
    public delegate void VoidDelegate1<T>(T arg1);
    public delegate void VoidDelegate2<T, U>(T arg1, U arg2);
    public delegate void VoidDelegate3<T, U, V>(T arg1, U arg2, V arg3);
    public delegate void VoidDelegate4<T, U, V, G>(T arg1, U arg2, V arg3, G arg4);
    //声明bool返回值委托
    public delegate bool BoolDelegate0();
    public delegate bool BoolDelegate1<T>(T arg1);
    public delegate bool BoolDelegate2<T, U>(T arg1, U arg2);
    public delegate bool BoolDelegate3<T, U, V>(T arg1, U arg2, V arg3);
    //声明int返回值委托
    public delegate int IntDelegate0();
    public delegate int IntDelegate1<T>(T arg1);
    public delegate int IntDelegate2<T, U>(T arg1, U arg2);
    public delegate int IntDelegate3<T, U, V>(T arg1, U arg2, V arg3);
    //声明object返回值委托
    public delegate object ObjectDelegate0();
    public delegate object ObjectDelegate1<T>(T arg1);
    public delegate object ObjectDelegate2<T, U>(T arg1, U arg2);
    public delegate object ObjectDelegate3<T, U, V>(T arg1, U arg2, V arg3);
    #endregion

    // 十六进制数字定义
    private static char[] gCharsHex = "0123456789ABCDEF".ToCharArray();
    // 绘制纯色背景图定义
    private static Texture2D[] gColorTextures = null;

    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------外部函数------------------------------------------------------- */
    /* ====================================================================================================================*/

    #region 字符串和16进制,10进制数字的转换

    // Byte转16进制字符串
    public static String Bytes2HexStr(byte[] bs, int index, int length)
    {
        StringBuilder sb = new StringBuilder("");
        sb.Append("[");

        int bit;
        for (int i = index; i < index + length; i++)
        {
            bit = (bs[i] & 0x0f0) >> 4;
            sb.Append(gCharsHex[bit]);
            bit = bs[i] & 0x0f;
            sb.Append(gCharsHex[bit]);
            sb.Append(" ");
        }

        sb.Append("]");
        return sb.ToString();
    }

    // Byte转10进制字符串
    public static String Bytes2DecStr(byte[] bs, int index, int length)
    {
        StringBuilder sb = new StringBuilder("");
        sb.Append("[");

        for (int i = index; i < index + length; i++)
        {
            sb.Append(bs[i]);
            sb.Append("  ");
        }

        sb.Append("]");
        return sb.ToString();
    }

    // String转16进制字符串
    public static String Str2HexStr(string str, int index, int length)
    {
        StringBuilder sb = new StringBuilder("");
        sb.Append("[");

        byte[] bs = System.Text.Encoding.UTF8.GetBytes(str);
        int bit;
        for (int i = index; i < index + length; i++)
        {
            bit = (bs[i] & 0x0f0) >> 4;
            sb.Append(gCharsHex[bit]);
            bit = bs[i] & 0x0f;
            sb.Append(gCharsHex[bit]);
            sb.Append(" ");
        }

        sb.Append("]");
        return sb.ToString();
    }

    // String转10进制字符串
    public static String String2DecStr(string str, int index, int length)
    {
        StringBuilder sb = new StringBuilder("");
        byte[] bs = System.Text.Encoding.UTF8.GetBytes(str);
        sb.Append("[");

        for (int i = index; i < index + length; i++)
        {
            sb.Append(bs[i]);
            sb.Append("  ");
        }
        sb.Append("]");
        return sb.ToString();
    }

    /************************************
     * 函数说明: 10进制数字字串转换成int
     * 返 回 值: string
     * 注意事项: 
    ************************************/
    public static int HexStrToInt(string szHex)
    {
        int iTemp = 0;
        int iIndex = 0; //多少位
        int iFu = 1;    //是否为负数
        int iKsun = 0;  //16的倍数
        int[] sum = new int[32];
        if (string.IsNullOrEmpty(szHex))
        {
            return 0;
        }
        if (szHex[0] == '-')
        {
            iFu = -1;
            szHex = szHex.Substring(1);
        }
        if (szHex.Length > 2 && szHex[0] == '0' && (szHex[1] == 'x' || szHex[1] == 'X'))
        {
            szHex = szHex.Substring(2);
        }

        int i = 0;
        iIndex = szHex.Length - 1;
        while (iIndex >= 0 && i < sum.Length)
        {
            if (szHex[iIndex] >= 'A' && szHex[iIndex] <= 'F')
            {
                sum[i] = (int)(szHex[iIndex] - 'A') + 10;
            }
            else if (szHex[iIndex] >= 'a' && szHex[iIndex] <= 'f')
            {
                sum[i] = (int)(szHex[iIndex] - 'a') + 10;
            }
            else if (szHex[iIndex] >= '0' && szHex[iIndex] <= '9')
            {
                sum[i] = szHex[iIndex] - '0';
            }
            else
            {
                i--;
            }
            iIndex--;
            i++;
        }
        if (i == 0)
        {
            return 0;
        }
        iKsun = 1;
        for (int j = 0; j < i; j++)
        {
            iTemp += sum[j] * iKsun;
            iKsun *= 16;
        }
        return iTemp * iFu;
    }
    #endregion

    #region 获取颜色字符串
    /************************************
     * 函数说明: 获取颜色字符串 eg [FF00FF]
     * 返 回 值: string
     * 参数说明: mColor
     * 注意事项: 
      ************************************/
    public static string GetColorString(Color32 mColor)
    {
        StringBuilder sb = new StringBuilder("");

        sb.Append("[");
        sb.Append(gCharsHex[mColor.a >> 4]);
        sb.Append(gCharsHex[mColor.a & 0x0f]);

        sb.Append(gCharsHex[mColor.g >> 4]);
        sb.Append(gCharsHex[mColor.g & 0x0f]);

        sb.Append(gCharsHex[mColor.b >> 4]);
        sb.Append(gCharsHex[mColor.b & 0x0f]);
        sb.Append("]");

        return sb.ToString();
    }
    #endregion

    #region 获取文件数据,文件创建,文件删除
    /************************************
     * 函数说明: 获取文件数据
     * 返 回 值: byte[]
     * 参数说明: szFile 文件全路径
     * 注意事项: 如果打开失败返回null
      ************************************/
    public static byte[] GetFileData(string szFile)
    {
        byte[] szByte = null;
        do
        {
            FileStream fs = null;
            try
            {
                if (File.Exists(szFile) == false)
                {
                    break;
                }
                fs = File.OpenRead(szFile);
                if (fs == null)
                {
                    break;
                }
                if (fs.Length > 0)
                {
                    szByte = new byte[fs.Length];
                    fs.Read(szByte, 0, (int)fs.Length);
                }
                fs.Close();
            }
            catch (System.Exception ex)
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
#if !UNITY_EDITOR
                    Debug.LogWarning("GetFileData : System.Exception == " + ex.ToString());
#endif
            }
        } while (false);
        return szByte;
    }

    /************************************
     * 函数说明: 创建文件
     * 返 回 值: bool
     * 参数说明: szFile 文件全路径
     * 参数说明: szByte 创建文件写入数据 允许为空
     * 注意事项: 如果文件存在,那么文件会被清空
      ************************************/
    public static bool CreatFile(string szFile, byte[] szByte)
    {
        bool bRet = false;
        do
        {
            FileStream fs = File.Open(szFile, FileMode.Create, FileAccess.ReadWrite);
            if (fs == null)
            {
                break;
            }
            if (szByte != null && szByte.Length > 0)
            {
                fs.Write(szByte, 0, szByte.Length);
            }
            fs.Close();
            bRet = true;
        } while (false);
        return bRet;
    }

    /************************************
      * 函数说明: 判断文件是否存在 
      * 返 回 值: bool
      * 参数说明: szFile
      * 注意事项: 
       ************************************/
    public static bool FileExist(string szFile)
    {
        try
        {
#if UNITY_IPHONE && !UNITY_EDITOR
               // return IOSInterface.IsFileExist(szFile);
#endif
            return File.Exists(szFile);
            //FileStream fs = File.Open(szFile, FileMode.Open);
            //if (fs == null)
            //{
            //    return false;
            //}
            //else
            //{
            //    fs.Close();
            //    fs = null;
            //}
            //return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    /************************************
     * 函数说明: 删除文件
     * 返 回 值: bool
     * 参数说明: szFile
     * 注意事项: 
    ************************************/
    public static bool DelFile(string szFile)
    {
        try
        {
            File.Delete(szFile);
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }
    #endregion

    #region 打开一个xml
    /************************************
     * 函数说明: 打开一个xml
     * 返 回 值: System.Xml.XmlDocument 
     * 参数说明: 传入Assert目录下的文件名
     * 注意事项: 解析示例
     * XmlDocument xmlDoc = OpenXml("xmlName.xml");
     * XmlNodeList pNodeList = xmlDoc.SelectNodes(string.Format("/xmlNode/Name ..."));
     * foreach (XmlNode pNode in pNodeList)
     * {
     *      pNode.Attributes["id"].Value;
     * }
     * 注: SelectNodes("item") 从当前节点的儿子节点中选择名称为 item 的节点。
     *     SelectNodes("/item") 从根节点的儿子节点中选择名称为 item 的节点。
     *     SelectNodes("//item") 从任意位置的节点上选择名称为 item 的节点
     *     SelectNodes(".") 选择当前节点。
     *     SelectNodes("..") 选择当前节点的父节点。
     *     SelectNodes("//item[@name]") 在 SelectNodes("//item") 的基础上，增加了一个限制，就是要求拥有 name 属性。
     *     http://blog.csdn.net/eric_guodongliang/article/details/7187880
      ************************************/
    public static XmlDocument OpenXml(string szName)
    {
        string szXmlFile = "";
        XmlDocument xmlDoc = null;
        /** @brief: 优先获取外部T卡资源 如果没有找到 从手机内部查找 */
        {
            byte[] szByte = null;
            if (szName.StartsWith("Config/") == false)
            {
                szName = "Config/" + szName;
            }
            /** @brief: 尝试从外部T卡获取资源 */
            if (szByte == null)
            {
                szXmlFile = PathUtils.MakeFilePath(szName, PathUtils.PathType.MobileTCardStreamAssert);
                szByte = GetFileData(szXmlFile);
            }
            /** @brief: 如果找到解析数据 */
            if (szByte != null)
            {
                //此处可以进行解密
                //WGDecoder.DecodeForFileBuff(ref szBuff);
                //把byte写入流中
                MemoryStream memStream = new MemoryStream();
                BufferedStream buffStream = new BufferedStream(memStream);
                buffStream.Write(szByte, 0, szByte.Length);
                buffStream.Seek(0, SeekOrigin.Begin);
                xmlDoc = new XmlDocument();
                xmlDoc.Load(buffStream);
                buffStream.Close();
                memStream = null;
                buffStream = null;
            }
            /** @brief: 都没有找到只能从Resource目录查询 */
            else
            {
                xmlDoc = OpenResourcesXml(szName);
            }
        }
        if (xmlDoc == null)
        {
            Debug.LogError("OpenXml Error , File : " + szXmlFile);
        }
        return xmlDoc;
    }
    /************************************
     * 函数说明: 直接打开Resource目录的xml
     * 返 回 值: System.Xml.XmlDocument
     * 参数说明: szName
     * 注意事项: 
    ************************************/
    public static XmlDocument OpenResourcesXml(string szName)
    {
        XmlDocument xmlDoc = null;
        string szResName = szName.Substring(0, szName.Length - 4);
        /** @brief: 由于我们xml仅加载一次就释放,所以可以直接使用Resource.Load加载并释放 否则资源管理器的话不要释放,但速度会提升 */
        TextAsset xmlData = Resources.Load(szResName, typeof(TextAsset)) as TextAsset;
        if (xmlData == null)
        {
            xmlDoc = null;
        }
        else
        {
            byte[] szBuff = new byte[xmlData.bytes.Length];
            byte[] szData = xmlData.bytes;
            ByteCopyToByte(ref szData, ref szBuff, 0);
            //此处可以进行解密
            //WGDecoder.DecodeForFileBuff(ref szBuff);
            //把byte写入流中
            MemoryStream memStream = new MemoryStream();
            BufferedStream buffStream = new BufferedStream(memStream);
            buffStream.Write(szBuff, 0, szBuff.Length);
            buffStream.Seek(0, SeekOrigin.Begin);
            xmlDoc = new XmlDocument();
            xmlDoc.Load(buffStream);
            buffStream.Close();
            /** @brief: 释放资源 */
            Resources.UnloadAsset(xmlData);
            xmlData = null;
        }
        if (xmlDoc == null)
        {
            Debug.LogError("OpenResourcesXml Error , File : " + szName);
        }
        return xmlDoc;
    }
    #endregion

    #region 解析读取XML
    /************************************
     * 函数说明: 读取一个xml节点的int值
     * 返 回 值: int 
     ************************************/
    public static int XmlReadInt(XmlNode xmlNode, string key, int def)
    {
        int result = 0;
        try
        {
            result = def;
            if (xmlNode != null)
            {
                if (xmlNode.Attributes.GetNamedItem(key) != null)
                {
                    result = int.Parse(xmlNode.Attributes[key].Value);
                }
            }
        }
        catch (System.Exception)
        {
            result = def;
        }
        return result;
    }

    /************************************
     * 函数说明: 读取一个xml节点的float值
     * 返 回 值: float 
     ************************************/
    public static float XmlReadFloat(XmlNode xmlNode, string key, float def)
    {
        float result = 0f;
        try
        {
            result = def;
            if (xmlNode != null)
            {
                if (xmlNode.Attributes.GetNamedItem(key) != null)
                {
                    result = float.Parse(xmlNode.Attributes.GetNamedItem(key).Value);
                }
            }
        }
        catch (System.Exception)
        {
            result = def;
        }
        return result;
    }

    /************************************
     * 函数说明: 读取一个xml节点的long值
     * 返 回 值: float 
     ************************************/
    public static long XmlReadLong(XmlNode xmlNode, string key, long def)
    {
        long result = 0;
        try
        {
            result = def;
            if (xmlNode != null)
            {
                if (xmlNode.Attributes.GetNamedItem(key) != null)
                {
                    result = long.Parse(xmlNode.Attributes[key].Value);
                }
            }
        }
        catch (System.Exception)
        {
            result = def;
        }
        return result;
    }

    /************************************
     * 函数说明: 读取一个xml节点的string值
     * 返 回 值: string 
     ************************************/
    public static string XmlReadString(XmlNode xmlNode, string key, string def)
    {
        string result = "";
        try
        {
            result = def;
            if (xmlNode != null)
            {
                if (xmlNode.Attributes.GetNamedItem(key) != null)
                {
                    result = xmlNode.Attributes[key].Value;
                }
            }
        }
        catch (System.Exception)
        {
            result = def;
        }
        return result;
    }

    /************************************
     * 函数说明: 读取一个xml节点的List<int>值
     * 返 回 值: List<int> 
     ************************************/
    public static List<int> XmlReadIntList(XmlNode xmlNode, string key, int[] def)
    {
        List<int> result = XmlReadIntList(xmlNode, key);
        if (result.Count == 0 && def != null)
        {
            result.AddRange(def);
        }
        return result;
    }

    /************************************
     * 函数说明: 读取一个xml节点的List<int>值
     * 返 回 值: List<int> 
     ************************************/
    public static List<int> XmlReadIntList(XmlNode xmlNode, string key)
    {
        string szData = "";
        if (xmlNode != null)
        {
            if (xmlNode.Attributes.GetNamedItem(key) != null)
            {
                szData = xmlNode.Attributes[key].Value;
            }
        }
        List<int> mArray = new List<int>();
        if (string.IsNullOrEmpty(szData))
        {
            return mArray;
        }
        while (string.IsNullOrEmpty(szData) == false)
        {
            string szNum = GetNumBuff(szData, ref szData);
            if (string.IsNullOrEmpty(szNum))
            {
                break;
            }
            else
            {
                int iValue = int.Parse(szNum);
                mArray.Add(iValue);
            }
        }
        return mArray;
    }
    #endregion

    #region bit字节操作
    /************************************
     * 函数说明: 查看一个bit
     * 返 回 值: int 
     * 参数说明:
     * 注意事项: 
      ************************************/
    public static int GetBitValue(int c, int b)
    {
        return ((c & (1 << b)) != 0 ? 1 : 0);
    }

    /************************************
     * 函数说明: 翻转一个bit
     * 返 回 值: int 
     * 参数说明:
     * 注意事项: 
    ************************************/
    public static int ReversionBitValue(int c, int b)
    {
        return c ^= (1 << b);
    }

    /************************************
     * 函数说明: 设置bit值为0
     * 返 回 值: int 
     * 参数说明:
     * 注意事项: 
    ************************************/
    public static int SetBitValueFalse(int c, int b)
    {
        return c &= ~(1 << b);
    }

    /************************************
     * 函数说明: 设置bit值为1
     * 返 回 值: int 
     * 参数说明:
     * 注意事项: 
    ************************************/
    public static int SetBitValueTrue(int c, int b)
    {
        return c |= 1 << b;
    }
    #endregion

    #region 字符串提取数字或者指定字符串,常用操作
    /************************************
     * 函数说明: 从一个byte[]串中获取第一个数字的字符串,返回数字的字符串
     * 返 回 值: string
     * 参数说明: szData 输入数据 , szTail 剩余待处理的数据
     * 注意事项: 只提取十进制的数字
      ************************************/
    public static string GetNumBuff(string szData, ref string szTail)
    {
        bool bRet = false;
        if (string.IsNullOrEmpty(szData))
        {
            return null;
        }

        int iIndex = 0;
        string szNum = "";
        bool bIsFu = false;
        for (iIndex = 0; iIndex < szData.Length; iIndex++)
        {
            if (szData[iIndex] >= '0' && szData[iIndex] <= '9')
            {
                szNum += szData[iIndex];
                bRet = true;
            }
            else
            {
                /** @brief: 负数 */
                if (szData[iIndex] == '-' && bRet == false)
                {
                    bIsFu = true;
                }
                else
                {
                    if (bRet == true)
                    {
                        break;
                    }
                }
            }
        }
        if (szTail != null)
        {
            if (szData.Length == iIndex)
            {
                szTail = "";
            }
            else
            {
                szTail = szData.Substring(iIndex);
            }
        }
        if (bIsFu == true && string.IsNullOrEmpty(szNum) == false)
        {
            szNum = "-" + szNum;
        }
        return szNum;
    }
    public static string GetNumBuff(string szData)
    {
        string szTail = "";
        return GetNumBuff(szData, ref szTail);
    }
    /************************************
     * 函数说明: 截取中间字符串
     * 返 回 值: string 
     * 参数说明: 获取中间的字符串eg "www.baidu.comAA"  src = "www." dst = ".com",那么中间字符串就是baidu,返回新的剩余字符串 "AA"
     * 注意事项: 返回的字符串是新的字符串
    ************************************/
    public static string GetMidBuffer(ref string szSavebuf, string szData, string szSrc, string szDst)
    {
        string szTemp = szData;
        if (szSavebuf == null)
        {
            return null;
        }
        if (string.IsNullOrEmpty(szData))
        {
            return null;
        }
        if (string.IsNullOrEmpty(szSrc))
        {

        }
        else
        {
            int iPos = szTemp.IndexOf(szSrc);
            if (iPos == -1)
            {
                return null;
            }
            szTemp = szTemp.Substring(iPos + szSrc.Length);
            szData = szTemp;
        }
        if (string.IsNullOrEmpty(szDst))
        {

        }
        else
        {
            int iPos = szTemp.IndexOf(szDst);
            if (iPos == -1)
            {
                return null;
            }
            szTemp = szTemp.Substring(0, iPos);
            szData = szData.Substring(iPos + szDst.Length);
        }
        szSavebuf = szTemp;
        return szData;
    }

    /************************************
     * 函数说明: 提取一个字符串中的所有int数据常用于解析123#456类似数据
     * 返 回 值: ArrayList 
     * 参数说明: 
     * 注意事项: 
    ************************************/
    public static ArrayList GetIntArrayWithString(string szData)
    {
        ArrayList mArray = new ArrayList();
        if (string.IsNullOrEmpty(szData))
        {
            return mArray;
        }
        while (string.IsNullOrEmpty(szData) == false)
        {
            string szNum = GetNumBuff(szData, ref szData);
            if (string.IsNullOrEmpty(szNum))
            {
                break;
            }
            else
            {
                int iValue = int.Parse(szNum);
                mArray.Add(iValue);
            }
        }
        return mArray;
    }
    public static int[] GetIntVectorWithString(string szData)
    {
        ArrayList mArray = GetIntArrayWithString(szData);
        return ArrayListTo<int>(mArray);
    }
    #endregion

    #region Byte[] 对Byte[]的拷贝
    /************************************
     * 函数说明: Byte[] 对Byte[]的拷贝
     * 返 回 值: void
     * 注意事项: 自己确保下标Ok, 确保有足够长度存储数据
      ************************************/
    public static bool ByteCopyToByte(ref byte[] inByte, int inSrc, int inDst, ref byte[] outByte, int outScr)
    {
        if ((inDst <= inSrc || inSrc < 0) || (outScr < 0))
        {
            return false;
        }
        int iCount = inDst - inSrc;
        if (iCount > (outByte.Length - outScr))
        {
            iCount = outByte.Length - outScr;
            Debug.LogWarning("ByteCopyToByte : Lenght Warning");
        }
        Array.Copy(inByte, inSrc, outByte, outScr, iCount);
        return true;
    }
    public static bool ByteCopyToByte(ref byte[] inByte, ref byte[] outByte, int outScr)
    {
        return ByteCopyToByte(ref inByte, 0, inByte.Length, ref outByte, outScr);
    }
    #endregion

    #region string和byte[]相互转换
    /************************************
     * 函数说明: string转换成byte[]
     * 返 回 值: byte[]
     * 参数说明: szBuff
     * 注意事项: 
      ************************************/
    public static byte[] StringToByte(string szBuff)
    {
        byte[] byteArray = System.Text.Encoding.Default.GetBytes(szBuff);
        return byteArray;
    }
    public static string ByteToString(byte[] szByte)
    {
        string szBuff = System.Text.Encoding.Default.GetString(szByte);
        return szBuff;
    }
    #endregion

    #region 设置child到parent同时初始化自己的scale为(1,1,1)
    /*********************************
     * 函数说明: 设置child到parent同时初始化自己的scale为(1,1,1)
     * 返 回 值: 
     * 参数说明: 
     * 注意事项: 
     *********************************/
    public static void SetNGUIParent(Transform parent, Transform child)
    {
        Vector3 position = child.localPosition;
        child.parent = parent;
        child.localPosition = position;
        child.localScale = Vector3.one;
    }
    #endregion

    #region 获取子节点身上的某个Script脚本或者某个GameObject,主要用来减少判断
    /************************************
     * 函数说明: 获取子节点身上的某个Script脚本或者某个GameObject,主要用来减少判断
     * 返 回 值: T
     * 参数说明: mParent 父节点
     * 参数说明: szPath 相对于父节点的路径
     * 注意事项: 
      ************************************/
    public static T FindFromChild<T>(GameObject mParent, string szPath) where T : UnityEngine.Object
    {
        return FindFromChild<T>(mParent, szPath, true);
    }
    public static T FindFromChild<T>(GameObject mParent, string szPath, bool bLog) where T : UnityEngine.Object
    {
        T ret = default(T);
        if (mParent == null || string.IsNullOrEmpty(szPath))
        {
            if (bLog)
            {
                Debug.Log("FindFromChild : Error Argument ");
            }
            return ret;
        }
        Transform mTransform = mParent.transform.Find(szPath);
        if (mTransform == null || mTransform.gameObject == null)
        {
            if (bLog)
            {
                Debug.LogError("FindFromChild : Error for find " + mParent.name + "/" + szPath);
            }
            return ret;
        }
        if (typeof(T).ToString() == typeof(GameObject).ToString())
        {
            ret = (T)(UnityEngine.Object)mTransform.gameObject;
        }
        else
        {
            ret = (T)(UnityEngine.Object)mTransform.gameObject.GetComponent(typeof(T).ToString());
            if (ret == null)
            {
                if (bLog)
                {
                    Debug.LogError("FindFromChild Not Find " + mParent.name + "/" + szPath + " " + typeof(T).ToString());
                }
            }
        }
        return ret;
    }
    #endregion

    #region 把ArrayList的T[]
    /************************************
     * 函数说明: 把ArrayList的T[]
     * 返 回 值: T[]
     * 注意事项: 内部添加的是一样的
      ************************************/
    public static T[] ArrayListTo<T>(ArrayList mArray)
    {
        T[] Temp = null;
        if (mArray != null && mArray.Count > 0)
        {
            Temp = new T[mArray.Count];
            for (int i = 0; i < mArray.Count; i++)
            {
                Temp[i] = (T)mArray[i];
            }
        }
        return Temp;
    }
    #endregion

    #region NGUI中的物体获取屏幕坐标
    /************************************
     * 函数说明: NGUI中的物体获取屏幕坐标
     * 返 回 值: UnityEngine.Vector3
     * 注意事项: 无
      ************************************/
#if !USER_GRAPHIC_DESIGNER
    public static Vector3 NGUIGetScreenPosition(GameObject target)
    {

        Vector3 targetPos = Vector2.zero;
        // if (target != null)
        // {
        //     if (UIManagerDef.g2DUICamera != null)
        //     {
        //         Camera nguiCamera = UIManagerDef.g2DUICamera.camera;
        //         // 首先把世界坐标转换成屏幕坐标
        //         Vector3 pos = nguiCamera.WorldToViewportPoint(target.transform.position);
        //         pos.x = pos.x * UIManagerDef.GetRealScreenWidth();
        //         pos.y = pos.y * UIManagerDef.GetRealScreenHeight();
        //         pos.z = 0;
        //         targetPos = pos;
        //     }
        // }
        return targetPos;
    }
#endif
    #endregion

    #region 绘制几何图形
    /*********************************
     * 函数说明: 画圆
     * 返 回 值: void
     * 参数说明: trans
     * 参数说明: radius
     * 参数说明: color
     * 注意事项: 无
     *********************************/
    public static void DrawCircle(Transform trans, float radius, Color color)
    {
        if (trans == null)
            return;

        // 设置矩阵
        Matrix4x4 defaultMatrix = Gizmos.matrix;
        Gizmos.matrix = trans.localToWorldMatrix;
        Color defaultColor = Gizmos.color;
        Gizmos.color = color;

        // 绘制圆环
        Vector3 beginPoint = Vector3.zero;
        Vector3 firstPoint = Vector3.zero;
        for (float theta = 0; theta < 2 * Mathf.PI; theta += 0.1f)  //值越低圆环越平滑
        {
            float x = radius * Mathf.Cos(theta);
            float z = radius * Mathf.Sin(theta);
            Vector3 endPoint = new Vector3(x, 0, z);
            if (theta == 0)
                firstPoint = endPoint;
            else
                Gizmos.DrawLine(beginPoint, endPoint);
            beginPoint = endPoint;
        }
        // 绘制最后一条线段
        Gizmos.DrawLine(firstPoint, beginPoint);

        Gizmos.color = defaultColor;
        Gizmos.matrix = defaultMatrix;
    }
    public static void DrawCircle(Vector3 point, float radius, Color color)
    {
        Color defaultColor = Gizmos.color;
        Gizmos.color = color;

        // 绘制圆环
        Vector3 beginPoint = Vector3.zero;
        Vector3 firstPoint = Vector3.zero;
        for (float theta = 0; theta < 2 * Mathf.PI; theta += 0.1f)  //值越低圆环越平滑
        {
            float x = radius * Mathf.Cos(theta);
            float z = radius * Mathf.Sin(theta);
            Vector3 endPoint = new Vector3(x, 0, z) + point;
            if (theta == 0)
            {
                firstPoint = endPoint;
            }
            else
            {
                Gizmos.DrawLine(beginPoint, endPoint);
            }
            beginPoint = endPoint;
        }
        // 绘制最后一条线段
        Gizmos.DrawLine(firstPoint, beginPoint);
        Gizmos.color = defaultColor;
    }

    /*********************************
     * 函数说明: 画矩形
     * 返 回 值: void
     * 参数说明: trans
     * 参数说明: width
     * 参数说明: height
     * 参数说明: anchor
     * 参数说明: color
     * 注意事项: 无
     *********************************/
    public static void DrawRect(Transform trans, float width, float height, Vector2 anchor, Color color)
    {
        if (trans == null)
        {
            return;
        }

        Matrix4x4 defaultMatrix = Gizmos.matrix;
        Gizmos.matrix = trans.localToWorldMatrix;
        Color defaultColor = Gizmos.color;
        Gizmos.color = color;

        Vector3 p1 = new Vector3(-width * anchor.x, 0, -height * anchor.y);
        Vector3 p2 = new Vector3(p1.x, 0, p1.z + height);
        Vector3 p3 = new Vector3(p2.x + width, 0, p2.z);
        Vector3 p4 = new Vector3(p3.x, 0, p1.z);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);

        Gizmos.color = defaultColor;
        Gizmos.matrix = defaultMatrix;
    }
    #endregion

    #region 创建纯色背景图
    /************************************
     * 函数说明: 获取要绘制的纹理
     * 返 回 值: UnityEngine.Texture2D
     * 参数说明: color 颜色枚举 或者颜色具体值
     * 参数说明: iWidth 宽度
     * 参数说明: iHeight 高度
     * 注意事项: 无
      ************************************/
    public enum ColorTextrue
    {
        Red,
        RedA,
        Green,
        GreenA,
        Blue,
        BlueA,
        White,
        WhiteA,
        Black,
        BlackA,
        Max
    }
    public static Texture2D GetColorTexture(ColorTextrue color)
    {
        if (gColorTextures == null)
        {
            InitColorTextures();
        }
        return gColorTextures[(int)color];
    }
    public static Texture2D GetColorTexture(Color32 color)
    {
        return GetColorTexture_Private(color, 2, 2);
    }
    public static Texture2D GetColorTexture(ColorTextrue color, int iWidth, int iHeight)
    {
        if (gColorTextures == null)
        {
            InitColorTextures();
        }
        iWidth = Math.Max(2, iWidth);
        iHeight = Math.Max(2, iHeight);
        return GetColorTexture_Private(gColorTextures[(int)color].GetPixel(0, 0), iWidth, iHeight);
    }
    public static Texture2D GetColorTexture(Color32 color, int iWidth, int iHeight)
    {
        if (gColorTextures == null)
        {
            InitColorTextures();
        }
        iWidth = Math.Max(2, iWidth);
        iHeight = Math.Max(2, iHeight);
        return GetColorTexture_Private(color, iWidth, iHeight);
    }
    #endregion
    #region 查找或者创建GameObject
    /// <summary>
    /// 根据路径查找GO，如果没有的话，手动创建一个
    /// </summary>
    /// <param name="url"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static GameObject FindOrCreate(string url, string name = "GameObject")
    {
        GameObject go = GameObject.Find(url);
        if (null == go)
        {
            go = new GameObject(name);
        }
        return go;
    }
    #endregion

    #region 获取脚本
    /************************************
     * 函数说明: 获取脚本
     * 返 回 值: T
     * 参数说明: go
     * 注意事项: 如果没有自动添加脚本
      ************************************/
    public static T GetScript<T>(GameObject go) where T : UnityEngine.Component
    {
        T mScript = null;
        do
        {
            if (go == null)
            {
                break;
            }
            mScript = go.GetComponent<T>();
            if (mScript == null)
            {
                mScript = go.AddComponent<T>();
            }
        } while (false);

        return mScript;
    }

    public static T[] GetSelfAndChildsScript<T>(GameObject go, bool includeInactive = false) where T : UnityEngine.Component
    {
        T[] mVecScript = null;
        do
        {
            if (go == null)
            {
                break;
            }

            mVecScript = go.GetComponentsInChildren<T>(includeInactive);

        } while (false);

        return mVecScript;
    }
    #endregion

    #region 获取GameObject的全路径
    /************************************
     * 函数说明: 获取路径
     * 返 回 值: string
     * 参数说明: from
     * 注意事项: 
      ************************************/
    public static string GetGameObjectPath(GameObject target)
    {
        if (target == null)
        {
            return "";
        }
        string szPath = target.transform.name;
        while (target.transform.parent != null)
        {
            szPath = target.transform.parent.name + "/" + szPath;
            target = target.transform.parent.gameObject;
        }
        return szPath;
    }
    #endregion

    #region 常用文字字串处理
    /*********************************
     * 函数说明: 限制文字字数
     * 返 回 值: string
     * 参数说明: text
     * 参数说明: maxCharNum
     * 注意事项: 无
     *********************************/
    public static string SetTextMaxCount(string text, int maxCharNum)
    {
        /* 该方法用于验证中文和字母 */
        int count = 0;
        int index = 0;
        Regex pattern = new Regex("^[\u4e00-\u9fa5-?_]+$");///^[\u4E00-\u9FA5\uF900-\uFA2D]+$/  //验证中文
        Regex pattern1 = new Regex("\u0008|\r");
        text = pattern1.Replace(text, "");
        for (int i = 0; i < text.Length; i++)
        {
            string str = text[i].ToString();
            if (pattern.IsMatch(str))
            {
                // 中文
                count += 2;
            }
            else
            {
                // 其他
                count += 1;
            }
            if (count <= maxCharNum)
            {
                index++;
            }
        }
        return text.Substring(0, index);
    }

    /*********************************
     * 函数说明: 统一金币文本获取
     * 返 回 值: string
     * 参数说明: money
     * 注意事项: 无
     *********************************/
    public static string GetMoneyText(long value)
    {
        value = Math.Max(0, value);
        string str = value.ToString();
        int length = str.Length;

        if (length >= 7)
        {
            str = str.Substring(0, length - 4);
            str += "W";
            return str;
        }
        int surplse = length % 3;
        int num = length / 3;
        num = (surplse > 0) ? num : (num - 1);

        for (int i = 0; i < num; i++)
        {
            str = str.Insert(length - 3 * (i + 1), ",");
        }
        return str;
    }

    /*********************************
     * 函数说明: 统一时间获取接口
     * 返 回 值: string
     * 参数说明: second
     * 注意事项: 无
     *********************************/
    public static string GetTimeText(int value)
    {
        value = Math.Max(0, value);
        int hour = value / 3600;
        int min = value % 3600 / 60;
        int sec = value % 3600 % 60;
        return (hour > 9 ? hour.ToString() : "0" + hour.ToString()) + (min > 9 ? min.ToString() : "0" + min.ToString()) + (sec > 9 ? min.ToString() : "0" + sec.ToString());
    }
    #endregion

    #region 判断字符串是否是数字
    /*********************************
     * 函数说明: 判定是否是数字
     * 返 回 值: bool
     * 参数说明: szData
     * 注意事项: 无
     *********************************/
    public static bool StrIsNum(string szText)
    {
        if (string.IsNullOrEmpty(szText))
        {
            return false;
        }

        for (int iIndex = 0; iIndex < szText.Length; iIndex++)
        {
            if (szText[iIndex] >= '0' && szText[iIndex] <= '9')
            {
                continue;
            }
            else
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    /* ====================================================================================================================*/
    /** @brief: -------------------------------------------内部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数------------------------------------------------------- */
    /** @brief: -------------------------------------------内部函数------------------------------------------------------- */
    /* ====================================================================================================================*/

    #region 获取指定颜色
    /*********************************
     * 函数说明: 初始化默认颜色记录
     * 返 回 值: void
     * 注意事项: 无
     *********************************/
    static void InitColorTextures()
    {
        gColorTextures = new Texture2D[(int)ColorTextrue.Max];
        //初始化纯颜色图片
        gColorTextures[(int)ColorTextrue.Red] = GetColorTexture(new Color(1, 0, 0, 1));
        gColorTextures[(int)ColorTextrue.RedA] = GetColorTexture(new Color(1, 0, 0, 0.5f));
        gColorTextures[(int)ColorTextrue.Blue] = GetColorTexture(new Color(0, 0, 1, 1));
        gColorTextures[(int)ColorTextrue.BlueA] = GetColorTexture(new Color(0, 0, 1, 0.5f));
        gColorTextures[(int)ColorTextrue.Green] = GetColorTexture(new Color(0, 1, 0, 1));
        gColorTextures[(int)ColorTextrue.GreenA] = GetColorTexture(new Color(0, 1, 0, 0.5f));
        gColorTextures[(int)ColorTextrue.White] = GetColorTexture(new Color(1, 1, 1, 1));
        gColorTextures[(int)ColorTextrue.WhiteA] = GetColorTexture(new Color(1, 1, 1, 0.5f));
        gColorTextures[(int)ColorTextrue.Black] = GetColorTexture(new Color(0, 0, 0, 1));
        gColorTextures[(int)ColorTextrue.BlackA] = GetColorTexture(new Color(0, 0, 0, 0.5f));
    }

    /*********************************
     * 函数说明: 获取指定颜色的纹理
     * 返 回 值: UnityEngine.Texture2D
     * 参数说明: color
     * 参数说明: iWidth
     * 参数说明: iHeight
     * 注意事项: 无
     *********************************/
    static Texture2D GetColorTexture_Private(Color color, int iWidth, int iHeight)
    {
        Texture2D texRet = new Texture2D(iWidth, iHeight);
        Color[] colors = new Color[iWidth * iHeight];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }
        texRet.SetPixels(colors);
        texRet.Apply();
        return texRet;
    }
    #endregion

    /// <summary> 删除制定文件夹以及meta文件 </summary>
    /// <param name="path"> 文件夹路径 </param>
    public static void DeleteDirAndMeta(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            string metaFile = path.Substring(0, path.Length - 1) + ".meta";
            if (File.Exists(metaFile))
            {
                File.Delete(metaFile);
            }
        }
    }

    /// <summary> float 0值比较 </summary>
    public static bool EqualZero(float f, float percision = 0.00001f)
    {
        return f < percision && f > -percision;
    }

    public static bool MoreThanZero(float f, float percision = 0.00001f)
    {
        return f > percision;
    }

    public static bool LessThanZero(float f, float percision = 0.00001f)
    {
        return f < -percision;
    }

    /// <summary> 通过文件hash值判定文件是否一致 </summary>
    /// <returns> true：文件一致 </returns>
    /// <returns> false：文件不一致 </returns>
    public static bool IsTheSameFile(string filePathA, string filePathB)
    {
        using(HashAlgorithm hash = HashAlgorithm.Create())
        {
            if (!File.Exists(filePathA))
            {
                return !File.Exists(filePathB);
            }

            if (!File.Exists(filePathB))
            {
                return false;
            }

            using (FileStream file1 = new FileStream(filePathA, FileMode.Open), file2 = new FileStream(filePathB, FileMode.Open))
            {
                if (file1.Length != file2.Length)
                {
                    return false;
                }

                byte[] hashByte1 = hash.ComputeHash(file1);
                byte[] hashByte2 = hash.ComputeHash(file2);
                string str1 = BitConverter.ToString(hashByte1);
                string str2 = BitConverter.ToString(hashByte2);
                return (str1 == str2);
            }
        }
    }

    static DateTime start = new DateTime(1970, 1, 1);
    /// <summary>
    /// 获取当前毫秒数 
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static long GetMilliseconds(DateTime dt)
    {
        TimeSpan span = dt - start;
        return (long)span.Milliseconds;
    }

    /// <summary>
    /// 添加元素到字典中
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <param name="dic"></param>
    /// <param name="key"></param>
    /// <param name="val"></param>
 
    public static void AddItemToDic<K, V>(Dictionary<K, V> dic, K key, V val)
    {
        if (!dic.ContainsKey(key))
            dic.Add(key, val);
        else
            dic[key] = val;
    }
}