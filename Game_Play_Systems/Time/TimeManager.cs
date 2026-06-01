using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class TimeManager : SingletonMono<TimeManager>
    {
        protected override bool PersistAcrossScenes => true;
        private DateTime curTime;
        public string currentTime;
        private Timer timer;
        public bool isDay;
        private bool isInit;
        private void Start()
        {
            StartCoroutine(test());
        }
        private void Update()
        {
            if(isInit)
            {
                curTime = curTime.AddSeconds(Time.unscaledDeltaTime);
                currentTime = curTime.ToString();
            }
        }
        IEnumerator test()
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            GetTime();
        }
        public void GetTime(Action onComplete = null)
        {
            // TODO zhaorui
            //EvtDsp.ReturnEvt< ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, GetTimeTask(), (string result) => onComplete?.Invoke());
        }

        public void SetServerTime(long unixSeconds)
        {
            curTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).LocalDateTime;
            AnalyzeTime();
            isInit = true;
            EvtDsp.TriggerEvt(EvtNames.On_Set_Time);
        }
        private ServerTask GetTimeTask()
        {
            var req = new Cmd.GetServerCurrentTimeReq();
            ServerTask task = new ServerTask(req, (string result, ServerTask task) =>
            {
                if(result != "nodata")
                {
                    curTime = DateTime.ParseExact(
                        result,
                        "yyyy/M/d H:mm:ss",
                        null
                    );
                    AnalyzeTime();
                    isInit = true;
                    EvtDsp.TriggerEvt(EvtNames.On_Set_Time);
                }
            });
            return task;
        }
        private void AnalyzeTime()
        {
            int month = curTime.Month;
            TimeSpan currentTime = curTime.TimeOfDay;

            string season;
            TimeSpan dayStart;
            TimeSpan dayEnd;

            // 判断季节并设置对应的白天时间段
            if (month >= 3 && month <= 5) // 春季
            {
                season = "春季";
                dayStart = new TimeSpan(6, 0, 0);   // 06:00
                dayEnd = new TimeSpan(19, 0, 0);    // 19:00
            }
            else if (month >= 6 && month <= 8) // 夏季
            {
                season = "夏季";
                dayStart = new TimeSpan(5, 0, 0);   // 05:00
                dayEnd = new TimeSpan(20, 0, 0);    // 20:00
            }
            else if (month >= 9 && month <= 11) // 秋季
            {
                season = "秋季";
                dayStart = new TimeSpan(6, 30, 0);  // 06:30
                dayEnd = new TimeSpan(17, 20, 0);   // 17:20
            }
            else // 冬季 (12, 1, 2月)
            {
                season = "冬季";
                dayStart = new TimeSpan(6, 50, 0);  // 06:50
                dayEnd = new TimeSpan(17, 0, 0);    // 17:00
            }

            if (currentTime >= dayStart && currentTime <= dayEnd)
            {
                isDay = true;
            }
            else
            {
                isDay = false;
            }
            return;
        }
    }
}
