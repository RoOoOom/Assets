public class GameConfig
{
    //是否使用本地资源
    public static bool useLocalRes = false;
    //是否显示帧率
    public static bool showFPS = false;
    //本地初始版本文件名称
    public const string LOCAL_VERSION_FILE = "version.txt";
    //本地Bundle版本文件名称
    public const string LOCAL_CONFIG_FILE = "config.txt";
    //本地HttpBundle版本文件名称
    public const string LOCAL_HTTP_CONFIG_FILE = "http_config.txt";
    //bundle的hascode文件名称
    public const string LOCAL_BUNDLE_HASH = "bundle_hash.json";
    //下载文件信息名称
    public const string LOCAL_DOWNLOAD_INFO_FILE = "http_down_info.txt";
    //http整包文件名称
    public const string HTTP_ZIP_FILE = "http_zip.zip";
    //至少保证传输的协议数量
    public const int MustSendCount = 5;
    //网络ping值提示
    public const int NetHintLimit = 1000;
    //初始语言
    public static Language language = Language.ZH_CN;
    //是否显示log-console
    public static bool showLogConsole = false;

    public static string HOST_RES()
    {
        return ServerInfoConfig.GetResURL() + PathUtils.osDir + "/";
    }

    public static string HOST_RES_ZIP()
    {
        return ServerInfoConfig.GetResZipURL() + PathUtils.osDir + "/";
    }
}
