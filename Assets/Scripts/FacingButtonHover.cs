/// <summary>
/// FacingButtonHover is used to rotate the holograms to face the button that is being hovered over
/// Created By Alisdair 4-11-14
/// v4-11-14.0
/// </summary>
using UnityEngine;
using System.Collections;

public class FacingButtonHover : MonoBehaviour {
	public Game.Facing facing;
	public InputOutput inputOutput;

	void Start(){
		inputOutput = GameObject.Find("GameController").GetComponent<InputOutput>();
	}

	public void faceHologram()
	{
			//if(Debug.isDebugBuild) Debug.Log("MOUSE OVER FACING BUTTON");
			inputOutput.faceHolograms(facing);
	}
	
}
