using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class Square {

	//Created by Ian Mallett 1.9.14

	//Edits

	//Ian Mallett 8.9.14
	//Added Serializable tag and door Unit

	//Ian Mallett 9.9.14
	//Added isOccupied variable and model reference

	public Vector2 position;
	public bool isOccupied;
	public Unit occupant;
	public Unit door;
	public bool hasDoor;
	public bool doorIsOpen;
	public GameObject model;
}
