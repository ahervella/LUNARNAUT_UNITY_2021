using UnityEngine;

//TODO: make a styalizing system?
[CreateAssetMenu(fileName = "SO_ATT_", menuName = "ScriptableObjects/AnimatedText")]
public class SO_AnimatedTextTemplate : ScriptableObject
{
    [SerializeField]
    private AnimatedText.ATDetails details;
    public AnimatedText.ATDetails Details => details;
}
