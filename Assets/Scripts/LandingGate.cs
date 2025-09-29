using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController2D))]
public class LandingGate : MonoBehaviour
{
	PlayerController2D pc;
	Rigidbody2D rb;
	public Transform startPlatform;

	struct ContactState { public bool awarded; public bool penalized; }
	readonly Dictionary<int, ContactState> contactMap = new Dictionary<int, ContactState>();
	readonly HashSet<int> scoredPlatformIds = new HashSet<int>();

	void Awake(){
		pc = GetComponent<PlayerController2D>();
		rb = GetComponent<Rigidbody2D>();
		if (!startPlatform)
		{
			var sp = GameObject.Find("StartPlatform");
			if (sp) startPlatform = sp.transform;
		}
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		var plat = col.collider.GetComponent<PlatformColor>();
		if (!plat) return;
		if (IsStartPlatform(col.collider)) return;

		if (!HasTopContact(col)) return;

		if (rb && rb.velocity.y > 0.05f) return;

		int id = col.collider.GetInstanceID();
		var state = contactMap.ContainsKey(id) ? contactMap[id] : default;
		int platGoId = plat.gameObject.GetInstanceID();

		if (pc.bottomColor == plat.color)
		{
			if (!state.awarded && !scoredPlatformIds.Contains(platGoId))
			{
				if (GameManager.I && GameManager.I.debugLogs)
					Debug.Log($"✔ Correct landing (+10) | bottom={pc.bottomColor}, platform={plat.color} (first time on this platform)");
				if (GameManager.I) GameManager.I.CorrectLandingAward();
				state.awarded = true;
				contactMap[id] = state;
				scoredPlatformIds.Add(platGoId);
			}
		}
		else
		{
			if (!state.penalized)
			{
				if (GameManager.I && GameManager.I.debugLogs)
					Debug.Log($"✖ Wrong color (lose life) | bottom={pc.bottomColor}, platform={plat.color}");
				if (GameManager.I) GameManager.I.WrongLandingPenalty();
				state.penalized = true;
				contactMap[id] = state;
			}
		}
	}

	void OnCollisionStay2D(Collision2D col)
	{
		var plat = col.collider.GetComponent<PlatformColor>();
		if (!plat) return;
		if (IsStartPlatform(col.collider)) return;

		if (!HasTopContact(col)) return;

		int id = col.collider.GetInstanceID();
		var state = contactMap.ContainsKey(id) ? contactMap[id] : default;

		if (pc.bottomColor != plat.color && !state.penalized)
		{
			if (GameManager.I && GameManager.I.debugLogs)
				Debug.Log($"✖ Wrong color (lose life) | bottom={pc.bottomColor}, platform={plat.color}");
			if (GameManager.I) GameManager.I.WrongLandingPenalty();
			state.penalized = true;
			contactMap[id] = state;
		}
	}

	void OnCollisionExit2D(Collision2D col)
	{
		int id = col.collider.GetInstanceID();
		if (contactMap.ContainsKey(id)) contactMap.Remove(id);
	}

	bool IsStartPlatform(Collider2D col)
	{
		if (startPlatform && col.transform == startPlatform) return true;
		if (col.CompareTag("StartPlatform")) return true;
		return false;
	}

	static bool HasTopContact(Collision2D col)
	{
		for (int i = 0; i < col.contactCount; i++)
		{
			var n = col.GetContact(i).normal;
			if (n.y >= 0.5f) return true;
		}
		return false;
	}
}