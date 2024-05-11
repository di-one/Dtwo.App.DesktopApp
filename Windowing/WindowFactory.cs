using Dtwo.API;
using Microsoft.AspNetCore.Components.WebView.Maui;

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtwo.App.DesktopApp.Windowing
{
    public static class WindowFactory
    {
        public static Window CreateWindow(API.View.Window window)
        {
            Window systemWindow = new Window();

            systemWindow.Created += (args, obj) => OnWindowCreated(systemWindow, window);

            systemWindow.Title = window.Options.Title;
            systemWindow.Width = window.Options.Width;
            systemWindow.Height = window.Options.Height;
            systemWindow.X = window.Options.PositionX;
            systemWindow.Y = window.Options.PositionY;

            string hostPage = window.Options.RenderPath;

            var blazorWebView = new BlazorWebView()
            {
                HostPage = hostPage,
                RootComponents = { new RootComponent { ComponentType = window.Options.Type, Selector = "#" + window.Options.RenderSelectorId } }
            };

            var page = new ContentPage
            {
                Content = blazorWebView
            };

            systemWindow.Page = page;

            try
            {
                Application.Current.OpenWindow(systemWindow);
            }
            catch (Exception ex)
            {
               LogManager.LogError(
                   $"{nameof(WindowFactory)}.{CreateWindow}",
                   "ex : " + ex.Message + " " + ex.ToString());
            }


            return systemWindow;
        }

        private static void OnWindowCreated(Window systemWindow, API.View.Window windowTemplate)
        {
#if WINDOWS
            if (systemWindow.Handler?.PlatformView is Microsoft.UI.Xaml.Window window)
            {
                nint nativeWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WindowId win32WindowsId = Win32Interop.GetWindowIdFromWindow(nativeWindowHandle);
                AppWindow winuiAppWindow = AppWindow.GetFromWindowId(win32WindowsId);

                var presenter = winuiAppWindow.Presenter as OverlappedPresenter;

                presenter.IsAlwaysOnTop = windowTemplate.Options.TopMost;
                presenter.IsResizable = windowTemplate.Options.Resizable;

                if (windowTemplate.Options.HideTitleBar)
                {
                    window.ExtendsContentIntoTitleBar = false;
                    if (winuiAppWindow.Presenter is OverlappedPresenter p)
                    {
                        p.SetBorderAndTitleBar(false, false);
                    }
                }
            }
#endif
        }
    }
}
