//Used this tutorial's code and instructions for doing all of this:
//https://gamedevelopment.tutsplus.com/tutorials/how-to-use-a-shader-to-dynamically-swap-a-sprites-colors--cms-25129

using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AstroColorPalette : MonoBehaviour
{
    public enum RED_COLOR_SWAP_INDEX
    {
        OUTLINE_DARK,// = 102,
        OUTLINE_MED,// = 153,
        OUTLINE_LIGHT,// = 204,
        VISOR,// = 153,//51,
        VISOR_DARK,// = 110,//37,
        SUIT_JOINTS,// = 102,//43,
        SUIT_JOINTS_DARK,// = 69,//26,
        BOOT_HIGHLIGHT,// = 245,
        HELMET_HIGHLIGHT,
        SUIT_MED_LIGHT,// = 223,
        SUIT_LIGHT,// = 234,
        SUIT,// = 0
        LIGHT_1,
        LIGHT_2,
        LIGHT_3,
        VISOR_HIGHLIGHT
    }

    private Dictionary<RED_COLOR_SWAP_INDEX, Vector3> colorDict = new Dictionary<RED_COLOR_SWAP_INDEX, Vector3>
    {
        { RED_COLOR_SWAP_INDEX.OUTLINE_DARK, new Vector3(0,102,102) },
        { RED_COLOR_SWAP_INDEX.OUTLINE_MED, new Vector3(0,153,153) },
        { RED_COLOR_SWAP_INDEX.OUTLINE_LIGHT, new Vector3(0,204,204) },
        { RED_COLOR_SWAP_INDEX.VISOR, new Vector3(0,153,0) },
        { RED_COLOR_SWAP_INDEX.VISOR_DARK, new Vector3(0,110,184) },
        { RED_COLOR_SWAP_INDEX.SUIT_JOINTS, new Vector3(0,102,0) },
        { RED_COLOR_SWAP_INDEX.SUIT_JOINTS_DARK, new Vector3(0,69,173) },
        { RED_COLOR_SWAP_INDEX.BOOT_HIGHLIGHT, new Vector3(0,91,70) },
        { RED_COLOR_SWAP_INDEX.HELMET_HIGHLIGHT, new Vector3(0,124,193) },
        { RED_COLOR_SWAP_INDEX.SUIT_MED_LIGHT, new Vector3(0,223,223) },
        { RED_COLOR_SWAP_INDEX.SUIT_LIGHT, new Vector3(0,234,234) },
        { RED_COLOR_SWAP_INDEX.SUIT, new Vector3(0,0,0) },
        { RED_COLOR_SWAP_INDEX.LIGHT_1, new Vector3(0,140,240) },
        { RED_COLOR_SWAP_INDEX.LIGHT_2, new Vector3(0,201,162) },
        { RED_COLOR_SWAP_INDEX.LIGHT_3, new Vector3(0,193,77) },
        { RED_COLOR_SWAP_INDEX.VISOR_HIGHLIGHT, new Vector3(0,193,158) }
    };

    protected SpriteRenderer spriteRenderer;
    private Texture2D cachedColorSwapTex;


    [SerializeField]
    protected List<ColorKVP> colorCompDict;


    [Serializable]
    protected class ColorKVP
    {

        [SerializeField]
        private RED_COLOR_SWAP_INDEX astroComponent;
        public RED_COLOR_SWAP_INDEX AstroComponent => astroComponent;

        [SerializeField]
        private Color newColor;
        public Color NewColor => newColor;
    }

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        InitColorSwapTex();
        SwapAllColorComponents(colorCompDict);
    }

    protected void InitColorSwapTex()
    {
        ResetColorSwapTex();

        UpdateTexture();


        //no need to do this since we know we're always gonna use a 1x256, set in declaration
        //cachedSpriteColors = new Color[colorSwapTex.width];
    }

    protected void UpdateTexture()
    {
        //apply changes
        cachedColorSwapTex.Apply();

        //set this new color swap texture onto the _SwapTex variable in our custom shader material that we made
        spriteRenderer.material.SetTexture("_SwapTex", cachedColorSwapTex);
    }

    protected void ResetColorSwapTex()
    {
        //create new 256x1 texture for all possible values of red
        cachedColorSwapTex = new Texture2D(256, 256, TextureFormat.RGBA32, false, false);
        cachedColorSwapTex.filterMode = FilterMode.Point;

        //default black color for each pixel in texture
        for (int i = 0; i < cachedColorSwapTex.width; i++)
        {
            for (int k = 0; k < cachedColorSwapTex.height; k++)
            {
                cachedColorSwapTex.SetPixel(i, k, new Color(0.0f, 0.0f, 0.0f, 0.0f));
            }
        }
    }

    protected void SwapColor(RED_COLOR_SWAP_INDEX index, Color color)
    {
        //cachedSpriteColors[index] = color;
        cachedColorSwapTex.SetPixel((int)colorDict[index].y, (int)colorDict[index].z, color);
    }


    protected void SwapAllColorComponents(List<ColorKVP> colorDict)
    {
        foreach (ColorKVP ckvp in colorDict)//
        {
            //Debug.LogFormat("waz dis? {0}", (int)ckvp.AstroComponent);
            SwapColor(ckvp.AstroComponent, ckvp.NewColor);
        }

        cachedColorSwapTex.Apply();
    }
}
