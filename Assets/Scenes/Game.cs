using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Game : PersistableObject {
	
	// singleton time
	public static Game Instance { get; private set; }
	// OnEnable better than Awake - happens each time after each component's Awake 
	// method unless the component was saved in a disabled state.
	void OnEnable () {
		Instance = this;
	}
	
	const int saveVersion = 2;

	[SerializeField] ShapeFactory shapeFactory;
	[SerializeField] PersistentStorage storage;
	[SerializeField] KeyCode createKey = KeyCode.C;
	[SerializeField] KeyCode newGameKey = KeyCode.N;
	[SerializeField] KeyCode saveKey = KeyCode.S;
	[SerializeField] KeyCode loadKey = KeyCode.L;
	[SerializeField] KeyCode destroyKey = KeyCode.X;
	public SpawnZone SpawnZoneOfLevel { get; set; }
	
	public float CreationSpeed { get; set; }
	public float DestructionSpeed { get; set; }
	[SerializeField] int levelCount;
	
	float creationProgress, destructionProgress;
	List<Shape> shapes;
	private string savePath;
	int loadedLevelBuildIndex;
	
	void Start () {
		shapes = new List<Shape>();
		Debug.Log(Application.isEditor);

		if (Application.isEditor)
		{
//			Scene loadedLevel = SceneManager.GetSceneByName("Level 1");
//			if (loadedLevel.isLoaded)
//			{
//				SceneManager.SetActiveScene(loadedLevel);
//				return;
//			}
			Debug.Log(SceneManager.sceneCount);
			for (int i = 0; i < SceneManager.sceneCount; i++) {
				Scene loadedScene = SceneManager.GetSceneAt(i);
				if (loadedScene.name.Contains("Level ") )
				{
					Debug.Log(loadedScene.name);
					SceneManager.SetActiveScene(loadedScene);
					loadedLevelBuildIndex = loadedScene.buildIndex;
					return;
				}
			}
		}

		StartCoroutine(LoadLevel(1));
	}
	
	void BeginNewGame()
	{
		for (int i = 0; i < shapes.Count; i++) {
			shapeFactory.Reclaim(shapes[i]);
		}
		shapes.Clear();
	}

	void CreateShape () {
		Shape instance = shapeFactory.GetRandom();
		Transform t = instance.transform;
		// t.localPosition = Random.insideUnitSphere * 5f;
		t.localPosition = SpawnZoneOfLevel.SpawnPoint;
		t.localRotation = Random.rotation;
		t.localScale = Vector3.one * Random.Range(0.1f, 1f);
		instance.SetColor(Random.ColorHSV(
			hueMin: 0f, hueMax: 1f,
			saturationMin: 0.5f, saturationMax: 1f,
			valueMin: 0.25f, valueMax: 1f,
			alphaMin: 1f, alphaMax: 1f
		));
		shapes.Add(instance);
	}
	
	
	
	void DestroyShape () {
		if (shapes.Count > 0)
		{
			int index = Random.Range(0, shapes.Count);
			// We must destroy the game object entirely, not just the shape component
			// Destroy(shapes[index].gameObject);
			
			// GC sucks in gaming life, recycle using pools
			shapeFactory.Reclaim(shapes[index]);
			
			// Since we care about speed, not order, we move the last element in and delete teh last index 
			int lastIndex = shapes.Count - 1;
			shapes[index] = shapes[lastIndex];
			shapes.RemoveAt(lastIndex);
		}

	}
	
	
	void Update () {
		if (Input.GetKeyDown(createKey)) {
			CreateShape();
		} else if (Input.GetKey(newGameKey)) {
			BeginNewGame();
		} else if (Input.GetKeyDown(saveKey)) {
			storage.Save(this, saveVersion);
		} else if (Input.GetKeyDown(loadKey)) {
			BeginNewGame();
			storage.Load(this);
		} else if (Input.GetKeyDown(destroyKey)) {
			DestroyShape();
		} else {
			for (int i = 1; i <= levelCount; i++) {
				if (Input.GetKeyDown(KeyCode.Alpha0 + i)) {
					BeginNewGame();
					StartCoroutine(LoadLevel(i));
					return;
				}
			}
		}
		
		creationProgress += Time.deltaTime * CreationSpeed;
		while (creationProgress >= 1f) {
			creationProgress -= 1f;
			CreateShape();
		}
		destructionProgress += Time.deltaTime * DestructionSpeed;
		while (destructionProgress >= 1f) {
			destructionProgress -= 1f;
			DestroyShape();
		}
	}
	
	public override void Save (GameDataWriter writer) {
		writer.Write(shapes.Count);
		writer.Write(loadedLevelBuildIndex);
		for (int i = 0; i < shapes.Count; i++) {
			writer.Write(shapes[i].ShapeId);
			writer.Write(shapes[i].MaterialId);
			shapes[i].Save(writer);
		}
	}
	
	public override void Load (GameDataReader reader) {
		int version = reader.Version;
		int count = version <= 0 ? -version : reader.ReadInt();
		StartCoroutine(LoadLevel(version < 2 ? 1 : reader.ReadInt()));
		for (int i = 0; i < count; i++) {
			int shapeId = reader.ReadInt();
			int materialId = version > 0 ? reader.ReadInt() : 0;
			Shape instance = shapeFactory.Get(shapeId, materialId);
			instance.Load(reader);
			shapes.Add(instance);
		}
	}
	/**
	 * Glorious hacks... We want to add an additive scene, but it
	 * has to be loaded first ie -
	 * so...
	 * yield return null
	 * 	return function to stack so it runs on next frame
	 *  so, the scene will be loaded on the next frame
	 * 
	 */
//	IEnumerator LoadLevel () {
//		SceneManager.LoadScene("Level 1", LoadSceneMode.Additive);
//		yield return null;
//		SceneManager.SetActiveScene(SceneManager.GetSceneByName("Level 1"));
//	}
	
	
	IEnumerator LoadLevel (int levelBuildIndex) {
		enabled = false;
		if (loadedLevelBuildIndex > 0) {
			yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
		}
		yield return SceneManager.LoadSceneAsync(
			levelBuildIndex, LoadSceneMode.Additive
		);
		SceneManager.SetActiveScene(
			SceneManager.GetSceneByBuildIndex(levelBuildIndex)
		);
		loadedLevelBuildIndex = levelBuildIndex;
		
		enabled = true;
	}

	
}