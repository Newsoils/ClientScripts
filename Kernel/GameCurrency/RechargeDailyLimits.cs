using System.Collections.Generic;

public class RechargeDailyLimits
{
    public string date;
    public Dictionary<int, int> counts = new Dictionary<int, int>();
}

public class RechargeWithLimitResponse
{
    public string currencyData;
    public RechargeDailyLimits rechargeLimits;
}
