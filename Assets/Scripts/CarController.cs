using UnityEngine;

public enum Gear { P, R, N, D }

public class CarController : MonoBehaviour {
  public CarStats S; public bool player, locked; public Gear gear = Gear.P; public int manualGear = 1; public float nitro = 1f;
  public bool braking, nitroOn;
  WheelCollider[] wc; Transform[] wm; Rigidbody rb; Light head;
  float tqMul = 1, brMul = 1, stMul = 1, spdMul = 1, steerCur;

  public Vector2 Footprint { get { return new Vector2(S.body.x, S.body.z); } }
  public float Speed { get { return rb.velocity.magnitude * 3.6f; } }
  public float FwdSpeed { get { return Vector3.Dot(rb.velocity, transform.forward) * 3.6f; } }

  public void Init(CarStats s, WheelCollider[] w, Transform[] m, bool pl, Light l) {
    S = s; wc = w; wm = m; player = pl; head = l; rb = GetComponent<Rigidbody>();
    var sv = SaveManager.Car(s.id);
    tqMul = 1f + .10f * sv.engine; spdMul = 1f + .04f * sv.engine; brMul = 1f + .12f * sv.brakes; stMul = 1f + .04f * sv.steering;
  }
  public void SetLights(bool on) { if (head) head.enabled = on; }
  public void SetGear(Gear g) { gear = g; }
  public void ShiftManual(int d) { manualGear = Mathf.Clamp(manualGear + d, 1, 5); }

  void FixedUpdate() {
    if (!player || wc == null || !wc[0].enabled) return;
    var inp = GameInput.I; float dt = Time.fixedDeltaTime;
    if (locked) { for (int i = 0; i < 4; i++) { wc[i].motorTorque = 0; wc[i].brakeTorque = 9000; } return; }
    float throttle = inp.Throttle, brake = inp.Brake;
    float fwd = FwdSpeed; bool manual = SaveManager.D.manual;
    float dir = gear == Gear.D ? 1f : gear == Gear.R ? -1f : 0f;
    float maxK = gear == Gear.R ? 25f : S.maxKmh * spdMul; float tm = 1f;
    if (manual && gear == Gear.D) { maxK *= manualGear / 5f; tm = 2.1f - .3f * manualGear; }
    nitroOn = inp.Nitro && nitro > .02f && gear == Gear.D && throttle > .1f;
    if (nitroOn) { nitro -= dt * .2f; tm *= 1.8f; maxK *= 1.15f; } else nitro = Mathf.Min(1f, nitro + dt * .05f);
    float tq = throttle * dir * S.torque * tqMul * tm;
    if (Mathf.Abs(fwd) > maxK && Mathf.Sign(fwd) == dir) tq = 0;
    float all = brake * S.brake * .5f * brMul;
    if (gear == Gear.P) all = Mathf.Max(all, 6000f);
    else if (throttle < .05f && gear != Gear.N) all = Mathf.Max(all, 250f);
    for (int i = 0; i < 4; i++) {
      wc[i].motorTorque = i >= 2 ? tq : 0f;
      float b = all; if (i >= 2 && inp.Handbrake) b = Mathf.Max(b, 9000f);
      wc[i].brakeTorque = b;
    }
    braking = brake > .1f || inp.Handbrake;
    float k = Mathf.Lerp(1f, .4f, Mathf.Abs(fwd) / 90f);
    steerCur = Mathf.MoveTowards(steerCur, inp.Steer * S.steer * stMul * k, 120f * dt);
    wc[0].steerAngle = steerCur; wc[1].steerAngle = steerCur;
  }

  void Update() {
    if (wc == null || !wc[0].enabled) return;
    for (int i = 0; i < 4; i++) { Vector3 p; Quaternion q; wc[i].GetWorldPose(out p, out q); wm[i].SetPositionAndRotation(p, q); }
    if (player && AudioManager.I != null && GameInput.I != null) {
      float top = S.maxKmh * spdMul * (SaveManager.D.manual && gear == Gear.D ? manualGear / 5f : 1f);
      float rpm = Mathf.Clamp01(Speed / top) * .75f + GameInput.I.Throttle * .25f;
      AudioManager.I.SetEngine(rpm, GameInput.I.Throttle);
      AudioManager.I.Reverse(gear == Gear.R && Speed > .3f);
      AudioManager.I.Brake(braking && Speed > 10f);
    }
  }

  void OnCollisionEnter(Collision c) {
    if (!player) return;
    string n = c.gameObject.name;
    if (n.StartsWith("Ground") || n.StartsWith("Ramp") || n.StartsWith("Platform")) return;
    if (c.relativeVelocity.magnitude < 1f) return;
    if (GameManager.I != null) GameManager.I.OnCarCollision();
  }
}
