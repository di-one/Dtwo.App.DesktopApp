#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

using Dtwo.API;
using Dtwo.API.DofusBase;
using Dtwo.API.View.Components.Layout;
using Dtwo.Core;
using Dtwo.Core.Dofus2;
using MudBlazor;
using Dtwo.API.View.Components;
using System.Diagnostics;
using Dtwo.Plugins;
using System.Collections.Generic;
using Dtwo.App.DesktopApp.Windowing;
using System.Xml;
using Dtwo.API.Inputs;


namespace Dtwo.App.DesktopApp
{
    public static class AppManager
    {
        private static Window m_mainWindow;
        public static Window? MainWindow
        {
            get
            {
#if WINDOWS
                if (m_mainWindow == null)
                {
                    m_mainWindow = Application.Current.Windows[0];
                }
#endif
                return m_mainWindow;
            }
        }
        public static CoreBase? Core { get; private set; }
        public static ComponentsProviderSettings ComponentsProviderSettings { get; private set; }
        public static MainLayoutSettings MainLayoutSettings { get; private set; }

#if WINDOWS
        public static WindowDrag MainWindowDrag;
#endif

        public static Dtwo.API.DofusBase.Data.DofusBindingInfos BindingInfos { get; set; }

        public static Action OnMainLayoutSettingsUpdated { get; set; }

        public static bool DofusVersionIsSelected { get; private set; } 

        public static void StartCore(EDofusVersion dofusVersion, Action<bool> onCoreStarted)
        {
            if (Core != null)
            {
                LogManager.LogWarning("Core already started", 1);
                return;
            }

            object lockedState = new object();
            StartProgress(lockedState);

            if (dofusVersion == EDofusVersion.Retro)
            {
                Core = new RetroCore();
            }
            else
            {
                Core = new Dofus2Core();
            }

            Core.Start(dofusVersion, null, (success) =>
            {
                DofusVersionIsSelected = true;
                Dtwo.Core.Plugins.PluginsManager.OnCoreLoaded();
                UpdateMainLayoutSettings();

                if (success)
                {
                    onCoreStarted?.Invoke(success);
                }
                else
                {
                    LogManager.LogError("Error on start core", 1);
                }

				StopProgress(lockedState);
            }, ViewManager.OnProgressStep);
        }

        public static void StopProgress(object lockedState)
        {
            ViewManager.ProgressStop(lockedState);
        }

        public static void StartProgress(object lockedState)
        {
            ViewManager.ProgressStart(lockedState);
        }

        public static void Init(ComponentsProviderSettings componentsProviderSettings)
        {
            WindowsEventsListener.StartListen();
            InputKeyListener.Instance.StartListen();
            InputKeyWindow.Init();

            LoadBindingInfos();

            ComponentsProviderSettings = componentsProviderSettings;

            // test
            MainLayoutSettings = new MainLayoutSettings(GetDefaultSideBarEntries());

#if WINDOWS
            CustomWindow.Init();
            //MainWindow = Application.Current.Windows[0];
#endif
        }
#if WINDOWS
        public static void InitMainWindow(AppWindow window)
        {
            MainWindowDrag = new WindowDrag(window);
        }
#endif

        // Todo : Class for this


        private static List<MainLayoutSettings.SidebarEntry> GetDefaultSideBarEntries()
        {
            return new List<MainLayoutSettings.SidebarEntry>()
            {
                new MainLayoutSettings.SidebarEntry()
                {
                    Name = "Options",
                    Url = "/Options",
                    Icon = "settings"
                },
                new MainLayoutSettings.SidebarEntry()
                {
                    Name = "Setup",
                    Url = "/Setup",
                    Icon = "login",
                    IsVisible = () => !DofusVersionIsSelected
                },
                new MainLayoutSettings.SidebarEntry()
                {
                    Name = "Sniffer",
                    Url = "/Sniffer",
                    Icon = "hearing"
                }
            };
        }

        private static void UpdateMainLayoutSettings()
        {
            MainLayoutSettings.SideBarEntries.Clear();
            MainLayoutSettings.SideBarEntries.AddRange(GetDefaultSideBarEntries());

            List <MainLayoutSettings.SidebarEntry> entries = new List<MainLayoutSettings.SidebarEntry>();


            if (ViewPluginsManager.GetViewPlugins() != null)
            {
                foreach (var p in ViewPluginsManager.GetViewPlugins())
                {

                    foreach (var t in p.Pages)
                    {
                        if (t.Value.ShowInNavbar)
                        {
                            if (t.Value.NeedSelectedAccount && DofusVersionIsSelected == false)
                            {
                                continue;
                            }

                            string path = $"viewplugin/{t.Value.Route}";
                            entries.Add(new MainLayoutSettings.SidebarEntry()
                            {
                                Name = t.Value.Title,
                                Url = path,
                                Icon = "home" // Todo : get icon from plugin
                            });
                        }
                    }
                }
            }

            OnMainLayoutSettingsUpdated?.Invoke();
            MainLayoutSettings.SideBarEntries.AddRange(entries);
        }

        private static void LoadBindingInfos()
        {
            if (File.Exists(Paths.Dofus2BindingInfosPath) == false)
            {
                CreateAndSaveBindingInfos();
                return;
            }

            string content = File.ReadAllText(Paths.Dofus2BindingInfosPath);
            BindingInfos = Newtonsoft.Json.JsonConvert.DeserializeObject<Dtwo.API.DofusBase.Data.DofusBindingInfos>(content);

            if (BindingInfos == null)
            {
                CreateAndSaveBindingInfos();
            }
        }

        private static void CreateAndSaveBindingInfos() // Todo : create class for this
        {
            LogManager.Log("Create default binding infos");

            BindingInfos = new API.DofusBase.Data.DofusBindingInfos();
            SaveBindingInfos();
        }

        public static void SaveBindingInfos()
        {
            if (Paths.Dofus2BindingInfosPath == null)
            {
                LogManager.LogError("Error on save binding infos : Dofus2BindingInfosPath is null", 1);
                return;
            }

            if (Directory.Exists(Paths.Dofus2BindingInfosPath) == false)
            {
                Directory.CreateDirectory(Paths.Dofus2BindingInfosPath);
            }

            try
            {
                File.WriteAllText(Paths.Dofus2BindingInfosPath, Newtonsoft.Json.JsonConvert.SerializeObject(BindingInfos));
            }
            catch (Exception ex)
            {
                LogManager.LogError("Error on save binding infos : " + ex.Message, 1);
                return;
            }
        }

        public static void CloseWindow()
        {
#if WINDOWS
            Application.Current.CloseWindow(MainWindow);
#endif
        }

        public static void MinimizeWindow()
        {
#if WINDOWS
            var presenter = GetPresenter() as OverlappedPresenter;

            presenter?.Minimize();
#endif
        }

        public static void MaximizeWindow()
        {
#if WINDOWS
            var presenter = GetPresenter() as OverlappedPresenter;

            presenter?.Maximize();
#endif
        }

        public static void StartDragWindow()
        {
#if WINDOWS
            MainWindowDrag.StartDragWindow();
#endif
        }
#if WINDOWS
        private static AppWindowPresenter GetPresenter()
        {
            var nativeWindow = MainWindow.Handler.PlatformView;
            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            WindowId WindowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            AppWindow appWindow = AppWindow.GetFromWindowId(WindowId);

            return appWindow.Presenter;
        }
#endif
        public static void OnSelectDofusVersion()
        {
            DofusVersionIsSelected = true;
        }
    }
}
