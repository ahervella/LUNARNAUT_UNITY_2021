//Part of get set ability for inspector fields, provided by: https://answers.unity.com/questions/14985/how-do-i-expose-class-properties-not-fields-in-c.html?childToView=1513032#answer-1513032


using UnityEngine;
public sealed class GetSetAttribute : PropertyAttribute
{
    public readonly string name;
    public bool dirty;

    public GetSetAttribute(string name)
    {
        this.name = name;
    }
}