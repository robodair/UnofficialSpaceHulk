using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Path {

	//Created by Ian Mallett 1.9.14

	//Edits
	//Ian Mallett 14.9.14
	//Changed path to a List of MoveType values.
	//Added initialSquare and Facing, and finalSquare and Facing variables
	//Added constructor
	//Added clone constructor

	//Ian Mallett 15.9.14
	//Fixed clone constructor so that it behaved properly.

	public int APCost;
	public Vector2 initialSquare;
	public Game.Facing initialFacing;
	public Vector2 finalSquare;
	public Game.Facing finalFacing;
	public List<Game.MoveType> path;

	public Path (Vector2 initialSquare, Game.Facing initialFacing)
	{
		APCost = 0;
		this.initialSquare = initialSquare;
		this.initialFacing = initialFacing;
		finalSquare = initialSquare;
		finalFacing = initialFacing;
		path = new List<Game.MoveType>();
	}


	public Path (Path copyPath)
	{
		this.APCost = copyPath.APCost;
		this.initialSquare = copyPath.initialSquare;
		this.initialFacing = copyPath.initialFacing;
		this.finalSquare = copyPath.finalSquare;
		this.finalFacing = copyPath.finalFacing;
		this.path = new List<Game.MoveType>(copyPath.path);
	}
}
