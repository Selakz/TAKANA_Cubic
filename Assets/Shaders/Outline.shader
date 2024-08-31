Shader "Custom/Outline" {
    Properties {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01
    }
 
    SubShader {
        Tags { "Queue"="Transparent" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            fixed4 _OutlineColor;
            float _OutlineWidth;
            sampler2D _MainTex;
 
            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
 
            half4 frag (v2f i) : SV_Target {
                half4 col = tex2D(_MainTex, i.uv);
                half4 outline = col.a * _OutlineColor;
                outline.a = step(_OutlineWidth, outline.a);
                return outline;
            }
            ENDCG
        }
    }
}
