/*  
    Class: CreatePlanetFromScript
    Version: 0.1.1 (alpha release)
    Date: 2018-01-10
    Author: Stefan Persson
    (C) Imphenzia AB

    This script demonstrates:
    * Simple random planet creation from script
    * Planet creation from a Base64 encoded JSON string from script
    * UI slider to override one "liquidLevel" shader property (fast) and one "cloudsCoverage" procedural texture property (slow)
    * Subscribing to OnTextureBuildComplete messages
    * Changing texture resolution
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// If you don't use this directive you have to use the fully qualified name for each method call, e.g. ProceduralPlanets.Manager.Instance.CreatePlanet() instead of Manager.Instance.CreatePlanet();
using ProceduralPlanets;

public class CreatePlanetFromScript : MonoBehaviour {
    // UI elements
    public Slider uiSliderLiquidLevel;
    public Slider uiSliderCloudsCoverage;
    public Dropdown uiDropdownResolution;
    public Text uiTextMessage;

    // Camera parent gameObject, used for easy orbit
    public Transform cameraParent;

    // Private variables
    private Planet _planet;
    private float _dragSpeed = 0.1f;
    private float _mouseSpeed = 10f;
    private bool _ignoreOverrideFlag = false;

    /// <summary>
    /// The Start() method is a default monobehavior method that runs every time a game/scene is started.
    /// </summary>
    void Start()
    {
        // Create a planet from predefined Base64 JSON string
        CreatePlanetUsingBase64EncodedJSON();

        // Add listeners and delegate methods to sliders and dropdowns
        uiSliderLiquidLevel.onValueChanged.AddListener(delegate { OverrideLiquidLevel(); });
        uiSliderCloudsCoverage.onValueChanged.AddListener(delegate { OverrideCloudsCoverage(); });
        uiDropdownResolution.onValueChanged.AddListener(delegate { SetTextureResolution(); });

        // The UI buttons to create planets are configured in the UI component inspector where it calls methods in this script on the OnClicked evenets.

        // Clear the UI text message.
        uiTextMessage.text = "";
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {

        // If the pointer (or finger) is over any UI object, return without executing the code below
        if (IsPointerOverUIObject()) return;

        // COMPUTERS (or devices without touch support) - Camera Rotate and Zoom
        if (!Input.touchSupported)
        {
            // If left mouse button is held down...
            if (Input.GetMouseButton(0))
            {
                // The main camera has been paranted to a gameobject that is in the center of a scene to make it easy to orbit by rotating the parent gameobject.
                cameraParent.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * _mouseSpeed, 0));

                // Move the camera closer/away based on Mouse Y position
                Camera.main.transform.localPosition = new Vector3(0, 5, Camera.main.transform.localPosition.z + Input.GetAxis("Mouse Y") * _mouseSpeed * 0.01f);
            }
        }

        // TOUCH DEVICES - Camera Rotate and Zoom
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            // Get movement of the finger since last frame
            Vector2 _touchDeltaPosition = Input.GetTouch(0).deltaPosition;

            // The main camera has been paranted to a gameobject that is in the center of a scene to make it easy to orbit by rotating the parent gameobject.
            // Rotation is performed based on touch device dragging left/right
            cameraParent.Rotate(new Vector3(0, _touchDeltaPosition.x * _dragSpeed, 0));

            // Move the camera closer/away based on touch device dragging up/down
            Camera.main.transform.localPosition = new Vector3(0, 5, Camera.main.transform.localPosition.z + Input.GetAxis("Mouse Y") * _dragSpeed);
        }
    }

    /// <summary>
    /// Creates a simple random planet.
    /// </summary>
    public void CreateRandomPlanet()
    {
        // If a planet already exists in the scene, destroy it
        if (_planet != null) DestroyImmediate(_planet.gameObject);

        // Use Manager.Instance.CreatePlanet method to create a new planet - when only a position is included as an argument it will create a totally random planet
        // and select a planet blueprint based on the blueprint's probability value.
        _planet = Manager.Instance.CreatePlanet(Vector3.zero);

        // Add this gameobject to the message subscription list. The planet will call OnTextuireBuildComplete(float _time) 
        // of this gameobject every time it finishes a texture rebuild job.
        _planet.SubscribeMessageOnTextureBuildComplete(gameObject);

        // Set the UI text message to indicate that textures are being rebuilt.
        uiTextMessage.text = "Rebuilding textures...";
    }

    /// <summary>
    /// Creates a planet based on a previously exported Base64 encoded JSON string (exported via the inspector of a planet in the Unity editor).
    /// </summary>
    public void CreatePlanetUsingBase64EncodedJSON()
    {
        // If a planet already exists in the scene, destroy it
        if (_planet != null) DestroyImmediate(_planet.gameObject);

        // Use Manager.Instance.CreatePlanet method to create a new planet and include the Base64 JSON string as a second argument to force all properties.
        _planet = Manager.Instance.CreatePlanet(Vector3.zero, "eyJjYXRlZ29yeSI6InBsYW5ldCIsInR5cGUiOiJTb2xpZFBsYW5ldCIsInZlcnNpb24iOiIwLjEuMCIsInBsYW5ldCI6eyJwbGFuZXRTZWVkIjo3MDcwNjQ3NDMsInZhcmlhdGlvblNlZWQiOjU4NzQsImJsdWVwcmludEluZGV4IjozLCJibHVlcHJpbnROYW1lIjoiSWNlIiwicHJvcGVydHlGbG9hdHMiOnsiYWxpZW5pemF0aW9uIjowLCJjb250aW5lbnRTZWVkIjoyMTcsImNvbnRpbmVudFNpemUiOjAuNDY1MzE4NiwiY29udGluZW50Q29tcGxleGl0eSI6MC4xNjA2MDE5LCJjb2FzdGFsRGV0YWlsIjowLjYxNjIyMzUsImNvYXN0YWxSZWFjaCI6MC42MzUwNzIxLCJsaXF1aWRMZXZlbCI6MC4zODg5NzcsImxpcXVpZE9wYWNpdHkiOjAuMjE0MDk4MywibGlxdWlkU2hhbGxvdyI6MC4xNDg4NjY5LCJsaXF1aWRTcGVjdWxhclBvd2VyIjowLjExMTM0NDUsInBvbGFyQ2FwQW1vdW50IjowLjI0OTIyOTcsImF0bW9zcGhlcmVFeHRlcm5hbFNpemUiOjAuNjk2NjQ0MiwiYXRtb3NwaGVyZUV4dGVybmFsRGVuc2l0eSI6MC42OTY2NDQyLCJhdG1vc3BoZXJlSW50ZXJuYWxEZW5zaXR5IjowLjgzMTk5MjcsImNsb3Vkc09wYWNpdHkiOjAuMDc5MTI4MzgsImNsb3Vkc1NlZWQiOjE0NCwiY2xvdWRzQ292ZXJhZ2UiOjAuNDIwNjU3MSwiY2xvdWRzTGF5ZXIxIjowLjczMzA0MDIsImNsb3Vkc0xheWVyMiI6MC44OTE0OTc2LCJjbG91ZHNMYXllcjMiOjAuMDU3MjAzNDEsImNsb3Vkc1NoYXJwbmVzcyI6MC44ODc2NzgsImNsb3Vkc1JvdWdobmVzcyI6MC43OTE3MzgyLCJjbG91ZHNUaWxpbmciOjYsImNsb3Vkc1NwZWVkIjowLjYyMzY1MzYsImNsb3Vkc0hlaWdodCI6MC4yNzY4OTIyLCJjbG91ZHNTaGFkb3ciOjAuMTU0ODkwMywibGF2YUFtb3VudCI6MCwibGF2YUNvbXBsZXhpdHkiOjAsImxhdmFGcmVxdWVuY3kiOjAuMzI1OTMyMSwibGF2YURldGFpbCI6MC41MzUyMzI3LCJsYXZhUmVhY2giOjAuNjM5NzkyLCJsYXZhQ29sb3JWYXJpYXRpb24iOjAuODU5NTUwMSwibGF2YUZsb3dTcGVlZCI6MC4xNzM4NjI3LCJsYXZhR2xvd0Ftb3VudCI6MC40MzUwODUyLCJzdXJmYWNlVGlsaW5nIjoxMSwic3VyZmFjZVJvdWdobmVzcyI6MC41OTIyNzg3LCJjb21wb3NpdGlvblNlZWQiOjIxNywiY29tcG9zaXRpb25UaWxpbmciOjQsImNvbXBvc2l0aW9uQ2hhb3MiOjAuNjg2NzY0MiwiY29tcG9zaXRpb25CYWxhbmNlIjowLjQ2MzI0NDMsImNvbXBvc2l0aW9uQ29udHJhc3QiOjAuMzY1NzgyMywiYmlvbWUxU2VlZCI6NTgsImJpb21lMUNoYW9zIjowLjAwNDE5NTMzMywiYmlvbWUxQmFsYW5jZSI6MC45MjE1MDY5LCJiaW9tZTFDb250cmFzdCI6MC43MDUzOTk5LCJiaW9tZTFDb2xvclZhcmlhdGlvbiI6MC40NjE4NTcyLCJiaW9tZTFTYXR1cmF0aW9uIjowLjIyODI3OTgsImJpb21lMUJyaWdodG5lc3MiOjAuMTE5NzY3MywiYmlvbWUxU3VyZmFjZUJ1bXAiOjAuNzY2NDgyOSwiYmlvbWUxQ3JhdGVyc1NtYWxsIjowLjk3OTg4MDIsImJpb21lMUNyYXRlcnNNZWRpdW0iOjAuODY0ODI2OCwiYmlvbWUxQ3JhdGVyc0xhcmdlIjowLjcxNDk1NzYsImJpb21lMUNyYXRlcnNFcm9zaW9uIjowLjU1NTc2MjUsImJpb21lMUNyYXRlcnNEaWZmdXNlIjowLjM5NzEzMDksImJpb21lMUNyYXRlcnNCdW1wIjowLjI2MzU2ODYsImJpb21lMUNhbnlvbnNEaWZmdXNlIjowLjA5Nzg0NTA4LCJiaW9tZTFDYW55b25zQnVtcCI6MC45NDYxNjMxLCJiaW9tZTJTZWVkIjoyMDMsImJpb21lMkNoYW9zIjowLjcyNTI5ODMsImJpb21lMkJhbGFuY2UiOjAuNjIyNjA1NCwiYmlvbWUyQ29udHJhc3QiOjAuMjY2MzY3NiwiYmlvbWUyQ29sb3JWYXJpYXRpb24iOjAuNDU4OTUyMiwiYmlvbWUyU2F0dXJhdGlvbiI6MC4wNjE2NTczNCwiYmlvbWUyQnJpZ2h0bmVzcyI6MC45NTg5NDA0LCJiaW9tZTJTdXJmYWNlQnVtcCI6MC4zNzMzMjAzLCJiaW9tZTJDcmF0ZXJzU21hbGwiOjAuNTI0NzIxMywiYmlvbWUyQ3JhdGVyc01lZGl1bSI6MC40Mjg3MjIzLCJiaW9tZTJDcmF0ZXJzTGFyZ2UiOjAuMzMwODQ5MiwiYmlvbWUyQ3JhdGVyc0Vyb3Npb24iOjAuMTk0NTY1MSwiYmlvbWUyQ3JhdGVyc0RpZmZ1c2UiOjAuMTAxNzMxMywiYmlvbWUyQ3JhdGVyc0J1bXAiOjAuNzY4NjgzMiwiYmlvbWUyQ2FueW9uc0RpZmZ1c2UiOjAuNjMwOTMxNCwiYmlvbWUyQ2FueW9uc0J1bXAiOjAuNTI1NDQzMywiY2l0aWVzU2VlZCI6NTYsImNpdGllc1BvcHVsYXRpb24iOjAuMDExNDc2MDcsImNpdGllc0FkdmFuY2VtZW50IjowLCJjaXRpZXNHbG93IjowLjIxMjQ3ODIsImNpdGllc1RpbGluZyI6Nn0sInByb3BlcnR5TWF0ZXJpYWxzIjp7ImNvbXBvc2l0aW9uIjp7ImluZGV4IjoyLCJuYW1lIjoiU29saWRfRnVzZWQifSwicG9sYXJJY2UiOnsiaW5kZXgiOjAsIm5hbWUiOiJQb2xhcl9JY2UifSwiY2xvdWRzIjp7ImluZGV4IjoxLCJuYW1lIjoiQ2xvdWRzXzAyIn0sImxhdmEiOnsiaW5kZXgiOjAsIm5hbWUiOiJMYXZhXzAxIn0sImJpb21lMVR5cGUiOnsiaW5kZXgiOjEsIm5hbWUiOiJCaW9tZV9EdXN0In0sImJpb21lMlR5cGUiOnsiaW5kZXgiOjMsIm5hbWUiOiJCaW9tZV9JY2UifSwiY2l0aWVzIjp7ImluZGV4IjowLCJuYW1lIjoiQ2l0aWVzIn19LCJwcm9wZXJ0eUNvbG9ycyI6eyJzcGVjdWxhckNvbG9yIjp7InIiOjAuNDE2NzU5LCJnIjowLjM3MDU5MDQsImIiOjAuMDYxMTE2ODJ9LCJsaXF1aWRDb2xvciI6eyJyIjoxLCJnIjoxLCJiIjoxfSwiaWNlQ29sb3IiOnsiciI6MC45OTc5NDkyLCJnIjowLjk3Mjk0NTIsImIiOjAuOTY3MDYxM30sImF0bW9zcGhlcmVDb2xvciI6eyJyIjowLjgyNDk0NTQsImciOjAuODcxMTYzNiwiYiI6MC44OTMwOTQ1fSwidHdpbGlnaHRDb2xvciI6eyJyIjowLjEyMzc2NDksImciOjAuMDUyMzI4NDUsImIiOjAuMjI0OTI4NH0sImNsb3Vkc0NvbG9yIjp7InIiOjEsImciOjEsImIiOjF9LCJsYXZhR2xvd0NvbG9yIjp7InIiOjEsImciOjAuODYyMzk1LCJiIjowfSwiY2l0aWVzQ29sb3IiOnsiciI6MC45ODUzODY4LCJnIjowLjk4MTY2MTMsImIiOjAuOTMwMzM0fX19fQ==");

        // Add this gameobject to the message subscription list. The planet will call OnTextuireBuildComplete(float _time) 
        // of this gameobject every time it finishes a texture rebuild job.
        _planet.SubscribeMessageOnTextureBuildComplete(gameObject);

        // Set the UI text message to indicate that textures are being rebuilt.
        uiTextMessage.text = "Rebuilding textures...";
    }

    /// <summary>
    /// This method is automatically called by a planet once it has finished rebuilding any textures.
    /// You must call the SubscribeMessageOnTextureBuildComplete(GameObject _gameObject) method of the planet to add a gameobject to the message/event subscription.
    /// </summary>
    /// <param name="_time">The time argument contains time it took for the last rebuild process to finish in seconds.</param>
    void OnTextureBuildComplete(float _time)
    {
        // Display a log message that the method was called and how long it took to rebuild the textures.
        Debug.Log("Planet done regenerating textures (" + _time + " seconds)");

        // Set a temporary ignore flag while updating the slider values because the slider onValueChanged
        // delegate methods are executed when a value is set which would call the override method of the planet again.
        _ignoreOverrideFlag = true;

        // When a planet has finished building the textures, update the slider positions
        uiSliderLiquidLevel.value = _planet.GetPropertyFloat("liquidLevel");
        uiSliderCloudsCoverage.value = _planet.GetPropertyFloat("cloudsCoverage");

        // Update the ui text message to dispaly texture rebuild time
        uiTextMessage.text = "Build time: " + _time.ToString("F2") + " seconds.";

        // Remove the temporary ignore flag
        _ignoreOverrideFlag = false;
    }

    /// <summary>
    /// Overrides Liquid Level of the planet with a value from the UI slider.
    /// This method is called every time the slider is moved because an onValueChange delegate was added in the Start() method.
    /// </summary>
    public void OverrideLiquidLevel()
    {
        // Override the "liquidLevel" property to the value set by the UI slider
        // The liquidLevel property is fairly cheap to call performance-wise because it only creates a lookup texture for the shader.
        _planet.OverridePropertyFloat("liquidLevel", uiSliderLiquidLevel.value);
    }

    /// <summary>
    /// Overrides Cloud Coverage of the planet with a value from the UI slider.
    /// This method is called every time the slider is moved because an onValueChange delegate was added in the Start() method.
    /// </summary>
    public void OverrideCloudsCoverage()
    {
        // A temporary flag was set in the OnTextureBuildComplete() method to avoid this from being called twice when the UI value is updated from the script.
        if (_ignoreOverrideFlag) return;

        // Override the "cloudsCoverage" property to the value set by the UI slider
        // The cloudsCoverage property is fairly expensive to call performance-wise because it needs to rebuild the cloud procedural texture.
        _planet.OverridePropertyFloat("cloudsCoverage", uiSliderCloudsCoverage.value);
    }

    /// <summary>
    /// Sets the texture resolutions. They can be set independently of one another and usually you want to keep the Composition and Clouds
    /// textures higher than the other textures because they tile less.
    /// </summary>
    public void SetTextureResolution()
    {
        /* 
            Valid resolution values are:
                0 = 16 x 16 (lowest)
                1 = 32 x 32
                2 = 64 x 64
                3 = 128 x 128
                4 = 256 x 256
                5 = 512 x 512
                6 = 1024 x 1024
                7 = 2048 x 2048
        */

        Manager.Instance.resolutionBiomes = uiDropdownResolution.value;        
        Manager.Instance.resolutionCities = uiDropdownResolution.value;
        Manager.Instance.resolutionClouds = uiDropdownResolution.value;        
        Manager.Instance.resolutionContinent = uiDropdownResolution.value;
        Manager.Instance.resolutionLava = uiDropdownResolution.value;

        // Call the UpdateProceduralTexture() method of the planet and use the string argument "All" to update all textures.
        // (You can also specify individual textures, i.e. "Biome1", "Biome2", "Composition", "Clouds", "Cities", "Lava", and "PolarIce")
        _planet.UpdateProceduralTexture("All");

        uiTextMessage.text = "Rebuilding textures...";
    }

    /// <summary>
    /// Helper method that detects if mouse or a touch finger is over a Raycast target UI element or not.
    /// This is to avoid camera rotation/zoom when interacting with the UI sliders.
    /// </summary>
    /// <returns>True/False</returns>
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
