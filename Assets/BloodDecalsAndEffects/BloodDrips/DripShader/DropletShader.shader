// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Blood/DropletShader"
{
	Properties
	{
		_Color_GlossA("Color_Gloss(A)", Color) = (0,0,0,0)
		_Cutoff( "Mask Clip Value", Float ) = 0.4
		_AlbedoAlpha("Albedo/Alpha", 2D) = "white" {}
		_NormalMap("NormalMap", 2D) = "bump" {}
		_EmissiveAmount("EmissiveAmount", Float) = 0
		_NormalMapPower("NormalMapPower", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _NormalMapPower;
		uniform float4 _Color_GlossA;
		uniform sampler2D _AlbedoAlpha;
		uniform float4 _AlbedoAlpha_ST;
		uniform float _EmissiveAmount;
		uniform float _Cutoff = 0.4;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float3 lerpResult7 = lerp( float3(0.5,0.5,1) , UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) ) , _NormalMapPower);
			o.Normal = lerpResult7;
			float2 uv_AlbedoAlpha = i.uv_texcoord * _AlbedoAlpha_ST.xy + _AlbedoAlpha_ST.zw;
			float4 tex2DNode3 = tex2D( _AlbedoAlpha, uv_AlbedoAlpha );
			float4 temp_output_4_0 = ( _Color_GlossA * tex2DNode3 );
			o.Albedo = temp_output_4_0.rgb;
			o.Emission = ( temp_output_4_0 * _EmissiveAmount ).rgb;
			o.Smoothness = _Color_GlossA.a;
			o.Alpha = 1;
			clip( tex2DNode3.r - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16200
19;345;1376;698;1488.405;124.3104;1.337257;True;False
Node;AmplifyShaderEditor.SamplerNode;3;-629,115;Float;True;Property;_AlbedoAlpha;Albedo/Alpha;2;0;Create;True;0;0;False;0;None;cd00e24887933cd438beac1761efe323;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;2;-559,-60;Float;False;Property;_Color_GlossA;Color_Gloss(A);0;0;Create;True;0;0;False;0;0,0,0,0;0.4485294,0,0,0.822;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-236,32;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-322.3168,220.7019;Float;False;Property;_EmissiveAmount;EmissiveAmount;4;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-617,662;Float;False;Property;_NormalMapPower;NormalMapPower;5;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;6;-618,496;Float;False;Constant;_Vector0;Vector 0;4;0;Create;True;0;0;False;0;0.5,0.5,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;5;-700,309;Float;True;Property;_NormalMap;NormalMap;3;0;Create;True;0;0;False;0;None;ad2628b3f1d045c449e1ac755cc7bdc3;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-77.59875,192.6195;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;7;-295,446;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;141.0983,156.7157;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Blood/DropletShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.4;True;True;0;True;TransparentCutout;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;2;0
WireConnection;4;1;3;0
WireConnection;10;0;4;0
WireConnection;10;1;9;0
WireConnection;7;0;6;0
WireConnection;7;1;5;0
WireConnection;7;2;8;0
WireConnection;0;0;4;0
WireConnection;0;1;7;0
WireConnection;0;2;10;0
WireConnection;0;4;2;4
WireConnection;0;10;3;1
ASEEND*/
//CHKSM=563D24E344056840B6E6FA3C612B0AF049BEAC8B