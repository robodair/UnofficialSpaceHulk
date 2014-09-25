using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//
public class InputHandler : MonoBehaviour {
	
	public Game gameController;
    public GameObject moveTarget;
    public Map mapController;

	public InputOutput ioController;
	Vector2 moveTargetSquare;
	Dictionary<Square, int> availableSquares;

	//Sets the GameState to MoveSelection, enabling user to start inputting the move command
	public void movement()
	{
		availableSquares = gameController.algorithm.availableSquares (gameController.selectedUnit);
		foreach(Square square in availableSquares.Keys)
		{
			Debug.LogWarning (square.position);
		}
		gameController.gameState = Game.GameState.MoveSelection;
		Debug.LogWarning (gameController.gameState);
		//ioController.showAvailableSquares(); 
		//For when there's some sort of thing to show which squares are available
	}

	//Starts the movement action, setting the target positions and bringing up the facing 
	//selection buttons where the user clicked
	public void moving()
	{
		Vector3 moveTargetVector = moveTarget.transform.position;
		moveTargetSquare = new Vector2(moveTargetVector.x, moveTargetVector.z);

		foreach(Square square in availableSquares.Keys)
		{
			if (moveTargetSquare == square.position)
			{
				Debug.LogWarning (moveTarget);
				Debug.LogWarning (moveTargetVector);
				Debug.LogWarning (moveTargetSquare);
				ioController.instantiateFacingSelection (moveTargetSquare);
				break;
			}
		}
	}
	
	//Once the facing selection has been done, creates an ActionManager to handle the movement, 
	//then SHOULD deselect the unit and set the GameState back to Inactive
	public void orientationClicked (Game.Facing facing)
	{
		Debug.Log ("I got to here");
		ActionManager actionManager = new ActionManager (gameController.selectedUnit, Game.ActionType.Move);

		actionManager.path = gameController.algorithm.getPath (gameController.selectedUnit.position, gameController.selectedUnit.facing, 
		                                                       moveTargetSquare, facing, 
		                                                       UnitData.getMoveSet (gameController.selectedUnit.unitType));
		actionManager.performAction();
		
		Debug.Log ("I actually got to here");
		gameController.gameState = Game.GameState.Inactive;
		if (gameController.unitSelected != null)
			gameController.deselect ();
		Debug.LogWarning (gameController.gameState);
		Debug.LogWarning (gameController.selectedUnit.position);
		ioController.resetMap ();
	}

	//Sets the GameState to AttackSelection, enabling user to start inputting the attack command
	public void attack()
	{
		gameController.gameState = Game.GameState.AttackSelection;
		Debug.LogWarning (gameController.gameState);
	}
}