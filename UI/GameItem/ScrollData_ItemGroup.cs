using CLIP.Project_Mouse;

public class ScrollData_ItemGroup
{
    public int groupId;
    public string name;
    public int obtainCount;
    public int totalCount;
    public string res_url;


    public ScrollData_ItemGroup(ItemGroupData data)
    {
        groupId = data.group_id;
        name = data.itemGroupInfo != null ? data.itemGroupInfo.group_name : string.Empty;
        obtainCount = data.obtainCount;
        totalCount = data.totalCount;
        res_url = data.itemGroupInfo.res_url;
    }
}
