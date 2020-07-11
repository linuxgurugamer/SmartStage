using System;
using UnityEngine;
using KSP.UI.Screens;
//using KSP_Log;

using ToolbarControl_NS;
using static SmartStage.Plugin;


namespace SmartStage
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(Plugin.VAB_MODID, Plugin.MODNAME);

            //ToolbarControl.RegisterMod(Plugin.FLIGHT_MODID, Plugin.MODNAME + " Flight");
        }
    }

#if false
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class InitLog : MonoBehaviour
    {
        protected void Awake()
        {
            Plugin.Log = new KSP_Log.Log("SmartStage"
#if DEBUG
                , KSP_Log.Log.LEVEL.INFO
#endif
                );
        }
    }
#endif
}
