using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class LandscapeEditorWindow : EditorWindow
{
    HydraulicErosion water = new HydraulicErosion();
    FaultErosion faults = new FaultErosion();
    ThermalErosion thermal = new ThermalErosion();

    Landscape land = new Landscape();

    Terrain terrain;

    private void Awake()
    {
        terrain = FindObjectOfType<Terrain>();
    }

    [MenuItem("Window/LanscapeEditor")]
    public static void ShowWindow()
    {
        GetWindow<LandscapeEditorWindow>("BetterTerrain");
    }

    float depth = 0;
    float decreaseDistance = 0;
    float droppletLifetime = 0;
    float threshHold = 0;
    bool generateFaults = true;
    bool generateWater = true;
    bool generateThermal = true;

    bool openMenu = false;
    string[] names = new string[4] { "General", "Faults", "Hydraulic", "Thermal" };

    int choosen = 0;

    private int perlinLayers = 0;

    private float startFrequency = 0;

    private float startAmplitude = 0;

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

        for (int i = 0; i < names.Length; i++)
        {

            //EditorGUILayout.Space();

            if (GUILayout.Button(names[i]))
            {
                choosen = i;
            }

        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical("box2", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

        EditorGUILayout.HelpBox(names[choosen] + " Modifier", MessageType.None);

        switch (choosen)
        {
            case 0:
                perlinLayers = EditorGUILayout.IntField("Amount of stacked noise", perlinLayers);
                startFrequency = EditorGUILayout.FloatField("start Frequency", startFrequency);
                startAmplitude = EditorGUILayout.FloatField("start Amplitude", startAmplitude);
                break;
            case 1:
                generateFaults = GUILayout.Toggle(generateFaults, GUIContent.none);

                if (generateFaults)
                {
                    depth = EditorGUILayout.FloatField("Maximum height", depth);
                    decreaseDistance = EditorGUILayout.FloatField("Decrease distance", decreaseDistance);
                }

                break;
            case 2:
                generateWater = GUILayout.Toggle(generateWater, GUIContent.none);

                if (generateWater)
                {
                    droppletLifetime = EditorGUILayout.FloatField("Duration of a rain drop", droppletLifetime);
                }
                break;
            case 3:
                generateThermal = GUILayout.Toggle(generateThermal, GUIContent.none);

                if (generateThermal)
                {
                    threshHold = EditorGUILayout.FloatField("Amount of smoothness", threshHold);
                }
                break;
        }

        if (GUILayout.Button("Generate"))
        {
            float[,] heightData
                = land.Generate(terrain.terrainData.heightmapResolution,
                terrain.terrainData.heightmapResolution, perlinLayers,
                startFrequency, startAmplitude);

            if (generateFaults)
            {
                heightData = faults.Generator(heightData, depth, decreaseDistance);
            }
            if (generateWater)
            {
                heightData = water.Generator(heightData, droppletLifetime);
            }
            if (generateThermal)
            {
                heightData = thermal.Generator(heightData, threshHold);
            }

            terrain.terrainData.SetHeights(0, 0, heightData);
        }



    }
}
