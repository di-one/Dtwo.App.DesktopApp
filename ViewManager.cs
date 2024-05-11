using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtwo.App
{
    public static class ViewManager
    {
        public static Action? OnDarkModeChange;

        public static bool UseDarkMode { get; private set; }

        private static Action? m_onProgressStart;
        private static Action? m_onProgressStop;
        public static Action<int>? OnProgressStep; // Todo : autre implémentation

        private static List<object> m_lockedStates = new List<object>();

		public static bool IsOnProgress { get; private set; } = false;

        private static object m_lock = new object();

        public static void RegisterOnProgressStart(Action action)
        {
            m_onProgressStart += action;
        }

        public static void RegisterOnProgressStop(Action action)
        {
            m_onProgressStop += action;
        }

        public static void UnregisterOnProgressStart(Action action)
        {
            m_onProgressStart -= action;
        }

        public static void UnregisterOnProgressStop(Action action)
        {
            m_onProgressStop -= action;
        }

        public static void ProgressStart(object lockedState)
        {
            lock (m_lock) // MultiThreading prevention
            {
                if (m_lockedStates.Contains(lockedState))
                {
                    return;
                }

                if (m_lockedStates.Count == 0)
                {
                    m_onProgressStart?.Invoke();
                }

                m_lockedStates.Add(lockedState);
            }
        }

        public static void ProgressStop(object lockedState)
        {
            lock (m_lock)  // MultiThreading prevention
            {
                if (m_lockedStates.Contains(lockedState) == false)
                {
                    return;
                }

                m_lockedStates.Remove(lockedState);

                if (m_lockedStates.Count == 0)
                {
                    m_onProgressStop?.Invoke();
                }
            }
        }
	}
}
