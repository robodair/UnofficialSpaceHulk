using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	//Created Ian Mallett 1.9.14

	//Edits
	//Ian Mallett 1.9.14
	//Added Enumerated Types and Method Signatures
	//Assigned moveTransform and facingDierction Dictionaries

	//Ian Mallett 8.9.14
	//Fixed a bug where dictionaries were not declared

	//Ian Mallett 9.9.14
	//Removed Paused GameState, as the gameIsPaused boolean already exists

	//Ian Mallett 10.9.14
	//Changed quaternions from working in X,Z plane to working in X,Y plane
	//so that they would be more efficient to use with Vector2 values.
	//Added reference to the InputOutput class and the NetModule class
	//Added functionality to selectUnit and deselect classes.
	//Added actionAvailable method.
	//Replaced Preturn game state with Deployment and RevealPhase states

	//Ian Mallett 11.9.14
	//Fixed several errors involved with interaction with the InputOutput class.

	//Ian Mallett 14.9.14
	//Fixed a bug where deselection couldn't be outputted, becuase the selectedUnit was null
	//Fixed a bug where reselection occurred because the selectedUnit was not initially null
	//Added a check for whether there is a unit selected when deselecting
	//Fixed a bug where the selectUnit method would almost never pass any ActionTypes to the
	//ioModule, because the state was being set after, rather than before the available actions
	//were found.

	//Ian Mallett 15.9.14
	//Removed the Debug.Log statements checking the selectedUnit value.
	//Added a reference to the Algorithm class
	//Added the unitSelected flag.

	//Ian Mallett 18.9.14
	//Added basic triggers to the game.

	//Ian Mallett 22.9.14
	//Added resetLoS method, and the call to it in Start()
	//Added makeName method, and added functionality to the deploy method.

	//Ian Mallett 23.9.14
	//Added partial functionality to setTurn method.

	//Ian Mallett 25.9.14
	//Added partial functionality to deployment() method.
	//Changed Starting GameState to StartingSMDeployment and StartingGSDeployment
	//Added deployBlip with no functionality method

	//Ian Mallett 26.9.14
	//Continued adding functionality to the placeBlip method.
	//Began adding functionality to the actionPhase and revealPhase methods
	//Removed the nextPhase method, as it does not have any purpose or use.

	//Alisdair Robertson 2.10.14
	// Added a call to the defineBtnEndTurn() method of the InputOutput class whenever the gamestate changes

	//Ian Mallett 2.10.14
	//Added support for CP when checking whether actions are available.

	//Ian Mallett 3.10.14
	//Fixed a bug where the selected unit was not always reselected
	//when the turn was changed.

	//Ian Mallett 4.10.14
	//Added the changeGameState method with defineEndTurn call.
	//Changed all instances where the gameState was manually set with calls to this method. Also removed
	//calls to the defineEndTurn method outside of the changeGameState method.



	/* Game Class
	 * The Game class is the class that stores and manages all the abstract
	 * game data. This means that it stores all the data relating to the
	 * state of the game (excluding the data stored by the map class), as
	 * well as data relating to the player on this instance of the game,
	 * and their actions. This class also stores all the enumerated types
	 * and for movement, stores a pair of dictionaries connecting directions
	 * or rotations to their equivalent Vector2 or quaternion values. Note
	 * that the quaternion values are for calculating changes in Vector2
	 * values, and as such are mostly useless in 3D space, as they dictate
	 * rotations around the Z axis.
	 * This class also contains generic methods for changing data or
	 * incrementing data, such as moving to the next stage in a turn,
	 * selecting and deselecting units, and checking the triggers, which are
	 * hardcoded into this class in each scene.
	 */

	//The state the game is currently in, see code design document for brief description of each.
	public enum GameState{StartingSMDeployment, StartingGSDeployment, DeploymentPhase, RevealPhase,
		Inactive, InactiveSelected, AttackSelection, MoveSelection,
		PerformAction, ShowAction, ActionWait, Reveal, NetworkWait};
	public GameState gameState;

	//The types of player/team
	public enum PlayerType{SM, GS};
	//The type of player that is playing on this instance of the game
	public PlayerType thisPlayer;
	//The player type of the player whose turn it is currently.
	public PlayerType playerTurn;

	//Types of entity
	public enum EntityType{SM, GS, Blip, Door, Square};

	//Types of action. Note that attack is the Genestealer attack, and is therefore melee
	public enum ActionType{Attack, Shoot, Move, ToggleDoor, Overwatch, Reveal};

	//Types of movement
	public enum MoveType{Forward, Right, Left, Back, FrontRight, FrontLeft, BackRight, BackLeft, TurnRight,
		TurnLeft, TurnBack, ForwardTurnRight, ForwardTurnLeft, FrontRightTurnRight, FrontRightTurnLeft,
		FrontLeftTurnRight, FrontLeftTurnLeft, RightTurnRight, RightTurnLeft, LeftTurnRight, LeftTurnLeft,
		BackTurnRight, BackTurnLeft, BackRightTurnRight, BackRightTurnLeft, BackLeftTurnRight, BackLeftTurnLeft};

	/* Stores the relative values by which any given MoveType transforms a unit.
	 * The relative values by which the MoveTypes transform units are stored as an
	 * object[], with the first value being the Vector2 of the relative movement,
	 * wherein Y is forwards, and X is rightwards movement.
	 * The second value is the relative turning amount, represented as a Quaternion.
	 */
	public Dictionary<MoveType, object[]> moveTransform;

	public enum Facing{North, East, South, West};

	public Dictionary<Facing, Quaternion> facingDirection;

	public bool gameIsPaused;

	public bool gameIsMultiplayer;

	//The unit class for the unit that is currently selected.
	public bool unitSelected;
	public Unit selectedUnit;

	public Map gameMap;

	public InputOutput ioModule;

	public Algorithm algorithm;

