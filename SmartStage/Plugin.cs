using System;
using UnityEngine;
using KSP.UI.Screens;

using ToolbarControl_NS;

namespace SmartStage
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Plugin : MonoBehaviour
    {
        public enum state { inactive, active }

        //ApplicationLauncherButton vabButton;
        //ApplicationLauncherButton flightButton;

        ToolbarControl vabToolbarControl, flightToolbarControl;

        readonly Texture2D[] textures;

        state _state = state.inactive;
        public state State
        {
            get { return _state; }
            set
            {
                if (value == _state)
                    return;
                _state = value;
#if false
                vabButton?.SetTexture(Texture);
				flightButton?.SetTexture(Texture);
#endif
                string bigIcon = _state == 0 ? "SmartStage/SmartStage38" : "SmartStage/SmartStage38-active";

                string smallIcon = _state == 0 ? "SmartStage/SmartStage24" : "SmartStage/SmartStage24-active";
                vabToolbarControl?.SetTexture(bigIcon, smallIcon);
                flightToolbarControl?.SetTexture(bigIcon, smallIcon);

            }
        }
        Texture2D Texture { get { return textures[(int)_state]; } }

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

        public bool useBlizzy = false;
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

        public Plugin()
        {
            textures = new Texture2D[]{
                GameDatabase.Instance.GetTexture("SmartStage/SmartStage38", false),
                GameDatabase.Instance.GetTexture("SmartStage/SmartStage38-active", false)
            };
        }

        public void Start()
        {
            if (KSP.IO.File.Exists<MainWindow>("settings.cfg"))
            {
                try
                {
                    var settings = ConfigNode.Load(KSP.IO.IOUtils.GetFilePathFor(typeof(MainWindow), "settings.cfg"));
                    autoUpdateStaging = settings.GetValue("autoUpdateStaging") == bool.TrueString;
                    useBlizzy = settings.GetValue("useBlizzy") == bool.TrueString;
                }
                catch (Exception) { }
                try
                {
                    var settings = ConfigNode.Load(KSP.IO.IOUtils.GetFilePathFor(typeof(MainWindow), "settings.cfg"));
                    showInFlight = settings.GetValue("showInFlight") == bool.TrueString;
                }
                catch (Exception) { }
            }
            GameEvents.onGUIApplicationLauncherReady.Add(AddButton);
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

        void AddButton()
        {
            if (vabToolbarControl != null) // || ! ApplicationLauncher.Ready)
                return;
#if false
            vabButton = ApplicationLauncher.Instance.AddModApplication(
				() => ShowWindow(true),
				() => ShowWindow(false),
				null, null,
				null, null,
				ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH,
				Texture);
#endif
            vabToolbarControl = gameObject.AddComponent<ToolbarControl>();
            vabToolbarControl.AddToAllToolbars(ShowWindowTrue, ShowWindowFalse,
                ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH,
                "SmartStage_NS",
                "vabSmartStageButton",
                "SmartStage/SmartStage38-active",
                "SmartStage/SmartStage38",
                "SmartStage/SmartStage24-active",
                "SmartStage/SmartStage24",
                "Smart Stage"
            );
            vabToolbarControl.UseBlizzy(useBlizzy);


            if (showInFlight)
            {
#if false
                flightButton = ApplicationLauncher.Instance.AddModApplication(
					SimulationLogic.inFlightComputeStages, SimulationLogic.inFlightComputeStages,
					null, null,
					null, null,
					ApplicationLauncher.AppScenes.FLIGHT,
					Texture);
#endif
                flightToolbarControl = gameObject.AddComponent<ToolbarControl>();
                flightToolbarControl.AddToAllToolbars(SimulationLogic.inFlightComputeStages, SimulationLogic.inFlightComputeStages,
                    ApplicationLauncher.AppScenes.FLIGHT,
                    "SmartStage_NS",
                    "flightSmartStageButton",
                    "SmartStage/SmartStage38-active",
                    "SmartStage/SmartStage38",
                    "SmartStage/SmartStage24-active",
                    "SmartStage/SmartStage24",
                    "Smart Stage"
                );
                flightToolbarControl.UseBlizzy(useBlizzy);
            }
        }

        void RemoveButton()
        {
#if false
            if (flightButton != null)
				ApplicationLauncher.Instance.RemoveModApplication(flightButton);
			flightButton = null;
			if (vabButton != null)
				ApplicationLauncher.Instance.RemoveModApplication(vabButton);
			vabButton = null;
#endif
            if (vabToolbarControl != null)
            {
                vabToolbarControl.OnDestroy();
                Destroy(vabToolbarControl);
            }
            if (flightToolbarControl != null)
            {
                flightToolbarControl.OnDestroy();
                Destroy(vabToolbarControl);
            }

        }
        void ShowWindowTrue()
        {
            if (gui != null)
                gui.ShowWindow = true;
        }
        void ShowWindowFalse()
        {
            if (gui != null)
                gui.ShowWindow = false;
        }
        void ShowWindow(bool shown)
        {
            if (gui != null)
                gui.ShowWindow = shown;
        }

        public void OnGUI()
        {
            if (vabToolbarControl != null)
                vabToolbarControl.UseBlizzy(useBlizzy);
            if (flightToolbarControl != null)
                flightToolbarControl.UseBlizzy(useBlizzy);
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
            settings.AddValue("useBlizzy", useBlizzy);
            settings.Save(KSP.IO.IOUtils.GetFilePathFor(typeof(MainWindow), "settings.cfg"));
        }
    }
}

