//using CLIP.Framework_Core.Network;
//using CLIP.Framework_Core.Serialization;
//using CLIP.Framework_Unity;
//using CLIP.Project_Mouse.Game_Play_System;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using UnityEngine;

//public class Planting_System_Receiver : SingletonMono<Planting_System_Receiver>, IMsg_Receiver
//{
//	private Planting_System_Manager _plantingManager;
//	private NetWork_Center_WSS _networkCenter;
//	private Player_Social_Manager _socialManager;

//	private const string ReceiverName = "Planting_System_Receiver";
//	private const string DataKey = "Planting_Data";

//	#region Unity Life Cycle

//	private void Start()
//	{
//		_plantingManager = GetComponent<Planting_System_Manager>();
//		if (_plantingManager == null)
//		{
//			Debug.LogError($"{ReceiverName}: Planting_Manager not found");
//			return;
//		}

//		_networkCenter = NetWork_Center_WSS.instance;
//		if (_networkCenter == null)
//		{
//			Debug.LogError($"{ReceiverName}: NetworkCenter not initialized");
//			return;
//		}

//		_socialManager = Player_Social_Manager._instance;

//		BindUnityEvents();
//		RegisterToNetwork();

//		Debug.Log($"{ReceiverName} initialized on {gameObject.name}");
//	}

//	protected override void OnDestroy()
//	{
//		base.OnDestroy();
//		UnbindUnityEvents();
//		UnregisterFromNetwork();
//	}

//	#endregion

//	#region UnityEvent Binding

//	private void BindUnityEvents()
//	{
//		if (_plantingManager != null)
//		{
//			_plantingManager._update_data_from_server.AddListener(UpdateDataFromServer);
//			_plantingManager._upload_data_to_server.AddListener(UploadDataToServer);
//		}
//	}

//	private void UnbindUnityEvents()
//	{
//		if (_plantingManager != null)
//		{
//			_plantingManager._update_data_from_server.RemoveListener(UpdateDataFromServer);
//			_plantingManager._upload_data_to_server.RemoveListener(UploadDataToServer);
//		}
//	}

//	#endregion

//	#region Network Send Helpers

//	private void SendMsg(string action, string detail)
//	{
//		if (_networkCenter == null || !_networkCenter._connect_to_player_server)
//			return;

//		Network_Msg msg = new Network_Msg
//		{
//			sender = ReceiverName,
//			action_target = "Player_Server",
//			action = action,
//			detail_info = detail,
//			_sending_mode = Msg_Sending_Mode.Client_to_Server
//		};

//		_networkCenter.send_via_wss(msg);
//	}

//	#endregion

//	#region Client -> Server Methods

//	private void UpdateDataFromServer()
//	{
//		if (_plantingManager == null)
//		{
//			Log.Info($"{ReceiverName}: Planting_Manager is not initialized");
//			return;
//		}

//		bool isLoadFriend = false;
//		string friendName = string.Empty;

//		if (_socialManager != null)
//		{
//			friendName = _socialManager._next_visit_room_friend_name;
//			isLoadFriend = _socialManager._on_visit_friend_room;
//		}

//		if (isLoadFriend)
//		{
//			Log.Info($"on visit friend roomData - planting system");

//			// 发送JSON数组：[friendName, dataKey]
//			string jsonArray = $"[\"{friendName}\",\"{DataKey}\"]";
//			SendMsg("Get_Friend_Data", jsonArray);
//			return;
//		}

//		Log.Info($"{ReceiverName} Update_Data_From_Server");
//		SendMsg("Get_Data", DataKey);
//	}

//	private void UploadDataToServer()
//	{
//		if (_plantingManager == null || _plantingManager._level_Info == null)
//		{
//			Log.Info($"{ReceiverName}: Planting data is not initialized");
//			return;
//		}

//		bool isLoadFriend = false;
//		string friendName = string.Empty;

//		if (_socialManager != null)
//		{
//			friendName = _socialManager._next_visit_room_friend_name;
//			isLoadFriend = _socialManager._on_visit_friend_room;
//		}

//		// 序列化种植系统数据
//		string serializedData = Serialization_Provider.SerializeObject(_plantingManager._level_Info);

//		if (isLoadFriend)
//		{
//			Log.Info($"Upload_Data_To_Server - planting system for friend: {friendName}");

//			// 发送JSON数组：[friendName, dataKey, serializedData]
//			string jsonArray = $"[\"{friendName}\",\"{DataKey}\",\"{serializedData}\"]";
//			SendMsg("Save_Friend_Data", jsonArray);
//			return;
//		}

//		// 发送JSON数组：[dataKey, serializedData]
//		string jsonArrayNormal = $"[\"{DataKey}\",\"{serializedData}\"]";
//		SendMsg("Save_Data", jsonArrayNormal);
//	}

//	#endregion

//	#region IMsg_Receiver (Server -> Client)

//	public string get_receiver_name()
//	{
//		return ReceiverName;
//	}

//	public void receive_msg(Network_Msg msg)
//	{
//		if (_plantingManager == null)
//			return;
//        Log.Custom($"Receive Message : action = {msg.action},detail = {msg.detail_info}", ReceiverName);

//        switch (msg.action)
//		{
//			case "Response_Data":
//				HandleResponseData(msg.detail_info);
//				break;

//			default:
//				Debug.LogWarning($"{ReceiverName}: Unknown action {msg.action}");
//				break;
//		}

//		Msg_Dispatcher._instance.set_locking(false);
//		Log.Info($"in {ReceiverName} receive_msg_OK");
//	}

//	private void HandleResponseData(string detailInfo)
//	{
//		try
//		{
//			// 使用提供的JSON解析方式
//			var data = Serialization_Provider.DeserializeObject<string[]>(detailInfo);

//			if (data != null && data.Length == 2)
//			{
//				if (data[0] == DataKey)
//				{
//					_plantingManager.load_current_state_from_json(data[1]);
//					Debug.Log($"{ReceiverName}: Planting data loaded successfully");
//				}
//			}
//		}
//		catch (System.Exception ex)
//		{
//			Debug.LogError($"{ReceiverName}: Failed to parse response data - {ex.Message}");
//		}
//	}

//	#endregion

//	#region Network Register

//	private void RegisterToNetwork()
//	{
//		_networkCenter?.Add_Receiver(ReceiverName, this);
//	}

//	private void UnregisterFromNetwork()
//	{
//		_networkCenter?.Remove_Receiver(ReceiverName);
//	}

//	#endregion

//	#region Public Methods

//	// 提供外部调用的方法来更新种植系统数据
//	public void RequestUpdateData()
//	{
//		UpdateDataFromServer();
//	}

//	// 提供外部调用的方法来上传种植系统数据
//	public void RequestUploadData()
//	{
//		UploadDataToServer();
//	}

//	#endregion
//}