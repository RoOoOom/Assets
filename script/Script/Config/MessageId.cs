/**
 *  ��Ϣ����
 *  ������Ϣ�� net_ ��ͷ
 *  ������Ϣ�� msg_ ��ͷ
 */
public class MessageId
{
    /******************** ������������Ϣ ********************/
    public const int NET_MESSAGE_BEGIN = 10000;
    public const int GM_CMD = 61002;		//GM����

    /******************** �����Ǳ�����Ϣ ********************/
    public const int MSG_DEFIND_GAME = 10000000;
    public const int DOWNLOAD_VERSION = 10000001;          // ���ذ汾�ļ�
    public const int DOWNLOADED_VERSION = 10000002;        // ���ذ汾�ļ�����
    public const int DOWNLOAD_VERSION_ERROR = 10000003;    // ���ذ汾�ļ�����

    public const int DOWNLOAD_CONFIG = 10000010;                // ���������ļ�
    public const int DOWNLOADED_CONFIG = 10000011;              // ���������ļ�����
    public const int DOWNLOAD_CONFIG_ERROR = 10000012;          // ���������ļ�����
    public const int DOWNLOAD_CONFIG_PROGRESS = 10000013;       // �����ļ����ؽ���

    public const int DECOMPRESS_CONFIG = 10000020;              // ��ѹ�����ļ�
    public const int DECOMPRESSED_CONFIG = 10000021;            // ��ѹ�����ļ�����
    public const int DECOMPRESS_CONFIG_PGOGRESS = 10000022;     // �����ļ���ѹ����

    public const int LOAD_LOCAL_CONFIG = 10000030;              // ���ر���config�ļ�
    public const int LOADED_LOCAL_CONFIG = 10000031;            // ���ر���config�ļ����
    public const int LOAD_LOCAL_CONFIG_ERROR = 10000032;        // ���ر���config�ļ�����

    public const int TIPS = 10000040;                       // ��ʾtips

    public const int DOWNLOAD_FILE = 10000050;              // �����ļ�

    public const int REGISTER_LUA = 10000060;               // ע��lua�ļ�
    public const int LUA_REGISTERED = 10000061;             // lua�ļ��������
}