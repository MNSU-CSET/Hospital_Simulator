// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Blood/DripShader"
{
	Properties
	{
		_ColorMask("ColorMask", 2D) = "white" {}
		_Albedo("Albedo", 2D) = "white" {}
		_NormalMap("NormalMap", 2D) = "bump" {}
		_BaseColorGloss("BaseColor-Gloss", Color) = (1,1,1,0)
		_ActualFrame("ActualFrame", Range( 0 , 63.5)) = 0
		_AnimSpeed("AnimSpeed", Range( 0 , 360)) = 0
		_NormalMapPower("NormalMapPower", Range( 0 , 1)) = 1
		_EmissiveLevel("EmissiveLevel", Range( 0 , 1)) = 0
		_OpacityMaster("OpacityMaster", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _NormalMap;
		uniform float _AnimSpeed;
		uniform float _ActualFrame;
		uniform float _NormalMapPower;
		uniform float4 _BaseColorGloss;
		uniform sampler2D _Albedo;
		uniform float _EmissiveLevel;
		uniform sampler2D _ColorMask;
		uniform float _OpacityMaster;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			// *** BEGIN Flipbook UV Animation vars ***
			// Total tiles of Flipbook Texture
			float fbtotaltiles6 = 8.0 * 8.0;
			// Offsets for cols and rows of Flipbook Texture
			float fbcolsoffset6 = 1.0f / 8.0;
			float fbrowsoffset6 = 1.0f / 8.0;
			// Speed of animation
			float fbspeed6 = _Time[ 1 ] * _AnimSpeed;
			// UV Tiling (col and row offset)
			float2 fbtiling6 = float2(fbcolsoffset6, fbrowsoffset6);
			// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
			// Calculate current tile linear index
			float fbcurrenttileindex6 = round( fmod( fbspeed6 + _ActualFrame, fbtotaltiles6) );
			fbcurrenttileindex6 += ( fbcurrenttileindex6 < 0) ? fbtotaltiles6 : 0;
			// Obtain Offset X coordinate from current tile linear index
			float fblinearindextox6 = round ( fmod ( fbcurrenttileindex6, 8.0 ) );
			// Multiply Offset X by coloffset
			float fboffsetx6 = fblinearindextox6 * fbcolsoffset6;
			// Obtain Offset Y coordinate from current tile linear index
			float fblinearindextoy6 = round( fmod( ( fbcurrenttileindex6 - fblinearindextox6 ) / 8.0, 8.0 ) );
			// Reverse Y to get tiles from Top to Bottom
			fblinearindextoy6 = (int)(8.0-1) - fblinearindextoy6;
			// Multiply Offset Y by rowoffset
			float fboffsety6 = fblinearindextoy6 * fbrowsoffset6;
			// UV Offset
			float2 fboffset6 = float2(fboffsetx6, fboffsety6);
			// Flipbook UV
			half2 fbuv6 = i.uv_texcoord * fbtiling6 + fboffset6;
			// *** END Flipbook UV Animation vars ***
			float3 lerpResult12 = lerp( float3(0.5,0.5,1) , UnpackNormal( tex2D( _NormalMap, fbuv6 ) ) , _NormalMapPower);
			o.Normal = lerpResult12;
			float4 temp_output_5_0 = ( _BaseColorGloss * tex2D( _Albedo, fbuv6 ) );
			o.Albedo = temp_output_5_0.rgb;
			o.Emission = ( temp_output_5_0 * _EmissiveLevel ).rgb;
			o.Smoothness = _BaseColorGloss.a;
			o.Alpha = ( tex2D( _ColorMask, fbuv6 ).r * _OpacityMaster );
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
-24.66667;510;2397;1489;2080.79;758.6271;1.515012;True;True
Node;AmplifyShaderEditor.RangedFloatNode;7;-1447.99,50.65996;Float;False;Property;_AnimSpeed;AnimSpeed;5;0;Create;True;0;0;0;False;0;False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-1473.615,167.5359;Float;False;Property;_ActualFrame;ActualFrame;4;0;Create;True;0;0;0;False;0;False;0;63.5;0;63.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-1428.872,-134.7059;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCFlipBookUVAnimation;6;-1076.157,-8.231398;Inherit;False;0;0;6;0;FLOAT2;0,0;False;1;FLOAT;8;False;2;FLOAT;8;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;2;-493,-157;Inherit;True;Property;_Albedo;Albedo;1;0;Create;True;0;0;0;False;0;False;-1;None;2ffa475f530b74245b2fd58c8f2b44f9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-454.9999,-333.3;Float;False;Property;_BaseColorGloss;BaseColor-Gloss;3;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.2794118,0,0,0.953;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-490,47;Inherit;True;Property;_ColorMask;ColorMask;0;0;Create;True;0;0;0;False;0;False;-1;None;ef3274f7534f55643bb0f3722d5148d4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;15;-169.1802,8.309418;Float;False;Property;_EmissiveLevel;EmissiveLevel;7;0;Create;True;0;0;0;False;0;False;0;0.486;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-102,-195;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;3;-484.9998,237.7999;Inherit;True;Property;_NormalMap;NormalMap;2;0;Create;True;0;0;0;False;0;False;-1;None;71822fbdcd35bfa41baa692bf58e3a77;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;11;-420.0463,430.894;Float;False;Constant;_Vector0;Vector 0;6;0;Create;True;0;0;0;False;0;False;0.5,0.5,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;13;-433.427,653.3469;Float;False;Property;_NormalMapPower;NormalMapPower;6;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-109.7594,451.8674;Inherit;False;Property;_OpacityMaster;OpacityMaster;8;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;154.013,-30.57095;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;12;-109.1538,341.063;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;208.9205,153.9994;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;558.1185,-61.1642;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Blood/DripShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;d3d9;d3d11_9x;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;ps4;psp2;n3ds;wiiu;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;10;0
WireConnection;6;3;7;0
WireConnection;6;4;14;0
WireConnection;2;1;6;0
WireConnection;1;1;6;0
WireConnection;5;0;4;0
WireConnection;5;1;2;0
WireConnection;3;1;6;0
WireConnection;16;0;5;0
WireConnection;16;1;15;0
WireConnection;12;0;11;0
WireConnection;12;1;3;0
WireConnection;12;2;13;0
WireConnection;20;0;1;1
WireConnection;20;1;24;0
WireConnection;0;0;5;0
WireConnection;0;1;12;0
WireConnection;0;2;16;0
WireConnection;0;4;4;4
WireConnection;0;9;20;0
ASEEND*/
//CHKSM=0D6A5F20EAEC9F5393CD5E74AC8028D64D282DAF