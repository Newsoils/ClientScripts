using System.Collections.Generic;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 玩家全屋存档的根对象（多房间快照）。
    /// <para><b>网络（Protobuf）</b>：<c>Cmd.GetRoomDataRes.RoomData</c> 为 repeated string，
    /// 每个元素是一段 JSON，反序列化为一个 <see cref="RoomData"/>；客户端组装为本类型的 <see cref="rooms"/>。</para>
    /// <para><b>上传</b>：<c>Cmd.ChangeRoomDataReq</c> 可同时带增量（Add/Upd/Del + <c>RoomItemData</c>）与整房 JSON（repeated string <c>RoomData</c>），以后端约定为准。</para>
    /// <para><b>本地</b>：与 <c>Room.json</c> 等 JSON 根对象字段一致。</para>
    /// <para><b>运行时</b>：<c>RoomSystem</c> 维护与各场景房间实例同步的 <see cref="RoomData"/> 列表。</para>
    /// </summary>
    [System.Serializable]
    public class RoomSaveData
    {
        public List<RoomData> rooms = new();
    }
}
