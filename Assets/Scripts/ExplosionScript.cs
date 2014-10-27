/// <summary>
/// ExplosionScript is for the automatic removal of explosion objects by themself
/// Fully self-contatined, the explosion object script starts fading the object as soon as it is instantiated
/// 
/// Created by Alisdair Robertson 15/10/14
/// v28-10-14.0
/// </summary>
/// 
using UnityEngine;
using System.Collections;

public class ExplosionScript : MonoBehaviour {

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {											// Update is called once per frame
		particleSystem.renderer.sortingOrder = 0;				// Show this on top of everything else
		Color color = this.gameObject.renderer.material.color;	// Get the material color
		color.a -= 0.03f;										// Make it slightly more transparent
		this.gameObject.renderer.material.color = color;		// Reassign the color to the material
		
		if (color.a <= 0) { 									// If the gameobject is now fully transparent, remove it.
			Destroy (this.gameObject);
		}
	}
}
