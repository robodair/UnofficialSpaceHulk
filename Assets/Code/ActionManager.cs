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
	private int j = 0;


	public ActionManager(Unit unit, Game.ActionType action)	//Contents modified by Nick Lee 16-9-14
	{
		GameObject gameController = GameObject.FindWithTag ("GameController");
		game = gameController.GetComponent<Game> ();
		//gets local game controller
		actionUsed = action; //gets the action
		this.unit = unit; //the unit using the action
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

	private void update(Game.ActionType actionUpdate)//Created by Nick Lee 16-9-14, modified 18-9-14
	{
		if (unit.unitType == Game.EntityType.GS && !shot) {
			marines = game.gameMap.getMarines ();
			for (int i = 0; i < marines.Count; i++) {
				if(marines[i].currentLoS.Contains(unit.position) && marines[i].isOnOverwatch)
				{
					overwatchShot = true;
					shootMethod (marines[i], unit);
					shot = true;
				}
			}
		}
		makeActions (actionUpdate);
	}

	private void postAction()//Created by Nick Lee 18-9-14
	{
		marines = game.gameMap.getMarines ();
		for (int i = 0; i < marines.Count; i++) {
			marines[i].currentLoS = game.algorithm.findLoS(marines[i]);
		}//updates line of sight for all marines
		//add involuntary reveal later
	}

	private void moveMethod()//Created by Nick Lee 23-9-14, modified 18-9-14
	{
		for (int i = 0; i < path.path.Count; i++) {
			if (!attackMove) {
				Movement = path.path [i];
			}
			else
				Movement = customPath.path[i];

			removeAP (unit, UnitData.getMoveSet(unit.unitType)[Movement]);
			moving = (Vector2)game.moveTransform[Movement][0];
			moving = game.facingDirection[unit.facing] * moving;
			moving = unit.position + moving;

			Quaternion direction = game.facingDirection[unit.facing]*((Quaternion)game.moveTransform[Movement][1]);
			if(Mathf.Abs (direction.eulerAngles.z-0) < 0.1f)
			{
				compassFacing = Game.Facing.North;
			}
			else if(Mathf.Abs (direction.eulerAngles.z-270) < 0.1f)
			{
				compassFacing = Game.Facing.East;
			}
			else if(Mathf.Abs (direction.eulerAngles.z-180) < 0.1f)
			{
				compassFacing = Game.Facing.South;
			}
			else if(Mathf.Abs (direction.eulerAngles.z-90) < 0.1f)
			{
				compassFacing = Game.Facing.West;
			}
			else
				Debug.Log ("Invalid unit facing: ActionManager, move method");

			game.gameMap.shiftUnit(unit.position, moving, compassFacing);
			postAction ();
			update (Game.ActionType.Move);
		}
	}

	private void attackMethod(Unit attacker, Unit defender)//Created by Nick Lee 18-9-14
	{ 
		Game.Facing defFacing;
		int Die1;
		int Die2;
		int Die3;
		int Die4;
		if (attacker.unitType == Game.EntityType.GS) {
			Die1 = diceRoll (attacker, Game.ActionType.Attack);
			Die2 = diceRoll (attacker, Game.ActionType.Attack);
			Die3 = diceRoll (attacker, Game.ActionType.Attack);
			Die4 = diceRoll (defender, Game.ActionType.Attack);
		} else {
			Die1 = diceRoll (defender, Game.ActionType.Attack);
			Die2 = diceRoll (defender, Game.ActionType.Attack);
			Die3 = diceRoll (defender, Game.ActionType.Attack);
			Die4 = diceRoll (attacker, Game.ActionType.Attack);
		}
		if (Die2 > Die1)
			Die1 = Die2;
		if (Die3 > Die1)
			Die1 = Die3;
		//creates the dice rolls and then gets highest roll for the genestealer

		defender.isOnOverwatch = false;
		Quaternion defDirection = game.facingDirection[defender.facing];
		Quaternion attDirection = game.facingDirection[attacker.facing];
		if (Mathf.Abs (Mathf.Abs (attDirection.eulerAngles.z - defDirection.eulerAngles.z) - 180) < 0.1f) {
			if(Die1 > Die4)
				game.gameMap.removeUnit (defender.position);
				destroyedUnits.Add (defender);
			if(Die4 > Die1)
				game.gameMap.removeUnit (attacker.position);
					destroyedUnits.Add (attacker);
		} else {
			if(Die1 > Die4)
				game.gameMap.removeUnit (defender.position);
				destroyedUnits.Add (defender);
			if(Die4 >= Die1)
			{
				switch(attacker.facing)
				{
					case Game.Facing.North:
					{
						defFacing = Game.Facing.South;
						break;
					}
					case Game.Facing.East:
					{
						defFacing = Game.Facing.West;
						break;
					}
					case Game.Facing.South:
					{
						defFacing = Game.Facing.North;
						break;
					}
					case Game.Facing.West:
					{
						defFacing = Game.Facing.East;
						break;
					}
					default:
					{
						defFacing = defender.facing;
						break;
					}
				}
				customPath = game.algorithm.getPath (defender.position, defender.facing, defender.position, defFacing, UnitData.getMoveSet(defender.unitType));
				attackMove = true;
				moveMethod ();
				game.gameMap.shiftUnit(defender.position, defender.position, compassFacing);
			}
		}
		postAction ();
		update (Game.ActionType.Attack);
	}

	private void shootMethod(Unit shooter, Unit shootie)//Created by Nick Lee 18-9-14
	{
		int die1 = diceRoll(shooter, Game.ActionType.Shoot);
		int die2 = diceRoll(shooter, Game.ActionType.Shoot);
		if (!shooter.isJammed) {
				if (shooter.hasSustainedFire && shooter.sustainedFireTarget == shootie) {
				sustainedFireChanged.Add (shooter, shootie);
						if (die1 >= 5 || die1 >= 5) {
								game.gameMap.removeUnit (shootie.position);
								destroyedUnits.Add (shootie);
								sustainedFireChanged.Remove (shooter);
						}
						//sustained fire shots (kill on 5's)
				} else {
						if (die1 >= 6 || die1 >= 6) {
								game.gameMap.removeUnit (shootie.position);
								destroyedUnits.Add (shootie);
								sustainedFireChanged.Remove (shooter);
						}
						//non-sustained fire shots (kill on 6's)
				}
				if (overwatchShot && die1 == die2) {
						shooter.isJammed = true;
						shooter.isOnOverwatch = false;
						shooter.hasSustainedFire = false;
						shooter.sustainedFireTarget = null;
						sustainedFireChanged.Remove (shooter);
						unitJams = true;
				}
		}
		postAction ();
		update (Game.ActionType.Shoot);
		shot = false;
	}

	private void revealMethod()//Created by Nick Lee 18-9-14
	{
		postAction ();
		update (Game.ActionType.Reveal);
	}

	private void toggleDoorMethod()//Created by Nick Lee 18-9-14
	{
		postAction ();
		update (Game.ActionType.ToggleDoor);
	}

	private void overwatchMethod()//Created by Nick Lee 18-9-14
	{
		unit.isOnOverwatch = true;
		lostOverwatch.Remove (unit);
		postAction ();
		update (Game.ActionType.Overwatch);
	}

	private int diceRoll(Unit roller, Game.ActionType shootOrAttack)//Created by Nick Lee 16-9-14
	{
		int die = Random.Range (1, 6);
		return die;
		if(roller.unitType.Equals(game.playerTurn) && shootOrAttack == Game.ActionType.Attack)
		{
			dieRolls[j] = die;
			j++;
			if(game.playerTurn == Game.PlayerType.GS && j == 3)
				dieRolled.Add (game.playerTurn, dieRolls);
			else if(game.playerTurn == Game.PlayerType.SM && j == 1)
				dieRolled.Add (game.playerTurn, dieRolls);
		} else if (shootOrAttack == Game.ActionType.Shoot){
			dieRolls[j] = die;
			j++;
			if(j == 2)
				dieRolled.Add (game.playerTurn, dieRolls);
		}
	}

	private void removeAP (Unit userUnit, int APUsed) //Created by Nick Lee 23-9-14
	{
		if (userUnit.AP <= 0) {
			//Remember to add command points
		} else {
			userUnit.AP = userUnit.AP - APUsed;
		}
	}

	private void makeActions(Game.ActionType actionMade) //Created by Nick Lee 23-9-14
	{
		actionType = actionMade;
		executor = unit;
		APCost = UnitData.getAPCost(actionMade);
		marines = game.gameMap.getMarines();
		for(int u = 0; u < marines.Count; u++)
			completeLoS.Add (marines[u], game.algorithm.findLoS(marines[u]));
		APCost = UnitData.getAPCost(actionType);

		if (actionUsed == Game.ActionType.Move) {
			executie = null;
			movePosition = moving; 
			moveFacing = compassFacing;
			APCost = UnitData.getMoveSet(unit.unitType)[Movement];
			unitJams = false;
			destroyedUnits = null;
			if(unit.sustainedFireTarget != null)
				sustainedFireLost.Add (unit.sustainedFireTarget);
			else
				sustainedFireLost = null;
			sustainedFireChanged.Add (unit, unit.sustainedFireTarget);
			dieRolled = null;
		}
		else if (actionUsed == Game.ActionType.Attack) {
			executie = target;
			movePosition = unit.position; 
			moveFacing = unit.facing;
			if(unit.hasSustainedFire == true)
			{
				unit.hasSustainedFire = false;
				unit.sustainedFireTarget = null;
				sustainedFireLost.Add (unit);
			}
			if(target.hasSustainedFire == true)
			{
				unit.hasSustainedFire = false;
				unit.sustainedFireTarget = null;
				sustainedFireLost.Add (target);
			}
			sustainedFireChanged = null;

		}
		else if (actionUsed == Game.ActionType.Shoot) {
			executie = target;
			movePosition = unit.position; 
			moveFacing = unit.facing;
			if(unit.sustainedFireTarget != target)
				sustainedFireLost.Add (unit.sustainedFireTarget);
			else
				sustainedFireLost = null;
		}
		else if (actionUsed == Game.ActionType.Reveal) {

		}
		else if (actionUsed == Game.ActionType.ToggleDoor) {

		}
		else if (actionUsed == Game.ActionType.Overwatch) {

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

		actionType = Game.ActionType.Move;
		executor = null;
		executie = null;
		movePosition = unit.position;
		moveFacing = unit.facing;
		APCost = 0;
		unitJams = false;
		destroyedUnits = null;
		sustainedFireLost = null;
		completeLoS = null;
		sustainedFireChanged = null;
		lostOverwatch = null;
		dieRolled = null;
		j = 0;
	}
}