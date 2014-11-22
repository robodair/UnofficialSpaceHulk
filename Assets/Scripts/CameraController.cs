using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraController : MonoBehaviour {

	//Created by Ian Mallett 11.9.14

	//Edits
	//Ian Mallett 11.9.14
	//Created arrow key support with bounds.
	//Changed code to work based on lookingPosition.
	//Added functionality for camera to rotate.
	//Added functionality for camera to move with mouse.

	//Ian Mallett 13.9.14
	//Added functionality for camera to zoom.

	//Ian Mallett 14.9.14
	//Removed the obselete rotationScaleFactor variable.
	//Renamed the maxZoomOffset variable to zoomCurvatureFactor

	//Ian Mallett 15.9.14
	//Added functionality to prevent the camera from creeping along
	//a boundary

	//Ian Mallett 1.11.14
	//Made the initial looking position variable, so that the camera can be centred on a specific point.

	//This class handles all the controls for the movement of the camera.
	//WASD or Arrow keys and moving the mouse to the edge of the game screen
	//will move the camera within a set of bounds, and Q and E will orbit
	//the camera around the spot it is currently looking at. The mouse also
	//zooms the camera in and out over the spot it is currently looking at

	public bool canZoom;
	public bool useInitialSize;
	public int scrollSpeed;
	public float zoomSpeed;
	public float mouseScrollFactor;
	public int rotationSpeed;
	public int boundsBuffer;
	public float minZoom;
	public float maxZoom;
	public float zoomCurvatureFactor;
	public int mouseMovementPadding;
	public Vector3 initialLookingPosition;
	public Map map;
	public MapBuilder builder;
	
	private Vector2 minBounds;
	private Vector2 maxBounds;
	private Vector2 mapSize;
	//The initial position of the camera is taken to be looking
	//at the position (0, 0)
	private Vector3 lateralOffset;
	private Vector3 verticalOffset;
	private Vector3 lookingPosition;
	private float rotatingToAngle;
	private float depression; //Angle of depression
	//Zoom factors
	private float translation;
	private float heightScaleFactor;


	void Start()
	{
		//Set the initial data
		lookingPosition = initialLookingPosition;
		lateralOffset = new Vector3 (gameObject.transform.position.x, 0, gameObject.transform.position.z) - lookingPosition;
		verticalOffset = new Vector3 (0, gameObject.transform.position.y, 0);
		rotatingToAngle = gameObject.transform.rotation.eulerAngles.y;
		depression = gameObject.transform.rotation.eulerAngles.x;

		if (useInitialSize)
		{
			mapSize = map.size;
			minBounds = new Vector2 (-boundsBuffer, -boundsBuffer);
			maxBounds = mapSize + new Vector2 (boundsBuffer, boundsBuffer);
		}
		else
		{
			minBounds = new Vector2 (-boundsBuffer, -boundsBuffer);
			maxBounds = new Vector2 (boundsBuffer, boundsBuffer);
		}

		if (canZoom)
		{
			//Calculate zoom factors
			translation = Mathf.Log (zoomCurvatureFactor * -lateralOffset.x);
			heightScaleFactor = verticalOffset.y / (translation - Mathf.Log (lateralOffset.magnitude));
		}
	}

	void Update()
	{
		//Rotate the camera using Q/E
		if (Input.GetKeyDown (KeyCode.Q))
		{
			rotatingToAngle = (rotatingToAngle + 45) % 360;
		}
		if (Input.GetKeyDown (KeyCode.E))
		{
			rotatingToAngle = (rotatingToAngle + 315) % 360;
		}



		//Move the camera based on WASD/Arrow key input
		//Find the movement amounts
		Vector3 movement = new Vector3();
		float horizMove = Input.GetAxisRaw ("Horizontal");
		float vertMove = Input.GetAxisRaw ("Vertical");

		if (Input.mousePosition.x < mouseMovementPadding)
		{
			horizMove -= mouseScrollFactor;
		}
		if (Input.mousePosition.x >= Screen.width - mouseMovementPadding)
		{
			horizMove += mouseScrollFactor;
		}
		if (Input.mousePosition.y < mouseMovementPadding)
		{
			vertMove -= mouseScrollFactor;
		}
		if (Input.mousePosition.y >= Screen.height - mouseMovementPadding)
		{
			vertMove += mouseScrollFactor;
		}
		

		//Move the looking position
		movement.x = horizMove * scrollSpeed * Time.deltaTime;
		movement.z = vertMove * scrollSpeed * Time.deltaTime;

		movement = Quaternion.AngleAxis (gameObject.transform.rotation.eulerAngles.y, Vector3.up) * movement;
		Vector3 forecastPosition = lookingPosition + movement;


		//If looking position and forecastPosition are both on the bounds,
		//and the movement angle is outside of 50 degrees of the boundary
		//edge don't move
		if (((Mathf.Abs (lookingPosition.x - maxBounds.x) < 0.01f) &&
		     (lookingPosition.x + movement.x - maxBounds.x > 0.01f) &&
		     (Mathf.Abs (Vector3.Angle (Vector3.right, movement) - 90) > 50.1f)) ||

		    ((Mathf.Abs (lookingPosition.z - maxBounds.y) < 0.01f) &&
			 (lookingPosition.z + movement.z - maxBounds.y > 0.01f) &&
			 (Mathf.Abs (Vector3.Angle (Vector3.forward, movement) - 90) > 50.1f)) ||
		    
		    ((Mathf.Abs (lookingPosition.x - minBounds.x) < 0.01f) &&
			 (lookingPosition.x + movement.x - minBounds.x < 0.01f) &&
			 (Mathf.Abs (Vector3.Angle (Vector3.left, movement) - 90) > 50.1f)) ||
		    
		    ((Mathf.Abs (lookingPosition.z - minBounds.y) < 0.01f) &&
			 (lookingPosition.z + movement.z - minBounds.y < 0.01f) &&
			 (Mathf.Abs (Vector3.Angle (Vector3.back, movement) - 90) > 50.1f)))
		{
			movement = new Vector3(0, 0, 0);
		}
		else
		{
			//Regulate position to within bounds
			if (forecastPosition.x > maxBounds.x)
			{
				movement.x -= forecastPosition.x - maxBounds.x;
			}
			if (forecastPosition.x < minBounds.x)
			{
				movement.x -= forecastPosition.x - minBounds.x;
			}
			if (forecastPosition.z > maxBounds.y)
			{
				movement.z -= forecastPosition.z - maxBounds.y;
			}
			if (forecastPosition.z < minBounds.y)
			{
				movement.z -= forecastPosition.z - minBounds.y;
			}
		}

		lookingPosition += movement;

		if (canZoom)
		{
			//Find the Up/Down movement
			float zoom = -Input.GetAxis ("Mouse ScrollWheel");
			float destinationHeight = Mathf.Clamp (gameObject.transform.position.y + zoom * zoomSpeed * Time.deltaTime, maxZoom, minZoom);
			float zoomChange = destinationHeight - gameObject.transform.position.y;


			//Move the camera
			verticalOffset += new Vector3 (0, zoomChange, 0);

			lateralOffset = lateralOffset * (Mathf.Exp (translation - (verticalOffset.y)/(heightScaleFactor)) /
								Mathf.Exp (translation - (verticalOffset.y + zoomChange)/(heightScaleFactor)));


			depression = (Mathf.Atan (verticalOffset.magnitude / lateralOffset.magnitude))*Mathf.Rad2Deg;
		}


		//Rotate the camera laterally
		Quaternion currentRotation = Quaternion.Euler (new Vector3(0, gameObject.transform.rotation.eulerAngles.y, 0));
		Quaternion targetRotation = Quaternion.Euler (new Vector3 (0, rotatingToAngle, 0));
		gameObject.transform.rotation = Quaternion.Euler(new Vector3(depression,
		                                                     		 Quaternion.RotateTowards (currentRotation,
		                          															   targetRotation, 
		                          															   rotationSpeed * Time.deltaTime).eulerAngles.y,
		                                                     		 0));


		lateralOffset = Quaternion.FromToRotation(-lateralOffset,
		                                          new Vector3(gameObject.transform.forward.x, 0, gameObject.transform.forward.z))
						* lateralOffset;

		gameObject.transform.position = new Vector3(lookingPosition.x + lateralOffset.x,
													lookingPosition.y + verticalOffset.y,
													lookingPosition.z + lateralOffset.z);
	}
}
