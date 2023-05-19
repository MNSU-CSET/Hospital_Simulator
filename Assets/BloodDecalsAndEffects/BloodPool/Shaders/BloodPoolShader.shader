// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BloodPoolShader"
{
	Properties
	{
		_RoundNormal("RoundNormal", 2D) = "bump" {}
		_BloodSpillArea_Mask_A("BloodSpillArea_Mask_A", 2D) = "white" {}
		_BloodSpillArea_A_normal("BloodSpillArea_A_normal", 2D) = "white" {}
		_ToneBias("ToneBias", Range( 0 , 1)) = 0
		_ToneScale("ToneScale", Range( 0 , 5)) = 0
		_MainColorGloss("MainColor-Gloss", Color) = (0.3235294,0,0,0.966)
		_MeshScale("MeshScale", Range( 0 , 100)) = 0
		_Fade("Fade", Range( 0 , 1)) = 0.98
		_FadeRange("FadeRange", Range( -1 , 1)) = 0.5
		_Tiling1("Tiling1", Range( 0.1 , 30)) = 3
		_MotionSpeed1("MotionSpeed1", Range( 0 , 5)) = 2
		_Depth1("Depth1", Range( 0 , 5)) = 1
		_FalloffArea1("FalloffArea1", Range( 0 , 100)) = 57.47058
		_PositionOffset1("PositionOffset1", Vector) = (6.04,2.5,0,0)
		_Tiling2("Tiling2", Range( 0.1 , 30)) = 2.5
		_MotionSpeed2("MotionSpeed2", Range( 0 , 5)) = 2
		_Depth2("Depth2", Range( 0 , 5)) = 1
		_FalloffArea2("FalloffArea2", Range( 0 , 100)) = 57.47058
		_PositionOffset2("PositionOffset2", Vector) = (5,2.6,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _MeshScale;
		uniform sampler2D _BloodSpillArea_A_normal;
		uniform float4 _BloodSpillArea_A_normal_ST;
		uniform sampler2D _RoundNormal;
		uniform float _Depth1;
		uniform float _MotionSpeed1;
		uniform float _Tiling1;
		uniform float2 _PositionOffset1;
		uniform float _FalloffArea1;
		uniform float _Depth2;
		uniform float _MotionSpeed2;
		uniform float _Tiling2;
		uniform float2 _PositionOffset2;
		uniform float _FalloffArea2;
		uniform float4 _MainColorGloss;
		uniform sampler2D _BloodSpillArea_Mask_A;
		uniform float4 _BloodSpillArea_Mask_A_ST;
		uniform float _ToneBias;
		uniform float _ToneScale;
		uniform float _FadeRange;
		uniform float _Fade;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			v.vertex.xyz += ( _MeshScale * ase_vertex3Pos );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BloodSpillArea_A_normal = i.uv_texcoord * _BloodSpillArea_A_normal_ST.xy + _BloodSpillArea_A_normal_ST.zw;
			float3 _Vector0 = float3(0,0,1);
			float mulTime47 = _Time.y * _MotionSpeed1;
			float2 temp_cast_1 = (_Tiling1).xx;
			float2 uv_TexCoord32 = i.uv_texcoord * temp_cast_1 + ( _PositionOffset1 + ( _Tiling1 * -0.5 ) );
			float2 temp_cast_2 = (distance( float2( 0,0 ) , uv_TexCoord32 )).xx;
			float2 panner34 = ( mulTime47 * float2( -0.5,-0.5 ) + temp_cast_2);
			float2 uv_TexCoord45 = i.uv_texcoord + float2( -0.5,-0.5 );
			float3 lerpResult57 = lerp( _Vector0 , UnpackScaleNormal( tex2D( _RoundNormal, panner34 ), _Depth1 ) , pow( ( 1.0 - distance( uv_TexCoord45 , float2( 0,0 ) ) ) , ( 100.0 - _FalloffArea1 ) ));
			float mulTime82 = _Time.y * _MotionSpeed2;
			float2 temp_cast_3 = (_Tiling2).xx;
			float2 uv_TexCoord81 = i.uv_texcoord * temp_cast_3 + ( _PositionOffset2 + ( _Tiling2 * -0.5 ) );
			float2 temp_cast_4 = (distance( float2( 0,0 ) , uv_TexCoord81 )).xx;
			float2 panner88 = ( mulTime82 * float2( -0.5,-0.5 ) + temp_cast_4);
			float2 uv_TexCoord80 = i.uv_texcoord + float2( -0.5,-0.5 );
			float3 lerpResult92 = lerp( _Vector0 , UnpackScaleNormal( tex2D( _RoundNormal, panner88 ), _Depth2 ) , pow( ( 1.0 - distance( uv_TexCoord80 , float2( 0,0 ) ) ) , ( 100.0 - _FalloffArea2 ) ));
			o.Normal = BlendNormals( tex2D( _BloodSpillArea_A_normal, uv_BloodSpillArea_A_normal ).rgb , BlendNormals( lerpResult57 , lerpResult92 ) );
			float2 uv_BloodSpillArea_Mask_A = i.uv_texcoord * _BloodSpillArea_Mask_A_ST.xy + _BloodSpillArea_Mask_A_ST.zw;
			float4 tex2DNode1 = tex2D( _BloodSpillArea_Mask_A, uv_BloodSpillArea_Mask_A );
			o.Albedo = ( _MainColorGloss * ( ( tex2DNode1 + _ToneBias ) * _ToneScale ) ).rgb;
			o.Metallic = 0.1;
			o.Smoothness = _MainColorGloss.a;
			float lerpResult29 = lerp( tex2DNode1.a , ( tex2DNode1.r * tex2DNode1.a ) , _FadeRange);
			o.Alpha = ( lerpResult29 * ( 1.0 - _Fade ) );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 

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
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
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
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
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
Version=16200
81;243;1376;698;1760.731;-1092.038;1.224727;True;False
Node;AmplifyShaderEditor.CommentaryNode;73;-2132.771,790.8145;Float;False;2090.166;746.9825;RippleSet1;16;40;41;63;65;48;45;32;47;33;44;59;36;34;39;90;98;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;74;-2110.933,1632.367;Float;False;2090.166;746.9825;RippleSet2;16;89;88;87;85;84;83;82;81;80;79;78;77;76;75;43;99;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-2041.954,1341.476;Float;False;Property;_Tiling1;Tiling1;9;0;Create;True;0;0;False;0;3;7.9;0.1;30;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-2020.116,2183.029;Float;False;Property;_Tiling2;Tiling2;14;0;Create;True;0;0;False;0;2.5;10.8;0.1;30;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-1603.704,1022.412;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;77;-1991.141,1780.048;Float;False;Property;_PositionOffset2;PositionOffset2;18;0;Create;True;0;0;False;0;5,2.6;0.65,0.71;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-1581.866,1863.965;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;63;-2012.979,938.4959;Float;False;Property;_PositionOffset1;PositionOffset1;13;0;Create;True;0;0;False;0;6.04,2.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;78;-1286.551,1819.061;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;65;-1308.389,977.5083;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;81;-968.3469,1863.678;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;-0.5,-0.5;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;79;-2060.932,1682.367;Float;False;Property;_MotionSpeed2;MotionSpeed2;15;0;Create;True;0;0;False;0;2;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-2082.771,840.8145;Float;False;Property;_MotionSpeed1;MotionSpeed1;10;0;Create;True;0;0;False;0;2;2.56;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;80;-1609.418,2187.276;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;-0.5,-0.5;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;32;-990.1846,1022.126;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;-0.5,-0.5;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;45;-1663.915,1283.128;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;-0.5,-0.5;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DistanceOpNode;84;-1193.855,2121.753;Float;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;47;-809.2242,856.7902;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-921.6936,2304.083;Float;False;Property;_FalloffArea2;FalloffArea2;17;0;Create;True;0;0;False;0;57.47058;83.6;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;33;-687.47,1021.096;Float;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;83;-665.6324,1862.648;Float;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;82;-787.3865,1698.343;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;44;-1215.693,1280.2;Float;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;90;-875.6918,1462.718;Float;False;Property;_FalloffArea1;FalloffArea1;12;0;Create;True;0;0;False;0;57.47058;85.9;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;87;-904.4362,2089.809;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;69;-1636.359,293.6728;Float;True;Property;_RoundNormal;RoundNormal;0;0;Create;True;0;0;False;0;3c7d36b08e44f6044b809236eadfd0b6;3c7d36b08e44f6044b809236eadfd0b6;True;bump;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.OneMinusNode;36;-902.6985,1263.51;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;98;-529.5382,1392.957;Float;False;2;0;FLOAT;100;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;99;-455.6075,2140.094;Float;False;2;0;FLOAT;100;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;88;-309.5155,1773.594;Float;True;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.5,-0.5;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;34;-331.3534,932.0417;Float;True;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.5,-0.5;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-354.0751,2011.668;Float;False;Property;_Depth2;Depth2;16;0;Create;True;0;0;False;0;1;3.68;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-375.9129,1170.115;Float;False;Property;_Depth1;Depth1;11;0;Create;True;0;0;False;0;1;2.46;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;39;-303.6052,1273.627;Float;True;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-1388.338,-132.0553;Float;True;Property;_BloodSpillArea_Mask_A;BloodSpillArea_Mask_A;1;0;Create;True;0;0;False;0;6f98f51be33f737469b572ac63f7552c;6f98f51be33f737469b572ac63f7552c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;55;361.6078,545.2992;Float;True;Property;_Shape_Round_normal;Shape_Round_normal;7;0;Create;True;0;0;False;0;3c7d36b08e44f6044b809236eadfd0b6;3c7d36b08e44f6044b809236eadfd0b6;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;89;-281.7673,2115.18;Float;True;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;91;406.3351,1193.88;Float;True;Property;_TextureSample0;Texture Sample 0;7;0;Create;True;0;0;False;0;3c7d36b08e44f6044b809236eadfd0b6;3c7d36b08e44f6044b809236eadfd0b6;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;58;-60.95399,373.4299;Float;False;Constant;_Vector0;Vector 0;8;0;Create;True;0;0;False;0;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;97;-988.7698,-92.08617;Float;False;Property;_ToneScale;ToneScale;4;0;Create;True;0;0;False;0;0;0.72;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-459.9875,30.48713;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-620.8709,146.6922;Float;False;Property;_FadeRange;FadeRange;8;0;Create;True;0;0;False;0;0.5;0.042;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;92;814.3919,776.1454;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;57;798.3461,405.5949;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-287.1842,184.2726;Float;False;Property;_Fade;Fade;7;0;Create;True;0;0;False;0;0.98;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-978.8796,-169.9705;Float;False;Property;_ToneBias;ToneBias;3;0;Create;True;0;0;False;0;0;0.235;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;29;-98.6231,-63.88902;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;95;-621.6686,-165.6539;Float;False;ConstantBiasScale;-1;;1;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendNormalsNode;94;1125.695,564.4412;Float;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;21;-942.6665,366.4076;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;23;-957.909,277.89;Float;False;Property;_MeshScale;MeshScale;6;0;Create;True;0;0;False;0;0;6.1;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-990.0797,67.85089;Float;True;Property;_BloodSpillArea_A_normal;BloodSpillArea_A_normal;2;0;Create;True;0;0;False;0;4cd6f088fa4b811418128050a6ee8b58;4cd6f088fa4b811418128050a6ee8b58;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;3;-1265.857,-382.1127;Float;False;Property;_MainColorGloss;MainColor-Gloss;5;0;Create;True;0;0;False;0;0.3235294,0,0,0.966;0.3088235,0,0,0.965;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;30;93.01147,137.4423;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;70.98034,-269.5532;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;24;175.2087,-122.9261;Float;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;373.0175,-42.33733;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;30.51532,261.7023;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendNormalsNode;56;1041.338,151.3274;Float;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1443.831,-59.65219;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;BloodPoolShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;41;0;40;0
WireConnection;76;0;75;0
WireConnection;78;0;77;0
WireConnection;78;1;76;0
WireConnection;65;0;63;0
WireConnection;65;1;41;0
WireConnection;81;0;75;0
WireConnection;81;1;78;0
WireConnection;32;0;40;0
WireConnection;32;1;65;0
WireConnection;84;0;80;0
WireConnection;47;0;48;0
WireConnection;33;1;32;0
WireConnection;83;1;81;0
WireConnection;82;0;79;0
WireConnection;44;0;45;0
WireConnection;87;0;84;0
WireConnection;36;0;44;0
WireConnection;98;1;90;0
WireConnection;99;1;43;0
WireConnection;88;0;83;0
WireConnection;88;1;82;0
WireConnection;34;0;33;0
WireConnection;34;1;47;0
WireConnection;39;0;36;0
WireConnection;39;1;98;0
WireConnection;55;0;69;0
WireConnection;55;1;34;0
WireConnection;55;5;59;0
WireConnection;89;0;87;0
WireConnection;89;1;99;0
WireConnection;91;0;69;0
WireConnection;91;1;88;0
WireConnection;91;5;85;0
WireConnection;71;0;1;1
WireConnection;71;1;1;4
WireConnection;92;0;58;0
WireConnection;92;1;91;0
WireConnection;92;2;89;0
WireConnection;57;0;58;0
WireConnection;57;1;55;0
WireConnection;57;2;39;0
WireConnection;29;0;1;4
WireConnection;29;1;71;0
WireConnection;29;2;28;0
WireConnection;95;3;1;0
WireConnection;95;1;96;0
WireConnection;95;2;97;0
WireConnection;94;0;57;0
WireConnection;94;1;92;0
WireConnection;30;0;26;0
WireConnection;4;0;3;0
WireConnection;4;1;95;0
WireConnection;27;0;29;0
WireConnection;27;1;30;0
WireConnection;22;0;23;0
WireConnection;22;1;21;0
WireConnection;56;0;2;0
WireConnection;56;1;94;0
WireConnection;0;0;4;0
WireConnection;0;1;56;0
WireConnection;0;3;24;0
WireConnection;0;4;3;4
WireConnection;0;9;27;0
WireConnection;0;11;22;0
ASEEND*/
//CHKSM=D66FDBF1A547435C888D970AEE9707C36FB6F97A