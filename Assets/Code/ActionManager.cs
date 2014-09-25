using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionManager {

	//Created by Ian Mallett 1.9.14
	//modified By Nick Lee 23.9.14

	private Game.MoveType Movement;//Created by Nick Lee 18-9-14
	private Unit unit; //Created by Nick Lee 16-9-14
	private Game.ActionType actionUsed; //Created by Nick Lee 16-9-14
	private Game game;//Created by Nick Lee 16-9-14
	private Vector2 moving;//Created by Nick Lee 16-9-14
	private Game.Facing compassFacing;//Created by Nick Lee 16-9-14
	private List<Unit> marines;//Created by Nick Lee 18-9-14
	private List<Vector2> marinesLoS;//Created by Nick Lee 18-9-14
	private bool shot = false;//Created by Nick Lee 18-9-14
	private bool overwatchShot = false;//Created by Nick Lee 18-9-14
	private bool attackMove = false;//Created by Nick Lee 18-9-14
	private Path customPath;//Created by Nick Lee 18-9-14
	private List<Action> actions; //created by Nick Lee 22-9-14
	private Action returnAction; //created by Nick Lee 23-9-14
	private int[] dieRolls = new int[3];
	public Unit target;
	public Path path;
	//gameController.ioModule.showActionSequence(actions);

	//created by Nick Lee 24-Sep-14
	private Game.ActionType actionType;
	private Unit executor;
	private Unit executie;
	private Vector2 movePosition; 
	private Game.Facing moveFacing;
	private int APCost;
	private bool unitJams;
	private List<Unit> destroyedUnits;
	private List<Unit> sustainedFireLost;
	private Dictionary<Unit, List<Vector2>> completeLoS;
	private Dictionary<Unit, Unit> sustainedFireChanged;
	private List<Unit> lostOverwatch;
	private Dictionary<Game.PlayerType, int[]> dieRolled;
	private int dieRollsIterator = 0;


	public ActionManager(Unit unit, Game.ActionType action)	//Contents modified by Nick Lee 16-9-14
	{
		GameObject gameController = GameObject.FindWithTag ("GameController");
		game = gameController.GetComponent<Game> ();
		//gets local game controller
		actionUsed = action; //gets the action
		this.unit = unit; //the unit using the action
		completeLoS = new Dictionary<Unit, List<Vector2>> ();
		sustainedFireChanged = new Dictionary<Unit, Unit> ();
		dieRolled = new Dictionary<Game.PlayerType, int[]> ();
		lostOverwatch = new List<Unit> ();
		sustainedFireLost = new List<Unit> ();
		destroyedUnits = new List<Unit> ();
		actions = new List<Action> ();
		marinesLoS = new List<Vector2> ();
		marines = new List<Unit> ();
	}

	public void performAction() //Contents modified by Nick Lee 18-9-14
	{
		if (unit.isOnOverwatch) {
			unit.isOnOverwatch = false; //sets overwatch to false
			lostOverwatch.Add (unit);
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
		marines = game.gameMap.getMarines ();
		for (int i = 0; i < marines.Count; i++) {
			marines[i].currentLoS = game.algorithm.findLoS(marines[i]);
		}//updates line of sight for all marines
		if (unit.unitType == Game.EntityType.GS && !shot) { //if action is made by a genestealer
			for (int i = 0; i < marines.Count; i++) { //then for each marine
				if(marines[i].currentLoS.Contains(unit.position) && marines[i].isOnOverwatch)
				{
					overwatchShot = true; //set overwatch shot to true
					shot = true; //set shot equal to true
					shootMethod (marines[i], unit); //And run a shoot action against the genestealer
				}
			}
		}
		makeActions (actionUpdate);//make an action array
	}

	private void postAction()//Created by Nick Lee 18-9-14, modified 25-9-14
	{

	}

	private void moveMethod()//Created by Nick Lee 16-9-14, modified 25-9-14
	{
		Path currentPath;
		if (!attackMove) {
			currentPath = path;
		} else {
			currentPath = customPath;
			attackMove = false;
		}
		//sets the path to iterate through

		for (int i = 0; i < currentPath.path.Count; i++) { //iterates through all movements in the path
			Movement = currentPath.path[i];
			//determines whether movement is rotation caused by an attack

			removeAP (unit, UnitData.getMoveSet(unit.unitType)[Movement]);//removes required AP from unit
			moving = (Vector2)game.moveTransform[Movement][0]; //gets the object from the dictionary and converts to a vector2
			moving = game.facingDirection[unit.facing] * moving;
			moving = unit.position + moving; //gets final position

			Quaternion direction = game.facingDirection[unit.facing]*((Quaternion)game.moveTransform[Movement][1]);
			//gets the quaternion from the current facing and the required movement
			if(Mathf.Abs (direction.eulerAngles.z-0) < 0.1f)
			{
				compassFacing = Game.Facing.North;	//changes facing to north
			}
			else if(Mathf.Abs (direction.eulerAngles.z-270) < 0.1f)
			{
				compassFacing = Game.Facing.East;	//changes facing to east
			}
			else if(Mathf.Abs (direction.eulerAngles.z-180) < 0.1f)
			{
				compassFacing = Game.Facing.South;	//changes facing to south
			}
			else if(Mathf.Abs (direction.eulerAngles.z-90) < 0.1f)
			{
				compassFacing = Game.Facing.West;	//changes facing to west
			}
			else
				Debug.Log ("Invalid unit facing: ActionManager, move method");
			//error catching and message

			game.gameMap.shiftUnit(unit.position, moving, compassFacing);
			update (Game.ActionType.Move);
		}
		postAction ();
	}

	private void attackMethod(Unit attacker, Unit defender)//Created by Nick Lee 18-9-14, modified 25-9-14
	{ 
		Game.Facing defFacing;
		int Die1;
		int Die2;
		int Die3;
		int Die4;
		//creates die
		if (attacker.unitType == Game.EntityType.GS) {
			Die1 = diceRoll (attacker, Game.ActionType.Attack);
			Die2 = diceRoll (attacker, Game.ActionType.Attack);
			Die3 = diceRoll (attacker, Game.ActionType.Attack);
			Die4 = diceRoll (defender, Game.ActionType.Attack);
			//determines which die are attackers and which are defenders
		} else {
			Die1 = diceRoll (defender, Game.ActionType.Attack);
			Die2 = diceRoll (defender, Game.ActionType.Attack);
			Die3 = diceRoll (defender, Game.ActionType.Attack);
			Die4 = diceRoll (attacker, Game.ActionType.Attack);
			//determines which die are attackers and which are defenders
		}
		if (Die2 > Die1)
			Die1 = Die2;
		if (Die3 > Die1)
			Die1 = Die3;
		//creates the dice rolls and then gets highest roll for the genestealer

		defender.isOnOverwatch = false; //sets defenders overwatch to false
		Quaternion defDirection = game.facingDirection[defender.facing];
		Quaternion attDirection = game.facingDirection[attacker.facing];
		//gets the facing of the units
		if (Mathf.Abs (Mathf.Abs (attDirection.eulerAngles.z - defDirection.eulerAngles.z) - 180) < 0.1f) {
			//if units are facing each other
			if(Die1 > Die4)
			{
				game.gameMap.removeUnit (defender.position);
				destroyedUnits.Add (defender);
				//if attacker wins kill defender
			}
			if(Die4 > Die1)
			{
				game.gameMap.removeUnit (attacker.position);
				destroyedUnits.Add (attacker);
				//if defender wins kill attacker
			}
		} else { //if not facing each other
			if(Die1 > Die4)
			{
				game.gameMap.removeUnit (defender.position);
				destroyedUnits.Add (defender);
				//if attacker wins kill defender
			}
			if(Die4 >= Die1)
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
				update (Game.ActionType.Attack);
			}
		}
		postAction ();
	}

	private void shootMethod(Unit shooter, Unit shootie)//Created by Nick Lee 18-9-14, modified 25-9-14
	{
		int die1 = diceRoll(shooter, Game.ActionType.Shoot);
		int die2 = diceRoll(shooter, Game.ActionType.Shoot);
		//rolls 2 die
		if (!shooter.isJammed) {
			//makes sure they are not jammed
				if (shooter.hasSustainedFire && shooter.sustainedFireTarget == shootie) {
				//checks for sustained fire
						if (die1 >= 5 || die1 >= 5) {
							//if kill criteria are met
							game.gameMap.removeUnit (shootie.position);
							destroyedUnits.Add (shootie);
							sustainedFireChanged.Add (shooter, null);
							//removes unit being shot and changes required variable
						}
						//sustained fire shots (kill on 5's)
				} else {
				//if not sustained fire
					sustainedFireChanged.Add (shooter, shootie);
					if (die1 >= 6 || die1 >= 6) {
						game.gameMap.removeUnit (shootie.position);
						destroyedUnits.Add (shootie);
						//if criteria met kills unit
					} else {
						sustainedFireChanged.Add (shooter, shootie);
						shooter.sustainedFireTarget = target;
						shooter.hasSustainedFire = true;
						//if not killed changes sustained fire
					}
					//non-sustained fire shots (kill on 6's)
				}
				if (overwatchShot && die1 == die2) {
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
		update (Game.ActionType.Shoot);
		shot = false; //set shot to false
		postAction ();
	}

	private void revealMethod()//Created by Nick Lee 18-9-14, modified 25-9-14
	{
		postAction ();
		update (Game.ActionType.Reveal);
	}

	private void toggleDoorMethod()//Created by Nick Lee 18-9-14, modified 25-9-14
	{
		postAction ();
		update (Game.ActionType.ToggleDoor);
	}

	private void overwatchMethod()//Created by Nick Lee 18-9-14, modified 25-9-14
	{
		unit.isOnOverwatch = true; //sets overwatch to true
		update (Game.ActionType.Overwatch);
		postAction ();
	}

	private int diceRoll(Unit roller, Game.ActionType shootOrAttack)//Created by Nick Lee 16-9-14, modified 25-9-14
	{
		int die = Random.Range (1, 6);
		//creates the die int
		if(roller.unitType.Equals(game.playerTurn) && shootOrAttack == Game.ActionType.Attack)
		{//makes sure that the die are that of an attacker
			dieRolls[dieRollsIterator] = die; //at die rolls iterator adds the roll
			dieRollsIterator++; //adds one to the iterator
			if(game.playerTurn == Game.PlayerType.GS && dieRollsIterator == 3)
				dieRolled.Add (game.playerTurn, dieRolls); //if max die rolls for genestealer then adds to list
			else if(game.playerTurn == Game.PlayerType.SM && dieRollsIterator == 1)
			{
				for(int y = 0; y < 2; y++)
				{
					dieRolls[dieRollsIterator] = 0;
					dieRollsIterator++;
					//makes the reamainder of the array contain 0
				}
				dieRolled.Add (game.playerTurn, dieRolls); //if max die rolls for marine then adds to list
			}
		} else if (shootOrAttack == Game.ActionType.Shoot){ //makes sure die are from a shoot attack
			dieRolls[dieRollsIterator] = die;
			dieRollsIterator++;
			//sets value at iterator to die
			if(dieRollsIterator == 2)
				dieRolled.Add (game.playerTurn, dieRolls);
			//when all die are rolledd adds to list
		}
		return die; //returns the die value
	}

	private void removeAP (Unit userUnit, int APUsed) //Created by Nick Lee 23-9-14, modified 25-9-14
	{
		if (userUnit.AP <= 0) {
			//Remember to add command points
		} else {
			userUnit.AP = userUnit.AP - APUsed;
			//removes required amount of AP from units current AP count
		}
	}

	private void makeActions(Game.ActionType actionMade) //Created by Nick Lee 23-9-14, modified 25-9-14
	{
		actionType = actionMade; //gets action made
		executor = unit; //the unit executing the action
		marines = game.gameMap.getMarines();
		for(int u = 0; u < marines.Count; u++)
			completeLoS.Add (marines[u], game.algorithm.findLoS(marines[u]));
		//gets the updated LoS for all marines
		APCost = UnitData.getAPCost(actionType); //gets the AP cost of the action

		if (actionType == Game.ActionType.Move) {
			executie = null; //no target unit for moving
			movePosition = moving; //position to move to set by moving
			moveFacing = compassFacing; //facing set by compass facing
			APCost = UnitData.getMoveSet(unit.unitType)[Movement]; //APCost depends on type of movement
			unitJams = false; //cant jam
			destroyedUnits = null; //nothing can be killed by movement
			if(unit.sustainedFireTarget != null)
				sustainedFireLost.Add (unit.sustainedFireTarget); //if unit has sustained fire loses it
			else
				sustainedFireLost = null; //else set lost to null
			sustainedFireChanged = null; //cant gain sustained fire
			dieRolled = null; //no dice rolling required
		}
		else if (actionType == Game.ActionType.Attack) {
			executie = target; //target of the attack
			movePosition = unit.position; //no position change
			moveFacing = unit.facing; //no facing change
			if(unit.hasSustainedFire == true)
			{
				unit.hasSustainedFire = false;
				unit.sustainedFireTarget = null;
				sustainedFireLost.Add (unit);
				//if unit had sustained fire it loses it
			}
			if(target.hasSustainedFire == true)
			{
				unit.hasSustainedFire = false;
				unit.sustainedFireTarget = null;
				sustainedFireLost.Add (target);
				//if target had sustainedFireLost sustainedFireLost then LocationServiceStatus it
			}
			sustainedFireChanged = null; //no gain in sustained fire possible
		}
		else if (actionType == Game.ActionType.Shoot) {
			executie = target; //target set
			movePosition = unit.position; //position unchanged
			moveFacing = unit.facing; //facing unchanged
			sustainedFireLost = null; //sustained fire cannot be lost
		}
		else if (actionType == Game.ActionType.Reveal) {

		}
		else if (actionType == Game.ActionType.ToggleDoor) {

		}
		else if (actionType == Game.ActionType.Overwatch) {
			executie = null; //no target unit
			movePosition = moving; //no change movement
			moveFacing = compassFacing; //no change in facing
			unitJams = false; //no jamming
			destroyedUnits = null; //nothing destroyed
			sustainedFireChanged = null;
			sustainedFireLost = null;
			dieRolled = null;
		} else
			Debug.Log ("Error with action type , ActionManager");
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
		returnAction.sustainedFireChanged = sustainedFireChanged;
		returnAction.lostOverwatch = lostOverwatch;
		returnAction.diceRoll = dieRolled;
		actions.Add (returnAction);
		//creates a return Action and adds it to the list of actions

		actionType = Game.ActionType.Move;
		executor = null;
		executie = null;
		movePosition = unit.position;
		moveFacing = unit.facing;
		APCost = 0;
		unitJams = false;
		destroyedUnits.Clear ();
		sustainedFireLost.Clear ();
		completeLoS.Clear ();
		sustainedFireChanged.Clear ();
		lostOverwatch.Clear ();
		dieRolled.Clear ();
		dieRollsIterator = 0;
		//resets values
	}
}