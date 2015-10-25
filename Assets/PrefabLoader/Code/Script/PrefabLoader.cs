using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class PrefabLoader : MonoBehaviour {

	public const string AutoUpdateMenuPath = "Assets/PrefabLoader/Auto Update TransformData";
	public const string UpdateTransformDataAllMenuPath = "Assets/PrefabLoader/Update TransformData All";
	public const string InstantiateAllChildrenMenuPath = "Assets/PrefabLoader/Instantiate All Children";
	public const string RemoveAllChildrenMenuPath = "Assets/PrefabLoader/Remove All Children";

	public enum LoadMode {
		Reference = 0,
		ResourcesPath,
		AssetBundle,
	}

	public enum LoadTiming {
		OnAwake,
		OnStart,
		OnExternal,
	}

	[System.Serializable]
	public class PrefabInfo {
		[System.Serializable]
		public class TransformData {
			public Vector3 localPosition;
			public Quaternion localRotation;
			public Vector3 localScale;
			[SerializeField]
			internal bool isInit = false;

			public void Init () {
				localPosition = Vector3.zero;
				localRotation = Quaternion.identity;
				localScale = Vector3.one;
			}
		}

		public LoadMode loadMode;
		public GameObject prefab;
		public string path;
		public string name;

		public TransformData transformData;

		public GameObject instance;

		public string Name {
			get { 
				switch (loadMode) {
				case LoadMode.Reference:
					if (prefab) return prefab.name;
					break;
				case LoadMode.ResourcesPath:
					return System.IO.Path.GetFileName (path);
				case LoadMode.AssetBundle:
					return name;
				}
				return string.Empty;
			}
		}
	}

	public LoadTiming loadTiming;
	[SerializeField]
	public List<PrefabInfo> prefabInfoList;

	void Awake () {
		//Debug.Log ("OnEnable : Application.isPlaying = " + Application.isPlaying);
		if (!Application.isPlaying) {
			//Editor上で実行前にインスタンス化する.
			InstantiatePrefabAll ();
		} else {
			if (loadTiming == LoadTiming.OnAwake) {
				//通常実行時のロード.
				InstantiatePrefabAllAsync ();
			}
		}
	}

	void Start () {
		if (Application.isPlaying && loadTiming == LoadTiming.OnStart) {
			InstantiatePrefabAllAsync ();
		}
	}

	public void InstantiatePrefabAll () {
		if (prefabInfoList == null) return;
		for(int i=0; i<prefabInfoList.Count; i++) {
			InstantiatePrefabAtIndex (i);
		}
	}

	public void InstantiatePrefab (string name) {
		if (prefabInfoList == null) return;
		int index = prefabInfoList.FindIndex (info => info.Name == name);
		if(0 < index && index < prefabInfoList.Count) {
			InstantiatePrefabAtIndex (index);
		}else {
			Debug.LogError(name + "is not found in PrefabInfoList");
		}
	}

	public void InstantiatePrefabAtIndex (int index) {
		if (prefabInfoList == null) return;
		PrefabInfo info = prefabInfoList [index];
		if (info.instance != null) return;

		GameObject resource = null;
		switch (info.loadMode) {
		case LoadMode.Reference:
			resource = info.prefab;
			break;
		case LoadMode.ResourcesPath:
			resource = Resources.Load<GameObject>(info.path);
			break;
		case LoadMode.AssetBundle:
			//TODO
			//AssetBundleManagerを作ってから.
			throw new System.NotImplementedException ();
			break;
		}

		InstantiatePrefabInternal (resource, ref info);
	}


	public IEnumerator InstantiatePrefabAllAsync () {
		if (prefabInfoList == null) yield break;
		List<Coroutine> coroutineList = new List<Coroutine> ();
		for(int i=0; i<prefabInfoList.Count; i++) {
			coroutineList.Add( StartCoroutine(InstantiatePrefabAtIndexAsync (i)));
		}
		foreach (var coroutine in coroutineList) {
			yield return coroutine;
		}
	}

	public IEnumerator InstantiatePrefabAsync (string name) {
		if (prefabInfoList == null) yield break;
		int index = prefabInfoList.FindIndex (info => info.Name == name);
		if(0 < index && index < prefabInfoList.Count) {
			yield return StartCoroutine(InstantiatePrefabAtIndexAsync (index));
		}else {
			Debug.LogError(name + "is not found in PrefabInfoList");
		}
		yield break;
	}

	public IEnumerator InstantiatePrefabAtIndexAsync (int index) {
		if (prefabInfoList == null) yield break;
		PrefabInfo info = prefabInfoList [index];

		GameObject resource = null;
		switch (info.loadMode) {
		case LoadMode.Reference:
			resource = info.prefab;
			break;
		case LoadMode.ResourcesPath:
			ResourceRequest request = Resources.LoadAsync<GameObject> (info.path);
			yield return request;
			resource = request.asset as GameObject;
			break;
		case LoadMode.AssetBundle:
			//TODO
			//AssetBundleManagerを作ってから.
			break;
		}

		InstantiatePrefabInternal (resource, ref info);
		yield break;
	}

	public void RemoveInstantiatedPrefabAll () {
		if (prefabInfoList == null) return;
		for(int i=0; i<prefabInfoList.Count; i++) {
			RemoveInstantiatedPrefabAtIndex (i);
		}
	}

	public void RemoveInstantiatedPrefabAtIndex (int index) {
		if (prefabInfoList == null) return;
		PrefabInfo info = prefabInfoList[index];

		if (info.instance) {
			if (Application.isPlaying) {
				Destroy (info.instance);
			} else {
				DestroyImmediate (info.instance);
			}
		}
	}

	//子オブジェクトの配置データを更新.
	public void UpdateTransformDataAll () {
		if (prefabInfoList == null) return;
		for(int i=0; i<prefabInfoList.Count; i++) {
			UpdateTransformDataAtIndex (i);
		}
	}

	public void UpdateTransformDataAtIndex (int index) {
		if (prefabInfoList == null) return;
		PrefabInfo info = prefabInfoList[index];

		if (info.instance) {
			if (Application.isPlaying) return;
			#if UNITY_EDITOR
			Transform trans = info.instance.transform;
			info.transformData.isInit = true;
			info.transformData.localPosition = trans.localPosition;
			info.transformData.localRotation = trans.localRotation;
			info.transformData.localScale = trans.localScale;
			EditorUtility.SetDirty(this);
			#endif
		}
	}

	public bool IsNeedUpdateTransformData () {
		if (prefabInfoList == null) return false;
		return prefabInfoList
			.Where(info => info.instance != null)
			.Any (info => (info.instance.transform.localPosition != info.transformData.localPosition)
				|| (info.instance.transform.localRotation != info.transformData.localRotation)
				|| (info.instance.transform.localScale != info.transformData.localScale));
	}

	private void InstantiatePrefabInternal (GameObject resource, ref PrefabInfo info) {
		if (resource != null) {	
			GameObject inst = null;
			if (Application.isPlaying) {
				inst = Instantiate (resource) as GameObject;
				inst.name = inst.name.Replace ("(Clone)", "");
			} else {
				#if UNITY_EDITOR
				inst = PrefabUtility.InstantiatePrefab(resource) as GameObject;
				inst.name += "(Preview)";
				inst.hideFlags = HideFlags.DontSave;
				#endif
			}

			Transform trans = inst.transform;
			trans.SetParent (this.transform);
			if (!info.transformData.isInit) {
				info.transformData.Init ();
				info.transformData.isInit = true;
			}
			trans.localPosition = info.transformData.localPosition;
			trans.localRotation = info.transformData.localRotation;
			trans.localScale = info.transformData.localScale;

			info.instance = inst;

			#if UNITY_EDITOR
			EditorUtility.SetDirty(this);
			#endif
		}
	}

	#if UNITY_EDITOR
	public void EditorExecInstantiate () {
		this.InstantiatePrefabAll ();
	}
	public void EditorExecUpdateTransformData () {
		this.UpdateTransformDataAll();
	}
	public void EditorExecRemove () {
		this.RemoveInstantiatedPrefabAll ();
	}
	public void EditorExecSelect () {
		Selection.activeObject = PrefabUtility.GetPrefabParent (gameObject);
	}
	public void EditorExecRevert () {
		PrefabUtility.RevertPrefabInstance (this.gameObject);
	}
	public void EditorExecApply () {
		//一旦全ての生成した子プレバブを削除
		this.RemoveInstantiatedPrefabAll ();
		Object prefab = PrefabUtility.GetPrefabParent (gameObject);
		PrefabUtility.ReplacePrefab (gameObject, prefab);
		PrefabUtility.RevertPrefabInstance (gameObject);
	}

	void Update () {
		if (Menu.GetChecked (AutoUpdateMenuPath)) {
			EditorExecUpdateTransformData ();
			//EditorUtility.SetDirty (this);
		}
	}

	static PrefabLoader () {
		bool check = EditorPrefs.GetBool ("PrefabLoader_AutoUpdateMenuPath", true);
		Menu.SetChecked (AutoUpdateMenuPath, check);
		Debug.Log ("test : " + check);
	}

	[MenuItem(AutoUpdateMenuPath)]
	public static void MenuAutoUpdateTransformData () {
		bool check = EditorPrefs.GetBool ("PrefabLoader_AutoUpdateMenuPath", true);
		Menu.SetChecked (AutoUpdateMenuPath, !check);
		EditorPrefs.SetBool ("PrefabLoader_AutoUpdateMenuPath", !check);
	}

	[MenuItem(UpdateTransformDataAllMenuPath)]
	public static void UpdateTransformDataAllMenu () {
		foreach(var loader in FindObjectsOfType<PrefabLoader>()) {
			loader.EditorExecUpdateTransformData ();
			//EditorUtility.SetDirty (loader);
		}
	}

	[MenuItem(InstantiateAllChildrenMenuPath)]
	public static void InstantiateAllChildrenMenu () {
		foreach(var loader in FindObjectsOfType<PrefabLoader>()) {
			loader.EditorExecInstantiate ();
		}
	}

	[MenuItem(RemoveAllChildrenMenuPath)]
	public static void RemoveAllChildrenMenu () {
		foreach(var loader in FindObjectsOfType<PrefabLoader>()) {
			loader.EditorExecRemove ();
		}
	}

	#endif
}