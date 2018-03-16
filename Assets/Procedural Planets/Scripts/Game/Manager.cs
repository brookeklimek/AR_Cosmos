/*  
    Class: ProceduralPlanets.Manager
    Version: 0.1.1 (initial alpha release)
    Date: 2018-01-10
    Author: Stefan Persson
    (C) Imphenzia AB

    This manager component is always required to exists as exactly one persistant instance. 
    The manager has several purposes:
    1) It manages the blueprints that planets depend upon to obtain their configuration
    2) It queues and creates textures and allocates them to the planets
    3) It is used to create new planets either from the inspector or via script using the CreatePlanet() method.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralPlanets.SimpleJSON;

namespace ProceduralPlanets
{
    [ExecuteInEditMode]
    public class Manager : Singleton<Manager>
    {
        // Debug option for alpha version
        public int DEBUG_LEVEL = 0;

        // List of planet blueprints
        public List<BlueprintSolidPlanet> listSolidPlanetBlueprints = new List<BlueprintSolidPlanet>(0);
        public List<BlueprintGasPlanet> listGasPlanetBlueprints = new List<BlueprintGasPlanet>(0);        
        
        // Solid planet procedural materials
        public List<ProceduralMaterial> solidCompositionMaterials = new List<ProceduralMaterial>(0);
        public List<ProceduralMaterial> solidBiome1Materials = new List<ProceduralMaterial>(0);
        public List<ProceduralMaterial> solidBiome2Materials = new List<ProceduralMaterial>(0);
        public List<ProceduralMaterial> solidCloudsMaterials = new List<ProceduralMaterial>(0);
        public List<ProceduralMaterial> solidCitiesMaterials = new List<ProceduralMaterial>(0);
        public List<ProceduralMaterial> solidLavaMaterials = new List<ProceduralMaterial>(0);
        public List<ProceduralMaterial> solidPolarIceMaterials = new List<ProceduralMaterial>(0);

        // Ring procedural material
        public List<ProceduralMaterial> ringMaterials = new List<ProceduralMaterial>(0);
        
        // Resolution settings
        public int resolutionContinent = 7;
        public int resolutionBiomes = 6;
        public int resolutionClouds = 7;
        public int resolutionCities = 6;
        public int resolutionLava = 6;
        public int resolutionPolarIce = 6;

        // Automatically update textures when changed?
        public bool autoUpdateTextures = true;

        // Flag whether or not the manager is currently building any textures        
        public bool isBuilding = false;

        // Private class used to manage queueing and processing of procedural textures
        private class TextureQueue
        {
            public ProceduralMaterial proceduralMaterial;
            public ProceduralTexture proceduralTextureDiffuse;
            public ProceduralTexture proceduralTextureNormal;
            public ProceduralTexture proceduralTextureFlow;
            public PropertyFloat[] propertyFloats;
            public Object targetObject;
            public string textureName;
            public bool hasMipmaps;
            public bool isLinear;
            public bool isDone;
            public bool isBuilding;
            public enum State { WAITING, PROCESSING, FINISHED }
            public State state;
        }
        
        // A list containing the entire texture queue - the manager will process these one by one and remove them from the list/queue
        private List<TextureQueue> _listTextureQueue = new List<TextureQueue>();

        // Dictionary of planet bluprints and their calculate probability to be used for newly created planets
        private Dictionary<BlueprintPlanet, float> _planetBlueprintDictionary = new Dictionary<BlueprintPlanet, float>();

        /// <summary>
        /// Refreshes Blueprint Lists and Dictionary upon every start.
        /// </summary>
        public void Start()
        {
            if (DEBUG_LEVEL > 0)
            {
                Debug.Log("Manager.cs: DEBUG_LEVEL: " + DEBUG_LEVEL);
                Debug.Log("- copyTextureSupport: " + SystemInfo.copyTextureSupport);
                Debug.Log("- deviceModel: " + SystemInfo.deviceModel);
                Debug.Log("- deviceType: " + SystemInfo.deviceModel);
                Debug.Log("- graphicsDeviceType: " + SystemInfo.graphicsDeviceType);
                Debug.Log("- graphicsDeviceVendor: " + SystemInfo.graphicsDeviceVendor);
                Debug.Log("- graphicsDeviceVersion: " + SystemInfo.graphicsDeviceVersion);
                Debug.Log("- graphicsMemorySize: " + SystemInfo.graphicsMemorySize);
                Debug.Log("- graphicsMultiThreaded: " + SystemInfo.graphicsMultiThreaded);
                Debug.Log("- graphicsShaderLevel: " + SystemInfo.graphicsShaderLevel);
                Debug.Log("- maxTextureSize: " + SystemInfo.maxTextureSize);
                Debug.Log("- graphicsShaderLevel: " + SystemInfo.graphicsShaderLevel);
                Debug.Log("- operatingSystem: " + SystemInfo.operatingSystem);
                Debug.Log("- processorType: " + SystemInfo.processorType);
                Debug.Log("- processorFrequency: " + SystemInfo.processorFrequency);
                Debug.Log("- systemMemorySize: " + SystemInfo.systemMemorySize);
                Debug.Log("- Manager.cs: Start()");
            }

            RefreshLists();
            RefreshBlueprintDictionary();
        }

        /// <summary>
        /// Reset is called when this component is initially added to a gameobject or when reset is hit in the inspector.
        /// </summary>
        public void Reset()
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs: Reset()");

            // Refresh lists containing planet blueprints
            RefreshLists();
            // Refresh the dictionary containing planet blueprints and existence probabilities
            RefreshBlueprintDictionary();
        }

        /// <summary>
        /// Adds the Update() method to EditorApplication Update so it runs when not in play mode
        /// </summary>
        void OnEnable()
        {
#if UNITY_EDITOR
            // Add the Update() method to execute in Editor
            UnityEditor.EditorApplication.update += Update;
#endif
        }

        /// <summary>
        /// Removes the Update() method from EditorApplication so it doesn't run in Editor (since it's now disabled)
        /// </summary>
        void OnDisable()
        {
#if UNITY_EDITOR
            // Remove the Update() method from executing in Editor
            UnityEditor.EditorApplication.update -= Update;
#endif
        }
    
        void Update()
        {
            // Call planet factory update, this handles all texture building and queueing of texture builds
            PlanetFactoryUpdate();
        }

        public Planet CreatePlanet(Vector3 _position, string _jsonString)
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs: CreatePlanet(" + _position + "," + _jsonString + ")");

            // Validate and (if necessary decode) JSON string
            if (_jsonString.IsBase64()) _jsonString = _jsonString.FromBase64();
            _jsonString = _jsonString.Replace("&quot;", "\"");
            var N = JSON.Parse(_jsonString);
            if (N["category"] != "planet")
            {
                Debug.LogError("Failed to build planet. No planet category in JSON string.");
                return null;
            }

            // Create the planet using the JSON string
            return CreatePlanet(_position, -1, "", _jsonString);
        }

        public Planet CreatePlanet(Vector3 _position, int _planetSeed = -1, string _blueprintName = "", string _jsonString = null)
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs:CreatePlanet(" + _position + "," + _planetSeed + ", " + _blueprintName + "," + _jsonString + ")");

            // Refresh the blueprint dictionary 
            RefreshBlueprintDictionary();

            // If no seed is specified, use random seed
            if (_planetSeed < 0) _planetSeed = Random.Range(0, int.MaxValue - 1000000);

            // Get a random planet blueprint based on probability
            float _r = Random.Range(0.0f, 1.0f);

            // Select a blueprint 
            float _previousValue = 0f;
            Object _previousKey = null;
            BlueprintPlanet _newPlanetBlueprint = null;
            foreach (KeyValuePair<BlueprintPlanet, float> _k in _planetBlueprintDictionary)
            {
                // Select blueprint randomly based on blueprint probability (since no blueprint name was specified)
                if (_blueprintName == "")
                {
                    // Find blueprint by comparing probability to random value
                    if (_r > _previousValue && _r < _k.Value)
                        _newPlanetBlueprint = _k.Key;
                    _previousValue = _k.Value;
                    _previousKey = _k.Key;
                }
                else
                {
                    // Set a specific blueprint
                    if (_blueprintName == _k.Key.name)
                        _newPlanetBlueprint = _k.Key;
                }
            }

            // If blueprint was not found, log error and return null
            if (_newPlanetBlueprint == null)
            {
                RefreshLists();
                RefreshBlueprintDictionary();                
                Debug.LogError("Could not find the specified blueprint. Refreshing Manager and aborting. Please try again.");
                return null;
            }
            
            // Create new planet gameobject
            GameObject _planetGameObject = new GameObject();

            // Deactivate the gameobject to prevent Awake() initialization to be called prior to planet having an assigned blueprint
            _planetGameObject.SetActive(false);

            // Set name and position of planet
            _planetGameObject.name = "New Procedural Planet";
            _planetGameObject.transform.position = Vector3.zero;

            // Find the planet class (same blueprint type but without the 'Blueprint' prefix).
            System.Type _planetClass = System.Type.GetType("ProceduralPlanets." + _newPlanetBlueprint.GetType().Name.Replace("Blueprint",""));

            if (_planetClass == null)
            {
                // Planet creation was not successful, destroy gameobject
                Destroy(_planetGameObject);

                // Log error and return
                Debug.LogError("There is no planet class as specified by blueprint type (" + "ProceduralPlanets." + _newPlanetBlueprint.GetType().Name.Replace("Blueprint", "") + ").");
                return null;
            }

            // Ensure that the class is in fact a subclass of Planet and nothing else
            if (_planetClass.IsSubclassOf(typeof(Planet)))
            {
                // Add the appropriate component class for this planet
                Planet _p = (Planet)_planetGameObject.AddComponent(_planetClass);

                // Set the planet blueprint
                _p.SetPlanetBlueprint(GetPlanetBlueprintIndex(_newPlanetBlueprint), false, false);

                // Determine if this is a seed/blueprint instatiated planet or a json string importet planet
                if (_jsonString == null)
                {
                    _p.planetSeed = _planetSeed;

                    // Verify if blueprint has planetary ring blueprint
                    BlueprintRing _blueprintRing = null;
                    if ((_newPlanetBlueprint).transform.Find("Ring") != null)
                        _blueprintRing = (_newPlanetBlueprint).transform.Find("Ring").GetComponent<BlueprintRing>();

                    // Verify the probability of (and create if within probability) a ring existing for this particular planet
                    if (_blueprintRing != null)
                        if (_r < (_newPlanetBlueprint).ringProbability)
                            _planetGameObject.GetComponent<Planet>().CreateRing();
                    _p.initJSONSettings = "";
                }
                else
                {
                    // Set init JSON string - settings within string will be used to set up planet (and optional ring if present)
                    _p.initJSONSettings = _jsonString;

                    var N = JSON.Parse(_jsonString);
                    if (N != null)
                        if (N["ring"] != null)
                            _p.CreateRing(_jsonString);
                }                
                // Set the planet gameobject to active to execute Awake() method which will set configure planet according to seed/blueprint/json-string
                _planetGameObject.SetActive(true);
            }
            else
            {
                // Planet creation was not successful, destroy gameobject
                Destroy(_planetGameObject);

                // Log error and return
                Debug.LogError("The planet class is not derived from Planet class.");
                return null;
            }

            // Return a reference to the new planet        
            return _planetGameObject.GetComponent<Planet>();
        }

        /// <summary>
        /// Refreshes planet blueprint lists bases on the children of the Manager. It ensures there are no blueprint duplicate names.
        /// </summary>
        public void RefreshLists()
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs:RefreshLists()");

            // Create a list to store all blueprint names to ensure there are no duplicate names
            List<string> _blueprintNames = new List<string>();

            // Clear the solid planet blueprint list
            listSolidPlanetBlueprints.Clear();

            // Find all solid planet blueprints that are children of the manager (there should be no other blueprints components in any scene other than as direct children of the manager.
            BlueprintSolidPlanet[] _ps = gameObject.GetComponentsInChildren<BlueprintSolidPlanet>();

            // Iterate through each solid planet blueprint
            foreach (BlueprintSolidPlanet _p in _ps)
            {
                // Add the solid blueprint to the list of solid planet blueprints
                listSolidPlanetBlueprints.Add(_p);
                // Add the name of the blueprint used to check for duplicates
                _blueprintNames.Add(_p.gameObject.name);
            }

            // Clear the gas planet blueprint list
            listGasPlanetBlueprints.Clear();

            // Find all gas planet blueprints that are children of the manager (there should be no other blueprints components in any scene other than as direct children of the manager.
            BlueprintGasPlanet[] _pg = gameObject.GetComponentsInChildren<BlueprintGasPlanet>();

            // Iterate through each gas planet blueprint
            foreach (BlueprintGasPlanet _p in _pg)
            {
                // Add the gas blueprint to the list of gas planet blueprints
                listGasPlanetBlueprints.Add(_p);
                // Add the name of the blueprint used to check for duplicates
                _blueprintNames.Add(_p.gameObject.name);
            }                

            // Check for duplicate blueprint names
            if (_blueprintNames.HasDuplicates())
            {
                // Throw error to the debug log if duplicate blueprint names are found.
                Debug.LogError("Blueprints cannot have the same name.");
            }
        }

        /// <summary>
        /// Refreshes the blueprint dictionary which is used to store probability of each blueprint in relation of one another (so some planet types can be more common than others)
        /// </summary>
        public void RefreshBlueprintDictionary()
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs:RefreshBlueprintDictionary()");

            // Clear the blueprint dictionary
            _planetBlueprintDictionary.Clear();

            // Set the total probability to zero
            float _total = 0f;

            // Calculate the total probability value by iterating through all solid and gas planet blueprints
            foreach (BlueprintSolidPlanet _p in listSolidPlanetBlueprints)
                _total += _p.probability;
            foreach (BlueprintGasPlanet _p in listGasPlanetBlueprints)
                _total += _p.probability;

            // Set the temporary counter value to zero
            float _value = 0f;

            // Iterate through each solid planet blueprint
            foreach (BlueprintSolidPlanet _p in listSolidPlanetBlueprints)
            {
                // Add the blueprint as a key and the probability divided by the total amount for a unique "slot" in the lookup dictionary
                _planetBlueprintDictionary.Add(_p, _value + (_p.probability / _total));
                // Add the counter to move the "slot" forward in the lookup dictionary
                _value += _p.probability / _total;
            }

            // Iterate through each gas planet blueprint
            foreach (BlueprintGasPlanet _p in listGasPlanetBlueprints)
            {
                // Add the blueprint as a key and the probability divided by the total amount for a unique "slot" in the lookup dictionary
                _planetBlueprintDictionary.Add(_p, _value + (_p.probability / _total));
                // Add the counter to move the "slot" forward in the lookup dictionary
                _value += _p.probability / _total;
            }

        }

        /// <summary>
        /// Gets the ring blueprint by parent planet blueprint name
        /// </summary>
        /// <param name="_planetBlueprintName"></param>
        /// <returns>BlueprintRing component of a planet blueprint.</returns>
        public BlueprintRing GetRingBlueprintByPlanetBlueprintName(string _planetBlueprintName)
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs: GetRingBlueprintByPlanetBlueprintName(" + _planetBlueprintName + ")");

            // Refresh lists and dictionary
            RefreshLists();
            RefreshBlueprintDictionary();

            // Find the planet blueprint by name (it should be a child of this manager game object)
            Transform _planetBlueprint = transform.Find(_planetBlueprintName);

            // If no planet blueprint is found, log an error and return null
            if (_planetBlueprint == null)
            {
                Debug.LogError("No planet blueprint found by the name of '" + _planetBlueprintName + "' - returning null.");
                return null;
            }

            // Find the ring blueprint child transform of the planet transform, it should always be named "Ring"
            Transform _ringBlueprint = _planetBlueprint.Find("Ring");

            // If no ring blueprint is found, log an error and return null
            if (_ringBlueprint == null)
            {
                Debug.LogWarning("No ring blueprint found under the planet blueprint by the name of '" + _planetBlueprintName+ "' - returning null.");
                return null;
            }

            // Return the BlueprintRing component of the ring blueprint gameobject            
            return _ringBlueprint.GetComponent<BlueprintRing>();
        }

        /// <summary>
        /// Gets the index number of a planet blueprint based on the blueprint name (could be either solid or gas planet)
        /// </summary>
        /// <param name="_name"></param>
        /// <returns>Integer number of a planet blueprint (in the list of the planet type)</returns>
        public int GetPlanetBlueprintIndexByName(string _name)
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs: GetPlanetBlueprintIndexByName(" + _name + ")");

            // Refresh lists and dictionary
            RefreshLists();
            RefreshBlueprintDictionary();

            // Iterate through all the solid planet blueprints
            for (int _i = 0; _i < listSolidPlanetBlueprints.Count; _i++)
                // If the name exists in the solid planet blueprint list...
                if (listSolidPlanetBlueprints[_i].name == _name)
                    // Return the index number
                    return _i;

            // Iterate through all the gas planet blueprints
            for (int _i = 0; _i < listGasPlanetBlueprints.Count; _i++)
                // If the name exists in the gas planet blueprint list...
                if (listGasPlanetBlueprints[_i].name == _name)
                    // Return the index number
                    return _i;

            // No blueprint was found by that name - log error and return -1
            Debug.LogError("No blueprint name found by the name of '" + _name + "'");
            return -1;
        }

        /// <summary>
        /// Gets the planet blueprint name by index in a blueprint list
        /// </summary>
        /// <param name="_index"></param>
        /// <param name="_caller"></param>
        /// <returns>Name of the planet blueprint</returns>
        public string GetPlanetBlueprintNameByIndex(int _index, Object _caller)
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs: GetPlanetBlueprintNameByIndex(" + _index + "," + _caller + ")");

            // Refresh lists and dictionary
            RefreshLists();
            RefreshBlueprintDictionary();

            // If the caller type is a solid planet, look in the solid planet blueprint list
            if (_caller.GetType() == typeof(BlueprintSolidPlanet) || _caller.GetType() == typeof(SolidPlanet)) 
            {
                // If index is larger than the list count - return ""
                if (_index >= listSolidPlanetBlueprints.Count) return "";
                // Return name in the list at the index position
                return listSolidPlanetBlueprints[_index].name;
            }

            // If the caller type is a gas planet, look in the gas planet blueprint list
            if (_caller.GetType() == typeof(BlueprintGasPlanet) || _caller.GetType() == typeof(GasPlanet))
            {
                // If index is larger than the list count - return ""
                if (_index >= listSolidPlanetBlueprints.Count) return "";
                // Return name in the list at the index position
                return listGasPlanetBlueprints[_index].name;
            }

            // No blueprint was found - return ""
            return "";
        }

        /// <summary>
        /// Gets the planet blueprint by index in a blueprint list.
        /// </summary>
        /// <param name="_index"></param>
        /// <param name="_caller"></param>
        /// <returns>Planet blueprint</returns>
        public BlueprintPlanet GetPlanetBlueprintByIndex(int _index, Object _caller)
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs: GetPlanetBlueprintByIndex(" + _index + "," + _caller + ")");

            // Refresh lists and dictionary
            RefreshLists();
            RefreshBlueprintDictionary();

            // If the caller type is a solid planet, look in the solid planet blueprint list
            if (_caller.GetType() == typeof(BlueprintSolidPlanet) || _caller.GetType() == typeof(SolidPlanet))
            {
                // If index is larger than the list count - return null
                if (_index >= listSolidPlanetBlueprints.Count) return null;
                // Return name in the list at the index position
                return listSolidPlanetBlueprints[_index];
            }

            // If the caller type is a gas planet, look in the gas planet blueprint list
            if (_caller.GetType() == typeof(BlueprintGasPlanet) || _caller.GetType() == typeof(GasPlanet))
            {
                // If index is larger than the list count - return null
                if (_index >= listSolidPlanetBlueprints.Count) return null;
                // Return name in the list at the index position
                return listGasPlanetBlueprints[_index];
            }

            // No blueprint was found - return null
            return null;
        }

        /// <summary>
        /// Get planet blueprint index based on solid planet blueprint object
        /// </summary>
        /// <param name="_object"></param>
        /// <returns>Integer index of a planet blueprint in the manager list of blueprints</returns>
        private int GetPlanetBlueprintIndex(Object _object)
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs: GetPlanetBlueprintIndex(" + _object + ")");

            // Refresh lists and dictionary
            RefreshLists();
            RefreshBlueprintDictionary();

            // If the caller type is a solid planet, look in the solid planet blueprint list
            if (_object.GetType() == typeof(BlueprintSolidPlanet) || _object.GetType() == typeof(SolidPlanet))
                // Iterate through all the solid planet blueprints
                for (int _i = 0; _i < listSolidPlanetBlueprints.Count; _i++)
                    // If the object is found in the list...
                    if (((BlueprintSolidPlanet)_object) == listSolidPlanetBlueprints[_i])
                        // Return the index number of the blueprint
                        return _i;

            // If the caller type is a gas planet, look in the solid planet blueprint list
            if (_object.GetType() == typeof(BlueprintGasPlanet) || _object.GetType() == typeof(GasPlanet))
                // Iterate through all the solid planet blueprints
                for (int _i = 0; _i < listGasPlanetBlueprints.Count; _i++)
                    // If the object is found in the list...
                    if (((BlueprintSolidPlanet)_object) == listGasPlanetBlueprints[_i])
                        // Return the index number of the blueprint
                        return _i;

            // No blueprint was found - return -1
            return -1;
        }

        /// <summary>
        /// Build a procedural texture by adding it to the texture queue. 
        /// Since there is only one instance of procedural materials they need to be built in a serial process so this method adds the procedural material to the queue
        /// which contains a reference back to the target object so the texture can be sent back once it has been built. The texture queue is processed in the Update() method.
        /// </summary>
        /// <param name="_proceduralMaterial"></param>
        /// <param name="_textureName"></param>
        /// <param name="_hasMipmaps"></param>
        /// <param name="_isLinear"></param>
        /// <param name="_propertyFloats"></param>
        /// <param name="_targetObject"></param>
        public void BuildTexture(ProceduralMaterial _proceduralMaterial, string _textureName, bool _hasMipmaps, bool _isLinear, PropertyFloat[] _propertyFloats, Object _targetObject)
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs: BuildTexture(" + _proceduralMaterial + "," + _textureName + "," + _hasMipmaps + "," + _isLinear + "," + _propertyFloats + "," + _targetObject + ")");

            // Add to the texture queue
            TextureQueue _tq = new TextureQueue();
            _tq.proceduralMaterial = _proceduralMaterial;
            _tq.propertyFloats = _propertyFloats;
            _tq.textureName = _textureName;
            _tq.targetObject = _targetObject;
            _tq.hasMipmaps = _hasMipmaps;
            _tq.isLinear = _isLinear;
            _tq.state = TextureQueue.State.WAITING;
            _tq.isDone = false;
            _listTextureQueue.Add(_tq);

        }   
             
        /// <summary>
        /// Allows a Planet to query if there are textures in the queue that are waiting to be processed or is being processed.
        /// </summary>
        /// <param name="_caller"></param>
        /// <returns>True/False</returns>
        public bool HasTexturesInQueue(Object _caller)
        {
            if (DEBUG_LEVEL > 1) Debug.Log("Manager.cs: HasTexturesInQueue(" + _caller + ")");
            if (_caller == null) return false;

            foreach (TextureQueue _tq in _listTextureQueue)
                if (_tq.targetObject != null)
                {
                    if (_tq.targetObject.GetType().IsSubclassOf(typeof(Planet)))
                        if (((Planet)_tq.targetObject).gameObject == _caller) return true;
                    if (_tq.targetObject.GetType() == typeof(Ring))
                        if (((Ring)_tq.targetObject).gameObject == _caller) return true;
                }                
            return false;
        }

        /// <summary>
        /// Handles the texture queue by looking at the queue every frame and starts processing a texture in the queue if nothing is currently being processed.
        /// </summary>
        void PlanetFactoryUpdate()
        {
            // Iterate through the texture queue
            for (int _i = _listTextureQueue.Count - 1; _i >= 0; _i--)
                // If a texture is done processing
                if (_listTextureQueue[_i].isDone || _listTextureQueue[_i].targetObject == null)
                    // Remove it from the queue
                    _listTextureQueue.RemoveAt(_i);

            // If there is no item in the texture queue - return (because there is nothing to do)
            if (_listTextureQueue.Count == 0) return;
           
            // Iterate through the texture queue...
            for (int _i = 0; _i < _listTextureQueue.Count; _i++)
                // If any item in the texture queue is processing - return (because we only want to build one texture at a time)
                if (_listTextureQueue[_i].isBuilding) return;

            // Get the reference to the first item in the texture queue
            TextureQueue _tqItem = _listTextureQueue[0];

            // Set the building flag of the texture queue item to true (so no other texture can be processed until this one is done)
            _tqItem.isBuilding = true;

            // Iterate through all the property floats that were included for this procedural texture
            for (int _i = 0; _i < _tqItem.propertyFloats.Length; _i++)
            {
                // Get a reference to each PropertyFloat
                PropertyFloat _p = _tqItem.propertyFloats[_i];

                // Use a temporary variable for value because we may want to get an interprolated value between the min/max of the property float
                float _value = _p.value;

                // If the PropertyFloat is set to LERP - set the value to by using the value to get an interpolated value between the min and max of the property float
                if (_p.shaderMethod == PropertyFloat.Method.LERP) _value = _p.GetPropertyLerp();

                // Set the procedural material float to the value to be used when building the texture
                _tqItem.proceduralMaterial.SetProceduralFloat(_tqItem.propertyFloats[_i].shaderProperty, _value);
            }

            // Set the resolution based on the texture map name (this can and should be optimized!)
            switch (_tqItem.textureName)
            {
                case "Maps":
                    _tqItem.proceduralMaterial.SetProceduralVector("$outputsize", new Vector4(resolutionContinent + 4, resolutionContinent + 4, 0, 0));
                    break;
                case "Biome1":
                    _tqItem.proceduralMaterial.SetProceduralVector("$outputsize", new Vector4(resolutionBiomes + 4, resolutionBiomes + 4, 0, 0));
                    break;
                case "Biome2":
                    _tqItem.proceduralMaterial.SetProceduralVector("$outputsize", new Vector4(resolutionBiomes + 4, resolutionBiomes + 4, 0, 0));
                    break;
                case "Clouds":
                    _tqItem.proceduralMaterial.SetProceduralVector("$outputsize", new Vector4(resolutionClouds + 4, resolutionClouds + 4, 0, 0));
                    break;
                case "Cities":
                    _tqItem.proceduralMaterial.SetProceduralVector("$outputsize", new Vector4(resolutionCities + 4, resolutionCities + 4, 0, 0));
                    break;
                case "Lava":
                    _tqItem.proceduralMaterial.SetProceduralVector("$outputsize", new Vector4(resolutionLava + 4, resolutionLava + 4, 0, 0));
                    break;
                case "PolarIce":
                    _tqItem.proceduralMaterial.SetProceduralVector("$outputsize", new Vector4(resolutionPolarIce + 4, resolutionPolarIce + 4, 0, 0));
                    break;
            }

            // Start rebuilding the texture(s) of the procedural material
            _tqItem.proceduralMaterial.RebuildTextures();

            // We need to build textures both in the editor and in-game...
            if (Application.isPlaying)
                // If in Play mode - start a normal co-routine to build the texture
                StartCoroutine(BuildTextureRoutine(_tqItem));
            else
            {
#if UNITY_EDITOR
                // If in Editor mode - a workaround has to be implemented since normal co-routines do not run
                EditorCoroutine.start(BuildTextureRoutine(_tqItem));
#endif
            }
        }

        /// <summary>
        /// Copies a texture and sends it back to the original object once a procedural material has finished building a texture.
        /// </summary>
        /// <param name="_textureQueue"></param>
        /// <returns>IEnumerator (coroutine)</returns>
        IEnumerator BuildTextureRoutine(TextureQueue _textureQueue)
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs: IEnumerator BuildTextureRoutine(" + _textureQueue + ")");

            // While texture is buing built - wait (by yield return null of the coroutine)
            while (_textureQueue.proceduralMaterial.isProcessing)
                yield return null;

            // Procedrual material is no longer processing - get the texture references
            _textureQueue.proceduralTextureDiffuse = _textureQueue.proceduralMaterial.GetGeneratedTexture(_textureQueue.proceduralMaterial.name + "_Diffuse");
            _textureQueue.proceduralTextureNormal = _textureQueue.proceduralMaterial.GetGeneratedTexture(_textureQueue.proceduralMaterial.name + "_Normal");
            _textureQueue.proceduralTextureFlow = _textureQueue.proceduralMaterial.GetGeneratedTexture(_textureQueue.proceduralMaterial.name + "_Flow");

            // Create a new Texture2D maching the resolutuion and properties of the procedural material
            Texture2D _textureDiffuse = new Texture2D(_textureQueue.proceduralTextureDiffuse.width, _textureQueue.proceduralTextureDiffuse.height, _textureQueue.proceduralTextureDiffuse.format, _textureQueue.hasMipmaps, _textureQueue.isLinear);

            // Copy the procedural diffuse texture to the newly created texture
            Graphics.CopyTexture(_textureQueue.proceduralTextureDiffuse, _textureDiffuse);

            // Set normal and flow textures to null (as they may or may not be used)
            Texture2D _textureNormal = null;
            Texture2D _textureFlow = null;

            // If there is a normal map for this texture queue item...
            if (_textureQueue.proceduralTextureNormal != null)
            {
                // Create a new Texture2D matching the resolution and properties of the procedural material
                _textureNormal = new Texture2D(_textureQueue.proceduralTextureNormal.width, _textureQueue.proceduralTextureNormal.height, _textureQueue.proceduralTextureNormal.format, _textureQueue.hasMipmaps, _textureQueue.isLinear);
                // Copy the procedural normal map texture to the newly created texture
                Graphics.CopyTexture(_textureQueue.proceduralTextureNormal, _textureNormal);
            }

            // If there is a flow map for this texture queue item...
            if (_textureQueue.proceduralTextureFlow != null)
            {
                // Create a new Texture2D matching the resolution and properties of the procedural material
                _textureFlow = new Texture2D(_textureQueue.proceduralTextureFlow.width, _textureQueue.proceduralTextureFlow.height, _textureQueue.proceduralTextureFlow.format, _textureQueue.hasMipmaps, _textureQueue.isLinear);
                // Copy the procedural normal map texture to the newly created texture
                Graphics.CopyTexture(_textureQueue.proceduralTextureFlow, _textureFlow);
            }


            // If the target object is a planet - call the SetTexture method of the Planet object and include the newly copied textures
            if (_textureQueue.targetObject is Planet)
                ((Planet)_textureQueue.targetObject).SetTexture(_textureQueue.textureName, _textureDiffuse, _textureNormal, _textureFlow);

            // If the target object is a ring - call the SetTexture method of the Ring object and include the newly copied texture
            if (_textureQueue.targetObject is Ring)
                ((Ring)_textureQueue.targetObject).SetTexture(_textureQueue.textureName, _textureDiffuse);

            // Set the isDone flag of this texture queue item - it will be removed in the next frame cycle
            _textureQueue.isDone = true;
        }

        /// <summary>
        /// Answers the questions if any textures are being built for a particular planet at the moment.
        /// </summary>
        /// <param name="_planet"></param>
        /// <returns>True/False</returns>
        public bool IsBuilding(Planet _planet)
        {
            // If the planet method is directly reporting that it's regenerating textures - return true
            if (_planet.IsRegeneratingTextures()) return true;

            // Iterate through the texture queue...
            foreach (TextureQueue _tq in _listTextureQueue)
                // If a texture queue item belongs to the planet - return true
                if (_tq.targetObject == _planet) return true;

            // No texture is being processed for the planet - return false
            return false;
        }

        /// <summary>
        /// Exports all planet blueprints (and any child ring blueprints) to clipboard as a JSON string.
        /// </summary>
        public void ExportAllBlueprintsToClipboard()
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs: ExportAllBlueprintsToClipboard()");

            // Initialize the JSON string
            string _str = "{";

            // Iterate through all solid planet blueprints
            for (int _i = 0; _i < listSolidPlanetBlueprints.Count; _i++)
            {
                // Add an item with counter (to avoid duplicate keys)
                _str += "\r\n  \"item" + _i + "\" : ";
                // Add the exported solid planet blueprint (and take care of formatting)
                _str += listSolidPlanetBlueprints[_i].ExportToJSON(true).Replace("\r\n  ", "\r\n    ").Replace("\r\n}", "\r\n  }");
                // If there are more solid planet blueprints (or any gas planet blueprints), add a comma
                if (_i < listSolidPlanetBlueprints.Count - 1 || listGasPlanetBlueprints.Count > 0) _str += ",";
            }

            // Iterate through all gas planet blueprints
            for (int _i = 0; _i < listGasPlanetBlueprints.Count; _i++)
            {
                // Add an item with counter (to avoid duplicate keys)
                _str += "\r\n  \"item" + _i + "\" : ";
                // Add the exported gas planet blueprint (and take care of formatting)
                _str += listGasPlanetBlueprints[_i].ExportToJSON(true).Replace("\r\n  ", "\r\n    ").Replace("\r\n}", "\r\n  }");
                // If there are more gas planet blueprints, add a comma
                if (_i < listGasPlanetBlueprints.Count - 1) _str += ",";
            }

            // Close the JSON string
            _str += "\r\n}";

            // Copy JSON string to clip board
            GUIUtility.systemCopyBuffer = _str;

            // Show an editor dialog to confirm export to clipboard.
#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog("Finished", "All blueprints were saved to clipboard", "Close");
#endif
        }

        /// <summary>
        /// Imports blueprints from clipboard.
        /// </summary>
        public void ImportBlueprintsFromClipboard()
        {
            if (DEBUG_LEVEL > 0) Debug.Log("Manager.cs: ImportBlueprintsFromClipboard()");

            // Parse the JSON string from the clipboard
            var N = JSON.Parse(GUIUtility.systemCopyBuffer);

            // Validate JSON to ensure it's a planetSolid string and that it contains required properties.
            if (N == null)
            {
                Debug.LogWarning("Corrupt - could not parse JSON. Aborting.");
                return;
            }

            // Iterate through all numbered items
            int _i = 0;
            while (N["item" + _i] != null)
            {
                // If item is a blueprint...
                if (N["item" + _i]["category"] == "blueprint")
                {
                    // If blueprint already exists...
                    if (transform.Find(N["item" + _i]["name"]) != null)
                        // Destroy the blueprint
                        DestroyImmediate(transform.Find(N["item" + _i]["name"]).gameObject);

                    // Create a new gameobject for the planet blueprint
                    GameObject _go = new GameObject();

                    // Set the name of the planet blueprint
                    _go.name = N["item" + _i]["name"];

                    // Parent the new blueprint to the manager
                    _go.transform.parent = transform;

                    // Set the blueprint class to type of planet blueprint
                    System.Type _blueprintClass = System.Type.GetType("ProceduralPlanets." + N["item" + _i]["type"]);

                    if (_blueprintClass == null)
                    {
                        Debug.LogError("The specified blueprint class does not exist (" + "ProceduralPlanets." + N["item" + _i]["type"] + "). Skipping.");                        
                    }
                    else
                    {
                        // If the blueprint is a subclass of BlueprintPlanet...
                        if (_blueprintClass.IsSubclassOf(typeof(BlueprintPlanet)))
                        {
                            // Add the component of the blueprint planet
                            BlueprintPlanet _c = (BlueprintPlanet)_go.AddComponent(_blueprintClass);

                            // Call the import method of the planet blueprint to import the specific planet blueprint
                            _c.ImportFromJSON(N["item" + _i].ToString());
                        }
                    }
                }
                // Increment the counter
                _i++;
            }
        }

        /// <summary>
        /// Gets ta new unique blueprint name
        /// </summary>
        /// <returns>String containing a unique blueprint name</returns>
        public string GetUniqueBlueprintName()
        {
            if (DEBUG_LEVEL > 0) Debug.Log("GetUniqueBlueprintName()");

            // Initialize temporary variables
            int _i = 0;
            string _name = "";

            // Loop until a new name is found (or until a ceiling is hit to avoid infinite loop)
            while (_name == "" || _i++ > 999)
                // If a child transform can't be found with a name - set the name variable
                if (transform.Find("New_Blueprint_" + _i) == null) _name = "New_Blueprint_" + _i;

            // Return the unique name string
            return _name;
        }
    }
}
