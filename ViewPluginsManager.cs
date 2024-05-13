using Dtwo.API.View;
using Dtwo.API;
using Dtwo.Core.Plugins;
namespace Dtwo.App.DesktopApp
{
    internal class ViewPluginsManager
    {
        private static List<ViewPlugin> m_viewPlugins = new List<ViewPlugin>();

        public static List<ViewPlugin>? GetViewPlugins() => new List<ViewPlugin>(m_viewPlugins);

        public static void Init()
        {
            PluginsManager.OnLoadPluginEvent += OnLoadPlugin;
        }

        public static API.View.Page? FindViewPlugin(string name)
        {
            for (int i = 0; i < m_viewPlugins.Count; i++)
            {
                var viewPlugin = m_viewPlugins[i];
                if (viewPlugin.Pages.ContainsKey(name))
                {
                    return viewPlugin.Pages[name];
                }
            }

            return null;
        }


        public static void OnLoadPlugin(Plugin plugin)
        {
            LogManager.Log($"OnLoadPlugin 1 ");

            if (plugin == null)
                return;

            LogManager.Log("OnLoadPlugin 2 : " + plugin.Infos.Name);



            ViewPlugin vp = plugin as ViewPlugin;

            if (vp != null)
            {
                m_viewPlugins.Add(vp);
                PluginsViewHelper.AddViewPlugin(vp);
            }
        }
    }
}
