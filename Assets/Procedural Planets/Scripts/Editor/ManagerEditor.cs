/*  
    Class: ProceduralPlanets.ManagerEditor
    Version: 0.1.1 (alpha release)
    Date: 2018-01-10
    Author: Stefan Persson
    (C) Imphenzia AB

    Custom inspector editor for Manager
*/

using UnityEngine;
using UnityEditor;

namespace ProceduralPlanets
{
    [CustomEditor(typeof(Manager))]
    public class ManagerEditor : Editor
    {
        // Constants
        const float WIDTH_BLUEPRINT_TYPE = 35f;
        const float WIDTH_BLUEPRINT_NAME = 120f;
        const float WIDTH_BLUEPRINT_BUTTON = 20f;
        const float WIDTH_BLUEPRINT_PERCENT = 50f;

//        [System.NonSerialized]
        // Private Variables
        private string[] resolutionsDisplay = { "16 x 16", "32 x 32", "64 x 64", "128 x 128", "256 x 256", "512 x 512", "1024 x 1024", "2048 x 2048" };

        // Reference to target object
        private Manager _script;

        // Serialized Object
        SerializedObject _target;

        // Serialized Properties    
        SerializedProperty _debug_level;
        SerializedProperty _masterSeed;
        SerializedProperty _listSolidPlanetBlueprints;
        SerializedProperty _listGasPlanetBlueprints;
        SerializedProperty _solidCompositionMaterials;
        SerializedProperty _solidBiome1Materials;
        SerializedProperty _solidBiome2Materials;
        SerializedProperty _solidCloudsMaterials;
        SerializedProperty _solidCitiesMaterials;
        SerializedProperty _solidLavaMaterials;
        SerializedProperty _solidPolarIceMaterials;
        SerializedProperty _ringMaterials;
        SerializedProperty _autoUpdateShader;
        SerializedProperty _autoUpdateTextures;
        SerializedProperty _resolutionContinent;
        SerializedProperty _resolutionBiomes;
        SerializedProperty _resolutionClouds;
        SerializedProperty _resolutionCities;
        SerializedProperty _resolutionLava;
        SerializedProperty _resolutionPolarIce;

        void OnEnable()
        {
            // Get the target object
            _target = new SerializedObject(target);

            // Get a reference to the target script component
            _script = (Manager)target;

            // Find the properties of the target
            _debug_level = _target.FindProperty("DEBUG_LEVEL");
            _listSolidPlanetBlueprints = _target.FindProperty("listSolidPlanetBlueprints");
            _listGasPlanetBlueprints = _target.FindProperty("listGasPlanetBlueprints");
            _solidCompositionMaterials = _target.FindProperty("solidCompositionMaterials");
            _solidBiome1Materials = _target.FindProperty("solidBiome1Materials");
            _solidBiome2Materials = _target.FindProperty("solidBiome2Materials");
            _solidCloudsMaterials = _target.FindProperty("solidCloudsMaterials");
            _solidCitiesMaterials = _target.FindProperty("solidCitiesMaterials");
            _solidLavaMaterials = _target.FindProperty("solidLavaMaterials");
            _solidPolarIceMaterials = _target.FindProperty("solidPolarIceMaterials");
            _ringMaterials = _target.FindProperty("ringMaterials");
            _resolutionContinent = _target.FindProperty("resolutionContinent");
            _resolutionBiomes = _target.FindProperty("resolutionBiomes");
            _resolutionClouds = _target.FindProperty("resolutionClouds");
            _resolutionCities = _target.FindProperty("resolutionCities");
            _resolutionLava = _target.FindProperty("resolutionLava");
            _resolutionPolarIce = _target.FindProperty("resolutionPolarIce");
            _autoUpdateTextures = _target.FindProperty("autoUpdateTextures");
        }

