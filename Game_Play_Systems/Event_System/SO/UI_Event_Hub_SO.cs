using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

#endif
using UnityEngine;
using UnityEngine.Events;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Game_Play_System
        {
            namespace Event_System
            {

                [CreateAssetMenu(fileName = "UI_Event_Hub_SO", menuName = "Project_Mouse/Event_System/UI_Event_Hub_SO")]
                public class UI_Event_Hub_SO : ScriptableObject
                {
                    public UnityEvent _switch_to_workbench_mode=new UnityEvent();

                    public UnityEvent _switch_to_storage_mode = new UnityEvent();
                    public UnityEvent _switch_to_add_food_mode = new UnityEvent();
                }
            }
            }
        }
    } 
 