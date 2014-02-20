using UnityEngine;
using System.Collections;

public class FearMeter: MonoBehaviour {
	private static UISlider slider;

	public void Start () {
		slider = GetComponent<UISlider>();
	}

	public static void updateMeter (float val) {
		if (val > 1.0f)
			val = 1.0f;
		slider.foregroundWidget.transform.localScale = 
			new Vector3(val, slider.foregroundWidget.transform.localScale.y, 
			            slider.foregroundWidget.transform.localScale.z);
	}
}
