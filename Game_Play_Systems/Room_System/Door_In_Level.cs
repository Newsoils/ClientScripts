using CLIP.Project_Mouse.Kernel;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class Door_In_Level : MonoBehaviour, IClick
    {
        bool isEnable = true;
        public Room_Door_Info _door_info;
        public Renderer _renderer;
        public string doorTargetRoomName;

        void Start()
        {
            _door_info._door_name = this.gameObject.name;
            _renderer = this.GetComponent<Renderer>();
        }

        public void on_select_door()
        {
            string _name = this.gameObject.name;
            Debug.Log("Selected door: " + this.gameObject.name);

            //if(Indoor_Room_Game_Manager._activc_instance!=null) Indoor_Room_Game_Manager._activc_instance.on_select_door(_name);
        }
        public void SwitchRoom()
        {
            if (string.IsNullOrEmpty(doorTargetRoomName)) return;
            RoomSystem.Instance.SwitchRoomByName(doorTargetRoomName);
        }
        public bool OnClick(Vector3 p)
        {
            if (isEnable)
            {
                SwitchRoom();
                return true;
            }
            return false;
        }
        public void EnableDoor(bool value)
        {
            isEnable = value;
        }
    }

}
