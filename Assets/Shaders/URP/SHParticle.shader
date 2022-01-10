Shader "Custom/Universal Render Pipeline/SHParticle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AlphaExponent ("Alpha Exponent", Float) = 0.8
        [HideInInspector] _MinPosition ("_MinPosition", Vector) = (0,0,0,0)
        [HideInInspector] _MaxPosition ("_MinPosition", Vector) = (0,0,0,0)
        [HideInInspector] _ShTex ("SH Texture", 3D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalRenderPipeline" }
        LOD 100
        Blend SrcAlpha One
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION; // 頂点座標
                half4 color : COLOR; // 頂点カラー
                float4 texCoord0 : TEXCOORD0; 
                float4 texCoord1 : TEXCOORD1;
            };

            struct v2f
            {
                float2 texCoord : TEXCOORD0; 
                half4 color : TEXCOORD1;
                float4 positionHCS : SV_POSITION; // clip space position
            };

            CBUFFER_START(UnityPerMaterial)
                sampler2D _MainTex;
                sampler3D _ShTex;
                float4 _MainTex_ST;
                float3 _MinPosition;
                float3 _MaxPosition;
                half _AlphaExponent;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.texCoord = TRANSFORM_TEX(v.texCoord0.xy, _MainTex);

                // パーティクル中心座標 (Custom Vertex Streamsで指定した座標を取り出す)
                float3 worldPosition = float3(v.texCoord0.zw, v.texCoord1.x); 
                float4 shCoord = 0;
                shCoord.xyz = (worldPosition - _MinPosition) / (_MaxPosition - _MinPosition); // テクスチャ座標を復元

                // 0 ~ 1の範囲に収める
                shCoord = saturate(shCoord);

                // ライト情報を取り出す
                half4 light = tex3Dlod(_ShTex, shCoord); // 頂点テクスチャフェッチ(VTF)
                light.a = (light.x + light.y + light.z) / 3.0;
                light.a = saturate(light.a);
                light.a = pow(light.a, _AlphaExponent);
                o.color = v.color * light;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 c = tex2D(_MainTex, i.texCoord);
                c *= i.color;
                return c;
            }
            ENDHLSL
        }
    }
}
