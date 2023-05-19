// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Blood/BloodDecalShader"
{
	Properties
	{
		_MainTexture("MainTexture", 2D) = "white" {}
		_TrasparencyPower("TrasparencyPower", Range( 0.1 , 1)) = 0.6
		_MainColorTintFadeA("MainColorTint-Fade(A)", Color) = (0.9558824,0.9277682,0.9277682,1)
		_PreDesaturate("PreDesaturate", Range( 0 , 1)) = 0
		_PreContrast("PreContrast", Range( 0 , 1)) = 0
		_Gloss("Gloss", Range( 0 , 1)) = 0.7
		_Metallic("Metallic", Range( 0 , 1)) = 0.25
		_MainNormalMap("MainNormalMap", 2D) = "bump" {}
		_Noise1("Noise1", Range( 0 , 50)) = 0
		_Noise2("Noise2", Range( 0 , 25)) = 3
		_Scale("Scale", Range( 0 , 1)) = 0
		_Bias("Bias", Range( 0 , 1)) = 0
		_DriedBloodEffectLevel("DriedBloodEffectLevel", Range( 0 , 1)) = 0.5
		_DriedBlood_Curvature("DriedBlood_Curvature", 2D) = "white" {}
		_DriedBlood_Normal("DriedBlood_Normal", 2D) = "bump" {}
		_DriedBloodColorGloss("DriedBloodColor-Gloss", Color) = (0.083045,0.08415466,0.08823532,0)
		_DriedBloodEffect_Tiling("DriedBloodEffect_Tiling", Range( 0 , 1000)) = 0
		_DriedBlood_Metallic("DriedBlood_Metallic", Range( 0 , 1)) = 0.5
		_DriedOpacityLevel("DriedOpacityLevel", Range( 0 , 5)) = 0
		_OverallNormalPower("OverallNormalPower", Range( 0 , 4)) = 0.8
		_Noise_Texture("Noise_Texture", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _MainNormalMap;
		uniform float4 _MainNormalMap_ST;
		uniform sampler2D _DriedBlood_Normal;
		uniform float _DriedBloodEffect_Tiling;
		uniform float _DriedBloodEffectLevel;
		uniform float _OverallNormalPower;
		uniform float _Noise1;
		uniform float _Noise2;
		uniform sampler2D _Noise_Texture;
		uniform float4 _Noise_Texture_ST;
		uniform float _Bias;
		uniform float _Scale;
		uniform float4 _MainColorTintFadeA;
		uniform sampler2D _MainTexture;
		uniform float4 _MainTexture_ST;
		uniform float _PreDesaturate;
		uniform float _PreContrast;
		uniform float4 _DriedBloodColorGloss;
		uniform sampler2D _DriedBlood_Curvature;
		uniform float _Metallic;
		uniform float _DriedBlood_Metallic;
		uniform float _Gloss;
		uniform float _TrasparencyPower;
		uniform float _DriedOpacityLevel;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_MainNormalMap = i.uv_texcoord * _MainNormalMap_ST.xy + _MainNormalMap_ST.zw;
			float2 temp_cast_0 = (_DriedBloodEffect_Tiling).xx;
			float2 uv_TexCoord38 = i.uv_texcoord * temp_cast_0;
			float3 lerpResult45 = lerp( UnpackNormal( tex2D( _MainNormalMap, uv_MainNormalMap ) ) , UnpackNormal( tex2D( _DriedBlood_Normal, uv_TexCoord38 ) ) , _DriedBloodEffectLevel);
			float3 lerpResult53 = lerp( float3(0,0,1) , lerpResult45 , _OverallNormalPower);
			o.Normal = lerpResult53;
			float2 temp_cast_1 = (_Noise1).xx;
			float2 uv_TexCoord10 = i.uv_texcoord * temp_cast_1;
			float simplePerlin2D7 = snoise( uv_TexCoord10 );
			float2 temp_cast_2 = (( _Noise1 * _Noise2 )).xx;
			float2 uv_TexCoord13 = i.uv_texcoord * temp_cast_2;
			float simplePerlin2D12 = snoise( uv_TexCoord13 );
			float2 uv_Noise_Texture = i.uv_texcoord * _Noise_Texture_ST.xy + _Noise_Texture_ST.zw;
			float4 saferPower55 = max( ( ( ( 1.0 - ( simplePerlin2D7 * simplePerlin2D12 * tex2D( _Noise_Texture, uv_Noise_Texture ) ) ) + _Bias ) * _Scale ) , 0.0001 );
			float4 temp_cast_3 = (( 3.0 * _Bias )).xxxx;
			float temp_output_2_0_g2 = _DriedBloodEffectLevel;
			float temp_output_3_0_g2 = ( 1.0 - temp_output_2_0_g2 );
			float3 appendResult7_g2 = (float3(temp_output_3_0_g2 , temp_output_3_0_g2 , temp_output_3_0_g2));
			float3 temp_output_67_0 = ( ( pow( saferPower55 , temp_cast_3 ).rgb * temp_output_2_0_g2 ) + appendResult7_g2 );
			float3 temp_cast_5 = (1.0).xxx;
			float3 clampResult59 = clamp( temp_output_67_0 , float3( 0,0,0 ) , temp_cast_5 );
			float2 uv_MainTexture = i.uv_texcoord * _MainTexture_ST.xy + _MainTexture_ST.zw;
			float4 tex2DNode1 = tex2D( _MainTexture, uv_MainTexture );
			float3 desaturateInitialColor72 = tex2DNode1.rgb;
			float desaturateDot72 = dot( desaturateInitialColor72, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar72 = lerp( desaturateInitialColor72, desaturateDot72.xxx, _PreDesaturate );
			float3 temp_output_74_0 = ( desaturateVar72 + desaturateVar72 + desaturateVar72 + desaturateVar72 );
			float3 lerpResult78 = lerp( desaturateVar72 , saturate( ( temp_output_74_0 * temp_output_74_0 ) ) , _PreContrast);
			float4 blendOpSrc24 = float4( clampResult59 , 0.0 );
			float4 blendOpDest24 = ( _MainColorTintFadeA * float4( lerpResult78 , 0.0 ) );
			float4 blendOpSrc36 = _DriedBloodColorGloss;
			float4 blendOpDest36 = tex2D( _DriedBlood_Curvature, uv_TexCoord38 );
			float3 temp_output_60_0 = ( clampResult59 * _DriedBloodEffectLevel );
			float4 lerpResult34 = lerp( ( saturate( (( blendOpDest24 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest24 ) * ( 1.0 - blendOpSrc24 ) ) : ( 2.0 * blendOpDest24 * blendOpSrc24 ) ) )) , (( blendOpDest36 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest36 ) * ( 1.0 - blendOpSrc36 ) ) : ( 2.0 * blendOpDest36 * blendOpSrc36 ) ) , float4( temp_output_60_0 , 0.0 ));
			o.Albedo = lerpResult34.rgb;
			float lerpResult49 = lerp( _Metallic , _DriedBlood_Metallic , temp_output_60_0.x);
			o.Metallic = lerpResult49;
			float lerpResult46 = lerp( _Gloss , _DriedBloodColorGloss.a , temp_output_60_0.x);
			o.Smoothness = lerpResult46;
			float3 saferPower26 = max( ( temp_output_67_0 * pow( tex2DNode1.a , _TrasparencyPower ) ) , 0.0001 );
			float3 temp_cast_13 = (1.0).xxx;
			float3 clampResult25 = clamp( pow( saferPower26 , 6.0 ) , float3( 0,0,0 ) , temp_cast_13 );
			float3 temp_output_27_0 = ( clampResult25 * _MainColorTintFadeA.a );
			float3 lerpResult65 = lerp( temp_output_27_0 , ( temp_output_27_0 * _DriedOpacityLevel ) , _DriedBloodEffectLevel);
			o.Alpha = lerpResult65.x;
		}

		ENDCG
		CGPROGRAM
		#pragma exclude_renderers xboxseries playstation switch nomrt 
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18912
-82;624;2397;1099;1071.238;669.9827;1.051767;True;True
Node;AmplifyShaderEditor.RangedFloatNode;11;-1776.535,-693.7427;Float;False;Property;_Noise1;Noise1;8;0;Create;True;0;0;0;False;0;False;0;21.6;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1801.91,-589.9119;Float;False;Property;_Noise2;Noise2;9;0;Create;True;0;0;0;False;0;False;3;9.2;0;25;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-1376.99,-600.8535;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;13;-1135.254,-582.3997;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-1157.807,-728.2891;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;7;-799.4476,-721.9736;Inherit;False;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;63;-1573.12,-391.4929;Inherit;True;Property;_Noise_Texture;Noise_Texture;20;0;Create;True;0;0;0;False;0;False;-1;None;512a2c50bd2f7c643bba3890aa293ad8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;12;-819.1803,-627.0765;Inherit;False;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-491.4413,-664.6424;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-389.9713,-551.8475;Float;False;Property;_Bias;Bias;11;0;Create;True;0;0;0;False;0;False;0;0.07;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;18;-180.9798,-671.8869;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-399.2943,-476.1721;Float;False;Property;_Scale;Scale;10;0;Create;True;0;0;0;False;0;False;0;0.136;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-865,-215.5;Inherit;True;Property;_MainTexture;MainTexture;0;0;Create;True;0;0;0;False;0;False;-1;None;308ca4ba1cb8acb49858ceb0a3a2d4b4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;73;-517.3271,-145.1171;Inherit;False;Property;_PreDesaturate;PreDesaturate;3;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;19;-13.50874,-567.6967;Inherit;False;ConstantBiasScale;-1;;1;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;COLOR;0,0,0,0;False;1;FLOAT;0.1;False;2;FLOAT;1.25;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;19.53736,-692.769;Inherit;False;2;2;0;FLOAT;3;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;55;255.7104,-665.3391;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.DesaturateOpNode;72;-510.9178,-320.0795;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;70;-753.1951,21.89014;Float;False;Property;_TrasparencyPower;TrasparencyPower;1;0;Create;True;0;0;0;False;0;False;0.6;0.147;0.1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-13.69085,-122.3695;Float;False;Property;_DriedBloodEffectLevel;DriedBloodEffectLevel;12;0;Create;True;0;0;0;False;0;False;0.5;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;69;-411.1951,-50.10986;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;74;-325.2682,-295.6172;Inherit;False;4;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;67;352.9132,-524.514;Inherit;False;Lerp White To;-1;;2;047d7c189c36a62438973bad9d37b1c2;0;2;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;415.0267,-367.0195;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;-127.5033,-275.4407;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;77;-249.5033,-194.4407;Inherit;False;Property;_PreContrast;PreContrast;4;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;26;596.8022,-389.5192;Inherit;False;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;6;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;76;-9.503296,-268.4407;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;79;440.5036,-250.556;Inherit;False;Constant;_Float0;Float 0;22;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-801.1509,332.2745;Float;False;Property;_DriedBloodEffect_Tiling;DriedBloodEffect_Tiling;16;0;Create;True;0;0;0;False;0;False;0;154;0;1000;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;38;-414.6653,316.8217;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;78;114.4967,-289.4407;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;2;-128.3676,-439.0108;Float;False;Property;_MainColorTintFadeA;MainColorTint-Fade(A);2;0;Create;True;0;0;0;False;0;False;0.9558824,0.9277682,0.9277682,1;0.1102941,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;25;793.9135,-288.8587;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;30;76.26976,208.7128;Inherit;True;Property;_DriedBlood_Curvature;DriedBlood_Curvature;13;0;Create;True;0;0;0;False;0;False;-1;None;308ca4ba1cb8acb49858ceb0a3a2d4b4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;31;93.4674,455.4361;Inherit;True;Property;_DriedBlood_Normal;DriedBlood_Normal;14;0;Create;True;0;0;0;False;0;False;-1;None;004c57b0cd4b5f84d8165ef5658d8ca4;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-295.6611,104.7148;Inherit;True;Property;_MainNormalMap;MainNormalMap;7;0;Create;True;0;0;0;False;0;False;-1;None;004c57b0cd4b5f84d8165ef5658d8ca4;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;35;89.04,25.6489;Float;False;Property;_DriedBloodColorGloss;DriedBloodColor-Gloss;15;0;Create;True;0;0;0;False;0;False;0.083045,0.08415466,0.08823532,0;0.1176471,0.05371674,0.01903114,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;59;552.5435,-695.8274;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;984.1426,-212.8668;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;64;1788.009,352.2007;Float;False;Property;_DriedOpacityLevel;DriedOpacityLevel;18;0;Create;True;0;0;0;False;0;False;0;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;151.5066,-393.128;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;29;1196.652,-293.1428;Float;False;Property;_Metallic;Metallic;6;0;Create;True;0;0;0;False;0;False;0.25;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;1031.557,-506.9965;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;48;1431.811,-235.9101;Float;False;Property;_DriedBlood_Metallic;DriedBlood_Metallic;17;0;Create;True;0;0;0;False;0;False;0.5;0.275;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;36;501.5292,82.17787;Inherit;True;Overlay;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;28;1279.782,-48.37531;Float;False;Property;_Gloss;Gloss;5;0;Create;True;0;0;0;False;0;False;0.7;0.94;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;54;1869.155,-10.91348;Float;False;Constant;_Vector0;Vector 0;15;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.BlendOpsNode;24;822.3588,-655.3137;Inherit;False;Overlay;True;3;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;52;1789.844,170.389;Float;False;Property;_OverallNormalPower;OverallNormalPower;19;0;Create;True;0;0;0;False;0;False;0.8;0.32;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;2162.269,271.5419;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;45;1177.034,61.08569;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-160.8493,-932.7679;Float;False;Property;_NoiseInFreshBlood;NoiseInFreshBlood;21;0;Create;True;0;0;0;False;0;False;0;0.465;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;46;1664.826,-142.7851;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;65;2508.326,111.2945;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;49;1725.874,-256.7047;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;53;2176.358,30.26415;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;34;1425.697,-621.0692;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2829.506,-232.972;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Blood/BloodDecalShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;d3d9;d3d11_9x;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;ps4;psp2;n3ds;wiiu;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;14;0;11;0
WireConnection;14;1;16;0
WireConnection;13;0;14;0
WireConnection;10;0;11;0
WireConnection;7;0;10;0
WireConnection;12;0;13;0
WireConnection;17;0;7;0
WireConnection;17;1;12;0
WireConnection;17;2;63;0
WireConnection;18;0;17;0
WireConnection;19;3;18;0
WireConnection;19;1;21;0
WireConnection;19;2;20;0
WireConnection;56;1;21;0
WireConnection;55;0;19;0
WireConnection;55;1;56;0
WireConnection;72;0;1;0
WireConnection;72;1;73;0
WireConnection;69;0;1;4
WireConnection;69;1;70;0
WireConnection;74;0;72;0
WireConnection;74;1;72;0
WireConnection;74;2;72;0
WireConnection;74;3;72;0
WireConnection;67;1;55;0
WireConnection;67;2;43;0
WireConnection;22;0;67;0
WireConnection;22;1;69;0
WireConnection;75;0;74;0
WireConnection;75;1;74;0
WireConnection;26;0;22;0
WireConnection;76;0;75;0
WireConnection;38;0;39;0
WireConnection;78;0;72;0
WireConnection;78;1;76;0
WireConnection;78;2;77;0
WireConnection;25;0;26;0
WireConnection;25;2;79;0
WireConnection;30;1;38;0
WireConnection;31;1;38;0
WireConnection;59;0;67;0
WireConnection;59;2;79;0
WireConnection;27;0;25;0
WireConnection;27;1;2;4
WireConnection;3;0;2;0
WireConnection;3;1;78;0
WireConnection;60;0;59;0
WireConnection;60;1;43;0
WireConnection;36;0;35;0
WireConnection;36;1;30;0
WireConnection;24;0;59;0
WireConnection;24;1;3;0
WireConnection;66;0;27;0
WireConnection;66;1;64;0
WireConnection;45;0;4;0
WireConnection;45;1;31;0
WireConnection;45;2;43;0
WireConnection;46;0;28;0
WireConnection;46;1;35;4
WireConnection;46;2;60;0
WireConnection;65;0;27;0
WireConnection;65;1;66;0
WireConnection;65;2;43;0
WireConnection;49;0;29;0
WireConnection;49;1;48;0
WireConnection;49;2;60;0
WireConnection;53;0;54;0
WireConnection;53;1;45;0
WireConnection;53;2;52;0
WireConnection;34;0;24;0
WireConnection;34;1;36;0
WireConnection;34;2;60;0
WireConnection;0;0;34;0
WireConnection;0;1;53;0
WireConnection;0;3;49;0
WireConnection;0;4;46;0
WireConnection;0;9;65;0
ASEEND*/
//CHKSM=5A2A67E4352DF07B2B370358920D630659B1D926