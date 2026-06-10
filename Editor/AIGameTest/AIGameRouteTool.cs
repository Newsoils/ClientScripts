#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CLIP.Project_Mouse.AIGameTestTools
{
    [McpForUnityTool(
        "ai_game_route",
        Description = "Restart the game and run one composed test route from Boot to its target UI.",
        Group = "core")]
    public static class AIGameRouteTool
    {
        private const string BootScenePath = "Assets/Scenes/Boot.unity";
        private static EditorApplication.CallbackFunction activeTick;

        public sealed class Parameters
        {
            [ToolParameter("Top-level route id from AI_Tools/AIGameTest/routes, for example login, shopping, or dispatch.")]
            public string route { get; set; }

            [ToolParameter("Optional account used by login subroutes.", Required = false)]
            public string account { get; set; }
        }

        public static Task<object> HandleCommand(JObject parameters)
        {
            var routeId = parameters?["route"]?.ToString();
            if (string.IsNullOrWhiteSpace(routeId))
                return Task.FromResult<object>(new ErrorResponse("'route' is required."));

            if (!TryLoadRoute(routeId, out var route, out var segments, out var error))
                return Task.FromResult<object>(new ErrorResponse(error));

            var account = parameters?["account"]?.ToString() ?? string.Empty;
            if (segments.Any(segment => ContainsAccountParameter(segment)) && string.IsNullOrWhiteSpace(account))
                return Task.FromResult<object>(new ErrorResponse("'account' is required by this route."));

            var history = new JArray();
            var ruleRuns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var startedAt = EditorApplication.timeSinceStartup;
            var maxSeconds = route["maxSeconds"]?.Value<double>() ?? 180d;
            var maxSteps = route["maxSteps"]?.Value<int>() ?? 80;
            var segmentIndex = 0;
            var successStreak = 0;
            var restartRequested = false;
            var playRequested = false;
            string pendingJobId = null;
            string pendingRuleId = null;

            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                try
                {
                    if (EditorApplication.timeSinceStartup - startedAt > maxSeconds || history.Count >= maxSteps)
                    {
                        Finish(new ErrorResponse("Route timed out.", BuildResult(routeId, false, segments, segmentIndex, history)), tick);
                        return;
                    }

                    if (!restartRequested)
                    {
                        restartRequested = true;
                        if (EditorApplication.isPlaying)
                        {
                            EditorApplication.isPlaying = false;
                            return;
                        }
                    }

                    if (!EditorApplication.isPlaying)
                    {
                        if (playRequested)
                            return;

                        EditorSceneManager.OpenScene(BootScenePath);
                        playRequested = true;
                        EditorApplication.isPlaying = true;
                        return;
                    }

                    if (segmentIndex >= segments.Count)
                    {
                        Finish(new SuccessResponse("Route completed.", BuildResult(routeId, true, segments, segmentIndex, history)), tick);
                        return;
                    }

                    var segment = segments[segmentIndex];
                    var actionCompleted = false;
                    if (!string.IsNullOrWhiteSpace(pendingJobId))
                    {
                        var poll = JObject.Parse(AIGameTestDriver.PollVisualActionJson(pendingJobId));
                        if (string.Equals(poll["status"]?.ToString(), "Pending", StringComparison.OrdinalIgnoreCase))
                            return;

                        poll["segment"] = segment["id"];
                        poll["rule"] = pendingRuleId;
                        history.Add(poll);
                        pendingJobId = null;
                        pendingRuleId = null;
                        actionCompleted = true;
                    }

                    if (Matches(segment["success"] as JObject))
                    {
                        successStreak = actionCompleted ? successStreak + 1 : successStreak;
                        var successConfirmRuns = Math.Max(1, segment["successConfirmRuns"]?.Value<int>() ?? 1);
                        if (successStreak >= successConfirmRuns)
                        {
                            segmentIndex++;
                            successStreak = 0;
                            ruleRuns.Clear();
                            if (segmentIndex >= segments.Count)
                            {
                                Finish(new SuccessResponse("Route completed.", BuildResult(routeId, true, segments, segmentIndex, history)), tick);
                                return;
                            }
                            return;
                        }

                        BeginAction(new JObject { ["type"] = "observe" }, "confirm_success", ref pendingJobId, ref pendingRuleId);
                        return;
                    }
                    successStreak = 0;

                    var selected = SelectRule(segment["rules"] as JArray, ruleRuns);
                    var action = selected?["action"]?.DeepClone() as JObject ?? new JObject { ["type"] = "observe" };
                    ReplaceParameters(action, account);

                    var ruleId = selected?["id"]?.ToString() ?? "observe";
                    if (selected != null)
                        ruleRuns[ruleId] = ruleRuns.TryGetValue(ruleId, out var count) ? count + 1 : 1;

                    BeginAction(action, ruleId, ref pendingJobId, ref pendingRuleId);
                }
                catch (Exception exception)
                {
                    Finish(new ErrorResponse("Route failed: " + exception.Message), tick);
                }
            };

            if (activeTick != null)
                EditorApplication.update -= activeTick;
            AIGameTestAutoRefreshGuard.Lock();
            activeTick = tick;
            EditorApplication.update += tick;
            return Task.FromResult<object>(new SuccessResponse(
                "Route scheduled. It will restart the game and continue without another command.",
                new { route = routeId, from = route["from"]?.ToString(), to = route["to"]?.ToString(), segments = segments.Select(segment => segment["id"]?.ToString()).ToArray() }));
        }

        private static void BeginAction(JObject action, string ruleId, ref string pendingJobId, ref string pendingRuleId)
        {
            var begin = JObject.Parse(AIGameTestDriver.BeginVisualActionJson(action.ToString(Formatting.None)));
            pendingJobId = begin["jobId"]?.ToString();
            pendingRuleId = ruleId;
            if (string.IsNullOrWhiteSpace(pendingJobId))
                throw new InvalidOperationException("Route could not start action: " + begin.ToString(Formatting.None));
        }

        private static bool TryLoadRoute(string routeId, out JObject route, out List<JObject> segments, out string error)
        {
            route = null;
            segments = new List<JObject>();
            error = null;

            var routePath = GetDefinitionPath("routes", routeId);
            if (!File.Exists(routePath))
            {
                error = "Route not found: " + routeId;
                return false;
            }

            try
            {
                route = JObject.Parse(File.ReadAllText(routePath));
                var definitions = route["segments"] as JArray;
                if (definitions == null || definitions.Count == 0)
                {
                    error = "Top-level route must contain at least one segment.";
                    return false;
                }

                for (var index = 0; index < definitions.Count; index++)
                {
                    if (definitions[index] is not JObject definition)
                    {
                        error = $"Segment at index {index} must be an object.";
                        return false;
                    }

                    var use = definition["use"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(use))
                    {
                        var subroutePath = GetDefinitionPath("subroutes", use);
                        if (!File.Exists(subroutePath))
                        {
                            error = "Subroute not found: " + use;
                            return false;
                        }

                        var segment = JObject.Parse(File.ReadAllText(subroutePath));
                        var from = segment["from"]?.ToString();
                        var to = segment["to"]?.ToString();
                        if (!string.Equals(from, "main", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(to, "main", StringComparison.OrdinalIgnoreCase))
                        {
                            error = $"Reusable subroute '{use}' must start or end at main.";
                            return false;
                        }
                        segments.Add(segment);
                        continue;
                    }

                    if (definition["inline"] is JObject inline)
                    {
                        var segment = (JObject)inline.DeepClone();
                        if (segment["id"] == null)
                            segment["id"] = "inline_" + (index + 1);
                        segments.Add(segment);
                        continue;
                    }

                    error = $"Segment at index {index} must contain 'use' or 'inline'.";
                    return false;
                }

                var expected = route["from"]?.ToString();
                foreach (var segment in segments)
                {
                    if (!string.Equals(expected, segment["from"]?.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        error = $"Segment chain is broken before '{segment["id"]}'. Expected from='{expected}'.";
                        return false;
                    }
                    expected = segment["to"]?.ToString();
                }

                if (!string.Equals(expected, route["to"]?.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    error = $"Route target mismatch. Expected to='{route["to"]}', actual='{expected}'.";
                    return false;
                }
                return true;
            }
            catch (Exception exception)
            {
                error = "Invalid route JSON: " + exception.Message;
                return false;
            }
        }

        private static JObject SelectRule(JArray rules, Dictionary<string, int> ruleRuns)
        {
            if (rules == null) return null;
            foreach (var token in rules.OfType<JObject>())
            {
                var id = token["id"]?.ToString() ?? string.Empty;
                var maxRuns = token["maxRuns"]?.Value<int>() ?? int.MaxValue;
                var runs = ruleRuns.TryGetValue(id, out var count) ? count : 0;
                if (runs < maxRuns && Matches(token["when"] as JObject))
                    return token;
            }
            return null;
        }

        private static bool Matches(JObject condition)
        {
            if (condition == null) return false;
            var scene = condition["scene"]?.ToString();
            if (!string.IsNullOrWhiteSpace(scene) && SceneManager.GetActiveScene().name != scene)
                return false;

            var scenePrefix = condition["scenePrefix"]?.ToString();
            if (!string.IsNullOrWhiteSpace(scenePrefix) &&
                !SceneManager.GetActiveScene().name.StartsWith(scenePrefix, StringComparison.Ordinal))
                return false;

            var activePath = condition["activePath"]?.ToString();
            if (!string.IsNullOrWhiteSpace(activePath) && !IsPathActive(activePath))
                return false;

            var activeName = condition["activeName"]?.ToString();
            if (!string.IsNullOrWhiteSpace(activeName) && !IsNameActive(activeName))
                return false;

            foreach (var path in condition["inactivePaths"]?.Values<string>() ?? Enumerable.Empty<string>())
            {
                if (IsPathActive(path))
                    return false;
            }
            return true;
        }

        private static bool IsPathActive(string path)
        {
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Any(go => go != null && go.scene.IsValid() && go.activeInHierarchy && PathOf(go.transform) == path);
        }

        private static bool IsNameActive(string name)
        {
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Any(go => go != null && go.scene.IsValid() && go.activeInHierarchy && go.name == name);
        }

        private static string PathOf(Transform transform)
        {
            var names = new List<string>();
            while (transform != null)
            {
                names.Add(transform.name);
                transform = transform.parent;
            }
            names.Reverse();
            return string.Join("/", names);
        }

        private static bool ContainsAccountParameter(JToken token)
        {
            return token.ToString(Formatting.None).Contains("${account}");
        }

        private static void ReplaceParameters(JToken token, string account)
        {
            if (token is JValue value && value.Type == JTokenType.String)
            {
                value.Value = value.ToString().Replace("${account}", account);
                return;
            }

            if (token is not JContainer container)
                return;
            foreach (var child in container.Children().ToList())
                ReplaceParameters(child, account);
        }

        private static object BuildResult(string route, bool success, List<JObject> segments, int segmentIndex, JArray history)
        {
            var last = history.Last as JObject;
            var compactHistory = new JArray(history.OfType<JObject>()
                .Where(item =>
                    !string.Equals(item["rule"]?.ToString(), "observe", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(item["rule"]?.ToString(), "confirm_success", StringComparison.OrdinalIgnoreCase))
                .Select(item => new JObject
                {
                    ["segment"] = item["segment"],
                    ["rule"] = item["rule"],
                    ["success"] = item["success"],
                    ["reason"] = item["reason"],
                    ["scene"] = item["scene"]
                }));
            return new
            {
                route,
                success,
                completedSegments = Math.Min(segmentIndex, segments.Count),
                totalSegments = segments.Count,
                steps = history.Count,
                scene = EditorApplication.isPlaying ? SceneManager.GetActiveScene().name : string.Empty,
                state = last?["state"]?.ToString(),
                screenshot = last?["screenshot"]?.ToString(),
                history = compactHistory
            };
        }

        private static void Finish(object result, EditorApplication.CallbackFunction tick)
        {
            EditorApplication.update -= tick;
            if (activeTick == tick)
                activeTick = null;
            AIGameTestAutoRefreshGuard.UnlockIfNotPlaying();
            WriteLatestResult(result);
            Debug.Log("[AIGameRoute] " + JsonConvert.SerializeObject(result));
        }

        private static void WriteLatestResult(object result)
        {
            var path = GetDefinitionPath("results", "latest");
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            File.WriteAllText(path, JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        private static string GetDefinitionPath(string folder, string id)
        {
            var unityProject = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            var workspace = Directory.GetParent(unityProject)?.FullName ?? unityProject;
            return Path.Combine(workspace, "AI_Tools", "AIGameTest", folder, id + ".json");
        }
    }
}
#endif
