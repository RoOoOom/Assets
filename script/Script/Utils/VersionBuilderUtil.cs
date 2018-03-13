using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface ICodeProgress
{
    /// <summary>
    /// Callback progress.
    /// </summary>
    /// <param name="inSize">
    /// input size. -1 if unknown.
    /// </param>
    /// <param name="outSize">
    /// output size. -1 if unknown.
    /// </param>
    void SetProgress(Int64 inSize, Int64 outSize);
};

public class ZipCodeProgress : ICodeProgress
{
    public delegate void ProgressDelegate(Int64 fileSize, Int64 processSize);

    public ProgressDelegate m_ProgressDelegate = null;

    public ZipCodeProgress(ProgressDelegate del)
    {
        m_ProgressDelegate = del;
    }

    public void SetProgress(Int64 inSize, Int64 outSize)
    {
        m_ProgressDelegate(inSize, outSize);
    }
}

public class ZipFileDesc
{
    public int id = 0;
    public int startIndex = 0;
    public int size = 0;
    public int nameLength = 0;
    public string name = "";
    public byte[] data = null;
};

public class VersionBuilderUtil
{
    public const string MANIFEST_FILE_SUF = ".manifest";
    public const string BUNDLE_FIEL_SUF = ".ab";
    public const string TXT_SUF = ".txt";

    public static VersionConfig ReadVersion(string path)
    {
        VersionConfig version = new VersionConfig();
        if (File.Exists(path))
        {
            StreamReader sr = new StreamReader(path);
            string content = sr.ReadToEnd();
            sr.Close();
            version = JsonFx.Json.JsonReader.Deserialize<VersionConfig>(content);
        }
        else
        {
            Debug.LogError("目标路径没有版本文件: " + path);
        }
        return version;
    }

    public static VersionBundleConfig ReadConfig(string configFilePath)
    {
        VersionBundleConfig config = new VersionBundleConfig();
        if (File.Exists(configFilePath))
        {
            StreamReader sr = new StreamReader(configFilePath);
            string content = sr.ReadToEnd();
            sr.Close();
            config = JsonFx.Json.JsonReader.Deserialize<VersionBundleConfig>(content);
        }
        else
        {
            Debug.LogError("目标路径没有配置文件: " + configFilePath);
        }
        return config;
    }

    public static VersionBundleHashConfig ReadBundleHash(string path)
    {
        VersionBundleHashConfig hash = new VersionBundleHashConfig();
        if (File.Exists(path))
        {
            StreamReader sr = new StreamReader(path);
            string content = sr.ReadToEnd();
            sr.Close();
            hash = JsonFx.Json.JsonReader.Deserialize<VersionBundleHashConfig>(content);
        }
        else
        {
            Debug.LogError("目标路径没有hash文件: " + path);
        }
        return hash;
    }

    public static void WriteConfig(string configFilePath, VersionBundleConfig config)
    {
        if (null == config)
        {
            Debug.LogError("配置文件不能为空，请检查一下");
            return;
        }

#if UNITY_EDITOR
        if (EditorUtility.DisplayDialog("路径检测", "是否保存配置文件到： " + configFilePath, "确定", "取消"))
        {
            if (File.Exists(configFilePath)) File.Delete(configFilePath); 

            File.WriteAllText(configFilePath, JsonFx.Json.JsonWriter.Serialize(config));
        }
#endif
    }

    public static List<VersionBundle> ReadManifestFile(string path)
    {
        List<VersionBundle> bundles = new List<VersionBundle>();
        if (File.Exists(path))
        {
            StreamReader sr = new StreamReader(path);
            string content = sr.ReadToEnd();
            sr.Close();
            string[] lines = content.Split(new string[] { "\n" }, StringSplitOptions.None);
            VersionBundle bundle = null;
            List<string> dependencies = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("Info_"))
                {
                    if (null != bundle)
                    {
                        bundle.dependency = dependencies.ToArray();
                        bundles.Add(bundle);
                        bundle = new VersionBundle();
                    }
                    else bundle = new VersionBundle();
                }
                else if (lines[i].Contains("Name"))
                {
                    string[] name = lines[i].Split(new string[] { ":" }, StringSplitOptions.None);
                    bundle.id = name[1].Trim();
                }
                else if (lines[i].Contains("Dependencies"))
                {
                    dependencies.Clear();
                }
                else if (lines[i].Contains("Dependency"))
                {
                    string[] dependency = lines[i].Split(new string[] { ":" }, StringSplitOptions.None);
                    dependencies.Add(dependency[1].Trim());
                }
            }

