#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CLIP.ProjectMouse.Editor.AgentTools
{
    [InitializeOnLoad]
    public static class UnityAgentCommandRunner
    {
        static readonly string QueueRoot = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "UnityAgent");
        static readonly string Inbox = Path.Combine(QueueRoot, "inbox");
        static readonly string Processing = Path.Combine(QueueRoot, "processing");
        static readonly string Outbox = Path.Combine(QueueRoot, "outbox");
        static readonly string Failed = Path.Combine(QueueRoot, "failed");
        static double nextPollTime;
        static bool busy;

        static UnityAgentCommandRunner()
        {
            EnsureDirs();
            EditorApplication.update += Poll;
        }

        [MenuItem("Tools/AI Tools/Unity Agent/Process Pending Commands")]
        public static void ProcessPendingCommandsMenu() => ProcessOne();

        [MenuItem("Tools/AI Tools/Unity Agent/Open Queue Folder")]
        public static void OpenQueueFolder()
        {
            EnsureDirs();
            EditorUtility.RevealInFinder(QueueRoot);
        }

        static void Poll()
        {
            if (busy || EditorApplication.timeSinceStartup < nextPollTime) return;
            nextPollTime = EditorApplication.timeSinceStartup + 0.25d;
            ProcessOne();
        }

        static void ProcessOne()
        {
            EnsureDirs();
            var src = Directory.GetFiles(Inbox, "*.json").OrderBy(File.GetCreationTimeUtc).FirstOrDefault();
            if (string.IsNullOrEmpty(src)) return;
            busy = true;
            var file = Path.GetFileName(src);
            var work = Path.Combine(Processing, file);
            try
            {
                if (File.Exists(work)) File.Delete(work);
                File.Move(src, work);
                var req = JsonConvert.DeserializeObject<Req>(File.ReadAllText(work));
                if (req == null || string.IsNullOrWhiteSpace(req.id)) throw new InvalidOperationException("Invalid UnityAgent request.");
                WriteJson(Path.Combine(Outbox, req.id + ".json"), Execute(req));
                File.Delete(work);
            }
            catch (Exception ex)
            {
                var id = Path.GetFileNameWithoutExtension(file);
                WriteJson(Path.Combine(Failed, id + ".json"), Res.Fail(id, string.Empty, ex));
                if (File.Exists(work)) File.Delete(work);
            }
            finally { busy = false; }
        }

        static Res Execute(Req req)
        {
            var res = new Res { id = req.id, command = req.command, ok = true, startedAt = DateTime.UtcNow.ToString("O") };
            try
            {
                res.result = req.command switch
                {
                    "ping" => new { message = "pong", unityVersion = Application.unityVersion, projectPath = Directory.GetCurrentDirectory(), EditorApplication.isCompiling, EditorApplication.isPlaying },
                    "console.get" => ConsoleGet(req.args),
                    "compile.check" => CompileCheck(req.args),
                    "mcp.invoke" => McpInvoke(req.args),
                    _ => throw new NotSupportedException("Unsupported UnityAgent command: " + req.command)
                };
            }
            catch (Exception ex)
            {
                res.ok = false;
                res.error = Err.From(ex);
            }
            res.finishedAt = DateTime.UtcNow.ToString("O");
            return res;
        }

        static object ConsoleGet(JObject args)
        {
            var entries = ReadConsole(Math.Max(1, Int(args, "maxEntries", 100)), Str(args, "logType", string.Empty), Bool(args, "includeStackTrace", false));
            return new { count = entries.Count, entries };
        }

        static object CompileCheck(JObject args)
        {
            AssetDatabase.Refresh();
            var includeWarnings = Bool(args, "includeWarnings", false);
            var entries = ReadConsole(Math.Max(1, Int(args, "maxEntries", 100)), string.Empty, Bool(args, "includeStackTrace", false))
                .Where(e => e.type == "Error" || e.type == "Exception" || e.type == "Assert" || (includeWarnings && e.type == "Warning"))
                .ToList();
            return new { EditorApplication.isCompiling, ok = !entries.Any(e => e.type == "Error" || e.type == "Exception" || e.type == "Assert"), count = entries.Count, entries };
        }

        static object McpInvoke(JObject args)
        {
            var toolName = Str(args, "toolName", string.Empty);
            if (string.IsNullOrWhiteSpace(toolName)) throw new ArgumentException("mcp.invoke requires toolName.");
            var argumentsJson = Str(args, "argumentsJson", "{}");

            var editorType = Type.GetType("com.IvanMurzak.Unity.MCP.UnityMcpPluginEditor, com.IvanMurzak.Unity.MCP.Editor")
                ?? throw new InvalidOperationException("Cannot find UnityMcpPluginEditor type. Is com.ivanmurzak.unity.mcp compiled?");
            var hasInstance = Convert.ToBoolean(editorType.GetProperty("HasInstance", BindingFlags.Static | BindingFlags.Public)?.GetValue(null));
            if (!hasInstance) throw new InvalidOperationException("UnityMcpPluginEditor is not initialized.");
            var plugin = editorType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)?.GetValue(null)
                ?? throw new InvalidOperationException("UnityMcpPluginEditor.Instance is null.");
            var tools = plugin.GetType().GetProperty("Tools", BindingFlags.Instance | BindingFlags.Public)?.GetValue(plugin)
                ?? throw new InvalidOperationException("Unity MCP tool manager is not initialized.");

            var jsonElementType = typeof(JsonElement);
            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), jsonElementType);
            var parameters = System.Text.Json.JsonSerializer.Deserialize(argumentsJson, dictionaryType)
                ?? Activator.CreateInstance(dictionaryType);
            var requestType = Type.GetType("com.IvanMurzak.McpPlugin.Common.Model.RequestCallTool, McpPlugin.Common")
                ?? throw new InvalidOperationException("Cannot find RequestCallTool type.");
            var request = Activator.CreateInstance(requestType, toolName, parameters)
                ?? throw new InvalidOperationException("Cannot create RequestCallTool.");

            try
            {
                var runMethod = tools.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(method => method.Name == "RunCallTool" && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType.IsAssignableFrom(requestType))
                    ?? throw new InvalidOperationException("Cannot find compatible Tools.RunCallTool.");
                var task = runMethod.Invoke(tools, new[] { request })
                    ?? throw new InvalidOperationException("RunCallTool returned null.");
                var result = task.GetType().GetProperty("Result")?.GetValue(task);
                var json = System.Text.Json.JsonSerializer.Serialize(result);
                return JsonConvert.DeserializeObject<object>(json);
            }
            finally
            {
                if (request is IDisposable disposable) disposable.Dispose();
            }
        }

        static List<ConsoleEntry> ReadConsole(int max, string filter, bool stack)
        {
            var entriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor");
            var entryType = Type.GetType("UnityEditor.LogEntry,UnityEditor");
            if (entriesType == null || entryType == null) return new List<ConsoleEntry> { new ConsoleEntry { type = "Error", message = "Unable to reflect UnityEditor.LogEntries." } };
            var start = entriesType.GetMethod("StartGettingEntries", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var end = entriesType.GetMethod("EndGettingEntries", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var get = entriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var countMethod = entriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (start == null || end == null || get == null || countMethod == null) return new List<ConsoleEntry>();
            var entry = Activator.CreateInstance(entryType);
            var message = entryType.GetField("message", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var trace = entryType.GetField("stackTrace", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var modeField = entryType.GetField("mode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var count = Convert.ToInt32(countMethod.Invoke(null, null));
            var result = new List<ConsoleEntry>();
            start.Invoke(null, null);
            try
            {
                for (var i = count - 1; i >= 0 && result.Count < max; i--)
                {
                    if (!(bool)get.Invoke(null, new[] { (object)i, entry })) continue;
                    var type = TypeFromMode(modeField != null ? Convert.ToInt32(modeField.GetValue(entry)) : 0);
                    if (!string.IsNullOrEmpty(filter) && !string.Equals(type, filter, StringComparison.OrdinalIgnoreCase)) continue;
                    result.Add(new ConsoleEntry { type = type, message = message != null ? Convert.ToString(message.GetValue(entry)) : string.Empty, stackTrace = stack && trace != null ? Convert.ToString(trace.GetValue(entry)) : string.Empty });
                }
            }
            finally { end.Invoke(null, null); }
            return result;
        }

        static string TypeFromMode(int mode)
        {
            const int Error = 1 << 0;
            const int Assert = 1 << 1;
            const int Log = 1 << 2;
            const int Fatal = 1 << 4;
            const int AssetImportError = 1 << 6;
            const int AssetImportWarning = 1 << 7;
            const int ScriptingError = 1 << 8;
            const int ScriptingWarning = 1 << 9;
            const int ScriptingLog = 1 << 10;
            const int ScriptCompileError = 1 << 11;
            const int ScriptCompileWarning = 1 << 12;

            if ((mode & (Error | Fatal | AssetImportError | ScriptingError | ScriptCompileError)) != 0) return "Error";
            if ((mode & Assert) != 0) return "Assert";
            if ((mode & (AssetImportWarning | ScriptingWarning | ScriptCompileWarning)) != 0) return "Warning";
            if ((mode & (Log | ScriptingLog)) != 0) return "Log";
            return "Log";
        }

        static void EnsureDirs()
        {
            Directory.CreateDirectory(Inbox); Directory.CreateDirectory(Processing); Directory.CreateDirectory(Outbox); Directory.CreateDirectory(Failed);
        }

        static void WriteJson(string path, object data)
        {
            var tmp = path + ".tmp";
            File.WriteAllText(tmp, JsonConvert.SerializeObject(data, Formatting.Indented));
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }

        static string Str(JObject args, string name, string def) => args != null && args.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var t) ? t.ToString() : def;
        static int Int(JObject args, string name, int def) => args != null && args.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var t) && t.Type != JTokenType.Null ? t.Value<int>() : def;
        static bool Bool(JObject args, string name, bool def) => args != null && args.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var t) && t.Type != JTokenType.Null ? t.Value<bool>() : def;

        [Serializable] sealed class Req { public string id; public string command; public JObject args; }
        [Serializable] sealed class Res { public string id; public string command; public bool ok; public object result; public Err error; public string startedAt; public string finishedAt; public static Res Fail(string id, string command, Exception ex) => new Res { id = id, command = command, ok = false, error = Err.From(ex), finishedAt = DateTime.UtcNow.ToString("O") }; }
        [Serializable] sealed class Err { public string type; public string message; public string stackTrace; public static Err From(Exception ex) => new Err { type = ex.GetType().FullName, message = ex.Message, stackTrace = ex.ToString() }; }
        [Serializable] sealed class ConsoleEntry { public string type; public string message; public string stackTrace; }
    }
}
#endif
