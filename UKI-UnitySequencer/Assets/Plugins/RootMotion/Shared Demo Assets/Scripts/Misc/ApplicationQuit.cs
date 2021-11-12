using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {

	// Safely getting out of full screen desktop builds
	public class ApplicationQuit : MonoBehaviour {

		void Update () {
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
		}
	}
}
