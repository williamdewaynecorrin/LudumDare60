Shader "LD60/CustomToon"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TintColor ("Tint", Color) = (1, 1, 1, 1)
        _AmbientColor ("Ambient Color", Color) = (0.1, 0.1, 0.1, 1)
        _Alpha("Alpha", Range(0, 1)) = 1
        _BandsCount("Bands Count", Range(2, 4)) = 2
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderType"="ForwardBase" 
            "PassFlags"="OnlyDirectional" 
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldNormal : NORMAL;

                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TintColor;
            float4 _AmbientColor;
            fixed _Alpha;
            int _BandsCount;

            // -- declarations
            uniform float4 _LightColor0;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f IN) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, IN.uv);
                float ndotl = dot(IN.worldNormal, _WorldSpaceLightPos0);

                // -- bands instead of straight up cel
                float shade = 0;
                float step = 1.0 / (float)_BandsCount;

                float curstep = 0;

                [unroll(4)]
                for(int i = 0; i < _BandsCount; ++i)
                {
                    if(ndotl <= curstep)
                    {
                        break;
                    }

                    curstep += step;
                    shade += step;
                }

                float ambient = ShadeSH9(float4(IN.worldNormal, 1.0));

                float3 finallight = lerp(ambient + _AmbientColor, _LightColor0.rgb, shade);
                return float4(col.rgb * _TintColor.rgb * finallight, _Alpha);
            }
            ENDCG
        }
    }
}