using System.Collections.Generic;
using System;
namespace CLIP.Framework_Core.Event
{
    public class EventException : Exception
    {
        public EventException(string message) : base(message)
        {
        }

        public EventException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class EvtDsp
    {
        public interface IEvtDspTriggerAddtionalCom
        {
            void TriggerEvt(string eventType);
            void TriggerEvt<T>(string eventType, T argl);
            void TriggerEvt<T, U>(string eventType, T argl, U arg2);
            void TriggerEvt<T, U, V>(string eventType, T argl, U arg2, V arg3);

            void TriggerEvt<T, U, V, W>(string eventType, T argl, U arg2, V arg3, W arg4);
            // R ReturnEvt<R>(string eventType);
            // R ReturnEvt<R, T>(string eventType,T arg1);
            // R ReturnEvt<R, T, U>(string eventType, T arg1,U arg2);
            // R ReturnEvt<R, T, U, V>(string eventType, T arg1, U arg2, V arg3);
            // R ReturnEvt<R, T, U, V, W>(string eventType, T arg1,U arg2,V arg3,W arg4);
        }


        public static EventController m_eventController = new EventController();

        public static Dictionary<string, Delegate> TheRouter
        {
            get { return EvtDsp.m_eventController.TheRouter; }
        }

        public static void MarkAsPermanent(string eventType)
        {
            EvtDsp.m_eventController.MarkAsPermanent(eventType);
        }

        public static void Cleanup()
        {
            EvtDsp.m_eventController.Cleanup();
        }

        public static void AddReturnEvt<R>(string eventType, Func<R> handler)
        {
            EvtDsp.m_eventController.AddReturnEventListener(eventType, handler);
        }

        public static void AddReturnEvt<T, R>(string eventType, Func<T, R> handler)
        {
            EvtDsp.m_eventController.AddReturnEventListener(eventType, handler);
        }

        public static void AddReturnEvt<T, U, R>(string eventType, Func<T, U, R> handler)
        {
            EvtDsp.m_eventController.AddReturnEventListener(eventType, handler);
        }

        public static void AddReturnEvt<T, U, V, R>(string eventType, Func<T, U, V, R> handler)
        {
            EvtDsp.m_eventController.AddReturnEventListener(eventType, handler);
        }

        public static void RemoveReturnEvt<R>(string eventType, Func<R> handler)
        {
            EvtDsp.m_eventController.RemoveReturnEventListener(eventType, handler);
        }

        public static void RemoveReturnEvt<T, R>(string eventType, Func<T, R> handler)
        {
            EvtDsp.m_eventController.RemoveReturnEventListener(eventType, handler);
        }

        public static void RemoveReturnEvt<T, U, R>(string eventType, Func<T, U, R> handler)
        {
            EvtDsp.m_eventController.RemoveReturnEventListener(eventType, handler);
        }

        public static void RemoveReturnEvt<T, U, V, R>(string eventType, Func<T, U, V, R> handler)
        {
            EvtDsp.m_eventController.RemoveReturnEventListener(eventType, handler);
        }

        public static void AddEvt(string eventType, Action handler)
        {
            EvtDsp.m_eventController.AddEventListener(eventType, handler);
        }

        public static void AddEvt<T>(string eventType, Action<T> handler)
        {
            EvtDsp.m_eventController.AddEventListener<T>(eventType, handler);
        }

        public static void AddEvt<T, U>(string eventType, Action<T, U> handler)
        {
            EvtDsp.m_eventController.AddEventListener<T, U>(eventType, handler);
        }

        public static void AddEvt<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            EvtDsp.m_eventController.AddEventListener<T, U, V>(eventType, handler);
        }

        public static void AddEvt<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
            EvtDsp.m_eventController.AddEventListener<T, U, V, W>(eventType, handler);
        }

        public static void RemoveEvt(string eventType, Action handler)
        {
            EvtDsp.m_eventController.RemoveEventListener(eventType, handler);
        }

