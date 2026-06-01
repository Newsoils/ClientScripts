using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CLIP.Framework_Unity.Asset;
using Google.Protobuf;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.Network
{
    /// <summary>
    /// Global proto message id map loaded from Resources/all.json.
    /// </summary>
    public static class RegisterMap
    {
        [Serializable]
        class MapEntry
        {
            public int id;
            public string name;
        }

        static readonly object Sync = new object();
        static Dictionary<int, string> _idToName;
        static Dictionary<string, int> _nameToId;
        static Dictionary<string, Type> _nameToType;
        static List<int> _allIds;
        static Dictionary<int, Delegate> _handlers;
        static MethodInfo _jsonParserParseString;

        public static bool IsLoaded { get; private set; }

        public static IReadOnlyList<int> AllMessageIds => _allIds ?? (IReadOnlyList<int>)Array.Empty<int>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LoadMessageMapBeforeFirstScene()
        {
            TryRegisterDefault();
        }

        public static bool TryRegisterDefault()
        {
            lock (Sync)
            {
                if (IsLoaded)
                    return true;

                ResetMaps();
                EnsureMapsInitialized();

                var resourcesPath = GameAssetsPathDefine.ProtoAllJsonResourcesPath;
                var textAsset = Resources.Load<TextAsset>(resourcesPath);
                if (textAsset == null || string.IsNullOrEmpty(textAsset.text))
                {
                    Debug.LogError($"[RegisterMap] all.json not found in Resources path: {resourcesPath}");
                    return false;
                }

                int added;
                try
                {
                    added = MergeEntriesFromText(textAsset.text);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[RegisterMap] parse all.json failed: Resources/{resourcesPath}\n{e}");
                    return false;
                }

                if (added <= 0)
                {
                    Debug.LogError($"[RegisterMap] all.json is empty: Resources/{resourcesPath}");
                    return false;
                }

                FinalizeAfterMerge($"Resources/{resourcesPath}");
                return true;
            }
        }

        static int MergeEntriesFromText(string text)
        {
            var root = JsonConvert.DeserializeObject<Dictionary<string, List<MapEntry>>>(text);
            if (root == null || root.Count == 0)
                return 0;

            int added = 0;
            foreach (var kv in root)
            {
                if (kv.Value == null)
                    continue;

                foreach (var e in kv.Value)
                {
                    if (e == null || string.IsNullOrEmpty(e.name))
                        continue;

                    if (_nameToId.TryGetValue(e.name, out var existingId))
                    {
                        if (existingId != e.id)
                            Debug.LogWarning($"[RegisterMap] duplicate name skipped: {e.name}, old id={existingId}, new id={e.id}");
                        continue;
                    }

                    if (_idToName.TryGetValue(e.id, out var existingName))
                    {
                        if (existingName != e.name)
                            Debug.LogWarning($"[RegisterMap] duplicate id skipped: {e.id}, old name={existingName}, new name={e.name}");
                        continue;
                    }

                    _idToName[e.id] = e.name;
                    _nameToId[e.name] = e.id;
                    added++;
                }
            }

            return added;
        }

        static void EnsureMapsInitialized()
        {
            _idToName ??= new Dictionary<int, string>();
            _nameToId ??= new Dictionary<string, int>(StringComparer.Ordinal);
            _nameToType ??= new Dictionary<string, Type>(StringComparer.Ordinal);
        }

        static void ResetMaps()
        {
            _idToName = null;
            _nameToId = null;
            _nameToType = null;
            _allIds = null;
            IsLoaded = false;
        }

        static void FinalizeAfterMerge(string sourceHint)
        {
            _allIds = new List<int>(_idToName.Keys);
            _allIds.Sort();
            RegisterMessageTypes();
            IsLoaded = true;
            Debug.Log($"[RegisterMap] loaded {_idToName.Count} entries ({sourceHint})");
        }

        static void RegisterMessageTypes()
        {
            foreach (var name in _nameToId.Keys)
            {
                var type = ResolveMessageType(name);
                if (type != null)
                    _nameToType[name] = type;
            }
        }

        static Type ResolveMessageType(string csharpMessageName)
        {
            var type = Type.GetType(csharpMessageName);
            if (type != null)
                return type;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    type = asm.GetType(csharpMessageName);
                    if (type != null)
                        return type;
                }
                catch
                {
                    // Some dynamic assemblies can throw while resolving types.
                }
            }

            return null;
        }

        public static bool TryGetName(int id, out string name)
        {
            name = null;
            return _idToName != null && _idToName.TryGetValue(id, out name);
        }

        public static bool TryGetId(string fullTypeName, out int id)
        {
            id = 0;
            return _nameToId != null && _nameToId.TryGetValue(fullTypeName, out id);
        }

        public static int GetMsgIdForType(Type messageType)
        {
            if (messageType == null || _nameToId == null)
                return 0;

            return _nameToId.TryGetValue(messageType.FullName ?? messageType.Name, out var id) ? id : 0;
        }

        public static void RegisterHandler(object res, object handler)
        {
            if (handler is not Delegate del)
            {
                Debug.LogError("[RegisterMap] RegisterHandler: handler must be a Delegate.");
                return;
            }

            var messageType = ResolveMessageTypeFromRes(res);
            if (messageType == null)
            {
                Debug.LogError("[RegisterMap] RegisterHandler: can not resolve message type from res.");
                return;
            }

            if (!typeof(IMessage).IsAssignableFrom(messageType))
            {
                Debug.LogError($"[RegisterMap] RegisterHandler: {messageType.FullName} does not implement IMessage.");
                return;
            }

            var nameKey = messageType.FullName ?? messageType.Name;
            lock (Sync)
            {
                if (_nameToId == null || !_nameToId.TryGetValue(nameKey, out var msgId))
                {
                    Debug.LogError($"[RegisterMap] RegisterHandler: message name not found in all.json: {nameKey}");
                    return;
                }

                var ps = del.Method.GetParameters();
                if (ps.Length != 1)
                {
                    Debug.LogError($"[RegisterMap] RegisterHandler: handler must accept exactly 1 parameter, actual={ps.Length}.");
                    return;
                }

                var p0 = ps[0].ParameterType;
                var validParameterType = p0 == typeof(object)
                    || p0 == typeof(IMessage)
                    || p0.IsAssignableFrom(messageType);
                if (!validParameterType)
                {
                    Debug.LogError($"[RegisterMap] RegisterHandler: parameter {p0.FullName} can not accept {messageType.FullName}.");
                    return;
                }

                _handlers ??= new Dictionary<int, Delegate>();
                if (_handlers.ContainsKey(msgId))
                    Debug.LogWarning($"[RegisterMap] RegisterHandler: duplicate msg_id={msgId} [{nameKey}], overwritten.");

                _handlers[msgId] = del;
            }
        }

        public static void ClearHandlers()
        {
            lock (Sync)
            {
                _handlers = null;
            }
        }

        public static bool TryInvokeHandler(int msgId, IMessage message)
        {
            if (message == null || _handlers == null)
                return false;

            Delegate del;
            lock (Sync)
            {
                if (!_handlers.TryGetValue(msgId, out del) || del == null)
                    return false;
            }

            try
            {
                var p0 = del.Method.GetParameters()[0].ParameterType;
                var runtimeType = message.GetType();
                if (!p0.IsAssignableFrom(runtimeType))
                {
                    Debug.LogError($"[RegisterMap] TryInvokeHandler: parameter expects {p0.FullName}, actual={runtimeType.FullName}");
                    return false;
                }

                del.DynamicInvoke(message);
                return true;
            }
            catch (Exception e)
            {
                _ = TryGetName(msgId, out var nameForLog);
                Debug.LogError($"[RegisterMap] TryInvokeHandler id={msgId} name={nameForLog}\n{e}");
                return false;
            }
        }

        static Type ResolveMessageTypeFromRes(object res)
        {
            if (res == null)
                return null;
            if (res is Type type)
                return type;
            return res.GetType();
        }

        public static bool TryGetMessageType(int id, out Type type)
        {
            type = null;
            if (!TryGetName(id, out var name) || _nameToType == null)
                return false;

            if (_nameToType.TryGetValue(name, out type) && type != null)
                return true;

            type = ResolveMessageType(name);
            if (type != null)
                _nameToType[name] = type;

            return type != null;
        }

        public static bool TryParseProtoJson(int msgId, string json, out IMessage message)
        {
            message = null;
            if (string.IsNullOrEmpty(json))
                return false;
            if (!TryGetMessageType(msgId, out var type))
                return false;
            if (!typeof(IMessage).IsAssignableFrom(type))
                return false;

            try
            {
                _jsonParserParseString ??= typeof(JsonParser)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .First(m => m.Name == "Parse"
                        && m.IsGenericMethodDefinition
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType == typeof(string));

                var parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
                var generic = _jsonParserParseString.MakeGenericMethod(type);
                message = (IMessage)generic.Invoke(parser, new object[] { json });
                return message != null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RegisterMap] TryParseProtoJson id={msgId} err={e.Message}");
                return false;
            }
        }

#if UNITY_EDITOR
        public static void Editor_ResetForTests()
        {
            lock (Sync)
            {
                IsLoaded = false;
                _idToName = null;
                _nameToId = null;
                _nameToType = null;
                _allIds = null;
                _handlers = null;
                _jsonParserParseString = null;
            }
        }
#endif
    }
}
