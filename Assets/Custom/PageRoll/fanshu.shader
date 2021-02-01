Shader "Custom/PageRoll"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SecondTex ("SecondTex", 2D) = "white" {}
        _Angle("Angle",Range(0,180))= 180
        _WaveLength("WaveLength",Range(1,10))=6.74

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Back

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

            sampler2D _SecondTex;
            float4 _SecondTex_ST;
            float _Angle;
            float _WaveLength;

            v2f vert (appdata v)
            {
                v2f o;
                float s;
                float c;
                //通过该方法可以计算出该角度的正余弦值
                sincos(radians(_Angle),s,c);
                //旋转矩阵
                float4x4 rotateMatrix={ 
                    c ,s,0,0,
                    -s,c,0,0,
                    0 ,0,1,0,
                    0 ,0,0,1
                };
                v.vertex -= float4(5,0,0,0);

                float left = sin(v.vertex.x*_WaveLength) * s ;
                float right = cos(v.vertex.x*_WaveLength) * (-c-1) ;
                float factor = smoothstep(88,92,_Angle);
                v.vertex.y = right*factor + (1-factor)*left;
      
                //顶点左乘以旋转矩阵
                v.vertex = mul(rotateMatrix,v.vertex);
                v.vertex += float4(5,0,0,0);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _SecondTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_SecondTex, i.uv);

                return col;
            }
            ENDCG
        }
        Pass
        {
            Cull Front

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
            float _Angle;
            float _WaveLength;

            v2f vert (appdata v)
            {
                v2f o;
                float s;
                float c;
                //通过该方法可以计算出该角度的正余弦值
                sincos(radians(_Angle),s,c);
                //旋转矩阵
                float4x4 rotateMatrix={ 
                    c ,s,0,0,
                    -s,c,0,0,
                    0 ,0,1,0,
                    0 ,0,0,1
                };
                v.vertex -= float4(5,0,0,0);

                float left = sin(v.vertex.x*_WaveLength) * s ;
                float right = cos(v.vertex.x*_WaveLength) * (-c-1) ;
                float factor = smoothstep(88,92,_Angle);
                v.vertex.y = right*factor + (1-factor)*left;
        
                //顶点左乘以旋转矩阵
                v.vertex = mul(rotateMatrix,v.vertex);
                v.vertex += float4(5,0,0,0);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
    FallBack  "Transparent/Cutout/VertexLit"
}
