using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Version = CKAN.Version;

namespace ModHidePlugin
{
    public class ModHidePlugin : CKAN.IGUIPlugin
    {
        public override string GetName()
        {
            return "Mod Hiding Support";
        }

        public override Version GetVersion()
        {
            return new Version("1");
        }

        public override void Initialize()
        {
            var loaded = HiddenModManager.Instance;

        }

        public override void Deinitialize()
        {
            HiddenModManager.Instance.SaveSettings();            


        }
    }
}
