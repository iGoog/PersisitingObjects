using UnityEngine;
using System.Collections.Generic;

public class Game : PersistableObject {
	
	const int saveVersion = 1;

	public ShapeFactory shapeFactory;
	public PersistentStorage storage;
	public KeyCode createKey = KeyCode.C;
	public KeyCode newGameKey = KeyCode.N;
	public KeyCode saveKey = KeyCode.S;
	public KeyCode loadKey = KeyCode.L;
	
	List<Shape> shapes;
	private string savePath;
	
	void Awake () {
		shapes = new List<Shape>();
	}
	
	void BeginNewGame()
	{
		for (int i = 0; i < shapes.Count; i++) {
			Destroy(shapes[i].gameObject);
		}
		shapes.Clear();
	}

	void CreateShape () {
		Shape instance = shapeFactory.GetRandom();
		Transform t = instance.transform;
		t.localPosition = Random.insideUnitSphere * 5f;
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
		}
	}
	
	public override void Save (GameDataWriter writer) {
		writer.Write(shapes.Count);
		for (int i = 0; i < shapes.Count; i++) {
			writer.Write(shapes[i].ShapeId);
			writer.Write(shapes[i].MaterialId);
			shapes[i].Save(writer);
		}
	}
	
	public override void Load (GameDataReader reader) {
		int version = reader.Version;
		int count = version <= 0 ? -version : reader.ReadInt();
		for (int i = 0; i < count; i++) {
			int shapeId = reader.ReadInt();
			int materialId = version > 0 ? reader.ReadInt() : 0;
			Shape instance = shapeFactory.Get(shapeId, materialId);
			instance.Load(reader);
			shapes.Add(instance);
		}
	}
	

	
}