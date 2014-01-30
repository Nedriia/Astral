using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//The inventory for each Prisoner/Body
//The inventory holds a maximum number of items. We work with null values so the player can see blank slots in the inventory
public class Inventory : MonoBehaviour {
	public int maxSize;
	
	private GameObject[] items;
	
	//Use this for initialization
	private void Start()  {	
		setInventorySize(maxSize);
	}
	
	//Sets the size of the inventory
	private void setInventorySize(int size) {
		items = new GameObject[maxSize];
	}
	
	//Adds an item to the inventory in the first available slot, starting from the front of the list
	public void add(GameObject item) {
		//Check if there are any empty slots and place the item in the one you find
		int emptyslot = getFirstEmptySlot();
		
		//If found, assign the item to that slot
		if (emptyslot >= 0)
			items[emptyslot] = item;
	}
	
	//Removes an item by index
	public void remove(int item) {
		//Make sure the index specified is within the array bounds
		if (item >= 0 && item < items.Length)
			items[item] = null;
	}
	
	
	//Removes a particular instance of an item
	public void remove(GameObject item) {
		for (int i = (items.Length - 1); i >= 0; i--) {
			if (items[i] != null) {
				//Get the reference of the object and compare it reference of the object specified in the parameter
				if (items[i] == item) {
					items[i] = null;
					break;
				}
			}
		}
	}
	
	//Finds the first empty slot in the inventory, starting from the front of the list
	public int getFirstEmptySlot() {
		for (int i = 0; i < items.Length; i++) {
			//A null value indicates an empty slot
			if (items[i] == null) return i;
		}
		
		return -1;
	}
	
	//Finds the first non-empty slot in the inventory, starting from the front of the list
	public int getFirstFilledSlot() {
		for (int i = 0; i < items.Length; i++) {
			//A null value indicates an empty slot
			if (items[i] != null) return i;
		}
		
		return -1;
	}
	
	//Finds the first empty slot in the inventory, starting from the end of the list
	public int getLastEmptySlot() {
		for (int i = (items.Length - 1); i >= 0; i--) {
			//A null value indicates an empty slot
			if (items[i] == null) return i;
		}
		
		return -1;
	}
	
	//Finds the first non-empty slot in the inventory, starting from the end of the list
	public int getLastFilledSlot() {
		for (int i = (items.Length - 1); i >= 0; i--) {
			//A null value indicates an empty slot
			if (items[i] != null) return i;
		}
		
		return -1;
	}
	
	//Checks if the inventory contains a particular item tag, starting from the front of the list
	public bool contains(GameObject item) {
		for (int i = 0; i < items.Length; i++) {
			if (items[i] != null) {
				//Get the tag of the item and compare it with the tag of the GameObject passed as a parameter
				if (items[i].tag == item.tag)
					return true;
			}
		}
		
		return false;
	}
	
	//Checks if the inventory is empty or not
	public bool isEmpty() {
		for (int i = 0; i < items.Length; i++) {
			if (items[i] != null) return false;
		}
		
		return true;
	}
	
	//Checks if the inventory is full or not
	public bool isFull() {
		return (getFirstEmptySlot() == -1);
	}
	
	//TEST - adding items
	private void OnTriggerEnter(Collider other) {
		if (isFull() == false && contains(other.gameObject) == false) {
			other.gameObject.SetActive(false);
			add(other.gameObject);
		}
	}
	
	//Update is called once per frame
	private void Update() {
		//TEST - removing items
		if (Input.GetKeyDown(KeyCode.R) == true) {
			if (isEmpty() == false) {
				remove(getLastFilledSlot());
			}
		}
	}
}
