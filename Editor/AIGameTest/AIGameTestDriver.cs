#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.AIGameTestTools
{
    public static class AIGameTestDriver
    {
        private const float RepeatClickWindowSeconds = 0.35f;
        private static readonly AIGameTestSession Session = new AIGameTestSession();
        private static readonly Dictionary<string, VisualActionJob> VisualJobs = new();
        private static readonly List<SimulatedSwipe> SimulatedSwipes = new();
        private static readonly List<SimulatedTap> SimulatedTaps = new();
        private static bool visualJobTickRegistered;

        public static string StartSessionJson(string label = null)
        {
            Session.Reset(label);
            return ToJson(Session.ToDto());
        }

        public static string GetSessionJson()
        {
            return ToJson(Session.ToDto());
        }

        public static string StopSessionJson()
        {
            Session.Stop();
            return ToJson(Session.ToDto());
        }

        public static string ObserveJson(bool includeInactiveKnownObjects = true, bool includeCandidates = true)
        {
            var observe = Observe(includeInactiveKnownObjects, includeCandidates);
            Session.LastObserve = observe;
            return ToJson(observe);
        }

        public static string ClickBestJson(string intent = "default", float postActionDelaySeconds = 0.5f)
        {
            var observe = Observe(true, true);
            var result = SelectBestCandidate(observe, intent);
            if (!result.success)
            {
                var noClick = ActionResultDto.NeedsDecision(result.reason, observe, observe);
                noClick.action = new ActionRequestDto { type = "click_best", intent = intent };
                noClick.screenshot = CaptureScreenshot(
                    BuildStepScreenshotName(noClick.action),
                    "Library/AIGameTestScreenshots",
                    true);
                Session.AddScreenshot(noClick.screenshot.projectPath);
                Session.AddAction(noClick);
                return ToJson(noClick);
            }

            var request = new ActionRequestDto
            {
                type = "click",
                path = result.candidate.path,
                intent = intent
            };
            return Act(request, postActionDelaySeconds, observe);
        }

        public static string ClickBestCompactJson(string intent = "default", float postActionDelaySeconds = 0.5f)
        {
            var full = JsonUtility.FromJson<ActionResultDto>(ClickBestJson(intent, postActionDelaySeconds));
            return ToJson(ToCompactActionResult(full));
        }

        public static string ActJson(string actionJson, float postActionDelaySeconds = 0.5f)
        {
            var request = JsonUtility.FromJson<ActionRequestDto>(actionJson);
            return Act(request, postActionDelaySeconds, null);
        }

        public static string ActCompactJson(string actionJson, float postActionDelaySeconds = 0.5f)
        {
            var full = JsonUtility.FromJson<ActionResultDto>(ActJson(actionJson, postActionDelaySeconds));
            return ToJson(ToCompactActionResult(full));
        }

        public static string BeginVisualActionJson(string actionJson, float captureDelaySeconds = 0.5f)
        {
            var request = JsonUtility.FromJson<ActionRequestDto>(actionJson);
            var before = BuildCompactState();
            var job = new VisualActionJob
            {
                jobId = Guid.NewGuid().ToString("N"),
                request = request,
                startedAt = EditorApplication.timeSinceStartup,
                captureAt = EditorApplication.timeSinceStartup + Math.Max(0.05f, captureDelaySeconds),
                before = before,
                status = "Pending"
            };
            if (request != null &&
                string.Equals(request.type, "wait", StringComparison.OrdinalIgnoreCase) &&
                request.duration > captureDelaySeconds)
            {
                job.captureAt = EditorApplication.timeSinceStartup + request.duration;
            }
            if (request != null &&
                string.Equals(request.type, "swipe", StringComparison.OrdinalIgnoreCase) &&
                request.duration > captureDelaySeconds)
            {
                job.captureAt = EditorApplication.timeSinceStartup + request.duration + 0.1f;
            }

            ExecuteImmediate(request, out job.actionSuccess, out job.reason);
            if (!job.actionSuccess)
                job.captureAt = EditorApplication.timeSinceStartup;

            VisualJobs[job.jobId] = job;
            EnsureVisualJobTick();
            return ToJson(new BeginVisualActionResultDto
            {
                status = "Pending",
                jobId = job.jobId,
                actionSuccess = job.actionSuccess,
                reason = job.reason,
                captureDelaySeconds = captureDelaySeconds
            });
        }

        public static string PollVisualActionJson(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId) || !VisualJobs.TryGetValue(jobId, out var job))
                return ToJson(new VisualActionResultDto
                {
                    status = "NotFound",
                    jobId = jobId,
                    success = false,
                    requiresDiagnosis = true,
                    reason = "visual action job not found"
                });

            if (job.status == "Pending" && EditorApplication.timeSinceStartup >= job.captureAt)
                CompleteVisualJob(job);

            if (job.status == "Pending")
                return ToJson(new VisualActionResultDto
                {
                    status = "Pending",
                    jobId = job.jobId,
                    success = job.actionSuccess,
                    reason = job.reason
                });

            return ToJson(job.result);
        }

        public static string CaptureScreenshotJson(string fileName = null, string folder = "Assets/Screenshots")
        {
            var result = CaptureScreenshot(fileName, folder, true);
            Session.AddScreenshot(result.projectPath);
            return ToJson(result);
        }

        public static string WaitUntilJson(string conditionJson, float timeoutSeconds = 5f, float pollIntervalSeconds = 0.1f)
        {
            var condition = JsonUtility.FromJson<WaitConditionDto>(conditionJson);
            var startedAt = EditorApplication.timeSinceStartup;
            ObserveDto last = null;
            string reason = "timeout";

            while (EditorApplication.timeSinceStartup - startedAt <= timeoutSeconds)
            {
                last = Observe(true, true);
                if (EvaluateCondition(condition, last, out reason))
                {
                    var success = new WaitResultDto
                    {
                        status = "Success",
                        success = true,
                        reason = reason,
                        elapsedSeconds = (float)(EditorApplication.timeSinceStartup - startedAt),
                        condition = condition,
                        observe = last
                    };
                    return ToJson(success);
                }

                PumpEditorFor(pollIntervalSeconds);
            }

            var failed = new WaitResultDto
            {
                status = "Timeout",
                success = false,
                reason = reason,
                elapsedSeconds = (float)(EditorApplication.timeSinceStartup - startedAt),
                condition = condition,
                observe = last ?? Observe(true, true)
            };
            return ToJson(failed);
        }

        [MenuItem("Tools/AI Game Test/Start Session")]
        private static void StartSessionMenu()
        {
            Debug.Log(StartSessionJson("menu"));
        }

        [MenuItem("Tools/AI Game Test/Observe UI")]
        private static void ObserveMenu()
        {
            Debug.Log(ObserveJson());
        }

        [MenuItem("Tools/AI Game Test/Click Best")]
        private static void ClickBestMenu()
        {
            Debug.Log(ClickBestJson("default"));
        }

        [MenuItem("Tools/AI Game Test/Capture Screenshot")]
        private static void CaptureScreenshotMenu()
        {
            Debug.Log(CaptureScreenshotJson());
        }

        private static string Act(ActionRequestDto request, float postActionDelaySeconds, ObserveDto before)
        {
            before ??= Observe(true, true);
            var startedAt = EditorApplication.timeSinceStartup;
            var result = new ActionResultDto
            {
                status = "Success",
                success = true,
                action = request,
                before = before
            };

            if (request == null || string.IsNullOrWhiteSpace(request.type))
            {
                result.status = "NeedsDecision";
                result.success = false;
                result.reason = "empty action";
            }
            else
            {
                var type = request.type.Trim().ToLowerInvariant();
                switch (type)
                {
                    case "click":
                        result.success = TryClick(request, out result.reason);
                        result.status = result.success ? "Success" : "NeedsDecision";
                        break;
                    case "click_best":
                        var selected = SelectBestCandidate(before, string.IsNullOrWhiteSpace(request.intent) ? "default" : request.intent);
                        if (selected.success)
                        {
                            request.path = selected.candidate.path;
                            request.instanceId = selected.candidate.instanceId;
                            result.success = TryClick(request, out result.reason);
                            result.reason = selected.reason + "; " + result.reason;
                            result.status = result.success ? "Success" : "NeedsDecision";
                        }
                        else
                        {
                            result.success = false;
                            result.reason = selected.reason;
                            result.status = "NeedsDecision";
                        }
                        break;
                    case "input":
                        result.success = TryInput(request, out result.reason);
                        result.status = result.success ? "Success" : "NeedsDecision";
                        break;
                    case "swipe":
                        result.success = TryStartSwipe(request, out result.reason);
                        result.status = result.success ? "Success" : "NeedsDecision";
                        break;
                    case "wait":
                        PumpEditorFor(request.duration > 0 ? request.duration : postActionDelaySeconds);
                        result.reason = "wait";
                        break;
                    case "screenshot":
                        var shot = CaptureScreenshot(
                            string.IsNullOrWhiteSpace(request.value) ? null : request.value,
                            string.IsNullOrWhiteSpace(request.path) ? "Assets/Screenshots" : request.path,
                            true);
                        Session.AddScreenshot(shot.projectPath);
                        result.reason = "screenshot requested: " + shot.fullPath;
                        break;
                    default:
                        result.success = false;
                        result.status = "NeedsDecision";
                        result.reason = "unsupported action type: " + request.type;
                        break;
                }
            }

            if (postActionDelaySeconds > 0 && request != null && !string.Equals(request.type, "wait", StringComparison.OrdinalIgnoreCase))
                PumpEditorFor(postActionDelaySeconds);

            result.after = Observe(true, true);
            result.elapsedSeconds = (float)(EditorApplication.timeSinceStartup - startedAt);

            if (result.success &&
                postActionDelaySeconds > 0.2f &&
                NoStateChange(before, result.after) &&
                request != null &&
                !string.Equals(request.type, "wait", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(request.type, "input", StringComparison.OrdinalIgnoreCase))
            {
                result.status = "NeedsVisualAnalysis";
                result.reason += "; observe summary unchanged after action";
                result.success = false;
            }

            var shouldInspectStepScreenshot =
                !result.success ||
                string.Equals(result.status, "NeedsDecision", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(result.status, "NeedsVisualAnalysis", StringComparison.OrdinalIgnoreCase);
            result.screenshot = CaptureScreenshot(
                BuildStepScreenshotName(request),
                "Library/AIGameTestScreenshots",
                shouldInspectStepScreenshot);
            Session.AddScreenshot(result.screenshot.projectPath);

            Session.AddAction(result);
            return ToJson(result);
        }

        private static void ExecuteImmediate(ActionRequestDto request, out bool success, out string reason)
        {
            success = false;
            reason = "empty action";
            if (request == null || string.IsNullOrWhiteSpace(request.type))
                return;

            switch (request.type.Trim().ToLowerInvariant())
            {
                case "click":
                    success = TryClick(request, out reason);
                    break;
                case "double_click":
                    var firstClick = TryClick(request, out var firstReason);
                    var secondClick = TryClick(request, out var secondReason);
                    success = firstClick || secondClick;
                    reason = "double click: " + firstReason + "; " + secondReason;
                    break;
                case "click_best":
                    var observe = Observe(true, true);
                    var selected = SelectBestCandidate(observe, string.IsNullOrWhiteSpace(request.intent) ? "default" : request.intent);
                    if (!selected.success)
                    {
                        reason = selected.reason;
                        return;
                    }
                    request.path = selected.candidate.path;
                    request.instanceId = selected.candidate.instanceId;
                    success = TryClick(request, out reason);
                    break;
                case "input":
                    success = TryInput(request, out reason);
                    break;
                case "swipe":
                    success = TryStartSwipe(request, out reason);
                    break;
                case "wait":
                    success = true;
                    reason = "wait";
                    break;
                case "observe":
                    success = true;
                    reason = "observe";
                    break;
                default:
                    reason = "unsupported async action type: " + request.type;
                    break;
            }
        }

        private static void EnsureVisualJobTick()
        {
            if (visualJobTickRegistered)
                return;
            visualJobTickRegistered = true;
            EditorApplication.update += TickVisualJobs;
        }

        private static void TickVisualJobs()
        {
            TickSimulatedSwipes();
            TickSimulatedTaps();

            foreach (var job in VisualJobs.Values.Where(j => j.status == "Pending" && EditorApplication.timeSinceStartup >= j.captureAt).ToList())
                CompleteVisualJob(job);

            if (VisualJobs.Values.Any(j => j.status == "Pending") || SimulatedSwipes.Count > 0 || SimulatedTaps.Count > 0)
                return;

            EditorApplication.update -= TickVisualJobs;
            visualJobTickRegistered = false;
        }

        private static bool TryStartSwipe(ActionRequestDto request, out string reason)
        {
            if (!EditorApplication.isPlaying)
            {
                reason = "swipe requires Play mode";
                return false;
            }

            var start = NormalizedToScreen(request.startX, request.startY);
            var end = NormalizedToScreen(request.endX, request.endY);
            var target = string.IsNullOrWhiteSpace(request.path) ? null : FindActionTarget(request, requireInput: false);
            if (!string.IsNullOrWhiteSpace(request.path) && target == null)
            {
                reason = "swipe target not found";
                return false;
            }

            var swipe = new SimulatedSwipe
            {
                target = target,
                start = start,
                end = end,
                last = start,
                startedAt = EditorApplication.timeSinceStartup,
                duration = Math.Max(0.1f, request.duration > 0 ? request.duration : 0.35f),
                pointer = EventSystem.current != null ? new PointerEventData(EventSystem.current) : null
            };
            swipe.pointer?.Reset();
            if (swipe.pointer != null)
            {
                swipe.pointer.position = start;
                swipe.pointer.pressPosition = start;
                swipe.pointer.button = PointerEventData.InputButton.Left;
            }

            BeginSwipe(swipe);
            SimulatedSwipes.Add(swipe);
            EnsureVisualJobTick();
            reason = $"swipe {start} -> {end} over {swipe.duration:F2}s" +
                     (target != null ? " on " + PathOf(target.transform) : " on game world");
            return true;
        }

        private static Vector2 NormalizedToScreen(float x, float y)
        {
            return new Vector2(
                Mathf.Clamp01(x) * Math.Max(1, Screen.width),
                Mathf.Clamp01(y) * Math.Max(1, Screen.height));
        }

        private static void BeginSwipe(SimulatedSwipe swipe)
        {
            SetInputManagerDrag(swipe.start, Vector2.zero, true, "OnDragBegin");
            if (swipe.target != null && swipe.pointer != null)
            {
                ExecuteEvents.Execute<IPointerDownHandler>(swipe.target, swipe.pointer, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute<IBeginDragHandler>(swipe.target, swipe.pointer, ExecuteEvents.beginDragHandler);
            }
        }

        private static void TickSimulatedSwipes()
        {
            foreach (var swipe in SimulatedSwipes.ToList())
            {
                var elapsed = EditorApplication.timeSinceStartup - swipe.startedAt;
                var progress = Mathf.Clamp01((float)(elapsed / swipe.duration));
                var current = Vector2.Lerp(swipe.start, swipe.end, progress);
                var delta = current - swipe.last;
                swipe.last = current;

                SetInputManagerDrag(current, delta, true, "OnSingleDrag");
                if (swipe.target != null && swipe.pointer != null)
                {
                    swipe.pointer.delta = delta;
                    swipe.pointer.position = current;
                    ExecuteEvents.Execute<IDragHandler>(swipe.target, swipe.pointer, ExecuteEvents.dragHandler);
                }

                if (progress < 1f)
                    continue;

                SetInputManagerDrag(current, Vector2.zero, false, "OnDragRelease");
                if (swipe.target != null && swipe.pointer != null)
                {
                    ExecuteEvents.Execute<IEndDragHandler>(swipe.target, swipe.pointer, ExecuteEvents.endDragHandler);
                    ExecuteEvents.Execute<IPointerUpHandler>(swipe.target, swipe.pointer, ExecuteEvents.pointerUpHandler);
                }
                SimulatedSwipes.Remove(swipe);
            }
        }

        private static void SetInputManagerDrag(Vector2 position, Vector2 delta, bool dragging, string eventName)
        {
            var inputManager = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .FirstOrDefault(component => component != null && component.GetType().Name == "InputManager");
            if (inputManager == null)
                return;

            var type = inputManager.GetType();
            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic;
            type.GetField("_singleDragDelta", flags)?.SetValue(inputManager, delta);
            type.GetField("_currentDragPosition", flags)?.SetValue(inputManager, position);
            type.GetField("_isDragging", flags)?.SetValue(inputManager, dragging);

            var callback = type.GetField(eventName, flags)?.GetValue(inputManager) as Delegate;
            if (callback == null)
                return;
            if (eventName == "OnSingleDrag")
                callback.DynamicInvoke(delta, position);
            else
                callback.DynamicInvoke(position);
        }

        private static void CompleteVisualJob(VisualActionJob job)
        {
            var after = BuildCompactState();
            var screenshot = CaptureScreenshot(
                "visual_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_" + SafeActionType(job.request) + ".png",
                "Library/AIGameTestScreenshots",
                !job.actionSuccess);
            Session.AddScreenshot(screenshot.projectPath);

            var changed = job.before.fingerprint != after.fingerprint;
            var requiresDiagnosis = !job.actionSuccess || screenshot.requiresVisualInspection;
            job.status = "Completed";
            job.result = new VisualActionResultDto
            {
                status = "Completed",
                jobId = job.jobId,
                success = job.actionSuccess,
                reason = job.reason,
                scene = after.scene,
                state = after.fingerprint,
                changed = changed,
                screenshot = screenshot.fullPath,
                screenshotWidth = screenshot.width,
                screenshotHeight = screenshot.height,
                onboardingComplete = IsOnboardingComplete(after),
                requiresDiagnosis = requiresDiagnosis
            };
        }

        private static CompactStateDto BuildCompactState()
        {
            var scene = SceneManager.GetActiveScene().name;
            var network = TryReadNetworkConnected(out var playerConnected);
            var main = IsPathActive("DontDestoryUI/MainCanvas/MainPanel") || HasActiveObjectNamed("MainPanel");
            var video = IsPathActive("Canvas/VideoPanel/Obj/BtnSkip");
            var reward = IsPathActive("DontDestoryUI/RewardPanel/PanelObj");
            var guide = IsPathActive("DontDestoryUI/GuideCanvas/GudiePanel/Root/SkipButton");
            return new CompactStateDto
            {
                scene = scene,
                mainPanelActive = main,
                videoActive = video,
                rewardActive = reward,
                guideActive = guide,
                fingerprint = scene + "|net=" + Bool01(network) + Bool01(playerConnected) +
                              "|main=" + Bool01(main) + "|video=" + Bool01(video) +
                              "|reward=" + Bool01(reward) + "|guide=" + Bool01(guide)
            };
        }

        private static string Bool01(bool value)
        {
            return value ? "1" : "0";
        }

        private static bool IsOnboardingComplete(CompactStateDto state)
        {
            return state != null &&
                   state.scene == "MainScene" &&
                   state.mainPanelActive &&
                   !state.videoActive &&
                   !state.rewardActive &&
                   !state.guideActive;
        }

        private static string SafeActionType(ActionRequestDto request)
        {
            return request != null && !string.IsNullOrWhiteSpace(request.type)
                ? request.type.Trim().ToLowerInvariant()
                : "unknown";
        }

        private static string BuildStepScreenshotName(ActionRequestDto request)
        {
            var actionType = request != null && !string.IsNullOrWhiteSpace(request.type)
                ? request.type.Trim().ToLowerInvariant()
                : "unknown";
            return "step_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_" + actionType + ".png";
        }

        private static ObserveDto Observe(bool includeInactiveKnownObjects, bool includeCandidates)
        {
            var scene = SceneManager.GetActiveScene();
            var dto = new ObserveDto
            {
                status = "Observed",
                sceneName = scene.name,
                sceneBuildIndex = scene.buildIndex,
                isPlaying = EditorApplication.isPlaying,
                timeSinceStartup = (float)EditorApplication.timeSinceStartup,
                sessionId = Session.SessionId
            };

            dto.visibleCanvases = FindVisibleCanvases();
            dto.interactableButtons = FindInteractableButtons();
            dto.visibleTexts = FindVisibleTexts();
            dto.blockingPanels = FindBlockingPanels(includeInactiveKnownObjects);
            dto.topRaycastGraphics = FindTopRaycastGraphics();
            dto.mainPanelActive = IsPathActive("DontDestoryUI/MainPanel") || HasActiveObjectNamed("MainPanel");
            dto.networkConnected = TryReadNetworkConnected(out var playerConnected);
            dto.playerServerConnected = playerConnected;
            dto.lastActions = Session.Actions.TakeLast(8).Select(ToActionSummary).ToList();

            if (includeCandidates)
                dto.actionCandidates = ScoreCandidates(dto.interactableButtons, "default");

            dto.summary = BuildSummary(dto);
            return dto;
        }

        private static ScreenshotResultDto CaptureScreenshot(string fileName, string folder, bool requiresVisualInspection)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "ai_game_test_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            if (!fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                fileName += ".png";

            if (string.IsNullOrWhiteSpace(folder))
                folder = "Assets/Screenshots";

            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            var normalizedFolder = folder.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            var fullFolder = Path.IsPathRooted(normalizedFolder)
                ? normalizedFolder
                : Path.Combine(projectRoot, normalizedFolder);

            Directory.CreateDirectory(fullFolder);
            var fullPath = Path.Combine(fullFolder, fileName);
            var capturedSynchronously = TryRenderGameView(fullPath, out var captureNote, out var averageBrightness, out var mostlyDark, out var width, out var height);
            if (!capturedSynchronously)
                ScreenCapture.CaptureScreenshot(fullPath);

            var projectPath = ToProjectRelativePath(fullPath, projectRoot);
            if (projectPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                EditorApplication.delayCall += AssetDatabase.Refresh;

            var inspectScreenshot = requiresVisualInspection || mostlyDark;
            return new ScreenshotResultDto
            {
                status = capturedSynchronously ? "Saved" : "Requested",
                success = true,
                projectPath = projectPath,
                fullPath = fullPath.Replace('\\', '/'),
                requiresVisualInspection = inspectScreenshot,
                averageBrightness = averageBrightness,
                mostlyDark = mostlyDark,
                width = width,
                height = height,
                note = capturedSynchronously
                    ? captureNote + (inspectScreenshot ? " Inspect fullPath because this step needs diagnosis." : " Screenshot retained; AI inspection is optional.")
                    : "ScreenCapture fallback is async." + (inspectScreenshot ? " Wait about 1 second, then inspect fullPath." : " Screenshot retained; AI inspection is optional.")
            };
        }

        private static bool TryRenderGameView(string fullPath, out string note, out float averageBrightness, out bool mostlyDark, out int width, out int height)
        {
            note = string.Empty;
            averageBrightness = -1f;
            mostlyDark = false;
            width = 0;
            height = 0;
            try
            {
                var gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
                var gameView = Resources.FindObjectsOfTypeAll(gameViewType).OfType<EditorWindow>().FirstOrDefault();
                if (gameView == null)
                {
                    note = "No GameView window found.";
                    return false;
                }

                var sizeProperty = gameViewType.GetProperty(
                    "targetRenderSize",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                var renderMethod = gameViewType.GetMethod(
                    "RenderView",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                    null,
                    new[] { typeof(Vector2), typeof(bool) },
                    null);
                if (sizeProperty == null || renderMethod == null)
                {
                    note = "GameView render API is unavailable.";
                    return false;
                }

                var targetSize = (Vector2)sizeProperty.GetValue(gameView);
                var renderTexture = renderMethod.Invoke(gameView, new object[] { targetSize, false }) as RenderTexture;
                if (renderTexture == null || renderTexture.width <= 0 || renderTexture.height <= 0)
                {
                    note = "GameView RenderView returned no texture.";
                    return false;
                }

                width = renderTexture.width;
                height = renderTexture.height;
                var previousActive = RenderTexture.active;
                RenderTexture.active = renderTexture;
                var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                RenderTexture.active = previousActive;
                var pixels = texture.GetPixels();
                FlipVertically(pixels, width, height);
                texture.SetPixels(pixels);
                texture.Apply();
                averageBrightness = AverageBrightness(pixels);
                mostlyDark = averageBrightness >= 0f && averageBrightness < 0.08f;
                File.WriteAllBytes(fullPath, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                note = "Rendered pure GameView at " + width + "x" + height + (mostlyDark ? "; screenshot is mostly dark." : ".");
                return true;
            }
            catch (Exception ex)
            {
                note = "GameView render failed: " + ex.Message;
                return false;
            }
        }

        private static void FlipVertically(Color[] pixels, int width, int height)
        {
            for (var y = 0; y < height / 2; y++)
            {
                var oppositeY = height - 1 - y;
                for (var x = 0; x < width; x++)
                {
                    var top = y * width + x;
                    var bottom = oppositeY * width + x;
                    (pixels[top], pixels[bottom]) = (pixels[bottom], pixels[top]);
                }
            }
        }

        private static float AverageBrightness(Color[] pixels)
        {
            if (pixels == null || pixels.Length == 0)
                return -1f;

            double sum = 0;
            var step = Math.Max(1, pixels.Length / 4096);
            var count = 0;
            for (var i = 0; i < pixels.Length; i += step)
            {
                var color = pixels[i];
                sum += (color.r + color.g + color.b) / 3.0;
                count++;
            }

            return count == 0 ? -1f : (float)(sum / count);
        }

        private static List<CanvasDto> FindVisibleCanvases()
        {
            return UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(c => c != null && c.gameObject.activeInHierarchy)
                .Select(c => new CanvasDto
                {
                    path = PathOf(c.transform),
                    name = c.name,
                    sortingOrder = c.sortingOrder,
                    renderMode = c.renderMode.ToString()
                })
                .OrderByDescending(c => c.sortingOrder)
                .Take(32)
                .ToList();
        }

        private static List<ButtonDto> FindInteractableButtons()
        {
            return UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(b =>
                    b != null &&
                    b.gameObject.activeInHierarchy &&
                    b.interactable &&
                    CanRealClick(b.gameObject, out _))
                .Select(ToButtonDto)
                .OrderByDescending(b => b.sortingOrder)
                .ThenBy(b => b.path)
                .Take(80)
                .ToList();
        }

        private static ButtonDto ToButtonDto(Button button)
        {
            var rect = button.GetComponent<RectTransform>();
            var canvas = button.GetComponentInParent<Canvas>();
            var texts = ReadTexts(button.gameObject);
            return new ButtonDto
            {
                id = PathOf(button.transform),
                instanceId = button.gameObject.GetInstanceID(),
                path = PathOf(button.transform),
                siblingPath = SiblingPathOf(button.transform),
                name = button.name,
                texts = texts,
                active = button.gameObject.activeInHierarchy,
                interactable = button.interactable,
                screenPosition = RectScreenPosition(rect),
                canvasPath = canvas != null ? PathOf(canvas.transform) : string.Empty,
                sortingOrder = canvas != null ? canvas.sortingOrder : 0,
                siblingIndex = button.transform.GetSiblingIndex(),
                components = button.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name).ToList(),
                parentTexts = ReadTexts(button.transform.parent != null ? button.transform.parent.gameObject : button.gameObject),
                scoreHints = new List<string>()
            };
        }

        private static List<TextDto> FindVisibleTexts()
        {
            var items = new List<TextDto>();
            items.AddRange(UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(t => t != null && t.gameObject.activeInHierarchy && !string.IsNullOrWhiteSpace(t.text))
                .Select(t => new TextDto { path = PathOf(t.transform), text = t.text, type = t.GetType().Name }));
            items.AddRange(UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(t => t != null && t.gameObject.activeInHierarchy && !string.IsNullOrWhiteSpace(t.text))
                .Select(t => new TextDto { path = PathOf(t.transform), text = t.text, type = t.GetType().Name }));
            return items.Take(120).ToList();
        }

        private static List<BlockingPanelDto> FindBlockingPanels(bool includeInactiveKnownObjects)
        {
            var panels = new List<BlockingPanelDto>();
            var knownPaths = new[]
            {
                "DontDestoryUI/RewardPanel/PanelObj",
                "DontDestoryUI/GuideCanvas/GudiePanel/Root/SkipButton",
                "Canvas/VideoPanel/Obj/BtnSkip"
            };

            foreach (var path in knownPaths)
            {
                var go = includeInactiveKnownObjects ? FindByPathIncludingInactive(path) : GameObject.Find(path);
                if (go == null) continue;
                panels.Add(new BlockingPanelDto
                {
                    path = path,
                    name = go.name,
                    active = go.activeInHierarchy,
                    reason = "known"
                });
            }

            var modalNames = new[] { "Panel", "Popup", "Dialog", "Mask", "Modal", "Reward", "Guide" };
            foreach (var go in UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                         .Select(rt => rt.gameObject)
                         .Where(go => go.activeInHierarchy && modalNames.Any(m => go.name.IndexOf(m, StringComparison.OrdinalIgnoreCase) >= 0))
                         .Take(40))
            {
                var path = PathOf(go.transform);
                if (panels.Any(p => p.path == path)) continue;
                panels.Add(new BlockingPanelDto
                {
                    path = path,
                    name = go.name,
                    active = true,
                    reason = "name looks blocking"
                });
            }

            return panels.Take(64).ToList();
        }

        private static List<GraphicDto> FindTopRaycastGraphics()
        {
            return UnityEngine.Object.FindObjectsByType<Graphic>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(g => g != null && g.gameObject.activeInHierarchy && g.raycastTarget)
                .Select(g =>
                {
                    var canvas = g.GetComponentInParent<Canvas>();
                    return new GraphicDto
                    {
                        path = PathOf(g.transform),
                        type = g.GetType().Name,
                        sortingOrder = canvas != null ? canvas.sortingOrder : 0
                    };
                })
                .OrderByDescending(g => g.sortingOrder)
                .Take(40)
                .ToList();
        }

        private static List<CandidateDto> ScoreCandidates(List<ButtonDto> buttons, string intent)
        {
            var highestSortingOrder = HighestSortingOrder(buttons);
            return buttons
                .Select(b => ScoreCandidate(b, intent, highestSortingOrder))
                .OrderByDescending(c => c.score)
                .Take(20)
                .ToList();
        }

        private static CandidateDto ScoreCandidate(ButtonDto button, string intent, int highestSortingOrder)
        {
            var key = (button.path + "|" + button.name + "|" + string.Join("|", button.texts ?? new List<string>())).ToLowerInvariant();
            var score = 100;
            var reasons = new List<string> { "active interactable button" };
            var candidateIntent = "default";

            AddIf(key.Contains("跳过") || key.Contains("skip"), 1000, "skip text/name/path", "skip");
            AddIf(key.Contains("guidecanvas") || key.Contains("gudiepanel") || key.Contains("tutorial") || key.Contains("guide"), 900, "guide/tutorial path", "skip");
            AddIf(key.Contains("videopanel") || key.Contains("btnskip"), 850, "video skip path", "skip");
            AddIf(key.Contains("领取") || key.Contains("奖励") || key.Contains("reward"), 800, "claim/reward text/path", "claim");
            AddIf(key.Contains("确认") || key.Contains("确定") || key.Contains("ok"), 700, "confirm text", "confirm");
            AddIf(key.Contains("关闭") || key.Contains("close") || key.Contains("exit"), 600, "close text/name/path", "close");
            AddIf(key.Contains("继续") || key.Contains("continue") || key.Contains("下一步"), 400, "continue text", "confirm");

            if (button.sortingOrder == highestSortingOrder)
            {
                score += 300;
                reasons.Add("highest canvas sortingOrder");
            }

            if (LooksBlocking(key))
            {
                score += 250;
                reasons.Add("looks blocking modal");
            }

            if (key.Contains("mainpanel"))
            {
                score -= 300;
                reasons.Add("main panel navigation penalty");
            }

            if (Session.LastClickedAt.TryGetValue(button.path, out var lastClick) &&
                EditorApplication.timeSinceStartup - lastClick < RepeatClickWindowSeconds)
            {
                score -= 500;
                reasons.Add("recently clicked penalty");
            }

            if (Session.LastNoChangePath == button.path)
            {
                score -= 800;
                reasons.Add("last click no state change penalty");
            }

            if ((button.texts == null || button.texts.Count == 0) && !LooksBlocking(key))
            {
                score -= 400;
                reasons.Add("no text outside blocking panel penalty");
            }

            if (!IntentMatches(intent, candidateIntent, key))
            {
                score -= 300;
                reasons.Add("intent mismatch penalty");
            }

            return new CandidateDto
            {
                path = button.path,
                instanceId = button.instanceId,
                siblingPath = button.siblingPath,
                intent = candidateIntent,
                score = score,
                reason = reasons
            };

            void AddIf(bool condition, int amount, string reason, string newIntent)
            {
                if (!condition) return;
                score += amount;
                reasons.Add(reason);
                if (candidateIntent == "default" || amount >= 700)
                    candidateIntent = newIntent;
            }
        }

        private static CandidateSelectionDto SelectBestCandidate(ObserveDto observe, string intent)
        {
            var candidates = ScoreCandidates(observe.interactableButtons ?? new List<ButtonDto>(), intent);
            if (candidates.Count == 0)
                return CandidateSelectionDto.Fail("no active interactable button");

            var filtered = candidates
                .Where(c => IntentMatches(intent, c.intent, c.path.ToLowerInvariant()))
                .OrderByDescending(c => c.score)
                .ToList();
            if (filtered.Count == 0)
                filtered = candidates.OrderByDescending(c => c.score).ToList();

            var best = filtered[0];
            var second = filtered.Count > 1 ? filtered[1] : null;
            if (best.score < 500)
                return CandidateSelectionDto.Fail("best score < 500: " + best.score);
            if (second != null && best.score - second.score < 150)
                return CandidateSelectionDto.Fail("top candidates too close: " + best.score + " vs " + second.score);
            if (candidates.Count > 10 && best.score < 800)
                return CandidateSelectionDto.Fail("too many candidates and low confidence");

            return CandidateSelectionDto.Ok(best, "selected " + best.path + " score=" + best.score);
        }

        private static bool TryClick(ActionRequestDto request, out string reason)
        {
            var go = FindActionTarget(request, requireInput: false);
            if (go == null)
            {
                reason = "target not found";
                return false;
            }
            if (!go.activeInHierarchy)
            {
                reason = "target inactive: " + PathOf(go.transform);
                return false;
            }

            var button = go.GetComponent<Button>();
            if (button != null && !button.interactable)
            {
                reason = "button not interactable: " + PathOf(go.transform);
                return false;
            }

            if (TryResolveRealClick(go, out var data, out var clickHandler, out reason))
            {
                var downHandler = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(
                    data.pointerCurrentRaycast.gameObject,
                    data,
                    ExecuteEvents.pointerDownHandler);
                data.pointerPress = downHandler;
                data.rawPointerPress = data.pointerCurrentRaycast.gameObject;
                if (downHandler != null)
                    ExecuteEvents.Execute<IPointerUpHandler>(downHandler, data, ExecuteEvents.pointerUpHandler);

                var clicked = ExecuteEvents.Execute<IPointerClickHandler>(clickHandler, data, ExecuteEvents.pointerClickHandler);
                if (!clicked)
                {
                    reason = "resolved target rejected pointer click: " + PathOf(clickHandler.transform);
                    return false;
                }

                var uiPath = PathOf(go.transform);
                Session.LastClickedAt[uiPath] = EditorApplication.timeSinceStartup;
                reason = "real UI click " + uiPath + " via raycast hit " + PathOf(data.pointerCurrentRaycast.gameObject.transform);
                return true;
            }

            if (ExecuteEvents.GetEventHandler<IPointerClickHandler>(go) != null)
                return false;

            if (!TryResolveWorldClick(go, out var worldPosition, out reason))
                return false;

            if (!TrySimulateInputManagerTap(worldPosition, out reason))
                return false;

            var worldPath = PathOf(go.transform);
            Session.LastClickedAt[worldPath] = EditorApplication.timeSinceStartup;
            reason = "real world tap " + worldPath + " through InputManager at " + worldPosition;
            return true;
        }

        private static bool CanRealClick(GameObject target, out string reason)
        {
            return TryResolveRealClick(target, out _, out _, out reason) ||
                   TryResolveWorldClick(target, out _, out reason);
        }

        private static bool TryResolveWorldClick(GameObject target, out Vector2 position, out string reason)
        {
            position = default;
            reason = string.Empty;
            if (!TryGetTargetScreenPosition(target, out position))
            {
                reason = "target has no valid screen position: " + PathOf(target.transform);
                return false;
            }

            if (EventSystem.current != null)
            {
                var data = new PointerEventData(EventSystem.current) { position = position };
                var uiHits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(data, uiHits);
                var blockingUi = uiHits.FirstOrDefault(hit => hit.module is GraphicRaycaster);
                if (blockingUi.gameObject != null)
                {
                    reason = "world click blocked by UI " + PathOf(blockingUi.gameObject.transform);
                    return false;
                }
            }

            var camera = Camera.main;
            if (camera == null)
            {
                reason = "no main camera";
                return false;
            }

            var targetClick = FindInterfaceInParents(target, "IClick");
            if (targetClick == null)
            {
                reason = "target has no IClick in parents: " + PathOf(target.transform);
                return false;
            }

            var clickManager = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .FirstOrDefault(component => component != null && component.GetType().Name == "ClickManager");
            var layerMask = Physics.DefaultRaycastLayers;
            var maxDistance = 100f;
            if (clickManager != null)
            {
                const System.Reflection.BindingFlags flags =
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic;
                var maskValue = clickManager.GetType().GetField("targetLayer", flags)?.GetValue(clickManager);
                if (maskValue is LayerMask mask)
                    layerMask = mask.value;
                var distanceValue = clickManager.GetType().GetField("maxDistance", flags)?.GetValue(clickManager);
                if (distanceValue is float distance)
                    maxDistance = distance;
            }

            var hits = Physics.RaycastAll(camera.ScreenPointToRay(position), maxDistance, layerMask)
                .OrderByDescending(hit => GetWorldClickPriority(hit.collider.gameObject))
                .ThenBy(hit => hit.distance);
            foreach (var hit in hits)
            {
                var hitClick = FindInterfaceInParents(hit.collider.gameObject, "IClick");
                if (hitClick == null)
                    continue;
                if (hitClick == targetClick)
                {
                    reason = "world click reachable";
                    return true;
                }

                reason = "world click blocked by " + PathOf(hit.collider.transform);
                return false;
            }

            reason = "world click raycast hit no IClick target at " + position;
            return false;
        }

        private static Component FindInterfaceInParents(GameObject target, string interfaceName)
        {
            for (var current = target.transform; current != null; current = current.parent)
            {
                var component = current.GetComponents<Component>()
                    .FirstOrDefault(item => item != null && item.GetType().GetInterfaces().Any(type => type.Name == interfaceName));
                if (component != null)
                    return component;
            }
            return null;
        }

        private static int GetWorldClickPriority(GameObject target)
        {
            var prioritized = FindInterfaceInParents(target, "IPrioritizedClick");
            var property = prioritized?.GetType().GetProperty("ClickPriority");
            return property?.GetValue(prioritized) is int priority ? priority : 0;
        }

        private static bool TrySimulateInputManagerTap(Vector2 position, out string reason)
        {
            var inputManager = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .FirstOrDefault(component => component != null && component.GetType().Name == "InputManager");
            if (inputManager == null)
            {
                reason = "no active InputManager";
                return false;
            }

            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic;
            var type = inputManager.GetType();
            type.GetField("_lastTapPosition", flags)?.SetValue(inputManager, position);
            type.GetField("WasSingleTapThisFrame", flags)?.SetValue(inputManager, true);
            type.GetField("_currentGesture", flags)?.SetValue(inputManager, Enum.Parse(type.GetField("_currentGesture", flags).FieldType, "SingleTap"));
            if (type.GetField("OnSingleTap", flags)?.GetValue(inputManager) is Delegate onSingleTap)
                onSingleTap.DynamicInvoke(position);

            SimulatedTaps.Add(new SimulatedTap { inputManager = inputManager, startFrame = Time.frameCount });
            EnsureVisualJobTick();
            reason = "InputManager tap emitted";
            return true;
        }

        private static void TickSimulatedTaps()
        {
            foreach (var tap in SimulatedTaps.ToList())
            {
                if (tap.inputManager == null || Time.frameCount <= tap.startFrame)
                    continue;
                tap.inputManager.GetType().GetField(
                    "WasSingleTapThisFrame",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                    ?.SetValue(tap.inputManager, false);
                SimulatedTaps.Remove(tap);
            }
        }

        private static bool TryResolveRealClick(
            GameObject target,
            out PointerEventData data,
            out GameObject clickHandler,
            out string reason)
        {
            data = null;
            clickHandler = null;
            reason = string.Empty;

            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                reason = "no active EventSystem";
                return false;
            }

            if (!TryGetTargetScreenPosition(target, out var position))
            {
                reason = "target has no valid screen position: " + PathOf(target.transform);
                return false;
            }

            data = new PointerEventData(eventSystem)
            {
                position = position,
                pressPosition = position,
                button = PointerEventData.InputButton.Left,
                pointerId = -1,
                clickCount = 1,
                clickTime = Time.unscaledTime
            };

            var results = new List<RaycastResult>();
            eventSystem.RaycastAll(data, results);
            if (results.Count == 0)
            {
                reason = "real click raycast hit nothing at " + position;
                return false;
            }

            var targetHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(target);
            if (targetHandler == null)
            {
                reason = "target has no pointer click handler: " + PathOf(target.transform);
                return false;
            }

            var top = results[0];
            var topHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(top.gameObject);
            if (topHandler != targetHandler)
            {
                reason = "real click blocked by " + PathOf(top.gameObject.transform) +
                         " at " + position + "; target=" + PathOf(target.transform);
                return false;
            }

            data.pointerCurrentRaycast = top;
            data.pointerPressRaycast = top;
            clickHandler = targetHandler;
            reason = "real click reachable";
            return true;
        }

        private static bool TryGetTargetScreenPosition(GameObject target, out Vector2 position)
        {
            position = default;
            var rect = target.GetComponent<RectTransform>();
            if (rect != null)
            {
                var canvas = rect.GetComponentInParent<Canvas>();
                var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? canvas.worldCamera
                    : null;
                position = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
                return float.IsFinite(position.x) && float.IsFinite(position.y);
            }

            var cameraForWorld = Camera.main;
            if (cameraForWorld == null)
                return false;
            var collider = target.GetComponentInChildren<Collider>();
            var renderer = target.GetComponentInChildren<Renderer>();
            var worldPosition = collider != null
                ? collider.bounds.center
                : renderer != null ? renderer.bounds.center : target.transform.position;
            var screen = cameraForWorld.WorldToScreenPoint(worldPosition);
            if (screen.z < 0)
                return false;
            position = screen;
            return float.IsFinite(position.x) && float.IsFinite(position.y);
        }

        private static bool TryInput(ActionRequestDto request, out string reason)
        {
            var go = FindActionTarget(request, requireInput: true);
            if (go == null)
            {
                reason = "input target not found";
                return false;
            }
            if (!go.activeInHierarchy)
            {
                reason = "input target inactive: " + PathOf(go.transform);
                return false;
            }

            var tmp = go.GetComponent<TMP_InputField>();
            if (tmp != null)
            {
                tmp.text = request.value ?? string.Empty;
                tmp.ForceLabelUpdate();
                tmp.onValueChanged.Invoke(tmp.text);
                tmp.onEndEdit.Invoke(tmp.text);
                reason = "input TMP_InputField " + PathOf(go.transform);
                return true;
            }

            var input = go.GetComponent<InputField>();
            if (input != null)
            {
                input.text = request.value ?? string.Empty;
                input.ForceLabelUpdate();
                input.onValueChanged.Invoke(input.text);
                input.onEndEdit.Invoke(input.text);
                reason = "input InputField " + PathOf(go.transform);
                return true;
            }

            reason = "target has no input field";
            return false;
        }

        private static GameObject FindActionTarget(ActionRequestDto request, bool requireInput)
        {
            if (request == null) return null;

            if (request.instanceId != 0)
            {
                var byId = EditorUtility.InstanceIDToObject(request.instanceId) as GameObject;
                if (byId != null)
                    return byId;
            }

            var candidates = UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go != null && go.scene.IsValid())
                .Where(go => !requireInput || go.GetComponent<TMP_InputField>() != null || go.GetComponent<InputField>() != null)
                .Where(go => requireInput ||
                             go.GetComponent<Button>() != null ||
                             go.GetComponents<IPointerClickHandler>().Any() ||
                             FindInterfaceInParents(go, "IClick") != null)
                .ToList();

            if (!string.IsNullOrWhiteSpace(request.path))
                candidates = candidates.Where(go => PathOf(go.transform) == request.path).ToList();

            if (!string.IsNullOrWhiteSpace(request.siblingPath))
                candidates = candidates.Where(go => SiblingPathOf(go.transform) == request.siblingPath).ToList();

            if (!string.IsNullOrWhiteSpace(request.withinPath))
                candidates = candidates.Where(go => PathOf(go.transform).StartsWith(request.withinPath, StringComparison.Ordinal)).ToList();

            if (!string.IsNullOrWhiteSpace(request.text))
                candidates = candidates.Where(go => ReadTexts(go).Any(t => t.Contains(request.text))).ToList();

            if (!string.IsNullOrWhiteSpace(request.containsText))
                candidates = candidates.Where(go => ReadTexts(go).Concat(go.transform.parent != null ? ReadTexts(go.transform.parent.gameObject) : Array.Empty<string>()).Any(t => t.Contains(request.containsText))).ToList();

            candidates = candidates
                .Where(go => go.activeInHierarchy)
                .OrderBy(go => SiblingPathOf(go.transform))
                .ThenBy(go => go.GetInstanceID())
                .ToList();

            var index = Math.Max(0, request.nth);
            return candidates.Count > index ? candidates[index] : candidates.FirstOrDefault();
        }

        private static bool EvaluateCondition(WaitConditionDto condition, ObserveDto observe, out string reason)
        {
            reason = string.Empty;
            if (condition == null || string.IsNullOrWhiteSpace(condition.type))
            {
                reason = "empty condition";
                return false;
            }

            var type = condition.type.Trim().ToLowerInvariant();
            switch (type)
            {
                case "object_appeared":
                    reason = "object appeared";
                    return IsPathActive(condition.path);
                case "object_disappeared":
                    reason = "object disappeared";
                    return !IsPathActive(condition.path);
                case "scene":
                    reason = "scene is " + condition.name;
                    return string.Equals(observe.sceneName, condition.name, StringComparison.OrdinalIgnoreCase);
                case "button_text_appeared":
                    reason = "button text appeared";
                    return (observe.interactableButtons ?? new List<ButtonDto>()).Any(b => b.texts != null && b.texts.Any(t => t.Contains(condition.text)));
                case "stable":
                    reason = "stable condition";
                    return IsStable(observe, condition);
                default:
                    reason = "unsupported condition type: " + condition.type;
                    return false;
            }
        }

        private static bool IsStable(ObserveDto observe, WaitConditionDto condition)
        {
            if (condition.noBlockingUi && (observe.blockingPanels ?? new List<BlockingPanelDto>()).Any(p => p.active && p.reason == "known"))
                return false;
            return true;
        }

        private static void PumpEditorFor(float seconds)
        {
            if (seconds <= 0) return;
            var end = EditorApplication.timeSinceStartup + seconds;
            while (EditorApplication.timeSinceStartup < end)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                Thread.Sleep(15);
            }
        }

        private static bool NoStateChange(ObserveDto before, ObserveDto after)
        {
            if (before == null || after == null) return false;
            return before.summary == after.summary;
        }

        private static string BuildSummary(ObserveDto dto)
        {
            var buttonCount = dto.interactableButtons?.Count ?? 0;
            var blockerSummary = string.Join("|", (dto.blockingPanels ?? new List<BlockingPanelDto>())
                .Where(p => p.active)
                .Select(p => p.path));
            return dto.sceneName + "|buttons=" + buttonCount + "|blockers=" + blockerSummary;
        }

        private static bool TryReadNetworkConnected(out bool playerConnected)
        {
            playerConnected = false;
            try
            {
                var type = Type.GetType("CLIP.Project_Mouse.Network.NetWork_Center_WSS, CLIP.Project_Mouse.Game_Play_Systems");
                if (type == null) return false;
                var instance = type.GetProperty("Instance")?.GetValue(null);
                var connectedField = type.GetField("_is_connected");
                var playerProp = type.GetProperty("IsConnectedToPlayerServer");
                playerConnected = playerProp != null && (bool)playerProp.GetValue(null);
                return instance != null && connectedField != null && (bool)connectedField.GetValue(instance);
            }
            catch
            {
                playerConnected = false;
                return false;
            }
        }

        private static bool IsPathActive(string path)
        {
            var go = FindByPathIncludingInactive(path);
            return go != null && go.activeInHierarchy;
        }

        private static bool HasActiveObjectNamed(string name)
        {
            return UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>()
                .Any(go => go != null && go.scene.IsValid() && go.name == name && go.activeInHierarchy);
        }

        private static GameObject FindButtonByText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            return UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(b => b != null && b.gameObject.activeInHierarchy && b.interactable)
                .FirstOrDefault(b => ReadTexts(b.gameObject).Any(t => t.Contains(text)))?.gameObject;
        }

        private static GameObject FindInputByText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            return UnityEngine.Object.FindObjectsByType<TMP_InputField>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .FirstOrDefault(i => i != null && i.gameObject.activeInHierarchy && PathOf(i.transform).IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)?.gameObject;
        }

        private static GameObject FindByPathIncludingInactive(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            return UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go != null && go.scene.IsValid() && PathOf(go.transform) == path);
        }

        private static string PathOf(Transform transform)
        {
            if (transform == null) return string.Empty;
            var names = new List<string>();
            while (transform != null)
            {
                names.Add(transform.name);
                transform = transform.parent;
            }
            names.Reverse();
            return string.Join("/", names);
        }

        private static string ToProjectRelativePath(string fullPath, string projectRoot)
        {
            var normalizedFullPath = Path.GetFullPath(fullPath);
            var normalizedRoot = Path.GetFullPath(projectRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (normalizedFullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                var relative = normalizedFullPath.Substring(normalizedRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return relative.Replace('\\', '/');
            }

            return normalizedFullPath.Replace('\\', '/');
        }

        private static string SiblingPathOf(Transform transform)
        {
            if (transform == null) return string.Empty;
            var parts = new List<string>();
            while (transform != null)
            {
                parts.Add(transform.name + "[" + transform.GetSiblingIndex() + "]");
                transform = transform.parent;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }

        private static List<string> ReadTexts(GameObject go)
        {
            var result = new List<string>();
            result.AddRange(go.GetComponentsInChildren<TMP_Text>(true)
                .Where(t => t != null && !string.IsNullOrWhiteSpace(t.text))
                .Select(t => t.text));
            result.AddRange(go.GetComponentsInChildren<Text>(true)
                .Where(t => t != null && !string.IsNullOrWhiteSpace(t.text))
                .Select(t => t.text));
            return result.Distinct().ToList();
        }

        private static Vector2Dto RectScreenPosition(RectTransform rect)
        {
            if (rect == null) return new Vector2Dto();
            var position = RectTransformUtility.WorldToScreenPoint(null, rect.position);
            return new Vector2Dto { x = position.x, y = position.y };
        }

        private static int HighestSortingOrder(List<ButtonDto> buttons)
        {
            return buttons.Count == 0 ? 0 : buttons.Max(b => b.sortingOrder);
        }

        private static bool LooksBlocking(string key)
        {
            return key.Contains("panel") || key.Contains("popup") || key.Contains("dialog") ||
                   key.Contains("mask") || key.Contains("modal") || key.Contains("reward") ||
                   key.Contains("guide");
        }

        private static bool IntentMatches(string intent, string candidateIntent, string key)
        {
            intent = string.IsNullOrWhiteSpace(intent) ? "default" : intent.ToLowerInvariant();
            candidateIntent = string.IsNullOrWhiteSpace(candidateIntent) ? "default" : candidateIntent.ToLowerInvariant();
            if (intent == "default") return true;
            if (intent == "skip_or_confirm")
                return candidateIntent is "skip" or "claim" or "confirm" or "close" || key.Contains("skip") || key.Contains("跳过");
            if (intent == "blocking_ui")
                return LooksBlocking(key);
            return candidateIntent == intent || key.Contains(intent);
        }

        private static string ToJson<T>(T value)
        {
            return JsonUtility.ToJson(value, true);
        }

        private static ActionSummaryDto ToActionSummary(ActionResultDto action)
        {
            if (action == null)
                return new ActionSummaryDto();

            return new ActionSummaryDto
            {
                status = action.status,
                success = action.success,
                reason = action.reason,
                elapsedSeconds = action.elapsedSeconds,
                type = action.action != null ? action.action.type : string.Empty,
                path = action.action != null ? action.action.path : string.Empty,
                intent = action.action != null ? action.action.intent : string.Empty,
                beforeSummary = action.before != null ? action.before.summary : string.Empty,
                afterSummary = action.after != null ? action.after.summary : string.Empty
            };
        }

        private static CompactActionResultDto ToCompactActionResult(ActionResultDto action)
        {
            if (action == null)
                return new CompactActionResultDto
                {
                    status = "NeedsDecision",
                    success = false,
                    reason = "action result unavailable",
                    requiresDetailedJson = true
                };

            return new CompactActionResultDto
            {
                status = action.status,
                success = action.success,
                reason = action.reason,
                elapsedSeconds = action.elapsedSeconds,
                action = action.action,
                beforeSummary = action.before != null ? action.before.summary : string.Empty,
                afterSummary = action.after != null ? action.after.summary : string.Empty,
                screenshot = action.screenshot,
                requiresDetailedJson =
                    !action.success ||
                    action.screenshot == null ||
                    action.screenshot.requiresVisualInspection
            };
        }

        [Serializable]
        public class ObserveDto
        {
            public string status;
            public string sessionId;
            public string sceneName;
            public int sceneBuildIndex;
            public bool isPlaying;
            public float timeSinceStartup;
            public bool networkConnected;
            public bool playerServerConnected;
            public bool mainPanelActive;
            public string summary;
            public List<CanvasDto> visibleCanvases = new();
            public List<ButtonDto> interactableButtons = new();
            public List<TextDto> visibleTexts = new();
            public List<BlockingPanelDto> blockingPanels = new();
            public List<GraphicDto> topRaycastGraphics = new();
            public List<CandidateDto> actionCandidates = new();
            public List<ActionSummaryDto> lastActions = new();
        }

        [Serializable]
        public class CanvasDto
        {
            public string path;
            public string name;
            public int sortingOrder;
            public string renderMode;
        }

        [Serializable]
        public class ButtonDto
        {
            public string id;
            public int instanceId;
            public string path;
            public string siblingPath;
            public string name;
            public List<string> texts = new();
            public List<string> parentTexts = new();
            public bool active;
            public bool interactable;
            public Vector2Dto screenPosition;
            public string canvasPath;
            public int sortingOrder;
            public int siblingIndex;
            public List<string> components = new();
            public List<string> scoreHints = new();
        }

        [Serializable]
        public class TextDto
        {
            public string path;
            public string text;
            public string type;
        }

        [Serializable]
        public class BlockingPanelDto
        {
            public string path;
            public string name;
            public bool active;
            public string reason;
        }

        [Serializable]
        public class GraphicDto
        {
            public string path;
            public string type;
            public int sortingOrder;
        }

        [Serializable]
        public class CandidateDto
        {
            public string path;
            public int instanceId;
            public string siblingPath;
            public string intent;
            public int score;
            public List<string> reason = new();
        }

        [Serializable]
        public class ActionRequestDto
        {
            public string type;
            public string path;
            public int instanceId;
            public string siblingPath;
            public string text;
            public string containsText;
            public string withinPath;
            public int nth;
            public string value;
            public string intent;
            public string key;
            public float duration;
            public float startX;
            public float startY;
            public float endX;
            public float endY;
        }

        [Serializable]
        public class ActionResultDto
        {
            public string status;
            public bool success;
            public string reason;
            public float elapsedSeconds;
            public ActionRequestDto action;
            public ObserveDto before;
            public ObserveDto after;
            public ScreenshotResultDto screenshot;

            public static ActionResultDto NeedsDecision(string reason, ObserveDto before, ObserveDto after)
            {
                return new ActionResultDto
                {
                    status = "NeedsDecision",
                    success = false,
                    reason = reason,
                    before = before,
                    after = after
                };
            }
        }

        [Serializable]
        public class ActionSummaryDto
        {
            public string status;
            public bool success;
            public string reason;
            public float elapsedSeconds;
            public string type;
            public string path;
            public string intent;
            public string beforeSummary;
            public string afterSummary;
        }

        [Serializable]
        public class CompactActionResultDto
        {
            public string status;
            public bool success;
            public string reason;
            public float elapsedSeconds;
            public ActionRequestDto action;
            public string beforeSummary;
            public string afterSummary;
            public ScreenshotResultDto screenshot;
            public bool requiresDetailedJson;
        }

        [Serializable]
        public class WaitConditionDto
        {
            public string type;
            public string path;
            public string name;
            public string text;
            public float duration;
            public bool noBlockingUi;
        }

        [Serializable]
        public class WaitResultDto
        {
            public string status;
            public bool success;
            public string reason;
            public float elapsedSeconds;
            public WaitConditionDto condition;
            public ObserveDto observe;
        }

        [Serializable]
        public class ScreenshotResultDto
        {
            public string status;
            public bool success;
            public string projectPath;
            public string fullPath;
            public bool requiresVisualInspection;
            public float averageBrightness;
            public bool mostlyDark;
            public int width;
            public int height;
            public string note;
        }

        [Serializable]
        public class BeginVisualActionResultDto
        {
            public string status;
            public string jobId;
            public bool actionSuccess;
            public string reason;
            public float captureDelaySeconds;
        }

        [Serializable]
        public class VisualActionResultDto
        {
            public string status;
            public string jobId;
            public bool success;
            public string reason;
            public string scene;
            public string state;
            public bool changed;
            public string screenshot;
            public int screenshotWidth;
            public int screenshotHeight;
            public bool onboardingComplete;
            public bool requiresDiagnosis;
        }

        private class CompactStateDto
        {
            public string scene;
            public string fingerprint;
            public bool mainPanelActive;
            public bool videoActive;
            public bool rewardActive;
            public bool guideActive;
        }

        private class VisualActionJob
        {
            public string jobId;
            public string status;
            public ActionRequestDto request;
            public bool actionSuccess;
            public string reason;
            public double startedAt;
            public double captureAt;
            public CompactStateDto before;
            public VisualActionResultDto result;
        }

        private class SimulatedSwipe
        {
            public GameObject target;
            public PointerEventData pointer;
            public Vector2 start;
            public Vector2 end;
            public Vector2 last;
            public double startedAt;
            public double duration;
        }

        private class SimulatedTap
        {
            public MonoBehaviour inputManager;
            public int startFrame;
        }

        [Serializable]
        public class Vector2Dto
        {
            public float x;
            public float y;
        }

        private class CandidateSelectionDto
        {
            public bool success;
            public string reason;
            public CandidateDto candidate;

            public static CandidateSelectionDto Ok(CandidateDto candidate, string reason)
            {
                return new CandidateSelectionDto { success = true, candidate = candidate, reason = reason };
            }

            public static CandidateSelectionDto Fail(string reason)
            {
                return new CandidateSelectionDto { success = false, reason = reason };
            }
        }

        [Serializable]
        public class SessionDto
        {
            public string sessionId;
            public string label;
            public float startedAt;
            public bool active;
            public ObserveDto lastObserve;
            public List<ActionSummaryDto> actions = new();
            public List<string> warnings = new();
            public List<string> errors = new();
            public List<string> screenshots = new();
        }

        private class AIGameTestSession
        {
            public string SessionId { get; private set; } = "not-started";
            public string Label { get; private set; } = string.Empty;
            public float StartedAt { get; private set; }
            public bool Active { get; private set; }
            public ObserveDto LastObserve;
            public readonly List<ActionResultDto> Actions = new();
            public readonly List<string> Warnings = new();
            public readonly List<string> Errors = new();
            public readonly List<string> Screenshots = new();
            public readonly Dictionary<string, double> LastClickedAt = new();
            public string LastNoChangePath = string.Empty;

            public void Reset(string label)
            {
                SessionId = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                Label = label ?? string.Empty;
                StartedAt = (float)EditorApplication.timeSinceStartup;
                Active = true;
                LastObserve = null;
                Actions.Clear();
                Warnings.Clear();
                Errors.Clear();
                Screenshots.Clear();
                LastClickedAt.Clear();
                LastNoChangePath = string.Empty;
                VisualJobs.Clear();
                if (visualJobTickRegistered)
                {
                    EditorApplication.update -= TickVisualJobs;
                    visualJobTickRegistered = false;
                }
            }

            public void Stop()
            {
                Active = false;
            }

            public void AddAction(ActionResultDto action)
            {
                if (action == null) return;
                Actions.Add(action);
                if (Actions.Count > 80)
                    Actions.RemoveAt(0);
                LastObserve = action.after;
                if (!action.success && action.reason != null && action.reason.Contains("observe summary unchanged") && action.action != null)
                    LastNoChangePath = action.action.path;
            }

            public void AddScreenshot(string path)
            {
                if (string.IsNullOrWhiteSpace(path)) return;
                Screenshots.Add(path);
                if (Screenshots.Count > 40)
                    Screenshots.RemoveAt(0);
            }

            public SessionDto ToDto()
            {
                return new SessionDto
                {
                    sessionId = SessionId,
                    label = Label,
                    startedAt = StartedAt,
                    active = Active,
                    lastObserve = LastObserve,
                    actions = Actions.TakeLast(30).Select(ToActionSummary).ToList(),
                    warnings = Warnings,
                    errors = Errors,
                    screenshots = Screenshots
                };
            }
        }
    }
}
#endif
