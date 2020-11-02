Shader "Custom/Raymarch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "SDFunc.cginc"

            //Scene info
            sampler2D _MainTex;
            uniform fixed4 _MainColor;
            uniform float4x4 _Frustum;
            uniform float4x4 _CamMatrix;            
            uniform float3 _Light;

            //Shape info
            uniform int _Shape;
            uniform float3 _Position;
            
            uniform float _SphereRadius;
            uniform float _TorusInner;
            uniform float _TorusOuter;
            uniform float _BoxRoundness;
            uniform float _ConeHeight;

            uniform float2 _ConeRatio;
            uniform float3 _Box;
            uniform float3 _RoundBox;

            

            //How many times each ray is marched
            //Higher values give higher resolution (and potentially longer draw distances) but lower performance
            static const int maxSteps = 100;
            
            //How close does a ray have to get to be consider a hit
            //Higher values give a sharper definition of shape but lower performance
            static const float epsilon = 0.01;
         
            //The maximum distance we want a ray to be from the nearest surface before giving up
            //Higher values give a longer draw distance but lower performance
            static const float maxDist = 100;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ray : TEXCOORD1;
            };

            struct ray
            {
                float3 origin;
                float3 direction;
                float3 position;
                float depth;
            };

            
            //This function will later be adjusted to handle more shapes & different kinds
            //For now it will just draw the distance from a sphere
            float SurfaceDistance(float3 p)
            {
                p -= _Position;

                switch (_Shape) 
                {
                    case 0:
                        return sdSphere(p, _SphereRadius);
                        break;

                    case 1:
                        return sdBox(p, _Box);
                        break;

                    case 2:
                        return sdTorus(p, _TorusOuter, _TorusInner);
                        break;

                    case 3:
                        return sdCone(p, _ConeRatio, _ConeHeight);
                        break;

                    case 4:
                        return sdRoundBox(p, _RoundBox, _BoxRoundness);
                        break;
                }

                return 0;
            }

            //For a signed distances field, the normal of any given point is defined as the gradient of the distance field
            //As such, subtracting the distance field of a slight smaller value by a slight large value produces a good approximation
            //This function is exceptionally expensive as it requires 6 more calls of a sign distance function PER PIXEL hit
            float3 CalculateNormal(float3 p)
            {
                                      
                float x = SurfaceDistance(float3(p.x + epsilon, p.y, p.z)) - SurfaceDistance(float3(p.x - epsilon, p.y, p.z));
                float y = SurfaceDistance(float3(p.x, p.y + epsilon, p.z)) - SurfaceDistance(float3(p.x, p.y - epsilon, p.z));
                float z = SurfaceDistance(float3(p.x, p.y, p.z + epsilon)) - SurfaceDistance(float3(p.x, p.y, p.z - epsilon));

                return normalize(float3(x,y,z));
            }

            //For each pixel on the screen
            fixed4 raymarch(ray r) 
            {
                //Start with a completely transparent pixel
                fixed4 pixelColor = fixed4(0, 0, 0, 0);
                //Cast out a ray at the pixel's UV coordinate
                float dst = 0;
                         
                //For a maximum of <maxStep> times,
                for (int i = 0; i < maxSteps; i++) 
                {
                    //Determine the distance from the nearest shape in the scene
                    r.position = r.origin + r.direction * dst;                  
                    float surfDist = SurfaceDistance(r.position);

                    //If the distance is sufficently small...
                    if (surfDist < epsilon)
                    {
                        //We "hit" the surface. Calculate the normal vector of the pixel and shade it based on the angle from the rays of light
                        float3 n = CalculateNormal(r.position);
                        
                        //This uses the lambertian model of lighting https://en.wikipedia.org/wiki/Lambertian_reflectance
                        float light = dot(-_Light.xyz, n).rrr;

                        
                        //Set the color of the pixel
                        //TODO replace alpha channel with a variable for transparency
                        pixelColor = fixed4(_MainColor.rgb * light, 1);
                        break;
                    }

                    //If the distance is not sufficently small, we missed.
                    //Move the ray's position forward and try again
                    dst += surfDist;
                    
                    
                    //If the distance is very large or a mesh is in the way
                    //we give up and break early

                    if (dst > maxDist || dst >= r.depth)
                        break;
                }

                //Give the frag function the color we want the pixel to be
                return pixelColor;

            }        

            v2f vert(appdata v)
            {
                v2f o;

                half index = v.vertex.z;
                v.vertex.z = 0;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.ray = _Frustum[(int)index].xyz;

                //Normalize along the z-axis
                //Absval function prevents scene from inverting
                o.ray /= abs(o.ray.z);

                //Places ray in worldspace so the depth buffer is calculated properly
                o.ray = mul(_CamMatrix, o.ray);
                return o;
            }

            uniform sampler2D _CameraDepthTexture;

            //Runs for every pixel on the screen
            fixed4 frag(v2f i) : SV_Target
            {
                ray r;
                
                //https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
                r.direction = normalize(i.ray.xyz);
                r.origin = _WorldSpaceCameraPos;                          

                r.depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
                r.depth *= length(i.ray.xyz);


                //The color of the pixel before any post processing done by the raymarch shader
                fixed3 base = tex2D(_MainTex, i.uv);

                //The color of the pixel after the raymarch function
                fixed4 col = raymarch(r);
                
                //Alpha blending function, derived via https://en.wikipedia.org/wiki/Alpha_compositing#Alpha_blending
                return fixed4(base * (1.0 - col.w) + col.xyz * col.w, 1.0);
            }
            ENDCG
        }
    }
}
