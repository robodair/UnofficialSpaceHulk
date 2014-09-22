using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Map : MonoBehaviour {

	//Created by Ian Mallett 1.9.14

	//Edits

	//Ian Mallett 8.9.14
	//Added DeploymentArea class to the Map class
	//Created getSquare method in Map class with functionality.
	//Gave full functionality to the hasSquare,  isOccupied,
	//getOccupant, hasDoor, isDoorOpen setDoorState, findUnit,
	//getUnit, areAdjacent and areLinked methods.
	//Added partial functionality to removeUnit method.

	//Ian Mallett 9.9.14
	//Added retrieveUnit method with full functionality
	//Completed functionality of removeUnit and shiftUnit methods.

	//Ian Mallett 10.9.14
	//Replaced all instances of .Equals(null) with != null, after
	//research and testing showing that .Equals(null) will cause
	//Null Reference Exceptions.

	//Ian Mallett 11.9.14
	//Added size variable

	//Ian Mallett 18.9.14
	//Added getMarines method with full functionality.

	//Ian Mallett 22.9.14
	//Added findArea method with full functionality
	//Added the placeUnit method, and made shiftUnit call it.

	/* Map Class
	 * The map class is the class that represents the map of the game.
	 * It stores the map as an array of Square objects which are editable
	 * in the inspector. It also stores a set of areas that are not
	 * "squares" but are part of the map, such as deployment zones.
	 * Unit in deployment zones are referenced with a Vector2 with a
	 * negative x value. The x value represents the index of the
	 * deployment area starting counting at -1 and going down.
	 * i.e. A unit at (-1, 0) would be the first unit in the first
	 * deployment zone in the "otherAreas" array.
	 * As the information in the map class is complex, most of
	 * the information in it is given out using methods.
	 */

	public Square[] map;
	public Vector2 size;
	public DeploymentArea[] otherAreas;


	//True if there is a square at the position
	public bool hasSquare(Vector2 position)
	{
		for (int i = 0; i < map.Length; i++)
		{
			if (map[i].position == position)
			{
				return true;
			}
		}
		return false;
	}

	//Returns the Square object at the position to
	//the caller. Null if there is no such Square
	public Square getSquare(Vector2 position)
	{
		for (int i = 0; i < map.Length; i++)
		{
			if (map[i].position == position)
			{
				return map[i];
			}
		}
		return null;
	}

	//Returns the DeploymentArea object that the given
	//GameObject represents. Null if there is no such
	//DeploymentArea
	public DeploymentArea findArea(GameObject model)
	{
		for (int i = 0; i < otherAreas.Length; i++)
		{
			if (otherAreas[i].model.Equals (model))
			{
				return otherAreas[i];
			}
		}

		return false;
	}

	//Returns true if the both squares exist, and the 
	//squares are within 1 of each other in both
	//dimensions, or if one position is a deployment
	//area and is adjacent to the other.
	public bool areAdjacent(Vector2 position1, Vector2 position2)
	{
		//Check for adjacency as squares
		if (hasSquare(position1) && hasSquare(position2))
		{
			if (Mathf.Abs(position1.x - position2.x) <= 1
			 &&	Mathf.Abs(position1.y - position2.y) <= 1)
			{
				return true;
			}
		}

		//Check for adjacency as a deployment area and a square
		if (position1.x < 0 && hasSquare(position2))
		{
			if (otherAreas.Length > (-1 - position1.x))
			{
				if (otherAreas[-1 - (int)position1.x].adjacentPosition.Equals(position2))
				{
					return true;
				}
			}
		}

		if (position2.x < 0 && hasSquare(position1))
		{
			if (otherAreas.Length > (-1 - position2.x))
			{
				if (otherAreas[-1 - (int)position2.x].adjacentPosition.Equals(position1))
				{
					return true;
				}
			}
		}

		//If none of the above the positions are not adjacent
		return false;
	}

	//Returns true if both squares exist, are adjacent,
	//and if they are diagonally adjacent, there must be
	//a square diagonlly adjacent to both that is not
	//occupied for them to be linked. If they are
	//cardinally adjacent or adjacent as a deployment area
	//and a square, they are linked.
	public bool areLinked(Vector2 position1, Vector2 position2)
	{
		if(areAdjacent(position1, position2))
		{
			//If they are diagonally adjacent
			if(position1.x != position2.x
			&& position1.y != position2.y)
			{
				//Check whether the squares adjacent to both are unoccupied
				//If either exists and is unoccupied the squares are linked
				Square adjSquare1 = getSquare(new Vector2(position1.x, position2.y));
				if (adjSquare1 != null)
				{
					if (!adjSquare1.isOccupied)
					{
						return true;
					}
				}

				Square adjSquare2 = getSquare(new Vector2(position2.x, position1.y));
				if (adjSquare2 != null)
				{
					if (!adjSquare2.isOccupied)
					{
						return true;
					}
				}
			}

			//If they are cardinally adjacent or adjacent
			//as a deployment position and a square, they
			//are linked
			else
			{
				return true;
			}
		}
		//If none of the above, they are not linked
		return false;
	}

	//Return the position of a unit given its game object
	public Vector2 findUnit(GameObject model)
	{
		Unit unit = getUnit(model);
		if (unit != null)
		{
			return unit.position;
		}

		Debug.LogError("No such unit for \"findUnit\" method");
		return new Vector2(0, 0);
	}

	//Return a unit class given its model
	public Unit getUnit(GameObject model)
	{
		//Search the map for units or doors
		for (int i = 0; i < map.Length; i++)
		{
			if (map[i].isOccupied)
			{
				if (map[i].occupant.gameObject.Equals(model))
				{
					return map[i].occupant;
				}
			}
			if (map[i].door.gameObject.Equals(model))
			{
				return map[i].door;
			}
		}

		//Search the other areas
		for (int i = 0; i < otherAreas.Length; i++)
		{
			for (int j = 0; j < otherAreas[i].units.Count; j++)
			{
				if (((Unit)otherAreas[i].units[j]).gameObject.Equals(model))
				{
					return (Unit)otherAreas[i].units[j];
				}
			}
		}

		//If no such unit, return null
		return null;
	}

	//Returns a boolean that is true if the square
	//at the position exists and is occupied.
	public bool isOccupied(Vector2 position)
	{
		Square square = getSquare(position);
		if (square != null)
		{
			return square.isOccupied;
		}
		return false;
	}

	//Returns the occupant of the square at
	//the position. Null if there is no such
	//unit.
	public Unit getOccupant(Vector2 position)
	{
		Square square = getSquare(position);
		if (square != null)
		{
			if (square.isOccupied)
			{
				return square.occupant;
			}
		}
		return null;
	}

	//Returns true if the square at the position
	//exists and has a door in it. Returns if
	//otherwise, or if there is no such square.
	public bool hasDoor(Vector2 position)
	{
		Square square = getSquare(position);
		if (square != null)
		{
			return square.hasDoor;
		}
		return false;
	}

	//Returns true if the square at the position
	//exists, has a door, and the door is open.
	//If there is no such door or no such square,
	//logs an error. Defaults to true to prevent
	//permanently blocking a path
	public bool isDoorOpen(Vector2 position)
	{
		Square square = getSquare(position);
		if (square != null)
		{
			if (square.hasDoor)
			{
				return square.doorIsOpen;
			}
			else
			{
				Debug.LogError("No such door for \"isDoorOpen\" method" +
				               "Method called for (" + position.x + ", " + position.y
				               + ")");
			}
		}
		else
		{
			Debug.LogError("No such square for \"isDoorOpen\" method.\r\n" +
						   "Method called for (" + position.x + ", " + position.y
			               + ")");
		}

		return true;
	}

	//Method sets the open state of the door at the position
	//to open if the boolean is true, or closed otherwise.
	//Logs an error if there is no such square or the square
	//has no door, or if the door is closing, and the square
	//is already occupied.
	public void setDoorState(Vector2 position, bool isOpen)
	{
		Square square = getSquare(position);
		if (square != null)
		{
			if (square.hasDoor)
			{
				//If opening the door
				if (isOpen == true)
				{
					//Remove the square's occupant if it was the door
					if (square.occupant.Equals(square.door))
					{
						square.occupant = null;
						square.isOccupied = false;
					}
					square.doorIsOpen = true;
				}
				//If closing the door
				else
				{
					//Only do something if the door was open
					if (square.doorIsOpen == true)
					{
						//Make the occupant of the square the door,
						//but if it was already occupied, log an error.
						if (!square.isOccupied)
						{
							square.doorIsOpen = false;
							square.occupant = square.door;
							square.isOccupied = true;
						}
						else
						{
							Debug.LogError("Square is already occupied for \"setDoorState\" " +
							               "method.\r\nMethod called for (" + position.x + ", " +
							               position.y + ")");
						}
					}
				}
			}
			else
			{
				Debug.LogError("No such door for \"setDoorState\" method" +
				               "Method called for (" + position.x + ", " + position.y +
				               ")");
			}
		}
		else
		{
			Debug.LogError("No such square for \"setDoorState\" method.\r\n" +
			               "Method called for (" + position.x + ", " + position.y +
			               ")");
		}
	}

	//Removes the unit from the inital position, and places it at the final
	//position and facing. If placing the unit in a deployment area, the
	//y coordinate does not have any effect, and the unit is simply placed
	//as the last index in the set. The unit's position is set to be appropriate.
	//Logs an error if the requested inital position does not exist, there is no
	//unit at the position, the final position does not exist, or the final
	//position is already occupied.
	public void shiftUnit(Vector2 initialPosition, Vector2 finalPosition, Game.Facing finalFacing)
	{
		//Remove the unit from the initial square.
		Unit movingUnit = retrieveUnit(initialPosition, "shiftUnit");

		if (movingUnit != null)
		{
			movingUnit.position = finalPosition;
			movingUnit.facing = finalFacing;
			placeUnit (movingUnit);
		}
	}

	//Places the given unit at the position on the map. Logs an error if
	//the square does not exist or is already occupied.
	public void placeUnit(Unit unit)
	{
		//Place the unit at the final position
		//If it is a square
		if (unit.position.x >= 0)
		{
			Square square = getSquare(unit.position);
			if (square != null)
			{
				//Not occupied
				if (!square.isOccupied)
				{
					square.occupant = unit;
					square.isOccupied = true;
				}
				//Already occupied
				else
				{
					Debug.LogError("Specified square already occupied for \"placeUnit\" " +
					               "method.\r\nMethod called for (" + unit.position.x + ", " +
					               unit.position.y + ")");
				}
			}
			//No such position
			else
			{
				Debug.LogError("No such destination square for \"placeUnit\" " +
				               "method.\r\nMethod called for (" + unit.position.x +
				               ", " + unit.position.y + ")");
			}
		}
		
		//If the position is a deployment area add it to the deployment area
		else if (otherAreas.Length > (-1 - (int)unit.position.x))
		{
			int areaIndex = -1 - (int)unit.position.x;
			DeploymentArea area = otherAreas[areaIndex];
			area.units.Add(unit);
		}
		
		//If there is no such position
		else
		{
			Debug.LogError("No such destination square for \"placeUnit\" " +
			               "method.\r\nMethod called for (" + unit.position.x +
			               ", " + unit.position.y + ")");
		}
	}

	//Removes the unit at the position, resetting all other positions
	//that change in the process. Logs an error if there is no such
	//unit or no such position.
	public void removeUnit(Vector2 position)
	{
		retrieveUnit(position, "removeUnit");
	}

	//Removes the unit at the position, resetting all other positions
	//that change in the process. Logs an error if there is no such
	//unit or no such position. Takes the calling method's name and
	//the position as parameters, and returns the class for the
	//removed unit.
	private Unit retrieveUnit(Vector2 position, string callingMethod)
	{
		Unit returnUnit;
		//If it is a position for a square
		if (position.x >= 0)
		{
			Square square = getSquare(position);
			if (square != null)
			{
				if (square.isOccupied)
				{
					//If the door is closed, it is removed
					if (!square.doorIsOpen)
					{
						returnUnit = square.occupant;
						square.occupant = null;
						square.isOccupied = false;
						square.doorIsOpen = true;
						square.hasDoor = false;
						return returnUnit;
					}
					//Otherwise, just remove the unit
					else
					{
						returnUnit = square.occupant;
						square.occupant = null;
						square.isOccupied = false;
						return returnUnit;
					}
				}
				//No such unit
				else
				{
					Debug.LogError("No such unit for \"" + callingMethod + "\" method\r\n" +
					               "Method called for (" + position.x + ", " + position.y +
					               ")");
					return null;
				}
			}
			//No such square
			else
			{
				Debug.LogError("No such square for \"" + callingMethod + "\" method\r\n" +
				               "Method called for (" + position.x + ", " + position.y +
				               ")");
				return null;
			}
		}
		//If it is a position in a deployment area
		else if (otherAreas.Length > (-1 - position.x))
		{
			int areaIndex = -1 - (int)position.x;
			DeploymentArea area = otherAreas[areaIndex];
			//Remove the unit
			if (area.units.Count > position.y)
			{
				returnUnit = (Unit)area.units[(int)position.y];
				area.units.RemoveAt((int)position.y);
				//Reset the positions of the remaining units
				for (int i = 0; i < area.units.Count; i++)
				{
					Unit unit = (Unit)area.units[i];
					unit.position = new Vector2(position.x, i);
				}
				return returnUnit;
			}
			//Unless there is no such unit
			else
			{
				Debug.LogError("No such unit for \"" + callingMethod + "\" method\r\n" +
				               "Method called for (" + position.x + ", " + position.y +
				               ")");
				return null;
			}
		}
		//No such position
		else
		{
			Debug.LogError("No such square for \"" + callingMethod + "\" method\r\n" +
			               "Method called for (" + position.x + ", " + position.y +
			               ")");
			return null;
		}
	}

	//Method to find all the marines on the map, especially for updating LoS or checking
	//overwatch and such.
	public List<Unit> getMarines()
	{
		List<Unit> returnList = new List<Unit>();

		for (int i = 0; i < map.Length; i++)
		{
			if (map[i].isOccupied)
			{
				if (map[i].occupant.unitType == Game.EntityType.SM)
				{
					returnList.Add (map[i].occupant);
				}
			}
		}

		return returnList;
	}
}
