using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionManager {

	//Created by Ian Mallett 1.9.14
	//modified By Nick Lee 18.9.14

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
	public Unit target;
	public Path path;

	public ActionManager(Unit unit, Game.ActionType action)	//Contents modified by Nick Lee 16-9-14
	{
		GameObject gameController = GameObject.FindWithTag ("GameController");
		game = gameController.GetComponent<Game> ();
		//gets local game controller
		actionUsed = action; //gets the action
		this.unit = unit; //the unit using the action
	}

	public void performAction() 	//Contents modified by Nick Lee 18-9-14
	{
		unit.isOnOverwatch = false; //sets overwatch to false
		if (actionUsed == Game.ActionType.Move) {
			moveMethod(); //if action is a movement
		}
		else if (actionUsed == Game.ActionType.Attack) {
			attackMethod(unit, target); //if action is a melee attack, requires target
		}
		else if (actionUsed == Game.ActionType.Shoot) {
			shootMethod(unit, target); //if action is shooting, requires target
		}
		else if (actionUsed == Game.ActionType.Reveal) {
			revealMethod(); //if action is a voluntary reveal
		}
		else if (actionUsed == Game.ActionType.ToggleDoor) {
			toggleDoorMethod(); //if action is toggling a door
		}
		else if (actionUsed == Game.ActionType.Overwatch) {
			overwatchMethod(); //if action is setting a unit to overwatch
		} else
			Debug.Log ("Error with action type , ActionManager");
		//error message and catching
	}

	private void update()//Created by Nick Lee 16-9-14, modified 18-9-14
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
	}

	private void postAction()//Created by Nick Lee 18-9-14
	{
		marines = game.gameMap.getMarines ();
		for (int i = 0; i < marines.Count; i++) {
			marines[i].currentLoS = game.algorithm.findLoS(marines[i]);
		}//updates line of sight for all marines
		//add involuntary reveal later
	}

	private void moveMethod()//Created by Nick Lee 16-9-14, modified 18-9-14
	{
		for (int i = 0; i < path.path.Count; i++) {
			if (!attackMove) {
				Movement = path.path [i];
			}
			else
				Movement = customPath.path[i];
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
			update ();
		}
	}

	private void attackMethod(Unit attacker, Unit defender)//Created by Nick Lee 18-9-14
	{ 
		Game.Facing defFacing;
		int attDie = diceRoll ();
		int attDie1 = diceRoll ();
		int attDie2 = diceRoll ();
		int defDie = diceRoll ();
		if (attDie1 > attDie)
			attDie = attDie1;
		if (attDie2 > attDie)
			attDie = attDie2;
		//creates the dice rolls and then gets highest roll for the genestealer

		defender.isOnOverwatch = false;
		Quaternion defDirection = game.facingDirection[defender.facing];
		Quaternion attDirection = game.facingDirection[attacker.facing];
		if (Mathf.Abs (Mathf.Abs (attDirection.eulerAngles.z - defDirection.eulerAngles.z) - 180) < 0.1f) {
			if(attDie > defDie)
				game.gameMap.removeUnit (defender.position);
			if(defDie > attDie)
				game.gameMap.removeUnit (attacker.position);
		} else {
			if(attDie > defDie)
				game.gameMap.removeUnit (defender.position);
			if(defDie >= attDie)
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
		update ();
	}

	private void shootMethod(Unit shooter, Unit shootie)//Created by Nick Lee 18-9-14
	{
		int die1 = diceRoll();
		int die2 = diceRoll();
		if (!shooter.isJammed) {
				if (shooter.hasSustainedFire && shooter.sustainedFireTarget == shootie) {
						if (die1 >= 5 || die1 >= 5) {
								game.gameMap.removeUnit (shootie.position);
						}
						//sustained fire shots (kill on 5's)
				} else {
						if (die1 >= 6 || die1 >= 6) {
								game.gameMap.removeUnit (shootie.position);
						}
						//non-sustained fire shots (kill on 6's)
				}
				if (overwatchShot && die1 == die2) {
						shooter.isJammed = true;
				}
		}
		postAction ();
		update ();
		shot = false;
	}

	private void revealMethod()//Created by Nick Lee 18-9-14
	{
		postAction ();
		update ();
	}

	private void toggleDoorMethod()//Created by Nick Lee 18-9-14
	{
		postAction ();
		update ();
	}

	private void overwatchMethod()//Created by Nick Lee 18-9-14
	{
		unit.isOnOverwatch = true;
		postAction ();
		update ();
	}

	private int diceRoll()//Created by Nick Lee 16-9-14
	{
		int die = Random.Range (1, 6);
		return die;
	}
}