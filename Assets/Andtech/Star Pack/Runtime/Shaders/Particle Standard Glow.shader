
Shader "Andtech/Star Pack/Particle Standard Glow" {
	Properties{
		[NoScaleOffset]
		_MainTex("Glow Mask", 2D) = "white" {}
		_Brightness("Brightness", Range(0, 1)) = 1
		_InvFade("Soft Particles Factor", Range(0.01, 3)) = 1
		_TwinkleAmount("Twinkle Amount", Range(0, 1)) = 0.8
		_TwinkleSpeed("Twinkle Speed", Range(1, 10)) = 5
	}

	Category{
		Tags {
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
			"PreviewType" = "Plane"
			"IgnoreProjector" = "True"
		}

		Blend SrcAlpha One
		Cull Off
		ZWrite Off
		Lighting Off
		ColorMask RGB

		SubShader {
			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#pragma shader_feature TWINKLE_ON

				#include "UnityCG.cginc"

				struct appdata {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float3 uv : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
				};

				sampler2D _MainTex;
				sampler2D_float _CameraDepthTexture;
				float _InvFade;
				fixed _Brightness;
				fixed _TwinkleAmount;
				fixed _TwinkleSpeed;

				v2f vert(appdata v) {
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.uv = v.uv.xy;
#ifdef SOFTPARTICLES_ON
					o.projPos = ComputeScreenPos(o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);
#endif
#if TWINKLE_ON
					o.color.a *= 1 + _TwinkleAmount * cos((_TwinkleSpeed * (_Time[1] + v.uv.z)) * 6.28);
#endif
					UNITY_TRANSFER_FOG(o, o.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target{
					fixed4 mask = tex2D(_MainTex, i.uv);
					fixed4 col = mask.r * i.color + mask.g * _Brightness;
					col.a = i.color.a * mask.a;
#ifdef SOFTPARTICLES_ON
					float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
					float partZ = i.projPos.z;
					float fade = saturate(_InvFade * (sceneZ - partZ));
					col.a *= fade;
#endif
					UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0, 0, 0, 0));

					return col;
				}
				ENDCG
			}
		}
	}

	Fallback "VertexLit"
	CustomEditor "Andtech.StarPack.Editor.ParticleStandardGlowShaderGUI"
}