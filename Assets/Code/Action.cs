using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Action {

	//Created by Ian Mallett 1.9.14

	//Edits

	//Ian Mallett 14.9.14
	//Changed the dice roll variable to a dictionary, from an int[]

	//Ian Mallett 17.9.14
	//Added sustainedFireChanged, lostOverwatch, unitJams, target, movePosition, and moveFacing

	//Ian Mallett 22.9.14
	//Added completeLoS variable.

	//Ian Mallett 23.9.14
	//Changed Arrays to Lists

	//Ian Mallett 25.9.14
	//Removed the extension of Monobehaviour. 

	//Ian Mallett 4.10.14
	//Improved documentation

	//Ian Mallett 22.10.14
	//Added triggerRemoved, gameOver, and winner variables

	/* This class represents an individual action. A single movement, a unit being set to
	 * overwatch, or a unit shooting at the opponent. Note that any variable that is irrelevant
	 * to the action will be null.
	 */

	//The type of action represented
	public Game.ActionType actionType;

	//The unit performing the action
	public Unit executor;

	//The target of a shoot action or attack action.
	public Unit target;

	//The target position of a single movement
	public Vector2 movePosition;

	//The target facing of a single movement
	public Game.Facing moveFacing;

	//The cost of the action, this may cut into CP.
	//Note that the AP cost of a shot during overwatch does
	//not cost AP.
	public int APCost;

	//Whether the unit performing the action jams.
	public bool unitJams;

	//The set of units that were killed or destroyed during the action
	public List<Unit> destroyedUnits;

	//The set of units that lost their sustained fire bonus (on any target)
	//as a result of the action.
	public List<Unit> sustainedFireLost;

	//The line of sight of every Space Marine on the map following this action.
	//Note that when the action sequence is given to the I/O module, the
	//currentLoS variable of the units will be the final LoS of the units.
	public Dictionary<Unit, List<Vector2>> completeLoS;

	//The line of sight of ever Space Marine on the map before this action.
	public Dictionary<Unit, List<Vector2>> prevLoS;

	//The set of units who gained a sustained fire bonus, or their sustained
	//fire target changed as a result of the action. These are coupled with
	//the new targed of the sustained fire.
	public Dictionary<Unit, Unit> sustainedFireChanged;

	//The set of units that lost their overwatch status as a result of the
	//action. Units that gain overwatch would do so within an Overwatch action
	//only.
	public List<Unit> lostOverwatch;

	//The dice roll related to the action. For a melee combat, there should be a set
	//of dice for each player, otherwise, there would generally be only one set of
	//dice.
	public Dictionary<Game.PlayerType, int[]> diceRoll;

	//The list of units removed by a trigger
	public List<Unit> triggerRemoved;

	//Whether this action causes the game to be over
	public bool gameOver;

	//If the game is over, this variable is the winning player
	public Game.PlayerType winner;
}
