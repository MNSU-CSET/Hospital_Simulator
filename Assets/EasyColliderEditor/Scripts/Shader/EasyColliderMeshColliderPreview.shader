// just a simple transparent color shader.
Shader "Custom/EasyColliderMeshColliderPreview" {
    Properties {
        _Color("Main Color", Color) = (1,1,1,1)
    }
    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        // For single convex-hulls so we can view them inside of the mesh with the new mesh offsets.
        Pass {
          ZTest always
          CGPROGRAM
              #pragma vertex vert
              #pragma fragment frag
              #pragma target 2.0
  
              #include "UnityCG.cginc"

              struct appdata_t {
                  float4 vertex : POSITION;
                  float4 normal: NORMAL;
              };
  
              struct v2f {
                  float4 vertex : SV_POSITION;
              };
  
              half4 _Color;
  
              v2f vert (appdata_t v)
              {
                v2f o;
                //world space vert
                o.vertex = mul(unity_ObjectToWorld, v.vertex);
                // world space normal
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                // offset by a small amount (1mm) (to prevent z-clipping)
                o.vertex.xyz += worldNormal * 0.001f;
                // back to to world space
                o.vertex = mul(unity_WorldToObject, o.vertex);
                // object to clip position
                o.vertex = UnityObjectToClipPos(o.vertex);
                return o;
              }
                  
                
  
              fixed4 frag (v2f i) : SV_Target
              {
                // all mesh-collider previews have an alpha value that can be changed if you wish.
                _Color.a = 0.8f;
                return _Color;
              }
          ENDCG
        }
        //VHACD pass, normal z-testing.
        Pass {
          CGPROGRAM
              #pragma vertex vert
              #pragma fragment frag
              #pragma target 2.0
  
              #include "UnityCG.cginc"

              struct appdata_t {
                  float4 vertex : POSITION;
                  float4 normal: NORMAL;
              };
  
              struct v2f {
                  float4 vertex : SV_POSITION;
              };
  
              half4 _Color;
  
              v2f vert (appdata_t v)
              {
                v2f o;
                //world space vert
                o.vertex = mul(unity_ObjectToWorld, v.vertex);
                // world space normal
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                // offset by a small amount (1mm) (to prevent z-clipping)
                o.vertex.xyz += worldNormal * 0.001f;
                // back to to world space
                o.vertex = mul(unity_WorldToObject, o.vertex);
                // object to clip position
                o.vertex = UnityObjectToClipPos(o.vertex);
                return o;
              }
                  
                
  
              fixed4 frag (v2f i) : SV_Target
              {
                // all mesh-collider previews have an alpha value that can be changed if you wish.
                _Color.a = 0.8f;
                return _Color;
              }
          ENDCG
        }
    }
 }