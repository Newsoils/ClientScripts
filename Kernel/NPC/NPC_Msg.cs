using CLIP.Project_Mouse.ENUM;

/// <summary>
/// NPC送给玩家的礼物信息
/// </summary>
public class NPC_Gift_To_PLayer_Info
{
    /// <summary>
    /// npc的ID
    /// </summary>
    public int NPC_ID { get; set; }
    /// <summary>
    /// 礼物的ID（和物品表对应）
    /// </summary>
    public int gift_ID { get; set; }
}

public class Gift_To_NPC_Msg
{
    public int npc_id;
    public Item_Type item_type;
    public Gift_To_NPC_Msg(int npc_id, Item_Type item_type)
    {
        this.npc_id = npc_id;
        this.item_type = item_type;
    }
}