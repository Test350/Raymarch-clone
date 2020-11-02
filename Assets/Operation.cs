using UnityEngine;

public class Operation : MonoBehaviour
{
    public enum Op
    {
        None,
        Subtract,
        Intersect,
        Blend
    }
    
    public Op operation;
    public float blendStrength;

    //This will become a list once size is dynamic
    //Currently hard coded for demo

    public RaymarchShape[] shapes = new RaymarchShape[2];


}
