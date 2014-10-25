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

	//Ian Mallett 21.9.14
	//Uncommented a line that had not been supported previously.

	//Ian Mallett 3.10.14
	//Made the AI set the turn back to the Space Marine player's turn.

	//Ian Mallett 7.10.14
	//Added support for creating paths out of a 

	//Ian Mallett 23.10.14
	//Added AI methods and created Genestealer AI
	//with sub-optimal movement and attacking.

	//Ian Mallett 24.10.14
	//Added functionality to the placeBlips method

	//Ian Mallett 25.10.14
	//Added functionality to the isInVision method
	//Added checks to canAttack method whether the unitType can attack.
	//Added an overload for the getPath method which prevents the path going into Space Marine vision
	//Prevented blips from moving into Space Marine vision
	//Add blip revealing

	public Game game;
	public Map map;

	private List<Game.MoveType> moveOrder;




	void Awake()
	{
		moveOrder = new List<Game.MoveType> ();

		moveOrder.Add (Game.MoveType.Forward);
		moveOrder.Add (Game.MoveType.TurnRight);
		moveOrder.Add (Game.MoveType.TurnLeft);
		moveOrder.Add (Game.MoveType.FrontRight);
		moveOrder.Add (Game.MoveType.FrontLeft);
		moveOrder.Add (Game.MoveType.Back);
		moveOrder.Add (Game.MoveType.TurnBack);
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
		return getPath (initialSquare, initialFacing, targetSquare, targetFacing, moveSet, true, false, false, false);
	}

	public Path getPath(Vector2 initialSquare, Game.Facing initialFacing,
	                    Vector2 targetSquare, Game.Facing targetFacing,
	                    Dictionary<Game.MoveType, int> moveSet,
	                    bool canMoveIntoVision)
	{
		return getPath (initialSquare, initialFacing, targetSquare, targetFacing, moveSet, canMoveIntoVision, false, false, false);
	}

	private Path getPath(Vector2 initialSquare, Game.Facing initialFacing,
						 Vector2 targetSquare, Game.Facing targetFacing,
						 Dictionary<Game.MoveType, int> moveSet, bool canMoveIntoVision,
	                     bool ignoreDoors, bool ignoreGenestealers, bool ignoreSpaceMarines)
	{
		//Create the set of visited positions
		List<Path> completedPositions = new List<Path>();

		//Create a set of current positions, and add the initial position to it
		List<Path> currentPositions = new List<Path>();
		currentPositions.Add (new Path(initialSquare, initialFacing));

		Path bestPath = null;
		bool pathComplete = false;


		//Check whether the best path is to go nowhere
		if (initialSquare == targetSquare &&
		    initialFacing == targetFacing)
		{
			return new Path(initialSquare, initialFacing);
		}


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

			//End if the shortest path is not shorter than the bestPath
			if (bestPath != null)
			{
				if (currentPath.APCost >= bestPath.APCost)
				{
					pathComplete = true;
					break;
				}
			}

			//Find each useful addition to the path
			for (int moveTypeIndex = 0; moveTypeIndex < moveOrder.Count; moveTypeIndex++)
			{
				Game.MoveType move = moveOrder[moveTypeIndex];
				//If the path wouldn't be trying to move off a deployment area unsuccessfully
				if (currentPath.finalSquare.x >= 0 || move == Game.MoveType.Forward)
				{
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
								if (!map.isOccupied(newPath.finalSquare) ||
								    newPath.finalSquare == initialSquare ||
								    (ignoreDoors && map.isOccupied (newPath.finalSquare) &&
								 	 map.getOccupant (newPath.finalSquare).unitType == Game.EntityType.Door) ||
								    (ignoreGenestealers && map.isOccupied (newPath.finalSquare) &&
								 	 (map.getOccupant (newPath.finalSquare).unitType == Game.EntityType.GS ||
								 	  map.getOccupant (newPath.finalSquare).unitType == Game.EntityType.Blip)) ||
								    (ignoreSpaceMarines && map.isOccupied (newPath.finalSquare) &&
								 	 map.getOccupant (newPath.finalSquare).unitType == Game.EntityType.SM))
								{
									if (canMoveIntoVision || !isInVision (newPath.finalSquare))
									{
										if (map.areLinked (currentPath.finalSquare, newPath.finalSquare))
										{
											//Check whether the path reaches the end
											if (newPath.finalSquare == targetSquare &&
											    newPath.finalFacing.Equals (targetFacing))
											{
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
		Vector2 newPosition = new Vector2();
		Game.Facing newFacing = Game.Facing.North;

		//If the unit is on a square
		if (path.finalSquare.x >= 0) {
			newPosition = (Vector2)path.finalSquare +
						  (Vector2)((Quaternion)game.facingDirection [path.finalFacing] *
									(Vector2)game.moveTransform [move] [0]);
			
			Quaternion newDirection = (Quaternion)game.facingDirection [path.finalFacing] *
									  (Quaternion)game.moveTransform [move] [1];
			
			
			//Find the new Facing
			newFacing = Game.Facing.North;
			foreach (Game.Facing facing in game.facingDirection.Keys) {
				if (Quaternion.Angle (game.facingDirection [facing], newDirection) < 1f) {
					newFacing = facing;
					break;
				}
			}
		}
		//If the unit is on a deployment area
		else
		{
			if (move == Game.MoveType.Forward)
			{
				if (map.otherAreas.Length > -1 - path.finalSquare.x)
				{
					newPosition = map.otherAreas[-1 - (int)path.finalSquare.x].adjacentPosition;
					newFacing = map.otherAreas[-1 - (int)path.finalSquare.x].relativePosition;
				}
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

		Dictionary<Game.MoveType, int> moveSet = UnitData.getMoveSet(unit.unitType);
		
		currentPaths.Add (new Path (unit.position, unit.facing));

		//While finding the set of paths
		while (currentPaths.Count > 0)
		{
			Path currentPath = currentPaths[0];

			
			
			
			//Find every addition to the path
			foreach (Game.MoveType move in moveSet.Keys)
			{
				if (currentPath.finalSquare.x >= 0 || move == Game.MoveType.Forward)
				{
					Path newPath = addMovement (currentPath, move, moveSet);

					//Check whether the unit can get there
					if (newPath.APCost <= unit.AP ||
					    (game.thisPlayer == Game.PlayerType.SM && newPath.APCost <= unit.AP + game.remainingCP))
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

	private bool isInVision(Vector2 position)
	{
		foreach (Unit unit in map.getUnits (Game.EntityType.SM))
		{
			foreach (Vector2 lookPos in unit.currentLoS)
			{
				if (lookPos == position)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AITurn()
	{
		Debug.Log ("MY TURN!");
		placeBlips ();
		revealBlips();
		continueAI ();
	}

	private void placeBlips()
	{
		List<int> deploymentAreaIndexes = new List<int>();
		for (int i = 0; i < map.otherAreas.Length; i++)
		{
			deploymentAreaIndexes.Add (-1);
			int nearestEntrance = 0;
			for (int j = 0; j < map.otherAreas.Length; j++)
			{
				DeploymentArea area = map.otherAreas[j];
				if (!isInVision(area.adjacentPosition) &&
				    !map.isOccupied(area.adjacentPosition) &&
				    !deploymentAreaIndexes.Contains (j))
				{
					//If the position is the nearest so far, make it the current nearest
					if (nearestEntrance == 0 ||
					    distanceToSM(new Vector2(-1 - j, 0)) < nearestEntrance)
					{
						nearestEntrance = distanceToSM(new Vector2(-1 - j, 0));
						deploymentAreaIndexes[i] = j;
					}
				}
			}
		}

		while (deploymentAreaIndexes.Contains (-1))
		{
			deploymentAreaIndexes.Remove (-1);
		}


		if (deploymentAreaIndexes.Count == 0 &&
		    map.otherAreas.Length > 0)
		{
			deploymentAreaIndexes.Add (0);
		}

		if (deploymentAreaIndexes.Count > 0)
		{
			for (int i = 0; i < game.blipsPerTurn; i++)
			{
				int noOfGSVar = Random.Range(0, 11);
				if (noOfGSVar < 4)
				{
					game.deployBlip(deploymentAreaIndexes[i % deploymentAreaIndexes.Count], 1);
				}
				else if (noOfGSVar < 7)
				{
					game.deployBlip(deploymentAreaIndexes[i % deploymentAreaIndexes.Count], 2);
				}
				else
				{
					game.deployBlip(deploymentAreaIndexes[i % deploymentAreaIndexes.Count], 3);
				}
			}
		}
	}

	private void revealBlips()
	{
		foreach (Unit blip in map.getUnits (Game.EntityType.Blip))
		{
			//The blip must be on the map
			if (blip.position.x > 0)
			{
				revealBlip(blip);
			}
		}
	}

	private void revealBlip (Unit blip)
	{
		int gsToPlace = blip.noOfGS;
		game.ioModule.removeUnit (blip.position);
		map.removeUnit (blip.position);
		//Place the central GS
		game.deploy (Game.EntityType.GS, blip.position, deployFacing (blip.position));
		gsToPlace--;

		if (gsToPlace > 0)
		{
			//Find the available reveal positions
			List<Vector2> availablePositions = new List<Vector2> ();
			for (int i = 0; i < 9; i++)
			{
				Vector2 checkPos = blip.position + new Vector2((i%3) - 1, (i/3) - 1);
				if (map.hasSquare(checkPos) && !map.isOccupied (checkPos) && !isInVision (checkPos))
				{
					availablePositions.Add (checkPos);
				}
			}

			if (availablePositions.Count < gsToPlace)
			{
				gsToPlace = availablePositions.Count;
			}

			//Sort available positions into nearestPositions
			List<Vector2> nearestPositions = new List<Vector2>();
			for (int i = 0; i < availablePositions.Count; i++)
			{
				Vector2 placePosition = availablePositions[i];
				for (int j = 0; j <= nearestPositions.Count; j++)
				{
					if (j == nearestPositions.Count)
					{
						nearestPositions.Add (placePosition);
						break;
					}
					else
					{
						if (distanceToSM(placePosition) < distanceToSM(nearestPositions[j]))
						{
							nearestPositions.Insert(j, placePosition);
							break;
						}
					}
				}
			}

			//Place a Genestealer in each nearest position in order, until out of Genestealers
			for (int i = 0; i < gsToPlace; i++)
			{
				game.deploy (Game.EntityType.GS, nearestPositions[i], deployFacing(nearestPositions[i]));
			}


		}

	}
	
	private Game.Facing deployFacing(Vector2 position)
	{
		Path toSM = findNearestSM (position, Game.Facing.North, true, true, true, orthoSet());
		if (toSM != null)
		{
			return firstFacing(toSM);
		}
		else
		{
			return Game.Facing.North;
		}
	}

	//The initial facing along the given path
	private Game.Facing firstFacing(Path path)
	{
		Path orthoPath = getPath (path.initialSquare, Game.Facing.North, path.finalSquare, Game.Facing.North, orthoSet(), true, true, true, true);

		Path secondPos = addMovement (new Path(orthoPath.initialSquare, orthoPath.initialFacing), orthoPath.path [0], orthoSet());



		for (int i = 0; i < 4; i++)
		{
			Game.Facing facing = (Game.Facing)i;
			if (secondPos.finalSquare == secondPos.initialSquare + (Vector2)(game.facingDirection[facing]*Vector2.up))
			{
				return facing;
			}
		}
		return Game.Facing.North;
	}
	
	public void continueAI()
	{
		//Find the next unit
		Unit activeUnit = nextUnit (Game.EntityType.GS);
		if (activeUnit == null)
		{
			activeUnit = nextUnit (Game.EntityType.Blip);
		}
		//If no next unit, end turn
		if (activeUnit == null)
		{
			game.setTurn (Game.PlayerType.SM);

		}
		else
		{
			bool actionPerformed = false;
			//If it can attack, attack
			if (canAttack(activeUnit))
			{
				actionPerformed = attackForward (activeUnit);
			}
			//If it can reach a Space Marine avoiding Genestealers, do so
			else if (canReachSM(activeUnit))
			{
				//Attempt to go to a position cardinally adjacent to the nearest SM. If this is impossible
				//(which it shouldn't ever be), use the path to the target.
				Path path = goToCardinal (activeUnit, nearestSM(activeUnit, true, false, true).position, true, false, true);
				if (path != null)
				{
					actionPerformed = nextAction(activeUnit, path);
				}
				else
				{
					path = findNearestSM(activeUnit, true, false, true);
					if (path != null)
					{
						actionPerformed = nextAction(activeUnit, path);
					}
					else
					{
						Debug.LogError (activeUnit.name + " \"canReachSM\", but no path was found");
					}
				}
			}
			//Attempt to move as close to a Space Marine as possible
			else
			{
				//Attempt to go to a position cardinally adjacent to the nearest SM. If this is impossible,
				//use the path to the target.
				Path path = goToCardinal (activeUnit, nearestSM (activeUnit, true, true, true).position , true, true, true);
				if (path != null)
				{
					actionPerformed = nextAction (activeUnit, path);
				}
				else
				{
					path = findNearestSM(activeUnit, true, false, true);
					if (path != null)
					{
						actionPerformed = nextAction (activeUnit, path);
					}
				}
			}

			//If no action was performed, remove the unit from the available units and retry
			if (!actionPerformed)
			{
				activeUnit.AP = 0;
				continueAI ();
			}
		}
	}

	private Unit nextUnit(Game.EntityType unitType)
	{
		List<Unit> availableUnits = new List<Unit> ();

		//Check for Units who have already performed an action,
		//and make a list of all genestealers with maximum ap
		foreach (Unit unit in map.getUnits (unitType))
		{
			if (unit.AP < UnitData.getMaxAP (unitType) &&
			    unit.AP != 0)
			{
				//Return any unit with less than Maximum AP
				return unit;
			}
			else if (unit.AP == UnitData.getMaxAP(unitType))
			{
				availableUnits.Add (unit);
			}
		}

		int shortestPath = 0;
		Unit nearestUnit = null;
		//Find the available Unit closest to a Space Marine
		foreach (Unit unit in availableUnits)
		{
			int distance = distanceToSM(unit.position);
			// If the Unit can't get to any Space Marines
			if (distance == 0)
			{
				unit.AP = 0;
			}
			else if ((shortestPath == 0 ||
					  distance < shortestPath))
			{
				shortestPath = distance;
				nearestUnit = unit;
			}
		}

		if (nearestUnit != null)
		{
			return nearestUnit;
		}

		return null;
	}

	//Perform the next action along the path. Returns whether an action was taken
	private bool nextAction(Unit executor, Path travelPath)
	{
		Path usePath = null;
		//Find the first occupant along the travel path, and make the unit
		//move towards next to that position
		Unit blockage = firstOccupant(executor, travelPath);
		if (blockage != null)
		{
			usePath = goToCardinal (executor, blockage.position, false, false, false);
		}
		//If already next to the position
		if (usePath == null || usePath.path.Count == 0)
		{
			usePath = travelPath;
		}

		//Find the path to the next square in the usePath
		Path nextPos = addMovement (new Path (executor.position, executor.facing),
		                               usePath.path [0],
		                               UnitData.getMoveSet (executor.unitType));
		//If it is a blip, it cannot move into LoS
		if (executor.unitType != Game.EntityType.Blip ||
		    !isInVision (nextPos.finalSquare))
		{
			//If the target square isn't occupied, or the movement is turning on the spot
			if (!map.isOccupied (nextPos.finalSquare) || nextPos.finalSquare == executor.position)
			{
				return move (executor, nextPos);
			}
			else if (map.getOccupant (nextPos.finalSquare).unitType == Game.EntityType.Door)
			{
				Path toDoor = goToCardinal (executor, nextPos.finalSquare, false, false, false);
				if (toDoor != null)
				{
					if (toDoor.path.Count == 0)
					{
						return openDoor(executor);
					}
					else
					{
						return nextAction (executor, toDoor);
					}
				}
				else
				{
					executor.AP = 0;
				}
			}
		}
		
		return false;
	}

	private int distanceToSM(Vector2 position)
	{
		Game.Facing facing = Game.Facing.North;
		//If it starts in a deployment area
		if (position.x < 0)
		{
			if (map.otherAreas.Length > -1 - (int)position.x)
			{
				facing = map.otherAreas[-1 - (int)position.x].relativePosition;
			}
		}
		Path toSM = findNearestSM (position, facing, true, true, true, orthoSet());
		if (toSM != null)
		{
			return toSM.APCost;
		}
		return 0;
	}

	private Unit nearestSM(Unit unit, bool ignoreDoors, bool ignoreGS, bool ignoreSM)
	{
		Path toSM = findNearestSM (unit.position, unit.facing, true, true, true, orthoSet());
		if (toSM != null)
		{
			if (map.isOccupied (toSM.finalSquare))
			{
				return map.getOccupant (toSM.finalSquare);
			}
		}
		return null;
	}

	private Path findNearestSM(Unit unit, bool ignoreDoors, bool ignoreGS, bool ignoreSM)
	{
		return findNearestSM (unit.position, unit.facing, ignoreDoors, ignoreGS, ignoreSM, orthoSet());
	}

	private Path findNearestSM(Vector2 position, Game.Facing facing, bool ignoreDoors, bool ignoreGS, bool ignoreSM, Dictionary<Game.MoveType, int> moveSet)
	{
		Path shortestPath = null;
		//Check each Space Marine to be the closest to the unit
		foreach(Unit SM in map.getUnits (Game.EntityType.SM))
		{
			Path newPath = getPath (position, facing, SM.position, facing,
			                        moveSet, true,
			                        ignoreDoors, ignoreGS, ignoreSM);

			if (shortestPath != null)
			{
				if (shortestPath.APCost > newPath.APCost)
				{
					shortestPath = newPath;
				}
			}
			else
			{
				shortestPath = newPath;
			}
		}

		return shortestPath;
	}

	private Dictionary<Game.MoveType, int> orthoSet()
	{
		Dictionary<Game.MoveType, int> moveSet = new Dictionary<Game.MoveType, int> ();
		moveSet.Add (Game.MoveType.Forward, 1);
		moveSet.Add (Game.MoveType.Right, 1);
		moveSet.Add (Game.MoveType.Left, 1);
		moveSet.Add (Game.MoveType.Back, 1);

		return moveSet;
	}

	private bool canAttack(Unit unit)
	{
		//If the unit can attack
		bool hasAttack = false;
		for (int i = 0; i < UnitData.getActionSet(unit.unitType).Length; i++)
		{
			if (UnitData.getActionSet(unit.unitType)[i] == Game.ActionType.Attack)
			{
				hasAttack = true;
			}
		}
		if (!hasAttack)
		{
			return false;
		}

		//If there is a target to attack
		Unit potentialTarget = map.getOccupant (unit.position + (Vector2)(game.facingDirection [unit.facing] * Vector2.up));
		if (potentialTarget != null)
		{
			return (potentialTarget.unitType == Game.EntityType.SM);
		}
		return false;
	}

	private bool attackForward(Unit unit)
	{
		Unit potentialTarget = map.getOccupant (unit.position + (Vector2)(game.facingDirection [unit.facing] * Vector2.up));
		if (potentialTarget != null)
		{
			ActionManager attack = new ActionManager(unit, Game.ActionType.Attack);
			attack.target = potentialTarget;
			attack.performAction ();
			return true;
		}
		else
		{
			Debug.LogError (unit.name + " tried to attack forward, but there was no target");
			return false;
		}
	}

	private bool move(Unit unit, Path path)
	{
		ActionManager move = new ActionManager (unit, Game.ActionType.Move);
		move.path = path;
		move.performAction ();
		return true;
	}

	private bool openDoor(Unit unit)
	{
		if (map.hasDoor (unit.position + (Vector2)(game.facingDirection[unit.facing]*Vector2.up)))
		{
			ActionManager openDoor = new ActionManager(unit, Game.ActionType.ToggleDoor);
			openDoor.performAction();
			return true;
		}
		return false;
	}

	private bool canReachSM(Unit unit)
	{
		Path testPath = findNearestSM(unit, true, false, true);
		if (testPath != null)
		{
			return testPath.APCost <= unit.AP;
		}
		else
		{
			return false;
		}
	}

	//Method to find the shortest path to a position next to the
	//target position, facing towards it
	private Path goToCardinal(Unit unit, Vector2 targetPosition, bool ignoreDoors, bool ignoreGS, bool ignoreSM)
	{
		Path shortestPath = null;
		for (int i = 0; i < 4; i++)
		{
			Vector2 testPosition = targetPosition + (Vector2)(game.facingDirection[(Game.Facing)i]*Vector2.up);
			Path testPath = getPath (unit.position, unit.facing, testPosition, (Game.Facing)((i + 2)%4), UnitData.getMoveSet (unit.unitType),
			                         true, ignoreDoors, ignoreGS, ignoreSM);
			if (testPath != null)
			{
				if (shortestPath == null)
				{
					shortestPath = testPath;
				}
				else if (testPath.APCost < shortestPath.APCost)
				{
					shortestPath = testPath;
				}
			}
		}

		return shortestPath;
	}

	private Unit firstOccupant(Unit unit, Path path)
	{
		Path testPath = new Path (path.initialSquare, path.initialFacing);
		for (int i = 0; i < path.path.Count; i++)
		{
			testPath = addMovement(testPath, path.path[i], UnitData.getMoveSet (unit.unitType));
			if (map.isOccupied (testPath.finalSquare) &&
			    testPath.finalSquare != testPath.initialSquare)
			{
				return map.getOccupant (testPath.finalSquare);
			}
		}
		return null;
	}
}
