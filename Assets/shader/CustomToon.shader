Shader "LD60/CustomToon"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TintColor ("Tint", Color) = (1, 1, 1, 1)
        _AmbientColor ("Ambient Color", Color) = (0.1, 0.1, 0.1, 1)
        _Alpha("Alpha", Range(0, 1)) = 1
        _BandsCount("Bands Count", Range(2, 4)) = 2
        _BandBlendMin("Band Blend (Min)", Range(0, 1)) = 0
        _BandBlendMax("Band Blend (Max)", Range(0, 1)) = 0.01
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="ForwardBase" 
            "PassFlags"="OnlyDirectional" 
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : NORMAL;
                float2 uv : TEXCOORD0;
                float3 viewDir : TEXCOORD1;

                UNITY_SHADOW_COORDS(2)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TintColor;
            float4 _AmbientColor;
            fixed _Alpha;
            fixed _BandBlendMin;
            fixed _BandBlendMax;
            int _BandsCount;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = WorldSpaceViewDir(v.vertex);

                UNITY_TRANSFER_SHADOW(o, o.pos);
                return o;
            }
            
            fixed4 frag (v2f IN) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, IN.uv);
                float ndotl = dot(IN.worldNormal, _WorldSpaceLightPos0);

                // -- bands instead of straight up cel
                float shade = 0;
                float step = 1.0 / (float)_BandsCount;

                [unroll(4)]
                for(int i = 0; i < _BandsCount; ++i)
                {
                    if(ndotl <= shade)
                    {
                        break;
                    }

                    shade += step;
                }

                shade = smoothstep(_BandBlendMin, _BandBlendMax, shade);

                // -- shadows and ambient
                // float shadow = UNITY_SHADOW_ATTENUATION(IN);
                float ambient = ShadeSH9(float4(IN.worldNormal, 1.0));

                float3 finallight = lerp(ambient + _AmbientColor, _LightColor0.rgb, shade);
                return float4(col.rgb * _TintColor.rgb * finallight, _Alpha);
            }
            ENDCG
        }

        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}