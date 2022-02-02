using UnityEditor;
using UnityEngine;

namespace Andtech.StarPack.Editor {

	/// <summary>
	/// Base class for Star Pack shader editors.
	/// </summary>
	public class StarPackShaderGUI : ShaderGUI {
		protected static class Styles {
			public static GUIContent mainOptionsText = EditorGUIUtility.TrTextContent("Main Options");
			public static GUIContent albedoText = EditorGUIUtility.TrTextContent("Glow Mask", "Colored area (R) white area (G).");
			public static GUIContent brightnessText = EditorGUIUtility.TrTextContent("Brightness", "Intensity of white areas.");
			public static GUIContent softParticlesFactorText = EditorGUIUtility.TrTextContent("Soft Particles Factor", "Softness of soft particles");
			public static GUIContent twinkleOptionsText = EditorGUIUtility.TrTextContent("Twinkle Options");
			public static GUIContent twinkleToggleText = EditorGUIUtility.TrTextContent("Enable Twinkle", "Enable the twinkle effect.");
			public static GUIContent twinkleAmountText = EditorGUIUtility.TrTextContent("Twinkle Intensity", "Intensity of the twinkle effect.");
			public static GUIContent twinkleSpeedText = EditorGUIUtility.TrTextContent("Twinkle Speed", "Rate of the twinkle effect.");

			public static GUIContent requiredVertexStreamsText = EditorGUIUtility.TrTextContent("Required Vertex Streams");
			public static GUIContent streamPositionText = EditorGUIUtility.TrTextContent("Position (POSITION.xyz)");
			public static GUIContent streamColorText = EditorGUIUtility.TrTextContent("Color (COLOR.xyzw)");
			public static GUIContent streamColorInstancedText = EditorGUIUtility.TrTextContent("Color (INSTANCED0.xyzw)");
			public static GUIContent streamUVText = EditorGUIUtility.TrTextContent("UV (TEXCOORD0.xy)");
			public static GUIContent streamRandomX = EditorGUIUtility.TrTextContent("StableRandom.x (TEXCOORD0.z)");
		}

		protected MaterialEditor m_MaterialEditor;
		protected MaterialProperty mainTex = null;
		protected MaterialProperty brightness = null;
		protected MaterialProperty invFade = null;
		protected MaterialProperty twinkleAmount = null;
		protected MaterialProperty twinkleSpeed = null;
		protected const string PROPERTY_MAINTEX = "_MainTex";
		protected const string PROPERTY_BRIGHTNESS = "_Brightness";
		protected const string PROPERTY_INVFADE = "_InvFade";
		protected const string PROPERTY_TWINKLEAMOUNT = "_TwinkleAmount";
		protected const string PROPERTY_TWINKLESPEED = "_TwinkleSpeed";
		protected const string KEYWORD_TWINKLEON = "TWINKLE_ON";

		protected void FindProperties(MaterialProperty[] props) {
			mainTex = FindProperty(PROPERTY_MAINTEX, props);
			brightness = FindProperty(PROPERTY_BRIGHTNESS, props);
			invFade = FindProperty(PROPERTY_INVFADE, props);
			twinkleAmount = FindProperty(PROPERTY_TWINKLEAMOUNT, props);
			twinkleSpeed = FindProperty(PROPERTY_TWINKLESPEED, props);
		}
	}
}