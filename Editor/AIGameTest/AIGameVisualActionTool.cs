#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace CLIP.Project_Mouse.AIGameTestTools
{
    [McpForUnityTool(
        "ai_game_visual_action",
        Description = "Execute one game UI action, wait while Unity advances frames, then return the new state and screenshot.",
        Group = "core")]
    public static class AIGameVisualActionTool
    {
        public sealed class Parameters
        {
            [ToolParameter("Action JSON. Examples: {\"type\":\"click\",\"path\":\"Canvas/Button\"} or {\"type\":\"swipe\",\"startX\":0.75,\"startY\":0.5,\"endX\":0.25,\"endY\":0.5,\"duration\":0.5}.")]
            public string action { get; set; }

            [ToolParameter("Seconds to let Unity advance before capturing the result.", Required = false, DefaultValue = "0.5")]
            public float delay_seconds { get; set; } = 0.5f;

            [ToolParameter("Maximum seconds before the action fails.", Required = false, DefaultValue = "10")]
            public float timeout_seconds { get; set; } = 10f;
        }

        public static Task<object> HandleCommand(JObject parameters)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                AIGameTestAutoRefreshGuard.Lock();

            var actionToken = parameters?["action"];
            if (actionToken == null)
                return Task.FromResult<object>(new ErrorResponse("'action' is required."));

            var actionJson = actionToken.Type == JTokenType.String
                ? actionToken.ToString()
                : actionToken.ToString(Formatting.None);
            var delaySeconds = Math.Max(0.05f, parameters?["delay_seconds"]?.Value<float>() ?? 0.5f);
            var timeoutSeconds = Math.Max(delaySeconds + 1f, parameters?["timeout_seconds"]?.Value<float>() ?? 10f);

            var begin = JObject.Parse(AIGameTestDriver.BeginVisualActionJson(actionJson, delaySeconds));
            var jobId = begin["jobId"]?.ToString();
            if (string.IsNullOrWhiteSpace(jobId))
                return Task.FromResult<object>(new ErrorResponse("Driver did not return a visual action job.", begin));

            var completion = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var startedAt = EditorApplication.timeSinceStartup;

            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                try
                {
                    if (EditorApplication.timeSinceStartup - startedAt > timeoutSeconds)
                    {
                        EditorApplication.update -= tick;
                        completion.TrySetResult(new ErrorResponse("Visual action timed out.", new { jobId }));
                        return;
                    }

                    var poll = JObject.Parse(AIGameTestDriver.PollVisualActionJson(jobId));
                    var status = poll["status"]?.ToString();
                    if (string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase))
                        return;

                    EditorApplication.update -= tick;
                    if (!string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        completion.TrySetResult(new ErrorResponse("Visual action failed.", poll));
                        return;
                    }

                    completion.TrySetResult(new SuccessResponse("Visual action completed.", poll));
                }
                catch (Exception exception)
                {
                    EditorApplication.update -= tick;
                    completion.TrySetResult(new ErrorResponse("Visual action error: " + exception.Message));
                }
            };

            EditorApplication.update += tick;
            return completion.Task;
        }
    }
}
#endif