            if (null != bundle)
            {
                bundle.dependency = dependencies.ToArray();
                bundles.Add(bundle);
            }
        }
        return bundles;
    }

    public static bool SetCRC(VersionBundle bundle, string dirPath)
    {
        string path = dirPath + bundle.id + MANIFEST_FILE_SUF;
        uint crc = ReadCRC(path);
        if (crc > 0)
        {
            bundle.crc = crc;
            return true;
        }
        return false;
    }

    /// <summary> 根据manifest文件读取crc值 </summary>
    /// <returns> crc值 </returns>
    public static uint ReadCRC(string manifestFilePath)
    {
        try
        {
            if (File.Exists(manifestFilePath))
            {
                string txtFilePath = manifestFilePath;
                StreamReader sr = new StreamReader(txtFilePath);
                string content = sr.ReadToEnd();
                sr.Close();
                string[] lines = content.Split(new string[] { "\n" }, StringSplitOptions.None);
                if (null != lines)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains("CRC"))
                        {
                            string[] crc = lines[i].Split(new string[] { ":" }, StringSplitOptions.None);
                            return uint.Parse(crc[1].Trim());
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("目标路径没有该资源： " + manifestFilePath);
            }
        }
        catch(Exception e)
        {
            Debug.LogError("读取CRC错误： " + manifestFilePath + "\n" + e.Message);
        }
        return 0;
    }

    /// <summary> 获取指定文件描述 </summary>
    /// <returns> 返回文件总大小 </returns>
    private static int _GetAllFileDesc(List<string> allFiles, Dictionary<int, ZipFileDesc> fileDic)
    {
        if (null == allFiles || allFiles.Count < 1)
        {
            Debug.LogError("文件数不能为空");
            return -1;
        }

        if (null == fileDic)
        {
            fileDic = new Dictionary<int,ZipFileDesc>();
        }
        fileDic.Clear();

        Debug.Log("文件数量 : " + allFiles.Count);
        FileInfo file = null;
        int id = 0;
        string path = null;
        FileStream fs = null;
        int totalSize = 0;
#if UNITY_EDITOR
        ZipCodeProgress progress = new ZipCodeProgress((a, b) => {
            float v = (float)b / a;
            if (v >= 1)
            {
                EditorUtility.ClearProgressBar();
            }
            else
            {
                EditorUtility.DisplayProgressBar("读取文件", "", v);
            }
        });
#endif

        for (int i = 0; i < allFiles.Count; i++)
        {
            path = allFiles[i];
            file = new FileInfo(path);
            ZipFileDesc desc = new ZipFileDesc();
            desc.id = id;
            desc.size = (int)file.Length;
            desc.name = Path.GetFileName(path);
            desc.nameLength = new System.Text.UTF8Encoding().GetBytes(desc.name).Length;
            fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            if (null == fs)
            {
                Debug.LogError("读取文件失败： " + path);
                return -1;
            }

            byte[] fileData = new byte[desc.size];
            fs.Read(fileData, 0, desc.size);
            desc.data = fileData;
            fs.Close();
            fileDic.Add(desc.id, desc);
            id++;
            totalSize += desc.size;
#if UNITY_EDITOR
            progress.SetProgress(allFiles.Count, i + 1);
#endif
        }
        return totalSize;
    }

    /// <summary> 打包文件到zip包 </summary>
    public static void PackFiles(List<string> allFiles, string zipFilePath, ZipCodeProgress progress, ZipCodeProgress lzmaProgress)
    {
        if (null == allFiles || allFiles.Count < 1)
        {
            Debug.LogError("文件数不能为空");
            return;
        }

        Dictionary<int, ZipFileDesc> fileDic = new Dictionary<int, ZipFileDesc>();
        int totalSize = _GetAllFileDesc(allFiles, fileDic);
        if (totalSize < 0)
        {
            return;
        }

        Debug.Log("文件总大小 : " + totalSize);

        /**  更新文件在UPK中的起始点  **/
        int firstfilestartpos = 0 + 4;
        for (int index = 0; index < fileDic.Count; index++)
        {
            firstfilestartpos += 4 + 4 + 4 + 4 + fileDic[index].nameLength;
        }

        int startPos = 0;
        for (int index = 0; index < fileDic.Count; index++)
        {
            if (index == 0)
            {
                startPos = firstfilestartpos;
            }
            else
            {
                startPos = fileDic[index - 1].startIndex + fileDic[index - 1].size;//上一个文件的开始+文件大小;
            }

            fileDic[index].startIndex = startPos;
        }

        //Stream fileStream = new MemoryStream();
        /**  写文件  **/
        FileStream fileStream = new FileStream(zipFilePath, FileMode.Create);

#if UNITY_EDITOR
        ZipCodeProgress fileProgress = new ZipCodeProgress((a, b) =>
        {
            float v = (float)b / a;
            if (v >= 1)
            {
                EditorUtility.ClearProgressBar();
            }
            else
            {
                EditorUtility.DisplayProgressBar("写入文件描述", "", v);
            }
        });
#endif

        /**  文件总数量  **/
        byte[] totaliddata = System.BitConverter.GetBytes(fileDic.Count);
        fileStream.Write(totaliddata, 0, totaliddata.Length);

        int lastIndex = fileDic.Count - 1;
        for (int index = 0; index <= lastIndex; index++)
        {
            /** 写入ID **/
            byte[] iddata = System.BitConverter.GetBytes(fileDic[index].id);
            fileStream.Write(iddata, 0, iddata.Length);

            /**  写入startIndex  **/
            byte[] startposdata = System.BitConverter.GetBytes(fileDic[index].startIndex);
            fileStream.Write(startposdata, 0, startposdata.Length);

            /**  写入size  **/
            byte[] sizedata = System.BitConverter.GetBytes(fileDic[index].size);
            fileStream.Write(sizedata, 0, sizedata.Length);

            /**  写入nameLength  **/
            byte[] pathLengthdata = System.BitConverter.GetBytes(fileDic[index].nameLength);
            fileStream.Write(pathLengthdata, 0, pathLengthdata.Length);

            /**  写入name  **/
            byte[] mypathdata = new System.Text.UTF8Encoding().GetBytes(fileDic[index].name);
            fileStream.Write(mypathdata, 0, mypathdata.Length);

#if UNITY_EDITOR
            if (null != fileProgress)
            {
                fileProgress.SetProgress(lastIndex, index);
            }
#endif
        }

        /**  写入文件数据  **/
        int totalprocessSize = 0;
        foreach (var infopair in fileDic)
        {
            ZipFileDesc info = infopair.Value;
            int size = info.size;
            byte[] tmpdata = null;
            int processSize = 0;
            while (processSize < size)
            {
                if (size - processSize < 4096)
                {
                    tmpdata = new byte[size - processSize];
                }
                else
                {
                    tmpdata = new byte[4096];
                }
                fileStream.Write(info.data, processSize, tmpdata.Length);

                processSize += tmpdata.Length;
                totalprocessSize += tmpdata.Length;

                progress.SetProgress(totalSize, totalprocessSize);
            }
        }
        fileStream.Flush();
        fileStream.Close();
        //// 重置读取位置
        //fileStream.Position = 0;

        //// 压缩并写入文件
        //Encoder coder = new Encoder();
        //FileStream output = new FileStream(zipFilePath, FileMode.Create);
        //coder.WriteCoderProperties(output);
        //byte[] data = BitConverter.GetBytes(fileStream.Length);
        //output.Write(data, 0, data.Length);
        //coder.Code(fileStream, output, fileStream.Length, -1, lzmaProgress);
        //output.Flush();
        //output.Close();
        //fileStream.Close();
        //fileDic.Clear();
    }

    /// <summary> 打包一个文件夹 </summary>
    public static void PackFolder(string dir, string zipfilepath, ZipCodeProgress progress, ZipCodeProgress lzmaProgress)
    {
        List<string> allFiles = new List<string>();
        PathUtils.GetAllFiles(dir, allFiles);
        PackFiles(allFiles, zipfilepath, progress, lzmaProgress);
    }

    /// <summary> 解压zip文件 </summary>
    public static void UnPack(string filePath, string dir, ZipCodeProgress progress, ZipCodeProgress lzmaProgress)
    {
        //Decoder coder = new Decoder();
        //FileStream input = new FileStream(filePath, FileMode.Open);
        //Stream upkFilestream = new MemoryStream();

        //byte[] properties = new byte[5];
        //input.Read(properties, 0, 5);

        //byte[] fileLengthBytes = new byte[8];
        //input.Read(fileLengthBytes, 0, 8);
        //long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

        //coder.SetDecoderProperties(properties);
        //coder.Code(input, upkFilestream, input.Length, fileLength, lzmaProgress);
        //upkFilestream.Flush();
        //input.Close();
        //upkFilestream.Seek(0, SeekOrigin.Begin);

        FileStream upkFilestream = new FileStream(filePath, FileMode.Open);

        int totalSize = 0;
        int offset = 0;
        //读取文件数量;
        byte[] totaliddata = new byte[4];
        upkFilestream.Read(totaliddata, 0, 4);
        int filecount = BitConverter.ToInt32(totaliddata, 0);
        offset += 4;
        //Debug.Log("filecount=" + filecount);

        Dictionary<int, ZipFileDesc> fileDic = new Dictionary<int, ZipFileDesc>();
        //读取所有文件信息;
        for (int index = 0; index < filecount; index++)
        {
            //读取id;
            byte[] iddata = new byte[4];
            upkFilestream.Seek(offset, SeekOrigin.Begin);
            upkFilestream.Read(iddata, 0, 4);
            int id = BitConverter.ToInt32(iddata, 0);
            offset += 4;

            //读取startIndex;
            byte[] startposdata = new byte[4];
            upkFilestream.Seek(offset, SeekOrigin.Begin);
            upkFilestream.Read(startposdata, 0, 4);
            int startIndex = BitConverter.ToInt32(startposdata, 0);
            offset += 4;

            //读取size;
            byte[] sizedata = new byte[4];
            upkFilestream.Seek(offset, SeekOrigin.Begin);
            upkFilestream.Read(sizedata, 0, 4);
            int size = BitConverter.ToInt32(sizedata, 0);
            offset += 4;

            //读取nameLength;
            byte[] pathLengthdata = new byte[4];
            upkFilestream.Seek(offset, SeekOrigin.Begin);
            upkFilestream.Read(pathLengthdata, 0, 4);
            int nameLength = BitConverter.ToInt32(pathLengthdata, 0);
            offset += 4;

            //读取name;
            byte[] name = new byte[nameLength];
            upkFilestream.Seek(offset, SeekOrigin.Begin);
            upkFilestream.Read(name, 0, nameLength);
            string path = new System.Text.UTF8Encoding().GetString(name);
            offset += nameLength;


            //添加到Dic;
            ZipFileDesc info = new ZipFileDesc();
            info.id = id;
            info.size = size;
            info.nameLength = nameLength;
            info.name = path;
            info.startIndex = startIndex;
            fileDic.Add(id, info);
            totalSize += size;
            // Debug.Log("id=" + id + " startPos=" + startIndex + " size=" + size + " pathLength=" + nameLength + " path=" + path);
        }

        //释放文件;
        int totalprocesssize = 0;
        progress.SetProgress((long)totalSize, (long)totalprocesssize);
        foreach (var infopair in fileDic)
        {
            ZipFileDesc info = infopair.Value;

            int startPos = info.startIndex;
            int size = info.size;
            string path = info.name;

            //创建文件;
            string filepath = dir + path;
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            FileStream fileStream = new FileStream(filepath, FileMode.Create);

            byte[] tmpfiledata;
            int processSize = 0;
            while (processSize < size)
            {
                if (size - processSize < 4096)
                {
                    tmpfiledata = new byte[size - processSize];
                }
                else
                {
                    tmpfiledata = new byte[4096];
                }

                //读取;
                upkFilestream.Seek(startPos + processSize, SeekOrigin.Begin);
                upkFilestream.Read(tmpfiledata, 0, tmpfiledata.Length);

                //写入;
                fileStream.Write(tmpfiledata, 0, tmpfiledata.Length);

                processSize += tmpfiledata.Length;
                totalprocesssize += tmpfiledata.Length;

                progress.SetProgress((long)totalSize, (long)totalprocesssize);
            }
            fileStream.Flush();
            fileStream.Close();
        }

        upkFilestream.Flush();
        upkFilestream.Close();
    }
}
