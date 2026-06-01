using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 纯逻辑类，封装主角交互选择（从交互表中筛选可执行交互）。不继承 MonoBehaviour。
    /// </summary>
    public class InteractSelector
    {
        private readonly CharacterNavigator _navigator;
        private readonly InteractPositionSO _interactPositions;
        private List<InteractInfo> _interactInfos;
        private Dictionary<InteractInfo, MainCharacterInteractState> _interactStates;

        public IReadOnlyList<InteractInfo> InteractInfos => _interactInfos;
        public IReadOnlyDictionary<InteractInfo, MainCharacterInteractState> InteractStates => _interactStates;

        public InteractSelector(CharacterNavigator navigator, InteractPositionSO interactPositions)
        {
            _navigator = navigator;
            _interactPositions = interactPositions;
        }

        public void InitInteract(StateMachine stateMachine, IndoorMainCharacter character)
        {
            string json = JsonDataManager.Load_Single_JsonData("project_mouse_tb_interact_info");
            _interactInfos = JsonConvert.DeserializeObject<List<InteractInfo>>(json);
            _interactStates = new Dictionary<InteractInfo, MainCharacterInteractState>();
            foreach (var info in _interactInfos)
            {
                _interactStates.Add(info, new MainCharacterInteractState(stateMachine, character, info));
            }
        }

        /// <summary>
        /// 从所有交互中随机选一个当前可执行的交互。
        /// </summary>
        public Interact SelectInteract(Room currentRoom, ref Vector3 liePosition, bool considerFindPath)
        {
            List<Interact> interacts = new List<Interact>();
            for (int i = 0; i < _interactInfos.Count; i++)
            {
                var info = _interactInfos[i];
                if (!CheckRoomLimit(info, currentRoom)) continue;
                if (!CheckPlacementLimit(info, currentRoom, ref liePosition, considerFindPath, out Interact interact)) continue;
                interacts.Add(interact);
            }
            if (interacts.Count > 0)
                return interacts[Random.Range(0, interacts.Count)];
            return null;
        }

        /// <summary>
        /// 对指定家具，筛选所有可执行交互并随机选一个（或按 interactName 精确匹配）。
        /// </summary>
        public Interact InteractSelectedPlacement(PlacementRuntime placement, string interactName)
        {
            List<Interact> interacts = new List<Interact>();
            for (int i = 0; i < _interactInfos.Count; i++)
            {
                if (IsInteractGroundOnly(_interactInfos[i])) continue;
                if (!InteractIncludesPlacementCategory(_interactInfos[i], placement.info.second_Category)) continue;
                if (!_interactPositions.TryGetPositionInfo(placement.Name, _interactInfos[i].interactName, out var data)) continue;
                if (_navigator.TryFindReachablePositionAroundPlacement(placement, true, out var navPos))
                {
                    interacts.Add(Interact.CreateSinglePlacement(navPos, placement, _interactInfos[i], data));
                }
            }
            if (interacts.Count > 0)
            {
                if (string.IsNullOrEmpty(interactName))
                {
                    return interacts[Random.Range(0, interacts.Count)];
                }
                else
                {
                    foreach (var interact in interacts)
                    {
                        if (interact.info.interactName == interactName)
                            return interact;
                    }
                    Debug.LogWarning("该家具没有名为" + interactName + "的可执行交互！");
                    return null;
                }
            }
            else
            {
                Debug.LogWarning("该家具没有任何可执行交互！");
                return null;
            }
        }

        /// <summary>
        /// 对指定花盆，筛选所有可执行交互并随机选一个（或按 interactName 精确匹配）。
        /// </summary>
        public Interact InteractSelectedPot(Pot pot, string interactName)
        {
            if (!_navigator.CanMoveToPot(pot, out Vector3 position))
            {
                Debug.Log("该花盆没有路径前往");
                return null;
            }
            List<Interact> interacts = new List<Interact>();
            for (int i = 0; i < _interactInfos.Count; i++)
            {
                if (InteractIncludesPlacementCategory(_interactInfos[i], Placement_Second_Category.Plant))
                    interacts.Add(Interact.CreateSinglePot(position, pot, _interactInfos[i]));
            }
            if (interacts.Count > 0)
            {
                if (string.IsNullOrEmpty(interactName))
                {
                    return interacts[Random.Range(0, interacts.Count)];
                }
                else
                {
                    foreach (var interact in interacts)
                    {
                        if (interact.info.interactName == interactName)
                            return interact;
                    }
                    Debug.LogWarning("该盆栽没有名为" + interactName + "的可执行交互！");
                    return null;
                }
            }
            else
            {
                Debug.LogWarning("该盆栽没有任何可执行交互！");
                return null;
            }
        }

        #region 内部判断

        private static bool IsInteractGroundOnly(InteractInfo info)
        {
            return info.placementLimit == null || info.placementLimit.Count == 0;
        }

        private static bool InteractIncludesPlacementCategory(InteractInfo info, Placement_Second_Category category)
        {
            return info.placementLimit != null && info.placementLimit.Contains(category);
        }

        private bool CheckRoomLimit(InteractInfo info, Room currentRoom)
        {
            if (info.roomLimit == null || info.roomLimit.Count == 0) return true;
            foreach (var room in info.roomLimit)
            {
                if (currentRoom != null && room == currentRoom.RoomName) return true;
            }
            return false;
        }

        private bool CheckPlacementLimit(InteractInfo info, Room currentRoom, ref Vector3 liePosition, bool considerFindPath, out Interact interact)
        {
            interact = null;
            if (IsInteractGroundOnly(info))
            {
                if (liePosition != Vector3.zero)
                {
                    interact = Interact.CreateLie(liePosition, info);
                    return true;
                }
                if (_navigator.CanMoveToRandomPosToLie(currentRoom, considerFindPath, out var newLiePos))
                {
                    liePosition = newLiePos;
                    interact = Interact.CreateLie(liePosition, info);
                    return true;
                }
                return false;
            }

            if (currentRoom == null) return false;

            var agg = new Interact { isLie = false, info = info };

            if (InteractIncludesPlacementCategory(info, Placement_Second_Category.Plant) && currentRoom.RoomType == RoomType.Balcony && PlantManager.Instance != null)
            {
                foreach (var pot in currentRoom.pots)
                {
                    if (_navigator.CanMoveToPot(pot, out Vector3 position, considerFindPath))
                        agg.AddCandidate(position, null, pot, null);
                }
            }

            foreach (var placement in currentRoom.placements)
            {
                if (!InteractIncludesPlacementCategory(info, placement.info.second_Category))
                    continue;
                if (!_interactPositions.TryGetPositionInfo(placement.Name, info.interactName, out var data))
                {
                    Debug.LogWarning("该家具缺少交互点！已跳过该家具的交互判定");
                    continue;
                }
                if (_navigator.TryFindReachablePositionAroundPlacement(placement, considerFindPath, out var navPos))
                {
                    agg.AddCandidate(navPos, placement, null, data);
                }
            }

            if (agg.positions.Count == 0) return false;
            agg.ChooseRandomCandidate();
            interact = agg;
            return true;
        }

        #endregion
    }
}
