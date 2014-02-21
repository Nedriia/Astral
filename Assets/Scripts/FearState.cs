using UnityEngine;
using System.Collections;

public class FearState : MonoBehaviour {
	private static UILabel textBox;

	void Start () {
		textBox = GetComponent<UILabel>();
	}

	public static void updateFearState (string val) {
		textBox.text = val;
	}
}
