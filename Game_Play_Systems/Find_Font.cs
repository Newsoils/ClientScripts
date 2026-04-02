using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace CLIP.Project_Mouse.Game_Play_System
{
    public class Find_Font : MonoBehaviour
    {
        // Start is called before the first frame update
        public Text _text;
        public TextMeshProUGUI _text_tmp;
        void Start()
        {
#if WX && !UNITY_EDITOR
        StartCoroutine(set_up_font());
#endif
        }

        // Update is called once per frame
        void Update()
        {

        }

        public IEnumerator set_up_font()
        {
            while (true)
            {
#if WX
                        yield return new WaitForSecondsRealtime(1f);
                        if (WeChat_Env.instance == null)
                        {
                            continue;
                        }
                        else
                        {
                            if (WeChat_Env.instance._wx_font == null)
                            {
                                WeChat_Env.instance.on_font_loaded.AddListener(() =>
                                {

                                    change_font(WeChat_Env.instance._wx_font);
                                });
                                yield break;
                            }
                            else
                            {
                                change_font(WeChat_Env.instance._wx_font);
                                yield break;
                            }
                        }
#endif
            }
        }
        public void change_font(Font _font)
        {
            if (_text != null) _text.font = _font;
            if (_text_tmp != null) _text_tmp.font = TMP_FontAsset.CreateFontAsset(_font);
        }
    }
}

