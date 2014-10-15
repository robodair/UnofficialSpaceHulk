using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//
public class InputHandler : MonoBehaviour {
	
	public Game gameController;
    public GameObject moveTarget;
	public GameObject attackTarget;
    public Map mapController;

	public RevealManager revealManager;
	Vector2 revealPosition;

	public InputOutput ioController;
	Vector2 moveTargetSquare;
	Dictionary<Square, int> availableSquares;

	//RB 8.10.14 Added for paths to specific facings checks
	Path newPath;
	bool north = false;
	bool east = false;
	bool south = false;
	bool west = false;

	public bool coloursSet = false;

	bool foundTarget = false;
	public bool facingInProgress = false;//RB 2.10.14 for stopping simultaneous movements

	//Sets the GameState to MoveSelection, enabling user to start inputting the move command
	public void movement()
	{
		availableSquares = gameController.algorithm.availableSquares (gameController.selectedUnit);
		gameController.changeGameState(Game.GameState.MoveSelection);
		gameController.selectUnit (gameController.selectedUnit.gameObject);
		foundTarget = false;
		showAvailableSquares ();
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


				if (squareAvailable(moveTargetSquare))
				{
					//RB 8.10.14 Paths to specific facings checks
					newPath = new Path(gameController.algorithm.getPath (gameController.selectedUnit.position, gameController.selectedUnit.facing, 
					                                                          moveTargetSquare, Game.Facing.North, 
					                                                          UnitData.getMoveSet (gameController.selectedUnit.unitType)));
					if (newPath.APCost <= gameController.selectedUnit.AP ||
					   	(gameController.thisPlayer == Game.PlayerType.SM && 
			 			 newPath.APCost <= gameController.selectedUnit.AP + gameController.remainingCP))
					{
						north = true;
					}
					else
						north = false;
					newPath = new Path(gameController.algorithm.getPath (gameController.selectedUnit.position, gameController.selectedUnit.facing, 
					                                                     moveTargetSquare, Game.Facing.East, 
					                                                     UnitData.getMoveSet (gameController.selectedUnit.unitType)));
					if (newPath.APCost <= gameController.selectedUnit.AP ||
					    (gameController.thisPlayer == Game.PlayerType.SM && 
					 newPath.APCost <= gameController.selectedUnit.AP + gameController.remainingCP))
					{
						east = true;
					}
					else
						east = false;
					newPath = new Path(gameController.algorithm.getPath (gameController.selectedUnit.position, gameController.selectedUnit.facing, 
					                                                     moveTargetSquare, Game.Facing.South, 
					                                                     UnitData.getMoveSet (gameController.selectedUnit.unitType)));
					if (newPath.APCost <= gameController.selectedUnit.AP ||
					    (gameController.thisPlayer == Game.PlayerType.SM && 
					 newPath.APCost <= gameController.selectedUnit.AP + gameController.remainingCP))
					{
						south = true;
					}
					else south = false;
					newPath = new Path(gameController.algorithm.getPath (gameController.selectedUnit.position, gameController.selectedUnit.facing, 
					                                                     moveTargetSquare, Game.Facing.West, 
					                                                     UnitData.getMoveSet (gameController.selectedUnit.unitType)));
					if (newPath.APCost <= gameController.selectedUnit.AP ||
					    (gameController.thisPlayer == Game.PlayerType.SM && 
					 newPath.APCost <= gameController.selectedUnit.AP + gameController.remainingCP))
					{
						west = true;
					}
					else
						west = false;

					foundTarget = true;
					ioController.instantiateFacingSelection (moveTargetSquare, north, east, south, west);
					facingInProgress = true;

					hideAvailableSquares();
				}
			}
		}
	}

	//RB 2.10.14
	//Support for highlighting checks in Interactible class
	public bool squareAvailable(Vector2 target)
	{
		if(gameController.unitSelected)
		{
			availableSquares = gameController.algorithm.availableSquares (gameController.selectedUnit);
			foreach (Square square in availableSquares.Keys)
			{
				if(target == square.position)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void showAvailableSquares ()
	{
		if(gameController.unitSelected)
		{
			availableSquares = gameController.algorithm.availableSquares (gameController.selectedUnit);
		
			//RB 8.10.14 Changes colour of all squares available to move to
			foreach (Square square in availableSquares.Keys)
			{
				square.model.renderer.material.color = Color.green;
			}
			coloursSet = true;
		}
	}

	public void hideAvailableSquares()
	{
		if(gameController.unitSelected)
		{
			foreach (Square square in availableSquares.Keys)
			{
				square.model.renderer.material.color = Color.white;
			}
			coloursSet = false;
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
		
		gameController.changeGameState(Game.GameState.Inactive);
		
		//if (gameController.unitSelected)
		//	gameController.deselect ();
		facingInProgress = false;
		//ioController.resetMap (); COmmented out for testing Alisdair 11-10-14
	}

	//Sets the GameState to AttackSelection, enabling user to start inputting the attack command
	public void attack()
	{
		gameController.changeGameState(Game.GameState.AttackSelection);
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
			gameController.changeGameState(Game.GameState.InactiveSelected);
			
			//ioController.resetMap();
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
		//ioController.resetMap ();
	}

	public void revealing (Vector2 position, List<Vector2> selectableSquares)
	{
		revealPosition = position;

	}

	public void revealOrientationClicked(Game.Facing facing)
	{
		revealManager.place (revealPosition, facing);
	}
}