        /// <summary>
        /// Displays and allows interaction in a custom inspector for Manager
        /// </summary>
        public override void OnInspectorGUI()
        {
            _script.RefreshLists();
            _script.RefreshBlueprintDictionary();
            _target.Update();            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("CREATE GAME OBJECTS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if (GUILayout.Button("Create Random Planet")) CreatePlanet();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("PLANET BLUEPRINTS", EditorStyles.boldLabel);
            float _total = 0f;

            for (int i = 0; i < _listSolidPlanetBlueprints.arraySize; i++)
                _total += ((BlueprintSolidPlanet)_listSolidPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue).probability;

            for (int i = 0; i < _listGasPlanetBlueprints.arraySize; i++)
                _total += ((BlueprintGasPlanet)_listGasPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue).probability;
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Solid Planet Blueprint")) CreateSolidPlanetBlueprint();
            GUI.enabled = false;
            if (GUILayout.Button("Create Gas Planet Blueprint")) CreateGasPlanetBlueprint();
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("TYPE", EditorStyles.boldLabel, GUILayout.Width(WIDTH_BLUEPRINT_TYPE));
            EditorGUILayout.LabelField("BLUEPRINT", EditorStyles.boldLabel, GUILayout.Width(WIDTH_BLUEPRINT_NAME + WIDTH_BLUEPRINT_BUTTON * 2));
            EditorGUILayout.LabelField("PROBABILITY", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();

            // Solid Planet Blueprints
            for (int i = 0; i < _listSolidPlanetBlueprints.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                float _value = ((BlueprintSolidPlanet)(_listSolidPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).probability;
                EditorGUILayout.LabelField("Solid", GUILayout.Width(WIDTH_BLUEPRINT_TYPE));
                ((BlueprintSolidPlanet)(_listSolidPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).gameObject.name = EditorGUILayout.TextField(((BlueprintSolidPlanet)(_listSolidPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).gameObject.name, GUILayout.Width(WIDTH_BLUEPRINT_NAME));
                if (GUILayout.Button(new GUIContent("E", "Edit this blueprint"), GUILayout.Width(WIDTH_BLUEPRINT_BUTTON)))
                    Selection.activeGameObject = ((BlueprintSolidPlanet)(_listSolidPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).gameObject;
                if (GUILayout.Button(new GUIContent("C", "Create planet using this blueprint"), GUILayout.Width(WIDTH_BLUEPRINT_BUTTON)))
                    _script.CreatePlanet(Vector3.zero, - 1, ((BlueprintSolidPlanet)(_listSolidPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).gameObject.name);
                _value = EditorGUILayout.Slider(_value, 0.0f, 1.0f);
                ((BlueprintSolidPlanet)(_listSolidPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).probability = _value;
                EditorGUILayout.LabelField(((_value / _total ) * 100).ToString("F1").Replace("NaN", "0") + "%", GUILayout.Width(WIDTH_BLUEPRINT_PERCENT));
                if (GUILayout.Button(new GUIContent("X", "Delete this blueprint"), GUILayout.Width(WIDTH_BLUEPRINT_BUTTON)))
                {
                    if (EditorUtility.DisplayDialog("Confirmation", "Are you sure you want to delete the blueprint?", "Yes", "Cancel"))
                    {
                        DestroyImmediate(((BlueprintSolidPlanet)(_listSolidPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).gameObject);
                        _script.RefreshLists();
                    }
                }
                    
                EditorGUILayout.EndHorizontal();
            }

            // Gas Planet Blueprints
            for (int i = 0; i < _listGasPlanetBlueprints.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                float _value = ((BlueprintGasPlanet)(_listGasPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).probability;
                EditorGUILayout.LabelField("Gas", GUILayout.Width(WIDTH_BLUEPRINT_TYPE));
                ((BlueprintGasPlanet)(_listGasPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).gameObject.name = EditorGUILayout.TextField(((BlueprintGasPlanet)(_listGasPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).gameObject.name, GUILayout.Width(WIDTH_BLUEPRINT_NAME));
                if (GUILayout.Button(new GUIContent("E", "Edit this blueprint"), GUILayout.Width(WIDTH_BLUEPRINT_BUTTON)))
                    Selection.activeGameObject = ((BlueprintGasPlanet)(_listGasPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).gameObject;
                if (GUILayout.Button(new GUIContent("C", "Create planet using this blueprint"), GUILayout.Width(WIDTH_BLUEPRINT_BUTTON)))
                    _script.CreatePlanet(Vector3.zero, - 1, ((BlueprintGasPlanet)(_listGasPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).gameObject.name);
                _value = EditorGUILayout.Slider(_value, 0.0f, 1.0f);
                ((BlueprintGasPlanet)(_listGasPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).probability = _value;
                EditorGUILayout.LabelField(((_value / _total) * 100).ToString("F1").Replace("NaN", "0") + "%", GUILayout.Width(WIDTH_BLUEPRINT_PERCENT));
                if (GUILayout.Button(new GUIContent("X", "Delete this blueprint"), GUILayout.Width(WIDTH_BLUEPRINT_BUTTON)))
                {
                    if (EditorUtility.DisplayDialog("Confirmation", "Are you sure you want to delete the blueprint?", "Yes", "Cancel"))
                    {
                        DestroyImmediate(((BlueprintGasPlanet)(_listGasPlanetBlueprints.GetArrayElementAtIndex(i).objectReferenceValue)).gameObject);
                        _script.RefreshLists();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck())
                _script.RefreshBlueprintDictionary();

            // Blueprint Tools

            EditorGUILayout.Space();            
            EditorGUILayout.LabelField("BLUEPRINT TOOLS", EditorStyles.boldLabel);

            if (GUILayout.Button("Export All Blueprints to Clipboard (JSON)"))
                ExportAllBlueprintsToClipboard();
            
            if (GUILayout.Button("Import Blueprints from Clipboard (JSON)"))
                if (EditorUtility.DisplayDialog("Confirmation", "This will overwrite any blueprints that have the same name as imported blueprints.", "Proceed", "Cancel"))
                    ImportAllBlueprintsFromClipboard();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // Procedural Materials

            EditorGUILayout.LabelField("PROCEDURAL MATERIALS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("SOLID PLANETS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_solidCompositionMaterials, true);
            EditorGUILayout.PropertyField(_solidBiome1Materials, true);
            EditorGUILayout.PropertyField(_solidBiome2Materials, true);
            EditorGUILayout.PropertyField(_solidCloudsMaterials, true);
            EditorGUILayout.PropertyField(_solidCitiesMaterials, true);
            EditorGUILayout.PropertyField(_solidLavaMaterials, true);
            EditorGUILayout.PropertyField(_solidPolarIceMaterials, true);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("GAS PLANETS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Not implemented yet");
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("RINGS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_ringMaterials, true);
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // Texture Settings

            EditorGUILayout.LabelField("TEXTURE SETTINGS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            _autoUpdateTextures.boolValue = EditorGUILayout.Toggle("Auto-rebuild Textures", _autoUpdateTextures.boolValue);
            if (EditorGUI.EndChangeCheck() && _autoUpdateTextures.boolValue)
            {
                _target.ApplyModifiedProperties();
                UpdateProceduralTexture("All");
            }
            EditorGUILayout.LabelField("Auto-rebuild textures in editor");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("SOLID PLANETS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            // Resolution Continent
            EditorGUI.BeginChangeCheck();
            _resolutionContinent.intValue = EditorGUILayout.Popup("Composition", _resolutionContinent.intValue, resolutionsDisplay);
            if (EditorGUI.EndChangeCheck())
            {
                _target.ApplyModifiedProperties();
                UpdateProceduralTexture("Maps");
            }

            // Resolution Clouds
            EditorGUI.BeginChangeCheck();
            _resolutionClouds.intValue = EditorGUILayout.Popup("Clouds", _resolutionClouds.intValue, resolutionsDisplay);
            if (EditorGUI.EndChangeCheck())
            {
                _target.ApplyModifiedProperties();
                UpdateProceduralTexture("Clouds");
            }

            // Resolution Cities
            EditorGUI.BeginChangeCheck();
            _resolutionCities.intValue = EditorGUILayout.Popup("Cities", _resolutionCities.intValue, resolutionsDisplay);
            if (EditorGUI.EndChangeCheck())
            {
                _target.ApplyModifiedProperties();
                UpdateProceduralTexture("Cities");
            }
            // Resolution Biomes
            EditorGUI.BeginChangeCheck();
            _resolutionBiomes.intValue = EditorGUILayout.Popup("Biomes", _resolutionBiomes.intValue, resolutionsDisplay);
            if (EditorGUI.EndChangeCheck())
            {
                _target.ApplyModifiedProperties();
                UpdateProceduralTexture("Biome1");
                UpdateProceduralTexture("Biome2");
            }

            // Resolution Lava
            EditorGUI.BeginChangeCheck();
            _resolutionLava.intValue = EditorGUILayout.Popup("Lava", _resolutionLava.intValue, resolutionsDisplay);
            if (EditorGUI.EndChangeCheck())
            {
                _target.ApplyModifiedProperties();
                UpdateProceduralTexture("Lava");
            }

            // Resolution PolarIce
            EditorGUI.BeginChangeCheck();
            _resolutionPolarIce.intValue = EditorGUILayout.Popup("PolarIce", _resolutionPolarIce.intValue, resolutionsDisplay);
            if (EditorGUI.EndChangeCheck())
            {
                _target.ApplyModifiedProperties();
                UpdateProceduralTexture("PolarIce");
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("ALPHA VERSION - DEBUG", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            _debug_level.intValue = EditorGUILayout.Popup(_debug_level.intValue, new string[] { "OFF", "BASIC", "FULL" });
            EditorGUI.indentLevel--;

            _target.ApplyModifiedProperties();
        }

        /// <summary>
        /// Creates a random planet at vector 0,0,0
        /// </summary>
        public void CreatePlanet()
        {
            _script.CreatePlanet(Vector3.zero);
        }

        /// <summary>
        /// Creates a solid planet blueprint.
        /// </summary>
        public void CreateSolidPlanetBlueprint()
        {
            GameObject _go = new GameObject();
            _go.name = _script.GetUniqueBlueprintName();
            _go.transform.parent = _script.transform;
            _go.AddComponent<BlueprintSolidPlanet>();
            _script.RefreshLists();
        }

        /// <summary>
        /// Creates a gas planet blueprint.
        /// </summary>
        public void CreateGasPlanetBlueprint()
        {
            GameObject _go = new GameObject();
            _go.name = _script.GetUniqueBlueprintName();
            _go.transform.parent = _script.transform;
            _go.AddComponent<BlueprintGasPlanet>();
            _script.RefreshLists();
        }

        /// <summary>
        /// Updfates procedural textures based on texture name.
        /// </summary>
        /// <param name="_textureName"></param>
        void UpdateProceduralTexture(string _textureName)
        {
            SolidPlanet[] _sp = FindObjectsOfType<SolidPlanet>();
            foreach (SolidPlanet _p in _sp)
                _p.UpdateProceduralTexture(_textureName);

            /* DISABLED UNTIL IMPLEMENTED

            GasPlanet[] _gp = FindObjectsOfType<GasPlanet>();
            foreach (GasPlanet _p in _gp)
                _p.UpdateProceduralTexture(_textureName);
            */
        }

        /// <summary>
        /// Exports all planet (and child ring) blueprints to clipboard as a JSON string with each blueprint being a unique item.
        /// </summary>
        void ExportAllBlueprintsToClipboard()
        {
            // Use the JSON export method in the target script
            _script.ExportAllBlueprintsToClipboard();
        }

        /// <summary>
        /// Imports planet (and child ring) blueprints from clipboard (must be a valid JSON string with blueprints as unique numbered items.
        /// </summary>
        void ImportAllBlueprintsFromClipboard()
        {
            // Use the JSON import method in the target script
            _script.ImportBlueprintsFromClipboard();
        }
    }
}
