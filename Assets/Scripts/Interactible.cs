using UnityEngine;
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

	GameObject blah;
	Game gameController;
	public enum SelectionType{Background, SM, GS, Blip, OpenDoor, ClosedDoor, Square, DeploymentZone};
	public SelectionType attemptedSelection;

	public EventSystem eventSystem; //Added by Alisdair 14/9/14

	void Start()
	{
		//Create a reference to the Game
		blah = GameObject.FindWithTag ("GameController");
		gameController = blah.GetComponent<Game>();

		//Find the event system Added By Alisdair 14/9/14
		eventSystem = GameObject.FindWithTag ("EventSystem").GetComponent<EventSystem>();
	}

	void OnMouseDown()
	{
		if (!eventSystem.IsPointerOverEventSystemObject()) { //if statement Added By Alisdair 14/9/14 Reference: http://forum.unity3d.com/threads/raycast-into-gui.263397/#post-1742031
			Debug.Log ("The pointer was clicked on an interactable GameObject"); //Added By Alisdair 14/9/14
			//th first if statement checks to see if the click is meant for the UI
				if (isSelectable () &&
					gameController.gameState != Game.GameState.AttackSelection &&
					gameController.gameState != Game.GameState.MoveSelection) {
					//Select the unit
						gameController.selectUnit (gameObject);
					} else if (gameController.gameState == Game.GameState.AttackSelection) {
			
					} else if (gameController.gameState == Game.GameState.MoveSelection) {

					} else {
						//deselect everything if not clicking on a valid selection
							if (gameController.selectedUnit != null)
								gameController.deselect ();
					}
				} 
		else {//Added By Alisdair 14/9/14
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