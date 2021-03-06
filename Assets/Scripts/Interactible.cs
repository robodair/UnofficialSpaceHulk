﻿using UnityEngine;
using UnityEngine.EventSystems; //Added By Alisdair 14/9/14
using System.Collections;

public class Interactible : MonoBehaviour {
	/*
	 * Created by Rory Bolt 13.9.14
	 * Checks if selections are valid and calls the Game class's selectUnit/deselect methods when clicking.
	 * 
	 * Edited By Alisdair Robertson 14/9/14
	 * Added a reference to the event system and an if statement to check if the click was cast over a UI element (edits have been commented)
	 */

	Game gameController;
	public enum SelectionType{Background, SM, GS, Blip, OpenDoor, ClosedDoor, Square, DeploymentZone};
	public SelectionType attemptedSelection;

	public Material gsColour;
	public Material smColour;
	public Material doorColour;
	public Material floorColour;

	Vector2 particleSquare;

	float red;
	float blue;
	float green;

	InputHandler inputHandlerController; //Added 18/9/14
	InputOutput ioController;

	void Start()
	{
		//Create a reference to the Game
		gameController = GameObject.FindWithTag ("GameController").GetComponent<Game>();
		
		//Create a reference to the GameController's InputHandler
		inputHandlerController = GameObject.FindWithTag ("GameController").GetComponent<InputHandler> ();
		ioController = GameObject.FindWithTag ("GameController").GetComponent<InputOutput> ();
	}

	void OnMouseOver(){ //Reworked RB 25.9.14
		if (gameController.gameState != Game.GameState.AttackSelection)
		{
			if (attemptedSelection == SelectionType.Square)
			{
				if(gameController.gameState != Game.GameState.Reveal)
				{
					if(!inputHandlerController.facingInProgress)
					{	
						if(gameController.unitSelected)
						{
							if (inputHandlerController.squareAvailable(new Vector2 (gameObject.transform.position.x, 
						                                                        	gameObject.transform.position.z))){
								gameObject.GetComponent<Renderer>().material.color = Color.blue;//RB 8.10.14 changed due to highlighting of all available squares

								// ====== Hologram Creation = Alisdair ===

								//if(Debug.isDebugBuild)Debug.Log ("Not allowed to show new Hologram?: " + inputHandlerController.keepHologram);
								if (!inputHandlerController.keepHologram){															// If allowed to, show the hologram for the square
									if(ioController.activePartSys.Count < 2){														// Allow only 2 particle systems to be active
										//Debug.Log ("Activatingparticles @: " + gameObject.transform.position);
										foreach (Transform ps in gameObject.transform){
											ps.gameObject.GetComponent<ParticleSystem>().enableEmission = true; 												// Show the emission effect Alisdair
											ioController.activePartSys.Add(ps.gameObject);												// store the active Particle systems
											//if (Debug.isDebugBuild) Debug.Log(ioController.activePartSys.Count);
										}
									}
									foreach (Transform ps in gameObject.transform){
										ps.gameObject.GetComponent<ParticleSystem>().emissionRate = ioController.normalEmissionRate;
									}
									ioController.hologramsActive = true;
									// =======================================
									}
							}
							else
								gameObject.GetComponent<Renderer>().material.color = new Color(0f, 0.6f, 0.1f);
						}
						else
							gameObject.GetComponent<Renderer>().material.color = new Color(0f, 0.6f, 0.1f);
					}
				}
				else{
					foreach (Transform ps in gameObject.transform){
						ps.gameObject.GetComponent<ParticleSystem>().emissionRate = ioController.normalEmissionRate; 																	// Show the emission effect Alisdair
					}
				}
			}
		}
		else if(gameController.thisPlayer == Game.PlayerType.SM)
		{ 
			if(attemptedSelection == SelectionType.GS ||
			   attemptedSelection == SelectionType.ClosedDoor)
				gameObject.GetComponent<Renderer>().material.color = Color.red;
		}
		else if(gameController.thisPlayer == Game.PlayerType.GS)
		{
			if(attemptedSelection == SelectionType.SM ||
			   attemptedSelection == SelectionType.ClosedDoor)
				gameObject.GetComponent<Renderer>().material.color = Color.red;
		}
	}
	
