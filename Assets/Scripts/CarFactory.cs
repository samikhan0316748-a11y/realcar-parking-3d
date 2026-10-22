using UnityEngine;

public static class CarFactory {
  static Material baseMat;
  public static readonly Color[] Paints = {
    new Color(.85f,.1f,.1f), new Color(.1f,.3f,.9f), new Color(.95f,.95f,.95f), new Color(.08f,.08f,.08f),
    new Color(.95f,.75f,.1f), new Color(.1f,.7f,.3f), new Color(.9f,.4f,.05f), new Color(.5f,.5f,.55f) };
  static readonly Color[] Rims = { new Color(.6f,.6f,.65f), new Color(.92f,.92f,.97f), new Color(.9f,.7f,.2f), new Color(.1f,.1f,.1f) };

  public static Material Mat(Color c) {
    if (baseMat == null) { var t = GameObject.CreatePrimitive(PrimitiveType.Cube); baseMat = t.GetComponent<Renderer>().sharedMaterial; Object.Destroy(t); }
    var m = new Material(baseMat); m.color = c; return m;
  }

  public static GameObject Prim(PrimitiveType t, Transform p, string n, Vector3 lp, Vector3 ls, Color c, bool col) {
    var g = GameObject.CreatePrimitive(t); g.name = n; g.transform.SetParent(p, false);
    g.transform.localPosition = lp; g.transform.localScale = ls;
    g.GetComponent<Renderer>().sharedMaterial = Mat(c);
    if (!col) Object.Destroy(g.GetComponent<Collider>());
    return g;
  }

  public static Vector3 WheelPos(CarStats s, int i) {
    return new Vector3((i % 2 == 0 ? -1 : 1) * (s.body.x / 2f - 0.08f), 0.45f, (i < 2 ? 1 : -1) * s.body.z * 0.32f);
  }

  // Builds the visible car (placeholder shapes, or Resources/Cars/<id> prefab if you added a real model). Returns the 4 wheel mesh holders.
  public static Transform[] BuildVisual(Transform root, CarStats s, Color paint, int wheelStyle) {
    var prefab = Resources.Load<GameObject>("Cars/" + s.id);
    if (prefab != null) {
      var m = Object.Instantiate(prefab, root); m.name = "Model";
      m.transform.localPosition = Vector3.zero; m.transform.localRotation = Quaternion.identity;
      foreach (var c in m.GetComponentsInChildren<Collider>()) Object.Destroy(c);
      foreach (var r in m.GetComponentsInChildren<Renderer>())
        foreach (var mt in r.materials) { string n = mt.name.ToLower(); if (n.Contains("paint") || n.Contains("body")) mt.color = paint; }
    } else {
      float by = 0.45f + s.body.y / 2f;
      CarFactory.Prim(PrimitiveType.Cube, root, "Body", new Vector3(0, by, 0), s.body, paint, false);
      float cy = 0.45f + s.body.y + s.cabin.y / 2f;
      Prim(PrimitiveType.Cube, root, "Cabin", new Vector3(0, cy, s.cabinZ), s.cabin, new Color(.1f, .14f, .2f), false);
      Prim(PrimitiveType.Cube, root, "Roof", new Vector3(0, cy + s.cabin.y / 2f, s.cabinZ), new Vector3(s.cabin.x, .06f, s.cabin.z), paint, false);
      Prim(PrimitiveType.Cube, root, "HeadL", new Vector3(-s.body.x * .3f, by, s.body.z / 2f), new Vector3(.35f, .12f, .05f), Color.white, false);
      Prim(PrimitiveType.Cube, root, "HeadR", new Vector3(s.body.x * .3f, by, s.body.z / 2f), new Vector3(.35f, .12f, .05f), Color.white, false);
      Prim(PrimitiveType.Cube, root, "TailL", new Vector3(-s.body.x * .3f, by, -s.body.z / 2f), new Vector3(.35f, .12f, .05f), Color.red, false);
      Prim(PrimitiveType.Cube, root, "TailR", new Vector3(s.body.x * .3f, by, -s.body.z / 2f), new Vector3(.35f, .12f, .05f), Color.red, false);
      if (s.id == "sports" || s.id == "super" || s.id == "muscle")
        Prim(PrimitiveType.Cube, root, "Spoiler", new Vector3(0, 0.45f + s.body.y + .25f, -s.body.z / 2f + .25f), new Vector3(s.body.x * .85f, .05f, .35f), paint * .6f, false);
      if (s.id == "pickup")
        Prim(PrimitiveType.Cube, root, "Bed", new Vector3(0, 0.45f + s.body.y + .12f, -1.55f), new Vector3(s.body.x * .95f, .25f, 1.9f), paint * .8f, false);
      if (s.id == "jeep")
        Prim(PrimitiveType.Cube, root, "Spare", new Vector3(0, 0.45f + s.body.y * .6f, -s.body.z / 2f - .15f), new Vector3(.8f, .8f, .25f), new Color(.1f, .1f, .1f), false);
    }
    var wm = new Transform[4];
    for (int i = 0; i < 4; i++) {
      var h = new GameObject("W" + i).transform; h.SetParent(root, false); h.localPosition = WheelPos(s, i);
      var tire = Prim(PrimitiveType.Cylinder, h, "Tire", Vector3.zero, new Vector3(.7f, .12f, .7f), new Color(.06f, .06f, .06f), false);
      tire.transform.localRotation = Quaternion.Euler(0, 0, 90);
      var rim = Prim(PrimitiveType.Cylinder, h, "Rim", Vector3.zero, new Vector3(.44f, .13f, .44f), Rims[Mathf.Clamp(wheelStyle, 0, 3)], false);
      rim.transform.localRotation = Quaternion.Euler(0, 0, 90);
      wm[i] = h;
    }
    return wm;
  }

