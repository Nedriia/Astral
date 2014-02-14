using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class LightManager {
	private static GameObject[] Lights;

	static LightManager() {
		Lights = GameObject.FindGameObjectsWithTag("Lights");
	}

	public static List<GameObject> getEncompassingLight (GameObject obj) {
		List<GameObject> isLit = new List<GameObject>();
		RaycastHit[] hits;
		RaycastHit hitRay;
		LayerMask mask = (1 << 9) | (1 << 10);
		float radius, currRadius, yPos;
		Vector3 tempPos, targetPos, tempDir = new Vector3();
		
		radius = currRadius = yPos = 0.0f;
		tempPos = tempDir = new Vector3();
		foreach (GameObject i in Lights) {
			if (i.light.enabled) {
				if (i.light.type == LightType.Spot) {
					yPos = i.transform.position.y * 1.1f;
					radius = i.light.spotAngle / (i.light.range - yPos);
					tempPos = i.transform.position;
				} else if (i.light.type == LightType.Point) {
					yPos = i.transform.position.y;
					radius = (i.light.range / yPos) * (i.light.intensity * 0.3f);
					tempPos = new Vector3(i.transform.position.x, i.transform.position.y + 30.0f, i.transform.position.z);
				}
				currRadius = Vector2.Distance(new Vector2(i.transform.position.x, i.transform.position.z),
				                              new Vector2(obj.transform.position.x, obj.transform.position.z));
				//Debug.Log("rad: " + radius + "cur: " + currRadius);

				if (currRadius <= radius)
				{
					hits = Physics.SphereCastAll(tempPos, radius, Vector3.down, Mathf.Infinity, mask.value);
					if (hits.Length > 0)
					{
						foreach (RaycastHit j in hits)
						{
							if (j.collider.gameObject.name == obj.name)
							{
								targetPos = new Vector3(j.transform.position.x, j.transform.position.y + 1.0f, j.transform.position.z);
								tempDir = targetPos - i.transform.position;
								if (Physics.Raycast(i.transform.position, tempDir, out hitRay, Mathf.Infinity, mask.value)) {
									//Debug.Log(hitRay.collider.gameObject.name);
								if (hitRay.collider.gameObject.name == obj.name)
								{
									isLit.Add(i);
									} }
							}
						}
					}
				}
			}
		}
		return isLit;
	}
}










