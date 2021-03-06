﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {
	private Tile playerTile;
	private Grid grid;
	private Vision vision;
	public Camera playerCam;
	public Sprite playerSprite;
	private bool rotate = true;
	private List<bool> movements;
	private bool hasLaser = false, shootLaser = false;
	private List<Vector3> rotations;

	public void StartUp(Tile p_playerTile, Grid p_grid) {
		movements = new List<bool>() {false, false, false, false}; 
		rotations = new List<Vector3>() {new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 180.0f), new Vector3(0.0f, 0.0f, -90.0f), new Vector3(0.0f, 0.0f, 90.0f)};
		playerTile = p_playerTile;
		grid = p_grid;
		playerTile.obj.GetComponent<SpriteRenderer>().sprite = playerSprite;
		vision = new Vision(grid, playerTile.obj.GetComponent<SpriteRenderer>().sprite);
		playerCam.GetComponent<CameraFollow>().SetTarget(playerTile.obj.GetComponent<Transform>());
		vision.CalculatePlayerVisibility(playerTile);
		playerTile.obj.GetComponent<SpriteRenderer>().color = Color.yellow;
	}

	void Update () {
		bool up = (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W));
		bool down = (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S));
		bool left = (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A));
		bool right = (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D));
		bool shift = Input.GetKey(KeyCode.LeftShift);
		List<bool> directions = new List<bool>() { right, left, down, up };
		if (shift && (right || left || down || up)) { // rotate instead of moving
//			for (int i = 0; i < 4; i++) {
//				movements[i] = false;
//			}
			for (int i = 0; i < 4; i++) {
				if (directions[i]) {
					rotate = true;
					playerTile.obj.GetComponent<Transform>().rotation = Quaternion.identity;
					playerTile.obj.GetComponent<Transform>().Rotate(rotations[i]);
					return;
				}
			}
		} else {
			if (up || down || left || right) {
				for (int i = 0; i < 4; i++) {
					if (directions[i]) {
						movements[i] = true;
						return;
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.Space))
				shootLaser = true;
		}
	}

	void FixedUpdate() {
		if (movements[0]) {
			if (PlayerMovementCheck('n')) {
				playerCam.GetComponent<CameraFollow>().SetTarget(playerTile.obj.GetComponent<Transform>());
			}
			movements[0] = false;
		} else if (movements[3]) {
			if (PlayerMovementCheck('e')) {
				playerCam.GetComponent<CameraFollow>().SetTarget(playerTile.obj.GetComponent<Transform>());
			}
			movements[3] = false;
		} else if (movements[2]) {
			if (PlayerMovementCheck('w')) {
				playerCam.GetComponent<CameraFollow>().SetTarget(playerTile.obj.GetComponent<Transform>());
			}
			movements[2] = false;
		} else if (movements[1]) {
			if (PlayerMovementCheck('s')) {
				playerCam.GetComponent<CameraFollow>().SetTarget(playerTile.obj.GetComponent<Transform>());
			}
			movements[1] = false;
		} else if (shootLaser && hasLaser) {
			ShootLaser();
			shootLaser = false;
			playerTile.obj.GetComponent<SpriteRenderer>().color = Color.yellow;
		}
	}

	private void ShootLaser()
	{
		hasLaser = false;
		print("Shooting laser");
	}
	
	private Tile PlayerMovement(Tile playerTile, Tile newPlayerTile)
	{
		playerTile.obj.GetComponent<Transform> ().rotation = Quaternion.identity;
		playerTile.obj.GetComponent<SpriteRenderer> ().sprite = playerTile.originalSprite;
		newPlayerTile.obj.GetComponent<SpriteRenderer> ().sprite = playerSprite;
		return newPlayerTile;
	}

	private bool pushedNorthBefore = false;
	private bool pushedSouthBefore = false;
	private bool pushedWestBefore = false;
	private bool pushedEastBefore = false;

	private bool PlayerMovementCheck(char direction)
	{
		bool pass = false;
		switch (direction) {
			case 'n':
				pass = NorthMovement();
				break;
			case 's':
				pass = SouthMovement();
				break;
			case 'e':
				pass = EastMovement();
				break;
			case 'w':
				pass = WestMovement();
				break;
			default:
				Debug.Log("Error, invalid direction supplied");
				break;
		}
		if (pass) {
			vision.CalculatePlayerVisibility(playerTile);
			CheckForLaser(playerTile);
			CheckForTroll(playerTile);
		}
		return pass;
	}

	private void CheckForLaser(Tile playerTile)
	{
		if (playerTile.isLaser && !hasLaser) {
			grid.CollectLaser(playerTile);
			hasLaser = true;
		}
		if (hasLaser) {
			playerTile.obj.GetComponent<SpriteRenderer>().color = Color.blue;
		} else {
			playerTile.obj.GetComponent<SpriteRenderer>().color = Color.yellow;
		}
	}

	private void CheckForTroll(Tile playerTile)
	{
		if (playerTile.isTroll) {
			lose();
		}
	}

	private void lose()
	{
		print("Trying to load");
		SceneManager.LoadScene("Resources/Scenes/New Main");
	}

	private bool NorthMovement()
	{
		bool pass = false;
		Tile possiblePushRecieveTile;

		pushedEastBefore = false;
		pushedWestBefore = false;
		pushedSouthBefore = false;

		if (!grid.GetNorthTile(playerTile).isWall) {
				pass = true;
				playerTile = PlayerMovement(playerTile, grid.GetNorthTile(playerTile));	
		} else {
			if (pushedNorthBefore) {
				if (!grid.GetNorthTile(playerTile).isEdge) {
					possiblePushRecieveTile = grid.GetNorthTile(grid.GetNorthTile(playerTile)); 
					if (!possiblePushRecieveTile.isWall) {
						pass = true;
						PushWall(possiblePushRecieveTile, grid.GetNorthTile(playerTile));
						playerTile = PlayerMovement(playerTile, grid.GetNorthTile(playerTile));
					}
				}
			} else {
				pushedNorthBefore = true;
			}
		}
		playerTile.obj.GetComponent<Transform>().rotation = Quaternion.identity;
		playerTile.obj.GetComponent<Transform>().Rotate(0.0f, 0.0f, 0.0f);
		return pass;
	}

	private bool SouthMovement()
	{
		bool pass = false;
		Tile possiblePushRecieveTile;

		pushedEastBefore = false;
		pushedWestBefore = false;
		pushedNorthBefore = false;

		if (!grid.GetSouthTile(playerTile).isWall) {
			pass = true;
			playerTile = PlayerMovement(playerTile, grid.GetSouthTile(playerTile));
		} else {
			if (pushedSouthBefore) {
				if (!grid.GetSouthTile(playerTile).isEdge) {
					possiblePushRecieveTile = grid.GetSouthTile(grid.GetSouthTile(playerTile));
					if (!possiblePushRecieveTile.isWall) {
						pass = true;
						PushWall(possiblePushRecieveTile, grid.GetSouthTile(playerTile));
						playerTile = PlayerMovement(playerTile, grid.GetSouthTile(playerTile));
					}
				}
			} else {
				pushedSouthBefore = true;
			}
		}
		playerTile.obj.GetComponent<Transform>().rotation = Quaternion.identity;
		playerTile.obj.GetComponent<Transform>().Rotate(0.0f, 0.0f, 180.0f);
		return pass;
	}

	private bool EastMovement()
	{
		bool pass = false;
		Tile possiblePushRecieveTile;

		pushedSouthBefore = false;
		pushedNorthBefore = false;
		pushedWestBefore = false;

		if (!grid.GetEastTile(playerTile).isWall) {
			pass = true;
			playerTile = PlayerMovement(playerTile, grid.GetEastTile(playerTile));
		} else {
			if (pushedEastBefore) {
				if (!grid.GetEastTile(playerTile).isEdge) {
					possiblePushRecieveTile = grid.GetEastTile(grid.GetEastTile(playerTile));
					if (!possiblePushRecieveTile.isWall) {
						pass = true;
						PushWall(possiblePushRecieveTile, grid.GetEastTile(playerTile));
						playerTile = PlayerMovement(playerTile, grid.GetEastTile(playerTile));
					}
				}
			} else {
				pushedEastBefore = true;
			}
		}
		playerTile.obj.GetComponent<Transform>().rotation = Quaternion.identity;
		playerTile.obj.GetComponent<Transform>().Rotate(0.0f, 0.0f, 90.0f);
		return pass;
	}

	private bool WestMovement()
	{
		bool pass = false;
		Tile possiblePushRecieveTile;

		pushedSouthBefore = false;
		pushedNorthBefore = false;
		pushedEastBefore = false;

		if (!grid.GetWestTile(playerTile).isWall) {
			pass = true;
			playerTile = PlayerMovement(playerTile, grid.GetWestTile(playerTile));
		} else {
			if (pushedWestBefore) {
				if (!grid.GetWestTile(playerTile).isEdge) {
					possiblePushRecieveTile = grid.GetWestTile(grid.GetWestTile(playerTile));
					if (!possiblePushRecieveTile.isWall) {
						pass = true;
						PushWall(possiblePushRecieveTile, grid.GetWestTile(playerTile));
						playerTile = PlayerMovement(playerTile, grid.GetWestTile(playerTile));
					}
				}
			} else {
				pushedWestBefore = true;
			}
		}
		playerTile.obj.GetComponent<Transform>().rotation = Quaternion.identity;
		playerTile.obj.GetComponent<Transform>().Rotate(0.0f, 0.0f, -90.0f);
		return pass;
	}

	void PushWall(Tile possiblePushRecieveTile, Tile newFloorTile)
	{
		grid.ChangeToWall(possiblePushRecieveTile);
		grid.ChangeToFloor(newFloorTile);
	}
}

