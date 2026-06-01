using System.Collections.Generic;
using CLIP.Framework_Unity;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class GachaManager : SingletonMono<GachaManager>
    {
        readonly Dictionary<long, ulong> _recordUIDByPoolID = new Dictionary<long, ulong>();
        readonly Dictionary<long, ulong> _recordUIDByTagID = new Dictionary<long, ulong>();

        public void ClearServerGachaData()
        {
            _recordUIDByPoolID.Clear();
            _recordUIDByTagID.Clear();
        }

        public void ApplyGachaListRes(Cmd.GachaListRes res)
        {
            ClearServerGachaData();

            if (res?.Infos == null)
                return;

            foreach (var info in res.Infos)
            {
                if (info == null || info.RecordID == 0UL)
                    continue;
                if (info.PoolID != 0L)
                    _recordUIDByPoolID[info.PoolID] = info.RecordID;
                if (info.TagID != 0L)
                    _recordUIDByTagID[info.TagID] = info.RecordID;
            }
        }

        public bool TryGetRecordUIDByPoolID(long poolID, out ulong recordUID)
        {
            return _recordUIDByPoolID.TryGetValue(poolID, out recordUID) && recordUID != 0UL;
        }

        public bool TryGetRecordUIDByTagID(long tagID, out ulong recordUID)
        {
            return _recordUIDByTagID.TryGetValue(tagID, out recordUID) && recordUID != 0UL;
        }
    }
}
