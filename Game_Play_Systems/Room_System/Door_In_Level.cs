using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Scene_View_Control;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class Door_In_Level : MonoBehaviour, IClick
    {
        public Room_Door_Info _door_info;
        public Renderer _renderer;
        public string doorTargetRoomName;

        private Room _ownerRoom;
        private Hide_Wall _parentWall;
        private bool _isInCurrentRoom;
        private bool _isWallVisible = true;

        bool IsClickable => _isInCurrentRoom && _isWallVisible;

        void Start()
        {
            _door_info._door_name = this.gameObject.name;
            _renderer = this.GetComponent<Renderer>();
            _ownerRoom = GetComponentInParent<Room>();
            _parentWall = GetComponentInParent<Hide_Wall>();

            if (_parentWall != null)
                _parentWall.OnVisableChange += OnWallVisibleChange;

            EvtDsp.AddEvt<Room>(EvtNames.SwitchRoom, OnSwitchRoom);
            if (RoomSystem.currentRoom != null)
                OnSwitchRoom(RoomSystem.currentRoom);
        }

        void OnDestroy()
        {
            EvtDsp.RemoveEvt<Room>(EvtNames.SwitchRoom, OnSwitchRoom);
            if (_parentWall != null)
                _parentWall.OnVisableChange -= OnWallVisibleChange;
        }

        private void OnSwitchRoom(Room newRoom)
        {
            _isInCurrentRoom = _ownerRoom != null && _ownerRoom == newRoom;
        }

        private void OnWallVisibleChange(bool visible)
        {
            _isWallVisible = visible;
        }

        public void on_select_door()
        {
            Debug.Log("Selected door: " + this.gameObject.name);
        }
        public void SwitchRoom()
        {
            if (string.IsNullOrEmpty(doorTargetRoomName)) return;
            RoomSystem.Instance.SwitchRoomByName(doorTargetRoomName);
        }
        public bool OnClick(Vector3 p)
        {
            if (IsClickable)
            {
                SwitchRoom();
                return true;
            }
            return false;
        }
        public void EnableDoor(bool value)
        {
            _isInCurrentRoom = value;
        }
    }

}
