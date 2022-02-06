using UnityEngine;

namespace Andtech.StarPack {

	public class ActivateDepthRendering : MonoBehaviour {

		private void Start() {
			Camera.main.depthTextureMode = DepthTextureMode.Depth;
		}
	}
}
