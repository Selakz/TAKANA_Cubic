Shader "Unlit/Any Plane"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Precision ("Precision", Float) = 0.01
        _LeftOffset ("Left Offset", Float) = 0
        _RightOffset ("Right Offset", Float) = 0
        _LeftPeriod ("Left Period", Float) = 1
        _RightPeriod ("Right Period", Float) = 1
        _LeftAmplitude ("Left Amplitude", Float) = 1
        _RightAmplitude ("Right Amplitude", Float) = 1
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
            ZWrite Off

            CGPROGRAM
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
            float _Precision;
            float _LeftOffset;
            float _RightOffset;
            float _LeftPeriod;
            float _RightPeriod;
            float _LeftAmplitude;
            float _RightAmplitude;
            float4 _MainTex_ST;
            float4 _LeftSamplePointArray[50];
            float4 _RightSamplePointArray[50];
            int _SamplePointCount;

            float2 GetLeftPoint(int index)
            {
                float4 raw = _LeftSamplePointArray[index >> 1];
                return (index & 1) == 0 ? raw.xy : raw.zw;
            }

            float2 GetRightPoint(int index)
            {
                float4 raw = _RightSamplePointArray[index >> 1];
                return (index & 1) == 0 ? raw.xy : raw.zw;
            }

            float BinarySearchInterpolateLeft(float x)
            {
                if (x <= GetLeftPoint(0).x) return GetLeftPoint(0).y;
                if (x >= GetLeftPoint(_SamplePointCount - 1).x) return GetLeftPoint(_SamplePointCount - 1).y;

                int low = 0;
                int high = _SamplePointCount - 1;
                [unroll(8)]
                while (low <= high)
                {
                    int mid = (low + high) / 2;
                    float midX = GetLeftPoint(mid).x;

                    if (midX < x)
                        low = mid + 1;
                    else
                        high = mid - 1;
                }

                float2 pLeft = GetLeftPoint(high);
                float2 pRight = GetLeftPoint(low);
                float t = (x - pLeft.x) / (pRight.x - pLeft.x);
                return lerp(pLeft.y, pRight.y, t);
            }

            float BinarySearchInterpolateRight(float x)
            {
                if (x <= GetRightPoint(0).x) return GetRightPoint(0).y;
                if (x >= GetRightPoint(_SamplePointCount - 1).x) return GetRightPoint(_SamplePointCount - 1).y;

                int low = 0;
                int high = _SamplePointCount - 1;
                [unroll(8)]
                while (low <= high)
                {
                    int mid = (low + high) / 2;
                    float midX = GetRightPoint(mid).x;

                    if (midX < x)
                        low = mid + 1;
                    else
                        high = mid - 1;
                }

                float2 pLeft = GetRightPoint(high);
                float2 pRight = GetRightPoint(low);
                float t = (x - pLeft.x) / (pRight.x - pLeft.x);
                return lerp(pLeft.y, pRight.y, t);
            }

            inline float2 GetLeftCurvedPoint(float x)
            {
                return float2(x, BinarySearchInterpolateLeft(x)) * float2(_LeftAmplitude, _LeftPeriod) + float2(0, _LeftOffset);
            }

            inline float2 GetRightCurvedPoint(float x)
            {
                return float2(x, BinarySearchInterpolateRight(x)) * float2(_RightAmplitude, _RightPeriod) + float2(0, _RightOffset);
            }

            float2 TransformedPoint(float x, float y)
            {
                float2 leftPoint = GetLeftCurvedPoint(x);
                float2 rightPoint = GetRightCurvedPoint(x);

                bool swapped = leftPoint.y > rightPoint.y;
                return y > 0
                           ? swapped
                                 ? leftPoint
                                 : rightPoint
                           : swapped
                           ? rightPoint
                           : leftPoint;
            }

            v2f vert(appdata v)
            {
                v2f o;
                v.vertex.yx = TransformedPoint(v.vertex.y, v.vertex.x);
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