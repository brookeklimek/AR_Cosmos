/*  
    Class: ProceduralPlanets.SolidPlanetEditor
    Version: 0.1.1 (initial alpha release)
    Date: 2018-01-10
    Author: Stefan Persson
    (C) Imphenzia AB

    Custom inspector editor for Solid Planets.
*/

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace ProceduralPlanets
{
    [CustomEditor(typeof(SolidPlanet))]
    [CanEditMultipleObjects]
    public class SolidPlanetEditor : Editor
    {
        // Constants
        private const float LABEL_WIDTH = 145;
        private const float VALUE_WIDTH = 90;
        private const float MODIFY_DELAY = 0.5f;

        // Flags
        private bool _resetOverrides = false;
        private bool _buttonRebuildTextures = false;
        private bool _newSeed = false;
        private bool _newVariation = false;
        private bool _newPlanetBlueprint = false;
        private float _modifyTimestamp;
        private bool _modifyWait;
        private bool _modifiedTextureBiome1 = false;
        private bool _modifiedTextureBiome2 = false;
        private bool _modifiedTextureClouds = false;
        private bool _modifiedTextureCities = false;
        private bool _modifiedTextureMaps = false;
        private bool _modifiedTextureLava = false;
        private bool _modifiedTextureLookups = false;
        private bool _modifiedShader = false;
        private bool _updateSerializedPlanetCacheRequired = false;

        // Color of modified settings in inspector
        private Color _colorModified = new Color(1.0f, 0.7f, 0.4f);

        //Reference to the target component (needed for some direct method calls)
        private SolidPlanet _script;

        // Serialized Object
        SerializedObject _target;

        // Serialized Properties
        SerializedProperty _serializedPlanetCache;
        SerializedProperty _rebuildMapsNeeded;
        SerializedProperty _rebuildBiome1Needed;
        SerializedProperty _rebuildBiome2Needed;
        SerializedProperty _rebuildCitiesNeeded;
        SerializedProperty _rebuildCloudsNeeded;
        SerializedProperty _rebuildLavaNeeded;
        SerializedProperty _rebuildLookupsNeeded;
        SerializedProperty _updateShaderNeeded;

        SerializedProperty _planetBlueprintIndex;
        SerializedProperty _planetBlueprintOverride;

        SerializedProperty _planetSeed;
        SerializedProperty _variationSeed;

        SerializedProperty _propertyFloats;
        SerializedProperty _propertyColors;
        SerializedProperty _propertyMaterials;

        // Unity cannot serialize dictionaries so lists are translated to dictionaries in the editor script and vice versa
        Dictionary<string, int> _indexFloats = new Dictionary<string, int>();
        Dictionary<string, int> _indexColors = new Dictionary<string, int>();
        Dictionary<string, int> _indexMaterials = new Dictionary<string, int>();

        void OnEnable()
        {
            // Get the target object
            _target = new SerializedObject(target);

            // Ensure editor application run Update method (doesn't happen every frame, only when OnInspectorGUI() is called.
            EditorApplication.update += Update;

            // Find the properties of the target
            _serializedPlanetCache = _target.FindProperty("serializedPlanetCache");
            _rebuildMapsNeeded = _target.FindProperty("rebuildMapsNeeded");
            _rebuildBiome1Needed = _target.FindProperty("rebuildBiome1Needed");
            _rebuildBiome2Needed = _target.FindProperty("rebuildBiome2Needed");
            _rebuildCitiesNeeded = _target.FindProperty("rebuildCitiesNeeded");
            _rebuildCloudsNeeded = _target.FindProperty("rebuildCloudsNeeded");
            _rebuildLavaNeeded = _target.FindProperty("rebuildLavaNeeded");
            _rebuildLookupsNeeded = _target.FindProperty("rebuildLookupsNeeded");
            _updateShaderNeeded = _target.FindProperty("updateShaderNeeded");
            _propertyFloats = _target.FindProperty("propertyFloats");
            _propertyColors = _target.FindProperty("propertyColors");
            _propertyMaterials = _target.FindProperty("propertyMaterials");
            _planetSeed = _target.FindProperty("planetSeed");
            _variationSeed = _target.FindProperty("variationSeed");
            _planetBlueprintIndex = _target.FindProperty("planetBlueprintIndex");
            _planetBlueprintOverride = _target.FindProperty("planetBlueprintOverride");
        }
        
        void OnDisable()
        {
            // Remove Update method event
            EditorApplication.update -= Update;
        }

        /// <summary>
        /// Displays and allows interaction in a custom inspector for Solid Planets
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Editing prefabs that are not instantiated in a scene is not supported.
            if (EditorUtility.IsPersistent(Selection.activeObject))
            {
                EditorGUILayout.Space();
                EditorGUILayout.Separator();
                EditorGUILayout.HelpBox("You must instantiate planet prefab in a scene to edit properties.", MessageType.Info);
                EditorGUILayout.Separator();
                EditorGUILayout.Space();
                return;
            }

            // Update the serialized object
            _target.Update();

            UpdateIndex();

            _script = (SolidPlanet)target;

            if (_script.RebuildTexturesNeeded())
                GUI.enabled = true;
            else
                GUI.enabled = false;

            _buttonRebuildTextures = GUILayout.Button("Rebuild Planet Textures");
            GUI.enabled = true;

            EditorGUILayout.LabelField("PLANET", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            // RANDOM
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            _planetSeed.intValue = EditorGUILayout.DelayedIntField("Planet Seed", _planetSeed.intValue);
            if (GUILayout.Button("Random")) _planetSeed.intValue = UnityEngine.Random.Range(0, int.MaxValue - 1000000);
            if (EditorGUI.EndChangeCheck())
            {
                _newSeed = true;
                _target.ApplyModifiedProperties();
                _target.Update();
                _modifiedTextureBiome1 = true;
                _modifiedTextureBiome2 = true;
                _modifiedTextureCities = true;
                _modifiedTextureClouds = true;
                _modifiedTextureLava = true;
                _modifiedTextureLookups = true;
                _modifiedTextureMaps = true;
            }
            EditorGUILayout.EndHorizontal();
            _resetOverrides = EditorGUILayout.Toggle("Reset Overrides", _resetOverrides);

            // VARIANT
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            _variationSeed.intValue = EditorGUILayout.IntField("Variant", _variationSeed.intValue);
            if (GUILayout.Button("Random")) _variationSeed.intValue = UnityEngine.Random.Range(0, 10000);
            if (EditorGUI.EndChangeCheck())
            {
                _newVariation = true;
                _target.ApplyModifiedProperties();
                _target.Update();
                _modifiedTextureBiome1 = true;
                _modifiedTextureBiome2 = true;
                _modifiedTextureCities = true;
                _modifiedTextureClouds = true;
                _modifiedTextureLava = true;
                _modifiedTextureLookups = true;
                _modifiedTextureMaps = true;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("PLANET TOOLS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if (GUILayout.Button("Export to Clipboard (JSON)"))
            {
                GUIUtility.systemCopyBuffer = _script.ExportToJSON(Planet.StringFormat.JSON_EASY_READ);
                EditorUtility.DisplayDialog("Finished", "Planet JSON was saved to clipboard", "Close");
            }
            if (GUILayout.Button("Export to Clipboard (Escaped JSON)"))
            {
                GUIUtility.systemCopyBuffer = _script.ExportToJSON(Planet.StringFormat.JSON_ESCAPED);
                EditorUtility.DisplayDialog("Finished", "Planet Escaped JSON string was saved to clipboard", "Close");
            }
            if (GUILayout.Button("Export to Clipboard (Base64 JSON)"))
            {
                GUIUtility.systemCopyBuffer = _script.ExportToJSON(Planet.StringFormat.JSON_BASE64);
                EditorUtility.DisplayDialog("Finished", "Planet Base64 string was saved to clipboard", "Close");
            }

            if (GUILayout.Button("Import from Clipboard (JSON / Base64)"))
            {
                _script.ImportFromJSON(GUIUtility.systemCopyBuffer, true);
                _target.Update();
                _target.ApplyModifiedProperties();
                _target.Update();
                _modifiedTextureBiome1 = true;
                _modifiedTextureBiome2 = true;
                _modifiedTextureCities = true;
                _modifiedTextureClouds = true;
                _modifiedTextureLava = true;
                _modifiedTextureLookups = true;
                _modifiedTextureMaps = true;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // PLANET BLUEPRINT
            EditorGUILayout.LabelField("PLANET BLUEPRINT", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            Manager.Instance.RefreshLists();
            List<string> _l = new List<string>();
            foreach (BlueprintSolidPlanet _cs in Manager.Instance.listSolidPlanetBlueprints)
                _l.Add(_cs.name);
            Color _orgGUIColor = GUI.color;
            EditorGUILayout.BeginHorizontal();

            if (_newSeed)
            {
                if (_resetOverrides || !_planetBlueprintOverride.boolValue)
                {
                    _script.SetPlanetBlueprint();
                    _updateSerializedPlanetCacheRequired = true;
                    _planetBlueprintOverride.boolValue = false;
                }
            }
            else
            {
                if (_planetBlueprintOverride.boolValue)
                {
                    GUI.color = _colorModified;
                }

                EditorGUI.BeginChangeCheck();
                _planetBlueprintIndex.intValue = EditorGUILayout.Popup("Planet Blueprint", _planetBlueprintIndex.intValue, _l.ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    _planetBlueprintOverride.boolValue = true;                    
                    _target.ApplyModifiedProperties();
                    _target.Update();
                    _script.SetPlanetBlueprint(_planetBlueprintIndex.intValue, true, true);
                    _newPlanetBlueprint = true;
                    _modifiedTextureBiome1 = true;
                    _modifiedTextureBiome2 = true;
                    _modifiedTextureCities = true;
                    _modifiedTextureClouds = true;
                    _modifiedTextureLava = true;
                    _modifiedTextureLookups = true;
                    _modifiedTextureMaps = true;
                    _modifiedShader = true;
                    _updateSerializedPlanetCacheRequired = true;
                }

                if (_planetBlueprintOverride.boolValue)
                {
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        _script.SetPlanetBlueprint(-1, true, true);
                        _newPlanetBlueprint = true;
                    }
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.Button(" ", GUILayout.Width(20));
                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
                GUI.color = _orgGUIColor;
                EditorGUI.indentLevel--;
            }
            // END PLANET BLUEPRINT

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("PLANET SETTINGS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            RenderPropertyFloat("alienization");
            RenderPropertyColor("specularColor");
            EditorGUI.indentLevel--;

            // CONTINENTS
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("CONTINENTS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            RenderPropertyFloat("continentSeed");
            RenderPropertyFloat("continentSize");
            RenderPropertyFloat("continentComplexity");
            EditorGUI.indentLevel--;

            // COASTS
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("COAST LINES", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            RenderPropertyFloat("coastalDetail");
            RenderPropertyFloat("coastalReach");
            EditorGUI.indentLevel--;

            // LIQUID 
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("LIQUID", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            RenderPropertyFloat("liquidLevel");
            if (GetPropertyFloat("liquidLevel").FindPropertyRelative("value").floatValue < 0.001f)
                GUI.enabled = false;
            RenderPropertyColor("liquidColor");
            RenderPropertyFloat("liquidOpacity");
            RenderPropertyFloat("liquidShallow");
            RenderPropertyFloat("liquidSpecularPower");
            GUI.enabled = true;
            EditorGUI.indentLevel--;

            // ICE
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ICE", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            RenderPropertyMaterial("polarIce");
            RenderPropertyFloat("polarCapAmount");
            RenderPropertyColor("iceColor");
            EditorGUI.indentLevel--;

            // ATMOSPHERE
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ATMOSPHERE", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            RenderPropertyColor("atmosphereColor");
            RenderPropertyFloat("atmosphereExternalSize");
            RenderPropertyFloat("atmosphereExternalDensity");
            RenderPropertyFloat("atmosphereInternalDensity");
            RenderPropertyColor("twilightColor");
            EditorGUI.indentLevel--;

            // CLOUDS
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("CLOUDS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            RenderPropertyFloat("cloudsOpacity");
            if (GetPropertyFloat("cloudsOpacity").FindPropertyRelative("value").floatValue < 0.0001f)
                GUI.enabled = false;
            RenderPropertyMaterial("clouds");
            RenderPropertyFloat("cloudsSeed");
            RenderPropertyColor("cloudsColor");
            RenderPropertyFloat("cloudsRoughness");
            RenderPropertyFloat("cloudsCoverage");
            RenderPropertyFloat("cloudsLayer1");
            RenderPropertyFloat("cloudsLayer2");
            RenderPropertyFloat("cloudsLayer3");
            RenderPropertyFloat("cloudsSharpness");
            RenderPropertyFloat("cloudsTiling");
            RenderPropertyFloat("cloudsSpeed");
            RenderPropertyFloat("cloudsHeight");
            RenderPropertyFloat("cloudsShadow");
            GUI.enabled = true;
            EditorGUI.indentLevel--;

            // LAVA
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("MOLTEN LAVA", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            RenderPropertyFloat("lavaAmount");
            if (GetPropertyFloat("lavaAmount").FindPropertyRelative("value").floatValue < 0.00001f)
                GUI.enabled = false;
            RenderPropertyMaterial("lava");
            RenderPropertyFloat("lavaComplexity");
            RenderPropertyFloat("lavaFrequency");
            RenderPropertyFloat("lavaDetail");
            RenderPropertyFloat("lavaReach");
            RenderPropertyFloat("lavaColorVariation");
            RenderPropertyFloat("lavaFlowSpeed");
            RenderPropertyFloat("lavaGlowAmount");
            RenderPropertyColor("lavaGlowColor");
            GUI.enabled = true;
            EditorGUI.indentLevel--;

            // SURFACE
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("SURFACES", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            RenderPropertyFloat("surfaceRoughness");
            RenderPropertyFloat("surfaceTiling");
            EditorGUI.indentLevel--;

            // COMPOSITION
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("COMPOSITION", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            RenderPropertyMaterial("composition");
            RenderPropertyFloat("compositionSeed");
            RenderPropertyFloat("compositionTiling");
            RenderPropertyFloat("compositionChaos");
            RenderPropertyFloat("compositionBalance");
            RenderPropertyFloat("compositionContrast");
            EditorGUI.indentLevel--;

            // BIOME 1
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("BIOME 1", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            RenderPropertyFloat("biome1Seed");
            RenderPropertyMaterial("biome1Type");
            RenderPropertyFloat("biome1Chaos");
            RenderPropertyFloat("biome1Balance");
            RenderPropertyFloat("biome1Contrast");
            RenderPropertyFloat("biome1ColorVariation");
            RenderPropertyFloat("biome1Saturation");
            RenderPropertyFloat("biome1Brightness");
            RenderPropertyFloat("biome1CratersSmall");
            RenderPropertyFloat("biome1CratersMedium");
            RenderPropertyFloat("biome1CratersLarge");
            RenderPropertyFloat("biome1CratersErosion");
            RenderPropertyFloat("biome1CratersDiffuse");
            RenderPropertyFloat("biome1CanyonsDiffuse");
            RenderPropertyFloat("biome1SurfaceBump");
            RenderPropertyFloat("biome1CratersBump");
            RenderPropertyFloat("biome1CanyonsBump");
            EditorGUI.indentLevel--;

            // BIOME 2
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("BIOME 2", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            RenderPropertyFloat("biome2Seed");
            RenderPropertyMaterial("biome2Type");
            RenderPropertyFloat("biome2Chaos");
            RenderPropertyFloat("biome2Balance");
            RenderPropertyFloat("biome2Contrast");
            RenderPropertyFloat("biome2ColorVariation");
            RenderPropertyFloat("biome2Saturation");
            RenderPropertyFloat("biome2Brightness");
            RenderPropertyFloat("biome2CratersSmall");
            RenderPropertyFloat("biome2CratersMedium");
            RenderPropertyFloat("biome2CratersLarge");
            RenderPropertyFloat("biome2CratersErosion");
            RenderPropertyFloat("biome2CratersDiffuse");
            RenderPropertyFloat("biome2CanyonsDiffuse");
            RenderPropertyFloat("biome2SurfaceBump");
            RenderPropertyFloat("biome2CratersBump");
            RenderPropertyFloat("biome2CanyonsBump");
            EditorGUI.indentLevel--;

            // CITIES
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("CITIES & POPULATION", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            RenderPropertyMaterial("cities");
            RenderPropertyFloat("citiesSeed");
            RenderPropertyFloat("citiesPopulation");
            RenderPropertyFloat("citiesAdvancement");
            RenderPropertyFloat("citiesGlow");
            RenderPropertyFloat("citiesTiling");
            RenderPropertyColor("citiesColor");
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            if (_modifiedShader)
            {
                _updateShaderNeeded.boolValue = true;
                _modifiedShader = false;
            }

            if (_modifiedTextureBiome1 || _modifiedTextureBiome2 || _modifiedTextureCities || _modifiedTextureClouds || _modifiedTextureLava || _modifiedTextureMaps)
            {
                if (!_modifyWait)
                {
                    _modifyTimestamp = Time.realtimeSinceStartup;
                    _modifyWait = true;
                }
            }

            if (_modifiedTextureLookups)
            {
                _rebuildLookupsNeeded.boolValue = true;
                _modifiedTextureLookups = false;
            }

            if (Manager.Instance.autoUpdateTextures || _buttonRebuildTextures)
            {
                if (_script.RebuildTexturesNeeded())
                {
                    _script.RebuildTextures();
                }
            }

            // Apply the modified properties        
            if (_newSeed || _newVariation || _newPlanetBlueprint)
            {
                _target.Update();
                _newSeed = false;
                _newVariation = false;
                _newPlanetBlueprint = false;                
                Repaint();
            }
            else
            {
                _target.ApplyModifiedProperties();
            }

            if (_updateSerializedPlanetCacheRequired)
            {
                _target.Update();
                _serializedPlanetCache.stringValue = _script.ExportToJSON(Planet.StringFormat.JSON_COMPACT);
                _target.ApplyModifiedProperties();
                _updateSerializedPlanetCacheRequired = false;
            }
        }

        /// <summary>
        /// Update runs in editor (only when changes are made) and a time delay is used to ensure that updates are not called too frequently which would otherwise lag the editor.
        /// </summary>
        void Update()
        {
            if (_modifyWait && Time.realtimeSinceStartup > _modifyTimestamp + MODIFY_DELAY)
            {
                _target.Update();

                if (_modifiedTextureMaps)
                {
                    _rebuildMapsNeeded.boolValue = true;
                    _modifiedTextureMaps = false;
                }
                if (_modifiedTextureCities)
                {
                    _rebuildCitiesNeeded.boolValue = true;
                    _modifiedTextureCities = false;
                }
                if (_modifiedTextureClouds)
                {
                    _rebuildCloudsNeeded.boolValue = true;
                    _modifiedTextureClouds = false;
                }
                if (_modifiedTextureLava)
                {
                    _rebuildLavaNeeded.boolValue = true;
                    _modifiedTextureLava = false;
                }
                if (_modifiedTextureBiome1)
                {
                    _rebuildBiome1Needed.boolValue = true;
                    _modifiedTextureBiome1 = false;
                }
                if (_modifiedTextureBiome2)
                {
                    _rebuildBiome2Needed.boolValue = true;
                    _modifiedTextureBiome2 = false;
                }
                _modifyTimestamp = 0f;
                _modifyWait = false;
                _target.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Renders a PropertyFloat in inspector base on the key
        /// </summary>
        /// <param name="_key"></param>
        void RenderPropertyFloat(string _key)
        {
            bool _GUIstate = GUI.enabled;
            Color _orgGUIColor = GUI.color;
            EditorGUILayout.BeginHorizontal();
            SerializedProperty _s = GetPropertyFloat(_key);
            if (_s == null)
            {
                Debug.LogError("PropertyFloat not found: " + _key);
                return;
            }

            if (_newSeed || _newVariation || _newPlanetBlueprint)
            {
                if ((_newSeed && _resetOverrides) || !_s.FindPropertyRelative("overrideRandom").boolValue)
                {
                    _script.SetPropertyFloat(_key);
                    _s.FindPropertyRelative("overrideRandom").boolValue = false;
                }
                return;
            }

            if (_s.FindPropertyRelative("overrideRandom").boolValue)
            {
                GUI.color = _colorModified;
            }

            EditorGUI.BeginChangeCheck();

            if (_s.FindPropertyRelative("clamp01").boolValue)
            {
                _s.FindPropertyRelative("value").floatValue = EditorGUILayout.Slider(_s.FindPropertyRelative("label").stringValue, _s.FindPropertyRelative("value").floatValue, 0.0f, 1.0f);
            }
            else
            {
                if (_s.FindPropertyRelative("displayAsInt").boolValue)
                {
                    _s.FindPropertyRelative("value").floatValue = EditorGUILayout.Slider(_s.FindPropertyRelative("label").stringValue, (int)_s.FindPropertyRelative("value").floatValue, (int)_s.FindPropertyRelative("minValue").floatValue, (int)_s.FindPropertyRelative("maxValue").floatValue);
                }
                else
                {
                    _s.FindPropertyRelative("value").floatValue = EditorGUILayout.Slider(_s.FindPropertyRelative("label").stringValue, _s.FindPropertyRelative("value").floatValue, _s.FindPropertyRelative("minValue").floatValue, _s.FindPropertyRelative("maxValue").floatValue);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                _s.FindPropertyRelative("overrideRandom").boolValue = true;
                _updateSerializedPlanetCacheRequired = true;
                _modifiedShader = true;
                HandleModifiedTextures(_s);
            }

            if (_s.FindPropertyRelative("overrideRandom").boolValue)
            {
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {                    
                    _target.ApplyModifiedProperties();
                    _script.SetPropertyFloat(_key);
                    _target.Update();
                    _updateSerializedPlanetCacheRequired = true;
                    HandleModifiedTextures(_s);
                }
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Button(" ", GUILayout.Width(20));
                GUI.enabled = true;
            }

            EditorGUILayout.EndHorizontal();
            GUI.color = _orgGUIColor;
            GUI.enabled = _GUIstate;
        }

        /// <summary>
        /// Gets the SerializedProperty of a PropertyFloat based on the key.
        /// </summary>
        /// <param name="_key"></param>
        /// <returns>SerializedProperty of PropertyFloat</returns>
        SerializedProperty GetPropertyFloat(string _key)
        {
            int _i = 0;
            if (!_indexFloats.TryGetValue(_key, out _i)) return null;
            return _propertyFloats.GetArrayElementAtIndex(_i);
        }

        /// <summary>
        /// Renders a editor fields for PropertyColor in the inspector
        /// </summary>
        /// <param name="_key"></param>
        void RenderPropertyColor(string _key)
        {
            bool _GUIstate = GUI.enabled;
            Color _orgGUIColor = GUI.color;
            EditorGUILayout.BeginHorizontal();

            SerializedProperty _s = GetPropertyColor(_key);
            if (_s == null)
            {
                Debug.LogError("PropertyColor not found.");
                return;
            }

            if (_newSeed || _newPlanetBlueprint)
            {
                if ((_newSeed && _resetOverrides) || !_s.FindPropertyRelative("overrideRandom").boolValue)
                {
                    _script.SetPropertyColor(_key);
                    _s.FindPropertyRelative("overrideRandom").boolValue = false;
                }
                return;
            }

            if (_s.FindPropertyRelative("overrideRandom").boolValue)
            {
                GUI.color = _colorModified;
            }

            EditorGUI.BeginChangeCheck();

            _s.FindPropertyRelative("color").colorValue = EditorGUILayout.ColorField(_s.FindPropertyRelative("label").stringValue, _s.FindPropertyRelative("color").colorValue);

            if (EditorGUI.EndChangeCheck())
            {
                _s.FindPropertyRelative("overrideRandom").boolValue = true;
                _modifiedShader = true;
                _updateSerializedPlanetCacheRequired = true;
            }

            if (_s.FindPropertyRelative("overrideRandom").boolValue)
            {
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    _target.ApplyModifiedProperties();
                    _script.SetPropertyColor(_key);
                    _target.Update();
                    _updateSerializedPlanetCacheRequired = true;
                }
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Button(" ", GUILayout.Width(20));
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

            GUI.color = _orgGUIColor;
            GUI.enabled = _GUIstate;
        }

        /// <summary>
        /// Gets the SerializedProperty of a PropertyColor based on the key.
        /// </summary>
        /// <param name="_key"></param>
        /// <returns>SerializedProperty of PropertyColor</returns>
        SerializedProperty GetPropertyColor(string _key)
        {
            int _i = 0;
            if (!_indexColors.TryGetValue(_key, out _i)) return null;
            return _propertyColors.GetArrayElementAtIndex(_i);
        }

        /// <summary>
        /// Renders custom inspector for PropertyMaterial in inspector. 
        /// </summary>
        /// <param name="_key"></param>
        void RenderPropertyMaterial(string _key)
        {
            bool _GUIstate = GUI.enabled;
            Color _orgGUIColor = GUI.color;
            EditorGUILayout.BeginHorizontal();
            SerializedProperty _s = GetPropertyMaterial(_key);
            if (_s == null)
            {
                Debug.LogError("PropertyMaterial not found.");
                return;
            }

            if (_newSeed || _newPlanetBlueprint)
            {
                if ((_newSeed && _resetOverrides) || !_s.FindPropertyRelative("overrideRandom").boolValue)
                {
                    _script.SetPropertyMaterial(_key);
                    _s.FindPropertyRelative("overrideRandom").boolValue = false;
                }
                return;
            }

            if (_s.FindPropertyRelative("overrideRandom").boolValue)
            {
                GUI.color = _colorModified;
            }

            EditorGUI.BeginChangeCheck();

            _s.FindPropertyRelative("value").intValue = EditorGUILayout.Popup(_s.FindPropertyRelative("label").stringValue, _s.FindPropertyRelative("value").intValue, GetRelativeStringArray(_s, "popupDisplay"));

            if (EditorGUI.EndChangeCheck())
            {
                _script.OverridePropertyMaterial(_key, _s.FindPropertyRelative("value").intValue);
                _s.FindPropertyRelative("overrideRandom").boolValue = true;
                _updateSerializedPlanetCacheRequired = true;
                HandleModifiedTextures(_s);
            }

            if (_s.FindPropertyRelative("overrideRandom").boolValue)
            {
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    _target.ApplyModifiedProperties();
                    _script.SetPropertyMaterial(_key);
                    _target.Update();
                    HandleModifiedTextures(_s);
                    _updateSerializedPlanetCacheRequired = true;
                }
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Button(" ", GUILayout.Width(20));
                GUI.enabled = true;
            }

            EditorGUILayout.EndHorizontal();
            GUI.color = _orgGUIColor;
            GUI.enabled = _GUIstate;
        }

        /// <summary>
        /// Handles any modified textures of - if the Property has one or more proceduralTexture reference the procedural textures need to be rebuilt.
        /// </summary>
        /// <param name="_s"></param>
        void HandleModifiedTextures(SerializedProperty _s)
        {
            if (_s.FindPropertyRelative("proceduralTextures").arraySize > 0)
            {
                for (int _i = 0; _i < _s.FindPropertyRelative("proceduralTextures").arraySize; _i++)
                {
                    switch (_s.FindPropertyRelative("proceduralTextures").GetArrayElementAtIndex(_i).stringValue)
                    {
                        case "Maps":
                            _modifiedTextureMaps = true;
                            break;
                        case "Lava":
                            _modifiedTextureLava = true;
                            break;
                        case "Clouds":
                            _modifiedTextureClouds = true;
                            break;
                        case "Cities":
                            _modifiedTextureCities = true;
                            break;
                        case "Biome1":
                            _modifiedTextureBiome1 = true;
                            break;
                        case "Biome2":
                            _modifiedTextureBiome2 = true;
                            break;
                        case "Lookups":
                            _modifiedTextureLookups = true;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the dictionaries used since Unity do not allow serialization of dictionaries (translation to/from lists are needed)
        /// </summary>
        void UpdateIndex()
        {
            _indexFloats.Clear();
            for (int _i = 0; _i < _propertyFloats.arraySize; _i++)
            {
                SerializedProperty property = _propertyFloats.GetArrayElementAtIndex(_i);
                _indexFloats.Add(property.FindPropertyRelative("key").stringValue, _i);
            }

            _indexColors.Clear();
            for (int _i = 0; _i < _propertyColors.arraySize; _i++)
            {
                SerializedProperty property = _propertyColors.GetArrayElementAtIndex(_i);
                _indexColors.Add(property.FindPropertyRelative("key").stringValue, _i);
            }

            _indexMaterials.Clear();
            for (int _i = 0; _i < _propertyMaterials.arraySize; _i++)
            {
                SerializedProperty property = _propertyMaterials.GetArrayElementAtIndex(_i);
                _indexMaterials.Add(property.FindPropertyRelative("key").stringValue, _i);
            }
        }

        /// <summary>
        /// Gets the SerializedProperty of a PropertyMaterial based on the key.
        /// </summary>
        /// <param name="_key"></param>
        /// <returns>SerializedProperty of PropertyMaterial</returns>
        SerializedProperty GetPropertyMaterial(string _key)
        {
            int _i = 0;
            if (!_indexMaterials.TryGetValue(_key, out _i)) return null;
            return _propertyMaterials.GetArrayElementAtIndex(_i);
        }

        /// <summary>
        /// Gets the relative string array of a SerializedProperty
        /// </summary>
        /// <param name="_s"></param>
        /// <param name="_relative"></param>
        /// <returns>String array of relative to a SerializedProperty</returns>
        string[] GetRelativeStringArray(SerializedProperty _s, string _relative)
        {
            string[] _string = new string[_s.FindPropertyRelative(_relative).arraySize];
            for (int _i = 0; _i < _s.FindPropertyRelative(_relative).arraySize; _i++)
                _string[_i] = _s.FindPropertyRelative(_relative).GetArrayElementAtIndex(_i).stringValue;
            return _string;
        }
    }
}
