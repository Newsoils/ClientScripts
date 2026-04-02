using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;


public enum Character_Type
{
    /// <summary>
    /// 主人物
    /// </summary>
    Main_Character,
    /// <summary>
    /// 商店换装人物
    /// </summary>
    Shopping_Character,
    /// <summary>
    /// 换装人物
    /// </summary>
    Target_Character
}

public class Character_Cloth_Manager : SingletonMono<Character_Cloth_Manager> 
{
    public List<cloth_info> clothInfo_db = new List<cloth_info>();
    public Dictionary<string, cloth_info> clothNameDic = new Dictionary<string, cloth_info>();

    private Cloth_Default_Config default_Config = new Cloth_Default_Config();

    /// <summary>
    /// 主人物信息
    /// </summary>
    public Character_Clothes_Info mainClothes;

    /// <summary>
    /// 换装的人物信息
    /// </summary>
    public Character_Clothes_Info targetCloth;

    /// <summary>
    /// 购物的人物信息
    /// </summary>
    public Character_Clothes_Info shoppingCloth;


    public Dictionary<string,List< string>> cloth_slot_Dic = new Dictionary<string, List<string>>()
    {
        {"top", new List<string>{ "head" } },
        {"upper",new List<string>{"body" }},
        {"lower",new List<string>{"leg" }},
        {"bottom",new List<string>{"shoes" }},
        {"dress",new List<string>{"body" , "leg" } },
        {"attachment",new List<string>{"" } },
    };

