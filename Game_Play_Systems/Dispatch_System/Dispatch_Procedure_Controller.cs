using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using UnityEngine;

public enum Procedure_Dispatch
{
    /// <summary>
    /// 选择背包阶段
    /// </summary>
    SelectBag = 0,
    /// <summary>
    /// 选择小食/食物阶段
    /// </summary>
    SelectFood = 1,
    /// <summary>
    /// 选择CD阶段
    /// </summary>
    SelectCD = 2,
    Default = 3
}

/// <summary>
/// 管理派遣场景流程
/// </summary>
public class Dispatch_Procedure_Controller : MonoBehaviour
{
    public Procedure_Dispatch current_Procedure;

    public GameObject procudure_SelectBag;
    public GameObject procudure_SelectFood;
    public GameObject procudure_SelectCD;

    public Transform foodRoot;
    public Transform snackRoot;
    public MeshRenderer cdMesh;


    private void Start()
    {
        EvtDsp.AddEvt<Procedure_Dispatch>(EvtNames.Dispatch_Switch_Procudure, EnterProcedure);
        EvtDsp.AddEvt<Item_Type, string>(EvtNames.Dispatch_Change_Item, ChangeItemPrefab);
        EvtDsp.AddEvt<int>(EvtNames.Dispatch_Refresh_Model, RefreshModel);
    }

    public void OnDestroy()
    {
        EvtDsp.RemoveEvt<Procedure_Dispatch>(EvtNames.Dispatch_Switch_Procudure, EnterProcedure);
        EvtDsp.RemoveEvt<Item_Type, string>(EvtNames.Dispatch_Change_Item, ChangeItemPrefab);
        EvtDsp.RemoveEvt<int>(EvtNames.Dispatch_Refresh_Model, RefreshModel);
    }


    public void RefreshModel(int bagIndex)
    {
        var current_info = Dispatch_Manager._instance.GetBagForModelPreview(bagIndex);
        if (current_info == null)
        {
            return;
        }
        if(current_info.foodName == null || string.IsNullOrEmpty(current_info.foodName))
        {
            ClearAllChildren(foodRoot);
        }
        else
        {
            ChangeItemPrefab(Item_Type.Food, current_info.foodName);
        }

        if (current_info.snackName == null || string.IsNullOrEmpty(current_info.snackName))
        {
            ClearAllChildren(snackRoot);
        }
        else
        {
            ChangeItemPrefab(Item_Type.Snack, current_info.snackName);
        }

        if (current_info.tapeName == null || string.IsNullOrEmpty(current_info.tapeName))
        {
            if (cdMesh != null)
                cdMesh.gameObject.SetActive(false);
        }
        else
        {
            if (cdMesh != null)
                cdMesh.gameObject.SetActive(true);
            ChangeItemPrefab(Item_Type.Tape, current_info.tapeName);
        }

    }

    public void ClearDispatchSceneModels()
    {
        ClearAllChildren(foodRoot);
        ClearAllChildren(snackRoot);
        if (cdMesh != null)
            cdMesh.gameObject.SetActive(false);
    }

    public void ChangeItemPrefab(Item_Type item_Type, string name)
    {
        _ = ChangeItemPrefabAsync(item_Type, name);
    }

    public async Task ChangeItemPrefabAsync(Item_Type item_Type,string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        switch (item_Type)
        {
            case Item_Type.Food:
                var food_resource_name = Dispatch_Manager.GetFoodInfo(name).model_resource_name;
                var foodOB = await GameAssets.Instance.LoadAsycByKey<GameObject>(food_resource_name);
                ClearAllChildren(foodRoot);
                if (foodOB!=null)
                {
                    Instantiate(foodOB, foodRoot);
                }
                break;
            case Item_Type.Snack:
                var snack_resource_name = Dispatch_Manager.GetSnackInfo(name).model_resource_name;
                var snackOB = await GameAssets.Instance.LoadAsycByKey<GameObject>(snack_resource_name);
                ClearAllChildren(snackRoot);

                if (snackOB!=null)
                {
                     Instantiate(snackOB, snackRoot);
                }
                break;
            case Item_Type.Tape:
                cdMesh.gameObject.SetActive(true);
                var cd_mt_name = Dispatch_Manager.GetCDInfo(name).mat_resource_name;
                var cd_mt = await GameAssets.Instance.LoadAsycByKey<Material>(cd_mt_name);
                cdMesh.material = cd_mt;
                break;
            default:
                break;
        }
    }

    public void EnterProcedure(Procedure_Dispatch procudure)
    {
        current_Procedure = procudure;
        procudure_SelectBag.SetActive(false);
        procudure_SelectFood.SetActive(false);
        procudure_SelectCD.SetActive(false);

        switch (procudure)
        {
            case Procedure_Dispatch.SelectBag:
                procudure_SelectBag.SetActive(true);
                ClearDispatchSceneModels();
                break;
            case Procedure_Dispatch.SelectFood:
                procudure_SelectFood.SetActive(true);
                //EvtDsp.TriggerEvt(EvtNames.Dispatch_Enter_SelectFood);
                break;
            case Procedure_Dispatch.SelectCD:
                procudure_SelectCD.SetActive(true);
                break;

        }
    }
    public static void ClearAllChildren(Transform root)
    {
        if (root == null)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            GameObject.Destroy(child.gameObject);
        }
    }

}
