//using System.Collections;
//using System.Collections.Generic;
//using Lean.Touch;
//using UnityEngine;
//using CLIP.Project_Mouse.UI;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class RemoveWeedState : IState
//            {
//                private LeanFinger activeFinger;
//                public void EnterState(PlantingInteractManager manager)
//                {
//                    manager.leanPitch.enabled = false;
//                    manager.leanMultiUpdate.enabled = false;
//                    PlantingInteractManager.Instance.SetCanRotatePot(false);
//                }

//                public void ExitState(PlantingInteractManager manager)
//                {
//                    manager.removeWeedParticle.Stop();
//                    manager.leanPitch.enabled = true;
//                    manager.leanMultiUpdate.enabled = true;
//                    PlantingInteractManager.Instance.SetCanRotatePot(true);
//                }

//                public void UpdateState(PlantingInteractManager manager)
//                {
//#if UNITY_EDITOR || UNITY_STANDALONE
//                    if (LeanTouch.Fingers.Count >= 2)
//                    {
//                        Debug.Log("检测到手指数量: " + LeanTouch.Fingers.Count);
//                        var finger = LeanTouch.Fingers[1];

//                        // 手指按下
//                        if (finger.Down && activeFinger == null)
//                        {
//                            activeFinger = finger;
//                            if (manager.removeWeedParticle != null && !manager.removeWeedParticle.isPlaying)
//                            {
//                                manager.SetParticleToFinger(finger, manager.removeWeedParticle);
//                                manager.removeWeedParticle.Play();
//                            }
//                        }
//                        // 手指移动
//                        else if (finger == activeFinger && finger.Set)
//                        {
//                            manager.SetParticleToFinger(finger, manager.removeWeedParticle);

//                            Ray ray = Camera.main.ScreenPointToRay(finger.ScreenPosition);
//                            if (Physics.Raycast(ray, out RaycastHit hit, 500f, PlantingInteractManager.Instance.removeWeedLayer))
//                            {
//                                var pot = hit.collider.GetComponentInParent<CLIP.Project_Mouse.Scene_View_Control.Pot_In_Scene>();
//                                if (pot != null)
//                                {
//                                    Debug.Log("划动碰到花盆: " + pot.gameObject.name);
//                                    if (pot._weed_go != null)
//                                    {
//                                        pot.on_select();
//                                    }
//                                }
//                            }
//                        }
//                        // 手指抬起
//                        else if (finger == activeFinger && finger.Up)
//                        {
//                            if (manager.removeWeedParticle != null)
//                            {
//                                manager.ChangeState(new NonePlantState());
//                            }
//                            activeFinger = null;

//                            foreach (var pot in CLIP.Project_Mouse.Game_Play_System.Planting_System.Planting_System_Manager.Instance._pot_list)
//                            {
//                                pot.on_exit_removing_weed();
//                            }
//                        }
//                    }
//#endif

//#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
//                    if (LeanTouch.Fingers.Count >= 1)
//                    {
//                        Debug.Log("检测到手指数量: " + LeanTouch.Fingers.Count);
//                        var finger = LeanTouch.Fingers[0];

//                        // 手指按下
//                        if (finger.Down && activeFinger == null)
//                        {
//                            activeFinger = finger;
//                            if (manager.removeWeedParticle != null && !manager.removeWeedParticle.isPlaying)
//                            {
//                                manager.SetParticleToFinger(finger, manager.removeWeedParticle);
//                                manager.removeWeedParticle.Play();
//                            }
//                        }
//                        // 手指移动
//                        else if (finger == activeFinger && finger.Set)
//                        {
//                            manager.SetParticleToFinger(finger, manager.removeWeedParticle);

//                            Ray ray = Camera.main.ScreenPointToRay(finger.ScreenPosition);
//                            if (Physics.Raycast(ray, out RaycastHit hit, 500f, PlantingInteractManager.Instance.removeWeedLayer))
//                            {
//                                var pot = hit.collider.GetComponentInParent<CLIP.Project_Mouse.Scene_View_Control.Pot_In_Scene>();
//                                if (pot != null)
//                                {
//                                    Debug.Log("划动碰到花盆: " + pot.gameObject.name);
//                                    if (pot._weed_go != null)
//                                    {
//                                        pot.on_select();
//                                    }
//                                }
//                            }
//                        }
//                        // 手指抬起
//                        else if (finger == activeFinger && finger.Up)
//                        {
//                            if (manager.removeWeedParticle != null)
//                            {
//                                manager.ChangeState(new NonePlantState());
//                            }
//                            activeFinger = null;

//                            foreach (var pot in CLIP.Project_Mouse.Game_Play_System.Planting_System.Planting_System_Manager.Instance._pot_list)
//                            {
//                                pot.on_exit_removing_weed();
//                            }
//                        }
//                    }
//#endif

//                }
//            }
//        }
//    }
//}

