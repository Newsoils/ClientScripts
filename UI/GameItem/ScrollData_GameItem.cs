using CLIP.Project_Mouse;
public class ScrollData_GameItem 
{
    public int id;
    public string name;
    public int count;
    public bool isSelected;

    public Enum_RarityType rarity;

    public ScrollData_GameItem()
    {
        this.name = "";
        this.id = -1;
        this.count = 0;
    }
    public ScrollData_GameItem(string name, int id, int count, Enum_RarityType rarity)
    {
        this.name = name;
        this.id = id;
        this.count = count;
        this.rarity = rarity;
    }
}
