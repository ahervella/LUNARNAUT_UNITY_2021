using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AstroColorChanger : AstroColorPalette
{
    [SerializeField]
    private List<ColorKVP> futureColorCompDict;

    private AstroAnim.SUIT currSuit;

    protected override void Awake()
    {
        //base.Awake();
        //ideally should make the List<ColorKVP> here the pastColorCompDict so that we can use the base.Awake,
        //but would have to add it in the inspector again manually and I'm lazy

        //this is from the base.Awake, changed the parameter of SwapAllColorComponents
        spriteRenderer = GetComponent<SpriteRenderer>();
        InitColorSwapTex();
        SwapAllColorComponents(futureColorCompDict);

        S_TimeTravel.Current.TimelineChanged -= S_TimeTravel_TimelineChanged;
        S_TimeTravel.Current.TimelineChanged += S_TimeTravel_TimelineChanged;

        AstroAnim.SuitChanged -= AstroAnim_SuitChanged;
        AstroAnim.SuitChanged += AstroAnim_SuitChanged;
    }

    private void S_TimeTravel_TimelineChanged()
    {
        ResetColorSwapTex();

        SwapAllColorComponents(S_TimeTravel.Current.InFuture() ? futureColorCompDict : colorCompDict);
        //ideally should request curr suit color from astro anim;
        AstroAnim_SuitChanged(currSuit);
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
}
