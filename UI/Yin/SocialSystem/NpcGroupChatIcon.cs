using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class NpcGroupChatIcon : MonoBehaviour
            {
                public NpcGroupChatDescription _parent_control;

                public NPC_Info info;
                public Image Image_Icon;
                public Image selectBg;

                public void InitNpcIcon(NPC_Info npc_info, NpcGroupChatDescription parent_control)
                {
                    info = npc_info;
                    _parent_control = parent_control;
                    _on_exit_selected();
                    // 加载头像
                }

                public void toggle_selection()
                {
                    _parent_control.ShowDescription(this);
                    _on_enter_selected();
                }

                public void _set_sprite(Sprite sp)
                {
                    Image_Icon.sprite = sp;
                }

                public void _on_enter_selected()
                {
                    Color c = selectBg.color;
                    c.a = 1f;
                    selectBg.color = c;
                }

                public void _on_exit_selected()
                {
                    Color c = selectBg.color;
                    c.a = 0f;
                    selectBg.color = c;
                }
            }
        }
    }
}