    // Start is called before the first frame update
    void Start()
    {
        clothInfo_db = JsonData_Manager.Load_ClothInfo_JsonData();
        clothNameDic = clothInfo_db.ToDictionary(x => x.cloth_name);
        default_Config = new Cloth_Default_Config();
        mainClothes = new Character_Clothes_Info();

        targetCloth = new Character_Clothes_Info();
        shoppingCloth = new Character_Clothes_Info();

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    /// <summary>
    /// 把换装人物的衣服信息设置到主角人物身上
    /// </summary>
    public void Sync_Main_Character_Cloth_Data()
    {
        mainClothes.Refresh_Cloth_Data(targetCloth);
    }

    public void Sync_Main_Character_Cloth()
    {
        mainClothes.Refresh_Cloth_Data(targetCloth);
        UpdateView(Character_Type.Main_Character);
        EvtDsp.TriggerEvt(EvtNames.ReSetMainCharacterRenderer);
    }

    //public void Set_Main_Character_Cloth_Data(Character_Clothes_Info cloth_Info)
    //{
    //    mainClothes.Refresh_Cloth_Data(cloth_Info);
    //}

    //public void Set_Main_Character_Cloth(Character_Clothes_Info cloth_Info)
    //{
    //    targetCloth.Refresh_Cloth_Data(cloth_Info);
    //    UpdateView(Character_Type.Main_Character);
    //}

    public void Init_ShoppongCharacter_Cloth()
    {
        shoppingCloth = new Character_Clothes_Info(mainClothes);
        UpdateView(Character_Type.Shopping_Character);
    }

    public void Init_Target_Character_Cloth()
    {
        targetCloth = new Character_Clothes_Info(mainClothes);
        UpdateView(Character_Type.Target_Character);
    }

    public bool IsUnsaved()
    {
        return mainClothes.SameAs(targetCloth);
    }

    public void Set_Target_Cloth_Data(Character_Clothes_Info cloth_Info)
    {
        targetCloth.Refresh_Cloth_Data(cloth_Info);
    }
    public void Set_Target_Cloth(Character_Clothes_Info cloth_Info)
    {
        targetCloth.Refresh_Cloth_Data(cloth_Info);
        UpdateView(Character_Type.Target_Character);
    }


    public void Add_Cloth(Character_Type type,string clothName)
    {
        if (string.IsNullOrEmpty(clothName)) return;
        switch (type)
        {
            case Character_Type.Main_Character:
                mainClothes.Add_Cloth(clothName, clothInfo_db, default_Config);
                UpdateAreaView(type, clothName);
                break;
            case Character_Type.Shopping_Character:
                shoppingCloth.Add_Cloth(clothName, clothInfo_db, default_Config);
                UpdateAreaView(type, clothName);
                break;
            case Character_Type.Target_Character:
                targetCloth.Add_Cloth(clothName, clothInfo_db, default_Config);
                UpdateAreaView(type, clothName);
                break;
        }

    }

    public void Remove_Cloth(Character_Type type, string clothName)
    {
        if (string.IsNullOrEmpty(clothName)) return;

        switch (type)
        {
            case Character_Type.Main_Character:
                mainClothes.Remove_cloth(clothName, default_Config);
                UpdateView(type);
                break;
            case Character_Type.Shopping_Character:
                shoppingCloth.Remove_cloth(clothName, default_Config);
                UpdateView(type);
                break;
            case Character_Type.Target_Character:
                targetCloth.Remove_cloth(clothName, default_Config);
                UpdateView(type);
                break;
        }

    }

    public void InitMainCharacter_Cloth_Default()
    {
        if (mainClothes == null)
        {
            Log.Error(Name + "Character clothes info or cloth info database is not set.");
            return;
        }
        mainClothes._top_name = default_Config.default_top;
        mainClothes._upper_name = default_Config.default_upper;
        mainClothes._lower_name = default_Config.default_lower;
        mainClothes._bottom_name = default_Config.default_buttom;
        mainClothes._dress_name = default_Config.default_dress;
        mainClothes._attachment_name = default_Config.default_attatchment;
        UpdateView(Character_Type.Main_Character);
    }

    //public void Try_auto_change_cloth()
    //{
    //    mainClothes.Try_auto_change_cloth(Global_Inventory_Manager.Instance._itemDB_SO._inventory,clothInfo_db, default_Config);
    //}

    public void Set_auto_change_cloth(bool is_auto_change_cloth)
    {
        mainClothes.is_auto_change_cloth = is_auto_change_cloth;
    }

    public void Upload_Main_Characer_Cloth_Info()
    {
        if (Global_Game_Manager._instance == null) return;

        mainClothes =GF_SP.DeserializeObject<Character_Clothes_Info>(GF_SP.SerializeObject(mainClothes));
        Global_Game_Manager._instance.upload_main_character_cloth();
    }

    

    public void Try_refresh_main_character_cloth()
    {

        ////？为什么自动换装的逻辑写在这里
        //if (mainClothes.Try_auto_change_cloth(_inventory,default_Config, DateTime.Now) == true)
        //{
        //    Global_Game_Manager.Instance.upload_main_character_cloth();
        //    targetCloth.Refresh_Cloth_Data(mainClothes);

        //    //自动换装，两边的衣服信息都要更新
         
        //    Debug.Log("try_refresh_main_character_cloth_OK_@_Update_to_Server");
        //}

        UpdateView(Character_Type.Target_Character);
        UpdateView(Character_Type.Main_Character);
        UpdateView(Character_Type.Shopping_Character);
    }

    #region View_Update
    /// <summary>
    /// 更新全部视觉
    /// </summary>
    /// <param name="isMainCharacter"></param>
    private void UpdateView(Character_Type character_Type)
    {
        switch(character_Type)
        {
            case Character_Type.Main_Character:
                EvtDsp.TriggerEvt<List<cloth_info>>(EvtNames.On_Main_Character_All_Cloth_Changed, mainClothes.Get_All_Cloth(clothInfo_db));
                break;
            case Character_Type.Target_Character:
                EvtDsp.TriggerEvt<List<cloth_info>>(EvtNames.On_Target_Character_All_Cloth_Changed, targetCloth.Get_All_Cloth(clothInfo_db));
                break;
            case Character_Type.Shopping_Character:
                EvtDsp.TriggerEvt<List<cloth_info>>(EvtNames.On_Shopping_Character_All_Cloth_Changed, shoppingCloth.Get_All_Cloth(clothInfo_db));
                break;
        }
      
    }

    private void UpdateAreaView(Character_Type character_Type,string clothName)
    {

        switch (character_Type)
        {
            case Character_Type.Main_Character:
                EvtDsp.TriggerEvt<cloth_info>(EvtNames.On_Main_Character_Single_Cloth_Changed, clothInfo_db.Find(x => x.cloth_name == clothName));
                break;
            case Character_Type.Target_Character:
                EvtDsp.TriggerEvt<cloth_info>(EvtNames.On_Target_Character_Single_Cloth_Changed, clothInfo_db.Find(x => x.cloth_name == clothName));
                break;
            case Character_Type.Shopping_Character:
                EvtDsp.TriggerEvt<cloth_info>(EvtNames.On_Shopping_Character_Single_Cloth_Changed, clothInfo_db.Find(x => x.cloth_name == clothName));
                break;
        }
     
    }

    #endregion
}
