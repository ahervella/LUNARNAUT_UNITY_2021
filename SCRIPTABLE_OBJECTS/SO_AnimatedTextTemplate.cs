using UnityEngine;

//TODO: make a styalizing system?
[CreateAssetMenu(fileName = "new_animatedText", menuName = "ScriptableObjects/AnimatedText")]
public class SO_AnimatedTextTemplate : ScriptableObject
{
    [SerializeField]
    private AnimatedText.ATDetails details;
    public AnimatedText.ATDetails Details => details;
}
