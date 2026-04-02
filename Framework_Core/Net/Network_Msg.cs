namespace CLIP.Framework_Core.Network
{
    public enum Msg_Sending_Mode
    {
        Client_to_Client,
        Server_to_Server,
        Client_to_Server,
        Server_to_Client
    }

    [System.Serializable]
    public struct Network_Msg
    {
        public string player_id;

        public string sender;

        public string action_target;

        public string action;

        public string detail_info;

        public Msg_Sending_Mode _sending_mode;

        public byte[] detail_data;

        public int msg_id;
        public Network_Msg(int _id = -1, string _player_id = "Default_Player_id")
        {
            player_id = "Default_Player_id";
            sender = "Default_Sender";
            action_target = "Default_Target";
            action = "Default_Action";
            detail_info = "Default_detail";
            _sending_mode = Msg_Sending_Mode.Client_to_Client;

            msg_id = _id;

            detail_data = null;
        }
    }
}