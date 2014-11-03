/// <summary>
/// Blood Removal handles the evenual removal of the blood particle system gameobjects
/// 
/// Created by Alisdair Robertson 3/11/14
/// v 3-11-14.0
/// </summary>
/// 
using UnityEngine;
using System.Collections;

public class BloodRemoval : MonoBehaviour {

	public float wait; 				// The time to wait before destroying the gameobject
	float elapsed;

	void Update () {				// Destroy the gameobject if the time has elapsed
		elapsed += Time.deltaTime;
		if (elapsed >= wait)
			Destroy (gameObject);
	}
}
