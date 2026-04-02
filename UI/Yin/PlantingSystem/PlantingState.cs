//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using UnityEngine;
//using CLIP.Project_Mouse.UI;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class PlantingState : IState
//            {
//                public void EnterState(PlantingInteractManager manager)
//                {
//                    PlantingInteractManager.Instance.CloseGhost();
//                    //PlantingInteractManager.Instance.uI_Control_Warehouse_Panel.DeselectItem();
//                    Planting_System_Manager._instance.on_deselect_pot();
//                }

//                public void ExitState(PlantingInteractManager manager)
//                {
//                    PlantingInteractManager.Instance.OnExitPlacePot();
//                }

//                public void UpdateState(PlantingInteractManager manager)
//                {
//                    //Planting_System_Manager.Instance.canPotRotate = false;
//                }
//            }
//        }
//    }
//}

