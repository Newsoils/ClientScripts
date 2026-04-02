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

                [CreateAssetMenu(fileName = "General_Event_Hub_SO", menuName = "Project_Mouse/Event_System/General_Event_Hub_SO")]
                public class General_Event_Hub_SO : ScriptableObject
                {
                    public UnityEvent _on_exit_game = new UnityEvent();


                    public void exit_game()
                    {
                        if(_on_exit_game!=null) _on_exit_game.Invoke();
                    }
                }
            }
        }
    }
}
