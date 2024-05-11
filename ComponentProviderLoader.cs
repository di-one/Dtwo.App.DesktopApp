using Dtwo.API.View.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtwo.App.DesktopApp
{
    internal class ComponentProviderLoader
    {
        public static ComponentsProviderSettings? LoadComponentsProviders()
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var asm in asms)
            {
                if (asm.FullName.Contains("Dtwo") == false)
                    continue;

                if (asm.FullName.Contains("ComponentsProvider") == false)
                {
                    continue;
                }

                var types = asm.GetTypes();
                foreach(var type in types)
                {
                    if (typeof(ComponentsProviderSettings).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                    {
                        var instance = (ComponentsProviderSettings)Activator.CreateInstance(type);
                        return instance;
                    }
                }
            }

            return null;
        }
    }
}
