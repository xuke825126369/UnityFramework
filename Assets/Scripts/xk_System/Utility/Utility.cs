using System;

/// <summary>
/// 对委托的一些操作
/// </summary>
/// <typeparam name="T"></typeparam>
public static class DelegateUtility
{
    public static bool CheckFunIsExist<T>(Action<T> mEvent, Action<T> fun)
    {
        if (mEvent == null)
        {
            return false;
        }
        Delegate[] mList = mEvent.GetInvocationList();
        return Array.Exists<Delegate>(mList, (x) => x.Equals(fun));
    }
}


public static class TimeUtility
{
    /// <summary>
    /// 时间戳转为C#格式时间
    /// </summary>
    /// <param name=”timeStamp”></param>
    /// <returns></returns>
    public static DateTime GetTime(ulong timeStamp)
    {
        DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        return dtStart.AddSeconds(timeStamp);
    }

    /// <summary>
    /// DateTime时间格式转换为Unix时间戳格式
    /// </summary>
    /// <param name=”time”></param>
    /// <returns></returns>
    public static ulong GetTimeStamp(System.DateTime time)
    {
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new  DateTime(1970, 1, 1));
        return (ulong)(time - startTime).TotalSeconds;
    }
}

