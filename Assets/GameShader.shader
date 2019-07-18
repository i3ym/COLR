Shader "Unlit/GameShader"
{
    Properties
    {
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

            float2 _Sizes = float2(30, 10); // Meteor, Bullet

            float2 _Counts = float2(0, 0); // meteors count | bullets count
            float2 _Meteors[100];
            float4 _MeteorColors[100];
            float2 _Bullets[50];

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f v) : SV_Target
            {
                for (int i = 0; i < _Counts.x; i++)
                    if (_Meteors[i].x + _Sizes.x > v.vertex.x && _Meteors[i].x - _Sizes.x < v.vertex.x && _Meteors[i].y + _Sizes.x > v.vertex.y && _Meteors[i].y - _Sizes.x < v.vertex.y)
                        return _MeteorColors[i];
                        
                for (int i = 0; i < _Counts.y; i++)
                    if (_Bullets[i].x + _Sizes.y > v.vertex.x && _Bullets[i].x - _Sizes.y < v.vertex.x && _Bullets[i].y + _Sizes.y > v.vertex.y && _Bullets[i].y - _Sizes.y < v.vertex.y)
                        return float4(1, 1, 1, 1);

                discard;
                return float4(0, 0, 0, 1);
            }
            ENDCG
        }
    }
}
