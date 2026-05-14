using System.Collections.Generic;

/// <summary>
/// 手持物配置（与 <c>project_mouse_tb_handheld_info</c> JSON 及 Resources 内 SKM 命名一致）。
/// </summary>
public class HandHeldInfo
{
    public string itemName;
    public string suitName;
    public List<string> slotsOccupied;
    public string colorKey;
}
