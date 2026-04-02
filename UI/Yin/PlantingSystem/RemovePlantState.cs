//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using CLIP.Project_Mouse.UI;
//using UnityEngine;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class RemovePlantState : IState
//            {
//                public void EnterState(PlantingInteractManager manager)
//                {
//                    Planting_System_Manager._instance.on_enter_remove_plant();
//                    PromptMessage.Instance.dontRemindDict[2] = false;
//                    PlantingInteractManager.Instance.SetCanRotatePot(false);
//                }

//                public void ExitState(PlantingInteractManager manager)
//                {
//                    Planting_System_Manager._instance.on_exit_remove_plant();
//                    PlantingInteractManager.Instance.SetCanRotatePot(true);
//                }

//                public void UpdateState(PlantingInteractManager manager)
//                {

//                }
//            }
//        }
//    }
//}