	void OnMouseExit(){
		if (gameController.thisPlayer == Game.PlayerType.SM)
		{
			if (attemptedSelection == SelectionType.GS)
				gameObject.GetComponent<Renderer>().material.color = gsColour.color;
		}
		else
		{
			if(attemptedSelection == SelectionType.SM)
				gameObject.GetComponent<Renderer>().material.color = smColour.color;
		}

		if (attemptedSelection == SelectionType.ClosedDoor)
			gameObject.GetComponent<Renderer>().material.color = doorColour.color;

		//RB 8.10.14 Redone to support highlighting of all available squares in movement
		if (attemptedSelection == SelectionType.Square)
		{
			red = gameObject.GetComponent<Renderer>().material.color.r;
			green = gameObject.GetComponent<Renderer>().material.color.g;
			blue = gameObject.GetComponent<Renderer>().material.color.b;
			if(gameController.gameState != Game.GameState.Reveal)
			{
				if(gameObject.GetComponent<Renderer>().material.color == Color.blue){
					gameObject.GetComponent<Renderer>().material.color = Color.green;

					// ====== Hologram Removal = Alisdair ===

					//if(Debug.isDebugBuild)Debug.Log ("Not Allowed To Remove Hologram?: " + inputHandlerController.keepHologram);
					if(!inputHandlerController.keepHologram){										// Remove the hologram on exit if allowed to Alisdair
						foreach (GameObject ps in ioController.activePartSys){
							ps.GetComponent<ParticleSystem>().enableEmission = false; 								// Stop the particle emission Alisdair
						}
						ioController.activePartSys.Clear();											// Reset the active Particle systems
						//Debug.Log("ACTIVE PART SYS SHOULD HAVE BEEN CLEARED: " + ioController.activePartSys.Count);
					}

					// =======================================
				}
				if (!inputHandlerController.coloursSet)
					gameObject.GetComponent<Renderer>().material.color = Color.white;
				else if (gameObject.GetComponent<Renderer>().material.color != Color.green)
				{
					gameObject.GetComponent<Renderer>().material.color = floorColour.color;
				}
			}

			else if(Mathf.Approximately(red, 0.68f) && Mathf.Approximately(green, 0.51f) && Mathf.Approximately(blue, 0.69f))
	        {
				gameObject.GetComponent<Renderer>().material.color = new Color(0.68f, 0.51f, 0.69f);
			}

			if (gameController.gameState == Game.GameState.Reveal){

				// === Hologram Highlight Alisdair ======


				if (ioController.selectFacingCanvasExists){
					if(new Vector2(ioController.currentFacingSelectionCanvas.transform.position.x, ioController.currentFacingSelectionCanvas.transform.position.z) != 
					   new Vector2(gameObject.transform.position.x, gameObject.transform.position.z)){
						foreach (Transform ps in gameObject.transform){

								ps.gameObject.GetComponent<ParticleSystem>().emissionRate = ioController.lowEmissionRate;
						}
					}
				}
				else {
					foreach (ParticleSystem ps in gameObject.GetComponentsInChildren<ParticleSystem>()){
						
						ps.emissionRate = ioController.lowEmissionRate;
					}
				}

				// ======================================
			}
		}
	}
	void OnMouseDown()
	{
		EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();
		if (!eventSystem.IsPointerOverGameObject())
        { //if statement Added By Alisdair 14/9/14 Reference: http://forum.unity3d.com/threads/raycast-into-gui.263397/#post-1742031
			//Debug.Log ("The pointer was clicked on an interactable GameObject"); //Added By Alisdair 14/9/14
			//th first if statement checks to see if the click is meant for the UI
			if (isSelectable ())
			{
				gameController.GetComponent<AudioSource>().PlayOneShot(ioController.clickSound);
				if (gameController.gameState == Game.GameState.AttackSelection)
	            {
					//Added RB 25.9.14
					inputHandlerController.attackTarget = gameObject;//Sets the target for the attack

					if (gameController.thisPlayer == Game.PlayerType.GS)//Genestealer player can attack, not shoot
					{
					//	inputHandlerController.attacking();
					}
					else
						inputHandlerController.shooting();//Space Marine player can shoot, not attack
				}
	            else if (gameController.gameState == Game.GameState.MoveSelection)
	            {
	                inputHandlerController.moveTarget = gameObject;
					inputHandlerController.moving ();
				}
				else if (gameController.gameState == Game.GameState.Reveal)
				{
					if(inputHandlerController.allowRevealSelection)
					{
						foreach (Vector2 position in inputHandlerController.selectableRevealPositions)
						{	
							if(position == new Vector2(gameObject.transform.position.x, gameObject.transform.position.z))
							{
								inputHandlerController.revealPosition = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
								inputHandlerController.continueRevealing();
							}
						}
					}
				}
				else
				{
					//Select the unit
					gameController.selectUnit (gameObject);
				}
			}
            else
            {
				//Additional checks RB 2.10.14
				//deselect everything if not clicking on a valid selection
				gameController.GetComponent<AudioSource>().PlayOneShot(ioController.click_error);
				if (gameController.unitSelected)
				{
					if(gameController.gameState == Game.GameState.MoveSelection)
					{


						// ====== Hologram & Facing canvas Removal = Alisdair ===

						//if(Debug.isDebugBuild)Debug.Log ("Keep Hologram becasue Facing Selection Destroyed: " + inputHandlerController.keepHologram);
						inputHandlerController.keepHologram = false;																// Allow removal of the hologram
						if (ioController.selectFacingCanvasExists){																	// Remove the hologram and the canvas from the square that it was on
							foreach (GameObject ps in ioController.activePartSys){
								//if(Debug.isDebugBuild)Debug.Log ("FadingParticles");
								ps.GetComponent<ParticleSystem>().enableEmission = false;
								//if(Debug.isDebugBuild)Debug.Log ("Emission: " + ps.enableEmission);
								ps.GetComponent<ParticleSystem>().emissionRate = ioController.normalEmissionRate;
								//if(Debug.isDebugBuild)Debug.Log ("Emission Rate: " + ps.enableEmission);
							}
							Destroy(ioController.currentFacingSelectionCanvas);														// Remove the select Facing Canvas
							ioController.selectFacingCanvasExists = false;															// Show that the canvas has been removed
							//if(Debug.isDebugBuild)Debug.Log(ioController.activePartSys.Count);
							ioController.activePartSys.Clear();																		// Reset the active Particle systems
							//Debug.Log("ACTIVE PART SYS SHOULD HAVE BEEN CLEARED: " + ioController.activePartSys.Count);
						}


						// =======================================

						inputHandlerController.facingInProgress = false;
						gameController.changeGameState(Game.GameState.InactiveSelected);
					}
					else if (gameController.gameState == Game.GameState.AttackSelection)
					{
						gameController.changeGameState(Game.GameState.InactiveSelected);
					}
					inputHandlerController.hideAvailableSquares();
					gameController.deselect ();
				}
			}
		}
		else
        {//Added By Alisdair 14/9/14
			//Debug.Log ("The pointer was clicked over a UI Element)");//Added By Alisdair 14/9/14
		}//Added By Alisdair 14/9/14
	}

