using System;
using UnityEditor;
using UnityEngine;

namespace Andtech.StarPack.Editor {

	/// <summary>
	/// Particle glow shader editor.
	/// </summary>
	public class ParticleStandardGlowShaderGUI : StarPackShaderGUI {

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
			FindProperties(properties);
			m_MaterialEditor = materialEditor;
			Material material = materialEditor.target as Material;

			ShaderPropertiesGUI(material);

			EditorGUILayout.Space();

			GUILayout.Label(Styles.requiredVertexStreamsText, EditorStyles.boldLabel);
			DoVertexStreamsArea(material);
		}

		private void ShaderPropertiesGUI(Material material) {
			// Use default labelWidth
			EditorGUIUtility.labelWidth = 0f;

			// Detect any changes to the material
			EditorGUI.BeginChangeCheck();
			{
				GUILayout.Label(Styles.mainOptionsText, EditorStyles.boldLabel);
				m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, mainTex);
				m_MaterialEditor.ShaderProperty(brightness, Styles.brightnessText);
				m_MaterialEditor.ShaderProperty(invFade, Styles.softParticlesFactorText);

				EditorGUILayout.Space();
				GUILayout.Label(Styles.twinkleOptionsText, EditorStyles.boldLabel);
				DoTwinkleArea();

				void DoTwinkleArea() {
					bool isTwinkleOn = Array.IndexOf(material.shaderKeywords, KEYWORD_TWINKLEON) != -1;
					EditorGUI.BeginChangeCheck();
					isTwinkleOn = EditorGUILayout.Toggle(Styles.twinkleToggleText, isTwinkleOn);
					if (EditorGUI.EndChangeCheck()) {
						// enable or disable the keyword based on checkbox
						if (isTwinkleOn)
							material.EnableKeyword(KEYWORD_TWINKLEON);
						else
							material.DisableKeyword(KEYWORD_TWINKLEON);
					}

					if (isTwinkleOn) {
						m_MaterialEditor.ShaderProperty(twinkleAmount, Styles.twinkleAmountText);
						m_MaterialEditor.ShaderProperty(twinkleSpeed, Styles.twinkleSpeedText);
					}
				}
			}
		}

		private void DoVertexStreamsArea(Material material) {
			bool isTwinkleOn = Array.IndexOf(material.shaderKeywords, KEYWORD_TWINKLEON) != -1;

			GUILayout.Label(Styles.streamPositionText, EditorStyles.label);
			GUILayout.Label(Styles.streamColorText, EditorStyles.label);
			GUILayout.Label(Styles.streamUVText, EditorStyles.label);
			if (isTwinkleOn) {
				GUILayout.Label(Styles.streamRandomX, EditorStyles.label);
			}
		}
	}
}
