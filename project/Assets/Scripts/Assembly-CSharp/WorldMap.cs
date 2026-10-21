using System;
using System.Collections;
using UnityEngine;

public class WorldMap : MonoBehaviour
{
	private class DebrisBit
	{
		public DebrisBot _botBit;

		public bool _free;
	}

	public GameObject _debrisBot;

	public int _debrisCount;

	public Color[] _botColors = new Color[4]
	{
		Color.blue,
		Color.green,
		Color.yellow,
		Color.magenta
	};

	private DebrisBit[] _debrisBits;

	private int _free;

	private void Start()
	{
		Camera worldCam = SinglePlayerUI.WorldCam;
		if (_debrisBot != null)
		{
			_debrisBits = new DebrisBit[_debrisCount];
			for (int i = 0; i < _debrisCount; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(_debrisBot) as GameObject;
				gameObject.transform.parent = base.transform;
				gameObject.transform.position = worldCam.transform.position + worldCam.transform.forward * 5f;
				gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
				AlignWith component = gameObject.GetComponent<AlignWith>();
				component.Target = SinglePlayerUI.WorldCam;
				Material material = gameObject.renderer.material;
				material.color = _botColors[UnityEngine.Random.Range(0, _botColors.Length)];
				gameObject.renderer.material = material;
				gameObject.SetActive(false);
				_debrisBits[i] = new DebrisBit();
				_debrisBits[i]._botBit = gameObject.GetComponent<DebrisBot>();
				_debrisBits[i]._free = true;
			}
			_free = _debrisBits.Length;
			StartCoroutine(Spawn());
		}
	}

	private void Update()
	{
		if (_debrisBits == null)
		{
			return;
		}
		for (int i = 0; i < _debrisBits.Length; i++)
		{
			if (OutsideActiveZone(_debrisBits[i]._botBit))
			{
				Deactivate(i);
			}
		}
	}

	private IEnumerator Spawn()
	{
		while (true)
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(0, 1));
			if (_free > 0)
			{
				SpawnNext();
			}
		}
	}

	private bool OutsideActiveZone(DebrisBot d)
	{
		Camera worldCam = SinglePlayerUI.WorldCam;
		float orthographicSize = worldCam.orthographicSize;
		float num = worldCam.orthographicSize * worldCam.aspect;
		Vector3 position = d.transform.position;
		Vector3 position2 = worldCam.transform.position;
		Vector3 lhs = position - position2;
		float num2 = Mathf.Abs(Vector3.Dot(lhs, d.transform.right));
		float num3 = Mathf.Abs(Vector3.Dot(lhs, d.transform.up));
		return num2 > num * 1.5f || num3 > orthographicSize * 1.5f;
	}

	private void SpawnNext()
	{
		DebrisBit debrisBit = Array.Find(_debrisBits, (DebrisBit thisd) => thisd._free);
		if (debrisBit != null)
		{
			Camera worldCam = SinglePlayerUI.WorldCam;
			float num = worldCam.orthographicSize * 1.1f;
			float num2 = num * worldCam.aspect;
			Vector3 position = worldCam.transform.position;
			Vector3 vector = UnityEngine.Random.insideUnitCircle.normalized;
			Vector3 vector2 = worldCam.transform.right * vector.x * num2 + worldCam.transform.up * vector.y * num;
			Vector3 position2 = position + vector2 + worldCam.transform.forward * 5f;
			debrisBit._botBit.transform.position = position2;
			debrisBit._botBit.gameObject.SetActive(true);
			debrisBit._botBit.InitMovement();
			debrisBit._free = false;
			debrisBit._botBit.renderer.sharedMaterial.color = _botColors[UnityEngine.Random.Range(0, _botColors.Length)];
		}
	}

	private void Deactivate(int i)
	{
		_debrisBits[i]._botBit.gameObject.SetActive(false);
		_debrisBits[i]._free = true;
	}
}