	bool isSelectable()
	{
		//Units are only selectable if the player controls them, so checks for ownership of the unit.
		//Current Exceptions as of 14.9.14: 
		//1. If the gameState is currently in AttackSelect, you cannot attack your own units, so will only be able to select enemy units and doors.
		//2. If the gameState is currently in MoveSelect, you can only move onto a square, so will only be able to select a square.
		//3. If the gameState is currently in Reveal, you can select squares for placement 20.10.14
		if (gameController.gameState == Game.GameState.AttackSelection) //Exception 1
		{
			if (gameController.thisPlayer == Game.PlayerType.SM)
				if (attemptedSelection == SelectionType.GS ||
				    attemptedSelection == SelectionType.ClosedDoor)
					return true;
			
			if (gameController.thisPlayer == Game.PlayerType.GS)
				if (attemptedSelection == SelectionType.SM ||
				    attemptedSelection == SelectionType.ClosedDoor)
					return true;
		}
		else if (gameController.gameState == Game.GameState.MoveSelection ||//Exception 2
		         gameController.gameState == Game.GameState.Reveal) //Exception 3
		{
			if(!inputHandlerController.facingInProgress)
				if (attemptedSelection == SelectionType.Square)
					return true;
		}

		else if (gameController.gameState != Game.GameState.ShowAction)
		{
			if (gameController.thisPlayer == Game.PlayerType.GS)
				if (attemptedSelection == SelectionType.GS ||
					attemptedSelection == SelectionType.Blip)	
					return true;

			if (gameController.thisPlayer == Game.PlayerType.SM)
					if (attemptedSelection == SelectionType.SM)
					return true;
		}
		return false;
	}
}