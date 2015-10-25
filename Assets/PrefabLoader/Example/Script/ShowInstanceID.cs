using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ShowInstanceID : MonoBehaviour {

	void OnGUI () {
		Vector3 screenPos = Camera.main.WorldToScreenPoint (transform.position + Vector3.up);
		Rect rect = new Rect(screenPos.x, Screen.height - screenPos.y, 200.0f, 30.0f);
		GUI.Label (rect, "Instance ID = " + gameObject.GetInstanceID());
	}

	#if UNITY_EDITOR
	void OnDrawGizmos () {
		Handles.Label (transform.position + Vector3.up, "Instance ID = " + gameObject.GetInstanceID());
//		Handles.BeginGUI ();
//		Handles.EndGUI ();
	}
	#endif
}
