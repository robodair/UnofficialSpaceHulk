using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//
public class InputHandler : MonoBehaviour {
	
	public Game gameController;
    public GameObject moveTarget;
	public GameObject attackTarget;
    public Map mapController;

	public InputOutput ioController;
	Vector2 moveTargetSquare;
	Dictionary<Square, int> availableSquares;
	bool foundTarget = false;
	public bool facingInProgress = false;//RB 2.10.14 for stopping simultaneous movements

	//Sets the GameState to MoveSelection, enabling user to start inputting the move command
	public void movement()
	{
		availableSquares = gameController.algorithm.availableSquares (gameController.selectedUnit);
		gameController.gameState = Game.GameState.MoveSelection;
		ioController.defineEndTurnBtn(); //Define the state of the end turn button Alisdair 2-10-14
		foundTarget = false;
		//ioController.showAvailableSquares(); 
		//For when there's some sort of thing to show which squares are available
	}

	//Starts the movement action, setting the target positions and bringing up the facing 
	//selection buttons where the user clicked
	public void moving()
	{
		if (!facingInProgress)
		{
			if (!foundTarget)
			{
				Vector3 moveTargetVector = moveTarget.transform.position;
				moveTargetSquare = new Vector2(moveTargetVector.x, moveTargetVector.z);

				foreach(Square square in availableSquares.Keys)
				{
					if (moveTargetSquare == square.position)
					{
						foundTarget = true;
						ioController.instantiateFacingSelection (moveTargetSquare);
						facingInProgress = true;
						break;
					}
				}
			}
		}
	}
	
	//Once the facing selection has been done, creates an ActionManager to handle the movement
	public void orientationClicked (Game.Facing facing)
	{
		ActionManager actionManager = new ActionManager (gameController.selectedUnit, Game.ActionType.Move);

		actionManager.path = gameController.algorithm.getPath (gameController.selectedUnit.position, gameController.selectedUnit.facing, 
		                                                       moveTargetSquare, facing, 
		                                                       UnitData.getMoveSet (gameController.selectedUnit.unitType));
		actionManager.performAction();
		
		gameController.gameState = Game.GameState.Inactive;
		ioController.defineEndTurnBtn(); //Define the state of the end turn button Alisdair 2-10-14
		
		//if (gameController.unitSelected)
		//	gameController.deselect ();
		facingInProgress = false;
		ioController.resetMap ();
	}

	//Sets the GameState to AttackSelection, enabling user to start inputting the attack command
	public void attack()
	{
		gameController.gameState = Game.GameState.AttackSelection;
		ioController.defineEndTurnBtn(); //Define the state of the end turn button Alisdair 2-10-14
		
		Debug.LogWarning (gameController.gameState);
	}

	public void attacking()
	{
		//do stuffs
	}

	public void shooting()
	{
		//do stuffs
		bool inLoS = false;
		foreach (Vector2 location in gameController.algorithm.findLoS(gameController.selectedUnit))
		{
			if (mapController.findUnit(attackTarget) == location)
			{
				inLoS = true;
			}
		}
		if(inLoS)
		{
			ActionManager actionManager = new ActionManager (gameController.selectedUnit, Game.ActionType.Shoot);
			actionManager.target = mapController.getUnit(attackTarget);
			actionManager.performAction();
			gameController.gameState = Game.GameState.InactiveSelected;
		ioController.defineEndTurnBtn(); //Define the state of the end turn button Alisdair 2-10-14
			
			ioController.resetMap();
		}
		else
		{
			Debug.LogWarning ("Target not in Line of Sight!");
		}
	}

	public void toggleDoor()
	{
		ActionManager actionManager = new ActionManager (gameController.selectedUnit, Game.ActionType.ToggleDoor);
		actionManager.performAction ();
		ioController.resetMap ();
	}
}