using UnityEngine;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine.UI;
using System;
using CLIP.Framework_Unity;
using CLIP.Framework_Core.Event;

namespace CLIP.Project_Mouse.UI
{
    public class EditPlacementPanel : UIPanelBase
    {
        public Canvas canvas;           // Overlay Canvas
        public GameObject obj;
        private RectTransform panelRoot; // 面板根节点

        public Button BTN_Delete;
        public Button BTN_Rotate;

        private GridObject curGridObject;
        private string curGridLayerUid;
  

        private void Start()
        {
            BTN_Delete.onClick.AddListener(DeletePlacement);
            BTN_Rotate.onClick.AddListener(RotatePlacement);
            canvas = GetComponentInParent<Canvas>();
            panelRoot  = obj.GetComponent<RectTransform>();

            EvtDsp.AddEvt<Vector3>(EvtNames.Move_Placement_Panel, SetPanelPosition);
            EvtDsp.AddEvt<GridObject, string, Vector3>(EvtNames.Open_Edit_Placement_Panel, OpenPanel);
            EvtDsp.AddEvt(EvtNames.Close_Edit_Placement_Panel, ClosePanel);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            BTN_Delete.onClick.RemoveListener(DeletePlacement);
            BTN_Rotate.onClick.RemoveListener(RotatePlacement);

            EvtDsp.RemoveEvt<Vector3>(EvtNames.Move_Placement_Panel, SetPanelPosition);
            EvtDsp.RemoveEvt<GridObject, string, Vector3>(EvtNames.Open_Edit_Placement_Panel, OpenPanel);
            EvtDsp.RemoveEvt(EvtNames.Close_Edit_Placement_Panel, ClosePanel);
        }

        public void DeletePlacement()
        {
            if (curGridObject != null && RoomSystem.currentRoom != null)
            {
                if (!GridObjectSystem.DeleteGridObject(curGridObject, RoomSystem.currentRoom, curGridLayerUid, curGridObject.data.position))
                    return;
                ClosePanel();
            }
            else
            {
                Log.Error("EditPlacementPanel ：curPlacement为空或者RoomSystem.currentRoom为空");
            }
        }

        public void RotatePlacement()
        {
            EditManager.Instance.HandleRotate();
        }

        private void OpenPanel(GridObject pl, string gridLayerUId, Vector3 hitPos)
        {
            UIManager.Instance.OpenPanel<EditPlacementPanel>(pl, gridLayerUId, hitPos);
        }
        private void OpenPanel(Pot pot, string gridLayerUId, Vector3 hitPos)
        {

        }

        public override void OpenPanel(params object[] data)
        {
            obj.SetActive(true);
            Vector3 hitpos = Vector3.zero;
            try
            {
                curGridObject = (GridObject)data[0];
                curGridLayerUid = (string)data[1];
                hitpos = (Vector3)data[2];

                var pos = curGridObject.transform.position;

                //var width = curPlacement.data.placementInfo.width;
                //var length = curPlacement.data.placementInfo.length;
                //var height = curPlacement.data.placementInfo.height;

                //SetPanelPosition(new Vector3(pos.x, hitpos.y + height, pos.y));

                SetPanelPosition(pos);
                RefreshDeleteButtonState();
            }
            catch (Exception e)
            {
                Log.Error(e.Message); 
            }
        }

        /// <summary>盆内有植物时不可删盆（用于隐藏删除钮与底层校验）。</summary>
        private static bool IsPotDeleteBlocked(GridObject obj) =>
            obj is Pot pot && PlantManager.Instance.GetPlantByPot(pot) != null;

        /// <summary>盆内有植物时隐藏删除按钮（关掉），非盆或空盆照常显示。</summary>
        private void RefreshDeleteButtonState()
        {
            bool showDelete = curGridObject == null || !IsPotDeleteBlocked(curGridObject);
            BTN_Delete.gameObject.SetActive(showDelete);
        }


        public override void ClosePanel()
        {
            obj.SetActive(false);
            curGridObject = null;
            curGridLayerUid = string.Empty;
        }

        public void SetPanelPosition(Vector3 worldPos)
        {
            // 1️⃣ 世界 → 屏幕
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // 2️⃣ 屏幕 → Canvas 本地坐标
            RectTransform canvasRect = canvas.transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                null, // ⚠️ Overlay 必须是 null
                out Vector2 localPos);

            // 3️⃣ 应用到 UI
            panelRoot.anchoredPosition = localPos;

            //不让UI飞出去，限制一下范围
            Vector2 pos = panelRoot.anchoredPosition;
            Vector2 size = panelRoot.sizeDelta;
            Vector2 half = size * 0.5f;

            Vector2 min = -canvasRect.sizeDelta * 0.5f + half;
            Vector2 max = canvasRect.sizeDelta * 0.5f - half;

            pos.x = Mathf.Clamp(pos.x, min.x, max.x);
            pos.y = Mathf.Clamp(pos.y, min.y, max.y);

            panelRoot.anchoredPosition = pos;
            if (curGridObject != null)
                RefreshDeleteButtonState();
        }


      
    }

}
