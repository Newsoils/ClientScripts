using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Game_Play_System
        {
            [Serializable]
            [CreateAssetMenu(fileName = "sound", menuName = "resources_so")]
            public class Sound : ScriptableObject
            {
                [Header("音频名称")]
                public string clipName;

                [Header("音频文件")]
                public AudioClip clip;

                [Header("音量")]
                [Range(0,1)]
                public float volume;
            }

            //请保证此处字符串与so中相同
            public class SoundName
            {
                //BGM
                public static string loginPage = "loginPage";
                public static string mainDay = "mainDay";
                public static string mainNight = "mainNight";

                //SFX

            }
        }
    }
}
