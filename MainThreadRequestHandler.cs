using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtwo.App.DesktopApp.Windowing;

namespace Dtwo.App.DesktopApp
{
    public static class MainThreadRequestHandler
    {
        private static ConcurrentQueue<Action> m_requests = new ConcurrentQueue<Action>();
        private static BindableObject m_context;

        public static void StartListenner()
        {
            m_context = AppManager.MainWindow;

            CheckQueue();
        }

        private static void CheckQueue()
        {
            if (!m_context.Dispatcher.IsDispatchRequired)
            {
                while (m_requests.TryDequeue(out var action))
                {
                    action.Invoke();
                }
            }

            m_context.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () => CheckQueue());
        }

        public static void RequestWindowOpen(API.View.Window windowTemplate, Action<Window>? onCreate = null)
        {
            m_requests.Enqueue(() =>
            {
                var window = WindowFactory.CreateWindow(windowTemplate);
                onCreate?.Invoke(window);
            });
        }

        public static void AddRequest(Action callback)
        {
            callback.Invoke();
        }
    }
}
