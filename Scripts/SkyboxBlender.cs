using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SkyboxBlender : MonoBehaviour {

    public enum BlendMode { Linear, Maximum, Add, Substract, Multiply }
    public enum ProbeResolution { _16, _32, _64, _128, _256, _512, _1024, _2048 }

    [Header("Input Skyboxes")]
    public Material skyBox1;
    public Material skyBox2;

    [Header("Blended Skybox")]
    public Material blendedSkybox;
    [Range(0, 8)] public float exposure = 1;
    [Range(0, 360)] public float rotation = 0;
    public Color tint = Color.white;
    [Range(0, 1)] public float invertColors = 0;
    public BlendMode blendMode = BlendMode.Linear;
    [Range(0, 1)] public float blend = 0;

    public bool bindOnStart = true;
    public bool updateLightingEveryFrame = true;
    public bool updateReflectionsEveryFrame = true;

    public ProbeResolution reflectionResolution = ProbeResolution._128;

    public bool updateInEditMode = true;

    [Header("Lazy Buttons")]
    public bool bindTextures;
    public bool initialize;
    public bool updateLighting;
    public bool updateReflections;

    private ReflectionProbe probeComponent = null;
    private GameObject probeGameObject = null;
    private Cubemap blendedCubemap = null;
    private int renderId = -1;

    #region MonoBehaviour Functions

    // Use this for initialization
    void Start () {

        if (bindOnStart)
            BindTextures();

        //Update the material parameters
        UpdateBlendedMaterialParameters();

        //Create the reflection probe
        UpdateReflectionProbe();
    }
	
	// Update is called once per frame
	void Update () {

        //Lazy buttons
        if (bindTextures)
        {
            BindTextures();
            bindTextures = false;
        }

        if (initialize)
        {
            Start();
            initialize = false;
        }

        if (updateLighting)
        {
            UpdateLighting();
            updateLighting = false;
        }

        if (updateReflections)
        {
            UpdateReflections();
            updateReflections = false;
        }

        //Update material parameters
        UpdateBlendedMaterialParameters();

        //Update reflections
        if (updateReflectionsEveryFrame)
            UpdateReflections();

        //Update lighting
        if (updateLightingEveryFrame)
            UpdateLighting();
    }
    
    private void OnValidate()
    {
        if (!updateInEditMode)
            return;

        //Update the reflection probe parameters
        UpdateReflectionProbe();

        Update();

        UpdateReflections();
        UpdateLighting();
    }
    
    #endregion 

    /// <summary>
    /// Get the probe resolution value
    /// </summary>
    int GetProbeResolution(ProbeResolution probeResolution)
    {
        switch (probeResolution)
        {
            case ProbeResolution._16:
                return 16;
            case ProbeResolution._32:
                return 32;
            case ProbeResolution._64:
                return 64;
            case ProbeResolution._128:
                return 128;
            case ProbeResolution._256:
                return 256;
            case ProbeResolution._512:
                return 512;
            case ProbeResolution._1024:
                return 1024;
            case ProbeResolution._2048:
                return 2048;
            default:
                return 128;
        }
    }

    /// <summary>
    /// Create a reflection probe gameobject and setup the cubemap for environment reflections
    /// </summary>
    void CreateReflectionProbe()
    {
        //Search for the reflection probe object
        probeGameObject = GameObject.Find("Skybox Blender Reflection Probe");

        if (!probeGameObject)
        {
            //Create the gameobject if its not here
            probeGameObject = new GameObject("Skybox Blender Reflection Probe");
            probeGameObject.transform.parent = gameObject.transform;
            // Use a location such that the new Reflection Probe will not interfere with other Reflection Probes in the scene.
            probeGameObject.transform.position = new Vector3(0, -1000, 0);
        }

        probeComponent = probeGameObject.GetComponent<ReflectionProbe>();

        if (!probeComponent)
        {
            // Create a Reflection Probe that only contains the Skybox. The Update function controls the Reflection Probe refresh.
            probeComponent = probeGameObject.AddComponent<ReflectionProbe>() as ReflectionProbe;
        }

        // A cubemap is used as a default specular reflection.
        blendedCubemap = new Cubemap(probeComponent.resolution, probeComponent.hdr ? TextureFormat.RGBAHalf : TextureFormat.RGBA32, true);

        //Set the render reflection mode to Custom
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        RenderSettings.customReflection = blendedCubemap;
    }

    /// <summary>
    /// Update the reflection probe
    /// </summary>
    void UpdateReflectionProbe()
    {
        if (!probeGameObject || !probeComponent)
            CreateReflectionProbe();

        probeComponent.resolution = GetProbeResolution(reflectionResolution);
        probeComponent.size = new Vector3(1, 1, 1);
        probeComponent.cullingMask = 0;
        probeComponent.clearFlags = ReflectionProbeClearFlags.Skybox;
        probeComponent.mode = ReflectionProbeMode.Realtime;
        probeComponent.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        probeComponent.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
    }

    /// <summary>
    /// Update the scene environment lighting
    /// </summary>
    void UpdateLighting()
    {
        DynamicGI.UpdateEnvironment();
    }

    /// <summary>
    /// Update the scene environment reflections
    /// </summary>
    void UpdateReflections()
    {
        // The Update function refreshes the Reflection Probe and copies the result to the default specular reflection Cubemap.

        // The texture associated with the real-time Reflection Probe is a render target and RenderSettings.customReflection is a Cubemap. We have to check the support if copying from render targets to Textures is supported.
        if ((SystemInfo.copyTextureSupport & CopyTextureSupport.RTToTexture) != 0)
        {
            // Wait until previous RenderProbe is finished before we refresh the Reflection Probe again.
            // renderId is a token used to figure out when the refresh of a Reflection Probe is finished. The refresh of a Reflection Probe can take mutiple frames when time-slicing is used.
            if (renderId == -1 || probeComponent.IsFinishedRendering(renderId))
            {
                if (probeComponent.IsFinishedRendering(renderId))
                {
                    // After the previous RenderProbe is finished, we copy the probe's texture to the cubemap and set it as a custom reflection in RenderSettings.
                    Graphics.CopyTexture(probeComponent.texture, blendedCubemap as Texture);

                    RenderSettings.customReflection = blendedCubemap;
                }

                renderId = probeComponent.RenderProbe();
            }
        }
    }

    /// <summary>
    /// Get the BlendMode index from the enumeration
    /// </summary>
    int GetBlendModeIndex(BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Linear:
                return 0;
            case BlendMode.Maximum:
                return 1;
            case BlendMode.Add:
                return 2;
            case BlendMode.Substract:
                return 3;
            case BlendMode.Multiply:
                return 4;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Bind the input skyboxes textures to the blended skybox
    /// </summary>
    void BindTextures()
    {
        blendedSkybox.SetTexture("_FrontTex_1", skyBox1.GetTexture("_FrontTex"));
        blendedSkybox.SetTexture("_BackTex_1", skyBox1.GetTexture("_BackTex"));
        blendedSkybox.SetTexture("_LeftTex_1", skyBox1.GetTexture("_LeftTex"));
        blendedSkybox.SetTexture("_RightTex_1", skyBox1.GetTexture("_RightTex"));
        blendedSkybox.SetTexture("_UpTex_1", skyBox1.GetTexture("_UpTex"));
        blendedSkybox.SetTexture("_DownTex_1", skyBox1.GetTexture("_DownTex"));

        blendedSkybox.SetTexture("_FrontTex_2", skyBox2.GetTexture("_FrontTex"));
        blendedSkybox.SetTexture("_BackTex_2", skyBox2.GetTexture("_BackTex"));
        blendedSkybox.SetTexture("_LeftTex_2", skyBox2.GetTexture("_LeftTex"));
        blendedSkybox.SetTexture("_RightTex_2", skyBox2.GetTexture("_RightTex"));
        blendedSkybox.SetTexture("_UpTex_2", skyBox2.GetTexture("_UpTex"));
        blendedSkybox.SetTexture("_DownTex_2", skyBox2.GetTexture("_DownTex"));
    }

    /// <summary>
    /// Update the material parameters
    /// </summary>
    void UpdateBlendedMaterialParameters()
    {
        blendedSkybox.SetColor("_Tint", tint);
        blendedSkybox.SetFloat("_Exposure", exposure);
        blendedSkybox.SetFloat("_Rotation", rotation);
        blendedSkybox.SetFloat("_Blend", blend);
        blendedSkybox.SetInt("_BlendMode", GetBlendModeIndex(blendMode));
        blendedSkybox.SetFloat("_InvertColors", invertColors);

    }

}
