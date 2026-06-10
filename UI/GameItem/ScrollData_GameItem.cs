using CLIP.Project_Mouse;
public class ScrollData_GameItem 
{
    public long uid;
    public int id;
    public string name;
    public int count;
    public bool isNew;
    public bool isSelected;

    public RarityType rarity;

    public ScrollData_GameItem()
    {
        this.uid = 0;
        this.name = "";
        this.id = -1;
        this.count = 0;
        this.isNew = false;
    }
    public ScrollData_GameItem(string name, int id, int count, RarityType rarity, long uid = 0, bool isNew = false)
    {
        this.uid = uid;
        this.name = name;
        this.id = id;
        this.count = count;
        this.rarity = rarity;
        this.isNew = isNew;
    }
}
