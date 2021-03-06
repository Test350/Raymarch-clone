﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RaymarchShape : MonoBehaviour
{ 
    public enum Shape
    {
        Sphere,
        Box,
        Torus,
        Cone,
        RoundedBox
    }

 
    public Shape shape;
    public Vector3 position
    {
        get
        {
            return this.transform.position;
        }
    }

    public float sphereRadius;

    public Vector3 boxDimensions;

    public Vector3 roundBoxDimensions;
    public float roundBoxFactor;

    public float torusOuterRadius;
    public float torusInnerRadius;

    public float coneHeight;
    public Vector2 coneRatio;
    
}

