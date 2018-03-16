/*  
    Class: ProceduralPlanets.SolidPlanet
    Version: 0.1.1 (alpha release)
    Date: 2018-01-10
    Author: Stefan Persson
    (C) Imphenzia AB

    This component is used by solid planets and it's derived from Planet.
*/

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProceduralPlanets.SimpleJSON;

namespace ProceduralPlanets
{
    // Execute in edit mode because we want to be able to change planet parameter and rebuild textures in editor
    [ExecuteInEditMode]

    // Require MeshFilter and MeshRenderer for planet
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SolidPlanet : Planet
    {
        // Flags to indicate if maps need to be rebuilt and if shader needs to be updated with new properties
        public bool rebuildMapsNeeded = false;
        public bool rebuildBiome1Needed = false;
        public bool rebuildBiome2Needed = false;
        public bool rebuildCitiesNeeded = false;
        public bool rebuildCloudsNeeded = false;
        public bool rebuildLavaNeeded = false;
        public bool rebuildLookupsNeeded = false;
        public bool updateShaderNeeded = false;

        // Procedural materials for planet textures
        public ProceduralMaterial proceduralMaterialMaps;
        public ProceduralMaterial proceduralMaterialBiome1;
        public ProceduralMaterial proceduralMaterialBiome2;
        public ProceduralMaterial proceduralMaterialPolarIce;
        public ProceduralMaterial proceduralMaterialCities;
        public ProceduralMaterial proceduralMaterialClouds;
        public ProceduralMaterial proceduralMaterialLava;

        // Textures used by the planet
        private Texture2D _textureMaps;
        private Texture2D _textureBiome1DiffSpec;
        private Texture2D _textureBiome1Normal;
        private Texture2D _textureBiome2DiffSpec;
        private Texture2D _textureBiome2Normal;
        private Texture2D _textureIceDiffuse;
        private Texture2D _textureCities;
        private Texture2D _textureClouds;
        private Texture2D _textureLavaDiffuse;
        private Texture2D _textureLavaFlow;

        // Lookup textures for fast shader lookup of liquid, lava and polar cap coverage
        private Texture2D _textureLookupLiquid;
        private Texture2D _textureLookupLava;
        private Texture2D _textureLookupLavaGlow;
        private Texture2D _textureLookupPolar;

        // Materials
        public Material material;
        public Material externalAtmosphereMaterial;

        // External Atmosphere
        private GameObject _externalAtmosphere;
        private MeshFilter _externalAtmosphereMeshFilter;
        private MeshRenderer _externalAtmosphereRenderer;


        /// <summary>
        /// Creates the planet with serialized parameters (this happens every play/stop in editor)
        /// </summary>
        protected override void Awake()
        {
            if (Manager.Instance.DEBUG_LEVEL > 0) Debug.Log("PlanetSolid.cs: Awake()");
            if (Manager.Instance.DEBUG_LEVEL > 0) Debug.Log("- PlanetVersion: " + PLANET_VERSION);

            // Set Shader property int IDs for increased performance when updating property parameters
            _shaderID_LocalStarPosition = Shader.PropertyToID("_LocalStarPosition");
            _shaderID_LocalStarColor = Shader.PropertyToID("_LocalStarColor");
            _shaderID_LocalStarIntensity = Shader.PropertyToID("_LocalStarIntensity");
            _shaderID_LocalStarAmbientIntensity = Shader.PropertyToID("_LocalStarAmbientIntensity");

            if (planetBlueprintIndex != -1 || blueprint == null)
                SetPlanetBlueprint(planetBlueprintIndex, true, false);

            // Ensure that there is a LocalStar in the scene.
            if (FindObjectOfType<LocalStar>() == null)
                Debug.LogWarning("There is no LocalStar in the scene. Planet will not be lit. Create a game object and add the LocalStar component. The position of the game object will be the light source.");

            // Create procedural octahedron sphere mesh (used by both planet and external atmosphere)
            _mesh = ProceduralOctahedron.Create(6, 5.0f);

            // Get reference to the MeshFilter component
            _meshFilter = gameObject.GetComponent<MeshFilter>();

            // Set the octahedron sphere to be the mesh of the MeshFilter component
            _meshFilter.sharedMesh = _mesh;

            // Get reference to MeshRenderer Component
            _meshRenderer = gameObject.GetComponent<MeshRenderer>();

            // Create the planet material and set the material for the MeshRenderer component
            if (material == null)
            {
                if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                    material = new Material(Shader.Find("ProceduralPlanets/SolidPlanetLinear"));
                else
                    material = new Material(Shader.Find("ProceduralPlanets/SolidPlanetGamma"));

                _meshRenderer.material = material;
            }

            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                material.shader = Shader.Find("ProceduralPlanets/SolidPlanetLinear");
            }                            
            else
            {
                material.shader = Shader.Find("ProceduralPlanets/SolidPlanetGamma");
            }
                

            // Create or get reference to external atmosphere gameobject
            if (transform.Find("ExternalAtmosphere") == null)
                _externalAtmosphere = new GameObject("ExternalAtmosphere");
            else
                _externalAtmosphere = transform.Find("ExternalAtmosphere").gameObject;

            // Parent atmosphere to planet and hide in hierarchy
            _externalAtmosphere.transform.parent = transform;
            _externalAtmosphere.gameObject.layer = gameObject.layer;
            _externalAtmosphere.transform.localPosition = Vector3.zero;
            _externalAtmosphere.gameObject.hideFlags = HideFlags.HideInHierarchy;
            //_externalAtmosphere.gameObject.hideFlags = HideFlags.None;

            // Create or get reference to atmosphere MeshFilter Component
            if (_externalAtmosphere.GetComponent<MeshFilter>() == null)
                _externalAtmosphereMeshFilter = _externalAtmosphere.AddComponent<MeshFilter>();
            else
                _externalAtmosphereMeshFilter = _externalAtmosphere.GetComponent<MeshFilter>();

            // Use the planet's procedural octahedron sphere mesh as the atmosphere mesh as well
            _externalAtmosphereMeshFilter.sharedMesh = _meshFilter.sharedMesh;

            // Create external atmosphere material
            if (externalAtmosphereMaterial == null)
                externalAtmosphereMaterial = new Material(Shader.Find("ProceduralPlanets/Atmosphere"));

            // Create or get reference to atmosphere MeshRenderer Component
            if (_externalAtmosphere.GetComponent<MeshRenderer>() == null)
                _externalAtmosphereRenderer = _externalAtmosphere.AddComponent<MeshRenderer>();
            else
                _externalAtmosphereRenderer = _externalAtmosphere.GetComponent<MeshRenderer>();

            // Set atmosphere material
            _externalAtmosphereRenderer.sharedMaterial = externalAtmosphereMaterial;

            // Disable shadows for atmosphere
            _externalAtmosphereRenderer.receiveShadows = false;
            _externalAtmosphereRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Clear properties lists
            propertyFloats.Clear();
            propertyMaterials.Clear();
            propertyColors.Clear();

            // Add property materials
            AddPropertyMaterial("composition", "Composition*", Manager.Instance.solidCompositionMaterials.ToArray(), 1, new string[] { "Maps" });
            AddPropertyMaterial("polarIce", "Polar Ice", Manager.Instance.solidPolarIceMaterials.ToArray(), 4321, new string[] { "PolarIce" });
            AddPropertyMaterial("clouds", "Clouds*", Manager.Instance.solidCloudsMaterials.ToArray(), 1, new string[] { "Clouds" });
            AddPropertyMaterial("lava", "Lava*", Manager.Instance.solidLavaMaterials.ToArray(), 1821, new string[] { "Lava" });
            AddPropertyMaterial("biome1Type", "Biome 1 Type*", Manager.Instance.solidBiome1Materials.ToArray(), 455, new string[] { "Biome1" });
            AddPropertyMaterial("biome2Type", "Biome 2 Type*", Manager.Instance.solidBiome2Materials.ToArray(), 615, new string[] { "Biome2" });
            AddPropertyMaterial("cities", "Cities*", Manager.Instance.solidCitiesMaterials.ToArray(), 1, new string[] { "Cities" });

            // Update dictionaries (for materials a this stage)
            UpdateDictionariesIfNeeded(true);

            // Set default properties (for materials at this stage)
            SetDefaultProperties();

            // Get references to newly created property materials
            proceduralMaterialMaps = _dictionaryMaterials["composition"].GetPropertyMaterial();
            proceduralMaterialBiome1 = _dictionaryMaterials["biome1Type"].GetPropertyMaterial();
            proceduralMaterialBiome2 = _dictionaryMaterials["biome2Type"].GetPropertyMaterial();
            proceduralMaterialPolarIce = _dictionaryMaterials["polarIce"].GetPropertyMaterial();
            proceduralMaterialLava = _dictionaryMaterials["lava"].GetPropertyMaterial();
            proceduralMaterialCities = _dictionaryMaterials["cities"].GetPropertyMaterial();
            proceduralMaterialClouds = _dictionaryMaterials["clouds"].GetPropertyMaterial();

            // Add Float (within a range of min/max) and color properties
            AddPropertyFloat("alienization", "Alienization*", 0.0f, 1.0f, true, false, 2, false, new string[] { "Biome1", "Biome2" }, new ProceduralMaterial[] { proceduralMaterialBiome1, proceduralMaterialBiome2 }, "Biome_Alienization", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyColor("specularColor", "Specular Color", new Color(0.4f, 0.4f, 0.05f, 1.0f), 0.05f, 0.05f, 0.05f, 5, false, material, "_ColorSpecular");
            AddPropertyFloat("continentSeed", "Continent Seed*", 0, 255, false, true, 10, true, new string[] { "Maps" }, proceduralMaterialMaps, "MapHeight_Random_Seed", PropertyFloat.Method.VALUE, PropertyFloat.DataType.INT);
            AddPropertyFloat("continentSize", "Continent Size", 10, 1, true, false, 30, false, null, material, "_TilingHeightBase", PropertyFloat.Method.LERP, PropertyFloat.DataType.INT);
            AddPropertyFloat("continentComplexity", "Continent Complexity*", 0.0f, 20.0f, true, false, 20, false, new string[] { "Maps" }, proceduralMaterialMaps, "MapHeight_Warp", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("coastalDetail", "Coastal Detail", 1, 50, true, false, 40, false, null, material, "_TilingHeightDetail", PropertyFloat.Method.LERP, PropertyFloat.DataType.INT);
            AddPropertyFloat("coastalReach", "Coastal Reach", 0.0f, 0.2f, true, false, 50, false, null, material, "_DetailHeight", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("liquidLevel", "Liquid Level", 0.0f, 1.0f, true, false, 60, true, new string[] { "Lookups" });
            AddPropertyColor("liquidColor", "LiquidColor", new Color(0.0f, 0.0f, 0.3f, 1.0f), 0.02f, 0.3f, 0.3f, 70, true, material, "_ColorLiquid");
            AddPropertyFloat("liquidOpacity", "Liquid Opacity", 0.0f, 1.0f, true, false, 80, false, null, material, "_LiquidOpacity", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("liquidShallow", "Shallow Distance", 0.0f, 1.0f, true, false, 90, false, new string[] { "Lookups" });
            AddPropertyFloat("liquidSpecularPower", "Specular Power", 1.0f, 50.0f, true, false, 100, false, null, material, "_SpecularPowerLiquid", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("polarCapAmount", "Polar Caps", 1.0f, 0.2f, true, false, 110, true, new string[] { "Lookups" });
            AddPropertyColor("iceColor", "Ice Color", new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.1f, 0.1f, 0.1f, 120, true, material, "_ColorIce");
            AddPropertyColor("atmosphereColor", "Atmosphere Color", new Color(0.2f, 0.75f, 1.0f, 1.0f), 0.2f, 0.2f, 0.2f, 130, true, new Material[] { material, externalAtmosphereMaterial }, "_ColorAtmosphere");
            AddPropertyFloat("atmosphereExternalSize", "External Size", 0.0f, 0.5f, true, false, 140, true);
            AddPropertyFloat("atmosphereExternalDensity", "External Density", 1.7f, 1.2f, true, false, 140, true);
            AddPropertyFloat("atmosphereInternalDensity", "Internal Density", 20.0f, 3.0f, true, false, 150, true, null, material, "_AtmosphereFalloff", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyColor("twilightColor", "Twilight Color", new Color(0.25f, 0.2f, 0.05f, 1.0f), 0.05f, 0.2f, 0.2f, 160, true, material, "_ColorTwilight");
            AddPropertyFloat("cloudsOpacity", "Clouds Opacity", 0.0f, 1.0f, true, false, 170, false, null, material, "_CloudOpacity", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyColor("cloudsColor", "Clouds Color", new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.1f, 0.1f, 0.1f, 180, true, material, "_ColorClouds");
            AddPropertyFloat("cloudsSeed", "Clouds Seed*", 0, 255, false, true, 200, true, new string[] { "Clouds" }, proceduralMaterialClouds, "$randomseed", PropertyFloat.Method.VALUE, PropertyFloat.DataType.INT);
            AddPropertyFloat("cloudsCoverage", "Clouds Coverage*", 1.0f, 0.2f, true, false, 190, true, new string[] { "Clouds" }, proceduralMaterialClouds, "Coverage", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("cloudsLayer1", "Clouds Layer 1*", 0.0f, 1.0f, true, false, 210, true, new string[] { "Clouds" }, proceduralMaterialClouds, "Layer1_Opacity", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("cloudsLayer2", "Clouds Layer 2*", 0.0f, 1.0f, true, false, 220, true, new string[] { "Clouds" }, proceduralMaterialClouds, "Layer2_Opacity", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("cloudsLayer3", "Clouds Layer 3*", 0.0f, 1.0f, true, false, 230, true, new string[] { "Clouds" }, proceduralMaterialClouds, "Layer3_Opacity", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("cloudsSharpness", "Clouds Sharpness*", 0.0f, 1.0f, true, false, 240, false, new string[] { "Clouds" }, proceduralMaterialClouds, "Sharpness", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("cloudsRoughness", "Clouds Roughness*", 0.0f, 1.0f, true, false, 250, false, new string[] { "Clouds" }, proceduralMaterialClouds, "Roughness", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("cloudsTiling", "Clouds Tiling", 1, 40, false, true, 260, false, null, material, "_TilingClouds", PropertyFloat.Method.VALUE, PropertyFloat.DataType.INT);
            AddPropertyFloat("cloudsSpeed", "Clouds Speed", 0.0f, 50.0f, true, false, 270, false, null, material, "_CloudSpeed", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("cloudsHeight", "Clouds height", 0.0f, 15.0f, true, false, 280, false, null, material, "_CloudHeight", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("cloudsShadow", "Clouds Shadow", 0.0f, 1.0f, true, false, 290, false, null, material, "_CloudShadow", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("lavaAmount", "Lava Amount", 0.0f, 1f, true, false, 300, true, new string[] { "Lookups" });
            AddPropertyFloat("lavaComplexity", "Lava Complexity*", 10.0f, 20.0f, true, true, 310, true, new string[] { "Maps" }, proceduralMaterialMaps, "MapLava_Warp", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("lavaFrequency", "Lava Frequency", 1, 10, true, false, 320, true, null, material, "_TilingLavaBase", PropertyFloat.Method.LERP, PropertyFloat.DataType.INT);
            AddPropertyFloat("lavaDetail", "Lava Detail", 1, 40, true, false, 330, true, null, material, "_TilingLavaDetail", PropertyFloat.Method.LERP, PropertyFloat.DataType.INT);
            AddPropertyFloat("lavaReach", "Lava Reach", 0.0f, 0.3f, true, false, 340, true, null, material, "_DetailLava", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("lavaColorVariation", "Color Variation*", 0.48f, 0.6f, true, false, 350, true, new string[] { "Lava" }, proceduralMaterialLava, "Lava_Hue", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("lavaFlowSpeed", "Flow Speed", 0.0f, 1.0f, true, false, 360, false, null, material, "_LavaFlowSpeed", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("lavaGlowAmount", "Glow Amount", 0.0f, 0.05f, true, false, 370, true, new string[] { "Lookups" });
            AddPropertyColor("lavaGlowColor", "Glow Color", new Color(1.0f, 0.4f, 0.0f, 1.0f), 0.2f, 0.2f, 0.4f, 380, true, material, "_ColorLavaGlow");
            AddPropertyFloat("surfaceTiling", "Surface Tiling", 1, 30, false, true, 390, false, null, material, "_TilingSurface", PropertyFloat.Method.VALUE, PropertyFloat.DataType.INT);
            AddPropertyFloat("surfaceRoughness", "Surface Roughness", 1.0f, 0.1f, true, false, 400, false, null, material, "_SurfaceRoughness", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("compositionSeed", "Composition Seed*", 0, 255, false, true, 10, true, new string[] { "Maps" }, proceduralMaterialMaps, "MapBiome_Random_Seed", PropertyFloat.Method.VALUE, PropertyFloat.DataType.INT);
            AddPropertyFloat("compositionTiling", "Composition Tiling", 1, 10, false, true, 410, true, null, material, "_TilingBiome", PropertyFloat.Method.VALUE, PropertyFloat.DataType.INT);
            AddPropertyFloat("compositionChaos", "Composition Chaos*", 1.0f, 10.0f, true, false, 420, true, new string[] { "Maps" }, proceduralMaterialMaps, "MapBiome_Warp", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("compositionBalance", "Composition Balance*", 0.0f, 1.0f, true, false, 430, true, new string[] { "Maps" }, proceduralMaterialMaps, "MapBiome_Balance", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("compositionContrast", "Composition Contrast*", 0.0f, 1.0f, true, false, 440, true, new string[] { "Maps" }, proceduralMaterialMaps, "MapBiome_Contrast", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1Seed", "Biome 1 Seed*", 0, 255, false, true, 450, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "$randomseed", PropertyFloat.Method.VALUE, PropertyFloat.DataType.INT);
            AddPropertyFloat("biome1Chaos", "Chaos*", 0.0f, 10.0f, true, false, 460, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Biome_Coverage_Warp", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1Balance", "Balance*", 0.0f, 1.0f, true, false, 470, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Biome_Coverage_Balance", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1Contrast", "Contrast*", 0.0f, 1.0f, true, false, 480, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Biome_Coverage_Contrast", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1ColorVariation", "Color Variation*", 0.0f, 1.0f, true, false, 490, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Biome_Hue", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1Saturation", "Saturation*", 0.0f, 1.0f, true, false, 500, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Biome_Saturation", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1Brightness", "Brightness*", 0.3f, 0.7f, true, false, 510, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Biome_Brightness", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1SurfaceBump", "Surface Bump*", 0.0f, 0.3f, true, false, 520, false, new string[] { "Biome1" }, proceduralMaterialBiome1, "Normal_Strength_Surface", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1CratersSmall", "Small Craters*", 0.0f, 1.0f, true, false, 530, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Craters_Small", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1CratersMedium", "Medium Craters*", 0.0f, 1.0f, true, false, 540, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Craters_Medium", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1CratersLarge", "Large Craters*", 0.0f, 1.0f, true, false, 550, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Craters_Large", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1CratersErosion", "Craters Erosion*", 0.0f, 1.0f, true, false, 560, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Crater_Erosion", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1CratersDiffuse", "Craters Diffuse*", 0.0f, 1.0f, true, false, 570, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Diffuse_Strength_Craters", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1CratersBump", "Craters Bump*", 0.0f, 1.0f, true, false, 580, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Normal_Strength_Craters", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1CanyonsDiffuse", "Canyons Diffuse*", 0.0f, 1.0f, true, false, 590, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Diffuse_Strength_Canyons", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome1CanyonsBump", "Canyons Bump*", 0.0f, 0.3f, true, false, 600, true, new string[] { "Biome1" }, proceduralMaterialBiome1, "Normal_Strength_Canyons", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2Seed", "Biome 2 Seed*", 0, 255, false, true, 610, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "$randomseed", PropertyFloat.Method.VALUE, PropertyFloat.DataType.INT);
            AddPropertyFloat("biome2Chaos", "Chaos*", 0.0f, 10.0f, true, false, 620, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Biome_Coverage_Warp", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2Balance", "Balance*", 0.0f, 1.0f, true, false, 630, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Biome_Coverage_Balance", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2Contrast", "Contrast*", 0.0f, 1.0f, true, false, 640, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Biome_Coverage_Contrast", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2ColorVariation", "Color Variation*", 0.0f, 1.0f, true, false, 650, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Biome_Hue", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2Saturation", "Saturation*", 0.0f, 1.0f, true, false, 660, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Biome_Saturation", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2Brightness", "Brightness*", 0.3f, 0.7f, true, false, 670, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Biome_Brightness", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2SurfaceBump", "Surface Bump*", 0.0f, 0.3f, true, false, 680, false, new string[] { "Biome2" }, proceduralMaterialBiome2, "Normal_Strength_Surface", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2CratersSmall", "Small Craters*", 0.0f, 1.0f, true, false, 690, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Craters_Small", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2CratersMedium", "Medium Craters*", 0.0f, 1.0f, true, false, 700, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Craters_Medium", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2CratersLarge", "Large Craters*", 0.0f, 1.0f, true, false, 710, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Craters_Large", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2CratersErosion", "Craters Erosion*", 0.0f, 1.0f, true, false, 720, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Crater_Erosion", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2CratersDiffuse", "Craters Diffuse*", 0.0f, 1.0f, true, false, 730, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Diffuse_Strength_Craters", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2CratersBump", "Craters Bump*", 0.0f, 1.0f, true, false, 740, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Normal_Strength_Craters", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2CanyonsDiffuse", "Canyons Diffuse*", 0.0f, 1.0f, true, false, 750, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Diffuse_Strength_Canyons", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("biome2CanyonsBump", "Canyons Bump*", 0.0f, 0.3f, true, false, 760, true, new string[] { "Biome2" }, proceduralMaterialBiome2, "Normal_Strength_Canyons", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("citiesSeed", "Random Seed", 0, 255, false, true, 954, true, new string[] { "Cities" }, proceduralMaterialCities, "$randomseed", PropertyFloat.Method.VALUE, PropertyFloat.DataType.INT);
            AddPropertyFloat("citiesPopulation", "Population*", 0.0f, 0.25f, true, false, 780, true, new string[] { "Cities" }, proceduralMaterialCities, "Population", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("citiesAdvancement", "Advancement*", 0.0f, 1.0f, true, false, 790, true, new string[] { "Cities" }, proceduralMaterialCities, "Advancement", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("citiesGlow", "Glow*", 0.0f, 1.0f, true, false, 925, true, new string[] { "Cities" }, proceduralMaterialCities, "Glow", PropertyFloat.Method.LERP, PropertyFloat.DataType.FLOAT);
            AddPropertyFloat("citiesTiling", "Tiling", 1, 10, false, true, 800, false, null, material, "_TilingCities", PropertyFloat.Method.VALUE, PropertyFloat.DataType.INT);
            AddPropertyColor("citiesColor", "Night Light Color", new Color(1.0f, 1.0f, 0.95f, 1.0f), 0.05f, 0.05f, 0.05f, 810, true, material, "_ColorCities");

            // Update dictionaries (again, now with all Float and Color properties too) 
            UpdateDictionariesIfNeeded(true);

            // Set default properties based on seed (this time for all properties)
            SetDefaultProperties();

           
            if (initJSONSettings != "")
            {
                // If initJSON string is set, configure planet according to the init string
                ImportFromJSON(initJSONSettings);
                initJSONSettings = "";
            }
            else
            {
                // Load planet settings from cache (if this is not a new planet) - this overwrites default settings if changes have been made
                if (serializedPlanetCache != null)
                    if (serializedPlanetCache.Length > 0)
                        ImportFromJSON(serializedPlanetCache, false);
            }

            // Update lookup textures (e.g. create lookup textures for liquid level so shader knows where on hight map to apply water)
            UpdateLookupTextures();

            // Update shader for planet lighting
            UpdateShaderLocalStar(true);

            // Force rebuild of planet textures to use all the correct properties for the planet
            RebuildTextures(true);

            updateShaderNeeded = true;
        }

        /// <summary>
        /// Sets the material textures. This public method is called by the Manager BuildTextureRoutine(). When a procedural texture is created
        /// by BuildTextureRoutine() it will be copied to a Texture2D which is then assigned to the planet using this method.
        /// </summary>
        /// <param name="_textureName"></param>
        /// <param name="_textureDiffuse"></param>
        /// <param name="_textureNormal"></param>
        /// <param name="_textureFlow"></param>
        public override void SetTexture(string _textureName, Texture2D _textureDiffuse, Texture2D _textureNormal, Texture2D _textureFlow)
        {
            if (Manager.Instance.DEBUG_LEVEL > 0) Debug.Log("PlanetSolid.cs: SetTexture(" + _textureName + "," + _textureDiffuse + "," + _textureNormal + "," + _textureFlow + ")");

            switch (_textureName)
            {
                case "Maps":
                    _textureMaps = _textureDiffuse;
                    material.SetTexture("_TexMaps", _textureMaps);
                    break;
                case "Biome1":
                    _textureBiome1DiffSpec = _textureDiffuse;
                    _textureBiome1Normal = _textureNormal;
                    material.SetTexture("_TexBiome1DiffSpec", _textureBiome1DiffSpec);
                    material.SetTexture("_TexBiome1Normal", _textureBiome1Normal);
                    break;
                case "Biome2":
                    _textureBiome2DiffSpec = _textureDiffuse;
                    _textureBiome2Normal = _textureNormal;
                    material.SetTexture("_TexBiome2DiffSpec", _textureBiome2DiffSpec);
                    material.SetTexture("_TexBiome2Normal", _textureBiome2Normal);
                    break;
                case "Lava":
                    _textureLavaDiffuse = _textureDiffuse;
                    _textureLavaFlow = _textureFlow;
                    material.SetTexture("_TexLavaDiffuse", _textureLavaDiffuse);
                    material.SetTexture("_TexLavaFlow", _textureLavaFlow);
                    break;
                case "Cities":
                    _textureCities = _textureDiffuse;
                    material.SetTexture("_TexCities", _textureCities);
                    break;
                case "Clouds":
                    _textureClouds = _textureDiffuse;
                    material.SetTexture("_TexClouds", _textureClouds);
                    break;
                case "PolarIce":
                    _textureIceDiffuse = _textureDiffuse;
                    material.SetTexture("_TexIceDiffuse", _textureIceDiffuse);
                    break;
            }
        }

        /// <summary>
        /// Updates local star position and checks if any textures need to be rebuilt. This happens every frame.
        /// </summary>
        void Update()
        {
            // Update local star position
            UpdateShaderLocalStar(false);

            // Rebuild any planet textures where procedural properties have been changed. This only executes if autoUpdateTextures is set in ProceduralPlanetManager.
            if (RebuildTexturesNeeded() && Manager.Instance.autoUpdateTextures && !IsRegeneratingTextures())
                RebuildTextures();

            // Update shader properties if any shader related properties have been changed
            if (updateShaderNeeded) UpdateShader();

            if (_isRebuildingTextures && !IsRegeneratingTextures())
            {
                if (!Manager.Instance.HasTexturesInQueue(gameObject))
                {
                    SendMessageOnTextureBuildComplete();
                    _isRebuildingTextures = false;
                    _timerStartBuildingTextures = 0f;
                }
            }
        }

        /// <summary>
        /// Determines if any procedural texture needs to be rebuilt based on flags. 
        /// Public because the editor script calls this as well.
        /// </summary>
        /// <returns></returns>
        public bool RebuildTexturesNeeded()
        {
            if (rebuildBiome1Needed || rebuildBiome2Needed || rebuildCitiesNeeded || rebuildCloudsNeeded || rebuildMapsNeeded || rebuildLavaNeeded || rebuildLookupsNeeded) return true;
            return false;
        }

        /// <summary>
        /// Rebuilds textures for procedural materials where properties have been changed (or alternatively all texture if force parameter is set to true).
        /// Public because the editor script calls this as well.
        /// </summary>
        /// <param name="_force"></param>
        public void RebuildTextures(bool _force = false)
        {
            if (Manager.Instance.DEBUG_LEVEL > 0) Debug.Log("PlanetSolid.cs: RebuildTextures(" + _force + ")");

            // Ensure that the dictionaries are updated (force flag is set to true for assurance)
            UpdateDictionariesIfNeeded(true);

            // If force flag was set, update all procedural textures for the planet.
            if (_force)
            {
                UpdateProceduralTexture("All");
                UpdateLookupTextures();
                rebuildMapsNeeded = false;
                rebuildBiome1Needed = false;
                rebuildBiome2Needed = false;
                rebuildCitiesNeeded = false;
                rebuildCloudsNeeded = false;
                rebuildLavaNeeded = false;
                rebuildLookupsNeeded = false;
                rebuildLookupsNeeded = false;
            }

            // Update individual procedural textures if needed
            if (rebuildMapsNeeded)
            {
                UpdateProceduralTexture("Maps");
                rebuildMapsNeeded = false;
            }
            if (rebuildBiome1Needed)
            {
                UpdateProceduralTexture("Biome1");
                rebuildBiome1Needed = false;
            }
            if (rebuildBiome2Needed)
            {
                UpdateProceduralTexture("Biome2");
                rebuildBiome2Needed = false;
            }
            if (rebuildCitiesNeeded)
            {
                UpdateProceduralTexture("Cities");
                rebuildCitiesNeeded = false;
            }
            if (rebuildCloudsNeeded)
            {
                UpdateProceduralTexture("Clouds");
                rebuildCloudsNeeded = false;
            }
            if (rebuildLavaNeeded)
            {
                UpdateProceduralTexture("Lava");
                rebuildLavaNeeded = false;
            }

            if (rebuildLookupsNeeded)
            {
                UpdateLookupTextures();
                rebuildLookupsNeeded = false;
            }
        }

        /// <summary>
        /// Updates procedural textures by calling BuildTexture method in Manager and submitting which texture to be built. 
        /// Array of float values for the texture is included as well as reference to the (this) object that the texture should be sent to.
        /// Public because Manager calls this when texture quality is changed.
        /// </summary>
        /// <param name="_textureName"></param>
        public override void UpdateProceduralTexture(string _textureName)
        {
            if (Manager.Instance.DEBUG_LEVEL > 0) Debug.Log("PlanetSolid.cs: UpdateProceduralTexture(" + _textureName + ")");

            if (_textureName == "All" || _textureName == "Maps")
            {
                // Build an array of PropertyFloat that affect Maps
                proceduralMaterialMaps = _dictionaryMaterials["composition"].GetPropertyMaterial();
                // Request ProceduralPlanetManager to build the new texture
                Manager.Instance.BuildTexture(proceduralMaterialMaps, "Maps", false, true, AssembleFloatArray("Maps"), this);
            }

            if (_textureName == "All" || _textureName == "Biome1")
            {
                proceduralMaterialBiome1 = _dictionaryMaterials["biome1Type"].GetPropertyMaterial();
                Manager.Instance.BuildTexture(proceduralMaterialBiome1, "Biome1", true, true, AssembleFloatArray("Biome1"), this);

            }
            if (_textureName == "All" || _textureName == "Biome2")
            {
                proceduralMaterialBiome2 = _dictionaryMaterials["biome2Type"].GetPropertyMaterial();
                Manager.Instance.BuildTexture(proceduralMaterialBiome2, "Biome2", true, true, AssembleFloatArray("Biome2"), this);
            }
            if (_textureName == "All" || _textureName == "Cities")
            {
                proceduralMaterialCities = _dictionaryMaterials["cities"].GetPropertyMaterial();
                Manager.Instance.BuildTexture(proceduralMaterialCities, "Cities", true, true, AssembleFloatArray("Cities"), this);

            }
            if (_textureName == "All" || _textureName == "Lava")
            {
                proceduralMaterialLava = _dictionaryMaterials["lava"].GetPropertyMaterial();
                Manager.Instance.BuildTexture(proceduralMaterialLava, "Lava", true, true, AssembleFloatArray("Lava"), this);
            }
            if (_textureName == "All" || _textureName == "Clouds")
            {
                proceduralMaterialClouds = _dictionaryMaterials["clouds"].GetPropertyMaterial();
                Manager.Instance.BuildTexture(proceduralMaterialClouds, "Clouds", true, false, AssembleFloatArray("Clouds"), this);
            }
            if (_textureName == "All" || _textureName == "PolarIce")
            {
                proceduralMaterialPolarIce = _dictionaryMaterials["polarIce"].GetPropertyMaterial();
                Manager.Instance.BuildTexture(proceduralMaterialPolarIce, "PolarIce", true, true, AssembleFloatArray("PolarIce"), this);
            }

            // Start timer when rebuild of textures started
            _timerStartBuildingTextures = Time.time;

            // Set flag that rebuild of textures has started
            _isRebuildingTextures = true;
        }

        /// <summary>
        /// Updates the lookup texture that shader uses to determine waterlevel, polar cap transition, lava coverage, and lava glow.
        /// </summary>
        void UpdateLookupTextures()
        {
            if (Manager.Instance.DEBUG_LEVEL > 0) Debug.Log("PlanetSolid.cs: UpdateLookupTextures()");

            _textureLookupLiquid = GenerateLookupSmoothTexture(_dictionaryFloats["liquidLevel"].GetPropertyLerp(), 0f, _dictionaryFloats["liquidShallow"].GetPropertyLerp());
            material.SetTexture("_TexLookupLiquid", _textureLookupLiquid);
            _textureLookupPolar = GenerateLookupTexture(_dictionaryFloats["polarCapAmount"].GetPropertyLerp());
            material.SetTexture("_TexLookupPolar", _textureLookupPolar);
            _textureLookupLava = GenerateLookupTexture(_dictionaryFloats["lavaAmount"].GetPropertyLerp());
            material.SetTexture("_TexLookupLava", _textureLookupLava);
            _textureLookupLavaGlow = GenerateLookupSmoothTexture(_dictionaryFloats["lavaAmount"].GetPropertyLerp(), 0f, _dictionaryFloats["lavaGlowAmount"].GetPropertyLerp());
            material.SetTexture("_TexLookupLavaGlow", _textureLookupLavaGlow);
        }

        /// <summary>
        /// Updates the shader properties for planet and atmosphere materials.
        /// </summary>
        void UpdateShader()
        {
            if (Manager.Instance.DEBUG_LEVEL > 1) Debug.Log("PlanetSolid.cs: UpdateShader()");

            // Update the dictionaries if needed
            UpdateDictionariesIfNeeded();

            // Update atmosphere shader properties
            if (_externalAtmosphere != null)
            {
                externalAtmosphereMaterial.SetColor("_AtmoColor", _dictionaryColors["atmosphereColor"].color);
                externalAtmosphereMaterial.SetFloat("_Size", _dictionaryFloats["atmosphereExternalSize"].GetPropertyLerp());
                float _aedMin = _dictionaryFloats["atmosphereExternalDensity"].minValue;
                float _aedMax = _dictionaryFloats["atmosphereExternalDensity"].maxValue;
                float _aedVal = _dictionaryFloats["atmosphereExternalDensity"].value;
                float _aesVal = _dictionaryFloats["atmosphereExternalSize"].value;
                externalAtmosphereMaterial.SetFloat("_Falloff", Mathf.Lerp(_aedMin * (1 + _aesVal), _aedMax * (1 + _aesVal), _aedVal));
            }

            // Update planet shader properties
            foreach (KeyValuePair<string, PropertyFloat> _pmm in _dictionaryFloats)
            {
                bool _isNormalShader = false;
                if (_pmm.Value.proceduralTextures != null)
                {
                    if (_pmm.Value.proceduralTextures.Length == 0)
                        _isNormalShader = true;
                }
                if (_pmm.Value.shaderProperty != null && _pmm.Value.materials != null && _isNormalShader) UpdatePropertyFloatShader(_pmm.Value);
            }
            foreach (KeyValuePair<string, PropertyColor> _pmm in _dictionaryColors)
            {
                if (_pmm.Value.shaderProperty != null && _pmm.Value.materials != null) UpdatePropertyColorShader(_pmm.Value);
            }

            // Create a list of keywords to be sent to the shader for enabling/disabling features for performance reasons
            List<string> _shaderKeywords = new List<string>();

            // Enable/disable code to render clouds in shader (shader section only exists if cloud opacity is not zero)
            if (_dictionaryFloats["cloudsOpacity"].GetPropertyLerp() > 0.00001f)
                _shaderKeywords.Add("CLOUDS_ON");
            else
                _shaderKeywords.Add("CLOUDS_OFF");

            // Enable/disable code to render lava in shader (shader section only exists if there is lava on the planet)
            if (_dictionaryFloats["lavaAmount"].GetPropertyLerp() > 0.00001f)
                _shaderKeywords.Add("LAVA_ON");
            else
                _shaderKeywords.Add("LAVA_OFF");

            // Set the shader keywords in the shader
            material.shaderKeywords = _shaderKeywords.ToArray();

            // Clear shader update needed flag
            updateShaderNeeded = false;
        }

        /// <summary>
        /// Sets the planet blueprint based on the index value (order of the GameObject of under the Manager gameobject. 
        /// Defaults to seed selected index. Optionally leave overridden values as is.
        /// Public because Manager calls this when creating a new planet.
        /// </summary>
        /// <param name="_index"></param>
        /// <param name="_leaveOverride"></param>        
        public override void SetPlanetBlueprint(int _index = -1, bool _leaveOverride = false, bool _setProperties = true)
        {
            if (Manager.Instance.DEBUG_LEVEL > 0) Debug.Log("PlanetSolid.cs: SetPlanetBlueprint(" + _index + "," + _leaveOverride + "," + _setProperties + ")");

            if (_index == -1)
            {
                // If index is set to -1 (default) the random seed will determine planet blueprint.
                planetBlueprintIndex = GetPlanetBlueprintSeededIndex();
                // Set override flag to false since random seed is determining planet blueprint.
                planetBlueprintOverride = false;
            }
            else
            {
                // Set planet blueprint to a specific index value.
                planetBlueprintIndex = _index;
                // Set override flag to true since we are overriding the planet blueprint.
                planetBlueprintOverride = true;
            }

            blueprint = Manager.Instance.GetPlanetBlueprintByIndex(planetBlueprintIndex, this);

            if (_setProperties)
            {
                // Set the default properties for the planet (and forward the override flag)
                SetDefaultProperties(_leaveOverride);

                // Rebuild textures (force rebuild of all textures)
                RebuildTextures(true);

                // Set flag to ensure shader is updated
                updateShaderNeeded = true;
            }

            serializedPlanetCache = ExportToJSON(StringFormat.JSON_COMPACT);
        }
        

        /// <summary>
        /// Gets the planet blueprint index (i.e. the order of a planet blueprint in the blueprint hierarchy of solid planets under the Manager game object
        /// based on the random seed.
        /// </summary>
        /// <returns></returns>
        protected override int GetPlanetBlueprintSeededIndex()
        {
            if (Manager.Instance.DEBUG_LEVEL > 0) Debug.Log("PlanetSolid.cs: GetPlanetBlueprintSeededIndex()");

            // Save the current random state
            Random.State _oldState = Random.state;
            // Initialize the random state with the planetSeed value
            Random.InitState(planetSeed);
            // Get the random planet blueprint index (based on the seed) from the Manager
            planetBlueprintIndex = Random.Range(0, Manager.Instance.listSolidPlanetBlueprints.Count);
            // Restore the previous random state
            Random.state = _oldState;
            // Return the new planet blueprint index.
            return planetBlueprintIndex;
        }

        /// <summary>
        /// Handles any modified textures by examining the string array to see if the array contains reference to one or more procedural texture(s).
        /// If there is a reference to a texture it means that it has been modified and the rebuild flag for the texture is set to true.
        /// </summary>
        /// <param name="_proceduralTextures"></param>
        protected override void HandleModifiedTextures(string[] _proceduralTextures)
        {
            if (Manager.Instance.DEBUG_LEVEL > 1) Debug.Log("PlanetSolid.cs: HandleModifiedTextures(" + _proceduralTextures + ")");

            foreach (string _s in _proceduralTextures)
            {
                switch (_s)
                {
                    case "Maps":
                        rebuildMapsNeeded = true;
                        break;
                    case "Cities":
                        rebuildCitiesNeeded = true;
                        break;
                    case "Clouds":
                        rebuildCloudsNeeded = true;
                        break;
                    case "Lava":
                        rebuildLavaNeeded = true;
                        break;
                    case "Biome1":
                        rebuildBiome1Needed = true;
                        break;
                    case "Biome2":
                        rebuildBiome2Needed = true;
                        break;
                    case "Lookups":
                        rebuildLookupsNeeded = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Answers the question if the planet is currently generating any procedural textures.
        /// Public so any script can query if planet is currently processing textures. Manager calls this when handling the texture building queue.
        /// </summary>
        /// <returns></returns>
        public override bool IsRegeneratingTextures()
        {
            if (Manager.Instance.DEBUG_LEVEL > 1) Debug.Log("PlanetSolid.cs: IsRegeneratingTextures()");

            if (proceduralMaterialBiome1 != null)
                if (proceduralMaterialBiome1.isProcessing) return true;
            if (proceduralMaterialBiome2 != null)
                if (proceduralMaterialBiome2.isProcessing) return true;
            if (proceduralMaterialCities != null)
                if (proceduralMaterialCities.isProcessing) return true;
            if (proceduralMaterialClouds != null)
                if (proceduralMaterialClouds.isProcessing) return true;
            if (proceduralMaterialPolarIce != null)
                if (proceduralMaterialPolarIce.isProcessing) return true;
            if (proceduralMaterialLava != null)
                if (proceduralMaterialLava.isProcessing) return true;
            if (proceduralMaterialMaps != null)
                if (proceduralMaterialMaps.isProcessing) return true;
            return false;
        }

        /// <summary>
        /// Updates the shader to take into account the properties of a local star in the scene (e.g. position, intensity, color of star).
        /// Used for lighting and shadows.
        /// </summary>
        /// <param name="_forceUpdate"></param>
        void UpdateShaderLocalStar(bool _forceUpdate)
        {
            // Ensure that a star shader setting cache exists, otherwise create one
            if (_localStarShaderCacheSettings == null)
                _localStarShaderCacheSettings = new LocalStar.ShaderCacheSettings();

            // If there is no nearest star instance...
            if (_localStarNearestInstance == null)
            {
                // Find and iterate through the local star objects in the scene
                foreach (LocalStar _ls in FindObjectsOfType<LocalStar>())
                {
                    // If there is no instance, set the first hit to be the nearest star
                    if (_localStarNearestInstance == null) _localStarNearestInstance = _ls;

                    // For subsequent local star, find the nearest one
                    if (Vector3.Distance(_localStarNearestInstance.transform.position, transform.position) < Vector3.Distance(_localStarNearestInstance.transform.position, transform.position))
                        _localStarNearestInstance = _ls;
                }
            }

            // If there are no local stars in the scene, return
            if (_localStarNearestInstance == null) return;

            // Detect if if local star position is different from the cache - if so, update the shader with new settings and update the cache
            if (Vector3.Distance(_localStarShaderCacheSettings.position, _localStarNearestInstance.transform.position) > 0.0001f || _forceUpdate)
            {
                _localStarShaderCacheSettings.position = _localStarNearestInstance.transform.position;
                _meshRenderer.sharedMaterial.SetVector(_shaderID_LocalStarPosition, _localStarNearestInstance.transform.position);
                _externalAtmosphereRenderer.sharedMaterial.SetVector(_shaderID_LocalStarPosition, _localStarNearestInstance.transform.position);

            }

            // Detect if if local star color is different from the cache - if so, update the shader with new settings and update the cache
            if (Mathf.Abs(_localStarShaderCacheSettings.color.r - _localStarNearestInstance.color.r) > 0.0001f ||
                Mathf.Abs(_localStarShaderCacheSettings.color.g - _localStarNearestInstance.color.g) > 0.0001f ||
                Mathf.Abs(_localStarShaderCacheSettings.color.b - _localStarNearestInstance.color.b) > 0.0001f ||
                _forceUpdate)
            {
                _localStarShaderCacheSettings.color = _localStarNearestInstance.color;
                _meshRenderer.sharedMaterial.SetColor(_shaderID_LocalStarColor, _localStarNearestInstance.color);
                _externalAtmosphereRenderer.sharedMaterial.SetColor(_shaderID_LocalStarColor, _localStarNearestInstance.color);
            }

            // Detect if if local star intensity is different from the cache - if so, update the shader with new settings and update the cache
            if (Mathf.Abs(_localStarShaderCacheSettings.intensity - _localStarNearestInstance.intensity) > 0.0001f || _forceUpdate)
            {
                _localStarShaderCacheSettings.intensity = _localStarNearestInstance.intensity;
                _meshRenderer.sharedMaterial.SetFloat(_shaderID_LocalStarIntensity, _localStarNearestInstance.intensity);
                _externalAtmosphereRenderer.sharedMaterial.SetFloat(_shaderID_LocalStarIntensity, _localStarNearestInstance.intensity);
            }

            // Detect if if local star ambient intensity is different from the cache - if so, update the shader with new settings and update the cache
            if (Mathf.Abs(_localStarShaderCacheSettings.ambientIntensity - _localStarNearestInstance.ambientIntensity) > 0.0001f || _forceUpdate)
            {
                _localStarShaderCacheSettings.ambientIntensity = _localStarNearestInstance.ambientIntensity;
                _meshRenderer.sharedMaterial.SetFloat(_shaderID_LocalStarAmbientIntensity, _localStarNearestInstance.ambientIntensity);
            }

        }

    }
}
