//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using Lean.Touch;
//using UnityEngine;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class IrrigatePlantState : IState
//            {
//                private LeanFinger activeFinger;
//                private float irrigateTime = 0;
//                public void EnterState(PlantingInteractManager manager)
//                {
//                    manager.leanPitch.enabled = false;
//                    manager.leanMultiUpdate.enabled = false;
//                    PlantingInteractManager.Instance.SetCanRotatePot(false);
//                }

//                public void ExitState(PlantingInteractManager manager)
//                {
//                    manager.wateringParticle.Stop();
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
//                            irrigateTime = 0;
//                            if (manager.wateringParticle != null && !manager.wateringParticle.isPlaying)
//                            {
//                                manager.SetParticleToFinger(finger, manager.wateringParticle);
//                                manager.wateringParticle.Play();
//                            }
//                        }
//                        // 手指移动
//                        else if (finger == activeFinger && finger.Set)
//                        {
//                            manager.SetParticleToFinger(finger, manager.wateringParticle);
//                            irrigateTime += Time.deltaTime;
//                            if (irrigateTime >= 3f)
//                            {
//                                Planting_System_Manager.Instance.try_irrigate_all_level_pot(100);
//                                Planting_System_Manager.Instance._planting_event_hub._invoke_on_show_up_prompt("已浇水");
//                            }
//                        }
//                        // 手指抬起
//                        else if (finger == activeFinger && finger.Up)
//                        {
//                            if (irrigateTime < 3f)
//                            {
//                                Planting_System_Manager.Instance.try_irrigate_all_level_pot(50);
//                                Planting_System_Manager.Instance._planting_event_hub._invoke_on_show_up_prompt("还未浇满水");
//                            }
//                            if (manager.wateringParticle != null)
//                            {
//                                manager.ChangeState(new NonePlantState());
//                            }
//                            activeFinger = null;
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
//                            irrigateTime = 0;
//                            if (manager.wateringParticle != null && !manager.wateringParticle.isPlaying)
//                            {
//                                manager.SetParticleToFinger(finger, manager.wateringParticle);
//                                manager.wateringParticle.Play();
//                            }
//                        }
//                        // 手指移动
//                        else if (finger == activeFinger && finger.Set)
//                        {
//                            manager.SetParticleToFinger(finger, manager.wateringParticle);
//                            irrigateTime += Time.deltaTime;
//                            if (irrigateTime >= 3f)
//                            {
//                                Planting_System_Manager.Instance.try_irrigate_all_level_pot(100);
//                                Planting_System_Manager.Instance._planting_event_hub._invoke_on_show_up_prompt("已浇水");
//                            }
//                        }
//                        // 手指抬起
//                        else if (finger == activeFinger && finger.Up)
//                        {
//                            if (irrigateTime < 3f)
//                            {
//                                Planting_System_Manager.Instance.try_irrigate_all_level_pot(50);
//                                Planting_System_Manager.Instance._planting_event_hub._invoke_on_show_up_prompt("还未浇满水");
//                            }
//                            if (manager.wateringParticle != null)
//                            {
//                                manager.ChangeState(new NonePlantState());
//                            }
//                            activeFinger = null;
//                        }
//                    }
//#endif
//                }
//            }
//            }
//     }
//}

