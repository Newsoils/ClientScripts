using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public static class EventCenter
    {
        private static readonly Dictionary<Type, HashSet<Delegate>> _subscriptions = new();
        private static readonly List<(Type type, Delegate action)> _pendingSubscribes = new();
        private static readonly List<(Type type, Delegate action)> _pendingUnsubscribes = new();
        private static bool _isPublishing;

        public static void Subscribe<TEvent>(Action<TEvent> action) where TEvent : class
        {
            if (_isPublishing)
                _pendingSubscribes.Add((typeof(TEvent), action));
            else
                AddSubscription(typeof(TEvent), action);
        }

        public static void Unsubscribe<TEvent>(Action<TEvent> action) where TEvent : class
        {
            if (_isPublishing)
                _pendingUnsubscribes.Add((typeof(TEvent), action));
            else
                RemoveSubscription(typeof(TEvent), action);
        }

        public static void Publish<TEvent>(TEvent evt) where TEvent : class
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out var actions))
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
                        Debug.LogError($"EventCenter: Error handling event {typeof(TEvent).Name}: {ex}");
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
            foreach (var (type, action) in _pendingUnsubscribes)
                RemoveSubscription(type, action);
            _pendingUnsubscribes.Clear();

            foreach (var (type, action) in _pendingSubscribes)
                AddSubscription(type, action);
            _pendingSubscribes.Clear();
        }

        private static void AddSubscription(Type type, Delegate action)
        {
            if (!_subscriptions.TryGetValue(type, out var set))
            {
                set = new HashSet<Delegate>();
                _subscriptions[type] = set;
            }
            set.Add(action);
        }

        private static void RemoveSubscription(Type type, Delegate action)
        {
            if (_subscriptions.TryGetValue(type, out var set))
            {
                set.Remove(action);
                if (set.Count == 0)
                    _subscriptions.Remove(type);
            }
        }

        public static void Clear()
        {
            _subscriptions.Clear();
            _pendingSubscribes.Clear();
            _pendingUnsubscribes.Clear();
        }
    }
}
