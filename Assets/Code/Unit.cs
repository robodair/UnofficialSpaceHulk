using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Unit {

	//Created by Ian Mallett 1.9.14

	//Edits
	//Ian Mallett 9.9.14
	//Added hasSustainedFire boolean

	//Ian Mallett 10.9.14
	//Removed gameObject from constructor
	//Wrote constructor body
	//Changed currentLoS from an array to a list

	//Ian Mallett 17.9.14
	//Changed sustainedFireTarget to unit type.

	//Ian Mallett 18.9.14
	//Added isJammed variable.
	
	//The unit class is a data storage class representing
	//a single unit or door. It stores the unit's permanent
	//and dynamic data, including the game object which is
	//the 3D model representing that unit.

	public string name;
	public int AP;
	public Game.EntityType unitType;
	public Vector2 position;
	public Game.Facing facing;
	public bool isOnOverwatch;
	public bool hasSustainedFire;
	public bool isJammed;
	public Unit sustainedFireTarget;
	public List<Vector2> currentLoS;
	public int noOfGS;
	public GameObject gameObject;

	//sprite objects added Alisdair 5-10-14
	public GameObject overwatchSprite;
	public GameObject sustainedFireSprite;

	public Unit(string name, Game.EntityType unitType,
	            Vector2 position, Game.Facing facing)
	{
		this.name = name;
		this.unitType = unitType;
		this.position = position;
		this.facing = facing;
		this.AP = 0;
		this.isOnOverwatch = false;
		this.hasSustainedFire = false;
		this.isJammed = false;
		this.sustainedFireTarget = null;
		this.currentLoS = new List<Vector2>();
		this.noOfGS = 1;
	}
}
