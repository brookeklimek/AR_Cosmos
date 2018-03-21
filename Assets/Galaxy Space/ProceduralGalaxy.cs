using UnityEngine;
using UnityEngine.UI;

public class ProceduralGalaxy : MonoBehaviour {

    #region Random Settings
    [Header("Random Settings")]

    [Tooltip("If it's zero, it will turn off random state")]
    public int Seed;
    #endregion

    #region Galaxy Settings
    [Header("Galaxy Settings")]

    public Texture2D GalaxyMap;

    public int GalaxyResolution;

    [Tooltip("If it's null, it will generate new one")]
    public string GalaxyName;

    [Tooltip("Stars brigtness for background generator")]
    public AnimationCurve Brightness;

    public Gradient GalaxyColor;
    #endregion

    #region Galaxy Arms Settings
    [Header("Galaxy Arms Settings")]

    public int ArmCount;

    public float ArmMaxOffset;

    private float ArmSeperationAngle;

    public float RotationFactor;

    public float RandomOffsetXY;

    public int ArmStarsCount;

    public AnimationCurve ArmBrightness;
    #endregion

    #region Particle System Settings

    public float ParticleSizeMultiplier = 9f;

    private ParticleSystem P_System;
    private ParticleSystem.Particle[] Particles;

    #endregion

    #region Output Source Settings
    [Header("Output Source Settings")]

    [Tooltip("Will be used for a 2D Map output")]
    public RawImage OutputImage;

    [Tooltip("Will be used for a Galaxy Name output")]
    public Text OutputName;

    [Tooltip("%n will be replaced with Galaxy Name")]
    public string OutputNameConstruction;

    private int EndStarsCount;
    #endregion



    private void OnEnable()
    {

        UpdateGalaxy();

    }

    private void Start () {

        GenerateGalaxyMap();

        GenerateVisualGalaxy();

        if (OutputImage)
            OutputImage.texture = GalaxyMap;

        if (GalaxyName == "")
            GenerateGalaxyName();

	}
	
    private void GenerateGalaxyName ()
    {

        GalaxyName = ProceduralGalaxyCaller.GenerateName(Random.Range(2, 10));

        if (OutputName)
        {

            OutputName.text = OutputNameConstruction.Replace("%n", GalaxyName);

        }

    }

    public void UpdateGalaxy ()
    {

        if (GalaxyMap)
        {
            GenerateVisualGalaxy();

            if (OutputImage)
            {
                OutputImage.texture = GalaxyMap;
            }

        }

        if (OutputName)
        {

            OutputName.text = OutputNameConstruction.Replace("%n", GalaxyName);

        }

    }

	public void GenerateGalaxyMap()
    {

        if (Seed != 0)
        {
            Random.InitState(Seed);
        }

        int _HalfResolution = GalaxyResolution / 2;

        GalaxyMap = new Texture2D(GalaxyResolution, GalaxyResolution);

        //Background Generator

        for (int Y = 0; Y < GalaxyResolution; Y++)
        {
            for (int X = 0; X < GalaxyResolution; X++)
            {


                    float Distance = (Vector2.Distance(new Vector2(X, Y), new Vector2(_HalfResolution, _HalfResolution))) / _HalfResolution;

                    float Noise = Random.Range(-0.1f, 0.1f);

                    float Density = Brightness.Evaluate(Distance) + Noise;

                    if (Density < 0.1f) { Density = 0; }

                    Color ThisColorWA = GalaxyColor.Evaluate(Distance);
                    Color ThisColor =  new Color(ThisColorWA.r, ThisColorWA.g, ThisColorWA.b, Density);

                    GalaxyMap.SetPixel(X, Y, ThisColor);

            }

        }



        //Arm Generator

        ArmSeperationAngle = 2 * Mathf.PI / ArmCount;

        for (int i = 0; i < ArmStarsCount; i++)
        {

            float RandomDistance = Random.value;
            float RandomAngle = Random.value * 2 * Mathf.PI;

            float ArmOffset = GaussianRandom.Value * ArmMaxOffset;

            ArmOffset /= 2;
            ArmOffset *= (1 / RandomDistance);

            float SquaredArmOffset = Mathf.Pow(ArmOffset, 2);
            if (ArmOffset < 0)
                SquaredArmOffset = SquaredArmOffset * -1;
            ArmOffset = SquaredArmOffset;

            float Rotation = RandomDistance * RotationFactor;

            RandomAngle = ((int)(RandomAngle / ArmSeperationAngle)) * ArmSeperationAngle + ArmOffset + Rotation;

            float SectorX = Mathf.Cos(RandomAngle) * RandomDistance;
            float SectorY = Mathf.Sin(RandomAngle) * RandomDistance;
            float RandomOffsetX = Random.value * RandomOffsetXY;
            float RandomOffsetY = Random.value * RandomOffsetXY;

            SectorX += RandomOffsetX;
            SectorY += RandomOffsetY;

            int PixelX = Mathf.Clamp((int)(((SectorX + 1f) / 2f) * GalaxyResolution), 0, this.GalaxyResolution - 1);
            int PixelY = Mathf.Clamp((int)(((SectorY + 1f) / 2f) * GalaxyResolution), 0, this.GalaxyResolution - 1);

            Color NewColor = GalaxyMap.GetPixel(PixelX, PixelY);
            NewColor = new Color(NewColor.r, NewColor.g, NewColor.b, ArmBrightness.Evaluate(RandomDistance));

            GalaxyMap.SetPixel(PixelX, PixelY, NewColor);
        }

        GalaxyMap.filterMode = FilterMode.Point;

        GalaxyMap.Apply();

        CalculateEndStarsCount();

    }

    private void CalculateEndStarsCount()
    {

        for (int Y = 0; Y < GalaxyResolution; Y++)
        {
            for (int X = 0; X < GalaxyResolution; X++)
            {

                if (GalaxyMap.GetPixel(X,Y).a > 0)
                {

                    EndStarsCount++;

                }

            }
        }

    }

    private void GenerateVisualGalaxy()
    {

        Initialize();

        P_System.GetParticles(Particles);

        int PixelNumber = 0;

        for (int Y = 0; Y < GalaxyResolution; Y++)
        {
            for (int X = 0; X < GalaxyResolution; X++)
            {

                if (GalaxyMap.GetPixel(X, Y).a <= 0) { continue; }

                Particles[PixelNumber].position = new Vector3(X * 2 + Random.Range(-2, 2), Random.Range(-2, 2), Y * 2 + Random.Range(-2, 2));

                Particles[PixelNumber].startColor = GalaxyMap.GetPixel(X, Y);

                Particles[PixelNumber].startSize = 3 + GalaxyMap.GetPixel(X, Y).a * ParticleSizeMultiplier;

                PixelNumber++;

            }
        }

        P_System.SetParticles(Particles, Particles.Length);

    }

    private void Initialize()
    {

        if (P_System == null)
            P_System = GetComponent<ParticleSystem>();

        var Main = P_System.main;
        Main.maxParticles = EndStarsCount;

        if (Particles == null || Particles.Length < P_System.main.maxParticles)
            Particles = new ParticleSystem.Particle[P_System.main.maxParticles];
        P_System.Stop();
    }
}
