using System.Collections.Generic;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Game_Play_System
        {
            public class CharacterClothesController : MonoBehaviour
            {
                public Character_Type type;
                public CharacterClothes clothes;

                // 辅助方法：提取颜色 Key
                private string GetColorKey(string color) => color switch
                {
                    "紫色" => "purple",
                    "绿色" => "green",
                    "蓝色" => "blue",
                    "粉色" => "pink",
                    "黄色" => "yellow",
                    _ => ""
                };

                private void Start()
                {
                    switch(type)
                    {
                        case Character_Type.Main_Character:
                            EvtDsp.AddEvt<List<cloth_info>>(EvtNames.On_Main_Character_All_Cloth_Changed, ChangeClothes);
                            EvtDsp.AddEvt<cloth_info>(EvtNames.On_Main_Character_Single_Cloth_Changed, ChangeSingleClothes);
                            break;
                        case Character_Type.Target_Character:
                            EvtDsp.AddEvt<List<cloth_info>>(EvtNames.On_Target_Character_All_Cloth_Changed, ChangeClothes);
                            EvtDsp.AddEvt<cloth_info>(EvtNames.On_Target_Character_Single_Cloth_Changed, ChangeSingleClothes);
                            break;
                        case Character_Type.Shopping_Character:
                            EvtDsp.AddEvt<List<cloth_info>>(EvtNames.On_Shopping_Character_All_Cloth_Changed, ChangeClothes);
                            EvtDsp.AddEvt<cloth_info>(EvtNames.On_Shopping_Character_Single_Cloth_Changed, ChangeSingleClothes);
                            break;
                    }

                }

                private void OnDestroy()
                {
                    switch (type)
                    {
                        case Character_Type.Main_Character:
                            EvtDsp.RemoveEvt<List<cloth_info>>(EvtNames.On_Main_Character_All_Cloth_Changed, ChangeClothes);
                            EvtDsp.RemoveEvt<cloth_info>(EvtNames.On_Main_Character_Single_Cloth_Changed, ChangeSingleClothes);
                            break;
                        case Character_Type.Target_Character:
                            EvtDsp.RemoveEvt<List<cloth_info>>(EvtNames.On_Target_Character_All_Cloth_Changed, ChangeClothes);
                            EvtDsp.RemoveEvt<cloth_info>(EvtNames.On_Target_Character_Single_Cloth_Changed, ChangeSingleClothes);
                            break;
                        case Character_Type.Shopping_Character:
                            EvtDsp.RemoveEvt<List<cloth_info>>(EvtNames.On_Shopping_Character_All_Cloth_Changed, ChangeClothes);
                            EvtDsp.RemoveEvt<cloth_info>(EvtNames.On_Shopping_Character_Single_Cloth_Changed, ChangeSingleClothes);
                            break;
                    }
                }
                public void ChangeClothes(List<cloth_info> infos)
                {
                    foreach(var info in infos)
                    {
                        ChangeSingleClothes(info);
                    }
                }

                public void ChangeSingleClothes(cloth_info info)
                {
                    _ = ChangeSingleClothesAsync(info);
                }

                private async Task ChangeSingleClothesAsync(cloth_info info)
                {
                    var color = info.cloth_color;
                    Material mat = null;
                    if(!( string.IsNullOrEmpty(color) || color == "原色"))
                    {
                        var gameAsset = GameAssets.Instance;
                        var colorkey = GetColorKey(color);
                        
                        if (!string.IsNullOrEmpty( colorkey))
                        {
                           mat = await gameAsset.GetAssetByKeyword<Material>(info.parent_cloth_name.ToUpper(), colorkey.ToUpper());
                        }
                    }
                  
                    foreach (var part in info.slots_occupied)
                    {
                        clothes.ChangeClothes(part, info.parent_cloth_name,mat);
                    }
                    IndoorMainCharacter._instance.SwitchRendererState(null);
                }
            }
        }
    }
}
