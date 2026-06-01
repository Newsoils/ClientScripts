using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class MissionUpdateInfo
    {
        public List<MissionRuntimeData> missionAdd = null;
        public List<MissionRuntimeData> missionUpdate = null;
        public List<ulong> missionDelete = null;
    }

    public class MissionManager : SingletonMono<MissionManager>
    {
        public Dictionary<int, MissionStaticData> missionStaticDic = new();
        public Dictionary<ulong, MissionRuntimeData> missionRuntimeDic = new();

        private void Start()
        {
            NotifyClaimableChanged();
            EvtDsp.AddEvt<MissionUpdateInfo>(EvtNames.OnMissionUpdate, UpdateMission);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EvtDsp.RemoveEvt<MissionUpdateInfo>(EvtNames.OnMissionUpdate, UpdateMission);
        }

        public void Init(List<MissionRuntimeData> missionRuntimeDatas)
        {
            JsonDataManager.LoadTaskData(out missionStaticDic);

            foreach (var missionRData in missionRuntimeDatas)
            {
                if (missionStaticDic.TryGetValue(missionRData.missionId, out var taskStaticData))
                {
                    missionRuntimeDic[missionRData.missionUId] = missionRData;
                    missionRData.target = taskStaticData.targetCount;
                }
                else
                {
                    Log.Error("收到了不存在ID的Mission！");
                }
            }

            foreach (var (_, missionRData) in missionRuntimeDic)
            {

                if (!missionRData.isAccept)
                    missionRData.AcceptTask();
            }
        }

        public void UpdateMission(MissionUpdateInfo info)
        {
            if (info.missionAdd != null)
            {
                foreach (var m in info.missionAdd)
                {
                    missionRuntimeDic.Add(m.missionUId, m);
                    m.target = missionStaticDic[m.missionId].targetCount;
                }
            }
            if (info.missionUpdate != null)
            {
                foreach (var m in info.missionUpdate)
                {
                    missionRuntimeDic[m.missionUId] = m;
                    m.target = missionStaticDic[m.missionId].targetCount;
                }
            }
            if (info.missionDelete != null)
            {
                foreach (var infoId in info.missionDelete)
                    missionRuntimeDic.Remove(infoId);
            }
            EvtDsp.TriggerEvt(EvtNames.OnMissionRefresh);
            NotifyClaimableChanged();
        }

        public void RemoveMission(ulong taskUId)
        {
            if (!missionRuntimeDic.TryGetValue(taskUId, out var task))
                return;

            missionRuntimeDic.Remove(taskUId);
            NotifyClaimableChanged();
            EvtDsp.TriggerEvt(EvtNames.OnMissionRefresh);
        }

        public bool HasClaimableReward()
        {
            foreach (var t in missionRuntimeDic.Values)
            {
                if (t.CanGetReward)
                    return true;
            }
            return false;
        }

        private static void NotifyClaimableChanged()
        {
            EvtDsp.TriggerEvt(EvtNames.Task_ClaimableChanged);
        }
    }
}
