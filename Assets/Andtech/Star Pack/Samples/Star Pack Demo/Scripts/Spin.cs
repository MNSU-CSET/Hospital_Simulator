using UnityEngine;

namespace Andtech.StarPack {

	public class Spin : MonoBehaviour {
		[SerializeField]
		private float speed = 180.0F;

		private void Update() {
			var eulerAngles = transform.eulerAngles;
			eulerAngles.y += speed * Time.deltaTime;

			transform.eulerAngles = eulerAngles;
		}
	}
}
