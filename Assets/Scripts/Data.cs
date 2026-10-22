using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class CarSave { public string id; public int engine, brakes, steering, suspension, wheel; public float r = 0.85f, g = 0.1f, b = 0.1f; }

[Serializable] public class SaveData {
  public int coins = 1500;
  public string selected = "sedan";
  public List<string> unlocked = new List<string> { "sedan" };
  public List<CarSave> cars = new List<CarSave>();
  public int[] stars = new int[100];
  public int controlMode = 0; // 0 wheel, 1 tilt, 2 buttons
  public float steerSens = 1f, camSens = 1f, volume = 1f;
  public bool manual, tiltInvert, music = true;
}

public static class SaveManager {
  const string KEY = "RCP3D_SAVE_V1";
  public static SaveData D;
  static SaveManager() { Load(); }
  public static void Load() {
    string j = PlayerPrefs.GetString(KEY, "");
    D = string.IsNullOrEmpty(j) ? new SaveData() : JsonUtility.FromJson<SaveData>(j);
    if (D == null) D = new SaveData();
    if (D.stars == null || D.stars.Length < 100) Array.Resize(ref D.stars, 100);
  }
  public static void Save() { PlayerPrefs.SetString(KEY, JsonUtility.ToJson(D)); PlayerPrefs.Save(); }
  public static CarSave Car(string id) {
    var c = D.cars.Find(x => x.id == id);
    if (c == null) { c = new CarSave { id = id }; D.cars.Add(c); }
    return c;
  }
  public static bool Owned(string id) { return D.unlocked.Contains(id); }
}

public class CarStats {
  public string id, name; public int price;
  public float torque, maxKmh, brake, steer, mass;
  public Vector3 body, cabin; public float cabinZ;
}

public static class CarCatalog {
  static CarStats N(string id, string name, int price, float tq, float kmh, float br, float st, float mass, Vector3 body, Vector3 cabin, float cz) {
    return new CarStats { id = id, name = name, price = price, torque = tq, maxKmh = kmh, brake = br, steer = st, mass = mass, body = body, cabin = cabin, cabinZ = cz };
  }
  public static readonly CarStats[] All = {
    N("sedan","Sedan",0,900,140,3000,32,1300,new Vector3(1.8f,.6f,4.5f),new Vector3(1.6f,.5f,2.2f),-.2f),
    N("sports","Sports Car",4000,1100,190,3400,30,1200,new Vector3(1.85f,.5f,4.3f),new Vector3(1.55f,.42f,1.8f),-.1f),
    N("super","Supercar",12000,1400,240,3800,28,1300,new Vector3(2f,.42f,4.6f),new Vector3(1.6f,.38f,1.7f),0f),
    N("suv","SUV",5000,1000,150,3200,30,1900,new Vector3(2f,.85f,4.7f),new Vector3(1.9f,.7f,2.8f),-.3f),
    N("muscle","Muscle Car",7000,1500,200,3300,29,1700,new Vector3(1.95f,.6f,4.9f),new Vector3(1.6f,.5f,2f),-.5f),
    N("luxury","Luxury Car",9000,1000,170,3500,31,2100,new Vector3(1.95f,.65f,5.1f),new Vector3(1.7f,.55f,2.7f),-.3f),
    N("hatch","Hatchback",1500,800,130,2800,36,1100,new Vector3(1.75f,.65f,3.9f),new Vector3(1.6f,.6f,2f),-.4f),
    N("pickup","Pickup Truck",6000,1200,150,3300,28,2200,new Vector3(2f,.8f,5.4f),new Vector3(1.9f,.7f,1.9f),1f),
    N("jeep","Off-road Jeep",8000,1300,130,3300,30,1900,new Vector3(1.9f,1f,4f),new Vector3(1.8f,.8f,2.4f),-.2f),
  };
  public static CarStats Get(string id) { foreach (var c in All) if (c.id == id) return c; return All[0]; }
  public static int Index(string id) { for (int i = 0; i < All.Length; i++) if (All[i].id == id) return i; return 0; }
}
