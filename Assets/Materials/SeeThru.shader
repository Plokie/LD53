Shader "Custom/SeeThru"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    // SubShader {
    //     ZWrite On
    //     ZTest Greater
    //     Lighting Off
        
    //     CGPROGRAM

    //     // Physically based Standard lighting model, and enable shadows on all light types
    //     #pragma surface surf Standard 

    //     // Use shader model 3.0 target, to get nicer looking lighting
    //     #pragma target 3.0

    //     sampler2D _MainTex;

    //     struct Input
    //     {
    //         float2 uv_MainTex;
    //     };

    //     half _Glossiness;
    //     half _Metallic;
    //     fixed4 _Color;

    //     void surf (Input IN, inout SurfaceOutputStandard o)
    //     {
    //         // Albedo comes from a texture tinted by color
    //         fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
    //         o.Albedo = c.rgb;
    //         // Metallic and smoothness come from slider variables
    //         o.Metallic = _Metallic;
    //         o.Smoothness = _Glossiness;
    //         o.Alpha = c.a;
    //     }
    //     ENDCG
    // }
    SubShader
    {
        Tags { "Queue"="Geometry+1" "RenderType"="Transparent" }

        Pass
        {
            ZWrite On
            ZTest Greater
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha
            // Blend One OneMinusSrcAlpha
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            uniform fixed4 ObjectColor;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = ObjectColor;
                o.worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                if (i.worldPos.y < 0.0) discard;
                return _Color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
