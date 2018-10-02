using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SkyboxBlender : MonoBehaviour {

    public enum BlendMode { Linear, Maximum, Add, Substract, Multiply }

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

    [Header("Lazy Buttons")]
    public bool bindTextures;

    private Dictionary<BlendMode, int> blendModeIndices;

    #region MonoBehaviour Functions

    // Use this for initialization
    void Start () {

        InitializeBlendModeIndices();

        if (bindOnStart)
            BindTextures();

        UpdateBlendedMaterialParameters();
    }
	
	// Update is called once per frame
	void Update () {

        if (bindTextures)
        {
            BindTextures();
            bindTextures = false;
        }

        UpdateBlendedMaterialParameters();
    }

    #endregion 

    void InitializeBlendModeIndices()
    {
        blendModeIndices = new Dictionary<BlendMode, int>();

        blendModeIndices.Add(BlendMode.Linear, 0);
        blendModeIndices.Add(BlendMode.Maximum, 1);
        blendModeIndices.Add(BlendMode.Add, 2);
        blendModeIndices.Add(BlendMode.Substract, 3);
        blendModeIndices.Add(BlendMode.Multiply, 4);
    }

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

    void UpdateBlendedMaterialParameters()
    {
        blendedSkybox.SetColor("_Tint", tint);
        blendedSkybox.SetFloat("_Exposure", exposure);
        blendedSkybox.SetFloat("_Rotation", rotation);
        blendedSkybox.SetFloat("_Blend", blend);
        blendedSkybox.SetInt("_BlendMode", blendModeIndices[blendMode]);
        blendedSkybox.SetFloat("_InvertColors", invertColors);

    }

}
