using Dtwo.API;
using Microsoft.Extensions.Logging;
using MatBlazor;
using MudBlazor.Services;
using System.Diagnostics;
using System.Reflection;
using Dtwo.API.View.Components;
using Dtwo.API.View.Components.Feedback;
using Dtwo.API.Inputs;
using System.Runtime.CompilerServices;
using Dtwo.App.DesktopApp.ComponentsProvider;








#if WINDOWS
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Windowing;
using Microsoft.UI;
#endif

namespace Dtwo.App.DesktopApp
{
    public static class MauiProgram
    {
        private static bool m_windowInitialized = false;

        public static MauiApp CreateMauiApp()
        {
            ViewPluginsManager.Init();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            API.Paths.Init();

            LogManager.OnLog += (LogMessage message) =>
            {
                // Debug
                if (Configuration.Configuration.Instance.IsDebug)
                {
                    Debug.WriteLine(message.Text);
                }

                // Notification
                if (message.Priority == 1)
                {
                    Notification.ESeverity severity = Notification.ESeverity.Info;
                    if (message.Type == MessageType.Error)
                        severity = Notification.ESeverity.Error;
                    else if (message.Type == MessageType.Warning)
                        severity = Notification.ESeverity.Warning;

                    Notification.Notify(new Notification.NotificationMessage()
                    {
                        Severity = severity,
                        Title = message.Title,
                        Text = message.Text,
                    });
                }

                // Log file
                if (Configuration.Configuration.Instance.LogToFile)
                {
                    LogFile.WriteLog(message);
                }
            };          

            Assembly? themeAsm;
            var componentProvider = ComponentProviderLoader.LoadComponentsProvider("", out themeAsm);

            if (themeAsm != null)
            {
                ComponentResolver.Init(themeAsm);
            }

            if (componentProvider != null)
            {
                componentProvider.Init(builder.Services);
            }

            AppManager.Init(componentProvider);

#if WINDOWS
            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(windows =>
                    windows.OnWindowCreated(window =>
                    {
                        if (m_windowInitialized)
                            return;

                        var mauiWindow = window as MauiWinUIWindow;

                        IntPtr nativeWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                        var win32WindowsId = Win32Interop.GetWindowIdFromWindow(nativeWindowHandle);
                        var winuiAppWindow = AppWindow.GetFromWindowId(win32WindowsId);

                        window.ExtendsContentIntoTitleBar = false;


                        if (winuiAppWindow.Presenter is OverlappedPresenter p)
                        {
                            p.SetBorderAndTitleBar(false, false);
                        }

                        MainThreadRequestHandler.StartListenner();

                        AppManager.InitMainWindow(winuiAppWindow);

                        m_windowInitialized = true;
                    }));
            });
#endif

            var app = builder.Build();

            return builder.Build();
        }

    }
}
