using System;
/// <summary>
/// 时间工具类
/// </summary>
public class TimeUtils
{
    private const string TIME_FORMAT_1 = "MM月dd日 HH:mm";
    private const string TIME_FORMAT_2 = "MM月dd日";
    private const string TIME_FORMAT_3 = "yyyy-MM-dd";
    private const string TIME_FORMAT_4 = "yyyy.MM.dd";


    /// <summary>
    /// 获取本地时间（单位毫秒）
    /// </summary>
    /// <returns></returns>
    public static long CurLocalTimeMilliSecond()
    {
        return System.DateTime.Now.Ticks / 10000;
    }

    /// <summary>
    /// 获取本地时间（单位秒）
    /// </summary>
    /// <returns></returns>
    public static long CurLocalTimeSecond()
    {
        return CurLocalTimeMilliSecond() / 1000;
    }

    /// <summary>
    /// 格式化时间
    /// </summary>
    /// <param name="time"> 时间戳，单位：秒 </param>
    /// <param name="format"> 格式化类型 </param>
    /// <returns></returns>
    public static string TimeFormat(long time, string format = TIME_FORMAT_1)
    {
        System.DateTime dt = System.DateTime.Parse("1970-1-1 8:00:00");
		System.DateTime dt1 = dt.AddSeconds(time);
		return dt1.ToString(format);
    }

    /// <summary>
    /// 获取分秒时
    /// </summary>
    /// <param name="time"></param>
    /// <returns> 时间数组：0位，小时；1位，分钟；2位，秒 </returns>
    public static int[] GetHMS(int time)
    {
        int hour = time / 3600;
        int min = (time % 3600) / 60;
        int second = time % 60;
        return new int[3]{hour, min, second};
    }

    /// <summary>
    /// 格式化分秒时，格式：00:00:00
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static string TimeFormatHMS(int time)
    {
        int[] hms = GetHMS(time);
        return string.Format("{0:00}:{1:00}:{2:00}", hms[0], hms[1], hms[2]);
    }
}
