Shader "BlendSkin/Standard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 4.5

        sampler2D _MainTex;

#if defined(SHADER_API_D3D11) || defined(SHADER_API_GLCORE) || defined(SHADER_API_METAL) || defined(SHADER_API_VULKAN) || defined(SHADER_API_PS4) || defined(SHADER_API_XBOXONE)
    #define HAS_STRUCTURED_BUFFER
#endif

#ifdef HAS_STRUCTURED_BUFFER
        StructuredBuffer<float3> _Points;
        StructuredBuffer<float3> _Normals;
        StructuredBuffer<float4> _Tangents;
#endif

        struct ia_out {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
            float4 texcoord1 : TEXCOORD1;
            float4 texcoord2 : TEXCOORD2;
            float4 texcoord3 : TEXCOORD3;
            fixed4 color : COLOR;
#ifdef HAS_STRUCTURED_BUFFER
            uint vertexID : SV_VertexID;
#endif
        };

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void vert(inout ia_out v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);

#ifdef HAS_STRUCTURED_BUFFER
            uint vi = v.vertexID;
            v.vertex.xyz = _Points[vi];
            v.normal = _Normals[vi];
            v.tangent = _Tangents[vi];
#endif
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
}
