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

	//Ian Mallett 22.10.14
	//Completed functionality of the place method
	//Made the manager change the game state to Reveal while revealing
	//Added involuntary and actionManager variables to handle overwatch shots against revealing Genestealers

	public bool currentlyRevealing;
	public int numberOfGS;
	public int numberOfGSToPlace;
	public Vector2 centralPosition;
	public List<Vector2> selectableSquares;
	public Game gameController;
	public InputHandler inputHandler;
	private bool involuntary;
	private ActionManager actionManager;

	public void involuntaryReveal(Vector2 blipPosition, ActionManager actionManager, Dictionary<Unit, List<Vector2>> prevLoS)
	{
		//Set the initial values
		Unit blip = null;
		if (gameController.gameMap.isOccupied (blipPosition))
		{
			blip = gameController.gameMap.getOccupant(blipPosition);
		}
		
		numberOfGS = blip.noOfGS;
		numberOfGSToPlace = numberOfGS;
		currentlyRevealing = true;
		centralPosition = blipPosition;
		involuntary = true;
		gameController.changeGameState (Game.GameState.Reveal);
		this.actionManager = actionManager;

		//Reselect the unit to make the buttons unavailable
		if (gameController.unitSelected)
		{
			gameController.selectUnit (gameController.selectedUnit.gameObject);
		}


		selectableSquares = findSelectableSquares (prevLoS);

		//Override number of genestealers if not enough square
		if (selectableSquares.Count < numberOfGSToPlace)
		{
			numberOfGSToPlace = selectableSquares.Count;
		}

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
		//Override if there is only one genestealer to place, the only
		//selectable square is the central one.
		if (numberOfGSToPlace != 1)
		{
			//Find the bottom left corner of the 3x3 grid
			Vector2 startingPos = centralPosition - Vector2.one;
			List<Vector2> returnList = new List<Vector2>();

			//Check each square in the 3x3 grid to be linked to the
			//central square, and not visible.
			for (int i = 0; i < 9 ; i++)
			{
				Vector2 checkingPos = startingPos + (i % 3) * Vector2.right + (i / 3) * Vector2.up;
				//Only add the square if the square exists and is not occupied. Override this
				//if the position is the position of the blip.
				if (gameController.gameMap.hasSquare(checkingPos) &&
				    (!gameController.gameMap.isOccupied(checkingPos) ||
				    checkingPos == centralPosition))
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
		else
		{
			List<Vector2> returnList = new List<Vector2>();
			returnList.Add (centralPosition);
			return returnList;
		}
	}

	public void place(Vector2 position, Game.Facing facing)
	{
		if (currentlyRevealing)
		{
			//Place the genestealer
			bool squareIsValid = false;
			//Check whether the square is in selectableSquares
			for (int i = 0; i < selectableSquares.Count; i++)
			{
				if (selectableSquares[i] == position)
				{
					squareIsValid = true;
					selectableSquares.RemoveAt(i);
					break;
				}
			}
			if (squareIsValid)
			{
				gameController.deploy (Game.EntityType.GS, position, facing);
				numberOfGSToPlace--;
			}
		}

		//If there are no genestealers left
		if (numberOfGSToPlace == 0)
		{
			//Finish the reveal, and continue the game
			currentlyRevealing = false;
			gameController.changeGameState (Game.GameState.Inactive);
			if (gameController.unitSelected)
			{
				gameController.selectUnit (gameController.selectedUnit.gameObject);
			}
			//Check for overwatch shots
			if (involuntary)
			{
				if (gameController.gameMap.isOccupied (centralPosition))
				{
					actionManager.postInvolReveal(gameController.gameMap.getOccupant (centralPosition));
				}
			}
			involuntary = false;
			//Unpause animations
			gameController.ioModule.continueActionSequence();
		}
	}
}
