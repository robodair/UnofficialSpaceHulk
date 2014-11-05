/* 
 * The InputOutput class handles graphic representation of the map and input from the GUI and mouse clicks
 * Created by Alisdair Robertson 9/9/2014
 * Version 5-11-14.0
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // For use of Lists Alisdair 3-10-14

public class InputOutput : MonoBehaviour {

	/// ============================
	/// Class Variable Declarations 
	/// ============================

		/// ++++++++++++++++
		/// Public Variables
		/// ++++++++++++++++

		// GAMEOBJECTS //
	public GameObject 
		// Map representation
	FloorPiecePrefab, 
	SpaceMarinePrefab, 
	GenestealerPrefab, 
	BlipPrefab, 
	OpenDoorPrefab, 
	ClosedDoorPrefab, 
	BlipDeploymentPiecePrefab, 
	escapeParticleSystem,
		// UI
	UICanvas,
	inGameMenuUI,
	endGameUI,
		//Facing Selection
	facingSelectionCanvas,
	currentFacingSelectionCanvas,
		// Hint Objects
	overwatchSprite,
	sustainedFireSprite,
	jammedUnitSprite,
		// Projectiles
	bulletPrefab, 
	explosionPrefab,
		// Blood
	sM_Blood_Particles,
	gS_Blood_Particles;

		// MESHES //
	public Mesh 
	genestealerMesh,
	spaceMarineMesh;


		//AUDIOCLIPS //
	public AudioClip 
	clickSound,
	click_error,
	game_start,
	gs_Death_Explosion,
	piece_Movement,
	sm_Death,
	sm_Escape,
	sm_Fire_1,
	sm_Fire_2,
	sm_Gunfire,
	sm_Jam,
	sm_Killed_GS,
	gs_Scream,
	doorClose,
	doorOpen,
	sm_Death_2;

		// LISTS //
	public List<GameObject> activePartSys = new List<GameObject>();

		// OTHER CLASSES //
	public Map mapClass; //Added 11/9/2014 Alisdair
	public Game gameClass; //Added 11/9/2014 Alisdair 
	public InputHandler inputHandlerController; // Rory Bolt 25.9.14

		// INSPECTOR CONTROLS //
		// Movement Speeds
	public float stepAmmount;
	public int rotateStepMultiplier;
		// Preferences
	public bool susFireOnlyOnSelection;
		// Placement
	public float unitElevation;
	public float floorReduction;
	public float fadeSpeed;
	public float lowEmissionRate;
	public float normalEmissionRate;

		// SCRIPT FEEDBACKS //
	public bool bulletsComplete = false;
	public bool selectFacingCanvasExists = false;
	public bool hologramsActive = false;

		/// +++++++++++++++++
		/// Private Variables
		/// +++++++++++++++++

		// GAMEOBJECTS //
	GameObject 
		// Buttons
	btnAttackGO, 
	btnShootGO, 
	btnMoveGO, 
	btnToggleDoorGO, 
	btnOverwatchGO, 
	btnRevealGO,
	btnEndTurnGO,
		// Menu Buttons
	btnExitGO,
	btnMenuGO,
	btnInfoGO,
		// End Game Buttons
	btnRestartGO,
		// Text
	unitAPText, 
	playerCPText,
		// Dice
	sMDie1, 
	sMDie2, 
	gSDie1, 
	gSDie2, 
	gSDie3,
		// UI
	uiCanvas,
	menuCanvas;

		// BUTTONS //
	Button 
		// Action Buttons
	btnAttack, 
	btnShoot, 
	btnMove, 
	btnToggleDoor, 
	btnOverwatch, 
	btnReveal,
		// Menu Buttons
	btnExit,
	btnMenu,
	btnInfo,
		// End Game Buttons
	btnRestart,
		// Turn Button
	btnEndTurn;

		// SELECTION //
		// Unit
	Unit selectedUnit;
		// Colors
	Color preSelectionColor;

		// LISTS //
		// Actions
	List<Action> showActionsList = new List<Action>();
		// Action Managers
	List<ActionManager> actionManagers = new List<ActionManager>();
		// Action Phases
	List<ShootPhase> shootPhaseList = new List<ShootPhase>();
	List <AttackPhase> attackPhaseList = new List<AttackPhase>();
		// Renderers
	List<Renderer> renderers = new List<Renderer>();
		// Holograms
	List<GameObject> hologramParticles = new List<GameObject>();
		// Sustained Fire Sprites
	List<GameObject> susFireSprites = new List<GameObject>();

		// ENUMERATED TYPES //
		// Phases
	enum ShootPhase{RotateTowards, CreateBullets, BulletsMoving, UnitDeath, RotateBack};
	enum AttackPhase{MoveTowards,  MoveBack, UnitDeath};

		// ACTION DISPLAY //
		// Movement
	float stepMoveAmmount;
	float stepRotateAmmount;
		// Action & Command Points
	int currentAP;
	int currentCP;
		// Processing
	bool isFirstLoopofAction = true;
	bool attackSuccessful = false;
	bool exeKilled = false;
		// Rotations
	Quaternion rotationBefore;
	Quaternion aimRot = Quaternion.identity;
		// Positions
	Vector3 exeInitPos = new Vector3();
	Vector3 exeUnitAttackPos = new Vector3();
		// Involuntary Reveal
	Action previousAction;
		// Pausing and playing
	bool letActionsPlay = true;
	bool soundsFirstPass = true;

	/// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	/// BEGIN METHODS
	/// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


	/// ======================
	/// Starting the instance
	/// ======================

	/// <summary>
	/// Start this instance of InputOutput and assing the varibles that are used to determine the move speed of the units
	/// </summary>
	public void Start(){
		stepMoveAmmount = stepAmmount;
		stepRotateAmmount = stepAmmount * rotateStepMultiplier;
	}


	/// ================
	/// Showing Actions
	/// ================

	/// <summary>
	/// Called once per frame, update is used to show any actions that need to be shown
	/// </summary>
	public void Update(){
		if (letActionsPlay){

			if(showActionsList.Count > 0){
				//if (Debug.isDebugBuild) Debug.Log ("ShowActionsList.Count != 0, incrementing an action");
				if (gameClass.gameState != Game.GameState.ShowAction){
					gameClass.changeGameState(Game.GameState.ShowAction);						//Change the state to showaction state
					}
				Action action = showActionsList[0];

				// if (Debug.isDebugBuild) Debug.Log ("The action is of type: " + action.actionType);

				exeInitPos = makePosition(action.executor.position, unitElevation);				// Store the original position of the executor

				if (isFirstLoopofAction){														// Display the dice roll on the first loop of the action
					resetDice();																// Clear the dice first in case the action doesn't have dice to display
					displayDice(action.diceRoll);
					preAction(action);															// Use the preAction method to display the changes to the hint sprites before the action is shown
				}

				// Make the action
				switch (action.actionType){

					/// ============================
					/// MOVE ACTION
					/// ============================
					case (Game.ActionType.Move):
						moveAction(action);
						break;

					/// =================
					/// OVERWATCH ACTION
					/// =================

					case (Game.ActionType.Overwatch):
						overwatchAction(action);
						break;

					///=============================
					/// ToggleDoor Action
					///=============================

					case (Game.ActionType.ToggleDoor):
						toggleDoorAction(action);
						break;

					///==================================================================
					/// ATTACK ACTION - FOR MELEE
					///==================================================================
					/// Attack will only be functional for one-way attacks (e.g GS attack SM)
					///++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

					case (Game.ActionType.Attack):

						attackAction(action);
						break;

					///============================================
					/// SHOOT ACTION - Ranged Attack (SM shoots GS)
					///============================================
					case (Game.ActionType.Shoot):
						shootAction(action);
						break;

					/// ===================================
					/// Involuntary Reveal Action
					/// ===================================

					case (Game.ActionType.InvoluntaryReveal):
						involuntaryRevealAction(action);																											
						break;																	// Call the involuntary reveal method

					default:
						if (Debug.isDebugBuild) Debug.LogWarning("Game to show an action now");
						break;
				}
			}
		}
	}

	/// <summary>
	/// Actions A move Action
	/// </summary>
	/// <param name="action">Action.</param>
	void moveAction(Action action){
		if (isFirstLoopofAction){
			deactivateHolograms();
			isFirstLoopofAction = false;
			action.executor.gameObject.audio.PlayOneShot(piece_Movement);
			return;															// Skip The first frame of a move action, ensures smooth movement (no jumps due to processing lag)
		}
		//if (Debug.isDebugBuild) Debug.Log("Update entered Move action sector of switch");
		
		// Create aim pos and rot
		Vector3 aimPos = makePosition(action.movePosition, unitElevation);
		//if (Debug.isDebugBuild) Debug.Log ("Aim position is " + aimPos);
		aimRot = makeRotation(makeFacing(action.moveFacing), action.executor.unitType);
		//if (Debug.isDebugBuild) Debug.Log ("Aim Rotation is: " + aimRot);
		
		// Make part of the movements
		action.executor.gameObject.transform.position = Vector3.MoveTowards(action.executor.gameObject.transform.position, aimPos, stepMoveAmmount*Time.deltaTime);
		action.executor.gameObject.transform.rotation = Quaternion.RotateTowards(action.executor.gameObject.transform.rotation, aimRot, stepRotateAmmount*Time.deltaTime);
		
		// Check to see if the unit is in the correct place and rotation, if so, finish the action and remove the action from the list
		// also update the unit AP and CP fields
		if (action.executor.gameObject.transform.position == aimPos){
			//if (Debug.isDebugBuild) Debug.Log("PositionEqual");
			if(action.executor.gameObject.transform.rotation.eulerAngles.y == aimRot.eulerAngles.y){
				finishAction(action);
				if(action.executor.position == gameClass.escapePosition)
					gameObject.audio.PlayOneShot(sm_Escape);
			}
			else{
				//if (Debug.isDebugBuild) Debug.Log ("Rotation not equal, Rotation aim is: " + aimRot.eulerAngles + "Current Rotation: " + action.executor.gameObject.transform.rotation.eulerAngles);
			}
		}
		else {
			//if (Debug.isDebugBuild) Debug.Log ("Position not Equal, Position target is: " + aimPos);
		}
	}
	
	/// <summary>
	/// Actions an Overwatch Action
	/// </summary>
	/// <param name="action">Action.</param>
	void overwatchAction(Action action){
		isFirstLoopofAction = false;
		// determine the position the sprite
		Vector3 spritePosition = action.executor.gameObject.transform.position;
		spritePosition.y += 1.5f;
		
		//instantiate the sprite at the position & give reference to the unit if the unit does not already have an overwatch sprite
		if (action.executor.overwatchSprite == null){
			//if (Debug.isDebugBuild) Debug.LogWarning("Creating a new Overwatch Sprite!");
			action.executor.overwatchSprite = (GameObject) Instantiate(overwatchSprite, spritePosition, Quaternion.identity);
		}
		
		finishAction(action);
	}
	
	/// <summary>
	/// Actions A toggle Door Action
	/// </summary>
	/// <param name="action">Action.</param>
	void toggleDoorAction(Action action){
		if(isFirstLoopofAction){
			isFirstLoopofAction = false;
			return;															// Skip The first frame of a action, ensures smooth movement (no jumps due to processing lag)
		}
		// Get the position that the door would be at based off the unit's position and rotation Alisdair 14-10-14
		//if (Debug.isDebugBuild) Debug.Log("Exe pos:" + action.executor.gameObject.transform.position);
		Vector2 doorMapPosition = new Vector2 (action.executor.gameObject.transform.position.x, action.executor.gameObject.transform.position.z);
		//if (Debug.isDebugBuild) Debug.Log ("converted to a pos in map: " + doorMapPosition);
		//if (Debug.isDebugBuild) Debug.LogError(action.executor.facing);
		switch (action.executor.facing){
		case (Game.Facing.North):
			doorMapPosition.y++;
			break;
			
		case (Game.Facing.East):
			doorMapPosition.x++;
			break;
			
		case (Game.Facing.South):
			doorMapPosition.y--;
			break;
			
		case (Game.Facing.West):;
			doorMapPosition.x--;
			break;
		}
		//if (Debug.isDebugBuild) Debug.Log ("Final Door Position: " + doorMapPosition);
		//if (Debug.isDebugBuild) Debug.Log ("Therefore door pos = " + makePosition(doorMapPosition, 2000));
		
		// Check if the position has a door, Get the door (unit), Toggle the door 
		Square mapSquareWithDoor = mapClass.getSquare(doorMapPosition);
		Unit doorToToggle;
		// Check that the door is there, then assign the variables for use in toggling
		//if (Debug.isDebugBuild) Debug.Log ("DOES POSITION HAVE DOOR?: " + mapClass.hasDoor(doorMapPosition));
		if (mapClass.hasDoor(doorMapPosition)){
			doorToToggle = mapClass.getOccupant(doorMapPosition);
			//target unit variables being assigned
			action.target = doorToToggle;
			
			//If the door is open (in the map class, which is up to date), make it an open door
			if (mapClass.isDoorOpen(doorMapPosition)){
				GameObject newDoor = (GameObject) Instantiate(OpenDoorPrefab, makePositionDoor(doorMapPosition, unitElevation, mapSquareWithDoor.door.facing, true), makeRotation(makeFacing(mapSquareWithDoor.door.facing), Game.EntityType.Door));
				Destroy(mapSquareWithDoor.door.gameObject);
				mapSquareWithDoor.door.gameObject = newDoor;
			}
			
			else { //if the door is closed in the map class, show this also
				GameObject newDoor = (GameObject) Instantiate(ClosedDoorPrefab, makePositionDoor(doorMapPosition, unitElevation, mapSquareWithDoor.door.facing, false), makeRotation(makeFacing(mapSquareWithDoor.door.facing), Game.EntityType.Door));
				Destroy(action.target.gameObject);
				mapSquareWithDoor.door.gameObject = newDoor;
			}
			// Play the correct door opening sound effect
			playDoorSound(mapSquareWithDoor.door.gameObject.audio, mapClass.isDoorOpen(doorMapPosition));

			//Finish the action and update the AP
			finishAction(action);
		}
		
	}
	
	/// <summary>
	/// Actions an Attack Action
	/// </summary>
	/// <param name="action">Action object</param>
	void attackAction(Action action){
		if (isFirstLoopofAction) {
			isFirstLoopofAction = false; 																			// Set that it is no longer the first loop
			renderers.Clear ();
			foreach (Unit un in action.destroyedUnits) {
				//if (Debug.isDebugBuild) Debug.Log ("There are " + action.destroyedUnits.Count + " units in the list of destroyed units");
				renderers.AddRange (un.gameObject.GetComponentsInChildren<Renderer> ());							// Get all the renderer gameobject components that need to be faded
				if (un.jammedUnitSprite != null) {
					renderers.Add (un.jammedUnitSprite.renderer);													// If there is a jam sprite for the unit, add it to be faded when the unit dies
				}
				if (un.overwatchSprite != null) {																	// If there is a jam sprite for the unit, add it to be faded when the unit dies
					renderers.Add (un.overwatchSprite.renderer);
				}
				if (un.sustainedFireSprite != null) {
					renderers.Add (un.sustainedFireSprite.renderer);												// If there is a jam sprite for the unit, add it to be faded when the unit dies
				}
				if (un.sustainedFireTargetSprite != null) {
					renderers.Add (un.sustainedFireTargetSprite.renderer);											// If there is a jam sprite for the unit, add it to be faded when the unit dies
				}

			}
			
			exeUnitAttackPos = Vector3.MoveTowards (action.executor.gameObject.transform.position, action.target.gameObject.transform.position, 0.7f); 		// Get the position the unit will be when the attack occurs
			
			attackPhaseList.Add (AttackPhase.MoveTowards); 															// Add the moving toward phase
			
			bool addEndPhases = true;																				// Bool set to determine if the end phases are needed or not
			if (action.destroyedUnits.Count > 0) { 																	// Determine if the attack was successful (for use when creating bullets)
				attackSuccessful = true;
				//if (Debug.isDebugBuild) Debug.LogWarning("Melee attack to be successful");
				attackPhaseList.Add (AttackPhase.UnitDeath); 														// If it is successful add the phase for unit death for the lineup
				//Check to see if the executor is the one that dies (if it is, do not add the move back and rotate back phases
				exeKilled = false;
				foreach (Unit unit in action.destroyedUnits) {
					if (unit == action.executor) {
						addEndPhases = false;
						exeKilled = true;
						break;
					}
				}
				
				
			}
			if (addEndPhases) {
				attackPhaseList.Add (AttackPhase.MoveBack); 															// Add the stage for moving back after the attack action
			}
			return;																									// Skip The first frame of a move action, ensures smooth movement (no jumps due to processing lag)
		}
		
		switch (attackPhaseList [0]) { 																				// Use a switch for actioning the phases
			
		case (AttackPhase.MoveTowards): 																		// Moving the executor towards the aim position
			action.executor.gameObject.transform.position = Vector3.MoveTowards (action.executor.gameObject.transform.position, exeUnitAttackPos, stepMoveAmmount * Time.deltaTime);
			
			if (action.executor.gameObject.transform.position == exeUnitAttackPos) {
				attackPhaseList.RemoveAt (0); 																	// Move to the next phase
			}	
			break;
			
		case(AttackPhase.UnitDeath): 
			if (soundsFirstPass){
				if(!exeKilled){
					playSmDeath(action.executor.gameObject.audio);
				}
				else if(exeKilled){
					action.executor.gameObject.audio.PlayOneShot(sm_Fire_2);
					//if(Debug.isDebugBuild) Debug.Log("SHOULD PLAY GS SCREAM");
					action.target.gameObject.audio.PlayOneShot(gs_Scream);
				}
				soundsFirstPass = false;
			}
			// Fade the gameobject out and then remove it
			Color color = Color.clear;
			foreach (Renderer rend in renderers) { 																// Decrease the alpha level on all of the child renderers (to fade out the gameobject)
				if (rend != null && rend.material.HasProperty("_Color")) {		
					color = rend.material.color;
					color.a -= fadeSpeed;
					rend.material.color = color;
				}
			}
			
			if (color.a <= 0.5f) { 																				// If the gameobject is now fully transparent, remove it.
				if (action.executor.sustainedFireSprite != null) {
					Destroy (action.executor.sustainedFireSprite);
				}
				if (action.executor.sustainedFireTargetSprite != null) {
					Destroy (action.executor.sustainedFireTargetSprite);
				}
				if (action.executor.overwatchSprite != null) {
					Destroy (action.executor.overwatchSprite);
				}
				if (action.target.sustainedFireSprite != null) {
					Destroy (action.target.sustainedFireSprite);
				}
				if (action.target.sustainedFireTargetSprite != null) {
					Destroy (action.target.sustainedFireTargetSprite);
				}
				if (action.target.overwatchSprite != null) {
					Destroy (action.target.overwatchSprite);
				}
				if (action.target.jammedUnitSprite != null) {
					Destroy (action.target.jammedUnitSprite);
				}
				foreach (Unit unit in action.destroyedUnits) {
					if (unit.unitType == Game.EntityType.GS){
						Instantiate(gS_Blood_Particles, unit.gameObject.transform.position, unit.gameObject.transform.rotation);
					}
					else if (unit.unitType == Game.EntityType.SM){
						Instantiate(sM_Blood_Particles, unit.gameObject.transform.position, unit.gameObject.transform.rotation);
					}
					Destroy (unit.gameObject);
				}
				attackPhaseList.RemoveAt (0); 																	// Move to the next phase
				if (exeKilled) {
					finishAction (action);
				}
			}
			break;
			
		case (AttackPhase.MoveBack):																			// Move the executor gameobject back to it's original position
			action.executor.gameObject.transform.position = Vector3.MoveTowards (action.executor.gameObject.transform.position, exeInitPos, stepMoveAmmount * Time.deltaTime);
			
			if (action.executor.gameObject.transform.position == exeInitPos) {
				attackPhaseList.RemoveAt (0); 																	// remove this phase
				finishAction (action);																			// End the action
			}
			break;
			
		}
	}
	
	/// <summary>
	/// Actions a Shoot action
	/// </summary>
	/// <param name="action">Action object</param>
	void shootAction(Action action){ 
		if (isFirstLoopofAction){
			isFirstLoopofAction = false;
			
			// ================================================================================== SHOOT ACTION ROTATION CALCULATIONS
			
			float offset = getBearing(action.executor.position, action.executor.facing, action.target.position);
			// if (Debug.isDebugBuild) Debug.LogWarning("Offset rotation IS: " + offset);
			rotationBefore = Quaternion.Euler(new Vector3(action.executor.gameObject.transform.rotation.eulerAngles.x, action.executor.gameObject.transform.rotation.eulerAngles.y, action.executor.gameObject.transform.rotation.eulerAngles.z)); 	// Store the initial rotation
			aimRot = new Quaternion();																				// Calculate the aim rotation
			aimRot = Quaternion.Euler(new Vector3 (rotationBefore.eulerAngles.x, rotationBefore.eulerAngles.y + offset, rotationBefore.eulerAngles.z));
			// if (Debug.isDebugBuild) Debug.LogWarning("Exe Rotation: " + action.executor.gameObject.transform.rotation.eulerAngles);
			// if (Debug.isDebugBuild) Debug.LogWarning("Beginning Rotation: " + rotationBefore.eulerAngles);
			// if (Debug.isDebugBuild) Debug.LogWarning("Aim Rotation: " + aimRot.eulerAngles);
			
			// ==================================================================================
			
			
			if (action.target.gameObject!= null){
				foreach(Renderer rend in action.target.gameObject.GetComponentsInChildren<Renderer>()){
					renderers.Add (rend);																				// Get all the renderer gameobject components that need to be faded
				}
			}
			
			
			isFirstLoopofAction = false; 																		// Set that it is no longer the first loop
			shootPhaseList.Add(ShootPhase.RotateTowards); 														// Add the rotation phase
			shootPhaseList.Add(ShootPhase.CreateBullets); 														// Add the phase for creating the bullets
			shootPhaseList.Add(ShootPhase.BulletsMoving); 														// Add the bullet moving phase
			
			if (action.destroyedUnits.Count > 0){ 																//Determine if the attack was successful (for use when creating bullets)
				attackSuccessful = true;
				//if (Debug.isDebugBuild) Debug.LogWarning("Shoot attack to be successful");
				shootPhaseList.Add(ShootPhase.UnitDeath); 														//If it is successful add the phase for unit death for the lineup
			}
			
			shootPhaseList.Add(ShootPhase.RotateBack); 															//Finally add the stage for rotating back after the shoot action
			return;																									// Set that it is no longer the first loop
		}
		
		switch (shootPhaseList[0]){ 																				// Use a switch for actioning the phases
			
		case (ShootPhase.RotateTowards): 																		// Rotating the SM to face the GS
			
			action.executor.gameObject.transform.rotation = Quaternion.RotateTowards(action.executor.gameObject.transform.rotation, aimRot, stepRotateAmmount*Time.deltaTime); // Increment the rotation
			
			if(action.executor.gameObject.transform.rotation.eulerAngles.y == aimRot.eulerAngles.y){
				if(!action.unitJams){
					if(action.target.unitType != Game.EntityType.Door){
						playSmBattleCry(action.executor.gameObject.audio);
					}
					else{
						action.executor.gameObject.audio.PlayOneShot(sm_Fire_1);
					}
				}
				shootPhaseList.RemoveAt(0); 															// Move to the next phase
			}
			break;
			
		case (ShootPhase.CreateBullets): 																		// Creating the bullets
			//if (Debug.isDebugBuild) Debug.LogWarning("Creating the bullets");
			
			Vector3 bulStart = makePosition(action.executor.position, unitElevation + 1.0f);							// Create the Vector3 positions for the bullets to start and end at
			Vector3 bulEnd = makePosition(action.target.position, unitElevation + 0.8f);
			
			if (!attackSuccessful){ 																			// If the attack is not successful make the bullets miss
				//if (Debug.isDebugBuild) Debug.Log("ATTACK NOT SUCCESSFUL, MAKING BULLETS MISS");
				int rand = Random.Range(1, 4);
				switch (rand){
				case 1: 																					// make the bullets miss above
					if (action.target.unitType == Game.EntityType.Door)											// If the unit is a door, make the miss higher
						bulEnd.y += 0.5f;
					bulEnd.y += 1;
					break;
				case 2: 																					// make the bullets miss below
					bulEnd.y -= 1;
					break;
				case 3: 																					// make the bullets miss to the left
					if (action.target.facing == Game.Facing.North || action.target.facing == Game.Facing.South)			
						bulEnd.x += 0.6f;
					else if (action.target.facing == Game.Facing.West || action.target.facing == Game.Facing.East)			
						bulEnd.z += 0.6f;
					else
						bulEnd.y -= 1;
					break;
				case 4: 																					// make the bullets miss to the right
					if (action.target.facing == Game.Facing.North || action.target.facing == Game.Facing.South)			
						bulEnd.x -= 0.6f;
					else if (action.target.facing == Game.Facing.West || action.target.facing == Game.Facing.East)			
						bulEnd.z -= 0.6f;
					else
						bulEnd.y -= 1;
					break; 
				}
			}
			
			createBullet(bulStart, bulEnd, attackSuccessful, 0.0f);												// Create a bullet
			createBullet(bulStart, bulEnd, attackSuccessful, 0.2f);												// Create a bullet
			createBullet(bulStart, bulEnd, attackSuccessful, 0.4f);												// Create a bullet
			createBullet(bulStart, bulEnd, attackSuccessful, 0.6f);												// Create a bullet
			createBullet(bulStart, bulEnd, attackSuccessful, 0.8f, true);										// Create a bullet, that changes the bullets complete variable when done
			action.executor.gameObject.audio.PlayOneShot(sm_Gunfire);											// Play the gunshots

			if(action.unitJams){																				// If the unit jammed in this action, display the jammed sprite
				showJam(action.executor);
				action.executor.gameObject.audio.PlayOneShot(sm_Jam);											// Play jam audio
			}
			
			shootPhaseList.RemoveAt(0); 																		// Move to the next phase
			break;
			
		case (ShootPhase.BulletsMoving): 																		// Waiting for the bullets to finish moving
			if (bulletsComplete){
				shootPhaseList.RemoveAt(0); 																	// Move to the next phase if all of the bullets have finished
				bulletsComplete = false; 																		// Reset the variable ready for the next shoot action
			}
			break;
			
		case(ShootPhase.UnitDeath): 																			// Fade the GS gameobject out and then remove it
			float alphaLevel = 255;
			
			if(action.executor.sustainedFireSprite != null)														// Add the renderers of the sustained fire sprites to be faded if they exist
				renderers.AddRange(action.executor.sustainedFireSprite.GetComponentsInChildren<Renderer>());
			if (action.executor.sustainedFireTargetSprite != null)
				renderers.AddRange(action.executor.sustainedFireTargetSprite.GetComponentsInChildren<Renderer>());
			
			if (renderers.Count > 0){
				Color color;
				foreach (Renderer rend in renderers){ 																// Decrease the alpha level on all of the child renderers (to fade out the gameobject)
					if (rend != null && rend.material.HasProperty("_Color")){
						color = rend.material.color;
						color.a -= fadeSpeed;
						alphaLevel = color.a;
						rend.material.color = color;
					}
				}
				
				if (alphaLevel <= 0.5f) { 																			// If the gameobject is now fully transparent, remove it.
					if(action.target.unitType == Game.EntityType.GS){
						Instantiate(gS_Blood_Particles, action.target.gameObject.transform.position, action.target.gameObject.transform.rotation);
					}

					if(action.executor.sustainedFireTargetSprite != null){
						susFireSprites.Remove(action.executor.sustainedFireTargetSprite);
						Destroy (action.executor.sustainedFireTargetSprite);
					}		

					Destroy (action.target.gameObject);
					shootPhaseList.RemoveAt(0); 																	// Move to the next phase
				}
			}
			else {
				action.executor.gameObject.audio.PlayOneShot(sm_Killed_GS);										// Play the successful kill sound
				shootPhaseList.RemoveAt(0); 																	// Move to the next phase
			}
			
			break;
			
		case(ShootPhase.RotateBack): 																			// Rotate the unit back to the position that it originally was at
			// if (Debug.isDebugBuild) Debug.LogWarning("RotateBack Phase");
			action.executor.gameObject.transform.rotation = Quaternion.RotateTowards(action.executor.gameObject.transform.rotation, rotationBefore, stepRotateAmmount*Time.deltaTime); // Increment the rotation
			
			if(action.executor.gameObject.transform.rotation.eulerAngles.y == rotationBefore.eulerAngles.y){ 											// If the rotation back has completed, end the action
				finishAction(action);
			}
			
			break;
		}
	}
	
	void involuntaryRevealAction(Action action){
		letActionsPlay = false;
		finishAction(action);
		GameObject.Find("GameController").GetComponent<RevealManager>().involuntaryReveal(action.executor.position, actionManagers[0], previousAction.prevLoS); 	
		actionManagers.RemoveAt(0);
	}


	/// =======================================
	/// Methods for Instantiating the map and UI
	/// =======================================

	/// <summary>
	/// Create the Game UI in the scene
	/// </summary>
	public void instantiateUI(){ //Method Added by Alisdair Robertson 11/9/14
		/*
		 * This method creates the UI and then links the buttons to the code here
		 */ 

		uiCanvas = (GameObject) Instantiate (UICanvas);//Instantiate the canvas

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

		//Assign the Menu Buttons
		btnExitGO = GameObject.Find ("BtnExit");
		btnExit = btnExitGO.GetComponent<Button>();
		btnExit.onClick.AddListener(() => {btnExitClicked();});

		btnMenuGO = GameObject.Find ("BtnMenu");
		btnMenu = btnMenuGO.GetComponent<Button>();
		btnMenu.onClick.AddListener(() => {openMenu();});

		btnInfoGO = GameObject.Find ("BtnShowInfo");
		btnInfo = btnInfoGO.GetComponent<Button>();
		btnInfo.onClick.AddListener(() => {btnInfoClicked();});

		//assign the text elements
		unitAPText = GameObject.Find("APText");
		playerCPText = GameObject.Find ("CPText");
		currentAP = 0;
		if(selectedUnit != null) currentAP = selectedUnit.AP;
		currentCP = gameClass.remainingCP;//Changed to reference game class Alisdair 11-10-14
		//if (Debug.isDebugBuild) Debug.Log("UpdateCPAP InstantiateUI with AP: " + 0);
		updateCPAP(0);

		//assign the dice text elements
		sMDie1 = GameObject.Find("SMDie1");
		sMDie2 = GameObject.Find("SMDie2");
		gSDie1 = GameObject.Find("GSDie1");
		gSDie2 = GameObject.Find("GSDie2");
		gSDie3 = GameObject.Find("GSDie3");
		//Set the dice to show X
		resetDice();
		

		//Assign the method to call from the end turn button added 2-10-14 Alisdair
		btnEndTurnGO = GameObject.Find ("BtnTurn");
		btnEndTurn = btnEndTurnGO.GetComponent<Button>();
		btnEndTurn.onClick.AddListener(() => {btnEndTurnClicked();});


		// Create the Menu Canvas
		menuCanvas = (GameObject) Instantiate (inGameMenuUI); // instantiate the menu
		GameObject.Find ("QuitGameButton").GetComponent<Button>().onClick.AddListener(() => {btnExitClicked();});		// Assign the exit button
		GameObject.Find ("ResumeGameButton").GetComponent<Button>().onClick.AddListener(() => {closeMenu();});			// Assign the resume button
		GameObject.Find ("TestNetworkButton").GetComponent<Button>().onClick.AddListener(() => {testNetwork();});		// Assign the network button to call the test network method
		Toggle susFireToggle = GameObject.Find ("SusFireDisplayToggle").GetComponent<Toggle>();
		susFireToggle.onValueChanged.AddListener( delegate{susFireOnlyOnSelectedUnit(susFireToggle.isOn);});		// Assign the toggle
		menuCanvas.SetActive(false);							// Hide the menu canvas
	}

	/// <summary>
	/// Opens the menu.
	/// </summary>
	public void openMenu(){
		//if(Debug.isDebugBuild) Debug.Log("Opening Menu");
		menuCanvas.SetActive(true);
		uiCanvas.SetActive(false);
	}

	/// <summary>
	/// Closes the menu.
	/// </summary>
	public void closeMenu(){
		//if(Debug.isDebugBuild) Debug.Log("Closing Menu");
		uiCanvas.SetActive(true);
		menuCanvas.SetActive(false);
	}

	public void testNetwork(){
		if(Debug.isDebugBuild) Debug.Log("TESTING NETWORK");
	}

	/// <summary>
	/// Generates the game map as represented in the Map class.
	/// </summary>
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
			float xPos = positionV2.x;
			float zPos = positionV2.y;

			GameObject floorPiece = (GameObject) Instantiate(FloorPiecePrefab, new Vector3(xPos, (unitElevation - floorReduction), zPos), Quaternion.identity); //Create the game object in the scene
			square.model = floorPiece; //Pass reference to the gameobject back to the square

			//Added Alisdair 11/9/2014 This are for passing the unit reference back to the square (if needed)
			GameObject doorPiece;

			//Go on to create units or doors - 11/9/14 Alisdair
			//if the square has a unit or door create that unit or door on it
			if (square.isOccupied){

				placeUnit (square.occupant);
			}
					
			//if the square has a door and it's open create it
			if (square.hasDoor && square.doorIsOpen){
				doorPiece = (GameObject) Instantiate(OpenDoorPrefab, makePositionDoor(square.position, unitElevation + 1, square.door.facing, true), makeRotation(makeFacing(square.door.facing), Game.EntityType.Door)); //Create the open door object above the floor object
				square.door.gameObject = doorPiece; //Pass reference to the gameobject back to the square
			}

		}

		Instantiate (escapeParticleSystem, makePosition(gameClass.escapePosition, unitElevation-floorReduction), escapeParticleSystem.transform.rotation);


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
			float xPos = adjPos.x;
			float zPos = adjPos.y;

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
					if (Debug.isDebugBuild) Debug.LogError("No valid relative position assigned to deployment piece adjacent to xPos: " + xPos + " zPos: " + zPos);
					break;
			}

			Quaternion depAreaFacing = Quaternion.Euler(0,0,0);
			//Added passing of reference to deployment area gameobjects back to the game class. Alisdair 26-9-2014
			depArea.model = (GameObject) Instantiate(BlipDeploymentPiecePrefab, new Vector3(xPos, (unitElevation - floorReduction), zPos), depAreaFacing); //Create the game object in the scene
			Vector3 position = depArea.model.transform.GetChild(0).transform.position;
			Vector3 newPosition = new Vector3 (position.x, 1.6f, position.z);
			depArea.model.transform.GetChild(0).transform.position = newPosition;

		}
		// Fill the list of hologram particles
		foreach (GameObject holo in FindObjectsOfType<GameObject>()) {
			if (holo.layer == 12) {
				hologramParticles.Add(holo);
			}
		}

		//Play the game start sound
		gameClass.audio.PlayOneShot(game_start);

	}


	/// ==============================================
	/// Acceptor methods for Showing action sequences
	/// ==============================================

	/// <summary>
	/// Adds the given list of action objects to the list to be shown
	/// </summary>
	/// <param name="actions">Action object Array.</param>
	//Recieve the array of actions to perform Alisdair
	public void showActionSequence(Action[] actions){
		//if (Debug.isDebugBuild) Debug.Log ("Showing an Action Sequence of length: " + actions.Length);
		updateGUIActions(); 																						//Disable the GUI Actions Alisdair 13-10-14
		showActionsList.AddRange(actions);
	}

	/// <summary>
	/// Adds the given list of action objects to the list to be shown and stores the action manager for use when the Involuntary reveal action is reached
	/// This is for dealing with Overwatch
	/// </summary>
	/// <param name="actions">Action object Array.</param>
	/// <param name="actionManager">ActionManager.</param>
	public void showActionSequence(Action[] actions, ActionManager actionManager){
		//if (Debug.isDebugBuild) Debug.Log ("Showing an Action Sequence of length: " + actions.Length);
		actionManagers.Add(actionManager);
		updateGUIActions(); 																						//Disable the GUI actions Alisdair 13-10-14
		showActionsList.AddRange(actions);
	}


	/// ==========================
	/// Selection and Deselection
	/// ==========================

	/// <summary>
	/// Shows the selection of the gameobject assigned to the unit given and adjusts the action buttons to reflect the selected unit
	/// </summary>
	/// <param name="unit">Unit.</param>
	/// <param name="actions">Actions.</param>
	public void selectUnit (Unit unit, Game.ActionType[] actions){ //Filled by Alisdair 11/9/2014
		deselect ();
		if (unit != null){																										// Check first to see that we have been passed a valid unit
			selectedUnit = unit;
			selectedUnit.gameObject.collider.enabled = false;																	// Disable the collider so that clicking on the square under the unity is easy
			setDoorCollidersEnabled(false);																						// Disable the door colliders so that move actions are easier
			// if (Debug.isDebugBuild) Debug.Log("Unit selected");
			if (selectedUnit.unitType == Game.EntityType.GS){
				particleToggle(selectedUnit.gameObject.GetComponentsInChildren<ParticleSystem>()[0], true);						// Turn the particle emittor on
			}
			else{
				particleToggle(selectedUnit.gameObject.GetComponent<ParticleSystem>(), true);									// Turn the particle emittor on
			}
			//update the GUI actionst 
			updateGUIActions(actions);

			//get and show the AP and CP for the unit
			currentAP = unit.AP;
			currentCP = gameClass.remainingCP;
			//if (Debug.isDebugBuild) Debug.Log("UpdateCPAP SELECTUNIT with AP: " + 0);
			updateCPAP(0);


			// Show the sustained fire sprites if the unit has any
			if(susFireOnlyOnSelection){
				showSusFire(unit);
			}
		}
	}

	/// <summary>
	/// Deselect the selected unit and disable the Action Buttons
	/// </summary>
	public void deselect(){ //Filled by Alisdair 11/9/2014
		/*
		 * This method removes the mesh renderer tint on the selected unit
		 */
		//set the render colour on the selected object back to what it was before selection
		if (selectedUnit!= null && selectedUnit.gameObject != null) {
			// Hide the sustained fire sprites for the unit
			if(susFireOnlyOnSelection){
				removeSusFire(selectedUnit);
			}

			selectedUnit.gameObject.collider.enabled = true;																	// Renable the collider so that the unit can be clicked on again 

			if (selectedUnit.unitType == Game.EntityType.GS){
				particleToggle(selectedUnit.gameObject.GetComponentsInChildren<ParticleSystem>()[0], false);						// Turn the particle emittor off
			}
			else{
				particleToggle(selectedUnit.gameObject.GetComponent<ParticleSystem>(), false);										// Turn the particle emittor off
			}

			selectedUnit = null;

			//set the gui to show no actions & set AP to 0
			updateGUIActions();
			//if (Debug.isDebugBuild) Debug.Log("UpdateCPAP DESELECT with AP: " + 0);
			currentAP = 0;
			updateCPAP(0);
		} 
		else {
			// if (Debug.isDebugBuild) Debug.Log("There is not a unit currently selected.");
		}
	}

	/// =====================================
	/// Methods for adding and removing units
	/// =====================================

	/// <summary>
	/// Show the deployment of units onto the map
	/// </summary>
	/// <param name="units">Units to show deployment of.</param>
	/// <param name="positions">Vector 2 Positions they must be deployed ay.</param>
	public void showDeployment(Unit[] units, Vector2[] positions){
		if (Debug.isDebugBuild) Debug.LogError("ShowDeployment method INCOMPLETE.");
	}

	/// <summary>
	/// Places the gameobject for the given unit in the correct position
	/// </summary>
	/// <param name="unit">Unit.</param>
	public void placeUnit(Unit unit){ //Added Gameobject Return 22/9/14 Alisdair
		//if (Debug.isDebugBuild) Debug.LogError("placeUnit method partially complete. Refer Alisdair.");

		//Check to see if the unit is being placed in a deployment area
		if (unit.position.x < 0){ // if the unit is being placed in a deployment area
			DeploymentArea depArea = mapClass.otherAreas[(int)(-1-unit.position.x)];
			Vector2 adjPos = depArea.adjacentPosition; //get position of adjecent piece
			
			//Converting Vector2 to Vector3
			//Vector2 y = Vector3 z (North/South)
			//Vector2 x = Vector3 x (East/West)
			//Vector3 y is vertical (leave at constant value)
			float xPos = adjPos.x;
			float zPos = adjPos.y;

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
					if (Debug.isDebugBuild) Debug.LogError("No valid relative position assigned to deployment piece adjacent to xPos: " + xPos + " zPos: " + zPos);
					break;
			}

			switch (unit.unitType){
					
				case Game.EntityType.Blip:
					unit.gameObject = (GameObject) Instantiate(BlipPrefab, new Vector3(xPos, unitElevation, zPos), makeRotation(makeFacing(unit.facing), Game.EntityType.Blip)); //Create the blip object

					refreshBlipCounts();
					break;
				default:
					if (Debug.isDebugBuild) Debug.LogError("There was not a valid unit to place into a deployment area.");
					break;
			}
		}
		else{ // If the unit is not in a deployment area, place as normal
			//Instantiate the unit at the position and pass a reference back to the unit class
			switch (unit.unitType){
				
				case Game.EntityType.Blip:
					unit.gameObject = (GameObject) Instantiate(BlipPrefab, makePosition(unit.position, unitElevation), makeRotation(makeFacing(unit.facing), Game.EntityType.Door)); //Create the blip object above the floor object
						
					break;
					
				case Game.EntityType.Door:
					unit.gameObject = (GameObject) Instantiate(ClosedDoorPrefab, makePositionDoor(unit.position, unitElevation, unit.facing, false), makeRotation(makeFacing(unit.facing), Game.EntityType.Door)); //Create the closed door object above the floor object
					
					mapClass.getSquare(unit.position).door.gameObject = unit.gameObject; //Pass reference to the gameobject back to the square
					break;
					
				case Game.EntityType.GS:
					unit.gameObject = (GameObject) Instantiate(GenestealerPrefab, makePosition(unit.position, unitElevation), makeRotation(makeFacing(unit.facing), Game.EntityType.GS)); //Create the blip object above the floor object
					break;
					
				case Game.EntityType.SM:
					unit.gameObject = (GameObject) Instantiate(SpaceMarinePrefab, makePosition(unit.position, unitElevation), makeRotation(makeFacing(unit.facing), Game.EntityType.SM)); //Create the blip object above the floor object
					
					break;
				default:
					if (Debug.isDebugBuild) Debug.LogError("There was not a valid unit to place");
					break;
			}
		}

		
	}

	/// <summary>
	/// Removes the gameobject of the unit at the specified position
	/// </summary>
	/// <param name="position">Position of the object to remove.</param>
	public void removeUnit(Vector2 position){
		if (mapClass.getSquare(position).isOccupied){
			Destroy(mapClass.getSquare(position).occupant.gameObject);
		}
	}

	/// ==================
	/// Resetting The Map
	/// ==================

	/// <summary>
	/// Resets the map to the representation held in the game class by removing all the gameobjects that are used to represent the map
	/// and then calling generateMap
	/// </summary>
	public void resetMap(){
		if (Debug.isDebugBuild) Debug.LogWarning("Reset Map Called");
		//Switched to iterating through a list of all the gameobjects, 
		//This way it doesn't matter if references in the map class are incorrect - Alisdair 25/9/14
		//http://answers.unity3d.com/questions/297171/find-all-objects-in-a-scene.html

		GameObject[] gameObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]; //Get an array of all the gameobjects

		foreach(GameObject gameObject in gameObjects)
		{
			if (gameObject.layer == 10){
				Destroy(gameObject); //If the gameobject is part of the map representation layer, destroy it. (otherwise leave it)
			}
		}

		//Alisdair 11/Sept/2014
		generateMap (); //Rerun generate map so that it matches the map class again

		if (gameClass.unitSelected){ //Reselecting unit after the map has been regenerated.
			gameClass.selectUnit(gameClass.selectedUnit.gameObject);
		}
	}


	/// ===========================
	/// Methods for Action Buttons
	/// ===========================

	/// <summary>
	/// Disables all the GUI action buttons.
	/// </summary>
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

	/// <summary>
	/// Updates the GUI Action buttons to reflect the commands that are allowed for the specific unit
	/// </summary>
	/// <param name="actions">List of ActionTypes that are the butons to enable.</param>
	void updateGUIActions(Game.ActionType[] actions){
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
				if (Debug.isDebugBuild) Debug.LogError("There was not an ActionType at position: " + i + " in the actions Array. updateGUIActions(Game.ActionType[] actions)");
				break;
			}
		}
	}

	/// <summary>
	/// Called when the Attack Button is clicked
	/// </summary>
	public void btnAttackClicked(){ //Added By Alisdair 14/9/14
		gameClass.audio.PlayOneShot (clickSound); //Sound added 29.10.14 RB
		inputHandlerController.attack ();
	}

	/// <summary>
	/// Called when the Move Button is clicked
	/// </summary>
	public void btnMoveClicked(){ //Added By Alisdair 14/9/14
		gameClass.audio.PlayOneShot (clickSound); //Sound added 29.10.14 RB
		inputHandlerController.keepHologram = false;	// Allow hologram creation
		//if(Debug.isDebugBuild)Debug.Log ("DISABLED HOLOGRAM KEEPING BECAUSE MOVE CLICKED");
		inputHandlerController.movement ();//RB 18/9/14
	}

	/// <summary>
	/// Called when the Overwatch Button is clicked
	/// </summary>
	public void btnOverwatchClicked(){ //Added By Alisdair 14/9/14
		gameClass.audio.PlayOneShot (clickSound); //Sound added 29.10.14 RB
		inputHandlerController.overwatchClicked();
	}

	/// <summary>
	/// Called when the Reveal Button is clicked
	/// </summary>
	public void btnRevealClicked(){ //Added By Alisdair 14/9/14
		/*
		 * This method needs to pass the button click back to the Game class so that action can be taken
		 */ 
		gameClass.audio.PlayOneShot (clickSound); //Sound added 29.10.14 RB
		if (Debug.isDebugBuild) Debug.LogWarning ("Reveal Button Clicked, this method is INCOMPLETE. Refer Alisdair");
	}

	/// <summary>
	/// Called when the Shoot Button is clicked
	/// </summary>
	public void btnShootClicked(){ //Added By Alisdair 14/9/14
		/*
		 * This method needs to pass the button click back to the Game class so that action can be taken
		 */ 
		gameClass.audio.PlayOneShot (clickSound); //Sound added 29.10.14 RB
		setDoorCollidersEnabled(true);											//Set the door collider so that they can be shot at if needed
		inputHandlerController.shoot ();
	}

	/// <summary>
	/// Called when the Toggle Door Button is clicked
	/// </summary>
	public void btnToggleDoorClicked(){ //Added By Alisdair 14/9/14
		/*
		 * This method needs to pass the button click back to the Game class so that action can be taken
		 */ 
		gameClass.audio.PlayOneShot (clickSound); //Sound added 29.10.14 RB
		inputHandlerController.toggleDoor ();
	}


	/// ================================
	/// Facing Selection Canvas Creation
	/// ================================

	/// <summary>
	/// Instantiates the facing selection canvas at a specified position (through the override method, this method is a shortcut for when all buttons are enabled)
	/// </summary>
	/// <param name="position">Position.</param>
	public void instantiateFacingSelection(Vector2 position){
		instantiateFacingSelection(position, true, true, true, true);
	}

	/// <summary>
	/// Instantiates the facing selection canvas and allows choosing of which buttons are active
	/// </summary>
	/// <param name="position">Vector 2 Position of the canvas.</param>
	/// <param name="north">If set to <c>true</c> north is active.</param>
	/// <param name="east">If set to <c>true</c> east is active.</param>
	/// <param name="south">If set to <c>true</c> south is active.</param>
	/// <param name="west">If set to <c>true</c> west is active.</param>
	public void instantiateFacingSelection(Vector2 position, bool north, bool east, bool south, bool west){
		selectFacingCanvasExists = true;
		//Create the canvas at the position
		currentFacingSelectionCanvas = (GameObject) Instantiate (facingSelectionCanvas, makePosition(position, 2), Quaternion.Euler (90, 0, 0));
		
		//Assign methods to the buttons
		foreach(Transform child in currentFacingSelectionCanvas.transform){
			switch(child.gameObject.name){
				case ("BtnNorth"):
					Button btnNorth = child.gameObject.GetComponent<Button>();
					btnNorth.onClick.AddListener(() => {btnFaceNorth();});
					btnNorth.interactable = north;
					break;
				case ("BtnEast"):
					Button btnEast = child.gameObject.GetComponent<Button>();
					btnEast.onClick.AddListener(() => {btnFaceEast();});
					btnEast.interactable = east;
					break;
				case ("BtnSouth"):
					Button btnSouth = child.gameObject.GetComponent<Button>();
					btnSouth.onClick.AddListener(() => {btnFaceSouth();});
					btnSouth.interactable = south;
					break;
				case ("BtnWest"):
					Button btnWest = child.gameObject.GetComponent<Button>();
					btnWest.onClick.AddListener(() => {btnFaceWest();});
					btnWest.interactable = west;
					break;
				default:
					if(Debug.isDebugBuild) Debug.LogError("A facing selection button was not correctly named");
					break;
			}
		}

	}

	/// ===========================================
	/// Methods for Facing Selection Canvas buttons
	/// ===========================================

	/// <summary>
	/// Called when the facing selection canvas north button is pressed
	/// </summary>
	public void btnFaceNorth(){
		gameClass.audio.PlayOneShot (clickSound); //Sound added 29.10.14 RB
		if(gameClass.gameState == Game.GameState.Reveal)
		{
			inputHandlerController.revealOrientationClicked(Game.Facing.North);
		}
		else
		{
			inputHandlerController.orientationClicked (Game.Facing.North);//RB 18.9.14
		}
		Destroy(currentFacingSelectionCanvas);
		selectFacingCanvasExists = false;
	}

	/// <summary>
	/// Called when the facing selection canvas east button is pressed
	/// </summary>
	public void btnFaceEast(){
		gameClass.audio.PlayOneShot (clickSound); //Sound added 29.10.14 RB
		if(gameClass.gameState == Game.GameState.Reveal)
		{
			inputHandlerController.revealOrientationClicked(Game.Facing.East);
		}
		else
		{
			inputHandlerController.orientationClicked (Game.Facing.East);//RB 18.9.14
		}
		Destroy(currentFacingSelectionCanvas);
		selectFacingCanvasExists = false;

	}

	/// <summary>
	/// Called when the facing selection canvas South button is pressed
	/// </summary>
	public void btnFaceSouth(){
		gameClass.audio.PlayOneShot (clickSound); //Sound added 29.10.14 RB
		if(gameClass.gameState == Game.GameState.Reveal)
		{
			inputHandlerController.revealOrientationClicked(Game.Facing.South);
		}
		else
		{
			inputHandlerController.orientationClicked (Game.Facing.South);//RB 18.9.14
		}
		Destroy(currentFacingSelectionCanvas);
		selectFacingCanvasExists = false;
		
	}

	/// <summary>
	/// Called when the facing selection canvas west button is pressed
	/// </summary>
	public void btnFaceWest(){
		gameClass.audio.PlayOneShot (clickSound); //Sound added 29.10.14 RB
		if(gameClass.gameState == Game.GameState.Reveal)
		{
			inputHandlerController.revealOrientationClicked(Game.Facing.West);
		}
		else
		{
			inputHandlerController.orientationClicked (Game.Facing.West);//RB 18.9.14
		}
		Destroy(currentFacingSelectionCanvas);
		selectFacingCanvasExists = false;
		
	}

	/// <summary>
	/// Converts the position given from a vector 2 to a vector 3 for the units, at the given elevation
	/// </summary>
	/// <returns>The Vector 3 position.</returns>
	/// <param name="position">Vector 2 Position</param>
	/// <param name="elevation">Elevation</param>
	Vector3 makePosition(Vector2 position, float elevation){
		Vector3 v3 = new Vector3 (position.x, elevation, position.y); 
		return v3;
	}

	/// <summary>
	/// Converts the position given from a vector 2 to a vector 3 for a open or closed door, at the given elevation and facing
	/// </summary>
	/// <returns>The Vector 3 door position</returns>
	/// <param name="position">Vector 2 Position.</param>
	/// <param name="elevation">Elevation.</param>
	/// <param name="facing">Facing of the door.</param>
	/// <param name="open">If set to <c>true</c> the door is open. (affects the offset perpendicular to the facing)</param>
	Vector3 makePositionDoor(Vector2 position, float elevation, Game.Facing facing, bool open){
		float offset = 0.0f;
		if (open){
			offset = -0.75f;
		}
		elevation++;
		Vector3 newPosition;

			if (facing == Game.Facing.East || facing == Game.Facing.West){
				newPosition = makePosition(position, elevation);
				newPosition.z -= offset;
				return newPosition;
			}

			else {
				newPosition = makePosition(position, elevation);
				newPosition.x += offset;
				return newPosition;
			}
	}


	/// <summary>
	/// Makes a Quaternion facing for the units from the Game class enumerated Type
	/// </summary>
	/// <returns>The Quaternion facing.</returns>
	/// <param name="facingEnum">Facing Enumarated Type.</param>
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
				if (Debug.isDebugBuild) Debug.LogError("Unable to determine facing set to Quaternion.identity.");
				return Quaternion.identity;
				
			
		}

	}

	/// <summary>
	/// Change the turn when the end turn button is clicked by the player.
	/// (Playerturn protection not needed here, as the button will not be active at all if it is not the client player's turn.
	/// </summary>
	void btnEndTurnClicked(){
		gameClass.audio.PlayOneShot (clickSound); //Sound added 29.10.14 RB
		switch (gameClass.playerTurn){
			case (Game.PlayerType.GS):
				gameClass.setTurn(Game.PlayerType.SM);
				break;
			case (Game.PlayerType.SM):
				gameClass.setTurn(Game.PlayerType.GS);
				break;
			default:
				if (Debug.isDebugBuild) Debug.LogError("Houston we have a problem. No player turn is assigned.");
				gameClass.setTurn(Game.PlayerType.GS);
				break;
		}
	}

	/// <summary>
	/// Make the end turn button interactable or not based on the game state
	/// </summary>
	public void defineEndTurnBtn(){
		//Check to see if the End turn button should be enabled or not, and assign the required state
		if(gameClass.gameState == Game.GameState.Inactive||
		   gameClass.gameState == Game.GameState.InactiveSelected){					// If the game is in a state where the turn can end
			if(gameClass.playerTurn == gameClass.thisPlayer){						// And if it is this player's turn, enable the end turn button
				btnEndTurn.interactable = true;
			}

		}
		else {																		// In all other circumstances, disable the end turn button
			btnEndTurn.interactable = false;
		}
	}

	/// <summary>
	/// Adjusts the rotation by the offset that has been specified for each entity type
	/// This is for models imported from 3ds Max, which have different axis values from unity
	/// </summary>
	/// <returns>The Quaternion rotation.</returns>
	/// <param name="reference">Reference generated by makeFacing.</param>
	/// <param name="type">the EntityType of the unit the rotation is for.</param>
	Quaternion makeRotation(Quaternion reference, Game.EntityType type){

		Quaternion returnQuaternion;

		switch (type){
			case (Game.EntityType.SM):
				returnQuaternion = Quaternion.Euler(reference.eulerAngles.x + 270, reference.eulerAngles.y - 90, reference.eulerAngles.z);
				return returnQuaternion;

			case (Game.EntityType.Door): // Door case added Alisdair 14-10-14
				returnQuaternion = Quaternion.Euler(reference.eulerAngles.x + 90, reference.eulerAngles.y, reference.eulerAngles.z);
				return returnQuaternion;
			case (Game.EntityType.GS):
				returnQuaternion = Quaternion.Euler(reference.eulerAngles.x, reference.eulerAngles.y + 180, reference.eulerAngles.z);
			return returnQuaternion;
			default:
				returnQuaternion = reference;
				return returnQuaternion;
		}
	}

	/// <summary>
	/// Removes overwatch sprite from the units in the list. Alisdair 5-10-14
	/// </summary>
	/// <param name="units">Units.</param>
	void removeOverwatch(List <Unit> units){
		//if (Debug.isDebugBuild) Debug.LogWarning ("There are: " + units.Count + " units in the list of units losing overwatch");
		foreach (Unit unit in units){
			if (unit.overwatchSprite != null)
				Destroy(unit.overwatchSprite);
		}
	}

	/// <summary>
	/// Removes overwatch sprite from all units.
	/// </summary>
	public void removeOverwatch(){
		foreach(GameObject obj in GameObject.FindGameObjectsWithTag("overWatchSprite")){
			Destroy(obj);
		}
	}

	/// <summary>
	/// Removes the sustained fire sprites from the units in the list
	/// </summary>
	/// <param name="units">Units.</param>
	void removeSusFire(List <Unit> units){
		//if (Debug.isDebugBuild) Debug.LogWarning ("There are: " + units.Count + " units in the list of units losing sustained fire");
		foreach (Unit unit in units){
			if (unit.sustainedFireSprite != null){
				susFireSprites.Remove(unit.sustainedFireSprite);
				susFireSprites.Remove(unit.sustainedFireTargetSprite);
				Destroy(unit.sustainedFireSprite);
				Destroy (unit.sustainedFireTargetSprite);
			}
		}
	}

	/// <summary>
	/// Hides the sustained fire sprites on the specified unit
	/// </summary>
	/// <param name="unit">Unit.</param>
	void removeSusFire(Unit unit){
		//if (Debug.isDebugBuild) Debug.Log ("Has Sustained Fire? " + unit.hasSustainedFire);
		if (unit.hasSustainedFire && unit.sustainedFireSprite != null){
			unit.sustainedFireSprite.gameObject.SetActive(false);
			unit.sustainedFireTargetSprite.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Updates the Action and Command point display by the ammount of AP and CP that is used.
	/// </summary>
	/// <param name="aPUsed">A P used.</param>
	void updateCPAP(int aPUsed){
		if(gameClass.playerTurn == gameClass.thisPlayer){												// Check whether there should be any display of point changes to the curent player
			//if (Debug.isDebugBuild) Debug.Log("begin Update CPAP, AP Used: " + aPUsed);
			// if it does not cut into CP, just display it
			if (aPUsed < currentAP || aPUsed == currentAP || gameClass.playerTurn == Game.PlayerType.GS){ // Display only AP deduction if the player is genestealer
				//if (Debug.isDebugBuild) Debug.Log(" aPUsed <= currentAP; Current AP: " + currentAP + " AP Used: " + aPUsed);
				currentAP = currentAP - aPUsed;
				//if (Debug.isDebugBuild) Debug.Log("New Current AP: " + currentAP);
				unitAPText.GetComponent<Text>().text = "Unit Action Points: " + currentAP;
				playerCPText.GetComponent<Text>().text = "Player Command Points: " + currentCP;
			}
			// If it does cut into CP, calculate how much, and then display it
			else{
				//if (Debug.isDebugBuild) Debug.Log("AP Used cuts into CP, Current AP: " + currentAP + ", Current CP: " + currentCP + ", AP Used: " + aPUsed);
				currentCP = (currentAP + currentCP) - aPUsed;
				//if (Debug.isDebugBuild) Debug.Log("New Current AP: " + currentAP + ", New Current CP: " + currentCP);
				currentAP = 0;
				//if (Debug.isDebugBuild) Debug.Log("Current AP set to: " + currentAP);
			
				unitAPText.GetComponent<Text>().text = "Unit Action Points: " + currentAP;
				playerCPText.GetComponent<Text>().text = "Player Command Points: " + currentCP;
			}
		}
	}

	/// =================================================================================================
	/// BEARING METHOD
	/// Method for calculating the bearing of one gameobject from another (in degrees) 15-10-14 Alisdair
	/// =================================================================================================

	/// <summary>
	/// Gives the bearing from one Vector3 value to another in the x z plane.
	/// </summary>
	/// <returns>Bearing in degrees from Home position to Target Position on the y axis</returns>
	/// <param name="homePosition">Home position.</param>
	/// <param name="targetPosition">Target position.</param>
	float getBearing(Vector2 homePosition, Game.Facing homeFacing, Vector2 targetPosition) {

		float acuteAngle = Vector2.Angle(gameClass.facingDirection[homeFacing]*Vector2.up, targetPosition-homePosition); // Find the acute angle between the two vectors
		//if(Debug.isDebugBuild) Debug.Log ("Acute Angle: " + acuteAngle);
		
		// Determine the quadrant of the target
		string quadrant = ""; 
		if (targetPosition.y >= homePosition.y){ // If the target is to the north
			quadrant = "N";
			//if (Debug.isDebugBuild) Debug.Log(quadrant);
		}
		else if (targetPosition.y <= homePosition.y) { // If the target is to the south
			quadrant = "S";
			//if (Debug.isDebugBuild) Debug.Log(quadrant);
		}
		if (targetPosition.x >= homePosition.x) { // If the target is to the east
			quadrant = quadrant + "E";
			//if (Debug.isDebugBuild) Debug.Log(quadrant);
		}
		else if (targetPosition.x <= homePosition.x){ // If the target is to the west
			quadrant = quadrant + "W";
			//if (Debug.isDebugBuild) Debug.Log(quadrant);
		}

		switch(homeFacing){	// Switch to determine the direction to rotate (+ or -) and then return the value based on the current facing of the gameobject

		case (Game.Facing.North):
			switch (quadrant){ 					// Switch determining the direction of rotation
				case("N"):
					return 0f; 					// North to north requires no rotation
				case("S"):
					return 180f; 				// North to south requires 180 degrees rotation
				case("E"):
					return 90f; 				// North to east is + 90
				case("W"):
					return -90f; 				// North to west is - 90
				case("NE"):
					return acuteAngle; 			// North to NE is a positive angle
				case("NW"):
					return -acuteAngle; 		// North to NW is a negative angle
				case("SE"):
					return 180f-acuteAngle; 	// North to SE is a positive angle
				case("SW"):
					return -180f+acuteAngle;	// North to SW is a negative angle
				default:
					if (Debug.isDebugBuild) Debug.LogError("You literally shot yourself in the foot, how'd you even manage that?");
						return 0f;					// For shooting yourself in the foot you don't need to change rotation
			}

		case (Game.Facing.South):
			switch (quadrant){ 					// Switch determining the direction of rotation
				case("N"):
					return 180f; 				// South to north requires 180 degree rotation
				case("S"):
					return 0f; 					// South to south requires no rotation
				case("E"):
					return -90f; 				// South to east is - 90
				case("W"):
					return 90f; 				// South to west is + 90
				case("NE"):
					return 180f-acuteAngle; 		// South to NE is positive angle
				case("NW"):
					return -180f+acuteAngle; 	// South to NW is negative angle
				case("SE"):
					return -acuteAngle;		 	// South to SE is negative
				case("SW"):
					return acuteAngle;			// South to SW is positive
				default:
					if (Debug.isDebugBuild) Debug.LogError("You literally shot yourself in the foot, how'd you even manage that?");
					return 0f;					// For shooting yourself in the foot you don't need to change rotation
			}
			
		case (Game.Facing.East):
			switch (quadrant){ 					// Switch determining the direction of rotation
				case("N"):
					return -90f; 				// East to north is - 90
				case("S"):
					return 90f; 				// East to south is + 90 
				case("E"):
					return 0f; 					// East to east requires no rotation
				case("W"):
					return 180f; 				// East to west requires 180 degree rotation
				case("NE"):
					return -acuteAngle; 		// East to NE is negative 
				case("NW"):
					return -180f+acuteAngle; 	// East to NW is negative
				case("SE"):
					return acuteAngle;		 	// East to SE is positive
				case("SW"):
					return 180f-acuteAngle;		// East to SW is positive
				default:
					if (Debug.isDebugBuild) Debug.LogError("You literally shot yourself in the foot, how'd you even manage that?");
					return 0f;					// For shooting yourself in the foot you don't need to change rotation
			}
			
		case (Game.Facing.West):
			switch (quadrant){ 					// Switch determining the direction of rotation
				case("N"):
					return 90f; 				// West to north is + 90
				case("S"):
					return -90f; 				// West to south is - 90
				case("E"):
					return 180f; 				// West to east requires 180 degrees
				case("W"):
					return 0f; 					// West to west requires no rotation
				case("NE"):
					return 180f-acuteAngle; 	// West to NE is positive
				case("NW"):
					return acuteAngle; 			// West to NW is positive
				case("SE"):
					return -180+acuteAngle;		// West to SE is negative
				case("SW"):
					return -acuteAngle;		// West to SW is negative
				default:
					if (Debug.isDebugBuild) Debug.LogError("You literally shot yourself in the foot, how'd you even manage that?");
					return 0f;					// For shooting yourself in the foot you don't need to change rotation
			}

		default:
			if (Debug.isDebugBuild) Debug.LogError ("A unit did not have a facing");
			return 0f; //if the unit had no facing return no rotation
		}
	}	
	
	/// ==================================
	/// BULLET CREATION
	/// ==================================

	/// <summary>
	/// Creates a bullet gameObject in the scene.
	/// </summary>
	/// <param name="startPos">Start position.</param>
	/// <param name="endPos">End position.</param>
	/// <param name="explodes">If set to <c>true</c> explodes.</param>
	/// <param name="waitSeconds">Wait seconds.</param>
	/// <param name="recallOnFinish">If set to <c>true</c> recall on finish.</param>
	// Method for creation and assignment of bullets 15-10-14 Alisdair
	private void createBullet(Vector3 startPos, Vector3 endPos, bool explodes, float waitSeconds, bool recallOnFinish){

		GameObject bullet = (GameObject) Instantiate(bulletPrefab, startPos, Quaternion.identity); // Instantiate the gameobject

		bullet.GetComponent<Bullet>().setParameters(endPos, this, explodes, recallOnFinish, waitSeconds, explosionPrefab); // Access the script and assign the variables
	}

	/// <summary>
	/// Override for creating a bullet that does not need to indicate when it's flight has completed.
	/// </summary>
	/// <param name="startPos">Start position.</param>
	/// <param name="endPos">End position.</param>
	/// <param name="explodes">If set to <c>true</c> explodes.</param>
	/// <param name="waitSeconds">Wait seconds.</param>
	// Override method for creating bullets
	private void createBullet(Vector3 startPos, Vector3 endPos, bool explodes, float waitSeconds){

		createBullet(startPos, endPos, explodes, waitSeconds, false); //Call the other method with preset variables
	}

	/// =================================
	/// Pre and post action methods
	/// =================================

	/// <summary>
	/// Changes overwatch and sustained fire sprites before the action takes place.
	/// </summary>
	/// <param name="action">The Action object.</param>
	void preAction(Action action){
		if (action.sustainedFireChanged.Count > 0)														// Create the sustained fire sprites
			showSusFire(action.sustainedFireChanged);
		removeSusFire(action.sustainedFireLost);														// Remove Overwatch and sustained fire from units that lost it before the action begins
		removeOverwatch(action.lostOverwatch);
	}

	/// <summary>
	/// Finishes the action by checking if the game is over, updating the sprite displays, refreshing the AP cost, etc
	/// </summary>
	/// <param name="action">The Action object.</param>
	void finishAction(Action action){
		deactivateHolograms();																			// Deactivate any holograms on the map
		faceAllHolograms();																				// Face all the holograms in the same direction

		setDoorCollidersEnabled(false);																	// Disable the door colliders to make it easy to click squares behind them
		previousAction = showActionsList[0];															// Reset all the variables ready for the next action
		updateCPAP(action.APCost);
		if (attackPhaseList.Count > 0)
			attackPhaseList.Clear();
		if (shootPhaseList.Count > 0)
			shootPhaseList.Clear();
		showActionsList.RemoveAt(0);
		attackSuccessful = false;
		isFirstLoopofAction = true;
		soundsFirstPass = true;																			// Allow sounds to play again
		renderers.Clear();

		refreshBlipCounts();																			// Display the new blip counts
		
		if (showActionsList.Count == 0){																// if that was the last action object in the list, then set the gamestate back to inactive & reselect the unit (to activate the buttons again)
			//if (Debug.isDebugBuild) Debug.Log ("LAST ACTION IN THE SEQUENCE SHOWN");
			if(gameClass.thisPlayer == gameClass.playerTurn){ 											// If it is the active player turn, change back to inactive after showing the sequence
				//if (Debug.isDebugBuild) Debug.Log ("IT WAS THE CLIENT'S TURN");
				if (gameClass.unitSelected){
					gameClass.changeGameState(Game.GameState.InactiveSelected);							
					gameClass.selectUnit(gameClass.selectedUnit.gameObject);
				}
				else{
					gameClass.changeGameState(Game.GameState.Inactive);
				}
			}

			if(gameClass.thisPlayer != gameClass.playerTurn){ 											//If it is the other player or AI turn change back to network wait
				//if (Debug.isDebugBuild) Debug.Log ("IT WAS NOT THE CLIENT'S TURN");
				gameClass.changeGameState(Game.GameState.NetworkWait);
				if (!gameClass.gameIsMultiplayer && !action.gameOver && action.actionType != Game.ActionType.InvoluntaryReveal){									
																										//If the game is not multiplayer and is not now over, and the last action was not an involuntary reveal, tell the AI to make another movement
						gameClass.algorithm.continueAI();
						//if (Debug.isDebugBuild) Debug.Log ("CONTINUED AI");
				}
				else{
					//if (Debug.isDebugBuild) Debug.Log ("DID NOT CONTINUE AI");
				}
			}

		}

		if(action.gameOver){																			// Check if the game has ended
			Destroy (GameObject.Find("UICanvasV2"));													// Remove the UI
			foreach( Unit unit in action.triggerRemoved){
				Destroy (unit.gameObject);																// Destroy the gameobjects that are to be removed
			}

			Destroy (uiCanvas);
			uiCanvas = (GameObject) Instantiate(endGameUI);																		// Show the End Game UI components
			if (action.winner == Game.PlayerType.SM){
				GameObject.Find("EndGameText").GetComponent<Text>().text = "SPACE MARINES WIN!";
				Color color = Color.red;
				color.a = 0.5f;
				GameObject.Find("EndGamePanel").GetComponent<Image>().color = color;
			}
			else{
				GameObject.Find("EndGameText").GetComponent<Text>().text = "GENESTEALERS WIN!";
				Color color = Color.magenta;
				color.a = 0.5f;
				GameObject.Find("EndGamePanel").GetComponent<Image>().color = color;
			}
			btnExitGO = GameObject.Find ("EndExitButton");												// Assign the exit button to work
			btnExit = btnExitGO.GetComponent<Button>();
			btnExit.onClick.AddListener(() => {btnExitClicked();});

			btnRestartGO = GameObject.Find ("RestartButton");													// Set up the restart button to reload the current level
			btnRestart = btnRestartGO.GetComponent<Button>();
			btnRestart.onClick.AddListener(() => {btnRestartClicked();});
				
		}
		else {																							// Even if the game has not ended, remove the units that have been removed by the trigger
			foreach( Unit unit in action.triggerRemoved){
				Destroy (unit.gameObject);																// Destroy the gameobjects that are to be removed
			}
		}
	

	}

	/// <summary>
	/// Shows the sustained fire sprites for all units
	/// </summary>
	/// <param name="sustainedFireChanged">Sustained fire changed unit discionary</param>
	void showSusFire(Dictionary<Unit, Unit> sustainedFireChanged){
		foreach(KeyValuePair<Unit, Unit> entry in sustainedFireChanged)
		{
			//Make a new sprite only if there is not already one in existance and if the unit gameobject still exists
			if (entry.Key.sustainedFireSprite == null && entry.Key.gameObject != null){
				Vector3 spritePosition = entry.Key.gameObject.transform.position;							// Create the sprites in the correct locations
				spritePosition.y += 2f;
				entry.Key.sustainedFireSprite = (GameObject) Instantiate(sustainedFireSprite, spritePosition, Quaternion.identity);
				entry.Key.sustainedFireSprite.transform.parent = entry.Key.gameObject.transform;			// Asign the transform of the spaceMarine as the parent so the sprite moves with it (This should actually never happen, but it would look weird if the sm moved away from it's sprite)
				susFireSprites.Add(entry.Key.sustainedFireSprite);
				if(susFireOnlyOnSelection){
					entry.Key.sustainedFireSprite.SetActive(false);
				}
			}

			if (entry.Value != null && entry.Value.gameObject != null){												// Destroy the old sprite and create a new one (the piece may have changed position)
				Destroy (entry.Key.sustainedFireTargetSprite);
				Vector3 spritePosition = entry.Value.gameObject.transform.position;
				spritePosition.y += 2f;
				entry.Key.sustainedFireTargetSprite = (GameObject) Instantiate(sustainedFireSprite, spritePosition, Quaternion.identity);
				entry.Key.sustainedFireTargetSprite.transform.parent = entry.Value.gameObject.transform;	// Asign the transform of the genestealer as the parent so the sprite moves with it
				susFireSprites.Add(entry.Key.sustainedFireTargetSprite);
				if(susFireOnlyOnSelection){
					entry.Key.sustainedFireTargetSprite.SetActive(false);
				}
			}

		}
	}

	/// <summary>
	/// Shows the sus fire sustained fire sprites for a specific unit
	/// </summary>
	/// <param name="unit">Unit.</param>
	void showSusFire(Unit unit){
		if (unit.hasSustainedFire){
			unit.sustainedFireSprite.gameObject.SetActive(true);
			unit.sustainedFireTargetSprite.gameObject.SetActive(true);
		}

	}

	/// <summary>
	/// Shows the sprite for a jammed unit.
	/// </summary>
	/// <param name="unit">Unit.</param>
	void showJam(Unit unit){
		//Make a new sprite only if there is not already one in existance
		if (unit.jammedUnitSprite == null){
			Vector3 spritePosition = unit.gameObject.transform.position;								// Create the sprites in the correct locations
			spritePosition.y += 2.5f;
			unit.jammedUnitSprite = (GameObject) Instantiate(jammedUnitSprite, spritePosition, Quaternion.identity);
		}
	}

	/// <summary>
	/// Removes the jam sprite from a specified unit.
	/// </summary>
	/// <param name="unit">Unit.</param>
	void removeJam(Unit unit){
		if(unit.jammedUnitSprite != null){
			Destroy (unit.jammedUnitSprite);
		}
		else{
			if (Debug.isDebugBuild) Debug.LogError("A unit did not have a Jam sprite to remove");
		}
	}

	/// <summary>
	/// Removes all jam sprites form the map.
	/// </summary>
	public void removeJam(){
		foreach(GameObject obj in GameObject.FindGameObjectsWithTag("jammedSprite")){
			Destroy(obj);
		}
	}

	/// <summary>
	/// Shows the die roll results.
	/// </summary>
	/// <param name="rolls">Dictionary of the playertype and one of the rolls rolled</param>
	void displayDice(Dictionary<Game.PlayerType, int[]> rolls){
		foreach(KeyValuePair<Game.PlayerType, int[]> entry in rolls){
			if (entry.Key == Game.PlayerType.SM){
				for (int i = 0; i < entry.Value.Length; i++){
					if (i == 0){
						sMDie1.GetComponent<Text>().text = entry.Value[i].ToString();
					}
					else{
						sMDie2.GetComponent<Text>().text = entry.Value[i].ToString();
					}
				}

			}
			else {
				for (int i = 0; i < entry.Value.Length; i++){
					if (i == 0){
						gSDie1.GetComponent<Text>().text = entry.Value[i].ToString();
					}
					else if (i == 1){
						gSDie2.GetComponent<Text>().text = entry.Value[i].ToString();
					}
					else{
						gSDie3.GetComponent<Text>().text = entry.Value[i].ToString();
					}
				}	
			}
		}
	}

	/// <summary>
	/// Resets the dice roll showing to X's
	/// </summary>
	void resetDice(){
			//SM die
			sMDie1.GetComponent<Text>().text = "X";
			sMDie2.GetComponent<Text>().text = "X";
			//GS die
			gSDie1.GetComponent<Text>().text = "X";
			gSDie2.GetComponent<Text>().text = "X";
			gSDie3.GetComponent<Text>().text = "X";
	}


	/// <summary>
	/// Continues the action sequence.
	/// </summary>
	public void continueActionSequence(){
		letActionsPlay = true;
	}

	/// <summary>
	/// Refreshs the blip counts above the deployment squares
	/// </summary>
	void refreshBlipCounts()
	{
		foreach(DeploymentArea depArea in mapClass.otherAreas){
			depArea.model.GetComponentInChildren<Text>().text = depArea.units.Count.ToString();
		}
	}

	/// <summary>
	/// Forces the display of action and command points.
	/// </summary>
	/// <param name="actionPoints">Action points.</param>
	/// <param name="commandPoints">Command points.</param>
	public void forceDisplayAPCP(int actionPoints, int commandPoints){
		unitAPText.GetComponent<Text>().text = "Unit Action Points: " + actionPoints;
		playerCPText.GetComponent<Text>().text = "Player Command Points: " + commandPoints;
	}

	/// <summary>
	/// Hot toggle method for the modes of displaying sustained fire
	/// </summary>
	/// <param name="active">If set to <c>true</c> then sustained fire sprites are only shown on selection</param>
	public void susFireOnlyOnSelectedUnit(bool active){
		//if(Debug.isDebugBuild) Debug.Log("Changing Sus Fire On selection to " + active);
		susFireOnlyOnSelection = active;
		foreach(GameObject obj in susFireSprites){			// change the state of all sustained fire gameobjects
			obj.SetActive(!susFireOnlyOnSelection);
		}
		if (gameClass.selectedUnit != null){																// reselect the currently selected unit if there is one
			gameClass.selectUnit(gameClass.selectedUnit.gameObject);
		}
	}

	/// <summary>
	/// Enable or disable the door colliders based on the parameter
	/// </summary>
	/// <param name="enabled">If set to <c>true</c> colliders are enabled.</param>
	void setDoorCollidersEnabled(bool enabled){
		//if (Debug.isDebugBuild) Debug.Log ("Setting Door Colliders to: " + enabled);
		foreach (GameObject door in GameObject.FindGameObjectsWithTag("Door")){
			if (door.activeInHierarchy){
					door.collider.enabled = enabled;
			}
		}
	}

	/// <summary>
	/// Removes the peanut butter.
	/// Functionality Required - IT'S EASTER EGG TIME!
	/// </summary>
	void removePeanutButter(){
			if (Debug.isDebugBuild) Debug.Log("PEANUT BUTTER REMOVED");
	}

	/// <summary>
	/// Enables or disables the specified particle system
	/// </summary>
	/// <param name="partSys">Particle system to enable.</param>
	/// <param name="toggle">If set to <c>true</c> system will be enabled.</param>
	void particleToggle(ParticleSystem partSys, bool toggle){
		if (toggle){
			partSys.enableEmission = toggle;
			partSys.Simulate(3);
			partSys.Play();
		}
		else {
			partSys.Stop();
			partSys.Clear();
		}

	}

	// =================================================
	// Hologram Methods
	// =================================================

	/// <summary>
	/// Defines the hologram units to show (GS or SM)
	/// </summary>
	public void defineHologram(){
		switch (gameClass.gameState) {
			case (Game.GameState.MoveSelection):
				if(gameClass.thisPlayer == Game.PlayerType.SM){
					//if(Debug.isDebugBuild) Debug.Log ("ACTIVE HOLOGRAM CHANGED TO SpaceMarine");
					changeHolograms(Game.EntityType.SM);
				}
				else{
					//if(Debug.isDebugBuild) Debug.Log ("ACTIVE HOLOGRAM CHANGED TO Genestealer");
					changeHolograms(Game.EntityType.GS);
				}
				break;
			case (Game.GameState.Reveal):
				//if(Debug.isDebugBuild) Debug.Log ("ACTIVE HOLOGRAM CHANGED TO Genestealer");
				changeHolograms(Game.EntityType.GS);
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Actually implements the change specified in defineHolograms
	/// </summary>
	/// <param name="type">UnitHolograms to Enable</param>
	void changeHolograms(Game.EntityType type){
		//if(Debug.isDebugBuild) Debug.Log ("THERE ARE: " + hologramParticles.Count + "Holograms that are being activated/deactivated");
		foreach (GameObject placementParticle in hologramParticles) {
			switch (type){
			case(Game.EntityType.SM):
				if(placementParticle.name == "GS_Placement_ParticleSystem")
					placementParticle.SetActive(false);
				else if(placementParticle.name == "SM_Placement_ParticleSystem")
					placementParticle.SetActive(true);
				break;

			case (Game.EntityType.GS):
				if(placementParticle.name == "GS_Placement_ParticleSystem")
					placementParticle.SetActive(true);
				else if(placementParticle.name == "SM_Placement_ParticleSystem")
					placementParticle.SetActive(false);
				break;
			}	
		}
	}

	/// <summary>
	/// Faces the active holograms to the facing specified
	/// </summary>
	/// <param name="facing">Facing.</param>
	public void faceHolograms(Game.Facing facing){
		
		foreach(GameObject hologram in activePartSys){
			if(hologram.name == "GS_Placement_ParticleSystem"){
				hologram.transform.rotation = makeRotation(makeFacing(facing), Game.EntityType.GS);
			}
			else if(hologram.name == "SM_Placement_ParticleSystem"){
				hologram.transform.rotation = makeRotation(makeFacing(facing), Game.EntityType.SM);
			}
			else{
				if(Debug.isDebugBuild) Debug.Log("NO CORRECT PARTICLE SYSTEM TYPE");
			}
		}
	}

	/// <summary>
	/// Faces all holograms to the rotation they began with
	/// </summary>
	public void faceAllHolograms(){
		
		foreach(GameObject hologram in hologramParticles){
			if(hologram.name == "GS_Placement_ParticleSystem"){
				hologram.transform.rotation = makeRotation(makeFacing(Game.Facing.West), Game.EntityType.GS);
			}
			else if(hologram.name == "SM_Placement_ParticleSystem"){
				hologram.transform.rotation = makeRotation(makeFacing(Game.Facing.East), Game.EntityType.SM);
			}
			else{
				if(Debug.isDebugBuild) Debug.Log("NO CORRECT PARTICLE SYSTEM TYPE");
			}
		}
	}

	/// <summary>
	/// Deactivates the holograms on the map
	/// </summary>
	void deactivateHolograms(){
		if (hologramsActive){
			//if (Debug.isDebugBuild) Debug.Log ("Deactivatng all holograms");
			foreach(GameObject ps in activePartSys)															// Deactivate any holograms on the map
			{
				ps.particleSystem.enableEmission = false;
			}
			hologramsActive = false;
			activePartSys.Clear();
		}
	}

	// ===========================================
	// MENU BUTTON CLICK METHODS
	// ===========================================

	/// <summary>
	/// Closes the application when the exit button is clicked
	/// </summary>
	void btnExitClicked(){
		if(Debug.isDebugBuild) Debug.LogWarning("APPLICATION WILL NOW CLOSE ON USER COMMAND");
		Application.Quit();
	}

	/// <summary>
	/// Opens the menu overlay
	/// </summary>
	void btnMenuClicked(){
		//if(Debug.isDebugBuild) Debug.Log("MenuButton Clicked");
		openMenu();
	}

	/// <summary>
	/// Opens the Info Overlay
	/// </summary>
	void btnInfoClicked(){
		
	}

	/// <summary>
	/// Reloads the current level (Effectively restarting the game)
	/// </summary>
	void btnRestartClicked(){
		if (Debug.isDebugBuild)
						Debug.LogWarning ("LEVEL RELOADING!");
		Application.LoadLevel(Application.loadedLevel);
	}

	// ==========================================
	// MULTI AUDIO METHODS
	// ==========================================

	/// <summary>
	/// Plays the sm death.
	/// </summary>
	/// <param name="audio">Audio.</param>
	void playSmDeath(AudioSource audio){
		int rand = Random.Range(1, 3);
		if (rand == 1) {
			audio.PlayOneShot(sm_Death);
		} 
		else {
			audio.PlayOneShot(sm_Death_2);
		}
	}

	/// <summary>
	/// Plays the door sound.
	/// </summary>
	/// <param name="audio">Audio.</param>
	/// <param name="isOpen">If set to <c>true</c> is open.</param>
	void playDoorSound(AudioSource audio, bool isOpen){
		if (isOpen) {
			audio.PlayOneShot (doorOpen);
		} 
		else {
			audio.PlayOneShot(doorClose);
		}
	}

	/// <summary>
	/// Plays the sm battle cry.
	/// </summary>
	/// <param name="audio">Audio.</param>
	void playSmBattleCry(AudioSource audio){
		int rand = Random.Range(1, 3);
		if (rand == 1) {
			audio.PlayOneShot(sm_Fire_1);
		} 
		else {
			audio.PlayOneShot(sm_Fire_2);
		}
	}
}

