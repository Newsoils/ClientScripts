using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using Cmd;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class MoneyManager : SingletonMono<MoneyManager>
    {
        /// <summary>
        /// ID查找钱币信息(注意!这里是game_item的ID并非currencyID)
        /// </summary>
        public Dictionary<int, GameCurrency> idDic;
        public Dictionary<string, GameCurrency> nameDic;
        public int curCoin => currencyData["鱼币"];
        public int curDiamond => currencyData["罐罐"];
        public Dictionary<string, int> currencyData = new Dictionary<string, int>
        {
            {"鱼币", 0 },
            {"罐罐", 0 }
        };
                
        protected override void Awake()
        {
            DontDestroyOnLoad(this);
        }
        private void Start()
        {
            JsonDataManager.Load_Currency_Data(out idDic, out nameDic);

            //EvtDsp.AddEvt<int>(EvtNames.GetCoin, (int changeAmount) => _ = ChangeCoin(changeAmount));
            //EvtDsp.AddEvt<int>(EvtNames.GetDiamond, (int changeAmount) => _ = ChangeDiamond(changeAmount));
            StartCoroutine(Load());
        }
        //这里只是为了等待网络中心初始化完成的临时方案
        IEnumerator Load()
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            LoadCurrency();
        }
        private async Task Test()
        {
            // TODO zhaorui2
            // await EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "ChangeCurrencyMulti", ChangeCurrencyTask("123",123,""), null);
            Debug.Log("123");
            await Task.CompletedTask;
        }
        public void LoadCurrency()
        {
            // TODO zhaorui2
            // EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "LoadCurrency", LoadCurrencyTask(), null);
        }
        public void GetMoneyCount(string currencyName)
        {

        }

        public void GetMoneyCount(int currenyID)
        {

        }

        public async Task ChangeCurrency(string currencyName, int amount, string source, Action<string> onTaskComplete = null)
        {
            List<(string, int)> change = new List<(string, int)> { (currencyName, amount) };
            await ChangeCurrencyMulti(change, source, onTaskComplete);
        }


        public void ChangeCurrency(int id,int amount, string source, Action<string> onTaskComplete = null)
        {
            string currencyName = idDic[id].currency_name;
            if (currencyName == null)
            {
                Log.Error($"MoneyMoney：未找到GameItem_ID为{id}的货币"); 
                return;
            }

            _ = ChangeCurrency(currencyName, amount, source, onTaskComplete);
        }

        public async Task ChangeCurrencyMulti(List<(string, int)> change, string source, Action<string> onTaskComplete = null)
        {
            if(change == null || change.Count == 0) return;
            // TODO zhaorui2
            // await EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "ChangeCurrency", ChangeCurrencyMultiTask(change, source), onTaskComplete);
            await Task.CompletedTask;
        }
        public ServerTask LoadCurrencyTask()
        {
            string defaultData = JsonConvert.SerializeObject(new Dictionary<string, int>
            {
                { "鱼币", 0 },
                { "罐罐", 0 }
            });
            ServerTask task = new ServerTask(new Cmd.EmptyReq(), (string data, ServerTask task) =>
            {
                if (data == null || data == "nodata")
                {
                    task.isBreak = true;
                    task.result = "货币数据加载失败";
                    return;
                }
                SaveToLocal(data);
                EvtDsp.TriggerEvt(EvtNames.RefreshUI);
            });
            return task;
        }
        public ServerTask ChangeCurrencyTask(string currencyName, int amount, string source)
        {
            List<(string, int)> change = new List<(string, int)> { (currencyName, amount) };
            return ChangeCurrencyMultiTask(change, source);
        }

        public ServerTask ChangeCurrencyTask(List<(int, int)> change,string source)
        {
            List<(string, int)> nameChange = new List<(string, int)>();
            foreach (var changeItem in change)
            {
                idDic.TryGetValue(changeItem.Item1, out var currency);
                if (currency == null) return null;
                nameChange.Add((currency.currency_name,changeItem.Item2));
            }
            return ChangeCurrencyMultiTask(nameChange, source);
        }

        public ServerTask ChangeCurrencyMultiTask(List<(string, int)> change, string source)
        {
            ServerTask task = new ServerTask(new Cmd.EmptyReq(), (string data, ServerTask task) =>
            {
                if (data == null || data == "nodata")
                {
                    task.isBreak = true;
                    task.result = "修改货币失败";
                    return;
                }
                if(data == "鱼币不足"||data == "罐罐不足")
                {
                    task.isBreak = true;
                    task.result = data;
                    return;
                }
                task.result = "success";
                SaveToLocal(data);
                EvtDsp.TriggerEvt(EvtNames.RefreshUI);
            });
            return task;
        }

       

        public Dictionary<string, int> SaveToLocal(string data)
        {
            return currencyData = JsonConvert.DeserializeObject<Dictionary<string, int>>(data);
        }
    }
}
