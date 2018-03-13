public class VersionConfig
{
    public string version;
    public string language;
    public string os;
    public OtherData[] other;//存放特殊内容，比如必须进行整包更新

    public int versionValue { get { if (_versionValue < 1) _versionValue = version == null ? 0 : int.Parse(version.Replace(".", "")); return _versionValue; } }
    private int _versionValue = 0;
}

public class OtherData
{
    public string key;
    public string value;
}

public class VersionBundleConfig
{
    public string version;
    public VersionBundle[] bundles;

    public int versionValue { get { if (_versionValue < 1) _versionValue = version == null ? 0 : int.Parse(version.Replace(".", "")); return _versionValue; } }
    private int _versionValue = 0;
}

public class VersionBundle
{
    public string id;
    public string version;
    public uint crc;
    public string[] dependency;
    public int versionValue { get { if (_versionValue < 1) _versionValue = version == null ? 0 : int.Parse(version.Replace(".", "")); return _versionValue; } }
    private int _versionValue = 0;

    public VersionBundle Clone()
    {
        VersionBundle vb = new VersionBundle();
        vb.id = this.id;
        vb.version = this.version;
        vb.crc = this.crc;
        if (null != this.dependency)
        {
            vb.dependency = new string[this.dependency.Length];
            for (int i = 0; i < this.dependency.Length; i++)
            {
                vb.dependency[i] = this.dependency[i];
            }
        }

        return vb;
    }
}

public class VersionBundleHashConfig
{
    public string version;
    public VersionBundleHash[] bundles;

    public int versionValue { get { if (_versionValue < 1) _versionValue = version == null ? 0 : int.Parse(version.Replace(".", "")); return _versionValue; } }
    private int _versionValue = 0;
}

public class VersionBundleHash
{
    public string id;
    public string hashValue;
}

public class DownloadFileInfo
{
    public long totalSize;
    public string[] ids;
}
