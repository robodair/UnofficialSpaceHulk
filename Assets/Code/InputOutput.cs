/* 
 * The InputOutput class handles graphic representation of the map and input from the GUI and mouse clicks
 * Created by Alisdair Robertson 9/9/2014
 * Version 13-10-14.0
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // For use of Lists Alisdair 3-10-14

public class InputOutput : MonoBehaviour {

	public GameObject FloorPiecePrefab; //Added 9/9/2014 Alisdair
	public GameObject SpaceMarinePrefab, GenestealerPrefab, BlipPrefab, OpenDoorPrefab, ClosedDoorPrefab, BlipDeploymentPiecePrefab; //Added 11/9/2014 Alisdair
	public Map mapClass; //Added 11/9/2014 Alisdair
	public Game gameClass; //Added 11/9/2014 Alisdair 

	GameObject selectedUnit; //Added by Alisdair 11/9/14

	//UI and Button References added 14/9/14 by Alisdair
	public GameObject UICanvas;
	GameObject btnAttackGO, btnShootGO, btnMoveGO, btnToggleDoorGO, btnOverwatchGO, btnRevealGO;
	Button btnAttack, btnShoot, btnMove, btnToggleDoor, btnOverwatch, btnReveal;

	//UI text field references 14 Sept 14 by Alisdair
	GameObject unitAPText, playerCPText;

	//Facing Selection canvas Added by Alisdair 17/9/2014
	public GameObject facingSelectionCanvas;
	public GameObject currentFacingSelectionCanvas; //Made public 2.10.14 RB

	//Rory Bolt 25.9.14
	public InputHandler inputHandlerController;

	//End Turn button added 2-10-2014 by Alisdair
	GameObject btnEndTurnGO;
	Button btnEndTurn;

	//Color store for selecton/deselection added Alisdair 3-10-2014
	Color preSelectionColor;

	//Other action showing declerations Alisdair 4-10-14 
	List<Action> showActionsList = new List<Action>();
	public float stepAmmount;
	public int rotateStepMultiplier;
	float stepMoveAmmount;
	float stepRotateAmmount;

	//Float for the elevation of all new units
	public float unitElevation;

	//Declerations for hint objects Alisdair 5-10-14 
	public GameObject overwatchSprite;
	public GameObject sustainedFireSprite;

	//variables for tracking AP and CP
	int currentAP;
	int currentCP;

	//assign the step ammounts in the start method Alisdair 12-10-14
	public void Start(){
		stepMoveAmmount = stepAmmount;
		stepRotateAmmount = stepAmmount * rotateStepMultiplier;
	}

	public void Update(){
		//Get the action from the first position in the list
			//Determine what action type it is
				//Use a switch to direct to correct type of action
					//e.g. if movement, move toward the position @ 1 * time.deltatime
				//Check if the action has been completed (to within a fudge distance)
				//If within fudge distance, complete the action and remove the action object from the list
				//else, leave the action to be iterated again in the next frame

		if(showActionsList.Count != 0){
			Debug.Log ("ShowActionsList.Count != 0, incrementing an action");
			gameClass.changeGameState(Game.GameState.ShowAction);//Cahange the state to showaction state
			Action action = showActionsList[0];
			//Debug.Log ("The action is of type: " + action.actionType);
			//Executor pos and rot
			Unit exeUnit = action.executor;
			Vector3 exePos = action.executor.gameObject.transform.position;
			Quaternion exeRot = action.executor.gameObject.transform.rotation;
			//Debug.Log ("Unit Position: " + exePos);
			//Debug.Log("Unit Rotation: " + exeRot);
		
			//Target pos and rot decleration
			Unit tarUnit;
			Vector3 tarPos;
			Quaternion tarRot;


			//Make the action
			switch (action.actionType){
			case (Game.ActionType.Move):
				//Debug.Log("Update entered Move action sector of switch");
			
				//Create aim pos and rot
				Vector3 aimPos = makePosition(action.movePosition, unitElevation);
				//Debug.Log ("Aim position is " + aimPos);
				Quaternion aimRot = makeRotation(makeFacing(action.moveFacing), exeUnit.unitType);
				//Debug.Log ("Aim Rotation is: " + aimRot);

				//Make part of the movements
				action.executor.gameObject.transform.position = Vector3.MoveTowards(exePos, aimPos, stepMoveAmmount*Time.deltaTime);
				action.executor.gameObject.transform.rotation = Quaternion.RotateTowards(exeRot, aimRot, stepRotateAmmount*Time.deltaTime);

				//Remove overwatch sprites for those units that have lost overwatch or sustained fire
				removeOverwatch(action.lostOverwatch);
				removeSusFire(action.sustainedFireLost);

				//Check to see if the unit is in the correct place and rotation, if so, finish the action and remove the action from the list
				//also update the unit AP and CP fields
				if (exePos == aimPos){
					//Debug.Log("PositionEqual");
					if(exeRot.eulerAngles.y == aimRot.eulerAngles.y){
						//Debug.Log("Rotation Equal");
						//Debug.Log("UpdateCPAP case MOVE with AP: " + action.APCost);
						updateCPAP(action.APCost);
						//Debug.Log("Removing Action Object move to position @: " + action.movePosition);
						showActionsList.RemoveAt(0);
					}
					else{
						//Debug.Log ("Rotation not equal, Rotation aim is: " + aimRot.eulerAngles + "Current Rotation: " + exeRot.eulerAngles);
					}
				}
				else {
					//Debug.Log ("Position not Equal, Position target is: " + aimPos);
				}

				break;

			case (Game.ActionType.Overwatch):
				//determine the position the sprite
				Vector3 spritePosition = exePos;
				spritePosition.y += 1.3f;

				//instantiate the sprite at the position & give reference to the unit
				exeUnit.overwatchSprite = (GameObject) Instantiate(overwatchSprite, spritePosition, Quaternion.identity);

				//Remove overwatch sprites for those units that have lost overwatch or sustained fire
				removeOverwatch(action.lostOverwatch);
				removeSusFire(action.sustainedFireLost);

				//update the CP & AP displays
				Debug.Log("UpdateCPAP case OVERWATCH with AP: " + action.APCost);
				updateCPAP(action.APCost);

				//remove the action
				showActionsList.RemoveAt(0);

				break;

			case (Game.ActionType.ToggleDoor):
				if (action.target != null){
					//target unit being assigned
					tarUnit = action.target;
					tarPos = action.target.gameObject.transform.position;
					tarRot = action.target.gameObject.transform.rotation;

					//Check if the door is open or closed, and replace it with the door of the other type
					if (tarUnit.gameObject.name.Equals("ClosedDoorPrefab")){
						GameObject newDoor = (GameObject) Instantiate(OpenDoorPrefab, tarPos, tarRot);
						Destroy(tarUnit.gameObject);
						tarUnit.gameObject = newDoor;
					}
					else if(tarUnit.gameObject.name.Equals("OpenDoorPrefab")){
						GameObject newDoor = (GameObject) Instantiate(ClosedDoorPrefab, tarPos, tarRot);
						Destroy(tarUnit.gameObject);
						tarUnit.gameObject = newDoor;
					}

					//Finish the action and update the AP
					Debug.Log("UpdateCPAP case TOGGLEDOOR with AP: " + action.APCost);
					updateCPAP(action.APCost);
				}
				break;

			default:
				Debug.LogWarning("Game to show an action now");
				
				break;
			}

			//if that was the last action object in the list, then set the gamestate back to inactive & reselect the unit (to activate the buttons again) ALisdair 13-10-14
			if (showActionsList.Count == 0){
				gameClass.changeGameState(Game.GameState.Inactive);
				if (gameClass.unitSelected){
					gameClass.selectUnit(gameClass.selectedUnit.gameObject);
				}
			}
		}
	}


	public void instantiateUI(){ //Method Added by Alisdair Robertson 11/9/14
		/*
		 * This method creates the UI and then links the buttons to the code here
		 */ 

		Instantiate (UICanvas);//Instantiate the canvas

		//Assign the buttons (and make then not interactable)
		btnAttackGO = GameObject.Find ("BtnAttack");
		btnAttack = btnAttackGO.GetComponent<Button>();
		btnAttack.onClick.AddListener(() => {btnAttackClicked();}); //Assigning the call through code Alisdair 14/9/14 http://answers.unity3d.com/questions/777818/46-ui-calling-function-on-button-click-via-script.html
		btnAttack.interactable = false;

		btnShootGO = GameObject.Find ("BtnShoot");
		btnShoot = btnShootGO.GetComponent<Button>();
		btnShoot.onClick.AddListener(() => {btnShootClicked();});
		btnShoot.interactable = false;

		btnMoveGO = GameObject.Find ("BtnMove");
		btnMove = btnMoveGO.GetComponent<Button>();
		btnMove.onClick.AddListener(() => {btnMoveClicked();});
		btnMove.interactable = false;

		btnToggleDoorGO = GameObject.Find ("BtnToggleDoor");
		btnToggleDoor = btnToggleDoorGO.GetComponent<Button>();
		btnToggleDoor.onClick.AddListener(() => {btnToggleDoorClicked();});
		btnToggleDoor.interactable = false;;

		btnOverwatchGO = GameObject.Find ("BtnOverwatch");
		btnOverwatch = btnOverwatchGO.GetComponent<Button>();
		btnOverwatch.onClick.AddListener(() => {btnOverwatchClicked();});
		btnOverwatch.interactable = false;

		btnRevealGO = GameObject.Find ("BtnReveal");
		btnReveal = btnRevealGO.GetComponent<Button>();
		btnReveal.onClick.AddListener(() => {btnRevealClicked();});
		btnReveal.interactable = false;


		//assign the text elements
		unitAPText = GameObject.Find("APText");
		playerCPText = GameObject.Find ("CPText");
		currentAP = 0;
		currentCP = gameClass.remainingCP;//Changed to reference game class Alisdair 11-10-14
		Debug.Log("UpdateCPAP InstantiateUI with AP: " + 0);
		updateCPAP(0);


		//Assign the method to call from the end turn button added 2-10-14 Alisdair
		btnEndTurnGO = GameObject.Find ("BtnTurn");
		btnEndTurn = btnEndTurnGO.GetComponent<Button>();
		btnEndTurn.onClick.AddListener(() => {btnEndTurnClicked();});
		defineEndTurnBtn(); //Set the state of the end turn button

	}

	public void generateMap () { //Method Added by Alisdair Robertson 9/9/2014

		/*
		 * The First part of this method is generating the visual map
		 */

		for (int i = 0 ; i < mapClass.map.Length ; i++){//Iterate through map list
			Square square = mapClass.map[i]; //extract square object
			Vector2 positionV2 = square.position; //get position

			//Converting Vector2 to Vector3
			//Vector2 y = Vector3 z (North/South)
			//Vector2 x = Vector3 x (East/West)
			//Vector3 y is vertical (leave at constant value)
			int xPos = (int) positionV2.x;
			int zPos = (int) positionV2.y;
			float baseYPos = -0.5f;

			GameObject floorPiece = (GameObject) Instantiate(FloorPiecePrefab, new Vector3(xPos, (baseYPos), zPos), Quaternion.identity); //Create the game object in the scene
			square.model = floorPiece; //Pass reference to the gameobject back to the square

			//Added Alisdair 11/9/2014 Theses are for passing the unit reference back to the square (if needed)
			GameObject doorPiece;
			GameObject unit;

			//Go on to create units or doors - 11/9/14 Alisdair
			//if the square has a unit or door create that unit or door on it
			if (square.isOccupied){

				//Assign facing
				Quaternion facing = makeFacing(square.occupant.facing);

				//Switch to place units
				switch (square.occupant.unitType){
				
					case Game.EntityType.Blip:
						unit = (GameObject) Instantiate(BlipPrefab, new Vector3(xPos, (unitElevation + 0.5f), zPos), makeRotation(facing, Game.EntityType.Door)); //Create the blip object above the floor object
						square.occupant.gameObject = unit; //Pass reference to the gameobject back to the square
						break;
	
					case Game.EntityType.Door:
						doorPiece = (GameObject) Instantiate(ClosedDoorPrefab, new Vector3(xPos, (unitElevation + 0.5f), zPos), makeRotation(facing, Game.EntityType.Door)); //Create the closed door object above the floor object
						square.door.gameObject = doorPiece; //Pass reference to the gameobject back to the square
						square.occupant.gameObject = doorPiece; //Pass the reference to the occupant as well Added by Alisdair 26/9/14
						break;
				
					case Game.EntityType.GS:
						unit = (GameObject) Instantiate(GenestealerPrefab, new Vector3(xPos, (unitElevation), zPos), makeRotation(facing, Game.EntityType.GS)); //Create the blip object above the floor object
						square.occupant.gameObject = unit; //Pass reference to the gameobject back to the square
						break;

					case Game.EntityType.SM:
						unit = (GameObject) Instantiate(SpaceMarinePrefab, new Vector3(xPos, (unitElevation), zPos), makeRotation(facing, Game.EntityType.SM)); //Create the blip object above the floor object
						square.occupant.gameObject = unit; //Pass reference to the gameobject back to the square
						break;
				}
			}
					
			//if the square has a door and it's open create it
			if (square.hasDoor && square.doorIsOpen){
				doorPiece = (GameObject) Instantiate(OpenDoorPrefab, new Vector3(xPos, (baseYPos + 0.55f), zPos), Quaternion.identity); //Create the open door object above the floor object
				square.door.gameObject = doorPiece; //Pass reference to the gameobject back to the square
			}

		}

		/*
		 * The second part of the generateMap method deals with generating the deployment points (these are not real parts of the map)
		 * Added By Alisdair 11/9/2014
		 */
			for (int i = 0 ; i < mapClass.otherAreas.Length; i++){//Iterate through map list
				DeploymentArea depArea = mapClass.otherAreas[i]; //extract square object
				Vector2 adjPos = depArea.adjacentPosition; //get position of adjecent piece
				
				//Converting Vector2 to Vector3
				//Vector2 y = Vector3 z (North/South)
				//Vector2 x = Vector3 x (East/West)
				//Vector3 y is vertical (leave at constant value)
				int xPos = (int) adjPos.x;
				int zPos = (int) adjPos.y;
				float baseYPos = -0.5f;

				//determine the position of the deployment area based on the facing 
				switch (depArea.relativePosition){

					case Game.Facing.North:
						zPos--;
						break;

					case Game.Facing.East:
						xPos--;
						break;

					case Game.Facing.South:
						zPos++;
						break;

					case Game.Facing.West:
						xPos++;
						break;
					default:
						Debug.LogError("No valid relative position assigned to deployment piece adjacent to xPos: " + xPos + " zPos: " + zPos);
						break;
				}

				Quaternion depAreaFacing = Quaternion.Euler(0,0,0);
				//Added passing of reference to deployment area gameobjects back to the game class. Alisdair 26-9-2014
				depArea.model = (GameObject) Instantiate(BlipDeploymentPiecePrefab, new Vector3(xPos, baseYPos, zPos), depAreaFacing); //Create the game object in the scene
			}


	}

	//Recieve the array of actions to perform Alisdair
	public void showActionSequence(Action[] actions){
		Debug.Log ("Showing an Action Sequence of length: " + actions.Length);
		updateGUIActions(); //Disable the GUI Actions Alisdair 13-10-14
		showActionsList.AddRange(actions);
	}

	//REcieve the array of actions to perform & an ActionManager that calculated the lead up to an involuntary reveal
	//This is for dealing with Overwatch Alisdair 11-10-14
	public void showActionSequence(Action[] actions, ActionManager actionManager){
		Debug.Log ("This showActionSequence does nothing with the Action Manager object, Ian please tell Alisdair what needs to happen, thxx :)" );
		Debug.Log ("Showing an Action Sequence of length: " + actions.Length);
		updateGUIActions(); //Disable the GUI actions Alisdair 13-10-14
		showActionsList.AddRange(actions);
	}

	public void selectUnit (Unit unit, Game.ActionType[] actions){ //Filled by Alisdair 11/9/2014
		/*
		 * Set the display to be appropriate to the selection of this unit, as well as showing/enabling the buttons for the action types.
		 */
		Debug.LogWarning("Unit selected");
		//deselect any previously selected units (if there are any)
		if (selectedUnit != null) {
			deselect ();
		}

		//assign the variable to the new unit
		selectedUnit = unit.gameObject;



		//colour the selectedUnit unit
		preSelectionColor = selectedUnit.renderer.material.color;
		selectedUnit.renderer.material.color = Color.cyan;

		//update the GUI actionst 
		updateGUIActions(actions);

		//get and show the AP and CP for the unit
		currentAP = unit.AP;
		currentCP = gameClass.remainingCP;
		Debug.Log("UpdateCPAP SELECTUNIT with AP: " + 0);
		updateCPAP(0);
	}

	public void deselect(){ //Filled by Alisdair 11/9/2014
		/*
		 * This method removes the mesh renderer tint on the selected unit
		 */
		Debug.LogWarning("Unit deselected");
		//set the render colour on the selected object back to nothing (if there is a selected unit)
		//Must change this to a tint later, rather than a full material colour
		if (selectedUnit != null) {
			selectedUnit.renderer.material.color = preSelectionColor;

			selectedUnit = null;

			//set the gui to show no actions & set AP to 0
			updateGUIActions();
			Debug.Log("UpdateCPAP DESELECT with AP: " + 0);
			currentAP = 0;
			updateCPAP(0);
		} 
		else {
			Debug.LogWarning ("There is not a unit selected.");
		}

	}

	public void showDeployment(Unit[] units, Vector2[] positions){
		Debug.LogError("ShowDeployment method INCOMPLETE. Refer Alisdair.");
	}

	public void placeUnit(Unit unit){ //Added Gameobject Return 22/9/14 Alisdair
		Debug.LogError("placeUnit method partially complete. Refer Alisdair.");


	//Needs to check if the unit is in a deployment area and place it appropriately if this is the case

	//Otherwise the unit is on the map already, therefore place it as normal

		//Instantiate the unit at the position and pass a reference back to the unit class
		switch (unit.unitType){

			case Game.EntityType.Blip:
				unit.gameObject = (GameObject) Instantiate(BlipPrefab, makePosition(unit.position, 1), makeFacing(unit.facing)); //Create the blip object above the floor object & pass it back to the Unit Class
				break;
		
			case Game.EntityType.Door:
				unit.gameObject = (GameObject) Instantiate(ClosedDoorPrefab, makePosition(unit.position, 1), makeFacing(unit.facing)); //Create the closed door object above the floor object unit.gameObject
				break;
		
			case Game.EntityType.GS:
				unit.gameObject = (GameObject) Instantiate(GenestealerPrefab, makePosition(unit.position, 1), makeFacing(unit.facing)); //Create the blip object above the floor objectunit.gameObject
				break;
		
			case Game.EntityType.SM:
				unit.gameObject = (GameObject) Instantiate(SpaceMarinePrefab, makePosition(unit.position, 1), makeFacing(unit.facing)); //Create the blip object above the floor objectunit.gameObject
				break;
		}
	}

	public void removeUnit(Vector2 position){
		Debug.LogError("removeUnit method INCOMPLETE. Refer Alisdair.");
	}

	public void resetMap(){

		//Added removing old gameobjects to this method - Alisdair 19-9-2014

//		Commented out as uneeded Alisdair 25/9/14
//
//		Debug.Log ("Resetting Map - Removing GameObjects");
//
//		for (int i = 0; i < mapClass.map.Length; i++) {
//
//			Square square = (Square) mapClass.map.GetValue(i);
//
//			try{
//				Destroy(square.occupant.gameObject);
//			}
//			catch (UnityException ex)
//			{
//				Debug.Log("Exception - no occupant at Position: " + square.position + "Exception: " + ex);
//			}
//
//			try{
//				Destroy (square.door.gameObject);
//			}
//			catch (UnityException ex)
//			{
//				Debug.Log("Exception - no door at Position: " + square.position + "Exception: " + ex);
//			}
//
//			try{
//				Destroy(square.model);
//			}
//			catch (UnityException ex)
//			{
//				Debug.Log("Exception - no model at Position: " + square.position + "Exception: " + ex);
//			}
//		}
//		

		//Switched to iterating through a list of all the gameobjects, 
		//This way it doesn't matter if references in the map class are incorrect - Alisdair 25/9/14
		//http://answers.unity3d.com/questions/297171/find-all-objects-in-a-scene.html

		GameObject[] gameObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]; //Get an array of all the gameobjects

		foreach(GameObject gameObject in gameObjects)
		{;
			if (gameObject.layer == 9){
				Destroy(gameObject); //If the gameobject is part of the map representation layer, destroy it. (otherwise leave it)
			}
		}

		//Alisdair 11/Sept/2014
		generateMap (); //Rerun generate map so that it matches the map class again

		if (gameClass.unitSelected){ //Reselecting unit after the map has been regenerated.
			gameClass.selectUnit(gameClass.selectedUnit.gameObject);
		}
	}

	void updateGUIActions(){
		/*
		 * This method disables all the action buttons on the GUI
		 */ 

		//make the buttons uninteractable
		btnAttack.interactable = false;
		btnShoot.interactable = false;
		btnMove.interactable = false;
		btnToggleDoor.interactable = false;;
		btnOverwatch.interactable = false;
		btnReveal.interactable = false;
	}

	void updateGUIActions(Game.ActionType[] actions){
		/*
		 * This method is for updating the GUI command buttons to reflect the commands that are allowed for the specific unit
		 */

		//disable all the buttons
		updateGUIActions ();

		//loop throguh all the objects in the list and enable the required buttons
		for (int i = 0; i < actions.Length; i++) {;

			switch (actions[i]){
			case Game.ActionType.Attack:
				btnAttack.interactable = true;
				break;
			case Game.ActionType.Move:
				btnMove.interactable = true;
				break;
			case Game.ActionType.Overwatch:
				btnOverwatch.interactable = true;;
				break;
			case Game.ActionType.Reveal:
				btnReveal.interactable = true;
				break;
			case Game.ActionType.Shoot:
				btnShoot.interactable = true;
				break;
			case Game.ActionType.ToggleDoor:
				btnToggleDoor.interactable = true;
				break;
			default:
				Debug.LogError("There was not an ActionType at position: " + i + " in the actions Array. updateGUIActions(Game.ActionType[] actions)");
				break;
			}
		}
	}

	public void btnAttackClicked(){ //Added By Alisdair 14/9/14
		/*
		 * This method needs to pass the button click back to the Game class so that action can be taken
		 */ 
		inputHandlerController.attack ();
	}

	public void btnMoveClicked(){ //Added By Alisdair 14/9/14
		/*
		 * This method needs to pass the button click back to the Game class so that action can be taken
		 */ 
		inputHandlerController.movement ();//RB 18/9/14
	}

	public void btnOverwatchClicked(){ //Added By Alisdair 14/9/14
		/*
		 * This method needs to pass the button click back to the Game class so that action can be taken
		 */ 
		Debug.LogWarning ("Overwatch Button Clicked, this method is INCOMPLETE. Refer Alisdair");
	}

	public void btnRevealClicked(){ //Added By Alisdair 14/9/14
		/*
		 * This method needs to pass the button click back to the Game class so that action can be taken
		 */ 
		Debug.LogWarning ("Reveal Button Clicked, this method is INCOMPLETE. Refer Alisdair");
	}

	public void btnShootClicked(){ //Added By Alisdair 14/9/14
		/*
		 * This method needs to pass the button click back to the Game class so that action can be taken
		 */ 
		inputHandlerController.attack ();
	}

	public void btnToggleDoorClicked(){ //Added By Alisdair 14/9/14
		/*
		 * This method needs to pass the button click back to the Game class so that action can be taken
		 */ 
		inputHandlerController.toggleDoor ();
	}

	//Method to create a facing selection canvas (at a specified position)
	// - Need to add new gameobject decleration to the top of this class for the button canvas
	// Needs to assign methods to call to the buttons
	public void instantiateFacingSelection(Vector2 position){
		instantiateFacingSelection(position, true, true, true, true);
	}

	//Second method to create a facing selection canvas, with parameters to select which buttons show up
	public void instantiateFacingSelection(Vector2 position, bool north, bool east, bool south, bool west){
		//Create the canvas at the position
		currentFacingSelectionCanvas = (GameObject) Instantiate (facingSelectionCanvas, makePosition(position, 2), Quaternion.Euler (90, 0, 0));
		
		//Assign methods to the buttons
		Button btnNorth = GameObject.Find ("BtnNorth").GetComponent<Button>();
		btnNorth.onClick.AddListener(() => {btnFaceNorth();});
		btnNorth.interactable = north;

		Button btnEast = GameObject.Find ("BtnEast").GetComponent<Button>();
		btnEast.onClick.AddListener(() => {btnFaceEast();});
		btnEast.interactable = east;

		Button btnSouth = GameObject.Find ("BtnSouth").GetComponent<Button>();
		btnSouth.onClick.AddListener(() => {btnFaceSouth();});
		btnSouth.interactable = south;

		Button btnWest = GameObject.Find ("BtnWest").GetComponent<Button>();
		btnWest.onClick.AddListener(() => {btnFaceWest();});
		btnWest.interactable = west;

	}

	//Methods for buttons - Aisdair 17-9-14
	//- each button needs to call a method on the selection/input class to tell it what button has been clicked
	// - the selection canvas that is the parent of the button then needs to be destroyed.
	public void btnFaceNorth(){
		inputHandlerController.orientationClicked (Game.Facing.North);//RB 18.9.14
		Destroy(currentFacingSelectionCanvas);
	}
	public void btnFaceEast(){
		inputHandlerController.orientationClicked (Game.Facing.East);//RB 18.9.14
		Destroy(currentFacingSelectionCanvas);

	}
	public void btnFaceSouth(){
		inputHandlerController.orientationClicked (Game.Facing.South);//RB 18.9.14
		Destroy(currentFacingSelectionCanvas);
		
	}
	public void btnFaceWest(){
		inputHandlerController.orientationClicked (Game.Facing.West);//RB 18.9.14
		Destroy(currentFacingSelectionCanvas);
		
	}

	//Method to convert vector 2 to vector 3 - Alisdair 22-9-14
	Vector3 makePosition(Vector2 position, float elevation){
		int xPos = (int) position.x;
		int zPos = (int) position.y;
		Vector3 v3 = new Vector3 (xPos, elevation, zPos); 
		return v3;
	}

	//Method to get facing from Game enum Added by Alisdair 22/9/14
	Quaternion makeFacing(Game.Facing facingEnum){

		switch (facingEnum){
			case Game.Facing.North:
				return Quaternion.identity;
			
			case Game.Facing.East:
				return Quaternion.Euler(0,90,0);
			
			case Game.Facing.South:
				return Quaternion.Euler(0,180,0);
			
			case Game.Facing.West:
				return Quaternion.Euler(0,270,0);
			
			default:
				Debug.LogError("Unable to determine facing set to Quaternion.identity.");
				return Quaternion.identity;
				
			
		}

	}

	//When end turn button clicked - Added 2-10-14 Alisdair
	void btnEndTurnClicked(){

		switch (gameClass.playerTurn){
			case (Game.PlayerType.GS):
				gameClass.setTurn(Game.PlayerType.SM);
				break;
			case (Game.PlayerType.SM):
				gameClass.setTurn(Game.PlayerType.GS);
				break;
			default:
				Debug.LogError("Houston we have a problem. No player turn is assigned.");
				gameClass.setTurn(Game.PlayerType.GS);
				break;
		}
	}

	//Method to activate or deactivate the end turn button (To be called every time something is clicked on in the interactable script ?Is this the best place?) - Added by 2-10-14 Alisdair
	public void defineEndTurnBtn(){
		//Check to see if the End turn button should be enabled or not, and assign the required state
		if (gameClass.gameState == Game.GameState.AttackSelection){
			if (gameClass.playerTurn == gameClass.thisPlayer) {
				btnEndTurn.interactable = false;
			}
		}

		else if (gameClass.gameState == Game.GameState.MoveSelection){
			if (gameClass.playerTurn == gameClass.thisPlayer) {
				btnEndTurn.interactable = false;
			}
		}

		else if (gameClass.gameState == Game.GameState.NetworkWait) {
			btnEndTurn.interactable = false;
		}

		else {
			btnEndTurn.interactable = true;
		}
	}

	//Adjust quaternion that makes sense for use with the space marine model - Alisdair 2-10-2014
	Quaternion makeRotation(Quaternion reference, Game.EntityType type){

		Quaternion returnQuaternion;

		switch (type){
			case (Game.EntityType.SM):
				int x = (int) reference.eulerAngles.x + 270;
				int y = (int) reference.eulerAngles.y - 90;
				int z = (int) reference.eulerAngles.z;
				returnQuaternion = Quaternion.Euler(x, y, z);
				return returnQuaternion;

			default:
				returnQuaternion = reference;
				return returnQuaternion;
		}
	}

	//Method to remove the overwatch sprites from all the units Alisdair 5-10-14
	void removeOverwatch(List <Unit> units){
		foreach (Unit unit in units){
			if (unit.sustainedFireSprite)
				Destroy(unit.overwatchSprite);
		}
	}

	//Method to remove sustained fire sprites
	void removeSusFire(List <Unit> units){
		foreach (Unit unit in units){
			if (unit.sustainedFireSprite)
				Destroy(unit.sustainedFireSprite);
		}
	}

	//Method to update the displayed CP and AP
	void updateCPAP(int aPUsed){
		Debug.Log("begin Update CPAP, AP Used: " + aPUsed);
		//if it does not cut into CP, just display it
		if (aPUsed < currentAP || aPUsed == currentAP){
			Debug.Log(" aPUsed <= currentAP; Current AP: " + currentAP + " AP Used: " + aPUsed);
			currentAP = currentAP - aPUsed;
			Debug.Log("New Current AP: " + currentAP);
			unitAPText.GetComponent<Text>().text = "Unit Action Points: " + currentAP;
			playerCPText.GetComponent<Text>().text = "Player Command Points: " + currentCP;
		}
		//If it does cut into CP, calculate how much, and then display it
		else{
			Debug.Log("AP Used cuts into CP, Current AP: " + currentAP + ", Current CP: " + currentCP + ", AP Used: " + aPUsed);
			currentCP = (currentAP + currentCP) - aPUsed;
			Debug.Log("New Current AP: " + currentAP + ", New Current CP: " + currentCP);
			currentAP = 0;
			Debug.Log("Current AP set to: " + currentAP);
		
			unitAPText.GetComponent<Text>().text = "Unit Action Points: " + currentAP;
			playerCPText.GetComponent<Text>().text = "Player Command Points: " + currentCP;
		}
	}

}
