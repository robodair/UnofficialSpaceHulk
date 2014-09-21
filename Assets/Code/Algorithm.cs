using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Algorithm : MonoBehaviour {

	//Created by Ian Mallett 14.9.14

	//Edits
	//Ian Mallett 14.9.14
	//Added game and map references.
	//Added getPath method with functionality
	//Added availableSquares, findLoS, and AITurn methods without functionality
	//Added checks to the getPath method to check whether the target squares exist
	//Added the order in which moves are checked, along with the Start method.

	//Ian Mallett 15.9.14
	//Fixed a bug where currentPath would be removed and would create an infinite loop.
	//Fixed a bug where bestPath would be set even if the target position didn't exist or was occupied.
	//Fixed a bug where the path would never test turning on the spot initially
	//Fixed a bug where south always became north

	//Ian Mallett 17.9.14
	//Started adding functionality to the findLoS

	//Ian Mallett 18.9.14
	//Finished functionality of findLoS method.
	//Changed the findLoS method's return type to List<Vector2>
	//Fixed a bug in the findLoS method caused by float precision.

	public Game game;
	public Map map;

	private List<Game.MoveType> moveOrder;




	void Awake()
	{
		moveOrder = new List<Game.MoveType> ();

		moveOrder.Add (Game.MoveType.Forward);
		moveOrder.Add (Game.MoveType.FrontRight);
		moveOrder.Add (Game.MoveType.FrontLeft);
		moveOrder.Add (Game.MoveType.Back);
		moveOrder.Add (Game.MoveType.TurnBack);
		moveOrder.Add (Game.MoveType.TurnRight);
		moveOrder.Add (Game.MoveType.TurnLeft);
		moveOrder.Add (Game.MoveType.BackLeft);
		moveOrder.Add (Game.MoveType.BackRight);
		moveOrder.Add (Game.MoveType.ForwardTurnRight);
		moveOrder.Add (Game.MoveType.ForwardTurnLeft);
		moveOrder.Add (Game.MoveType.RightTurnRight);
		moveOrder.Add (Game.MoveType.LeftTurnLeft);
		moveOrder.Add (Game.MoveType.RightTurnLeft);
		moveOrder.Add (Game.MoveType.LeftTurnRight);
		moveOrder.Add (Game.MoveType.Right);
		moveOrder.Add (Game.MoveType.Left);
		moveOrder.Add (Game.MoveType.BackTurnRight);
		moveOrder.Add (Game.MoveType.BackTurnLeft);
		moveOrder.Add (Game.MoveType.FrontRightTurnRight);
		moveOrder.Add (Game.MoveType.FrontLeftTurnLeft);
		moveOrder.Add (Game.MoveType.FrontRightTurnLeft);
		moveOrder.Add (Game.MoveType.FrontLeftTurnRight);
		moveOrder.Add (Game.MoveType.BackRightTurnRight);
		moveOrder.Add (Game.MoveType.BackLeftTurnLeft);
		moveOrder.Add (Game.MoveType.BackRightTurnLeft);
		moveOrder.Add (Game.MoveType.BackLeftTurnRight);
	}

	//This method finds the path with the least AP cost from the inital position and
	//facing to the final position and facing, using only the moves in the moveSet with
	//the AP costs in the 
	public Path getPath(Vector2 initialSquare, Game.Facing initialFacing,
	                    Vector2 targetSquare, Game.Facing targetFacing,
	                    Dictionary<Game.MoveType, int> moveSet)
	{
		return getPath (initialSquare, initialFacing, targetSquare, targetFacing, moveSet, false);
	}



	private Path getPath(Vector2 initialSquare, Game.Facing initialFacing,
						 Vector2 targetSquare, Game.Facing targetFacing,
						 Dictionary<Game.MoveType, int> moveSet, bool ignoreOccupants)
	{
		//Create the set of visited positions
		List<Path> completedPositions = new List<Path>();

		//Create a set of current positions, and add the initial position to it
		List<Path> currentPositions = new List<Path>();
		currentPositions.Add (new Path(initialSquare, initialFacing));

		Path bestPath = null;
		bool pathComplete = false;



		//While finding the path
		while (!pathComplete)
		{
			Path currentPath = currentPositions[0];

			//Find the first shortest path
			for (int i = 1; i < currentPositions.Count; i++)
			{
				if (currentPositions[i].APCost < currentPath.APCost)
				{
					currentPath = currentPositions[i];
					break;
				}
			}

			Debug.Log ("CurrentPath: ");
			
			for (int i = 0; i < currentPath.path.Count; i++)
			{
				Debug.Log (currentPath.path[i]);
			}
			
			Debug.Log ("/CurrentPath");


			//End if the shortest path is not shorter than the bestPath
			if (bestPath != null)
			{
				if (currentPath.APCost >= bestPath.APCost)
				{
					Debug.Log ("CurrentPath is shorter than bestPath");
					pathComplete = true;
					break;
				}
			}

			//Find each useful addition to the path
			for (int moveTypeIndex = 0; moveTypeIndex < moveOrder.Count; moveTypeIndex++)
			{
				Game.MoveType move = moveOrder[moveTypeIndex];
				if (moveSet.ContainsKey(move))
				{
					Path newPath = addMovement (currentPath, move, moveSet);


					//Check whether the path already exists
					bool destinationExists = false;

					for (int i = 0; i < currentPositions.Count; i++)
					{
						if (currentPositions[i].finalSquare == newPath.finalSquare &&
						    currentPositions[i].finalFacing.Equals (newPath.finalFacing))
						{
							if (currentPositions[i].APCost > newPath.APCost)
							{
								currentPositions.RemoveAt(i);
								break;
							}
							else
							{
								destinationExists = true;
								break;
							}
						}
					}
					for (int i = 0; i < completedPositions.Count; i++)
					{
						if (completedPositions[i].finalSquare == newPath.finalSquare &&
						    completedPositions[i].finalFacing.Equals (newPath.finalFacing))
						{
							if (completedPositions[i].APCost > newPath.APCost)
							{
								completedPositions.RemoveAt (i);
								break;
							}
							else
							{
								destinationExists = true;
								break;
							}
						}
					}

					//If the destination doesn't already exist, the unit is allowed to move to
					//the target position, add the new path to the current positions
					if (!destinationExists)
					{
						if (map.hasSquare (newPath.finalSquare))
						{
							if (ignoreOccupants || !map.isOccupied(newPath.finalSquare) ||
							    newPath.finalSquare == initialSquare)
							{
								if (map.areLinked (currentPath.finalSquare, newPath.finalSquare))
								{
									//Check whether the path reaches the end
									if (newPath.finalSquare == targetSquare &&
									    newPath.finalFacing.Equals (targetFacing))
									{
										Debug.Log("Setting best path as currentPath + " + newPath.path[newPath.path.Count - 1]);
										if (bestPath != null)
										{
											if (newPath.APCost < bestPath.APCost)
											{
												bestPath = newPath;
											}
										}
										else
										{
											bestPath = newPath;
										}
									}
									else
									{
										currentPositions.Add (newPath);
									}
								}
							}
						}
					}


				}
			}

			//Find the current path and move it to the completed paths
			for (int i = 0; i < currentPositions.Count; i++)
			{

				if (currentPositions[i].Equals(currentPath))
				{
					completedPositions.Add (currentPath);
					currentPositions.RemoveAt(i);
					break;
				}
			}

			//Check whether there are still paths left in currentPositions
			if (currentPositions.Count == 0)
			{
				pathComplete = true;
			}
		}

		return bestPath;
	}

	//Adds the movement to the path and returns a new path with different references.
	private Path addMovement(Path path, Game.MoveType move, Dictionary<Game.MoveType, int> moveSet)
	{
		Vector2 newPosition = (Vector2)path.finalSquare +
			(Vector2)((Quaternion)game.facingDirection[path.finalFacing] *
			          (Vector2)game.moveTransform[move][0]);
		
		Quaternion newDirection = (Quaternion)game.facingDirection[path.finalFacing] *
			(Quaternion)game.moveTransform[move][1];
		
		
		//Find the new Facing
		Game.Facing newFacing = Game.Facing.North;
		foreach (Game.Facing facing in game.facingDirection.Keys)
		{
			if (Quaternion.Angle (game.facingDirection[facing], newDirection) < 1f)
			{
				newFacing = facing;
				break;
			}
		}
		
		
		
		//Create the new path
		Path newPath = new Path(path);
		newPath.path.Add (move);
		newPath.APCost += moveSet[move];
		newPath.finalSquare = newPosition;
		newPath.finalFacing = newFacing;

		return newPath;
	}

	//Returns a set of squares that a unit can get to
	public Dictionary<Square, int> availableSquares(Unit unit)
	{
		List<Path> completedPaths = new List<Path>();

		List<Path> currentPaths = new List<Path>();

//		Dictionary<Game.MoveType, int> moveSet = UnitData.getMoveSet(unit.unitType);
		Dictionary<Game.MoveType, int> moveSet = new Dictionary<Game.MoveType, int>();
		
		currentPaths.Add (new Path (unit.position, unit.facing));

		//While finding the set of paths
		while (currentPaths.Count > 0)
		{
			Path currentPath = currentPaths[0];

			Debug.Log ("Current Path:");
			for (int i = 0; i < currentPath.path.Count; i++)
			{
				Debug.Log (currentPath.path[i]);
			}
			Debug.Log ("\\CurrentPath");
			
			
			
			//Find every addition to the path
			foreach (Game.MoveType move in moveSet.Keys)
			{
				Path newPath = addMovement (currentPath, move, moveSet);

				if (newPath.APCost <= unit.AP)
				{
					//Check whether the path already exists
					bool destinationExists = false;
					
					for (int i = 0; i < currentPaths.Count; i++)
					{
						if (currentPaths[i].finalSquare == newPath.finalSquare &&
						    currentPaths[i].finalFacing.Equals (newPath.finalFacing))
						{
							if (currentPaths[i].APCost > newPath.APCost)
							{
								currentPaths.RemoveAt(i);
								break;
							}
							else
							{
								destinationExists = true;
								break;
							}
						}
					}
					for (int i = 0; i < completedPaths.Count; i++)
					{
						if (completedPaths[i].finalSquare == newPath.finalSquare &&
						    completedPaths[i].finalFacing.Equals (newPath.finalFacing))
						{
							if (completedPaths[i].APCost > newPath.APCost)
							{
								completedPaths.RemoveAt (i);
								break;
							}
							else
							{
								destinationExists = true;
								break;
							}
						}
					}

					//If the destination doesn't already exist, the unit is allowed to move to
					//the target position, add the new path to the current paths
					if (!destinationExists)
					{
						if (map.hasSquare (newPath.finalSquare))
						{
							if (!map.isOccupied(newPath.finalSquare) ||
							    newPath.finalSquare == unit.position)
							{
								if (map.areLinked (currentPath.finalSquare, newPath.finalSquare))
								{

									currentPaths.Add (newPath);

								}
							}
						}
					}
				}
			}

			//Find the current path and move it to the completed paths
			for (int i = 0; i < currentPaths.Count; i++)
			{
				
				if (currentPaths[i].Equals(currentPath))
				{
					completedPaths.Add (currentPath);
					currentPaths.RemoveAt(i);
					break;
				}
			}
		}

		Dictionary<Square, int> returnSet = new Dictionary<Square, int>();
		//Build the return dictionary from the completedPaths
		for (int i = 0; i < completedPaths.Count; i++)
		{
			Square finalSquare = map.getSquare(completedPaths[i].finalSquare);
			if (returnSet.ContainsKey(finalSquare))
			{
				returnSet[finalSquare] = Mathf.Min (returnSet[finalSquare], completedPaths[i].APCost);
			}
			else
			{
				returnSet.Add (finalSquare, completedPaths[i].APCost);
			}
		}

		return returnSet;
	}

	public List<Vector2> findLoS(Unit unit)
	{
		//Find forwards and right
		Vector2 unitForwards = ((Quaternion)game.facingDirection [unit.facing] * Vector2.up);
		Vector2 unitLeft = ((Quaternion)game.facingDirection [unit.facing] * (-Vector2.right));

		List<Vector2> currentRow = new List<Vector2>();

		List<Vector2> previousRow = new List<Vector2>();
		previousRow.Add (unit.position);

		List<Vector2> visibleSquares = new List<Vector2>();

		//For each row unit complete
		int rowNumber = 1;
		while (previousRow.Count > 0)
		{
			int rowLength = 2 * rowNumber + 1;
			Vector2 startPoint = unit.position + (rowNumber * unitForwards) + (rowNumber * unitLeft);

			for (int i = 0; i < rowLength; i++)
			{
				Vector2 position = startPoint - (i * unitLeft);

				bool backRightExists = false;
				bool prevExists = false;
				bool backLeftExists = false;

				//Find the nearby squares in the previous row
				for (int j = 0; j < previousRow.Count; j++)
				{
					if (previousRow[j] == position - unitForwards)
					{
						prevExists = true;
					}

					else if (previousRow[j] == position - unitForwards - unitLeft)
					{
						backRightExists = true;
					}

					else if (previousRow[j] == position - unitForwards + unitLeft)
					{
						backLeftExists = true;
					}
				}


				//Cases for squares in the row
				//Leftwards
				if (i < (rowLength / 2) - 1)
				{
					//See diagonally
					if (backRightExists)
					{
						if (map.areLinked(position, position - unitForwards - unitLeft))
						{
							//If the square isn't occupied, the unit can see
							//through the square.
							if (!map.isOccupied (position))
							{
								currentRow.Add(position);
							}
							
							//Add the square no matter what
							visibleSquares.Add(position);
						}
					}
				}
				//One unit left
				else if (i == (rowLength / 2) - 1)
				{
					//Need to see straight diagonally and straight, unless in the first row
					if ((prevExists || rowNumber == 1) &&
					    backRightExists)
					{
						if (map.areLinked(position, position - unitForwards - unitLeft))
						{
							//If the square isn't occupied, the unit can see
							//through the square.
							if (!map.isOccupied (position))
							{
								currentRow.Add(position);
							}
							
							//Add the square no matter what
							visibleSquares.Add(position);
						}
					}
				}
				//Directly forward
				else if (i == (rowLength / 2))
				{
					//Need to see straight
					if (prevExists)
					{
						if (map.areLinked (position, position - unitForwards))
						{
							//If the square isn't occupied, the unit can see
							//through the square.
							if (!map.isOccupied (position))
							{
								currentRow.Add (position);
							}

							//Add the square no matter what
							visibleSquares.Add (position);
						}
					}
				}
				//One unit right
				else if (i == (rowLength / 2) + 1)
				{
					//Need to see straight and diagonally, unless in the first row
					if ((prevExists || rowNumber == 1) &&
					    backLeftExists)
					{
						if (map.areLinked (position, position - unitForwards + unitLeft))
						{
							//If the square isn't occupied, the unit can see
							//through the square.
							if (!map.isOccupied (position))
							{
								currentRow.Add (position);
							}
							
							//Add the square no matter what
							visibleSquares.Add (position);
						}
					}
				}
				//Rightwards
				else
				{
					//Need to see diagonally
					if (backLeftExists)
					{
						if (map.areLinked (position, position - unitForwards + unitLeft))
						{
							//If the square isn't occupied, the unit can see
							//through the square.
							if (!map.isOccupied (position))
							{
								currentRow.Add (position);
							}
							
							//Add the square no matter what
							visibleSquares.Add (position);
						}
					}
				}

			}

			//Set the previousRow to the currentRow, and clear the current row.
			previousRow = new List<Vector2>(currentRow);

			currentRow = new List<Vector2>();

			rowNumber++;
		}

		return visibleSquares;
	}

	public void AITurn()
	{

	}
}
