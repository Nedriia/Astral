using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ScenarioManager : MonoBehaviour {
    public GameObject portrait;
    public GameObject moveWall;
    public GameObject fakeGeneratorLights, realGeneratorLights;
    public GameObject bars;
    public GameObject tortureLights;

    public Transform portraitTo, wallTo, moveToBars;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.Alpha1)) {
            HOTween.To(portrait.transform, 0.5f, "position", portraitTo.position);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2)) {
            HOTween.To(moveWall.transform, 0.5f, "position", wallTo.position);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3)) {
            fakeGeneratorLights.SetActive(true);
            realGeneratorLights.SetActive(false);
        }
        if (Input.GetKeyUp(KeyCode.Alpha4)) {
            PossessionMaster.CurrentlyPossesing.Die();
            HOTween.To(bars.transform, 0.5f, "position", moveToBars.position);
            //tortureLights.SetActive(true);
        }
	}

}
