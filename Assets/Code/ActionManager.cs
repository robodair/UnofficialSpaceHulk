using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionManager {

	//Created by Ian Mallett 1.9.14
	//modified By Nick Lee 23.9.14
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
	private bool shot = false;//Created by Nick Lee 18-9-14
	private bool overwatchShot = false;//Created by Nick Lee 18-9-14
	private bool attackMove = false;//Created by Nick Lee 18-9-14
	private Path customPath;//Created by Nick Lee 18-9-14
	private List<Action> actions = new List<Action> (); //created by Nick Lee 22-9-14
	private Action returnAction = new Action (); //created by Nick Lee 23-9-14
	private bool movementStopped = false; //Created by Nick Lee 7-10-14
	private List<int> dieRolls = new List<int> (); //Created by Nick Lee 7-10-14

	//modified by Nick Lee 13-10-14
	private Game.ActionType actionType;
	private Unit executor;
	private Unit executie;
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

	public void performAction() //Contents modified by Nick Lee 18-9-14
	{
		if (unit.isOnOverwatch) {
			unit.isOnOverwatch = false; //sets overwatch to false
		}
		if (actionUsed == Game.ActionType.Move) {
			moveMethod(); //if action is a movement
		}
		else if (actionUsed == Game.ActionType.Attack) {
			removeAP (unit, UnitData.getAPCost (actionUsed));
			attackMethod(unit, target); //if action is a melee attack, requires target
		}
		else if (actionUsed == Game.ActionType.Shoot) {
			removeAP (unit, UnitData.getAPCost (actionUsed));
			shootMethod(unit, target); //if action is shooting, requires target
		}
		else if (actionUsed == Game.ActionType.Reveal) {
			revealMethod(); //if action is a voluntary reveal
		}
		else if (actionUsed == Game.ActionType.ToggleDoor) {
			removeAP (unit, UnitData.getAPCost (actionUsed));
			toggleDoorMethod(); //if action is toggling a door
		}
		else if (actionUsed == Game.ActionType.Overwatch) {
			removeAP (unit, UnitData.getAPCost (actionUsed));
			overwatchMethod(); //if action is setting a unit to overwatch
		} else
			Debug.Log ("Error with action type , ActionManager");
		//error message and catching
	}

	private void update(Game.ActionType actionUpdate)//Created by Nick Lee 16-9-14, modified 25-9-14
	{
		marines = game.gameMap.getUnits (Game.EntityType.SM);
		//makes a list of all marine units
		blips = game.gameMap.getUnits (Game.EntityType.Blip);
		//makes a list of all blip units

		updateLoS ();

		makeActions (actionUpdate);//make an action array

		if (unit.unitType == Game.EntityType.GS && !shot) { //if action is made by a genestealer
			for (int i = 0; i < marines.Count; i++) { //then for each marine
				if(marines[i].currentLoS.Contains(unit.position) && marines[i].isOnOverwatch)
				{
					overwatchShot = true; //set overwatch shot to true
					shot = true; //set shot equal to true
					shootMethod (marines[i], unit); //And run a shoot action against the genestealer
					overwatchShot = false; //set overwatch shot to false
				}
			}
		}

		for (int i = 0; i < marines.Count; i++) { //then for each marine
			for (int t = 0; t < blips.Count; t++) {//and each blip
				for (int u = 0; u < marines[i].currentLoS.Count; u++) {//and each square in sight
					if(marines[i].currentLoS[u] == blips[t].position && !blipsRevealed.Contains(blips[t]))
					{ //and the blip isnt already in the list but is within a marines LoS
						blipsRevealed.Add (blips[t]);
						movementStopped = true;
						//stops any further movement
					}
				}
			}
		}
		foreach (Unit blip in blipsRevealed) {
			InvoluntaryReveal (blip);
		}

		makePrevLoS ();
	}

	private void postAction()//Created by Nick Lee 18-9-14, modified 25-9-14
	{
		game.ioModule.showActionSequence(actions.ToArray (), this); //gives the action array to the input output module
	}

	private void moveMethod()//Created by Nick Lee 16-9-14, modified 9-10-14
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

		for (int i = 0; i < currentPath.path.Count; i++) { //iterates through all movements in the path
			if(!movementStopped) //if the unit wasn't killed by overwatch
			{
				Movement = currentPath.path [i];

				removeAP (unit, UnitData.getMoveSet (unit.unitType) [Movement]);//removes required AP from unit
				moving = (Vector2)game.moveTransform [Movement] [0]; //gets the object from the dictionary and converts to a vector2
				moving = game.facingDirection [unit.facing] * moving;
				moving = unit.position + moving; //gets final position

				Quaternion direction = game.facingDirection [unit.facing] * ((Quaternion)game.moveTransform [Movement] [1]);
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

				if(unit.position.x < 0f)
				{
					if (game.gameMap.otherAreas.Length > -1 - (int) unit.position.x)
					{
						moving = game.gameMap.otherAreas[-1 - (int)unit.position.x].adjacentPosition;
						compassFacing = game.gameMap.otherAreas[-1 - (int)unit.position.x].relativePosition;
					}
				}
				game.gameMap.shiftUnit (unit.position, moving, compassFacing);
				//moves the unit
			}
			else
				break;
				//if the unit was killed in the middle of a movement causes movements to stop
			update (Game.ActionType.Move); //update method for move
		}
		postAction (); //post action method
	}

	private void attackMethod(Unit attacker, Unit defender)//Created by Nick Lee 18-9-14, modified 25-9-14
	{ 
		Game.Facing defFacing; //defenders facing
		List<int> defDie = new List<int> (); //defenders dice rolls
		List<int> attDie = new List<int> (); //attackers dice rolls

		for (int f = 0; f < UnitData.getMeleeDice(attacker.unitType); f++) {
			attDie.Add (diceRoll ());
			dieRolled.Add (game.playerTurn, dieRolls.ToArray());
			//gets the dice rolled by the attacker and their values and adds them to an array with the unit
		}
		attDie.Sort (); //sorts the attackers dice

		for (int n = 0; n < UnitData.getMeleeDice(defender.unitType); n++) {
			defDie.Add (diceRoll ());
			//rolls a new dice and adds to the list
			if(game.playerTurn == Game.PlayerType.GS)
				dieRolled.Add (Game.PlayerType.SM, dieRolls.ToArray());
			else if(game.playerTurn == Game.PlayerType.SM)
				dieRolled.Add (Game.PlayerType.GS, dieRolls.ToArray());
			else
				Debug.Log ("error in determining player action melee, actionManager attackMethod");
			//adds the unit type in term of player type and adds to dierolled dictionary
		}
		defDie.Sort (); //sorts the defenders dice

		defender.isOnOverwatch = false; //sets defenders overwatch to false
		Quaternion defDirection = game.facingDirection[defender.facing];
		Quaternion attDirection = game.facingDirection[attacker.facing];
		//gets the facing of the units
		if (Mathf.Abs (Mathf.Abs (attDirection.eulerAngles.z - defDirection.eulerAngles.z) - 180) < 0.1f) {
			//if units are facing each other
			if(attDie[attDie.Count - 1] > defDie[defDie.Count - 1])
			{
				game.gameMap.removeUnit (defender.position);
				destroyedUnits.Add (defender);
				//if attacker wins kill defender
			}
			if(defDie[defDie.Count - 1] > attDie[attDie.Count - 1])
			{
				game.gameMap.removeUnit (attacker.position);
				destroyedUnits.Add (attacker);
				//if defender wins kill attacker
			}
		} else { //if not facing each other
			if(attDie[attDie.Count - 1] > defDie[defDie.Count - 1])
			{
				game.gameMap.removeUnit (defender.position);
				destroyedUnits.Add (defender);
				//if attacker wins kill defender
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
				customPath = game.algorithm.getPath (defender.position, defender.facing, defender.position, defFacing, UnitData.getMoveSet(defender.unitType));
				//creates path involving the units movement
				attackMove = true; //sets attack move to true
				moveMethod ();//makes a move
				update (Game.ActionType.Attack); //runs update for attack method
			}
		}
		if(attacker.unitType == Game.EntityType.GS)
		{
			Debug.Log ("attackers dice rolled: " + attDie[0] + ", " + attDie[1] + ", " + attDie[2]);
			Debug.Log ("defenders dice rolled: " + defDie[0]);
		}
		//for debugging processes gets die rolls
		if(attacker.unitType == Game.EntityType.SM)
		{
			Debug.Log ("defenders dice rolled: " + defDie[0] + ", " + defDie[1] + ", " + defDie[2]);
			Debug.Log ("attackers dice rolled: " + attDie[0]);
		}
		//for debugging processes gets die rolls
		postAction (); //runs postaction
	}

	private void shootMethod(Unit shooter, Unit shootie)//Created by Nick Lee 18-9-14, modified 13-10-14
	{
		List<int> Dice = new List<int> ();
		for (int n = 0; n < UnitData.getRangedDiceCount(shooter.unitType); n++) {
			Dice.Add (diceRoll ());
		}
		dieRolled.Add (game.playerTurn, Dice.ToArray());
		//rolls 2 die

		if (!shooter.isJammed) {
			//makes sure they are not jammed
				if (shooter.hasSustainedFire && shooter.sustainedFireTarget == shootie) {
					//checks for sustained fire
					if (Dice[0] >= 5 || Dice[1] >= 5) {
							//if kill criteria are met
							game.gameMap.removeUnit (shootie.position);
							destroyedUnits.Add (shootie);
							sustainedFireChanged.Add (shooter, null);
						if(overwatchShot){
							movementStopped = true;
						}
						//removes unit being shot and changes required variable
				}
				//sustained fire shots (kill on 5's)
				} else {
				//if not sustained fire
					sustainedFireChanged.Add (shooter, shootie);
					if (Dice[0] >= 6 || Dice[1] >= 6) {
						game.gameMap.removeUnit (shootie.position);
						destroyedUnits.Add (shootie);
						sustainedFireChanged[shooter] = null;
						if(overwatchShot){
							movementStopped = true;
						}
						//if criteria met kills unit
					} else {
						shooter.sustainedFireTarget = target;
						shooter.hasSustainedFire = true;
						//if not killed changes sustained fire
					}
					//non-sustained fire shots (kill on 6's)
				}
				if (overwatchShot && Dice[0] == Dice[1]) {
					shooter.isJammed = true;
					shooter.isOnOverwatch = false;
					shooter.hasSustainedFire = false;
					shooter.sustainedFireTarget = null;
					if(shooter.hasSustainedFire)
						sustainedFireChanged.Add (shooter, null);
					unitJams = true;
					//changes sustained fire and jam variables if required during overwatch
				}
		}
		Debug.Log ("dice one rolled: " + Dice[0]);
		Debug.Log ("dice two rolled: " + Dice[1]);
		update (Game.ActionType.Shoot);
		shot = false; //set shot to false
		postAction ();
	}

	private void revealMethod()//Created by Nick Lee 18-9-14, modified 25-9-14
	{
		update (Game.ActionType.Reveal);
		postAction ();
	}

	private void toggleDoorMethod()//Created by Nick Lee 18-9-14, modified 26-9-14
	{
		Movement = Game.MoveType.Forward;
		moving = (Vector2)game.moveTransform[Movement][0]; //gets the object from the dictionary and converts to a vector2
		moving = game.facingDirection[unit.facing] * moving;
		moving = unit.position + moving; //gets final position
		if (game.gameMap.hasDoor (moving)) {
			if (game.gameMap.isDoorOpen (moving))
				game.gameMap.setDoorState (moving, false); //sets door state to closed
			else
				game.gameMap.setDoorState (moving, true); //sets door stat to open
		}
		else
			Debug.Log ("no door in front of unit, actionManager, toggledoor");
		//error catching and message
		update(Game.ActionType.ToggleDoor);
		postAction ();
	}

	private void overwatchMethod()//Created by Nick Lee 18-9-14, modified 25-9-14
	{
		unit.isOnOverwatch = true; //sets overwatch to true
		update (Game.ActionType.Overwatch);
		postAction ();
	}

	private int diceRoll()//Created by Nick Lee 16-9-14, modified 7-10-14
	{
		int die = Random.Range (1, 7);
		//creates the die int
		dieRolls.Add (die);
		return die; //returns the die value
	}

	private void removeAP (Unit userUnit, int APUsed) //Created by Nick Lee 23-9-14, modified 6-10-14
	{
		if (userUnit.AP <= 0 && userUnit.unitType == Game.EntityType.SM) {
			game.remainingCP--;
			//removes CP used
		} else{
			userUnit.AP = userUnit.AP - APUsed;
			//removes required amount of AP from units current AP count
		}
	}

	private void makeActions(Game.ActionType actionMade) //Created by Nick Lee 23-9-14, modified 13-10-14
	{
		actionType = actionMade; //gets action made
		executor = unit; //the unit executing the action
		marines = game.gameMap.getUnits(Game.EntityType.SM);
		for(int u = 0; u < marines.Count; u++)
		{
			completeLoS.Add (marines[u], game.algorithm.findLoS(marines[u]));
		}
		//gets the updated LoS for all marines
		APCost = UnitData.getAPCost(actionType); //gets the AP cost of the action
		if (overwatchShot)
			APCost = 0;

		if (actionType == Game.ActionType.Move) {
			if(executor.isOnOverwatch)
				lostOverwatch.Add (executor);
			executie = null; //no target unit for moving
			movePosition = moving; //position to move to set by moving
			moveFacing = compassFacing; //facing set by compass facing
			APCost = UnitData.getMoveSet(executor.unitType)[Movement]; //APCost depends on type of movement
			unitJams = false; //cant jam
			destroyedUnits.Clear(); //nothing can be killed by movement
			if(executor.sustainedFireTarget != null)
				sustainedFireLost.Add (executor.sustainedFireTarget); //if unit has sustained fire loses it
			else
				sustainedFireLost.Clear(); //else set sustainedFirelost to null
			sustainedFireChanged.Clear(); //cant gain sustained fire
			dieRolled.Clear(); //no dice rolling required
		}
		else if (actionType == Game.ActionType.Attack) {
			if(executor.isOnOverwatch)
				lostOverwatch.Add (executor);
			if(target.isOnOverwatch)
				lostOverwatch.Add (target);
			executie = target; //target of the attack
			movePosition = executor.position; //no position change
			moveFacing = executor.facing; //no facing change
			if(executor.hasSustainedFire == true)
			{
				executor.hasSustainedFire = false;
				executor.sustainedFireTarget = null;
				sustainedFireLost.Add (executor);
				//if unit had sustained fire it loses it
			}
			if(target.hasSustainedFire == true)
			{
				executie.hasSustainedFire = false;
				executie.sustainedFireTarget = null;
				sustainedFireLost.Add (target);
			}
			sustainedFireChanged.Clear (); //no gain in sustained fire possible
		}
		else if (actionType == Game.ActionType.Shoot) {
			if(executor.isOnOverwatch)
				lostOverwatch.Add (executor);
			executie = target; //target set
			movePosition = executor.position; //position unchanged
			moveFacing = executor.facing; //facing unchanged
			sustainedFireLost.Clear(); //sustained fire cannot be lost
		}
		else if (actionType == Game.ActionType.Reveal) {

		}
		else if (actionType == Game.ActionType.ToggleDoor) {
			if (executor.hasSustainedFire) {
				executor.sustainedFireTarget = null;
				executor.hasSustainedFire = false;
				sustainedFireLost.Add (executor);
				sustainedFireChanged.Add (executor, null);
			}
			if (executor.isOnOverwatch) {
				executor.isOnOverwatch = false;
				lostOverwatch.Add (executor);
			}
		}
		else if (actionType == Game.ActionType.Overwatch) {
			executie = null; //no target unit
			movePosition = executor.position; //no change movement
			moveFacing = executor.facing; //no change in facing
			unitJams = false; //no jamming
			destroyedUnits.Clear(); //nothing destroyed
			if (executor.hasSustainedFire) {
				executor.sustainedFireTarget = null;
				executor.hasSustainedFire = false;
				sustainedFireLost.Add (executor);
				sustainedFireChanged.Add (executor, null);
			}
			dieRolled.Clear();
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
		
		movePosition = new Vector2(); 
		destroyedUnits = new List<Unit> ();
		sustainedFireLost = new List<Unit> ();
		completeLoS = new Dictionary<Unit, List<Vector2>> ();
		sustainedFireChanged = new Dictionary<Unit, Unit> ();
		lostOverwatch = new List<Unit> ();
		dieRolled = new Dictionary<Game.PlayerType, int[]> ();
		returnAction = new Action ();
		//resets values
	}

	private void InvoluntaryReveal (Unit blipRevealed) //created by Nick Lee 15-10-14, modified by 20-10-14
	{
		marines = game.gameMap.getUnits(Game.EntityType.SM);
		for(int u = 0; u < marines.Count; u++)
		{
			completeLoS.Add (marines[u], game.algorithm.findLoS(marines[u]));
		}

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

		movePosition = new Vector2(); 
		destroyedUnits = new List<Unit> ();
		sustainedFireLost = new List<Unit> ();
		completeLoS = new Dictionary<Unit, List<Vector2>> ();
		sustainedFireChanged = new Dictionary<Unit, Unit> ();
		lostOverwatch = new List<Unit> ();
		dieRolled = new Dictionary<Game.PlayerType, int[]> ();
		returnAction = new Action ();
		//resets variables

		postInvolReveal (game.gameMap.getOccupant (blipRevealed.position));
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
		prevLoS.Clear ();
		prevLoS = new Dictionary<Unit, List<Vector2>> ();
		marines = game.gameMap.getUnits (Game.EntityType.SM);
		for (int j = 0; j < marines.Count; j++)
			prevLoS.Add (marines[j], game.algorithm.findLoS(marines[j]));
		//resets prevLoS and sets it again
	}

	private void postInvolReveal(Unit centralGene) //created by Nick Lee 15-10-14, modified by 20-10-14
	{
		for (int i = 0; i < marines.Count; i++) { //then for each marine
			if(marines[i].currentLoS.Contains(centralGene.position) && marines[i].isOnOverwatch)
			{
				overwatchShot = true; //set overwatch shot to true
				shot = true; //set shot equal to true
				shootMethod (marines[i], centralGene); //And run a shoot action against the genestealer
				overwatchShot = false; //set overwatch shot to false
			}
		}
	}
}