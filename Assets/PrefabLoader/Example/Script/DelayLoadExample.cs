using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class DelayLoadExample : MonoBehaviour {

	IEnumerator Start () {
		Vector3 cameraPos = Camera.main.transform.position;

		var loaderList = FindObjectsOfType<PrefabLoader> ()
			.OrderBy(l=>Vector3.SqrMagnitude(l.transform.position - cameraPos));
	
		foreach (var loader in loaderList) {
			yield return StartCoroutine(loader.InstantiatePrefabAllAsync ());
			yield return 0;
		}
	}
}
