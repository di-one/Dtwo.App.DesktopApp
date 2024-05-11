
#if WINDOWS
using Dtwo;
using Dtwo.API;
using Dtwo.App;
using Dtwo.App.DesktopApp;
using Dtwo.App.DesktopApp.Windowing;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using System.Runtime.InteropServices;
using Windows.Graphics;
#endif


namespace Dtwo.App.DesktopApp.Windowing
{
#if WINDOWS
    public class CustomWindow
    {
        private static Dictionary<API.View.Window, CustomWindow> m_windows = new Dictionary<API.View.Window, CustomWindow>();

        private API.View.Window m_windowTemplate;
        private Window m_windowSystem;

        private nint m_hwnd;
        private static AppWindow m_appWindow;
        private WindowDrag m_windowDrag;

        public CustomWindow(API.View.Window windowTemplate, Window windowSystem)
        {
            m_windowTemplate = windowTemplate;
            m_windowSystem = windowSystem;
        }

        public static void Init()
        {
            API.View.Window.OnCreateWindow += OnCreateWindow;
            API.View.Window.OnCloseWindow += OnCloseWindow;
            API.View.Window.OnResizeWindow += OnResizeWindow;
            API.View.Window.OnStartDragWindow += OnStartDragWindow;
        }

        #region Static Methods
        private static void OnCloseWindow(API.View.Window window)
        {
            CustomWindow customWindow = null;
            if (m_windows.TryGetValue(window, out customWindow))
            {
                customWindow.CloseWindow();
            }
        }

        private static void OnCreateWindow(API.View.Window window, Action<nint?>? onCreateCallback = null)
        {
            MainThreadRequestHandler.RequestWindowOpen(window, (createdWindow) =>
            {
                CustomWindow customWindow = new CustomWindow(window, createdWindow);

                m_windows.Add(window, customWindow);
                customWindow.CreateWindow(onCreateCallback);
            });
        }

        private static void OnResizeWindow(API.View.Window window, API.View.Window.ResizeWindowOptions options)
        {
            CustomWindow customWindow = null;
            if (m_windows.TryGetValue(window, out customWindow))
            {
                customWindow.ResizeWindow(options);
            }
        }

        private static void OnStartDragWindow(API.View.Window window)
        {
            CustomWindow customWindow = null;
            if (m_windows.TryGetValue(window, out customWindow))
            {
                customWindow.StartDragWindow();
            }
        }

        #endregion

        public void CreateWindow(Action<nint?>? onCreateCallback)
        {
#if WINDOWS
            var xamlWindow = m_windowSystem.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(xamlWindow);

            m_hwnd = hwnd;

            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            m_appWindow = AppWindow.GetFromWindowId(windowId);

            m_windowDrag = new WindowDrag(m_appWindow);

            onCreateCallback?.Invoke(hwnd);
#endif
        }

        public void CloseWindow()
        {
#if WINDOWS
            MainThreadRequestHandler.AddRequest(() =>
            {
                Application.Current.CloseWindow(m_windowSystem);
                m_windows.Remove(m_windowTemplate);
            });
#endif
        }

        public void ResizeWindow(API.View.Window.ResizeWindowOptions options)
        {
#if WINDOWS
            if (m_windowSystem.Handler?.PlatformView is Microsoft.UI.Xaml.Window xamlWindow)
            {
                m_appWindow.Resize(new SizeInt32((int)options.Width, (int)options.Height));
            }
#endif
        }

        public void StartDragWindow()
        {
            m_windowDrag.StartDragWindow();
        }
    }
#endif

}
