using UnityEngine;

public class MyRangeAttribute : PropertyAttribute
{
    readonly public float min;
    readonly public float max;

    MyRangeAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}