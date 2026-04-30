using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Scene_View_Control;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    [System.Serializable]
    public class PlacementRuntime : GridObject
    {
        public string Name => data.name;
        public int ID => data.placementId;

        public Room_Placement_Info info;


        [Header("Hierarchy")]
        public GridLayerTag layerTag;
        public List<PlacementRuntime> subPlacements = new();
        public Transform subPlacementRoot;

        [Header("Render")]
        public List<Renderer> renderers = new();
        private List<Collider> colliders = new();
        public Material normalMat;


        [Header("Wall")]
        public Hide_Wall hideWall;

        private void Start()
        {
            //borderGenerator = GetComponentInChildren<PlacementBorderGenerator>();
            renderers = GetComponentsInChildren<Renderer>(true).ToList();
            colliders = GetComponentsInChildren<Collider>(true).ToList();
            Root = transform.Find("Root");

            ApplyRotation();
        }
        

        //========================
        // Rotation
        //========================

        public void Init(Room_Placement_Info pInfo = null)
        {
            if(pInfo!=null)
            {
                info = pInfo;
                var gridData = new GridData(info.length, info.width, info.height);
                Init(pInfo.room_placement_name, pInfo.room_placement_id,  gridData, info.placing_type);
            }
            else if(info!=null)
            {
                var gridData = new GridData(info.length, info.width, info.height);
                Init(info.room_placement_name, info.room_placement_id, gridData, info.placing_type);
            }
            else
            {
                Log.Error("Placement初始化失败，请提供info");
            }
        }

        //========================
        // Render Control
        //========================

        public void SetRenderState(bool visible)
        {
            if (renderers.Count == 0)
                renderers = GetComponentsInChildren<Renderer>(true).ToList();
            if (colliders.Count == 0)
                colliders = GetComponentsInChildren<Collider>(true).ToList();

            foreach (var r in renderers)
                if (r) r.enabled = visible;
            foreach (var c in colliders)
                if (c) c.enabled = visible;

            foreach (var sub in subPlacements)
                sub.SetRenderState(visible);
        }

        
    }
}