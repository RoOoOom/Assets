using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using UnityEngine;

public class CommonAlgorithm {
    /// <summary>
    /// 计算字符串的md5值
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string md5String(string content)
    {
        MD5 md5 = MD5.Create();

        byte[] buffer = Encoding.UTF8.GetBytes(content);

        StringBuilder sb = new StringBuilder();

        for(int i=0; i<buffer.Length;++i)
        {
            sb.Append(buffer[i].ToString("x2"));
        }
        return sb.ToString().ToLower();
    }

    /// <summary>
    /// 计算文件的md5值
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string md5file(string path)
    {
        MD5 md5 = MD5.Create();

        FileStream fs = new FileStream(path,FileMode.Open);
        byte[] buffer = md5.ComputeHash(fs);
        fs.Close();

        StringBuilder sb = new StringBuilder();

        for(int i =0;i<buffer.Length ;++i)
        {
            sb.Append(buffer[i].ToString("x2"));
        }

        return sb.ToString().ToLower();
    }
}