        public static void RemoveEvt<T>(string eventType, Action<T> handler)
        {
            EvtDsp.m_eventController.RemoveEventListener<T>(eventType, handler);
        }

        public static void RemoveEvt<T, U>(string eventType, Action<T, U> handler)
        {
            EvtDsp.m_eventController.RemoveEventListener<T, U>(eventType, handler);
        }

        public static void RemoveEvt<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            EvtDsp.m_eventController.RemoveEventListener<T, U, V>(eventType, handler);
        }

        public static void RemoveEvt<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
            EvtDsp.m_eventController.RemoveEventListener<T, U, V, W>(eventType, handler);
        }

        #region 为了外部业务层做一个消息挂钩机制-

        static public IEvtDspTriggerAddtionalCom extraTrigger = null;
        #endregion
        public static R ReturnEvt<R>(string eventType)
        {
            return EvtDsp.m_eventController.ReturnEvent<R>(eventType);
        }

        public static R ReturnEvt<T, R>(string eventType, T arg1)
        {
            return EvtDsp.m_eventController.ReturnEvent<T, R>(eventType, arg1);
        }

        public static R ReturnEvt<T, U, R>(string eventType, T argl, U arg2)
        {
            return EvtDsp.m_eventController.ReturnEvent<T, U, R>(eventType, argl, arg2);
        }

        public static R ReturnEvt<T, U, V, R>(string eventType, T argl, U arg2, V arg3)
        {
            return EvtDsp.m_eventController.ReturnEvent<T, U, V, R>(eventType, argl, arg2, arg3);
        }

        public static void TriggerEvt(string eventType)
        {
            EvtDsp.m_eventController.TriggerEvent(eventType);
            if (extraTrigger != null)
            {
                extraTrigger.TriggerEvt(eventType);
            }
        }

        public static void TriggerEvt<T>(string eventType, T argl)
        {
            EvtDsp.m_eventController.TriggerEvent<T>(eventType, argl);
            if (extraTrigger != null)
            {
                extraTrigger.TriggerEvt<T>(eventType, argl);
            }
        }

        public static void TriggerEvt<T, U>(string eventType, T arg1, U arg2)
        {
            EvtDsp.m_eventController.TriggerEvent<T, U>(eventType, arg1, arg2);
            if (extraTrigger != null)
            {
                extraTrigger.TriggerEvt<T, U>(eventType, arg1, arg2);
            }
        }

        public static void TriggerEvt<T, U, V>(string eventType, T arg1, U arg2, V arg3)
        {
            EvtDsp.m_eventController.TriggerEvent<T, U, V>(eventType, arg1, arg2, arg3);
            if (extraTrigger != null)
            {
                extraTrigger.TriggerEvt<T, U, V>(eventType, arg1, arg2, arg3);
            }
        }

        public static void TriggerEvt<T, U, V, W>(string eventType, T arg1, U arg2, V arg3, W arg4)
        {
            EvtDsp.m_eventController.TriggerEvent<T, U, V, W>(eventType, arg1, arg2, arg3, arg4);
            if (extraTrigger != null)
            {
                extraTrigger.TriggerEvt<T, U, V, W>(eventType, arg1, arg2, arg3, arg4);
            }
        }

    }

    public class EventController
    {
        private Dictionary<string, Delegate> m_theRouter = new Dictionary<string, Delegate>();
        private Dictionary<string, Delegate> m_theReturnRouter = new Dictionary<string, Delegate>();
        private List<string> m_permanentEvents = new List<string>();

        public Dictionary<string, Delegate> TheRouter
        {
            get { return this.m_theRouter; }
        }

        public void MarkAsPermanent(string eventType)
        {
            this.m_permanentEvents.Add(eventType);
        }

        public bool ContainsEvent(string eventType)
        {
            return this.m_theRouter.ContainsKey(eventType);
        }

        public void Cleanup()
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, Delegate> current in this.m_theRouter)
            {
                bool flag = false;

                foreach (string current2 in this.m_permanentEvents)
                {
                    if (current.Key == current2)
                    {
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    list.Add(current.Key);
                }
            }

            foreach (string current2 in list)
            {
                this.m_theRouter.Remove(current2);
            }

            foreach (KeyValuePair<string, Delegate> current in this.m_theReturnRouter)
            {
                bool flag = false;
                foreach (string current2 in this.m_permanentEvents)
                {
                    if (current.Key == current2)
                    {
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    list.Add(current.Key);
                }

            }
            foreach (string current2 in list)
            {
                this.m_theReturnRouter.Remove(current2);
            }
        }


        private void OnListenerReturnAdding(string eventType, Delegate listenerBeingAdded)
        {
            if (!this.m_theReturnRouter.ContainsKey(eventType))
            {
                this.m_theReturnRouter.Add(eventType, null);
            }

            Delegate @delegate = this.m_theReturnRouter[eventType];
            if (@delegate != null && @delegate.GetType() != listenerBeingAdded.GetType())
            {
                throw new EventException(string.Format(
                    "Try to add not correct event{0}. Current type is {1},adding type is {2}.", eventType,
                    @delegate.GetType(), listenerBeingAdded.GetType().Name));
            }

        }

        private bool OnListenerReturnRemoving(string eventType, Delegate listenerBeingRemoyed)
        {
            bool result;

            if (!this.m_theReturnRouter.ContainsKey(eventType))
            {
                result = false;
            }
            else
            {
                Delegate @delegate = this.m_theReturnRouter[eventType];
                if (@delegate != null && @delegate.GetType() != listenerBeingRemoyed.GetType())
                {
                    throw new EventException(string.Format(
                        "Remove listener {0}\' failed, Curype is {1}, adding type is (2).", eventType,
                        @delegate.GetType(),
                        listenerBeingRemoyed.GetType()));
                }

                result = true;
            }

            return result;
        }

        private void OnListenerReturnRemoved(string eventType)
        {
            if (this.m_theReturnRouter.ContainsKey(eventType) && this.m_theReturnRouter[eventType] == null)
                this.m_theReturnRouter.Remove(eventType);
        }


        private void OnListenerAdding(string eventType, Delegate listenerBeingAdded)
        {
            if (!this.m_theRouter.ContainsKey(eventType))
            {
                this.m_theRouter.Add(eventType, null);
            }

            Delegate @delegate = this.m_theRouter[eventType];
            if (@delegate != null && @delegate.GetType() != listenerBeingAdded.GetType())
            {
                throw new EventException(string.Format(
                    "Try to add not correct event {0}. Current type is {1}, adding type is {2}.", eventType,
                    @delegate.GetType().Name, listenerBeingAdded.GetType().Name));
            }
        }

        private bool OnListenerRemoving(string eventType, Delegate listenerBeingRemoved)
        {
            bool result;
            if (!this.m_theRouter.ContainsKey(eventType))
            {
                result = false;
            }
            else
            {
                Delegate @delegate = this.m_theRouter[eventType];
                if (@delegate != null && @delegate.GetType() != listenerBeingRemoved.GetType())
                {
                    throw new EventException(string.Format("Remove listener: {0} failed, Current type{1} is {2}."
                        , eventType,
                        @delegate.GetType(), listenerBeingRemoved.GetType()));
                }
                result = true;
            }
            return result;

        }


        private void OnListenerRemoved(string eventType)
        {
            if (this.m_theRouter.ContainsKey(eventType) & this.m_theRouter[eventType] == null)
                this.m_theRouter.Remove(eventType);
        }



        public void AddReturnEventListener<R>(string eventType, Func<R> handler)
        {
            this.OnListenerReturnAdding(eventType, handler);
            this.m_theReturnRouter[eventType] =
                (Func<R>)Delegate.Combine((Func<R>)this.m_theReturnRouter[eventType], handler);
        }

        public void AddReturnEventListener<T, R>(string eventType, Func<T, R> handler)
        {
            this.OnListenerReturnAdding(eventType, handler);
            this.m_theReturnRouter[eventType] =
                (Func<T, R>)Delegate.Combine((Func<T, R>)this.m_theReturnRouter[eventType], handler);
        }

        public void AddReturnEventListener<T, U, R>(string eventType, Func<T, U, R> handler)
        {
            this.OnListenerReturnAdding(eventType, handler);
            this.m_theReturnRouter[eventType] =
                (Func<T, U, R>)Delegate.Combine((Func<T, U, R>)this.m_theReturnRouter[eventType], handler);
        }

        public void AddReturnEventListener<T, U, V, R>(string eventType, Func<T, U, V, R> handler)
        {
            this.OnListenerReturnAdding(eventType, handler);
            this.m_theReturnRouter[eventType] =
                (Func<T, U, V, R>)Delegate.Combine((Func<T, U, V, R>)this.m_theReturnRouter[eventType], handler);
        }

        public void RemoveReturnEventListener<R>(string eventType, Func<R> handler)
        {
            if (this.OnListenerReturnRemoving(eventType, handler))
            {
                this.m_theReturnRouter[eventType] =
                    (Func<R>)Delegate.Remove((Func<R>)this.m_theReturnRouter[eventType], handler);
                this.OnListenerReturnRemoved(eventType);
            }
        }

        public void RemoveReturnEventListener<T, R>(string eventType, Func<T, R> handler)
        {
            if (this.OnListenerReturnRemoving(eventType, handler))
            {
                this.m_theReturnRouter[eventType] =
                    (Func<T, R>)Delegate.Remove((Func<T, R>)this.m_theReturnRouter[eventType], handler);
                this.OnListenerReturnRemoved(eventType);
            }

        }

        public void RemoveReturnEventListener<T, U, R>(string eventType, Func<T, U, R> handler)
        {
            if (this.OnListenerReturnRemoving(eventType, handler))
            {
                this.m_theReturnRouter[eventType] =
                    (Func<T, U, R>)Delegate.Remove((Func<T, U, R>)this.m_theReturnRouter[eventType], handler);
                this.OnListenerReturnRemoved(eventType);
            }
        }

        public void RemoveReturnEventListener<T, U, V, R>(string eventType, Func<T, U, V, R> handler)
        {
            if (this.OnListenerReturnRemoving(eventType, handler))
            {
                this.m_theReturnRouter[eventType] =
                    (Func<T, U, V, R>)Delegate.Remove((Func<T, U, V, R>)this.m_theReturnRouter[eventType], handler);
                this.OnListenerReturnRemoved(eventType);
            }

        }



        public void AddEventListener(string eventType, Action handler)
        {
            this.OnListenerAdding(eventType, handler);
            this.m_theRouter[eventType] = (Action)Delegate.Combine((Action)this.m_theRouter[eventType], handler);
        }

        public void AddEventListener<T>(string eventType, Action<T> handler)
        {
            this.OnListenerAdding(eventType, handler);
            this.m_theRouter[eventType] = (Action<T>)Delegate.Combine((Action<T>)this.m_theRouter[eventType], handler);
        }

        public void AddEventListener<T, U>(string eventType, Action<T, U> handler)
        {
            this.OnListenerAdding(eventType, handler);
            this.m_theRouter[eventType] =
                (Action<T, U>)Delegate.Combine((Action<T, U>)this.m_theRouter[eventType], handler);
        }

        public void AddEventListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            this.OnListenerAdding(eventType, handler);
            this.m_theRouter[eventType] =
                (Action<T, U, V>)Delegate.Combine((Action<T, U, V>)this.m_theRouter[eventType], handler);
        }

        public void AddEventListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
            this.OnListenerAdding(eventType, handler);
            this.m_theRouter[eventType] =
                (Action<T, U, V, W>)Delegate.Combine((Action<T, U, V, W>)this.m_theRouter[eventType], handler);
        }



        public void RemoveEventListener(string eventType, Action handler)
        {
            if (this.OnListenerRemoving(eventType, handler))
            {
                this.m_theRouter[eventType] = (Action)Delegate.Remove((Action)this.m_theRouter[eventType], handler);
                this.OnListenerRemoved(eventType);
            }
        }

        public void RemoveEventListener<T>(string eventType, Action<T> handler)
        {
            if (this.OnListenerRemoving(eventType, handler))
            {
                this.m_theRouter[eventType] =
                    (Action<T>)Delegate.Remove((Action<T>)this.m_theRouter[eventType], handler);
                this.OnListenerRemoved(eventType);
            }
        }

        public void RemoveEventListener<T, U>(string eventType, Action<T, U> handler)
        {
            if (this.OnListenerRemoving(eventType, handler))
            {
                this.m_theRouter[eventType] = (Action<T, U>)Delegate.Remove(
                    this.m_theRouter[eventType], handler);
                this.OnListenerRemoved(eventType);
            }
        }

        public void RemoveEventListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            if (this.OnListenerRemoving(eventType, handler))
            {
                this.m_theRouter[eventType] = (Action<T, U, V>)Delegate.Remove(
                    (Action<T, U, V>)this.m_theRouter[eventType], handler);
                this.OnListenerRemoved(eventType);
            }
        }

        public void RemoveEventListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
            if (this.OnListenerRemoving(eventType, handler))
            {
                this.m_theRouter[eventType] =
                    (Action<T, U, V, W>)Delegate.Remove((Action<T, U, V, W>)this.m_theRouter[eventType], handler);
                this.OnListenerRemoved(eventType);

            }
        }



        public R ReturnEvent<R>(string eventType)
        {
            Delegate @delegate;
            if (this.m_theReturnRouter.TryGetValue(eventType, out @delegate))
            {
                Delegate[] invocationList = @delegate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Func<R> func = invocationList[i] as Func<R>;
                    if (func == null)
                    {
                        throw new EventException(string.Format(
                            "TriggerEvent {0}error: types of parameters are not match.",
                            eventType));
                    }

                    try
                    {
                        return func();
                    }
                    catch (Exception ex)
                    {
                        //Debug.LogError(string.Format("{0} at {1} at {2}", ex.Message, func.Target.ToString(),
                        //    func.Method.Name));
                    }
                }

            }

            return default;
        }
        public R ReturnEvent<T, R>(string eventType, T arg1)
        {
            Delegate @delegate;
            if (this.m_theReturnRouter.TryGetValue(eventType, out @delegate))
            {
                Delegate[] invocationList = @delegate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Func<T, R> func = invocationList[i] as Func<T, R>;
                    if (func == null)
                    {
                        throw new EventException(string.Format(
                            "TriggerEvent {0}error: types of parameters are not match.",
                            eventType));
                    }

                    try
                    {
                        return func(arg1);
                    }
                    catch (Exception ex)
                    {
                        //Debug.LogError(string.Format("{0} at {1} at {2}", ex.Message, func.Target.ToString(),
                        //    func.Method.Name));
                    }
                }

            }

            return default;
        }

        public R ReturnEvent<T, V, R>(string eventType, T arg1, V arg2)
        {
            Delegate @delegate;
            if (this.m_theReturnRouter.TryGetValue(eventType, out @delegate))
            {
                Delegate[] invocationList = @delegate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Func<T, V, R> func = invocationList[i] as Func<T, V, R>;
                    if (func == null)
                    {
                        throw new EventException(string.Format(
                            "TriggerEvent {0}error: types of parameters are not match.",
                            eventType));
                    }

                    try
                    {
                        return func(arg1, arg2);
                    }
                    catch (Exception ex)
                    {
                        //Debug.LogError(string.Format("{0} at {1} at {2}", ex.Message, func.Target.ToString(),
                        //    func.Method.Name));
                    }
                }

            }

            return default;
        }

        public R ReturnEvent<T, V, U, R>(string eventType, T arg1, V arg2, U arg3)
        {
            Delegate @delegate;
            if (this.m_theReturnRouter.TryGetValue(eventType, out @delegate))
            {
                Delegate[] invocationList = @delegate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Func<T, V, U, R> func = invocationList[i] as Func<T, V, U, R>;
                    if (func == null)
                    {
                        throw new EventException(string.Format(
                            "TriggerEvent {0}error: types of parameters are not match.",
                            eventType));
                    }

                    try
                    {
                        return func(arg1, arg2, arg3);
                    }
                    catch (Exception ex)
                    {
                        //Debug.LogError(string.Format("{0} at {1} at {2}", ex.Message, func.Target.ToString(),
                        //    func.Method.Name));
                    }
                }

            }

            return default;
        }


        public void TriggerEvent(string eventType)
        {
            Delegate @delegate;
            if (this.m_theRouter.TryGetValue(eventType, out @delegate))
            {
                Delegate[] invocationList = @delegate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Action action = invocationList[i] as Action;
                    if (action == null)
                    {
                        throw new EventException(string.Format(
                            "TriggerEvent {0}error: types of parameters are not match.",
                            eventType));
                    }

                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        //Debug.LogError(ex.Message);
                    }

                }
            }
        }


        public void TriggerEvent<T>(string eventType, T arg1)
        {
            Delegate @delegate;
            if (this.m_theRouter.TryGetValue(eventType, out @delegate))
            {
                Delegate[] invocationList = @delegate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Action<T> action = invocationList[i] as Action<T>;
                    if (action == null)
                    {
                        throw new EventException(string.Format(
                            "TriggerEvent {0}error: types of parameters are not match.",
                            eventType));
                    }

                    try
                    {
                        action(arg1);
                    }
                    catch (Exception ex) {
                        //Debug.LogError(ex.Message);
                    }
                }
            }
        }

        public void TriggerEvent<T, U>(string eventType, T arg1, U arg2)
        {
            Delegate @delegate;
            if (this.m_theRouter.TryGetValue(eventType, out @delegate))
            {
                Delegate[] invocationList = @delegate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Action<T, U> action = invocationList[i] as Action<T, U>;
                    if (action == null)
                    {
                        throw new EventException(string.Format(
                            "TriggerEvent {0}error: types of parameters are not match.",
                            eventType));
                    }

                    try
                    {
                        action(arg1, arg2);
                    }
                    catch
                    {
                        //LoggerHelper.Except(ex, null);}
                    }

                }
            }
        }

        public void TriggerEvent<T, U, V>(string eventType, T arg1, U arg2, V arg3)
        {
            Delegate @delegate;
            if (this.m_theRouter.TryGetValue(eventType, out @delegate))
            {
                Delegate[] invocationList = @delegate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Action<T, U, V> action = invocationList[i] as Action<T, U, V>;
                    if (action == null)
                    {
                        throw new EventException(string.Format(
                            "TriggerEvent {0}error: types of parameters are not match.",
                            eventType));
                    }

                    try
                    {
                        action(arg1, arg2, arg3);
                    }
                    catch
                    {
                        //LoggerHelper.Except(ex, null);}
                    }

                }
            }
        }

        public void TriggerEvent<T, U, V, W>(string eventType, T arg1, U arg2, V arg3, W arg4)
        {
            Delegate @delegate;
            if (this.m_theRouter.TryGetValue(eventType, out @delegate))
            {
                Delegate[] invocationList = @delegate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Action<T, U, V, W> action = invocationList[i] as Action<T, U, V, W>;
                    if (action == null)
                    {
                        throw new EventException(string.Format(
                            "TriggerEvent {0}error: types of parameters are not match.",
                            eventType));
                    }

                    try
                    {
                        action(arg1, arg2, arg3, arg4);
                    }
                    catch
                    {
                        //LoggerHelper.Except(ex, null);}
                    }

                }
            }
        }

    }
}