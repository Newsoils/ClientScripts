using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 回收箱点击物品后的出售弹窗。数量范围 [1, 持有数量]，SellPrice = 单价 × 选择数量。
/// "确认回收" 走 <see cref="TryConfirmRecycle"/> → <see cref="PromptMessage"/> 二次确认 →
/// <see cref="DoRecycle"/> 一次性扣库存 + 入账鱼币 + 派发 <c>ReloadRecycleData</c>。
/// </summary>
public class RecycleSellPopup : MonoBehaviour
{
    [Header("根节点")]
    [Tooltip("可选：留空则默认用本对象自身作为面板根。")]
    public GameObject panelObj;

    [Header("主视觉")]
    public Image itemIcon;
    public Button btnExit;
    public List<Sprite> btnExits;
    public Image decorationFrame;
    public List<Sprite> decorationFrames;
    public Image rarityTag;
    public List<Sprite> rarityTags;
    public Image background;
    public List<Sprite> backgrounds;

    [Header("信息区")]
    public GameObject itemNumBG;
    public TMP_Text itemNum;
    public TMP_Text itemName;
    public TMP_Text itemSellPrice;
    public TMP_Text itemDescription;

    [Header("数量调整")]
    public Button btnMinus;
    public Button btnPlus;
    public Image quantityBG;
    public TMP_Text quantityText;

    [Header("确认回收")]
    [Tooltip("点击后按当前 quantity 扣库存 + 返还鱼币 (sell_price × quantity)。")]
    public Button btnConfirm;

    [Header("出售数量展示（可选）")]
    [Tooltip("可选：与 quantityText 同步显示当前选择的出售数量。")]
    public TMP_Text sellQuantityText;

    private Game_Item_In_Inventory _currentItem;
    private int _currentQuantity = 1;
    private int _unitPrice;
    private int _maxQuantity;

    private void Awake()
    {
        if (panelObj == null) panelObj = gameObject;
        if (panelObj != gameObject) panelObj.SetActive(false);
    }

    private void Start()
    {
        btnExit.onClick.AddListener(ClosePanel);
        btnMinus.onClick.AddListener(OnMinus);
        btnPlus.onClick.AddListener(OnPlus);
        btnConfirm.onClick.AddListener(TryConfirmRecycle);
    }

    private void OnDestroy()
    {
        btnExit.onClick.RemoveAllListeners();
        btnMinus.onClick.RemoveAllListeners();
        btnPlus.onClick.RemoveAllListeners();
        btnConfirm.onClick.RemoveAllListeners();
    }

    /// <summary>打开弹窗并绑定物品数据，数量初始值 = 1。</summary>
    public void Open(Game_Item_In_Inventory item)
    {
        if (item == null || item.item_info == null)
        {
            Debug.LogWarning("[RecycleSellPopup] Open called with null item.");
            return;
        }

        _currentItem = item;
        _unitPrice = Mathf.Max(0, item.item_info.sell_price);
        _maxQuantity = Mathf.Max(1, item._item_count);
        _currentQuantity = 1;

        if (!gameObject.activeSelf) gameObject.SetActive(true);
        panelObj.SetActive(true);

        if (!string.IsNullOrEmpty(item.item_info.res_url))
        {
            var parts = item.item_info.res_url.Split("#");
            // itemIcon 判空防"弹窗已销毁但 sprite 迟到送达"，不是字段兜底。
            if (parts.Length != 2)
            {
                Project_Mouse_Resource_Management.Load_Sprite(parts[0], (Sprite icon) => { if (itemIcon != null) itemIcon.sprite = icon; });
            }
            else
            {
                Project_Mouse_Resource_Management.load_sub_sprite(parts[0], parts[1], (Sprite icon) => { if (itemIcon != null) itemIcon.sprite = icon; });
            }
        }

        int rarityIdx = Mathf.Max(0, (int)item.item_info.rarity - 1);

        TrySetRaritySprite(btnExit.GetComponent<Image>(), btnExits, rarityIdx);
        TrySetRaritySprite(decorationFrame, decorationFrames, rarityIdx);
        TrySetRaritySprite(rarityTag, rarityTags, rarityIdx);
        TrySetRaritySprite(background, backgrounds, rarityIdx);

        itemNumBG.SetActive(true);
        itemNum.gameObject.SetActive(true);
        itemNum.text = item._item_count.ToString();
        itemName.text = item.item_name;
        itemDescription.text = item.item_info.desc;

        RefreshQuantityUI();
    }

