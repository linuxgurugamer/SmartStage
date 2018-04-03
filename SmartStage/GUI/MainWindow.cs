using System;
using UnityEngine;

using ClickThroughFix;

namespace SmartStage
{

    public class MainWindow
    {
        int windowId = GUIUtility.GetControlID(FocusType.Passive);
        Rect windowPosition, stagePosition, displayStagePosition;
        bool lockEditor;
        public bool ShowWindow = false;

        public bool showStages = false;

        bool advancedSimulation = false;
        AscentPlot plot;
        CelestialBody[] planetObjects = FlightGlobals.Bodies.ToArray();
        string[] planets = FlightGlobals.Bodies.ConvertAll(b => b.GetName()).ToArray();
        int planetId;
        EditableDouble maxAcceleration = new EditableDouble(0);

        readonly Plugin plugin;

        public MainWindow(Plugin plugin)
        {
            this.plugin = plugin;
            planetId = Array.IndexOf(planetObjects, Planetarium.fetch.Home);
            // Position will be computed dynamically to be on screen
            windowPosition = new Rect(Screen.width, Screen.height, 0, 0);
        }

        public void ComputeStages()
        {
            SimulationLogic ship = new SimulationLogic(
                EditorLogic.fetch.ship.parts,
                planetObjects[planetId],
                68,
                maxAcceleration,
                advancedSimulation,
                Vector3d.up);
            ship.computeStages();
            if (advancedSimulation)
                plot = new AscentPlot(ship.samples, ship.stages, 400, 400);
        }

        public void OnGUI()
        {
            lockEditor = ComboBox.DrawGUI();

            if (ShowWindow)
            {
                if (Event.current.type == EventType.Layout)
                {
                    windowPosition.x = Math.Min(windowPosition.x, Screen.width - windowPosition.width - 50);
                    windowPosition.y = Math.Min(windowPosition.y, Screen.height - windowPosition.height - 50);
                }
                windowPosition = ClickThruBlocker.GUILayoutWindow(windowId, windowPosition, drawWindow, "SmartStage");
                lockEditor |= windowPosition.Contains(Event.current.mousePosition);
            }

            if (showStages)
            {
                if (Event.current.type == EventType.Layout)
                {
                    stagePosition.x = Math.Min(stagePosition.x, Screen.width - stagePosition.width - 50);
                    stagePosition.y = Math.Min(stagePosition.y, Screen.height - stagePosition.height - 50);
                }
                stagePosition = ClickThruBlocker.GUILayoutWindow(windowId + 1, stagePosition, drawStagesWindow, "SmartStage Stages");
                lockEditor |= stagePosition.Contains(Event.current.mousePosition);
            }

            if (displayStage > 0)
            {
                if (Event.current.type == EventType.Layout)
                {
                    displayStagePosition.x = Math.Min(displayStagePosition.x, Screen.width - stagePosition.width - 50);
                     displayStagePosition.y = Math.Min(displayStagePosition.y, Screen.height - stagePosition.height - 50);
                }
                 displayStagePosition = ClickThruBlocker.GUILayoutWindow(windowId + 2, displayStagePosition, drawSingleStageWindow, "SmartStage Stages");
                lockEditor |= stagePosition.Contains(Event.current.mousePosition);
            }


            if (lockEditor)
                EditorLogic.fetch.Lock(true, true, true, "SmartStage");
            else
                EditorLogic.fetch.Unlock("SmartStage");
        }

        int[] partsCntPerStage;
        bool partsCntInitted = false;
        int displayStage = 0;

        void countPartsPerStage(KSP.UI.Screens.StageManager stageManager)
        {
            partsCntInitted = true;
            partsCntPerStage = new int[stageManager.Stages.Count + 1];
            for (int i = 0; i < EditorLogic.fetch.ship.parts.Count; i++)
            {
                var p = EditorLogic.fetch.ship.parts[i];

                if (p.Modules.Contains<ModuleEngines>() ||
                     p.Modules.Contains<ModuleEnginesFX>() ||
                     p.Modules.Contains<LaunchClamp>() ||
                     p.Modules.Contains<ModuleAnchoredDecoupler>() ||
                     p.Modules.Contains<ModuleProceduralFairing>() ||
                     p.Modules.Contains<ModuleDecouple>())
                    partsCntPerStage[p.inverseStage + 1]++;
            }
        }

        void drawSingleStageWindow(int windowid)
        {

        }

        void drawStagesWindow(int windowid)
        {
            var stageManager = KSP.UI.Screens.StageManager.Instance;
            if (!partsCntInitted)
                countPartsPerStage(stageManager);

            GUILayout.BeginVertical();

            for (int i = 1; i < stageManager.Stages.Count + 1; i++)
            {
                if (GUILayout.Button("Stage: " + i.ToString() + ", part count: " + partsCntPerStage[i].ToString()))
                {
                    displayStage = i;
                }

            }
            if (GUILayout.Button("Done"))
                showStages = false;
            GUILayout.EndVertical();

            GUI.DragWindow();
        }


        void drawWindow(int windowid)
        {
            bool draggable = true;
            GUILayout.BeginVertical();
            if (GUILayout.Button("Compute stages"))
                ComputeStages();
            if (KSP.UI.Screens.StageManager.Instance.Stages.Count <= 1)
                GUI.enabled = false;
            if (GUILayout.Button("Show stages"))
            {
                showStages = !showStages;
            }
            GUI.enabled = true;
            plugin.autoUpdateStaging = GUILayout.Toggle(plugin.autoUpdateStaging, "Automatically recompute staging");

            bool newAdvancedSimulation = GUILayout.Toggle(advancedSimulation, "Advanced simulation");
            if (!newAdvancedSimulation && advancedSimulation)
            {
                windowPosition.width = 0;
                windowPosition.height = 0;
            }
            advancedSimulation = newAdvancedSimulation;
            if (advancedSimulation)
            {
                int oldId = planetId;
                planetId = ComboBox.Box(planetId, planets, planets);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Max acceleration: ");
                maxAcceleration.text = GUILayout.TextField(maxAcceleration.text);
                GUILayout.EndHorizontal();

                if (plot != null)
                    draggable &= plot.draw();

                if (oldId != planetId)
                    ComputeStages();
            }
            plugin.showInFlight = GUILayout.Toggle(plugin.showInFlight, "Show icon in flight");
            plugin.useBlizzy = GUILayout.Toggle(plugin.useBlizzy, "Use Blizzy toolbar, if available");

            GUILayout.EndVertical();
            if (draggable)
                GUI.DragWindow();
        }

        public void onEditorShipModified(ShipConstruct v)
        {
            try
            {
                if (plugin.autoUpdateStaging)
                    ComputeStages();
            }
            catch (Exception) { }
        }

    }
}

