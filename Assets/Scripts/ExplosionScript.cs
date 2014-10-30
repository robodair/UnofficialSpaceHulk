/// <summary>
/// ExplosionScript is for the automatic removal of explosion objects by themself
/// Fully self-contatined, the explosion object script starts fading the object as soon as it is instantiated
/// 
/// Created by Alisdair Robertson 15/10/14
/// v28-10-14.1
/// </summary>
/// 
using UnityEngine;
using System.Collections;

public class ExplosionScript : MonoBehaviour {

	public AudioClip gs_Death_Explosion;
	float wait = 3.0f; // Wait timer so sound can finish

	void Start(){
		audio.PlayOneShot(gs_Death_Explosion);
	}
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {											// Update is called once per frame
		Color color = this.gameObject.renderer.material.color;	// Get the material color
		color.a -= 0.03f;										// Make it slightly more transparent
		this.gameObject.renderer.material.color = color;		// Reassign the color to the material
		
		if (color.a <= 0) { 									// If the gameobject is now fully transparent, remove it.
			if(wait <= 0){
				Destroy (this.gameObject);
			}
		}
	}
}