  // Non-moving car (parked cars, traffic).
  public static GameObject Static(CarStats s, Transform parent, Vector3 pos, float yaw, Color paint) {
    var g = new GameObject("Parked"); g.transform.SetParent(parent, false);
    g.transform.localPosition = pos; g.transform.localRotation = Quaternion.Euler(0, yaw, 0);
    var bc = g.AddComponent<BoxCollider>(); bc.size = new Vector3(s.body.x, s.body.y + .35f, s.body.z); bc.center = new Vector3(0, .1f + (s.body.y + .35f) / 2f, 0);
    BuildVisual(g.transform, s, paint, 0);
    return g;
  }

  // The drivable player car (physics). physics=false gives a display model for the garage.
  public static CarController Spawn(CarStats s, Vector3 pos, Quaternion rot, bool player, Color paint, int wheelStyle, bool physics = true) {
    var root = new GameObject("Car_" + s.id); root.transform.SetPositionAndRotation(pos, rot);
    var rb = root.AddComponent<Rigidbody>(); rb.mass = s.mass; rb.centerOfMass = new Vector3(0, 0.4f, 0);
    rb.interpolation = RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    var bc = root.AddComponent<BoxCollider>(); bc.size = s.body; bc.center = new Vector3(0, 0.45f + s.body.y / 2f, 0);
    var meshes = BuildVisual(root.transform, s, paint, wheelStyle);
    var lg = new GameObject("Head"); lg.transform.SetParent(root.transform, false); lg.transform.localPosition = new Vector3(0, .9f, s.body.z / 2f);
    var head = lg.AddComponent<Light>(); head.type = LightType.Spot; head.range = 24; head.spotAngle = 75; head.intensity = 3; head.enabled = false;
    var sv = SaveManager.Car(s.id);
    var wcs = new WheelCollider[4];
    for (int i = 0; i < 4; i++) {
      var g = new GameObject("WC" + i); g.transform.SetParent(root.transform, false); g.transform.localPosition = WheelPos(s, i);
      var w = g.AddComponent<WheelCollider>(); w.radius = .35f; w.mass = 20; w.suspensionDistance = .25f;
      var js = w.suspensionSpring; js.spring = s.mass / 4f * 9.81f / .12f * (1f + .1f * sv.suspension); js.damper = js.spring * .14f; js.targetPosition = .5f; w.suspensionSpring = js;
      var ff = w.forwardFriction; ff.stiffness = 2f; w.forwardFriction = ff;
      var sf = w.sidewaysFriction; sf.stiffness = 2.2f; w.sidewaysFriction = sf;
      wcs[i] = w;
    }
    var cc = root.AddComponent<CarController>(); cc.Init(s, wcs, meshes, player, head);
    if (!physics) { rb.isKinematic = true; foreach (var w in wcs) w.enabled = false; }
    return cc;
  }
}
