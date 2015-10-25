using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;


[CustomEditor(typeof(PrefabLoader))]
public class PrefabLoaderEditor : Editor {

	SerializedProperty loadTiming;
	SerializedProperty infoList;

	UnityEditorInternal.ReorderableList reorderableList;

	void OnEnable () {
		//		PrefabLoader loader = target as PrefabLoader;
		loadTiming = serializedObject.FindProperty ("loadTiming");
		infoList = serializedObject.FindProperty ("prefabInfoList");

		reorderableList = new UnityEditorInternal.ReorderableList (serializedObject, infoList, true, false, true, true);

		reorderableList.elementHeight = (EditorGUIUtility.singleLineHeight + 4f) * 2f;

		reorderableList.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "PrefabInfoList");
		};
		reorderableList.drawElementCallback = delegate(Rect position, int index, bool isActive, bool isFocused) {
			//EditorGUI.PropertyField(rect, , true);
			SerializedProperty property = infoList.GetArrayElementAtIndex(index);

			position.height = EditorGUIUtility.singleLineHeight;
			position.y += 2;
			Rect rect = position;

			switch (property.FindPropertyRelative ("loadMode").enumValueIndex) {
			case (int)PrefabLoader.LoadMode.Reference:
				EditorGUI.PropertyField (position, property.FindPropertyRelative ("loadMode"), new GUIContent("LoadMode"));
				rect.y += EditorGUIUtility.singleLineHeight + 2.0f;
				EditorGUI.PropertyField (rect, property.FindPropertyRelative ("prefab"), new GUIContent("Prefab"));
				break;
			case (int)PrefabLoader.LoadMode.ResourcesPath:
				EditorGUI.PropertyField (position, property.FindPropertyRelative ("loadMode"), new GUIContent("LoadMode"));
				rect.y += EditorGUIUtility.singleLineHeight + 2.0f;
				EditorGUI.PropertyField (rect, property.FindPropertyRelative ("path"), new GUIContent("Resources Path"));
				break;
			case (int)PrefabLoader.LoadMode.AssetBundle:
				position.width = EditorGUIUtility.labelWidth - 2.0f;
				EditorGUI.PropertyField (position, property.FindPropertyRelative ("loadMode"), GUIContent.none);
				position.x += position.width + 2.0f;
				position.width = rect.width - EditorGUIUtility.labelWidth;
				EditorGUI.LabelField(position, "Name");
				position.x += 40.0f;
				position.width -= 40.0f;
				EditorGUI.PropertyField (position, property.FindPropertyRelative ("name"), GUIContent.none);
				rect.y += EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField (rect, property.FindPropertyRelative ("path"), new GUIContent("AssetBundle Path"));
				break;
			}
		};
	}

	public override void OnInspectorGUI () {
		serializedObject.Update ();

		PrefabLoader loader = target as PrefabLoader;

		PrefabType prefabType = PrefabUtility.GetPrefabType (loader.gameObject);

		if (prefabType == PrefabType.PrefabInstance) {
			GUILayout.Label ("Prefabの管理 (更新/復元はこのボタンで必ずすること)");
			using (var scopeH = new GUILayout.HorizontalScope ()) {
				if (GUILayout.Button ("Select", EditorStyles.miniButtonLeft)) {
					loader.EditorExecSelect ();
				}
				if (GUILayout.Button ("Revert", EditorStyles.miniButtonMid)) {
					loader.EditorExecRevert ();
				}
				if (GUILayout.Button ("Apply", EditorStyles.miniButtonRight)) {
					loader.EditorExecApply ();
					loader.EditorExecInstantiate ();
				}
			}
		}
		if (prefabType != PrefabType.Prefab) {
			using (var scopeH = new GUILayout.HorizontalScope ()) {
				if (GUILayout.Button ("Instantiate", EditorStyles.miniButtonLeft)) {
					loader.EditorExecInstantiate ();
				}
				if (GUILayout.Button ("Update TransformData", EditorStyles.miniButtonMid)) {
					loader.EditorExecUpdateTransformData ();
				}
				if (GUILayout.Button ("Remove", EditorStyles.miniButtonRight)) {
					loader.EditorExecRemove ();
				}
			}
		}
			
		if (loader.IsNeedUpdateTransformData() && !Application.isPlaying) {
			EditorGUILayout.HelpBox (
				"子プレハブのTransform情報がシリアライズされていません\n" +
				"Update TransformDataで更新してください."
				, MessageType.Info);
		}

		GUILayout.Space (5f);

		EditorGUILayout.PropertyField (loadTiming, true);

		reorderableList.DoLayoutList ();

		serializedObject.ApplyModifiedProperties();
		//EditorGUILayout.PropertyField (info, true);
	}
}
