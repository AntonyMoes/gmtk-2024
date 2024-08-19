//Non-UV based tiling shader
Shader "Custom/S_Triplanar"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "black" {}
        _SecondaryTex ("Secondary Albedo", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Scale("Scale", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _MaskTex;
        sampler2D _SecondaryTex;
        float _Scale;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            float3 worldPos = IN.worldPos / _Scale;
            float3 normal = IN.worldNormal;
            float3 blends = abs(normal);
            blends /= blends.x + blends.y + blends.z;
            
            float3 projX = tex2D(_MainTex, worldPos.yz) * blends.x;
            float3 projY = tex2D(_MainTex, worldPos.xz) * blends.y;
            float3 projZ = tex2D(_MainTex, worldPos.xy) * blends.z;
            float3 albedo = projX + projY + projZ;
            
            float3 projMX = tex2D(_MaskTex, worldPos.yz) * blends.x;
            float3 projMY = tex2D(_MaskTex, worldPos.xz) * blends.y;
            float3 projMZ = tex2D(_MaskTex, worldPos.xy) * blends.z;
            float3 mask = projMX + projMY + projMZ;

            float3 proj1X = tex2D(_SecondaryTex, worldPos.yz) * blends.x;
            float3 proj1Y = tex2D(_SecondaryTex, worldPos.xz) * blends.y;
            float3 proj1Z = tex2D(_SecondaryTex, worldPos.xy) * blends.z;
            float3 albedo1 = proj1X + proj1Y + proj1Z;

            o.Albedo = albedo * (1 - mask) + albedo1 * mask;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

