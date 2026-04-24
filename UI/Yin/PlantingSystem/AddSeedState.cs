//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using UnityEngine;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class AddSeedState : IState
//            {
//                public void EnterState(PlantingInteractManager manager)
//                {
//                    Planting_System_Manager.Instance.can_edit_pot = false;
//                    Planting_System_Manager.Instance.on_deselect_pot();
//                    PlantingInteractManager.Instance.SetCanRotatePot(false);
//                }

//                public void ExitState(PlantingInteractManager manager)
//                {
//                    Planting_System_Manager.Instance.can_edit_pot = true;
//                    PlantingInteractManager.Instance.SetCanRotatePot(true);
//                }

//                public void UpdateState(PlantingInteractManager manager)
//                {

//                }
//            }

//        }
//    }
//}
