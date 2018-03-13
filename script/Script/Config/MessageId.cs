/**
 *  消息定义
 *  网络消息以 net_ 打头
 *  本地消息以 msg_ 打头
 */
public class MessageId
{
    /******************** 以下是网络消息 ********************/
    public const int NET_MESSAGE_BEGIN = 10000;
    public const int GM_CMD = 61002;		//GM命令

    /******************** 以下是本地消息 ********************/
    public const int MSG_DEFIND_GAME = 10000000;
    public const int DOWNLOAD_VERSION = 10000001;          // 下载版本文件
    public const int DOWNLOADED_VERSION = 10000002;        // 下载版本文件结束
    public const int DOWNLOAD_VERSION_ERROR = 10000003;    // 下载版本文件错误

    public const int DOWNLOAD_CONFIG = 10000010;                // 下载配置文件
    public const int DOWNLOADED_CONFIG = 10000011;              // 下载配置文件结束
    public const int DOWNLOAD_CONFIG_ERROR = 10000012;          // 下载配置文件错误
    public const int DOWNLOAD_CONFIG_PROGRESS = 10000013;       // 配置文件下载进度

    public const int DECOMPRESS_CONFIG = 10000020;              // 解压配置文件
    public const int DECOMPRESSED_CONFIG = 10000021;            // 解压配置文件结束
    public const int DECOMPRESS_CONFIG_PGOGRESS = 10000022;     // 配置文件解压进度

    public const int LOAD_LOCAL_CONFIG = 10000030;              // 加载本地config文件
    public const int LOADED_LOCAL_CONFIG = 10000031;            // 加载本地config文件完成
    public const int LOAD_LOCAL_CONFIG_ERROR = 10000032;        // 加载本地config文件错误

    public const int TIPS = 10000040;                       // 显示tips

    public const int DOWNLOAD_FILE = 10000050;              // 下载文件

    public const int REGISTER_LUA = 10000060;               // 注册lua文件
    public const int LUA_REGISTERED = 10000061;             // lua文件加载完毕
}