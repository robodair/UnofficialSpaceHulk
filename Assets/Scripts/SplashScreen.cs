/// <summary>
/// Splash screen display, moves to the actual game after displaying this image
/// </summary>
using UnityEngine;
using System.Collections;

public class SplashScreen : MonoBehaviour {

	public float timeToDisplayImage; 	// The time to wait
	public int nextLevelToLoad;			// The level to be loaded next
	
	private float timeCount;		//Holding Value

	void Update () {
		timeCount += Time.deltaTime;
		if (timeCount >= timeToDisplayImage) {
			Application.LoadLevel(nextLevelToLoad);
		}
	}
}
