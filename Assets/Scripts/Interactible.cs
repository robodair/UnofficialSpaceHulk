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

	InputHandler inputHandlerController; //Added 18/9/14


	public EventSystem eventSystem; //Added by Alisdair 14/9/14

	void Start()
	{
		//Create a reference to the Game
		gameController = GameObject.FindWithTag ("GameController").GetComponent<Game>();
		
		//Create a reference to the GameController's InputHandler
		inputHandlerController = GameObject.FindWithTag ("GameController").GetComponent<InputHandler> ();

		//Find the event system Added By Alisdair 14/9/14
		eventSystem = GameObject.FindWithTag ("EventSystem").GetComponent<EventSystem>();
	}

	void OnMouseOver(){ //Reworked RB 25.9.14
		if (gameController.gameState != Game.GameState.AttackSelection)
		{
			if (attemptedSelection == SelectionType.Square)
				gameObject.renderer.material.color = Color.green;
		}
		else if(gameController.thisPlayer == Game.PlayerType.SM)
		{ 
			if(attemptedSelection == SelectionType.GS ||
			   attemptedSelection == SelectionType.ClosedDoor)
				gameObject.renderer.material.color = Color.red;
		}
		else if(gameController.thisPlayer == Game.PlayerType.GS)
		{
			if(attemptedSelection == SelectionType.SM ||
			   attemptedSelection == SelectionType.ClosedDoor)
				gameObject.renderer.material.color = Color.red;
		}
	}

	void OnMouseExit(){
		if (gameController.thisPlayer == Game.PlayerType.SM)
		{
			if (attemptedSelection == SelectionType.Square ||
			    attemptedSelection == SelectionType.GS)
				gameObject.renderer.material.color = Color.white;
		}
		else 
			if(attemptedSelection == SelectionType.Square ||
			   attemptedSelection == SelectionType.SM)
				gameObject.renderer.material.color = Color.white;
		if (attemptedSelection == SelectionType.ClosedDoor)
			gameObject.renderer.material.color = Color.yellow;
	}
	void OnMouseDown()
	{
		if (!eventSystem.IsPointerOverEventSystemObject())
        { //if statement Added By Alisdair 14/9/14 Reference: http://forum.unity3d.com/threads/raycast-into-gui.263397/#post-1742031
			Debug.Log ("The pointer was clicked on an interactable GameObject"); //Added By Alisdair 14/9/14
			//th first if statement checks to see if the click is meant for the UI
			if (isSelectable ())
			{
				if (gameController.gameState == Game.GameState.AttackSelection)
	            {
					//Added RB 25.9.14
					inputHandlerController.attackTarget = gameObject;//Sets the target for the attack

					if (gameController.thisPlayer == Game.PlayerType.GS)//Genestealer player can attack, not shoot
						inputHandlerController.attacking();
					else
						inputHandlerController.shooting();//Space Marine player can shoot, not attack
				}
	            else if (gameController.gameState == Game.GameState.MoveSelection)
	            {
	                inputHandlerController.moveTarget = gameObject;
					inputHandlerController.moving ();
				}
				else
				{
					//Select the unit
					gameController.selectUnit (gameObject);
				}
			}
            else
            {
				//deselect everything if not clicking on a valid selection
				gameController.gameState = Game.GameState.InactiveSelected;
				if (gameController.unitSelected)
					gameController.deselect ();
			}
		}
		else
        {//Added By Alisdair 14/9/14
			Debug.Log ("The pointer was clicked over a UI Element)");//Added By Alisdair 14/9/14
		}//Added By Alisdair 14/9/14
	}

	bool isSelectable()
	{
		//Units are only selectable if the player controls them, so checks for ownership of the unit.
		//Current Exceptions as of 14.9.14: 
		//1. If the gameState is currently in AttackSelect, you cannot attack your own units, so will only be able to select enemy units and doors.
		//2. If the gameState is currently in MoveSelect, you can only move onto a square, so will only be able to select a square.
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
		else if (gameController.gameState == Game.GameState.MoveSelection) //Exception 2
		{
			if (attemptedSelection == SelectionType.Square)
				return true;
		}

		else
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