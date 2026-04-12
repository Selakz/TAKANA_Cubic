Shader "Unlit/Any Plane Curve"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Width ("Width", Float) = 0.05
        _Precision ("Precision", Float) = 0.01
        _Period ("Period", Float) = 1
        _Amplitude ("Amplitude", Float) = 1
        _SamplePointCount ("Sample Point Count", Integer) = 100
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
            float4 _MainTex_ST;
            float4 _SamplePointArray[50];
            int _SamplePointCount;

            float2 GetPoint(int index)
            {
                float4 raw = _SamplePointArray[index >> 1];
                return (index & 1) == 0 ? raw.xy : raw.zw;
            }

            float BinarySearchInterpolate(float x)
            {
                if (x <= GetPoint(0).x) return GetPoint(0).y;
                if (x >= GetPoint(_SamplePointCount - 1).x) return GetPoint(_SamplePointCount - 1).y;

                int low = 0;
                int high = _SamplePointCount - 1;
                [unroll(8)]
                while (low <= high)
                {
                    int mid = (low + high) / 2;
                    float midX = GetPoint(mid).x;

                    if (midX < x)
                        low = mid + 1;
                    else
                        high = mid - 1;
                }

                float2 pLeft = GetPoint(high);
                float2 pRight = GetPoint(low);
                float t = (x - pLeft.x) / (pRight.x - pLeft.x);
                return lerp(pLeft.y, pRight.y, t);
            }

            inline float2 GetCurvedPoint(float x)
            {
                return float2(x, BinarySearchInterpolate(x)) * float2(_Amplitude, _Period);
            }

            float2 TransformedPoint(float x, float y, float precision)
            {
                float2 result;
                float2 centerPoint = GetCurvedPoint(x);
                float width = _Width * (y > 0 ? 1 : -1);

                if (x < 1e-5f)
                {
                    float2 nextPoint = GetCurvedPoint(x + precision);
                    float2 direction = nextPoint - centerPoint;
                    float2 normal = normalize(float2(-direction.y, direction.x));
                    result = centerPoint + width * normal;
                }
                else if (x > 1 - 1e-5f)
                {
                    float2 lastPoint = GetCurvedPoint(x - precision);
                    float2 direction = centerPoint - lastPoint;
                    float2 normal = normalize(float2(-direction.y, direction.x));
                    result = centerPoint + width * normal;
                }
                else
                {
                    float2 lastPoint = GetCurvedPoint(x - precision);
                    float2 nextPoint = GetCurvedPoint(x + precision);
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