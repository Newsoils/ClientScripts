
public class GameCurrency
{
    public int currency_id;
    public string currency_name;
    /// <summary>
    /// 对应GameItem表中的ID
    /// </summary>
    public int item_id;
}

public class GameCurrencyMessage
{
    public int currency_id;
    public int amount;
    public string reason;
}