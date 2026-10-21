using EB;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using UnityEngine;

public class FSAudio : MonoBehaviour
{
	public AudioEvent _impactAudioEvent;

	public float _impactAudioMinVel = 1f;

	public float _impactAudioMaxVel = 10f;

	public AnimationCurve _scalarVolumeResponse = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private Body _body;

	private AudioEmitter _impactEmitter;

	private float _impactAudioVelRange;

	private void OnBodyReady()
	{
		FSBodyComponent component = GetComponent<FSBodyComponent>();
		if (component != null)
		{
			_body = component.PhysicsBody;
		}
		else
		{
			RangeWeapon component2 = GetComponent<RangeWeapon>();
			if (component2 != null)
			{
				_body = component2.MyBody;
			}
		}
		if (_body == null)
		{
			EB.Debug.LogError(string.Format("{0} has no FS body I can grab...aborting audio!", base.name));
		}
		_body.OnCollision += OnCollisionEvent;
		AudioEmitter component3 = GetComponent<AudioEmitter>();
		if (component3 != null && !component3.m_PlayAtStart)
		{
			_impactEmitter = component3;
		}
		else if (_impactAudioEvent != null)
		{
			_impactEmitter = base.gameObject.AddComponent<AudioEmitter>();
			_impactEmitter.m_PlayAtStart = false;
			_impactEmitter.m_PlayMode = AudioEmitter.PlayMode.Orphan;
			_impactEmitter.m_AudioEvent = _impactAudioEvent;
		}
		_impactAudioVelRange = Mathf.Max(1f, _impactAudioMaxVel - _impactAudioMinVel);
	}

	private bool OnCollisionEvent(Fixture a, Fixture b, Contact c)
	{
		PlayImpactAudio(c);
		return true;
	}

	private void PlayImpactAudio(Contact contact)
	{
		if (_impactEmitter != null && !_impactEmitter.GetIsPlaying())
		{
			FVector2 normal;
			FixedArray2<FVector2> points;
			contact.GetWorldManifold(out normal, out points);
			FVector2 linearVelocity = _body.LinearVelocity;
			float num = Mathf.Max(0f, 0f - FVector2.Dot(linearVelocity, normal) / FVector2.Dot(normal, normal));
			float num2 = (num * normal).Length();
			if (num2 > _impactAudioMinVel)
			{
				num2 = Mathf.Min(1f, (num2 - _impactAudioMinVel) / _impactAudioVelRange);
				float volumeScalar = _scalarVolumeResponse.Evaluate(num2);
				_impactEmitter.SetVolumeScalar(volumeScalar);
				_impactEmitter.Play();
			}
		}
	}
}
