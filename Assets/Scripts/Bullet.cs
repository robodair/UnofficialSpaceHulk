/// <summary>
/// Bullet is for the movement, and eventual destrying of a bullet object in the game.
/// A bullet is given all of the parameters that it needs through the setParameters method, which is called right after it is created
/// 
/// Created by Alisdair Robertson 15/10/14
/// v 15-10-2014.0
/// </summary>

using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {
		
	Vector3 targetPosition; 						// The position to move towards
	InputOutput ioClass; 							// io class reference
	bool explodes; 									// does the bullet explode at the end
	bool recallOnFinish;							// does this bullet need to tell the io class when it has finished
	float waitSeconds;								// time in seconds for the bullet to wait until it is allowed to move
	GameObject explosionPrefab; 					// the prefab of the explosion object
	public float bulletStep;						// ammount a bullet may move per frame

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {								// Update is called once per frame
		if (waitSeconds > 0) 						// wait until the time is elapsed before moving the bullet
			waitSeconds -= Time.deltaTime;
		else {
			this.gameObject.transform.position = Vector3.MoveTowards(this.gameObject.transform.position, targetPosition, bulletStep); 
													// Move the bullet toward the target
		}

		if (this.gameObject.transform.position == targetPosition){
													// if the bullet has reached the target
			if (explodes){ 							// if the bullet needs to explode, create the explosion prefab
				Instantiate(explosionPrefab, targetPosition, Quaternion.identity);
			}
			if (recallOnFinish){ 					// If the bullet needs to notify IOclass on it's finish, change the variable
				ioClass.bulletsComplete = true;
			}
			Destroy(this.gameObject); 				// destroy the bullet gameobject
		}
	
	}

	/// <summary>
	/// Sets the parameters of the bullet
	/// </summary>
	/// <param name="targetPosition">Target position.</param>
	/// <param name="ioClass">The game InputOutput class.</param>
	/// <param name="explodes">If set to <c>true</c> explodes upon reaching target.</param>
	/// <param name="recallOnFinish">If set to <c>true</c> changes bulletsComplete (in ioClass) to true on finish.</param>
	/// <param name="waitSeconds">Wait seconds before beginning to move.</param>
	/// <param name="explosionPrefab">Explosion Gameobject prefab.</param>
	public void setParameters(Vector3 targetPosition, InputOutput ioClass, bool explodes, bool recallOnFinish, float waitSeconds, GameObject explosionPrefab) {
		this.targetPosition = targetPosition;
		this.ioClass = ioClass;
		this.explodes = explodes;
		this.recallOnFinish = recallOnFinish;
		this.waitSeconds = waitSeconds;
		this.explosionPrefab = explosionPrefab;
	}
}