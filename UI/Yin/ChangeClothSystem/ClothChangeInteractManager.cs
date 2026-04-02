using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Client_Event_Systems;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class ClothChangeInteractManager : MonoBehaviour, IPointerDownHandler, IDragHandler
            {
                public static ClothChangeInteractManager Instance { get; private set; }

                [Header("UI")]
                public GameObject changeClothPanel;
                public RawImage rawImage;
                public GameObject preSetButton;
                public TMP_Text preSetButtonText;


                public GameObject currentCharacter;

                [Header("旋转模特")]
                private Vector2 lastPointerPos;
                public float rotateSpeed = 1f;

                [Header("预设服装")]
                public bool isPreWear = false;
                public bool isPutOn = false;
                public PreSetClothUnit currentPreSet;
                public PreSetClothUnit lastPreSet;

                [Header("Render Texture")]
                public RenderTexture characterRT;
                public RenderTexture modelRT;

                [Header("Event System")]
                public Change_Clothes_Warehouse_Event_Hub_SO change_Clothes_Warehouse_Event_Hub_SO;


                public Character_Clothes_Info saved;


                private void Awake()
                {
                    if (Instance != null && Instance != this)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    Instance = this;
                }

                private void Start()
                {
                    DontDestroyOnLoad(this.gameObject);
                }

                private void OnEnable()
                {
                    rawImage.texture = characterRT;
                    //if (CommonInteractManager.Instance != null)
                    //{
                    //    characterInLevel = CommonInteractManager.Instance.main_Character_AI_Control.GetComponent<Character_Clothes_in_Level>();
                    //    if (characterInLevel != null)
                    //    {
                    //        if (characterInLevel._character_clothes_info._cloth_preset.Count < 3)
                    //        {
                    //            int need = 3 - characterInLevel._character_clothes_info._cloth_preset.Count;
                    //            for (int i = 0; i < need; i++)
                    //            {
                    //                characterInLevel._character_clothes_info._cloth_preset.Add(null);
                    //            }
                    //        }
                    //    }
                    //}
                    //int needAdd = 3 - this.character._character_clothes_info._cloth_preset.Count;
                    //for (int i = 0; i < needAdd; i++)
                    //{
                    //    this.character._character_clothes_info._cloth_preset.Add(null);
                    //}
                    //needAdd = 3 - model._character_clothes_info._cloth_preset.Count;
                    //for (int i = 0; i < needAdd; i++)
                    //{
                    //    model._character_clothes_info._cloth_preset.Add(null);
                    //}

                    InitCharacterCloth();
                    change_Clothes_Warehouse_Event_Hub_SO._selected_item_change.AddListener(OnSelectWarehouseItem);
                    change_Clothes_Warehouse_Event_Hub_SO._deselected_item.AddListener(OnDeselectWarehouseItem);

                    //StartCoroutine(SyncMainCharacterClothToInLevelCo());
                }

                private void OnDisable()
                {
                    change_Clothes_Warehouse_Event_Hub_SO._selected_item_change.RemoveListener(OnSelectWarehouseItem);
                    change_Clothes_Warehouse_Event_Hub_SO._deselected_item.RemoveListener(OnDeselectWarehouseItem);
                }


                // 检查是否有操作未保存
                public bool isUnsaved()
                {
                    return Character_Cloth_Manager.Instance.IsUnsaved();
                }

                private bool IsPresetEmpty(Cloth_Suit preset)
                {
                    if (preset == null) return true;
                    return
                        preset._top_name == "NULL" &&
                        preset._upper_name == "NULL" &&
                        preset._dress_name == "NULL" &&
                        preset._lower_name == "NULL" &&
                        preset._bottom_name == "NULL" &&
                        preset._attachment_name == "NULL";
                }

                // 检查当前预设是否未保存
                public bool IsCurrentPresetUnsaved()
                {
                    //if (currentPreset == null) return false;

                    //var presetIndex = currentPreset.preSetIndex;
                    //var inLevelPreset = characterInLevel._character_clothes_info._cloth_preset;
                    //var modelPreset = model._character_clothes_info._cloth_preset;

                    //if (inLevelPreset.Count != modelPreset.Count) return true;

                    //var inLevelCloth = (presetIndex >= 0 && presetIndex < inLevelPreset.Count) ? inLevelPreset[presetIndex] : null;
                    //var modelCloth = (presetIndex >= 0 && presetIndex < modelPreset.Count) ? modelPreset[presetIndex] : null;

                    //if (inLevelCloth == null && modelCloth == null) return false;

                    //if (inLevelCloth == null || modelCloth == null) return true;

                    //if (inLevelCloth._top_name != modelCloth._top_name) return true;
                    //if (inLevelCloth._upper_name != modelCloth._upper_name) return true;
                    //if (inLevelCloth._dress_name != modelCloth._dress_name) return true;
                    //if (inLevelCloth._lower_name != modelCloth._lower_name) return true;
                    //if (inLevelCloth._bottom_name != modelCloth._bottom_name) return true;
                    //if (inLevelCloth._atttachment_name != modelCloth._atttachment_name) return true;

                    return false;
                }

                // 穿上/脱下预设服装
                public void PutOn()
                {
                    //if (isPutOn)
                    //{
                    //    if (isUnsaved())
                    //    {
                    //        PromptMessage.Instance.ShowPrompt(5, () =>
                    //        {
                    //            presetButtonText.text = "脱下";

                    //            lastPreset = currentPreset;
                    //            currentCharacter = character;
                    //            //SaveCurrentCharacterClothes();
                    //            character._character_clothes_info._cloth_preset = new List<Cloth_Suit>(model._character_clothes_info._cloth_preset);
                    //            character._character_clothes_info.WearPreset(currentPreset.preSetIndex);
                    //            character.update_view();
                    //            //DeselectPreWear();
                    //            DeselectWhenPutOn();
                    //            InitCharacterRotate();
                    //            isPutOn = false;
                    //            presetButton.SetActive(true);
                    //        });
                    //    }
                    //    else
                    //    {
                    //        presetButtonText.text = "脱下";

                    //        lastPreset = currentPreset;
                    //        currentCharacter = character;
                    //        //SaveCurrentCharacterClothes();
                    //        //DeselectPreWear();
                    //        character._character_clothes_info._cloth_preset = new List<Cloth_Suit>(model._character_clothes_info._cloth_preset);
                    //        character._character_clothes_info.WearPreset(currentPreset.preSetIndex);
                    //        character.update_view();
                    //        DeselectWhenPutOn();
                    //        InitCharacterRotate();
                    //        isPutOn = false;
                    //        presetButton.SetActive(true);
                    //    }
                    //}
                    //else
                    //{
                    //    lastPreset.OnTakeOff();
                    //    LoadSavedCharacterClothes();
                    //}
                    //change_Clothes_Warehouse_Event_Hub_SO._invoke_on_refresh_warehouse_UI();
                }

                // 保存当前角色衣服
                public void SaveCurrentCharacterClothes()
                {
                    var info = Character_Cloth_Manager.Instance.targetCloth;

                    saved = new Character_Clothes_Info (info);
                }

                // 载入保存的角色衣服
                public void LoadSavedCharacterClothes()
                {
                    Character_Cloth_Manager.Instance.Set_Target_Cloth(saved);
                }

                // 确认修改
                public void ConfirmCloth()
                {
                    var targetCloth = Character_Cloth_Manager.Instance.targetCloth;
                    if (isPreWear)
                    {
                        //TODO  预设
                    //    var newPreset = new Cloth_Suit
                    //    {
                    //        _top_name = currentCharacter._character_clothes_info._top_name,
                    //        _upper_name = currentCharacter._character_clothes_info._upper_name,
                    //        _dress_name = currentCharacter._character_clothes_info._dress_name,
                    //        _lower_name = currentCharacter._character_clothes_info._lower_name,
                    //        _bottom_name = currentCharacter._character_clothes_info._bottom_name,
                    //        _atttachment_name = currentCharacter._character_clothes_info._atttachment_name
                    //    };
                    //    targetCloth.change_preset(newPreset, currentPreset.preSetIndex);

                    //    presetButton.SetActive(false);
                    //    DeselectPreWear();
                    //}

                    //if (character != null && model != null && model._character_clothes_info != null)
                    //{
                    //    character._character_clothes_info._cloth_preset = new List<Cloth_Suit>(model._character_clothes_info._cloth_preset);
                    }

                    change_Clothes_Warehouse_Event_Hub_SO._invoke_on_refresh_warehouse_UI();
                    Character_Cloth_Manager.Instance.Upload_Main_Characer_Cloth_Info();
                }

                // 退出预设
                public void DeselectPreWear()
                {
                    currentPreSet.OnDeselect();
                    //rawImage.texture = characterRT;
                    //InitCharacterRotate();
                    preSetButton.SetActive(false);
                    isPreWear = false;
                }

                // 穿上
                public void DeselectWhenPutOn()
                {
                    
                }

                // 初始化角色衣服
                public void InitCharacterCloth()
                {
                    Character_Cloth_Manager.Instance.Init_Target_Character_Cloth();
                }

                // 选择仓库物品
                public void OnSelectWarehouseItem(string clothName)
                {
                    Character_Cloth_Manager.Instance.Add_Cloth(Character_Type.Target_Character, clothName);
                }

                // 取消选择仓库物品
                public void OnDeselectWarehouseItem(string clothName)
                {
                    Character_Cloth_Manager.Instance.Remove_Cloth(Character_Type.Target_Character, clothName);  
                }

                // 设置自动穿搭
                public void SetAutoChangeCloth(bool isAuto)
                {
                    Character_Cloth_Manager.Instance.targetCloth.is_auto_change_cloth = isAuto;
                }

                // 打开换装
                public void OpenChangeCloth()
                {
                    InitCharacterCloth();
                    //InitCharacterRotate();
                    isPreWear = false;

                    changeClothPanel.SetActive(true);
                    //StartCoroutine(OpenChangeClothCo());
                }

                //public IEnumerator OpenChangeClothCo()
                //{
                //    CommonInteractManager.Instance.ExitPhone();

                //    yield return new WaitForSeconds(1.5f);
                //    CommonInteractManager.Instance.ClosePanelWithoutUpAndMain();


                //}

                //private void InitCharacterRotate()
                //{
                //    var t = currentCharacter.transform;
                //    t.eulerAngles = new Vector3(t.eulerAngles.x, 180f, t.eulerAngles.z);
                //}

                // 关闭换装
                public void CloseChangeCloth()
                {
                    if (isUnsaved())
                    {
                        PromptMessage.Instance.ShowPrompt(5, () =>
                        {
                            changeClothPanel.SetActive(false);
                            //CommonInteractManager.Instance.OpenPanelWithoutUpAndMain();
                            Character_Cloth_Manager.Instance.Sync_Main_Character_Cloth();
                        });
                    }
                    else
                    {
                        changeClothPanel.SetActive(false);
                        //CommonInteractManager.Instance.OpenPanelWithoutUpAndMain();
                        Character_Cloth_Manager.Instance.Sync_Main_Character_Cloth();
                    }
                }

                public void OnPointerDown(PointerEventData eventData)
                {
                    if (IsPointerInRawImage(eventData.position))
                        lastPointerPos = eventData.position;
                }

                public void OnDrag(PointerEventData eventData)
                {
                    if (!IsPointerInRawImage(eventData.position)) return;

                    float deltaX = lastPointerPos.x - eventData.position.x;
                    lastPointerPos = eventData.position;

                    if (currentCharacter != null)
                    {
                        var t = currentCharacter.transform;
                        float newY = t.eulerAngles.y + deltaX * rotateSpeed;
                        t.eulerAngles = new Vector3(t.eulerAngles.x, newY, t.eulerAngles.z);
                    }
                }

                public bool IsPointerInRawImage(Vector2 screenPos)
                {
                    if (rawImage == null) return false;
                    RectTransform rt = rawImage.rectTransform;
                    // 将屏幕坐标转换为UI坐标
                    Vector2 localPoint;
                    var canvas = rawImage.canvas;
                    if (canvas == null) canvas = rawImage.GetComponentInParent<Canvas>();
                    Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                    return RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, cam, out localPoint)
                        && rt.rect.Contains(localPoint);
                }

         
            }
        }
    }
}

