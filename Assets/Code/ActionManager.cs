﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionManager {

	//Created by Ian Mallett 1.9.14
	//modified By Nick Lee 23.10.14
	public Unit target;
	public Path path;

	private Game.MoveType Movement;//Created by Nick Lee 18-9-14
	private Unit unit; //Created by Nick Lee 16-9-14
	private Game.ActionType actionUsed; //Created by Nick Lee 16-9-14
	private Game game;//Created by Nick Lee 16-9-14
	private Vector2 moving;//Created by Nick Lee 16-9-14
	private Game.Facing compassFacing;//Created by Nick Lee 16-9-14
	private List<Unit> marines = new List<Unit> ();//Created by Nick Lee 18-9-14
	private List<Unit> blips = new List<Unit> ();//Created by Nick Lee 15-10-14
	private List<Unit> blipsRevealed = new List<Unit> ();//Created by Nick Lee 15-10-14
	private List<Unit> marinesShot = new List<Unit> ();//Created by Nick Lee 31-10-14
	private bool shot = false;//Created by Nick Lee 18-9-14
	private bool overwatchShot = false;//Created by Nick Lee 18-9-14
	private bool attackMove = false;//Created by Nick Lee 18-9-14
	private bool sittingStill = false;//Created by Nick Lee 5-11-14
	private Path customPath;//Created by Nick Lee 18-9-14
	private List<Action> actions = new List<Action> (); //created by Nick Lee 22-9-14
	private Action returnAction = new Action (); //created by Nick Lee 23-9-14
	private bool movementStopped = false; //Created by Nick Lee 7-10-14
	private Unit involUnit; //Created by Nick Lee 7-10-14

	//modified by Nick Lee 13-10-14
	private Game.ActionType actionType;
	private Unit executor;
	private Unit executie;
	private Unit preExecutor;
	private Vector2 movePosition; 
	private Game.Facing moveFacing;
	private int APCost;
	private bool unitJams;
	private List<Unit> destroyedUnits = new List<Unit> ();
	private List<Unit> sustainedFireLost = new List<Unit> ();
	private Dictionary<Unit, List<Vector2>> completeLoS = new Dictionary<Unit, List<Vector2>> ();
	private Dictionary<Unit, Unit> sustainedFireChanged = new Dictionary<Unit, Unit> ();
	private List<Unit> lostOverwatch = new List<Unit> ();
	private Dictionary<Game.PlayerType, int[]> dieRolled = new Dictionary<Game.PlayerType, int[]> ();
	private Dictionary<Unit, List<Vector2>> prevLoS = new Dictionary<Unit, List<Vector2>> ();

	public ActionManager(Unit unit, Game.ActionType action)	//Contents modified by Nick Lee 16-9-14
	{
		GameObject gameController = GameObject.FindWithTag ("GameController");
		game = gameController.GetComponent<Game> ();
		//gets local game controller
		actionUsed = action; //gets the action
		this.unit = unit; //the unit using the action
		marines = game.gameMap.getUnits (Game.EntityType.SM);
		makePrevLoS ();
	}

	public void performAction() //Contents modified by Nick Lee 20-10-14
	{
		if (actionUsed == Game.ActionType.Move) {
			moveMethod(unit); //if action is a movement
		}
		else if (actionUsed == Game.ActionType.Attack) {
			removeAP (unit, UnitData.getAPCost (actionUsed));
			attackMethod(unit, target); //if action is a melee attack, requires target
		}
		else if (actionUsed == Game.ActionType.Shoot) {
			removeAP (unit, UnitData.getAPCost (actionUsed));
			shootMethod(unit, target); //if action is shooting, requires target
		}
		else if (actionUsed == Game.ActionType.ToggleDoor) {
			removeAP (unit, UnitData.getAPCost (actionUsed));
			toggleDoorMethod(unit); //if action is toggling a door
		}
		else if (actionUsed == Game.ActionType.Overwatch) {
			removeAP (unit, UnitData.getAPCost (actionUsed));
			overwatchMethod(unit); //if action is setting a unit to overwatch
		} else
			Debug.Log ("Error with action type , ActionManager");
		//error message and catching
	}

	private void update(Game.ActionType actionUpdate, Unit exe)//Created by Nick Lee 16-9-14, modified 4-11-14
	{
		updateLoS ();
		
		makeActions (actionUpdate, exe);//make an action array
		
		marines = game.gameMap.getUnits (Game.EntityType.SM);
		//makes a list of all marine units
		if (executor.unitType == Game.EntityType.GS && !shot) { //if action is made by a genestealer
			for (int i = 0; i < marines.Count; i++) { //then for each marine
				for (int q = 0; q < marines[i].currentLoS.Count; q++) { //then for each square
					if(marines[i].currentLoS[q] == executor.position && marines[i].isOnOverwatch && !marinesShot.Contains(marines[i]))
					{
						overwatchShot = true; //set overwatch shot to true
						shot = true; //set shot equal to true
						preExecutor = executor;
						shootMethod (marines[i], executor); //And run a shoot action against the genestealer
						marinesShot.Add (marines[i]);
						executor = preExecutor;
						shot = false;
						overwatchShot = false; //set overwatch shot to false
					}
				}
			}
		}
		marinesShot.Clear ();
		
		marines = game.gameMap.getUnits (Game.EntityType.SM);
		//makes a list of all marine units
		blips = game.gameMap.getUnits (Game.EntityType.Blip);
		//makes a list of all blip units
		for (int i = 0; i < marines.Count; i++) { //then for each marine
			for (int t = 0; t < blips.Count; t++) {//and each blip
				for (int u = 0; u < marines[i].currentLoS.Count; u++) {//and each square in sight
					if(marines[i].currentLoS[u] == blips[t].position && !blipsRevealed.Contains(blips[t]))
					{ //and the blip isnt already in the list but is within a marines LoS
						involUnit = executor;
						blipsRevealed.Add (blips[t]);
						movementStopped = true;
						//stops any further movement
					}
				}
			}
		}
		foreach (Unit blip in blipsRevealed) {
			InvoluntaryReveal (blip); //runs the involuntary reveal for every blip revealed
		}
		blipsRevealed.Clear ();
		
		makePrevLoS (); //makes the previous line of sight
	}

	private void postAction()//Created by Nick Lee 18-9-14, modified 23-10-14
	{
		game.ioModule.showActionSequence(actions.ToArray (), this); //gives the action array to the input output module
		if(actions.Count > 0)
			game.checkTriggers (actions[actions.Count - 1]); //checks the game for winning triggers
		actions = new List<Action> ();
		//makes a new version of the actions list
	}

	private void moveMethod(Unit mover)//Created by Nick Lee 16-9-14, modified 5-11-14
	{
		Path currentPath; //makes a path variable
		if (!attackMove) {
			currentPath = path;
		//if it isnt a movement caused by a melee attack
		} else {
			currentPath = customPath;
			attackMove = false;
			//else sets attackmove to false and gets the path made by the attack
		}
		//sets the path to iterate through
		if (currentPath.path.Count == 0) {
			moving = mover.position;
			compassFacing = mover.facing;
			sittingStill = true;
			update (Game.ActionType.Move, mover);
		} else {
			for (int i = 0; i < currentPath.path.Count; i++) { //iterates through all movements in the path
				if (!movementStopped) { //if the unit wasn't killed by overwatch
					Movement = currentPath.path [i];

					removeAP (mover, UnitData.getMoveSet (mover.unitType) [Movement]);//removes required AP from unit
					moving = (Vector2)game.moveTransform [Movement] [0]; //gets the object from the dictionary and converts to a vector2
					moving = game.facingDirection [mover.facing] * moving;
					moving = mover.position + moving; //gets final position

					Quaternion direction = game.facingDirection [mover.facing] * ((Quaternion)game.moveTransform [Movement] [1]);
					//gets the quaternion from the current facing and the required movement
					if (Mathf.Abs (direction.eulerAngles.z - 0) < 0.1f) {
							compassFacing = Game.Facing.North;	//changes facing to north
					} else if (Mathf.Abs (direction.eulerAngles.z - 270) < 0.1f) {
							compassFacing = Game.Facing.East;	//changes facing to east
					} else if (Mathf.Abs (direction.eulerAngles.z - 180) < 0.1f) {
							compassFacing = Game.Facing.South;	//changes facing to south
					} else if (Mathf.Abs (direction.eulerAngles.z - 90) < 0.1f) {
							compassFacing = Game.Facing.West;	//changes facing to west
					} else
							Debug.Log ("Invalid unit facing: ActionManager, move method");
					//error catching and message

					if (mover.position.x < 0f) {
							if (game.gameMap.otherAreas.Length > -1 - (int)mover.position.x) {
								moving = game.gameMap.otherAreas [-1 - (int)mover.position.x].adjacentPosition;
								compassFacing = game.gameMap.otherAreas [-1 - (int)mover.position.x].relativePosition;
							}
					}
					game.gameMap.shiftUnit (mover.position, moving, compassFacing);
					update (Game.ActionType.Move, mover); //update method for move;
					//moves the unit
				} else {
						movementStopped = false;
						break;
				}
				//if the unit was killed in the middle of a movement causes movements to stop
			}
		}
		postAction (); //post action method
	}

	private void attackMethod(Unit attacker, Unit defender)//Created by Nick Lee 18-9-14, modified 5-11-14
	{ 
		executor = attacker;
		executie = defender;

		Game.Facing defFacing; //defenders facing
		List<int> defDie = new List<int> (); //defenders dice rolls
		List<int> attDie = new List<int> (); //attackers dice rolls

		for (int f = 0; f < UnitData.getMeleeDice(attacker.unitType); f++) {
			if(attacker.name == "Nrick")
			{
				int Roll = diceRoll ();
				if(Roll < 4)
					defDie.Add (5);
				else
					defDie.Add (Roll);
			}
			else
				attDie.Add (diceRoll ());
		}
		dieRolled.Add (game.playerTurn, attDie.ToArray());
		attDie.Sort (); //sorts the attackers dice

		for (int n = 0; n < UnitData.getMeleeDice(defender.unitType); n++) {
			if(defender.name == "Nrick")
			{
				int Roll = diceRoll ();
				if(Roll < 4)
					defDie.Add (5);
				else
					defDie.Add (Roll);
			}
			else
				defDie.Add (diceRoll ());
		}
		if(game.playerTurn == Game.PlayerType.GS)
			dieRolled.Add (Game.PlayerType.SM, defDie.ToArray());
		else if(game.playerTurn == Game.PlayerType.SM)
			dieRolled.Add (Game.PlayerType.GS, defDie.ToArray());
		else
			Debug.Log ("error in determining player action melee, actionManager attackMethod");
		//adds the unit type in term of player type and adds to dierolled dictionary
		defDie.Sort (); //sorts the defenders dice
		
		Quaternion defDirection = game.facingDirection[defender.facing];
		Quaternion attDirection = game.facingDirection[attacker.facing];
		//gets the facing of the units
		if (Mathf.Abs (Mathf.Abs (attDirection.eulerAngles.z - defDirection.eulerAngles.z) - 180) < 0.1f) {
			//if units are facing each other
			if(attDie[attDie.Count - 1] > defDie[defDie.Count - 1])
			{
				kill (defender);
				//if attacker wins kill defender
			}
			if(defDie[defDie.Count - 1] > attDie[attDie.Count - 1])
			{
				kill (attacker);
				//if defender wins kill attacker
			}
			update (Game.ActionType.Attack, attacker); //runs update for attack method
			postAction (); //runs postaction
		} else { //if not facing each other
			if(attDie[attDie.Count - 1] > defDie[defDie.Count - 1])
			{
				kill (defender);
				//if attacker wins kill defender
				update (Game.ActionType.Attack, attacker); //runs update for attack method
				postAction (); //runs postaction
			}
			if(defDie[defDie.Count - 1] >= attDie[attDie.Count - 1])
			{ //if defender draws or wins
				switch(attacker.facing)
				{
					case Game.Facing.North:
					{
						defFacing = Game.Facing.South;
						break;
						//if attacker is facing north change defenders facing to south
					}
					case Game.Facing.East:
					{
						defFacing = Game.Facing.West;
						break;
						//if attacker is facing east change defenders facing to west
					}
					case Game.Facing.South:
					{
						defFacing = Game.Facing.North;
						break;
						//if attacker is facing south change defenders facing to north
					}
					case Game.Facing.West:
					{
						defFacing = Game.Facing.East;
						break;
						//if attacker is facing west change defenders facing to east
					}
					default:
					{
						defFacing = defender.facing;
						break;
						//if issue dont change anything
					}
				}
				update (Game.ActionType.Attack, attacker); //runs update for attack method
				customPath = game.algorithm.getPath (defender.position, defender.facing, defender.position, defFacing, UnitData.getMoveSet(defender.unitType));
				//creates path involving the units movement
				attackMove = true; //sets attack move to true
				postAction (); //runs postaction
				moveMethod (defender);//makes a move
			}
		}
	}

	private void shootMethod(Unit shooter, Unit shootie)//Created by Nick Lee 18-9-14, modified 21-10-14
	{
		executor = shooter;
		executie = shootie;

		List<int> Dice = new List<int> ();
		for (int n = 0; n < UnitData.getRangedDiceCount(shooter.unitType); n++) {
				if (shooter.name == "Nrick" && overwatchShot) {
						int jamDice = diceRoll ();
						Dice.Add (jamDice);
						Dice.Add (jamDice);
						n = 10;
				} else {
						Dice.Add (diceRoll ());
				}
		}
		dieRolled.Add (Game.PlayerType.SM, Dice.ToArray ());
		//rolls 2 die

		if (shootie != null) {
			if (!shooter.isJammed) {
				//makes sure they are not jammed
				if (shooter.sustainedFireTarget == shootie) {
					//checks for sustained fire
					if (Dice [0] >= 5 || Dice [1] >= 5) {
						//if kill criteria are met
						kill (shootie);
						voidSustainedFire (shooter);
						if (overwatchShot) {
								movementStopped = true;
						}
						//removes unit being shot and changes required variable
					}
					//sustained fire shots (kill on 5's)
				} else {
					//if not sustained fire
					if (Dice [0] >= 6 || Dice [1] >= 6) {
						kill (shootie);
						voidSustainedFire (shooter);
						if (overwatchShot) {
								movementStopped = true;
						}
						//if criteria met kills unit
					} else {
						shooter.sustainedFireTarget = shootie;
						shooter.hasSustainedFire = true;
						if (!sustainedFireChanged.ContainsKey (shooter))
								sustainedFireChanged.Add (shooter, shootie);
						else
								sustainedFireChanged [shooter] = shootie;
						//if not killed changes sustained fire
					}
					//non-sustained fire shots (kill on 6's)
				}
				if (overwatchShot && Dice [0] == Dice [1]) {
					shooter.isJammed = true;
					unitJams = true;
					voidOverwatch (shooter);
					voidSustainedFire (shooter);
					//changes sustained fire and jam variables if required during overwatch
				}
				if (!overwatchShot)
						voidOverwatch (shooter);
			}
			update (Game.ActionType.Shoot, shooter);
			postAction ();
		} else {
			update (Game.ActionType.Shoot, shooter);
			postAction ();
		}
	}

	private void toggleDoorMethod(Unit exe)//Created by Nick Lee 18-9-14, modified 26-9-14
	{
		Movement = Game.MoveType.Forward;
		moving = (Vector2)game.moveTransform[Movement][0]; //gets the object from the dictionary and converts to a vector2
		moving = game.facingDirection[exe.facing] * moving;
		moving = exe.position + moving; //gets final position

		if (game.gameMap.hasDoor (moving)) {
			if (game.gameMap.isDoorOpen (moving))
				game.gameMap.setDoorState (moving, false); //sets door state to closed
			else
				game.gameMap.setDoorState (moving, true); //sets door stat to open
		}
		else
			Debug.Log ("no door in front of unit, actionManager, toggledoor");
		//error catching and message
		update(Game.ActionType.ToggleDoor, exe);
		postAction ();
	}

	private void overwatchMethod(Unit exe)//Created by Nick Lee 18-9-14, modified 25-9-14
	{
		exe.isOnOverwatch = true; //sets overwatch to true
		update (Game.ActionType.Overwatch, exe);
		postAction ();
	}

	private int diceRoll()//Created by Nick Lee 16-9-14, modified 7-10-14
	{
		int die = Random.Range (1, 7);
		//creates the die int
		return die; //returns the die value
	}

	private void removeAP (Unit userUnit, int APUsed) //Created by Nick Lee 23-9-14, modified 6-10-14
	{
		for(int y = 0; y < APUsed; y++)
			if (userUnit.AP <= 0 && userUnit.unitType == Game.EntityType.SM) {
				game.remainingCP--;
				//removes CP used
			} else{
				userUnit.AP--;
				//removes required amount of AP from units current AP count
			}
	}

	private void makeActions(Game.ActionType actionMade, Unit exe) //Created by Nick Lee 23-9-14, modified 5-11-14
	{
		executor = exe;
		actionType = actionMade; //gets action made
		finishLoS ();
		//gets the updated LoS for all marines
		APCost = UnitData.getAPCost(actionType); //gets the AP cost of the action
		if (overwatchShot)
			APCost = 0;

		if (actionType == Game.ActionType.Move) {
			executie = null; //no target unit for moving
			movePosition = moving; //position to move to set by moving
			moveFacing = compassFacing; //facing set by compass facing
			if(!sittingStill)
				APCost = UnitData.getMoveSet(executor.unitType)[Movement]; //APCost depends on type of movement
			else
			{
				APCost = 0;
				sittingStill = false;
			}
			unitJams = false; //cant jam
			voidSustainedFire(executor);
			voidOverwatch(executor);
		}
		else if (actionType == Game.ActionType.Attack) {
			voidOverwatch(executor);
			voidOverwatch(executie);
			movePosition = executor.position; //no position change
			moveFacing = executor.facing; //no facing change
			voidSustainedFire(executor);
			voidSustainedFire(executie);
			unitJams = false;
		}
		else if (actionType == Game.ActionType.Shoot) {
			movePosition = executor.position; //position unchanged
			moveFacing = executor.facing; //facing unchanged
		}
		else if (actionType == Game.ActionType.ToggleDoor) {
			voidSustainedFire(executor);
			voidOverwatch(executor);
			unitJams = false;
		}
		else if (actionType == Game.ActionType.Overwatch) {
			executie = null; //no target unit
			movePosition = executor.position; //no change movement
			moveFacing = executor.facing; //no change in facing
			unitJams = false; //no jamming
		} else
			Debug.Log ("Error with action type , ActionManager, makeActions");
		//error message and catching

		returnAction.actionType = actionType;
		returnAction.executor = executor;
		returnAction.target = executie;
		returnAction.movePosition = movePosition;
		returnAction.moveFacing = moveFacing;
		returnAction.APCost = APCost;
		returnAction.unitJams = unitJams;
		returnAction.destroyedUnits = destroyedUnits;
		returnAction.sustainedFireLost = sustainedFireLost;
		returnAction.completeLoS = completeLoS;
		returnAction.prevLoS = prevLoS;
		returnAction.sustainedFireChanged = sustainedFireChanged;
		returnAction.lostOverwatch = lostOverwatch;
		returnAction.diceRoll = dieRolled;
		actions.Add (returnAction);
		//creates a return Action and adds it to the list of actions
		
		resetVariables ();
	}

	private void InvoluntaryReveal (Unit blipRevealed) //created by Nick Lee 15-10-14, modified by 20-10-14
	{
		finishLoS ();
		
		returnAction.actionType = Game.ActionType.InvoluntaryReveal; //involuntary reveal
		returnAction.executor = blipRevealed; //blip thats being revealed
		returnAction.target = null; //no target
		returnAction.movePosition = blipRevealed.position; //blips position
		returnAction.moveFacing = blipRevealed.facing; //blips facing
		returnAction.APCost = 0; //no cost for involuntary reveal
		returnAction.unitJams = false; //cant jam
		returnAction.completeLoS = completeLoS; //what the marines can see see see
		returnAction.prevLoS = prevLoS; //what the marines could see see see
		destroyedUnits.Clear ();
		returnAction.destroyedUnits = destroyedUnits; //no units die... yet
		sustainedFireLost.Clear ();
		returnAction.sustainedFireLost = sustainedFireLost; //blips cant shoot silly
		sustainedFireChanged.Clear ();
		returnAction.sustainedFireChanged = sustainedFireChanged; //no change
		lostOverwatch.Clear ();
		returnAction.lostOverwatch = lostOverwatch; //again no change
		dieRolled.Clear ();
		returnAction.diceRoll = dieRolled; //die rolls for reveal, thats stupid
		actions.Add (returnAction);
		
		resetVariables ();
	}

	private void updateLoS () //created by Nick Lee 15-10-14
	{
		marines = game.gameMap.getUnits (Game.EntityType.SM);
		for (int i = 0; i < marines.Count; i++) {
			marines[i].currentLoS = game.algorithm.findLoS(marines[i]);
		}//updates line of sight for all marines
	}

	private void makePrevLoS () //created by Nick Lee 15-10-14
	{
		prevLoS = new Dictionary<Unit, List<Vector2>> ();

		marines = game.gameMap.getUnits (Game.EntityType.SM);
		for (int j = 0; j < marines.Count; j++)
			prevLoS.Add (marines[j], game.algorithm.findLoS(marines[j]));
		//resets prevLoS and sets it again
	}

	public void postInvolReveal(Unit centralGene) //created by Nick Lee 15-10-14, modified by 5-11-14
	{
		marines = game.gameMap.getUnits (Game.EntityType.SM);
		//makes a list of all marine units
		if(involUnit != null){
			if (involUnit.unitType == Game.EntityType.Blip) { //if action is made by a genestealer
				for (int i = 0; i < marines.Count; i++) { //then for each marine
					for (int q = 0; q < marines[i].currentLoS.Count; q++) { //then for each square
						if(marines[i].currentLoS[q] == centralGene.position && marines[i].isOnOverwatch && !marinesShot.Contains(marines[i]))
						{
							overwatchShot = true; //set overwatch shot to true
							shot = true; //set shot equal to true
							shootMethod (marines[i], centralGene); //And run a shoot action against the genestealer
							marinesShot.Add (marines[i]);
							shot = false;
							overwatchShot = false; //set overwatch shot to false
						}
					}
				}
			}
		}
	}

	private void voidSustainedFire(Unit voided) //created by Nick Lee 21-10-14
	{
		if (voided.hasSustainedFire) {
			voided.sustainedFireTarget = null;
			voided.hasSustainedFire = false;
			sustainedFireLost.Add (voided);
			if(!sustainedFireChanged.ContainsKey(voided))
				sustainedFireChanged.Add (voided, null);
			else
				sustainedFireChanged[voided] = null;
			//makes the units sustained fire null and makes sure to record change for action list
		}
	}

	private void voidOverwatch(Unit voided) //created by Nick Lee 21-10-14
	{
		if (voided.isOnOverwatch) {
			voided.isOnOverwatch = false;
			lostOverwatch.Add (voided);
			//sets units overwatch to false and gets ready to give to action list
		}
	}

	private void resetVariables() //created by Nick Lee 21-10-14
	{
		unitJams = false;
		movePosition = new Vector2(); 
		destroyedUnits = new List<Unit> ();
		sustainedFireLost = new List<Unit> ();
		completeLoS = new Dictionary<Unit, List<Vector2>> ();
		sustainedFireChanged = new Dictionary<Unit, Unit> ();
		lostOverwatch = new List<Unit> ();
		dieRolled = new Dictionary<Game.PlayerType, int[]> ();
		returnAction = new Action ();
		//resets variables
	}

	private void finishLoS() //created by Nick Lee 21-10-14
	{
		marines = game.gameMap.getUnits(Game.EntityType.SM);
		for(int u = 0; u < marines.Count; u++)
		{
			completeLoS.Add (marines[u], game.algorithm.findLoS(marines[u]));
			//adds all marines line of sight to actions
		}
	}

	private void kill(Unit killed) //created by Nick Lee 21-10-14
	{
		if (killed != null) {
			game.gameMap.removeUnit (killed.position);
			destroyedUnits.Add (killed);
			//removes the unit

			marines = game.gameMap.getUnits (Game.EntityType.SM);
			for (int i = 0; i < marines.Count; i++) { //then for each marine
				if (marines [i].sustainedFireTarget == killed)
						voidSustainedFire (marines [i]);
				//removes sustained fire from the target for any marines that had sustained fire on it
			}
		}
	}
}