using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RevealManager : MonoBehaviour {

	//Created by Ian Mallett 1.9.14

	//Edits

	//Ian Mallett 22.9.14
	//Added variables
	//Added partial functionality to place method
	//Added partial functionality to voluntaryReveal method.

	public bool currentlyRevealing;
	public int numberOfGS;
	public int numberOfGSToPlace;
	public Vector2 centralPosition;
	public List<Vector2> selectableSquares;
	public Game gameController;


	public void involuntaryReveal(Vector2 blipPosition, ActionManager triggeringClass, List<Vector2> givenLoS)
	{

	}

	public void voluntaryReveal(Vector2 blipPosition)
	{
		if (gameController.thisPlayer == Game.PlayerType.GS)
		{
			if (!currentlyRevealing)
			{
				Unit blip = gameController.gameMap.getOccupant(blipPosition);
				if (blip != null)
				{
					if (blip.unitType == Game.EntityType.Blip)
					{
						//Set the reveal data
						centralPosition = blipPosition;
						numberOfGS = blip.noOfGS;
						numberOfGSToPlace = numberOfGS;
						selectableSquares = findSelectableSquares ();

						//Show the available squares to the player and place the
						//first genestealer in the central square.                               SEND ISSUE TO ALISDAIR
					}
				}
			}
			else
			{
				Debug.LogError("Reveal attempted while already revealing");
			}
		}
		else
		{
			Debug.LogError ("Voluntary Reveal attempted by Space Marine player");
		}
	}

	//Find all the squares that are selectable, given the blip is at the central position
	private List<Vector2> findSelectableSquares()
	{
		//Find every square that is visible
		List<Vector2> currentLoS = new List<Vector2> ();
		foreach (Unit unit in gameController.gameMap.getMarines ())
		{
			foreach (Vector2 position in unit.currentLoS)
			{
				if (!currentLoS.Contains(position))
				{
					currentLoS.Add (position);
				}
			}
		}

		return findSelectableSquares (currentLoS);
	}

	//Find all the squares that are selectable, given the blip is at the central position,
	//and using the given line of sight.
	private List<Vector2> findSelectableSquares(List<Vector2> givenLoS)
	{
		//Find the bottom left corner of the 3x3 grid
		Vector2 startingPos = centralPosition - Vector2.one;
		List<Vector2> returnList = new List<Vector2>();

		//Check each square in the 3x3 grid to be linked to the
		//central square, and not visible.
		for (int i = 0; i < 9 ; i++)
		{
			Vector2 checkingPos = startingPos + (i % 3) * Vector2.right + (i / 3) * Vector2.up;
			if (gameController.gameMap.areLinked(centralPosition, checkingPos))
			{
				if (!givenLoS.Contains (checkingPos))
				{
					returnList.Add(checkingPos);
				}
			}
		}

		return returnList;
	}

	public void place(Vector2 position, Game.Facing facing)
	{
		if (currentlyRevealing)
		{
			if (selectableSquares.Contains(position))
			{
				gameController.deploy (Game.EntityType.GS, position, facing);
				numberOfGSToPlace--;
			}
		}

		if (numberOfGSToPlace == 0)
		{
			currentlyRevealing = false;

			//Hide the reveal cues                                                       SEND ISSUE TO ALICE
		}
	}
}
