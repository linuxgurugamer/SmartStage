using System;
using UnityEngine;
using KSP.UI.Screens;

using ToolbarControl_NS;

namespace SmartStage
{

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Plugin : MonoBehaviour
    {
        public static KSP_Log.Log Log;
        public enum state { inactive, active }


        ToolbarControl toolbarControl; //, flightToolbarControl;

        state _state = state.inactive;
        public state State
        {
            get { return _state; }
            set
            {
                if (value == _state)
                    return;
                _state = value;
                string bigIcon = _state == 0 ? "SmartStage/SmartStage38" : "SmartStage/SmartStage38-active";

                string smallIcon = _state == 0 ? "SmartStage/SmartStage24" : "SmartStage/SmartStage24-active";
                toolbarControl?.SetTexture(bigIcon, smallIcon);
                //flightToolbarControl?.SetTexture(bigIcon, smallIcon);

            }
        }

        bool _showInFlight = false;
        public bool showInFlight
        {
            get { return _showInFlight; }
            set
            {
                if (value == _showInFlight)
                    return;
                _showInFlight = value;
                Save();
            }
        }

        bool _autoUpdateStaging = false;
        public bool autoUpdateStaging
        {
            get { return _autoUpdateStaging; }
            set
            {
                if (value == _autoUpdateStaging)
                    return;
                _autoUpdateStaging = value;
                State = value ? Plugin.state.active : Plugin.state.inactive;
                Save();
            }
        }

        MainWindow gui;


        public void Start()
        {
            if (KSP.IO.File.Exists<MainWindow>("settings.cfg"))
            {
                try
                {
                    var settings = ConfigNode.Load(KSP.IO.IOUtils.GetFilePathFor(typeof(MainWindow), "settings.cfg"));
                    autoUpdateStaging = settings.GetValue("autoUpdateStaging") == bool.TrueString;
                }
                catch (Exception) { }
                try
                {
                    var settings = ConfigNode.Load(KSP.IO.IOUtils.GetFilePathFor(typeof(MainWindow), "settings.cfg"));
                    showInFlight = settings.GetValue("showInFlight") == bool.TrueString;
                }
                catch (Exception) { }
            }
            //GameEvents.onGUIApplicationLauncherReady.Add(AddButton);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveButton);
            GameEvents.onEditorShipModified.Add(onEditorShipModified);
            GameEvents.onLevelWasLoaded.Add(sceneChanged);
            GameEvents.onGameSceneSwitchRequested.Add(sceneSwitchRequested);
            AddButton();
            DontDestroyOnLoad(this);
        }

        void sceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> ignored)
        {
            RemoveButton();
            gui = null;
            State = state.inactive;
        }

        void sceneChanged(GameScenes scene)
        {
            AddButton();
            if (scene == GameScenes.EDITOR)
            {
                gui = new MainWindow(this);
                if (autoUpdateStaging)
                    State = state.active;
            }
        }

        internal const string VAB_MODID = "VABSmartStage_NS";
        //internal const string FLIGHT_MODID = "SPHSmartStage_NS";
        internal const string MODNAME = "Smart Stage";

        void AddButton()
        {
            if (toolbarControl != null) // || ! ApplicationLauncher.Ready)
                return;

            ApplicationLauncher.AppScenes scenes = ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH;
            if (showInFlight)
                scenes |= ApplicationLauncher.AppScenes.FLIGHT;

            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(ShowWindowTrue, ShowWindowFalse,
                scenes,
                VAB_MODID,
                "vabSmartStageButton",
                "SmartStage/SmartStage38-active",
                "SmartStage/SmartStage38",
                "SmartStage/SmartStage24-active",
                "SmartStage/SmartStage24",
                MODNAME
            );

        }

        void RemoveButton()
        {
            if (toolbarControl != null)
            {
                toolbarControl.OnDestroy();
                Destroy(toolbarControl);
            }
        }
        void ShowWindowTrue()
        {
            if (HighLogic.LoadedSceneIsFlight)
                SimulationLogic.inFlightComputeStages();
            else
            {
                if (gui != null)
                    gui.ShowWindow = true;
            }
        }
        void ShowWindowFalse()
        {
            if (HighLogic.LoadedSceneIsFlight)
                SimulationLogic.inFlightComputeStages();
            else
            {
                if (gui != null)
                    gui.ShowWindow = false;
            }
        }
        void ShowWindow(bool shown)
        {
            if (gui != null)
                gui.ShowWindow = shown;
        }

        public void OnGUI()
        {
            gui?.OnGUI();
        }

        void onEditorShipModified(ShipConstruct ship)
        {
            gui?.onEditorShipModified(ship);
        }

        void Save()
        {
            ConfigNode settings = new ConfigNode("SmartStage");
            settings.AddValue("autoUpdateStaging", autoUpdateStaging);
            settings.AddValue("showInFlight", showInFlight);
            settings.Save(KSP.IO.IOUtils.GetFilePathFor(typeof(MainWindow), "settings.cfg"));
        }
    }
}

