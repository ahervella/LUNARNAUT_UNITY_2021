using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "SO_AstroAnimSettings", menuName = "ScriptableObjects/SO_AstroAnimSettings")]
public class SO_AstroAnimSettings : ScriptableObject
{
    [SerializeField]
    private List<AstroAnimSuitWrapper> animSuitWrappers = new List<AstroAnimSuitWrapper>();

    [NonSerialized]
    private Dictionary<AstroAnim.SUIT, Dictionary<AstroAnim.PLAYER_STATE, AnimationClip>> animDict = null;

    [Serializable]
    private class AstroAnimSuitWrapper
    {
        [SerializeField]
        private AstroAnim.SUIT suit = AstroAnim.SUIT.GGG;
        public AstroAnim.SUIT Suit => suit;

        [SerializeField]
        private List<AstroAnimWrapper> animWrappers = new List<AstroAnimWrapper>();
        public List<AstroAnimWrapper> AnimWrappers => animWrappers;
    } 

    [Serializable]
    private class AstroAnimWrapper
    {
        [SerializeField]
        private AstroAnim.PLAYER_STATE playerState = AstroAnim.PLAYER_STATE.NONE;
        public AstroAnim.PLAYER_STATE PlayerState => playerState;

        [SerializeField]
        private AnimationClip anim = null;
        public AnimationClip Anim => anim;
    }

    public AnimationClip GetAstroAnim(AstroAnim.SUIT suit, AstroAnim.PLAYER_STATE state)
    {
        if (animDict == null)
        {
            animDict = new Dictionary<AstroAnim.SUIT, Dictionary<AstroAnim.PLAYER_STATE, AnimationClip>>();
            CacheAstroAnims();
        }
        return animDict[suit][state];
    }

    private void CacheAstroAnims()
    {
        animDict.Clear();

        foreach (AstroAnimSuitWrapper aasw in animSuitWrappers)
        {
            Dictionary<AstroAnim.PLAYER_STATE, AnimationClip> entry = new Dictionary<AstroAnim.PLAYER_STATE, AnimationClip>();

            foreach (AstroAnimWrapper aaw in aasw.AnimWrappers)
            {
                entry.Add(aaw.PlayerState, aaw.Anim);
            }

            animDict.Add(aasw.Suit, entry);
        }
    }
}
