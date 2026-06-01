namespace CLIP.Framework_Core.Network
{
    public interface IMsg_Receiver
    {
        // public string get_receiver_name();

        public void receive_msg(Network_Msg _msg);

    }
}
