Shader "Unlit/Plane Curve"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Width ("Width", Float) = 0.05
        _Precision ("Precision", Float) = 0.01
        _Period ("Period", Float) = 1
        _Amplitude ("Amplitude", Float) = 1
        _EaseId ("EaseId", Int) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent" "RenderType" = "Transparent"
        }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #include "Lib/Easings.hlsl"
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Width;
            float _Precision;
            float _Period;
            float _Amplitude;
            int _EaseId;
            float4 _MainTex_ST;

            inline float2 GetEasedPoint(float x)
            {
                return float2(x, EaseById(0, 1, x, _EaseId)) * float2(_Amplitude, _Period);
            }

            float2 TransformedPoint(float x, float y, float precision)
            {
                float2 result;
                float2 centerPoint = GetEasedPoint(x);
                float width = _Width * (y > 0 ? 1 : -1);

                if (x < 1e-5f)
                {
                    float2 nextPoint = GetEasedPoint(x + precision);
                    float2 direction = nextPoint - centerPoint;
                    float2 normal = normalize(float2(-direction.y, direction.x));
                    result = centerPoint + width * normal;
                }
                else if (x > 1 - 1e-5f)
                {
                    float2 lastPoint = GetEasedPoint(x - precision);
                    float2 direction = centerPoint - lastPoint;
                    float2 normal = normalize(float2(-direction.y, direction.x));
                    result = centerPoint + width * normal;
                }
                else
                {
                    float2 lastPoint = GetEasedPoint(x - precision);
                    float2 nextPoint = GetEasedPoint(x + precision);
                    float2 direction1 = centerPoint - lastPoint;
                    float2 direction2 = nextPoint - centerPoint;
                    float2 normal1 = normalize(float2(-direction1.y, direction1.x));
                    float2 normal2 = normalize(float2(-direction2.y, direction2.x));
                    float2 point1 = lastPoint + width * normal1;
                    float2 point2 = centerPoint + width * normal2;
                    result = LineIntersection(direction1, point1, direction2, point2);
                }

                return float2(result.x, result.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                v.vertex.yx = TransformedPoint(v.vertex.y, v.vertex.x, _Precision);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv) * _Color;
            }
            ENDCG
        }
    }
}