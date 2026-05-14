using System;
using System.Collections.Generic;

[System.Serializable]
public class Player_DailyRuntimeData
{
    public DateTime _last_Gift_ResetTime;    // 上次送礼次数重置时间
    public int _gifts_Given_Daily_Count;           // 今天已送礼次数（最大2）
    public  List<int> _giftedNPCs_Today;  // 今天已送过礼的伙伴ID

    public Player_DailyRuntimeData(DateTime lastResetTime,int gifts_Given_Daily_Count,List<int> giftedNPCs_Today)
    {
        _last_Gift_ResetTime = lastResetTime;
        _gifts_Given_Daily_Count =  gifts_Given_Daily_Count;
        _giftedNPCs_Today = giftedNPCs_Today;
    }

    public Player_DailyRuntimeData()
    {
        _last_Gift_ResetTime = DateTime.MinValue;
        _gifts_Given_Daily_Count = 0;
        _giftedNPCs_Today = new List<int>();
    }
}
