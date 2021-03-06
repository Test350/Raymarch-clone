﻿using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]

public class RaymarchController : SceneViewFilter
{
    public Color _MainColor;

    [SerializeField] private Shader _Shader = null;
    
    public Operation _operation;

    private Material _material;
    private Camera _cam;
    private Transform _light;
    

    public Material Material
    {
        get
        {
            if (!_material && _Shader)
                _material = new Material(_Shader);         
            return _material;
        }
    }

    public Camera Cam
    {
        get
        {
            if (!_cam)
                _cam = GetComponent<Camera>();
            //Sometimes unity cameras don't render depth texture by default?
            //I spent a good hour and a half figuring that out...
            _cam.depthTextureMode = DepthTextureMode.Depth;
            return _cam;
        }
    }

    public Transform Light
    {
        get
        {
            Light l;

            if (!_light)
            {
                l = (Light)FindObjectOfType(typeof(Light));
                
                if(!l)
                {
                    return _light;
                }

                _light = l.transform;
            }
                
            return _light;
        }
    }

    static void Blit(RenderTexture source, RenderTexture destination, Material mat, int pass)
    {
        RenderTexture.active = destination;
        mat.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();
        mat.SetPass(pass);

        GL.Begin(GL.QUADS);

        //Bottom Left
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);

        //Bottom Right
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);

        //Top Right
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);

        //Top Left
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();
    }



    



    //Unity event function, called when an image is done rendering to apply post processing effects
    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!Material)
        {
            Graphics.Blit(source, destination);
            return;
        }

        //Passing Scene Values to raymarch shader
        Material.SetMatrix("_Frustum", GetFrustum(Cam));
        Material.SetMatrix("_CamMatrix", Cam.cameraToWorldMatrix);
        Material.SetColor("_MainColor", _MainColor);
        Material.SetVector("_Light", Light ? Light.forward : Vector3.down);

        /*Vector4[] pos = new Vector4[2];
        float[] shape = new float[2];
        float[] sphereRadius = new float[2];
        float[] torusInner = new float[2];
        float[] torusOuter = new float[2];
        float[] boxRoundness = new float[2];
        float[] coneHeight = new float[2];
        Vector4[] box = new Vector4[2];
        Vector4[] roundBox = new Vector4[2];
        Vector4[] coneRatio = new Vector4[2];
      
        for (int i = 0; i < this._operation.shapes.Length; i++)
        {
            pos[i] = this._operation.shapes[i].position;
            shape[i] = (int)this._operation.shapes[i].shape;
            sphereRadius[i] = this._operation.shapes[i].sphereRadius;
            torusInner[i] = this._operation.shapes[i].torusInnerRadius;
            torusOuter[i] = this._operation.shapes[i].torusOuterRadius;
            boxRoundness[i] = this._operation.shapes[i].roundBoxFactor;
            coneHeight[i] = this._operation.shapes[i].coneHeight;
            box[i] = this._operation.shapes[i].boxDimensions;
            roundBox[i] = this._operation.shapes[i].roundBoxDimensions;
            coneRatio[i] = this._operation.shapes[i].coneRatio;

        }

        //Pass Shape specific values to shader
        Material.SetVectorArray("_Position", pos);
        Material.SetFloatArray("_Shape", shape);
        
        Material.SetFloatArray("_SphereRadius", sphereRadius);
        Material.SetFloatArray("_TorusInner", torusInner);
        Material.SetFloatArray("_TorusOuter", torusOuter);
        Material.SetFloatArray("_BoxRoundness", boxRoundness);
        Material.SetFloatArray("_ConeHeight", coneHeight);

        Material.SetVectorArray("_Box", box);
        Material.SetVectorArray("_RoundBox", roundBox);
        Material.SetVectorArray("_ConeRatio", coneRatio);*/

        Material.SetFloat("_Blend", this._operation.blendStrength);

        Blit(source, destination, Material, 0);
    }

    //Returns a matrix containing the corner positions of the camera's view frustum
    private Matrix4x4 GetFrustum(Camera cam)
    {
        Matrix4x4 corners = Matrix4x4.identity;

        float camFOV = cam.fieldOfView;
        float camAr = cam.aspect;

        float camRatio = Mathf.Tan(camFOV * .5f * Mathf.Deg2Rad);

        Vector3 right = Vector3.right * camRatio * camAr;
        Vector3 up = Vector3.up * camRatio;

        Vector3 TL = (-Vector3.forward - right + up);
        Vector3 TR = (-Vector3.forward + right + up);
        Vector3 BR = (-Vector3.forward + right - up);
        Vector3 BL = (-Vector3.forward - right - up);

        corners.SetRow(0, TL);
        corners.SetRow(1, TR);
        corners.SetRow(2, BR);
        corners.SetRow(3, BL);

        return corners;
    }

    void PassShapeValues()
    {

    }
    
}