    public void ClosePanel()
    {
        panelObj.SetActive(false);
        _currentItem = null;
    }

    private void OnMinus()
    {
        if (_currentQuantity > 1)
        {
            _currentQuantity--;
            RefreshQuantityUI();
        }
    }

    private void OnPlus()
    {
        if (_currentQuantity < _maxQuantity)
        {
            _currentQuantity++;
            RefreshQuantityUI();
        }
    }

    private void RefreshQuantityUI()
    {
        string qtyStr = _currentQuantity.ToString();
        quantityText.text = qtyStr;
        if (sellQuantityText != null) sellQuantityText.text = qtyStr;
        itemSellPrice.text = (_unitPrice * _currentQuantity).ToString();
    }

    /// <summary>当前选择的回收数量。</summary>
    public int CurrentQuantity => _currentQuantity;
    /// <summary>当前弹窗绑定的物品。</summary>
    public Game_Item_In_Inventory CurrentItem => _currentItem;

    /// <summary>
    /// "确认回收" 按钮入口：参数校验通过后弹 <see cref="PromptMessage"/> 二次确认，
    /// 确认回调才会走到 <see cref="DoRecycle"/>。
    /// </summary>
    public void TryConfirmRecycle()
    {
        if (_currentItem == null)
        {
            Debug.Log("[RecycleSellPopup] No item selected; ignore.");
            return;
        }
        if (_currentQuantity <= 0) return;

        string itemName = _currentItem.item_name;
        var latest = Global_Inventory_Manager.GetItem(itemName);
        int haveCount = latest != null ? latest._item_count : 0;
        if (haveCount < _currentQuantity)
        {
            Debug.LogWarning($"[RecycleSellPopup] {itemName} 实际持有 {haveCount} < 请求 {_currentQuantity}，取消回收。");
            return;
        }

        int coins = _unitPrice * _currentQuantity;
        string message = $"是否回收 {itemName} x{_currentQuantity}，获得 {coins} 鱼币？";
        PromptMessage.Instance.ShowPrompt(message, () => DoRecycle(itemName, _currentQuantity, coins));
    }

    /// <summary>
    /// 真正执行回收：扣库存 + 返鱼币（<c>Global_Inventory_Manager.Change_Items_Count</c>，
    /// "鱼币" 内部路由到 <c>MoneyManager</c>），完成后派发 <c>ReloadRecycleData</c> + 关弹窗。
    /// </summary>
    private void DoRecycle(string itemName, int quantity, int coins)
    {
        // 二次确认期间持有数量可能变化，再校验一次防止超扣。
        var latest = Global_Inventory_Manager.GetItem(itemName);
        int haveCount = latest != null ? latest._item_count : 0;
        if (haveCount < quantity)
        {
            Debug.LogWarning($"[RecycleSellPopup] 二次确认时 {itemName} 实际持有 {haveCount} < 请求 {quantity}，取消回收。");
            return;
        }

        var changes = new List<(string, int)>
        {
            (itemName, -quantity),
            ("鱼币", coins),
        };
        Global_Inventory_Manager.Change_Items_Count(changes, "回收箱");

        Debug.Log($"[RecycleSellPopup] Recycle '{itemName}' x{quantity} -> +{coins} 鱼币");

        EvtDsp.TriggerEvt(EvtNames.ReloadRecycleData);
        ClosePanel();
    }

    private static void TrySetRaritySprite(Image target, List<Sprite> sprites, int idx)
    {
        if (sprites == null || sprites.Count == 0) return;
        int i = Mathf.Clamp(idx, 0, sprites.Count - 1);
        target.sprite = sprites[i];
    }
}
