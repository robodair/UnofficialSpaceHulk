using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DeploymentArea
{
	//Class created by Ian Mallett 9.9.14

	//Edits

	//Ian Mallett 22.9.14
	//Added the model variable.

	//Ian Mallett 15.10.14
	//Changed units to a list

	//This is a data storage class representing a deployment
	//area that can contain more than one unit. It also stores
	//the coordinates of the real square adjacent to it, and
	//the relative position of it.

	public List<Unit> units;
	public Vector2 adjacentPosition;
	public GameObject model;
	//This facing is the relative direction of the adjacent position
	//FROM the deployment area.
	public Game.Facing relativePosition;
	public Game.PlayerType owner;
}