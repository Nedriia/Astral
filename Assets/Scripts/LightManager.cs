using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class LightManager {
	private static GameObject[] Lights;
	private static float floorPos;
	
	static LightManager() {
		Lights = GameObject.FindGameObjectsWithTag("Lights");
		floorPos = GameObject.Find ("floor").transform.position.y;
	}
	
	public static float getRadius (GameObject obj) {
		float radius, yPos;
		
		radius = yPos = 0.0f;
		if (obj.light.type == LightType.Spot) {
			yPos = obj.transform.position.y - floorPos;
			radius = (obj.light.spotAngle - 120) + (obj.light.range / yPos) * (obj.light.intensity / 0.4f);
			
		} else if (obj.light.type == LightType.Point) {
			yPos = obj.transform.position.y - floorPos;
			radius = (obj.light.range / (yPos)) * (obj.light.intensity / 3.5f);
		}
		return radius;
	}
	
	public static List<GameObject> getEncompassingLight (GameObject obj) {
		List<GameObject> isLit = new List<GameObject>();
		RaycastHit[] hits;
		RaycastHit hitRay;
		LayerMask mask = (1 << 9) | (1 << 10);
		float radius, currRadius;
		Vector3 tempPos, targetPos, tempDir = new Vector3();
		
		radius = currRadius = 0.0f;
		tempPos = tempDir = new Vector3();
		foreach (GameObject i in Lights) {
			if (i.light.enabled) {
				if (i.light.type == LightType.Spot) {
					tempPos = new Vector3(i.transform.position.x, i.transform.position.y + 30.0f, i.transform.position.z);
					
				} else if (i.light.type == LightType.Point) {
					tempPos = new Vector3(i.transform.position.x, i.transform.position.y + 30.0f, i.transform.position.z);
				}
				radius = getRadius (i);
				currRadius = Vector2.Distance(new Vector2(i.transform.position.x, i.transform.position.z),
				                              new Vector2(obj.transform.position.x, obj.transform.position.z));
				if (currRadius <= radius)
				{
					hits = Physics.SphereCastAll(tempPos, radius, Vector3.down, Mathf.Infinity, mask.value);
					Debug.DrawRay(tempPos, Vector3.down, Color.yellow);
					if (hits.Length > 0)
					{
						foreach (RaycastHit j in hits)
						{
							if (j.collider.gameObject.name == obj.name)
							{
								targetPos = new Vector3(j.transform.position.x, j.transform.position.y + 1.0f, j.transform.position.z);
								tempDir = targetPos - i.transform.position;
								if (Physics.Raycast(i.transform.position, tempDir, out hitRay, Mathf.Infinity, mask.value)) {
									if (hitRay.collider.gameObject.name == obj.name){
										isLit.Add(i);
									} 
								}
							}
						}
					}
				}
			}
		}
		return isLit;
	}
}
