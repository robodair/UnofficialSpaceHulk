using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Action : MonoBehaviour {

	//Created by Ian Mallett 1.9.14

	//Edits

	//Ian Mallett 14.9.14
	//Changed the dice roll variable to a dictionary, from an int[]

	//Ian Mallett 17.9.14
	//Added sustainedFireChanged, lostOverwatch, unitJams, target, movePosition, and moveFacing
	
	public Game.ActionType actionType;
	public Unit executor;
	public Unit target;
	public Vector2 movePosition;
	public Game.Facing moveFacing;
	public int APCost;
	public bool unitJams;
	public Unit[] destroyedUnits;
	public Unit[] sustainedFireLost;
	public Dictionary<Unit, Unit> sustainedFireChanged;
	public Unit[] lostOverwatch;
	public Dictionary<Game.PlayerType, int[]> diceRoll;
}