//	public NetModule network;

	public int remainingCP;

	//
	//Map Specific
	//
	public int blipsPerTurn;


	//Triggers
	private int SMEscaped;
	public Vector2 escapePosition;

	void Start()
	{
		//Assign quaternions to the facings
		facingDirection = new Dictionary<Facing, Quaternion>();
		assignFacingQuaternions();
		//Assign transformations to each of the move types.
		moveTransform = new Dictionary<MoveType, object[]>();
		assignMoveTransforms();

		unitSelected = false;
		//Added 11/9/2014 by Alisdair
		ioModule.generateMap ();
		//Added 14/9/2014 by Alisdair
		ioModule.instantiateUI ();

		//Set the initial data
		resetLoS();
		
		//Triggers
		SMEscaped = 0;
		escapePosition = new Vector2(17, 7);
	}

	public void checkTriggers()
	{
		if (gameMap.isOccupied (escapePosition))
		{
			Unit unit = gameMap.getOccupant (escapePosition);
			if (unit.unitType == EntityType.SM)
			{
				ioModule.removeUnit(escapePosition);
				gameMap.removeUnit(escapePosition);
				SMEscaped++;
			}
		}
		
		if (SMEscaped == 2)
		{
			Debug.Log ("YOU WIN! Have a cookie.");
		}
		else if (gameMap.getMarines().Count == 0)
		{
			Debug.Log ("The genestealers won");
		}
	}

	public void setTurn(PlayerType newPlayer)
	{
		//For a GS turn
		if (newPlayer == PlayerType.GS)
		{
			playerTurn = PlayerType.GS;
			if (thisPlayer == PlayerType.GS)
			{
				resetAP (PlayerType.GS);
				if (blipsPerTurn > 0)
				{
					changeGameState(GameState.DeploymentPhase);
					deployment ();
				}
				else
				{
					changeGameState(GameState.RevealPhase);
					revealPhase ();
				}
			}
			else
			{
				changeGameState(GameState.NetworkWait);
				if (gameIsMultiplayer)
				{
					//Send to the network
				}
				else
				{
					resetAP (PlayerType.GS);
					algorithm.AITurn();
				}
			}
		}
		//For an SM turn
		else
		{
			//Set the player turn
			playerTurn = PlayerType.SM;
			if (thisPlayer == PlayerType.SM)
			{
				resetAP (PlayerType.SM);
				
				//Reselect the currently selected unit
				//and change the Game State.
				if (unitSelected)
				{
					changeGameState(GameState.InactiveSelected);
				}
				else
				{
					changeGameState(GameState.Inactive);
				}
				//Set the player CP
				remainingCP = Random.Range(1, 7);
				if (gameIsMultiplayer)
				{
					//Send to Network
				}
			}
			//If this is the genestealer player
			else
			{
				changeGameState(GameState.NetworkWait);

				//Update the display
			}
		}

		
		//Reselect the unit to match the new gameState
		if (unitSelected)
		{
			selectUnit (selectedUnit.gameObject);
		}
	}

	//Sets the current AP of every unit on the board belonging to the
	//given player to that unit's maximum AP.
	private void resetAP(PlayerType player)
	{
		if (player == PlayerType.SM)
		{
			foreach (Unit marine in gameMap.getMarines())
			{
				marine.AP = UnitData.getMaxAP(EntityType.SM);
			}
		}

		else
		{
			foreach (Square square in gameMap.map)
			{
				if (square.isOccupied)
				{
					if (square.occupant.unitType == EntityType.Blip ||
					    square.occupant.unitType == EntityType.GS)
					{
						square.occupant.AP = UnitData.getMaxAP(square.occupant.unitType);
					}
				}
			}
		}
	}

	public void deployment()
	{
		//Show the deployment phase for the appropriate player
		if (gameState == GameState.DeploymentPhase)
		{
			int[] blipSizes = new int[blipsPerTurn];
			for (int i = 0; i < blipsPerTurn; i++)
			{
				int blipNum = Random.Range (0, 22);
				if (blipNum < 8)
				{
					blipSizes[i] = 1;
				}
				else if (blipNum < 14)
				{
					blipSizes[i] = 2;
				}
				else
				{
					blipSizes[i] = 3;
				}
			}

			//Make ioModule show Genestealer deployment
		}
	}

	public void revealPhase()
	{
		changeGameState(GameState.RevealPhase);

		//Make ioModule show reveal phase													SEND ALICE ISSUE
	}

	public void actionPhase()
	{
		if (unitSelected)
		{
			changeGameState(GameState.InactiveSelected);
			selectUnit(selectedUnit.gameObject);
		}
		else
		{
			changeGameState(GameState.Inactive);
		}

		//Make ioModule show main phase
	}

	public void deploy(EntityType unitType, Vector2 position, Facing facing)
	{
		Unit placeUnit = new Unit (makeName(unitType), unitType, position, facing);
		placeUnit.AP = UnitData.getMaxAP (unitType);
		gameMap.placeUnit (placeUnit);
		ioModule.placeUnit (placeUnit);
	}

	//Method for placing a blip in a deploymentArea. The deploymentArea
	//parameter is the index of the deploymentArea in the map.
	public void deployBlip(int deploymentArea, int gsInBlip)
	{
		if (gameState == GameState.DeploymentPhase)
		{
			if (gameMap.otherAreas.Length > deploymentArea)
			{
				DeploymentArea targetArea = gameMap.otherAreas[deploymentArea];
				Unit blip = new Unit("Blip", EntityType.Blip, new Vector2(-1 - deploymentArea, 0), targetArea.relativePosition);
				gameMap.placeUnit (blip);
				ioModule.placeUnit (blip);
			}
			else
			{
				Debug.LogError("No such deploymentArea for \"deployBlip\" method");
			}
		}
		else
		{
			Debug.LogError("\"deployBlip\" method called in an invalid GameState");
		}
	}

	public void selectUnit(GameObject model)
	{
		//If reselection is allowed
		if (gameState != GameState.AttackSelection &&
		    gameState != GameState.MoveSelection)
		{
			//Deselect any previous unit
			if (unitSelected)
			{
				deselect();
			}
			//Get the unit and get its set of actions
			Unit unit = gameMap.getUnit(model);

			if (unit != null)
			{
				//Change the gameState
				if (gameState == GameState.Inactive)
				{
					changeGameState(GameState.InactiveSelected);
				}

				//Find the unit's actions
				ActionType[] actionSet = UnitData.getActionSet(unit.unitType);
				List<ActionType> availableActions = new List<ActionType>();
				for (int i = 0; i < actionSet.Length; i++)
				{
					if (actionAvailable(unit, actionSet[i]))
					{
						availableActions.Add(actionSet[i]);
					}
				}

				//Select the unit
				selectedUnit = unit;
				unitSelected = true;
				Debug.Log ("Selecting Unit");
				ioModule.selectUnit(unit, availableActions.ToArray());
			}
			//The unit doesn't exist
			else
			{
				Debug.Log("Could not find the unit to match appropriate " +
				          "model for \"selectUnit\" method");
			}
		}
		//(Re)Selection not allowed
		else
		{
			Debug.LogError("Selection was attempted while in an " +
			               "invalid game state.");
		}
	}

	//Deselect the current model. Change the mode to match, if necessary.
	public void deselect()
	{
		//If deselection is allowed
		if (gameState != GameState.MoveSelection &&
		    gameState != GameState.AttackSelection &&
		    gameState != GameState.Inactive)
		{
			if (unitSelected)
			{
				if (gameState == GameState.InactiveSelected)
				{
					changeGameState(GameState.Inactive);
				}
				ioModule.deselect();
				unitSelected = false;
				selectedUnit = null;
			}
		}
	}

	//Checks whether the unit at the position has the action available,
	//however, the method does not check whether the unit can perform
	//the action in general.
	private bool actionAvailable(Unit unit, ActionType action)
	{
		//Most actions are only available in the InactiveSelected state
		if (gameState == GameState.InactiveSelected)
		{
			if (unit.AP >= UnitData.getAPCost(action) ||
			   (thisPlayer == PlayerType.SM && unit.AP + remainingCP >= UnitData.getAPCost (action)))
			{
				switch (action)
				{
					case ActionType.Attack:
						//There must be a valid target directly in front
						Unit potentialTarget = gameMap.getOccupant(unit.position + ((Vector2)(facingDirection[unit.facing]*Vector2.up)));
						if (potentialTarget != null)
						{
							if (potentialTarget.unitType == EntityType.SM ||
						    	potentialTarget.unitType == EntityType.Door)
							{
								return true;
							}
						}
						break;

					case ActionType.Shoot:
						//Always available if AP allows
						return true;

					case ActionType.Move:
						//Always available if AP allows
						return true;

					case ActionType.ToggleDoor:
						//There must be a door directly in front
						if (gameMap.hasDoor(unit.position + ((Vector2)(facingDirection[unit.facing]*Vector2.up))))
					    {
							return true;
						}
						break;

					case ActionType.Overwatch:
						//Always available if AP allows
						return true;

					default:
						//Otherwise the action isn't allowed
						return false;
				}
			}
		}

		//Reveal is only available within the reveal phase
		if (gameState == GameState.RevealPhase)
		{
			if (action == ActionType.Reveal)
			{
				//Always available
				return true;
			}
		}

		//If none of the above, return false
		return false;

	}

	public void setPauseState(bool pauseState)
	{

	}

	public void resetLoS()
	{
		foreach (Unit unit in gameMap.getMarines ())
		{
			unit.currentLoS = algorithm.findLoS(unit);
		}
	}

	public void changeGameState(GameState newState)
	{
		gameState = newState;
		ioModule.defineEndTurnBtn();
	}

	//Randomly chooses a name from a set of names
	private string makeName(EntityType unitType)
	{
		if (unitType.Equals (EntityType.SM))
		{
			switch (Random.Range(0, 6))
			{
				case 0:
					return "Derpy";
				case 1:
					return "Nrick";
				case 2:
					return "Block Head";
				case 3:
					return "Brainiac";
				case 4:
					return "Omnio";
				case 5:
					return "Phteven";
				default:
					return "Ian";
			}
		}
		else if (unitType.Equals (EntityType.GS))
		{
			switch (Random.Range (0, 7))
			{
				case 0:
					return "Biter";
				case 1:
					return "Ankle-Biter";
				case 2:
					return "Claw";
				case 3:
					return "Jaws";
				case 4:
					return "Hungry";
				case 5:
					return "Fluffy";
				case 6:
					return "Mangler";
				default:
					return "Cute";
			}
		}
		else if (unitType.Equals (EntityType.Blip))
		{
			return "Blip";
		}
		else
		{
			return "Name";
		}
	}

	//Assign quaternion directions to each of the Facings in the facingDirection dictionary
	private void assignFacingQuaternions()
	{
		facingDirection.Add(Facing.North, Quaternion.Euler (0, 0, 0));
		facingDirection.Add(Facing.East, Quaternion.Euler (0, 0, 270));
		facingDirection.Add(Facing.South, Quaternion.Euler (0, 0, 180));
		facingDirection.Add(Facing.West, Quaternion.Euler (0, 0, 90));
	}

	//Assign transformations to each of the move types in the moveTranform dictionary.
	private void assignMoveTransforms()
	{
		//Forward
		object[] forward = {new Vector2(0, 1), facingDirection[Facing.North]};
		moveTransform.Add (MoveType.Forward, forward);
		
		//Right
		object[] right = {new Vector2(1, 0), facingDirection[Facing.North]};
		moveTransform.Add (MoveType.Right, right);
		
		//Left
		object[] left = {new Vector2(-1, 0), facingDirection[Facing.North]};
		moveTransform.Add (MoveType.Left, left);
		
		//Back
		object[] back = {new Vector2(0, -1), facingDirection[Facing.North]};
		moveTransform.Add (MoveType.Back, back);
		
		//FrontRight
		object[] frontRight = {new Vector2(1, 1), facingDirection[Facing.North]};
		moveTransform.Add (MoveType.FrontRight, frontRight);
		
		//FrontLeft
		object[] frontLeft = {new Vector2(-1, 1), facingDirection[Facing.North]};
		moveTransform.Add (MoveType.FrontLeft, frontLeft);
		
		//BackRight
		object[] backRight = {new Vector2(1, -1), facingDirection[Facing.North]};
		moveTransform.Add (MoveType.BackRight, backRight);
		
		//BackLeft
		object[] backLeft = {new Vector2(0, 1), facingDirection[Facing.North]};
		moveTransform.Add (MoveType.BackLeft, backLeft);
		
		//TurnRight
		object[] turnRight = {new Vector2(0, 0), facingDirection[Facing.East]};
		moveTransform.Add (MoveType.TurnRight, turnRight);
		
		//TurnLeft
		object[] turnLeft = {new Vector2(0, 0), facingDirection[Facing.West]};
		moveTransform.Add (MoveType.TurnLeft, turnLeft);
		
		//TurnBack
		object[] turnBack = {new Vector2(0, 0), facingDirection[Facing.South]};
		moveTransform.Add (MoveType.TurnBack, turnBack);
		
		//ForwardTurnRight
		object[] forwardTurnRight = {new Vector2(0, 1), facingDirection[Facing.East]};
		moveTransform.Add (MoveType.ForwardTurnRight, forwardTurnRight);
		
		//ForwardTurnLeft
		object[] forwardTurnLeft = {new Vector2(0, 1), facingDirection[Facing.West]};
		moveTransform.Add (MoveType.ForwardTurnLeft, forwardTurnLeft);
		
		//FrontRightTurnRight
		object[] frontRightTurnRight = {new Vector2(1, 1), facingDirection[Facing.East]};
		moveTransform.Add (MoveType.FrontRightTurnRight, frontRightTurnRight);
		
		//FrontRightTurnLeft
		object[] frontRightTurnLeft = {new Vector2(1, 1), facingDirection[Facing.West]};
		moveTransform.Add (MoveType.FrontRightTurnLeft, frontRightTurnLeft);
		
		//FrontLeftTurnRight
		object[] frontLeftTurnRight = {new Vector2(-1, 1), facingDirection[Facing.East]};
		moveTransform.Add (MoveType.FrontLeftTurnRight, frontLeftTurnRight);
		
		//FrontLeftTurnLeft
		object[] frontLeftTurnLeft = {new Vector2(-1, 1), facingDirection[Facing.West]};
		moveTransform.Add (MoveType.FrontLeftTurnLeft, frontLeftTurnLeft);
		
		//RightTurnRight
		object[] rightTurnRight = {new Vector2(1, 0), facingDirection[Facing.East]};
		moveTransform.Add (MoveType.RightTurnRight, rightTurnRight);
		
		//RightTurnLeft
		object[] rightTurnLeft = {new Vector2(1, 0), facingDirection[Facing.West]};
		moveTransform.Add (MoveType.RightTurnLeft, rightTurnLeft);
		
		//LeftTurnRight
		object[] leftTurnRight = {new Vector2(-1, 0), facingDirection[Facing.East]};
		moveTransform.Add (MoveType.LeftTurnRight, leftTurnRight);
		
		//LeftTurnLeft
		object[] leftTurnLeft = {new Vector2(-1, 0), facingDirection[Facing.West]};
		moveTransform.Add (MoveType.LeftTurnLeft, leftTurnLeft);
		
		//BackTurnRight
		object[] backTurnRight = {new Vector2(0, -1), facingDirection[Facing.East]};
		moveTransform.Add (MoveType.BackTurnRight, backTurnRight);
		
		//BackTurnLeft
		object[] backTurnLeft = {new Vector2(0, -1), facingDirection[Facing.West]};
		moveTransform.Add (MoveType.BackTurnLeft, backTurnLeft);
		
		//BackRightTurnRight
		object[] backRightTurnRight = {new Vector2(1, -1), facingDirection[Facing.East]};
		moveTransform.Add (MoveType.BackRightTurnRight, backRightTurnRight);
		
		//BackRightTurnLeft
		object[] backRightTurnLeft = {new Vector2(1, -1), facingDirection[Facing.West]};
		moveTransform.Add (MoveType.BackRightTurnLeft, backRightTurnLeft);
		
		//BackLeftTurnRight
		object[] backLeftTurnRight = {new Vector2(-1, -1), facingDirection[Facing.East]};
		moveTransform.Add (MoveType.BackLeftTurnRight, backLeftTurnRight);
		
		//BackLeftTurnLeft
		object[] backLeftTurnLeft = {new Vector2(-1, -1), facingDirection[Facing.West]};
		moveTransform.Add (MoveType.BackLeftTurnLeft, backLeftTurnLeft);
	}
}
