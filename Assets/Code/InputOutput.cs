/* 
 * The InputOutput class handles graphic representation of the map and input from the GUI and mouse clicks
 * Created by Alisdair Robertson 9/9/2014
 * Version 21-10-14.5
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

	// UI and Button References added 14/9/14 by Alisdair
	public GameObject UICanvas;
	GameObject btnAttackGO, btnShootGO, btnMoveGO, btnToggleDoorGO, btnOverwatchGO, btnRevealGO;
	Button btnAttack, btnShoot, btnMove, btnToggleDoor, btnOverwatch, btnReveal;

	// UI text field references 14 Sept 14 by Alisdair
	GameObject unitAPText, playerCPText;

	// Facing Selection canvas Added by Alisdair 17/9/2014
	public GameObject facingSelectionCanvas;
	public GameObject currentFacingSelectionCanvas; //Made public 2.10.14 RB

	// Rory Bolt 25.9.14
	public InputHandler inputHandlerController;

	// End Turn button added 2-10-2014 by Alisdair
	GameObject btnEndTurnGO;
	Button btnEndTurn;

	// Color store for selecton/deselection added Alisdair 3-10-2014
	Color preSelectionColor;

	// Other action showing declerations Alisdair 4-10-14 
	List<Action> showActionsList = new List<Action>();
	public float stepAmmount;
	public int rotateStepMultiplier;
	float stepMoveAmmount;
	float stepRotateAmmount;

	// Float for the elevation of all new units
	public float unitElevation;

	// Declerations for hint objects Alisdair 5-10-14 
	public GameObject overwatchSprite;
	public GameObject sustainedFireSprite;

	// variables for tracking AP and CP
	int currentAP;
	int currentCP;

	// Variables required for showing shoot actions - Alisdair 15-10-14
	enum ShootPhase{RotateTowards, CreateBullets, BulletsMoving, UnitDeath, RotateBack}; // The stages of shooting
	List<ShootPhase> shootPhaseList = new List<ShootPhase>(); // List to add the required phases of a shoot actions
	public GameObject bulletPrefab, explosionPrefab; 	// Bullet and explosion gameobjects
	bool isFirstLoopofAction = true; 	// Check to allow storage of a rotation to turn to
	Quaternion rotationBefore;			// Quaternion of the unit before turning to face the target
	bool attackSuccessful = false;		// Bool for inclusion of unit death
	public bool bulletsComplete = false; // Bool for finishing bullets moving phase
	Quaternion aimRot = Quaternion.identity; // Initially setup the aim rotation.

	// Variables for showing attack actions
	enum AttackPhase{RotateTowards, MoveTowards, Flash, MoveBack, UnitDeath, RotateBack}; 	// Stages of a melee attack
	List <AttackPhase> attackPhaseList = new List<AttackPhase>();							// The list to store the phases of the attack
	Vector3 exeInitPos = new Vector3();														// The store of the original unit position
	Vector3 exeUnitAttackPos = new Vector3();												// The position to move the unit to for the attack animation 
	public static float FLASH_WAIT = 0.2f;													// The length in seconds of a flash on or off stage
	public static int ALLOWED_NUM_FLASHES = 5;												// The number of flashes in a successful attack
	int numFlashes;																			// The number of flashes already gone
	float timeStore = 0;																	// Variable used for storing the time the flash has been active
	Color preFlashColor;																	// Variable used for storing the color of the unit before flashing it for an attack
	List<Renderer> renderers = new List<Renderer>();

	// Variables for dice
	GameObject sMDie1, sMDie2, gSDie1, gSDie2, gSDie3;

	// Variables for Involuntary reveal and freezing action sequence
	Action previousAction;
	List<ActionManager> actionManagers = new List<ActionManager>();
	bool letActionsPlay = true;

	// assign the step ammounts in the start method Alisdair 12-10-14
	public void Start(){
		stepMoveAmmount = stepAmmount;
		stepRotateAmmount = stepAmmount * rotateStepMultiplier;
	}

	public void Update(){
		if (letActionsPlay){
			// Get the action from the first position in the list
				// Determine what action type it is
					// Use a switch to direct to correct type of action
						// e.g. if movement, move toward the position
					// Check if the action has been completed
					// complete the action and remove the action object from the list
					// else, leave the action to be iterated again in the next frame

			if(showActionsList.Count != 0){
				//Debug.Log ("ShowActionsList.Count != 0, incrementing an action");
				gameClass.changeGameState(Game.GameState.ShowAction);//Change the state to showaction state
				Action action = showActionsList[0];
				//Debug.Log ("The action is of type: " + action.actionType);
				
				// Executor pos and rot
				Unit exeUnit = action.executor;
				Vector3 exePos = action.executor.gameObject.transform.position;
				Quaternion exeRot = action.executor.gameObject.transform.rotation;
				exeInitPos = new Vector3(exePos.x, exePos.y, exePos.z);						// Store the initial position of the executor
				//Debug.Log ("Unit Position: " + exePos);
				//Debug.Log("Unit Rotation: " + exeRot);
				
				// Target unit decleration
				Unit tarUnit;

				if (isFirstLoopofAction){													// Display the dice roll on the first loop of the action
					displayDice(action.diceRoll);
				}

				// Make the action
				switch (action.actionType){

					/// ============================
					/// MOVE ACTION
					/// ============================
					case (Game.ActionType.Move):
						isFirstLoopofAction = false;
						//Debug.Log("Update entered Move action sector of switch");
					
						// Create aim pos and rot
						Vector3 aimPos = makePosition(action.movePosition, unitElevation);
						//Debug.Log ("Aim position is " + aimPos);
						aimRot = makeRotation(makeFacing(action.moveFacing), exeUnit.unitType);
						//Debug.Log ("Aim Rotation is: " + aimRot);

						// Make part of the movements
						action.executor.gameObject.transform.position = Vector3.MoveTowards(exePos, aimPos, stepMoveAmmount*Time.deltaTime);
						action.executor.gameObject.transform.rotation = Quaternion.RotateTowards(exeRot, aimRot, stepRotateAmmount*Time.deltaTime);

						// Remove overwatch sprites for those units that have lost overwatch or sustained fire
						removeOverwatch(action.lostOverwatch);
						removeSusFire(action.sustainedFireLost);

						// Check to see if the unit is in the correct place and rotation, if so, finish the action and remove the action from the list
						// also update the unit AP and CP fields
						if (exePos == aimPos){
							//Debug.Log("PositionEqual");
							if(exeRot.eulerAngles.y == aimRot.eulerAngles.y){
								finishAction(action);
							}
							else{
								//Debug.Log ("Rotation not equal, Rotation aim is: " + aimRot.eulerAngles + "Current Rotation: " + exeRot.eulerAngles);
							}
						}
						else {
							//Debug.Log ("Position not Equal, Position target is: " + aimPos);
						}

						break;

					///==============================
					/// OVERWATCH ACTION
					/// 
					/// ++++++++++++++++++++
					/// INCOMPLETE

					case (Game.ActionType.Overwatch):
						isFirstLoopofAction = false;
						Debug.Log("IT'S OVERWATCH TIME!");
						// determine the position the sprite
						Vector3 spritePosition = exePos;
						spritePosition.y += 1.5f;

						//instantiate the sprite at the position & give reference to the unit
						action.executor.overwatchSprite = (GameObject) Instantiate(overwatchSprite, spritePosition, Quaternion.identity);

						finishAction(action);

						break;

					///=============================
					/// ToggleDoor Action
					///=============================

					case (Game.ActionType.ToggleDoor):
						isFirstLoopofAction = false;
						// Get the position that the door would be at based off the unit's position and rotation Alisdair 14-10-14
						//Debug.Log("Exe pos:" + exePos);
						Vector2 doorMapPosition = new Vector2 (exePos.x, exePos.z);
						//Debug.Log ("converted to a pos in map: " + doorMapPosition);
						//Debug.LogError(action.executor.facing);
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
						//Debug.Log ("Final Door Position: " + doorMapPosition);
						//Debug.Log ("Therefore door pos = " + makePosition(doorMapPosition, 2000));

							// Check if the position has a door, Get the door (unit), Toggle the door 
						Square mapSquareWithDoor = mapClass.getSquare(doorMapPosition);
						Unit doorToToggle;
						// Check that the door is there, then assign the variables for use in toggling
						//Debug.Log ("DOES POSITION HAVE DOOR?: " + mapClass.hasDoor(doorMapPosition));
						if (mapClass.hasDoor(doorMapPosition)){
							doorToToggle = mapClass.getOccupant(doorMapPosition);
							//target unit variables being assigned
							tarUnit = doorToToggle;

							//If the door is open (in the map class, which is up to date), make it an open door
							if (mapClass.isDoorOpen(doorMapPosition)){
								GameObject newDoor = (GameObject) Instantiate(OpenDoorPrefab, makePositionDoor(doorMapPosition, unitElevation, mapSquareWithDoor.door.facing, true), makeRotation(makeFacing(mapSquareWithDoor.door.facing), Game.EntityType.Door));
								Destroy(mapSquareWithDoor.door.gameObject);
								mapSquareWithDoor.door.gameObject = newDoor;
							}

							else { //if the door is closed in the map class, show this also
								GameObject newDoor = (GameObject) Instantiate(ClosedDoorPrefab, makePositionDoor(doorMapPosition, unitElevation, mapSquareWithDoor.door.facing, false), makeRotation(makeFacing(mapSquareWithDoor.door.facing), Game.EntityType.Door));
								Destroy(tarUnit.gameObject);
								mapSquareWithDoor.door.gameObject = newDoor;
							}

							//Finish the action and update the AP
							finishAction(action);
						}

						break;

					///==================================================================
					/// ATTACK ACTION - FOR MELEE
					///==================================================================
					/// Attack will only be functional for one-way attacks (e.g GS attack SM)
					///++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

					case (Game.ActionType.Attack):
							//Determine if the attack is GS > SM, SM > GS, or GS <> SM.
							// run the "animation"  of the attack (as characters are not rigged for animation, display this with game objects moving into one another and then back into their original position. (also use change of mesh colours)
								//gs to sm
								//sm to gs
								//sm & gs
						tarUnit = action.target;
						foreach(Renderer rend in action.target.gameObject.GetComponentsInChildren<Renderer>()){
							renderers.Add (rend);																					// Get all the renderer gameobject components that need to be faded
						}
						
						if (isFirstLoopofAction){
							rotationBefore = new Quaternion(exeRot.z, exeRot.y, exeRot.z, exeRot.w); 								// Store the initial rotation
							
							float offset = getBearing(exeUnit.gameObject, exeUnit.facing, tarUnit.gameObject);
							Debug.LogWarning("Offset rotation IS: " + offset);
							
							exeInitPos = new Vector3(exePos.x, exePos.y, exePos.z); 												// the initial position of the executor unit
							exeUnitAttackPos = Vector3.MoveTowards(exePos, tarUnit.gameObject.transform.position, 0.5f); 			// Get the position the unit will be when the attack occurs

							
							isFirstLoopofAction = false; 																			// Set that it is no longer the first loop
							attackPhaseList.Add(AttackPhase.RotateTowards); 														// Add the rotation phase
							attackPhaseList.Add(AttackPhase.MoveTowards); 															// Add the moving toward phase
							
							if (action.destroyedUnits.Count > 0){ 																	// Determine if the attack was successful (for use when creating bullets)
								attackSuccessful = true;
								//Debug.LogWarning("Melee attack to be successful");
								attackPhaseList.Add(AttackPhase.Flash);																// Add the flashing of the attacked unit
								attackPhaseList.Add(AttackPhase.UnitDeath); 														// If it is successful add the phase for unit death for the lineup
							}
							
							attackPhaseList.Add(AttackPhase.MoveBack); 																// add the stage for moving back after the attack action
							attackPhaseList.Add(AttackPhase.RotateBack);															// Rotate the unit back to it's original facing
							
						}

						switch (attackPhaseList[0]){ 																				// Use a switch for actioning the phases
								
							case (AttackPhase.RotateTowards): 																		// Rotating the executor to face the target
								Debug.LogWarning("Rotate Toward phase");
								
								//action.executor.gameObject.transform.rotation = Quaternion.RotateTowards(exeRot, aimRot, stepRotateAmmount*Time.deltaTime); // Increment the rotation
								
								//if(exeRot.eulerAngles.y == aimRot.eulerAngles.y){
								attackPhaseList.RemoveAt(0); 																		// Move to the next phase
								//}
								break;
								
							case (AttackPhase.MoveTowards): 																		// Moving the executor towards the aim position
								action.executor.gameObject.transform.position = Vector3.MoveTowards(exePos, exeUnitAttackPos, stepMoveAmmount*Time.deltaTime);
								
								if (exePos == exeUnitAttackPos){
									attackPhaseList.RemoveAt(0); 																	// Move to the next phase
								}	
								break;
								
							case (AttackPhase.Flash): 																				// Flash the target a colour to indicate that an attack is taking place
								
								if (action.target.gameObject.renderer.material.color != Color.yellow)								// Store the original color of the unit
									preFlashColor = new Color (action.target.gameObject.renderer.material.color.r, action.target.gameObject.renderer.material.color.g, action.target.gameObject.renderer.material.color.b);

								if (timeStore >= FLASH_WAIT){																		// If the timer is equal to the wait time, change the color and reset the timer
									if (action.target.gameObject.renderer.material.color != Color.yellow)
									{
										action.target.gameObject.renderer.material.color = Color.yellow;
									}

									else 
									{
										action.target.gameObject.renderer.material.color = preFlashColor;
										
									}
									timeStore = 0f;
									numFlashes++;
								}
								if (numFlashes >= ALLOWED_NUM_FLASHES){																// If the allowed number of flashes has been reached, move to the next phase
									attackPhaseList.RemoveAt(0);
								}
									timeStore += Time.deltaTime;
								break;

							case(AttackPhase.UnitDeath): 																			// Fade the GS gameobject out and then remove it
								float alphaLevel = 255;
								foreach (Renderer rend in renderers){ 																// Decrease the alpha level on all of the child renderers (to fade out the gameobject)
									Color color = rend.material.color;
									color.a -= 0.02f;
									alphaLevel = color.a;
									rend.material.color = color;
								}
								
								if (alphaLevel <= 0f) { 																			// If the gameobject is now fully transparent, remove it.
									if(action.executor.sustainedFireTargetSprite != null){
										Destroy (action.executor.sustainedFireTargetSprite);
									}
									Destroy (action.target.gameObject);
									attackPhaseList.RemoveAt(0); 																	// Move to the next phase
								}
								break;

							case (AttackPhase.MoveBack):																			// Move the executor gameobject back to it's original position
								action.executor.gameObject.transform.position = Vector3.MoveTowards(exePos, exeInitPos, stepMoveAmmount*Time.deltaTime);
								
								if (exePos == exeInitPos){
									attackPhaseList.RemoveAt(0); 																		// Move to the next phase
								}
								break;
								
							case(AttackPhase.RotateBack): 																			// Rotate the unit back to the position that it originally was at
								Debug.LogWarning("RotateBack Phase");
								//action.executor.gameObject.transform.rotation = Quaternion.RotateTowards(exeRot, rotationBefore, stepRotateAmmount*Time.deltaTime); 
								// Rotate the Space marine back to the correct direction
								
								//if(exeRot.eulerAngles.y == rotationBefore.eulerAngles.y){ // If the rotation back has completed, end the action
								finishAction(action);
								//}
								
								break;
							}
							break;

					///============================================
					/// SHOOT ACTION - Ranged Attack (SM shoots GS)
					///============================================
					case (Game.ActionType.Shoot):
						tarUnit = action.target;
						
						if (isFirstLoopofAction){
							rotationBefore = new Quaternion(exeRot.z, exeRot.y, exeRot.z, exeRot.w); 								// Store the initial rotation

							float offset = getBearing(exeUnit.gameObject, exeUnit.facing, tarUnit.gameObject);
							Debug.LogWarning("Offset rotation IS: " + offset);

						foreach(Renderer rend in action.target.gameObject.GetComponentsInChildren<Renderer>()){
							renderers.Add (rend);																					// Get all the renderer gameobject components that need to be faded
						}
							aimRot = Quaternion.Euler(aimRot.eulerAngles.x, aimRot.eulerAngles.y - offset, aimRot.eulerAngles.z); 	// Calculate the rotation to aim for that means the SM will be facing the GS

							isFirstLoopofAction = false; 																			// Set that it is no longer the first loop
							shootPhaseList.Add(ShootPhase.RotateTowards); 															// Add the rotation phase
							shootPhaseList.Add(ShootPhase.CreateBullets); 															// Add the phase for creating the bullets
							shootPhaseList.Add(ShootPhase.BulletsMoving); 															// Add the bullet moving phase

							if (action.destroyedUnits.Count > 0){ 																	//Determine if the attack was successful (for use when creating bullets)
								attackSuccessful = true;
								// Debug.LogWarning("Shoot attack to be successful");
								shootPhaseList.Add(ShootPhase.UnitDeath); 															//If it is successful add the phase for unit death for the lineup
							}

							shootPhaseList.Add(ShootPhase.RotateBack); 																//Finally add the stage for rotating back after the shoot action
							
						}

						switch (shootPhaseList[0]){ 																				// Use a switch for actioning the phases

							case (ShootPhase.RotateTowards): 																		// Rotating the SM to face the GS
								Debug.LogWarning("Rotate Toward phase");
								
								//action.executor.gameObject.transform.rotation = Quaternion.RotateTowards(exeRot, aimRot, stepRotateAmmount*Time.deltaTime); // Increment the rotation

								//if(exeRot.eulerAngles.y == aimRot.eulerAngles.y){
									shootPhaseList.RemoveAt(0); 																	// Move to the next phase
								//}
								break;

							case (ShootPhase.CreateBullets): 																		// Creating the bullets
								//Debug.LogWarning("Creating the bullets");
								
								Vector3 bulStart = new Vector3(exePos.x, unitElevation + 1.0f, exePos.z);							// Create the Vector3 positions for the bullets to start and end at
								Vector3 bulEnd = new Vector3(tarUnit.gameObject.transform.position.x, unitElevation + 0.8f, tarUnit.gameObject.transform.position.z);

								if (!attackSuccessful){ 																			// If the attack is not successful make the bullets miss
									//Debug.Log("ATTACK NOT SUCCESSFUL, MAKING BULLETS MISS");
									int rand = Random.Range(1, 4);
									switch (rand){
										case 1: 																					// make the bullets miss above
											if (tarUnit.unitType == Game.EntityType.Door)											// If the unit is a door, make the miss higher
												bulEnd.y += 0.5f;
											bulEnd.y += 1;
											break;
										case 2: 																					// make the bullets miss below
											bulEnd.y -= 1;
											break;
										case 3: 																					// make the bullets miss to the left
											if (tarUnit.facing == Game.Facing.North || tarUnit.facing == Game.Facing.South)			
												bulEnd.x += 0.6f;
											else if (tarUnit.facing == Game.Facing.West || tarUnit.facing == Game.Facing.East)			
												bulEnd.z += 0.6f;
											else
												bulEnd.y -= 1;
											break;
										case 4: 																					// make the bullets miss to the right
											if (tarUnit.facing == Game.Facing.North || tarUnit.facing == Game.Facing.South)			
												bulEnd.x -= 0.6f;
											else if (tarUnit.facing == Game.Facing.West || tarUnit.facing == Game.Facing.East)			
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

								
								foreach (Renderer rend in renderers){ 																// Decrease the alpha level on all of the child renderers (to fade out the gameobject)
									Color color = rend.material.color;
									color.a -= 0.02f;
									alphaLevel = color.a;
									rend.material.color = color;
								}

								if (alphaLevel <= 0f) { 																			// If the gameobject is now fully transparent, remove it.
									if(action.executor.sustainedFireTargetSprite != null){
										Destroy (action.executor.sustainedFireTargetSprite);
									}		
									Destroy (action.target.gameObject);
							        shootPhaseList.RemoveAt(0); 																	// Move to the next phase
								}

								break;

							case(ShootPhase.RotateBack): 																			// Rotate the unit back to the position that it originally was at
								Debug.LogWarning("RotateBack Phase");
								//action.executor.gameObject.transform.rotation = Quaternion.RotateTowards(exeRot, rotationBefore, stepRotateAmmount*Time.deltaTime); 
																																	// Rotate the Space marine back to the correct direction
								
								//if(exeRot.eulerAngles.y == rotationBefore.eulerAngles.y){ // If the rotation back has completed, end the action
								finishAction(action);
								//}
						
								break;
							}
							break;

					/// ===================================
					/// Involuntary Reveal Action
					/// ===================================

					case (Game.ActionType.InvoluntaryReveal):
						letActionsPlay = false;
						finishAction(action);
						GameObject.Find("GameController").GetComponent<RevealManager>().involuntaryReveal(action.executor.position, actionManagers[0], previousAction.prevLoS); 																												
						break;																											// Call the involuntary reveal method

					default:
						Debug.LogWarning("Game to show an action now");
						break;
				}
			}
		}
	}

	/// <summary>
	/// Create the Game UI in the scene
	/// </summary>
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
		//Debug.Log("UpdateCPAP InstantiateUI with AP: " + 0);
		updateCPAP(0);

		//assign the dice text elements
		sMDie1 = GameObject.Find("SMDie1");
		sMDie2 = GameObject.Find("SMDie2");
		gSDie1 = GameObject.Find("GSDie1");
		gSDie2 = GameObject.Find("GSDie2");
		gSDie3 = GameObject.Find("GSDie3");

		//Assign the method to call from the end turn button added 2-10-14 Alisdair
		btnEndTurnGO = GameObject.Find ("BtnTurn");
		btnEndTurn = btnEndTurnGO.GetComponent<Button>();
		btnEndTurn.onClick.AddListener(() => {btnEndTurnClicked();});
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
			int xPos = (int) positionV2.x;
			int zPos = (int) positionV2.y;

			GameObject floorPiece = (GameObject) Instantiate(FloorPiecePrefab, new Vector3(xPos, (-0.5f), zPos), Quaternion.identity); //Create the game object in the scene
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
				depArea.model = (GameObject) Instantiate(BlipDeploymentPiecePrefab, new Vector3(xPos, -0.5f, zPos), depAreaFacing); //Create the game object in the scene
				Vector3 position = depArea.model.transform.GetChild(0).transform.position;
				Vector3 newPosition = new Vector3 (position.x, 1.6f, position.z);
				depArea.model.transform.GetChild(0).transform.position = newPosition;

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
		Debug.Log ("Showing an Action Sequence of length: " + actions.Length);
		actionManagers.Add(actionManager);
		updateGUIActions(); //Disable the GUI actions Alisdair 13-10-14
		showActionsList.AddRange(actions);
	}

	public void selectUnit (Unit unit, Game.ActionType[] actions){ //Filled by Alisdair 11/9/2014
		/*
		 * Set the display to be appropriate to the selection of this unit, as well as showing/enabling the buttons for the action types.
		 */
		//Debug.LogWarning("Unit selected");
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
		//Debug.Log("UpdateCPAP SELECTUNIT with AP: " + 0);
		updateCPAP(0);
	}

	public void deselect(){ //Filled by Alisdair 11/9/2014
		/*
		 * This method removes the mesh renderer tint on the selected unit
		 */
		//Debug.LogWarning("Unit deselected");
		//set the render colour on the selected object back to nothing (if there is a selected unit)
		//Must change this to a tint later, rather than a full material colour
		if (selectedUnit != null) {
			selectedUnit.renderer.material.color = preSelectionColor;

			selectedUnit = null;

			//set the gui to show no actions & set AP to 0
			updateGUIActions();
			//Debug.Log("UpdateCPAP DESELECT with AP: " + 0);
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
		//Debug.LogError("placeUnit method partially complete. Refer Alisdair.");

		//Check to see if the unit is being placed in a deployment area
		if (unit.position.x < 0){ // if the unit is being placed in a deployment area
			DeploymentArea depArea = mapClass.otherAreas[(int)(-1-unit.position.x)];
			Vector2 adjPos = depArea.adjacentPosition; //get position of adjecent piece
			
			//Converting Vector2 to Vector3
			//Vector2 y = Vector3 z (North/South)
			//Vector2 x = Vector3 x (East/West)
			//Vector3 y is vertical (leave at constant value)
			int xPos = (int) adjPos.x;
			int zPos = (int) adjPos.y;

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

			switch (unit.unitType){
					
				case Game.EntityType.Blip:
					unit.gameObject = (GameObject) Instantiate(BlipPrefab, new Vector3(xPos, unitElevation, zPos), makeRotation(makeFacing(unit.facing), Game.EntityType.Blip)); //Create the blip object
					refreshBlipCounts();
					break;
				default:
					Debug.LogError("There was not a valid unit to place into a deployment area.");
					break;
			}
		}
		else{ // If the unit is not in a deployment area, place as normal
			//Instantiate the unit at the position and pass a reference back to the unit class
			switch (unit.unitType){
				
				case Game.EntityType.Blip:
					unit.gameObject = (GameObject) Instantiate(BlipPrefab, makePosition(unit.position, unitElevation + 0.5f), makeRotation(makeFacing(unit.facing), Game.EntityType.Door)); //Create the blip object above the floor object
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
					Debug.LogError("There was not a valid unit to place");
					break;
			}
		}

		
	}

	/// <summary>
	/// Removes the gameobject of the unit at the specified position
	/// </summary>
	/// <param name="position">Position of the object to remove.</param>
	public void removeUnit(Vector2 position){
		if (mapClass.getSquare(position).isOccupied)
			Destroy(mapClass.getSquare(position).occupant.gameObject);
	}

	public void resetMap(){
		Debug.LogWarning("Reset Map Called");

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
		inputHandlerController.overwatchClicked();
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
		if(gameClass.gameState == Game.GameState.Reveal)
		{
			inputHandlerController.revealOrientationClicked(Game.Facing.North);
		}
		else
		{
			inputHandlerController.orientationClicked (Game.Facing.North);//RB 18.9.14
		}
		Destroy(currentFacingSelectionCanvas);
	}

	public void btnFaceEast(){
		if(gameClass.gameState == Game.GameState.Reveal)
		{
			inputHandlerController.revealOrientationClicked(Game.Facing.East);
		}
		else
		{
			inputHandlerController.orientationClicked (Game.Facing.East);//RB 18.9.14
		}
		Destroy(currentFacingSelectionCanvas);

	}

	public void btnFaceSouth(){
		if(gameClass.gameState == Game.GameState.Reveal)
		{
			inputHandlerController.revealOrientationClicked(Game.Facing.South);
		}
		else
		{
			inputHandlerController.orientationClicked (Game.Facing.South);//RB 18.9.14
		}
		Destroy(currentFacingSelectionCanvas);
		
	}

	public void btnFaceWest(){
		if(gameClass.gameState == Game.GameState.Reveal)
		{
			inputHandlerController.revealOrientationClicked(Game.Facing.West);
		}
		else
		{
			inputHandlerController.orientationClicked (Game.Facing.West);//RB 18.9.14
		}
		Destroy(currentFacingSelectionCanvas);
		
	}

	//Method to convert vector 2 to vector 3 - Alisdair 22-9-14
	Vector3 makePosition(Vector2 position, float elevation){
		int xPos = (int) position.x;
		int zPos = (int) position.y;
		Vector3 v3 = new Vector3 (xPos, elevation, zPos); 
		return v3;
	}

	//Method for positioning open doors
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

	/// <summary>
	/// Make the end turn button interactable or not based on the game state
	/// </summary>
	public void defineEndTurnBtn(){
		//Check to see if the End turn button should be enabled or not, and assign the required state
		if (gameClass.gameState == Game.GameState.AttackSelection || 
		    gameClass.gameState == Game.GameState.MoveSelection || 
		    gameClass.gameState == Game.GameState.ShowAction ||
		    gameClass.gameState == Game.GameState.NetworkWait){
			if (gameClass.playerTurn == gameClass.thisPlayer) {
				btnEndTurn.interactable = false;
			}
		}

		else {
			btnEndTurn.interactable = true;
		}
	}

	//Adjust quaternion that makes sense for use with the space marine model - Alisdair 2-10-2014
	Quaternion makeRotation(Quaternion reference, Game.EntityType type){

		Quaternion returnQuaternion;
		int x, y, z;

		switch (type){
			case (Game.EntityType.SM):
				x = (int) reference.eulerAngles.x + 270;
				y = (int) reference.eulerAngles.y - 90;
				z = (int) reference.eulerAngles.z;
				returnQuaternion = Quaternion.Euler(x, y, z);
			return returnQuaternion;

			case (Game.EntityType.Door): // Door case added Alisdair 14-10-14
				x = (int) reference.eulerAngles.x + 90;
				y = (int) reference.eulerAngles.y;
				z = (int) reference.eulerAngles.z;
				returnQuaternion = Quaternion.Euler(x, y, z);
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
		Debug.LogWarning ("There are: " + units.Count + " units in the list of units losing overwatch");
		foreach (Unit unit in units){
			if (unit.sustainedFireSprite)
				Destroy(unit.overwatchSprite);
		}
	}

	/// <summary>
	/// Removes the sustained fire sprites from the units in the list
	/// </summary>
	/// <param name="units">Units.</param>
	void removeSusFire(List <Unit> units){
		Debug.LogWarning ("There are: " + units.Count + " units in the list of units losing sustained fire");
		foreach (Unit unit in units){
			if (unit.sustainedFireSprite)
				Destroy(unit.sustainedFireSprite);
				Destroy (unit.sustainedFireTargetSprite);
		}
	}

	//Method to update the displayed CP and AP
	void updateCPAP(int aPUsed){
		//Debug.Log("begin Update CPAP, AP Used: " + aPUsed);
		// if it does not cut into CP, just display it
		if (aPUsed < currentAP || aPUsed == currentAP){
			//Debug.Log(" aPUsed <= currentAP; Current AP: " + currentAP + " AP Used: " + aPUsed);
			currentAP = currentAP - aPUsed;
			//Debug.Log("New Current AP: " + currentAP);
			unitAPText.GetComponent<Text>().text = "Unit Action Points: " + currentAP;
			playerCPText.GetComponent<Text>().text = "Player Command Points: " + currentCP;
		}
		// If it does cut into CP, calculate how much, and then display it
		else{
			//Debug.Log("AP Used cuts into CP, Current AP: " + currentAP + ", Current CP: " + currentCP + ", AP Used: " + aPUsed);
			currentCP = (currentAP + currentCP) - aPUsed;
			//Debug.Log("New Current AP: " + currentAP + ", New Current CP: " + currentCP);
			currentAP = 0;
			//Debug.Log("Current AP set to: " + currentAP);
		
			unitAPText.GetComponent<Text>().text = "Unit Action Points: " + currentAP;
			playerCPText.GetComponent<Text>().text = "Player Command Points: " + currentCP;
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
	float getBearing(GameObject home, Game.Facing homeFacing, GameObject target) {

		Vector2 homePosition = new Vector2(home.transform.position.x, home.transform.position.z);
		Vector2 targetPosition = new Vector2(target.transform.position.x, target.transform.position.z); // Convert to Vector2 values for simplicity

		float acuteAngle = Vector2.Angle(homePosition, targetPosition); // Find the acute angle between the two vectors
		
		// Determine the quadrant of the target
		string quadrant = ""; 
		if (homePosition.y < targetPosition.y){ // If the target is to the north
			quadrant = "N";
		}
		else if (homePosition.y > targetPosition.y) { // If the target is to the south
			quadrant = "S";
		}
		if (homePosition.x < targetPosition.x) { // If the target is to the east
			quadrant = quadrant + "E";
		}
		else if (homePosition.x > targetPosition.x){ // If the target is to the west
			quadrant = quadrant + "W";
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
						Debug.LogError("You literally shot yourself in the foot, how'd you even manage that?");
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
					Debug.LogError("You literally shot yourself in the foot, how'd you even manage that?");
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
					Debug.LogError("You literally shot yourself in the foot, how'd you even manage that?");
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
					return -180+acuteAngle;		// West to SW is negative
				default:
					Debug.LogError("You literally shot yourself in the foot, how'd you even manage that?");
					return 0f;					// For shooting yourself in the foot you don't need to change rotation
			}

		default:
			Debug.LogError ("A unit did not have a facing");
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

	/// ==================================
	/// Ending an action
	/// ==================================

	void finishAction(Action action){
		previousAction = showActionsList[0];
		updateCPAP(action.APCost);
		if (showActionsList[0].sustainedFireChanged.Count > 0)											// Create the sustained fire sprites
			showSusFire(showActionsList[0].sustainedFireChanged);
		if (attackPhaseList.Count > 0)
			attackPhaseList.RemoveAt(0);
		if (shootPhaseList.Count > 0)
			shootPhaseList.RemoveAt(0);
		showActionsList.RemoveAt(0);
		attackSuccessful = false; 																		//Reset bools for use in next attack action
		isFirstLoopofAction = true;
		refreshBlipCounts();
		Debug.LogWarning("Units with susFire changed: " + action.sustainedFireChanged.Count);
		showSusFire(action.sustainedFireChanged);
		Debug.LogWarning("Units which LOST susFire: " + action.sustainedFireLost.Count);
		removeSusFire(action.sustainedFireLost);
		removeOverwatch(action.lostOverwatch);
		renderers.Clear();


		if (showActionsList.Count == 0){																// if that was the last action object in the list, then set the gamestate back to inactive & reselect the unit (to activate the buttons again)
			gameClass.changeGameState(Game.GameState.Inactive);
			if (gameClass.unitSelected){
				gameClass.changeGameState(Game.GameState.InactiveSelected);
				gameClass.selectUnit(gameClass.selectedUnit.gameObject);
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
			//Make a new sprite only if there is not already one in existance
			if (entry.Key.sustainedFireSprite == null){
				Vector3 spritePosition = entry.Key.gameObject.transform.position;							// Create the sprites in the correct locations
				spritePosition.y += 2f;
				entry.Key.sustainedFireSprite = (GameObject) Instantiate(sustainedFireSprite, spritePosition, Quaternion.identity);
			}

			if (entry.Value != null){																		// Destroy the old sprite and create a new one (the piece may have changed position)
				Destroy (entry.Key.sustainedFireTargetSprite);
				Vector3 spritePosition = entry.Value.gameObject.transform.position;
				spritePosition.y += 2f;
				entry.Key.sustainedFireTargetSprite = (GameObject) Instantiate(sustainedFireSprite, spritePosition, Quaternion.identity);
			}

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

	public void continueActionSequence(){
		letActionsPlay = true;
	}

	void refreshBlipCounts()
	{
		foreach(DeploymentArea depArea in mapClass.otherAreas){
			depArea.model.GetComponentInChildren<Text>().text = depArea.units.Count.ToString();
		}
	}

}