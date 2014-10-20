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

	//Ian Mallett 5.10.14
	//Removed the callingClass parameter from involuntaryReveal and changed the type
	//of prevLoS from a list of Vector2 variables to a dictionary of Units with lists
	//of Vector2s.

	//Ian Mallett 6.10.14
	//Added the actionManager parameter to the involuntaryReveal method.

	//Ian Mallett 20.10.14
	//Added functionality to the involuntaryReveal method
	//Improved findSelectableSquares method

	public bool currentlyRevealing;
	public int numberOfGS;
	public int numberOfGSToPlace;
	public Vector2 centralPosition;
	public List<Vector2> selectableSquares;
	public Game gameController;
	public InputHandler inputHandler;


	public void involuntaryReveal(Vector2 blipPosition, ActionManager actionManager, Dictionary<Unit, List<Vector2>> prevLoS)
	{
		//Set the initial values
		Unit blip = null;
		if (gameController.gameMap.getSquare (blipPosition).isOccupied)
		{
			blip = gameController.gameMap.getSquare(blipPosition).occupant;
		}

		numberOfGS = blip.noOfGS;
		numberOfGSToPlace = numberOfGS;
		currentlyRevealing = true;
		centralPosition = blipPosition;

		selectableSquares = findSelectableSquares (prevLoS);

		//Show the reveal
		gameController.ioModule.removeUnit (blipPosition);
		gameController.gameMap.removeUnit (blipPosition);
		inputHandler.revealing (blipPosition, selectableSquares);


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
		foreach (Unit unit in gameController.gameMap.getUnits(Game.EntityType.SM))
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

	//Find all the squares that are selectable, given the blip is at the central position
	//using the given LoS.
	private List<Vector2> findSelectableSquares(Dictionary<Unit, List<Vector2>> completeLoS)
	{
		List<Vector2> currentLoS = new List<Vector2> ();

		//For each square, add it to the list, provided it is new
		foreach (Unit unit in completeLoS.Keys)
		{
			foreach (Vector2 position in completeLoS[unit])
			{
				//Check whether position is already in currentLoS
				bool posExists = false;
				foreach (Vector2 checkPos in currentLoS)
				{
					if (position == checkPos)
					{
						posExists = true;
					}
				}
				if (!posExists)
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
				//Check whether givenLoS contains checkingPos
				bool posExists = false;
				foreach (Vector2 position in givenLoS)
				{
					if (position == checkingPos)
					{
						posExists = true;
						break;
					}
				}
				if (!posExists)
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
			bool squareIsValid = false;
			//Check whether the square is in selectableSquares
			foreach (Vector2 square in selectableSquares)
			{
				if (square == position)
				{
					squareIsValid = true;
				}
			}
			if (squareIsValid)
			{
				gameController.deploy (Game.EntityType.GS, position, facing);
				numberOfGSToPlace--;
			}
		}

		if (numberOfGSToPlace == 0)
		{
			currentlyRevealing = false;


		}
	}
}
