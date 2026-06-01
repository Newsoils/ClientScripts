using System.Collections;
using System.Collections.Generic;

public class DispatchBagInfo
{
    public string foodName;
    public string snackName;
    public string tapeName;
    public bool isPacked;

    /// <summary>三格中任意一格有物即视为已打包（列表态）。</summary>
    public bool HasAnyFilledSlot()
    {
        return !string.IsNullOrEmpty(foodName)
            || !string.IsNullOrEmpty(snackName)
            || !string.IsNullOrEmpty(tapeName);
    }

    public void SyncPackedFlag()
    {
        isPacked = HasAnyFilledSlot();
    }
}
