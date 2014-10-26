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
	public Vector2 revealPosition;
	public List<Vector2> selectableRevealPositions;
	public bool allowRevealSelection = false;

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
		if(gameController.unitSelected &&
		   coloursSet)
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
		facingInProgress = false;
	}

	//Sets the GameState to AttackSelection, enabling user to start inputting the attack command
	public void attack()
	{
		//gameController.changeGameState(Game.GameState.AttackSelection);
		gameController.selectUnit (gameController.selectedUnit.gameObject);
		Unit potentialTarget = mapController.getOccupant(gameController.selectedUnit.position + 
		                                                 ((Vector2)(gameController.facingDirection[gameController.selectedUnit.facing]*Vector2.up)));
		if (potentialTarget != null)
		{
			ActionManager actionManager = new ActionManager (gameController.selectedUnit, Game.ActionType.Attack);
			actionManager.target = potentialTarget;
			actionManager.performAction();
		}
	}

	public void shoot()
	{
		gameController.changeGameState(Game.GameState.AttackSelection);
		gameController.selectUnit (gameController.selectedUnit.gameObject);
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
	}

	public void overwatchClicked()
	{
		if(!gameController.selectedUnit.isOnOverwatch)
		{
			ActionManager actionManager = new ActionManager (gameController.selectedUnit, Game.ActionType.Overwatch);
			actionManager.performAction ();
		}
	}

	public void revealing (Vector2 position, List<Vector2> selectableSquares)
	{
		revealPosition = position;
		selectableRevealPositions = selectableSquares;
		showSelectableRevealSquares ();
		mapController.getSquare (revealPosition).model.renderer.material.color = Color.cyan;
		ioController.instantiateFacingSelection (revealPosition);
	}

	public void revealOrientationClicked(Game.Facing facing)
	{
		hideSelectableRevealSquares ();
		mapController.getSquare (revealPosition).model.renderer.material.color = Color.white;
		revealManager.place (revealPosition, facing);
		allowRevealSelection = true;
		if (revealManager.currentlyRevealing)
		{
			showSelectableRevealSquares ();
		}
	}

	public void continueRevealing()
	{
		mapController.getSquare (revealPosition).model.renderer.material.color = Color.cyan;
		ioController.instantiateFacingSelection (revealPosition);
		allowRevealSelection = false;
	}

	public void showSelectableRevealSquares()
	{
		foreach (Vector2 position in selectableRevealPositions)
		{
	        mapController.getSquare(position).model.renderer.material.color = new Color(0.68f, 0.51f, 0.69f);
		}
	}

	public void hideSelectableRevealSquares()
	{
		foreach (Vector2 position in selectableRevealPositions)
		{
			if(mapController.getSquare (position).model.renderer.material.color != Color.white)
				mapController.getSquare(position).model.renderer.material.color = Color.white;
		}
	}
}