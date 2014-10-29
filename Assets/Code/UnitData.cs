using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class UnitData{

	//Created by Ian Mallett 1.9.14
	//removed monobehaviour and made static 11-9-14, by Nick Lee

	public static int getMaxAP(Game.EntityType unitType)	//Written by Nick Lee, 13-9-2014
	{ //Returns the max AP based on the unit type
		if (unitType == Game.EntityType.SM) {
				return 4; //If space marine returns 4
		}
		else if (unitType == Game.EntityType.GS || unitType == Game.EntityType.Blip) {
				return 6; //If blip or genestealer returns 6
		} else {
				Debug.Log ("Invalid unit type selected, no max AP value. (getMaxAP, UnitData)");
			return 0;//Error catching and message
		}
	}//untested

	public static Game.ActionType[] getActionSet(Game.EntityType unitType)	// contents modified by Nick Lee, 18-9-2014
	{//Gets and returns the action set of each specific unit type
		if (unitType == Game.EntityType.Blip) {
			Game.ActionType[] BlipActions = {Game.ActionType.Move, Game.ActionType.Reveal, Game.ActionType.ToggleDoor};
			return BlipActions;
			//returns Blip actions if unit is a Blip
				}
		else if (unitType == Game.EntityType.GS) {
			Game.ActionType[] GSActions = {Game.ActionType.Move, Game.ActionType.ToggleDoor, Game.ActionType.Attack};
			return GSActions;
			//returns Genestealer actions if unit is a Genestealer
		}
		else if (unitType == Game.EntityType.SM){
			Game.ActionType[] SMActions = {Game.ActionType.Move, Game.ActionType.ToggleDoor, Game.ActionType.Shoot, Game.ActionType.Overwatch};
			return SMActions;
			//returns space marines actions if unit is a space marine
		} 
		else {
			Debug.Log ("Invalid actionSet, no valid unit type selected. (getActionSet, UnitData)");
			return null;
			//Error checking and message
		}
	}//untested

	public static Dictionary<Game.MoveType, int> getMoveSet(Game.EntityType unitType) // contents modified by Nick Lee, 25-10-2014, Ian Mallett 29.10.14
	{//Gets and returns the movement set of each specific unit type
		if (unitType == Game.EntityType.Blip) {
			Dictionary<Game.MoveType, int> BlipMovements = new Dictionary<Game.MoveType, int>();
			//Ian Mallett 29.10.14 Removed 0 AP cost turning
			BlipMovements.Add (Game.MoveType.Forward, 1);
			BlipMovements.Add (Game.MoveType.Left, 1);
			BlipMovements.Add (Game.MoveType.Right, 1);
			BlipMovements.Add (Game.MoveType.Back, 1);
			BlipMovements.Add (Game.MoveType.FrontRight, 1);
			BlipMovements.Add (Game.MoveType.FrontLeft, 1);
			BlipMovements.Add (Game.MoveType.BackRight, 1);
			BlipMovements.Add (Game.MoveType.BackLeft, 1);
			BlipMovements.Add (Game.MoveType.ForwardTurnRight, 1);
			BlipMovements.Add (Game.MoveType.ForwardTurnLeft, 1);
			BlipMovements.Add (Game.MoveType.FrontRightTurnRight, 1);
			BlipMovements.Add (Game.MoveType.FrontRightTurnLeft, 1);
			BlipMovements.Add (Game.MoveType.FrontLeftTurnRight, 1);
			BlipMovements.Add (Game.MoveType.FrontLeftTurnLeft, 1);
			BlipMovements.Add (Game.MoveType.RightTurnRight, 1);
			BlipMovements.Add (Game.MoveType.RightTurnLeft, 1);
			BlipMovements.Add (Game.MoveType.LeftTurnRight, 1);
			BlipMovements.Add (Game.MoveType.LeftTurnLeft, 1);
			BlipMovements.Add (Game.MoveType.BackTurnRight, 1);
			BlipMovements.Add (Game.MoveType.BackTurnLeft, 1);
			BlipMovements.Add (Game.MoveType.BackRightTurnLeft, 1);
			BlipMovements.Add (Game.MoveType.BackLeftTurnRight, 1);
			BlipMovements.Add (Game.MoveType.BackLeftTurnLeft, 1);
			BlipMovements.Add (Game.MoveType.BackRightTurnRight, 1);
			return BlipMovements;
			//returns Blip movements if unit is a blip
		}
		else if (unitType == Game.EntityType.GS) {
			Dictionary<Game.MoveType, int> GSMovements = new Dictionary<Game.MoveType, int>();
			GSMovements.Add (Game.MoveType.Forward, 1);
			GSMovements.Add (Game.MoveType.Left, 1);
			GSMovements.Add (Game.MoveType.Right, 1);
			GSMovements.Add (Game.MoveType.Back, 2);
			GSMovements.Add (Game.MoveType.FrontRight, 1);
			GSMovements.Add (Game.MoveType.FrontLeft, 1);
			GSMovements.Add (Game.MoveType.BackRight, 2);
			GSMovements.Add (Game.MoveType.BackLeft, 2);
			GSMovements.Add (Game.MoveType.TurnRight, 1);
			GSMovements.Add (Game.MoveType.TurnLeft, 1);
			GSMovements.Add (Game.MoveType.TurnBack, 2);
			GSMovements.Add (Game.MoveType.ForwardTurnRight, 1);
			GSMovements.Add (Game.MoveType.ForwardTurnLeft, 1);
			GSMovements.Add (Game.MoveType.FrontRightTurnRight, 1);
			GSMovements.Add (Game.MoveType.FrontRightTurnLeft, 1);
			GSMovements.Add (Game.MoveType.FrontLeftTurnRight, 1);
			GSMovements.Add (Game.MoveType.FrontLeftTurnLeft, 1);
			GSMovements.Add (Game.MoveType.RightTurnRight, 1);
			GSMovements.Add (Game.MoveType.RightTurnLeft, 1);
			GSMovements.Add (Game.MoveType.LeftTurnRight, 1);
			GSMovements.Add (Game.MoveType.LeftTurnLeft, 1);
			GSMovements.Add (Game.MoveType.BackTurnRight, 2);
			GSMovements.Add (Game.MoveType.BackTurnLeft, 2);
			GSMovements.Add (Game.MoveType.BackRightTurnLeft, 2);
			GSMovements.Add (Game.MoveType.BackLeftTurnRight, 2);
			GSMovements.Add (Game.MoveType.BackLeftTurnLeft, 2);
			GSMovements.Add (Game.MoveType.BackRightTurnRight, 2);
			return GSMovements;
			//returns Genestealer movements if unit is a Genestealer
		}
		else if (unitType == Game.EntityType.SM) {
			Dictionary<Game.MoveType, int> SMMovements = new Dictionary<Game.MoveType, int>();
			SMMovements.Add (Game.MoveType.Forward, 1);
			SMMovements.Add (Game.MoveType.Back, 2);
			SMMovements.Add (Game.MoveType.FrontRight, 1);
			SMMovements.Add (Game.MoveType.FrontLeft, 1);
			SMMovements.Add (Game.MoveType.BackRight, 2);
			SMMovements.Add (Game.MoveType.BackLeft, 2);
			SMMovements.Add (Game.MoveType.TurnRight, 1);
			SMMovements.Add (Game.MoveType.TurnLeft, 1);
			return SMMovements;
			//returns Space marine movements if unit is a Space marine
		} 
		else {
			Debug.Log ("Invalid actionSet, no valid unit type selected. (getMoveSet, UnitData)");
			return null;
			//Error checking ad message
		}
	}

	public static Game.ActionType[] getTargetActionSet(Game.EntityType executor, Game.EntityType target)
	{
		return null;
	}

	public static int getMeleeDice(Game.EntityType unitType)//contents modified by Nick Lee, 13-9-2014
	{//returns amount of melee dice (attacks) based on the unit type
		if (unitType == Game.EntityType.SM)
		{
			return 1; //if space marine returns 1
		}
		else if (unitType == Game.EntityType.GS)
		{
			return 3; //if genestealer returns 3
		}
		else {
			Debug.Log ("Invalid unit type selected, no melee available to selected unit. (getMeleeDice, UnitData)");
			return 0; //Error catching and message
		}
	} //untested

	public static int getDeathHitCount(Game.EntityType unitType)
	{//returns amount of hits a unit can take
		return 1; //hits required to die
	}

	public static int getRangedDiceCount(Game.EntityType unitType) //contents modified by Nick Lee, 13-9-2014
	{//returns the ranged dice (attacks) of a unit based on its type
		if (unitType == Game.EntityType.SM)
		{
			return 2; //if space marine returns 2
		}
		else {
			Debug.Log ("Invalid unit type selected, no ranged attack available to selected unit. (getRangedDiceCount, UnitData)");
			return 0; //Error catching and message
		}
	} //untested

	public static int getAPCost(Game.ActionType action) //Written by Nick Lee, 15-9-2014
	{//Gets the AP cost of various actions and returns them
		if (action == Game.ActionType.Move) {
			return 1;
		}//If action is move returns AP cost of action type move
		else if (action == Game.ActionType.Reveal) {
			return 0;
		}//If action is reveal returns AP cost of action type reveal
		else if (action == Game.ActionType.ToggleDoor) {
			return 1;
		}//If action is ToggleDoor returns AP cost of action type ToggleDoor
		else if (action == Game.ActionType.Attack) {
			return 1;
		}//If action is Attack returns AP cost of action type Attack
		else if (action == Game.ActionType.Shoot) {
			return 1;
		}//If action is Shoot returns AP cost of action type Shoot
		else if (action == Game.ActionType.Overwatch) {
						return 2; //If action is Overwatch returns AP cost of action type Overwatch
				} else {
						Debug.Log ("Invalid action, does not exist (getAPCost, UnitData)");
						return 0;
				}//Error checking and message
	}//untested
}
