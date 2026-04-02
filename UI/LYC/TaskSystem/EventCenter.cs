using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public class EventCenter
    {
        private static readonly Dictionary<Type, HashSet<Delegate>> Subscriptions = new Dictionary<Type, HashSet<Delegate>>();
        private static readonly List<(Type type, Delegate action)> PendingSubscribes = new List<(Type, Delegate)>();
        private static readonly List<(Type type, Delegate action)> PendingUnsubscribes = new List<(Type, Delegate)>();

        private static bool _isPublishing = false;

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="action">处理方法</param>
        /// <typeparam name="TEvent">事件类型</typeparam>
        public static void Subscribe<TEvent>(Action<TEvent> action) where TEvent : IEvent
        {
            if (_isPublishing)
            {
                PendingSubscribes.Add((typeof(TEvent), action));
            }
            else
            {
                AddSubscription(typeof(TEvent), action);
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <param name="action">处理方法</param>
        /// <typeparam name="TEvent">事件类型</typeparam>
        public static void Unsubscribe<TEvent>(Action<TEvent> action) where TEvent : IEvent
        {
            if (_isPublishing)
            {
                PendingUnsubscribes.Add((typeof(TEvent), action));
            }
            else
            {
                RemoveSubscription(typeof(TEvent), action);
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="evt">事件</param>
        /// <typeparam name="TEvent">事件类型</typeparam>
        public static void Publish<TEvent>(TEvent evt) where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            if (!Subscriptions.TryGetValue(eventType, out var actions))
                return;

            _isPublishing = true;
            try
            {
                foreach (var action in new List<Delegate>(actions))
                {
                    try
                    {
                        (action as Action<TEvent>)?.Invoke(evt);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"EventCenter: Error handling event {eventType.Name}: {ex}");
                    }
                }
            }
            finally
            {
                _isPublishing = false;
                ProcessPendingOperations();
            }
        }

        private static void ProcessPendingOperations()
        {
            // 处理取消订阅
            foreach (var (type, action) in PendingUnsubscribes)
            {
                RemoveSubscription(type, action);
            }
            PendingUnsubscribes.Clear();

            // 处理新订阅
            foreach (var (type, action) in PendingSubscribes)
            {
                AddSubscription(type, action);
            }
            PendingSubscribes.Clear();
        }

        private static void AddSubscription(Type type, Delegate action)
        {
            if (!Subscriptions.TryGetValue(type, out var set))
            {
                set = new HashSet<Delegate>();
                Subscriptions[type] = set;
            }
            set.Add(action);
        }

        private static void RemoveSubscription(Type type, Delegate action)
        {
            if (Subscriptions.TryGetValue(type, out var set))
            {
                set.Remove(action);
                if (set.Count == 0)
                {
                    Subscriptions.Remove(type);
                }
            }
        }

        public static void Clear()
        {
            Subscriptions.Clear();
            PendingSubscribes.Clear();
            PendingUnsubscribes.Clear();
        }

        public interface IEvent { }
    }
}