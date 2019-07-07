Shader "Unlit/Background"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float noise(float2 x)
            {
                float xhash = cos( x.x * 37.0 );
                float yhash = cos( x.y * 57.0 );
                return frac( 415.92653 * ( xhash + yhash ) );
            }
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 clr = tex2D(_MainTex, _Time.xy / 100 + i.uv * _ScreenParams.xy / 512);
                if (clr.x == 0 && rand(i.uv) > .9) return float4(1, 1, 1, 1);

                return float4(0, 0, 0, 1);
            }

            ENDCG
        }
    }
}
