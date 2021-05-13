//Used this tutorial's code and instructions for doing all of this:
//https://gamedevelopment.tutsplus.com/tutorials/how-to-use-a-shader-to-dynamically-swap-a-sprites-colors--cms-25129

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AstroColorChanger : MonoBehaviour
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

    private SpriteRenderer spriteRenderer;
    Texture2D cachedColorSwapTex;
    //Color[] cachedSpriteColors = Enumerable.Repeat(new Color(0f, 0f, 0f, 0f), 256).ToArray();//new Color[256];
    /*
    [SerializeField, GetSet("ToggleCustomColors")]
    private bool toggleCustomColors = true;
    public bool ToggleCustomColors
    {
        get => toggleCustomColors;
        set
        {
            toggleCustomColors = value;
            //ToggleApplyingColorComponents(value);
        }
    }*/
    /*
    [SerializeField, GetSet("SwapTex")]
    private Texture2D swapTex;
    public Texture2D SwapTex
    {
        get => swapTex;
        set
        {
            swapTex = value;
            if (value != null)
            {
                //InitColorSwapTex(value);
            }
        }
    }*/

    [SerializeField]
    private List<ColorKVP> futureColorCompDict;

    [SerializeField]
    private List<ColorKVP> colorCompDict;

    //private List<ColorKVP> currCompDict;

    private AstroAnim.SUIT currSuit;

    [Serializable]
    private class ColorKVP
    {
        /*
        [SerializeField]
        private bool active = false;
        public bool Active => active;*/

        [SerializeField]
        private RED_COLOR_SWAP_INDEX astroComponent;
        public RED_COLOR_SWAP_INDEX AstroComponent => astroComponent;

        [SerializeField]
        private Color newColor;
        public Color NewColor => newColor;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        InitColorSwapTex();
        SwapAllColorComponents();
        S_TimeTravel.Current.TimelineChanged -= S_TimeTravel_TimelineChanged;
        S_TimeTravel.Current.TimelineChanged += S_TimeTravel_TimelineChanged;

        AstroAnim.SuitChanged -= AstroAnim_SuitChanged;
        AstroAnim.SuitChanged += AstroAnim_SuitChanged;
        //S_TimeTravel_TimelineChanged();
    }

    private void S_TimeTravel_TimelineChanged()
    {
        ResetColorSwapTex();
        //currCompDict = colorCompDict;//S_TimeTravel.Current.InFuture() ? futureColorCompDict : colorCompDict;
        SwapAllColorComponents();
        //ideally should request curr suit color from astro anim;
        AstroAnim_SuitChanged(currSuit);

        //already called in suit update. Messy I know :(
        //UpdateTexture();
    }

    private void AstroAnim_SuitChanged(AstroAnim.SUIT suit)
    {
        Color g = Color.green;
        Color r = Color.red;
        currSuit = suit;
        switch (suit)
        {
            case AstroAnim.SUIT.GGG:
                SetSuitColors(g, g, g);
                break;
            case AstroAnim.SUIT.GGR:
                SetSuitColors(g, g, r);
                break;
            case AstroAnim.SUIT.GRR:
                SetSuitColors(g, r, r);
                break;
            case AstroAnim.SUIT.RGG:
                SetSuitColors(r, g, g);
                break;
            case AstroAnim.SUIT.RRG:
                SetSuitColors(r, r, g);
                break;
            case AstroAnim.SUIT.RRR:
                SetSuitColors(r, r, r);
                break;
        }

        UpdateTexture();
    }

    private void SetSuitColors(Color light1, Color light2, Color light3)
    {
        SwapColor(RED_COLOR_SWAP_INDEX.LIGHT_1, light1);
        SwapColor(RED_COLOR_SWAP_INDEX.LIGHT_2, light2);
        SwapColor(RED_COLOR_SWAP_INDEX.LIGHT_3, light3);
    }

    public void InitColorSwapTex()
    {
        ResetColorSwapTex();

        UpdateTexture();


        //no need to do this since we know we're always gonna use a 1x256, set in declaration
        //cachedSpriteColors = new Color[colorSwapTex.width];
    }

    private void UpdateTexture()
    {
        //apply changes
        cachedColorSwapTex.Apply();

        //set this new color swap texture onto the _SwapTex variable in our custom shader material that we made
        spriteRenderer.material.SetTexture("_SwapTex", cachedColorSwapTex);
    }

    private void ResetColorSwapTex()
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

    private void SwapColor(RED_COLOR_SWAP_INDEX index, Color color)
    {
        //cachedSpriteColors[index] = color;
        cachedColorSwapTex.SetPixel((int)colorDict[index].y, (int)colorDict[index].z, color);
    }

    private void SwapColor(int index, Color color)
    {
        //cachedSpriteColors[index] = color;
        cachedColorSwapTex.SetPixel(index, 0, color);
    }

    private void SwapAllColorComponents()
    {
        /*
        yield return new WaitForSeconds(3f);
        for (int i = 0; i <= 256; i++)
        {
            int k = i;
            SwapColor(k, new Color(0, 1, 0.3f, 1));
            cachedColorSwapTex.Apply();
            Debug.LogFormat("on color: {0}", k);
            yield return new WaitForSeconds(0.5f);
            SwapColor(k, new Color(0, 0, 0, 0));
            cachedColorSwapTex.Apply();
            //SwapColor(i, new Color(0, 0, 1, 1));
        }*/
        
        foreach (ColorKVP ckvp in S_TimeTravel.Current.InFuture()? futureColorCompDict : colorCompDict)
        {
            Debug.LogFormat("waz dis? {0}", (int)ckvp.AstroComponent);
            SwapColor(ckvp.AstroComponent, ckvp.NewColor);
        }
        
        cachedColorSwapTex.Apply();
    }

    /*
    private void ToggleApplyingColorComponents(bool on)
    {
        if (on)
        {
            Color defaultTransparentBlack = new Color(0, 0, 0, 0);
            for(int i = 0; i < cachedColorSwapTex.width; i++)
            {
                cachedColorSwapTex.SetPixel(i, 0, defaultTransparentBlack);
            }
        }
        else
        {
            //cachedColorSwapTex.width should be same as cachedSpriteColors.Length
            for (int i = 0; i < cachedSpriteColors.Length; i++)
            {
                cachedColorSwapTex.SetPixel(i, 0, cachedSpriteColors[i]);
            }
        }

        cachedColorSwapTex.Apply();
    }*/
}
