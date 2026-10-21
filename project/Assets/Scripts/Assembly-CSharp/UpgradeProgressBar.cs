using EB;
using UnityEngine;

public class UpgradeProgressBar : MonoBehaviour
{
	public float minScale = 1f;

	public float maxScale = 2f;

	private CharacterDB.MetaData metadata;

	private ClippedProgressBar progressOnEntry;

	private ClippedProgressBar updatedProgress;

	private ClippedProgressBar nextUpgrade;

	private ClippedProgressBar nextUpgradeBorder;

	private Vector3 originalPosition;

	public void SetMetadata(CharacterDB.MetaData md)
	{
		metadata = md;
		UpdateProgressBar(metadata.numUpgradeStars);
	}

	public void SetValue(float val)
	{
		progressOnEntry.SetValueImmediate(val);
		if (updatedProgress != null)
		{
			updatedProgress.SetValueImmediate(val);
		}
	}

	public void ShowUpgrade(float newVal, Action animCompleteCb)
	{
		updatedProgress.SetValueAnimated(newVal, SafeAction.Wrap(this, () =>
		{
			if (animCompleteCb != null)
			{
				animCompleteCb();
			}
		}));
	}

	public void SetNextUpgrade(float val)
	{
		if (nextUpgrade != null)
		{
			float num = nextUpgrade.GetValuePerPixel() * 2f;
			nextUpgrade.SetValueImmediate(val - num);
			nextUpgradeBorder.SetValueImmediate(val + num);
		}
	}

	private void Awake()
	{
		originalPosition = base.transform.localPosition;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "progressOnEntry");
		progressOnEntry = Util.FindComponent<ClippedProgressBar>(objectExactMatch);
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "updatedProgress");
		if (objectExactMatch != null)
		{
			updatedProgress = Util.FindComponent<ClippedProgressBar>(objectExactMatch);
		}
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "nextUpgradeFill");
		if (objectExactMatch != null)
		{
			nextUpgrade = Util.FindComponent<ClippedProgressBar>(objectExactMatch);
		}
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "nextUpgradeBorder");
		if (objectExactMatch != null)
		{
			nextUpgradeBorder = Util.FindComponent<ClippedProgressBar>(objectExactMatch);
		}
	}

	public void UpdateProgressBar(int stars)
	{
		int num = 5;
		int num2 = stars - num;
		float num3 = maxScale - minScale;
		float value = maxScale - (float)num2 / (float)num * num3;
		value = Mathf.Clamp(value, minScale, maxScale);
		value *= 0.9f;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "emptyBar");
		RemoveClones(objectExactMatch);
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(objectExactMatch, "star");
		Vector2 vector = DuplicateChild(objectExactMatch, "star", stars, value);
		GameObject obj = progressOnEntry.gameObject;
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(obj, "artContainer");
		RemoveClones(objectExactMatch3);
		DuplicateChild(objectExactMatch3, "star", stars, value);
		if (updatedProgress != null)
		{
			GameObject obj2 = updatedProgress.gameObject;
			objectExactMatch3 = Util.GetObjectExactMatch(obj2, "artContainer");
			RemoveClones(objectExactMatch3);
			DuplicateChild(objectExactMatch3, "star", stars, value);
		}
		if (nextUpgrade != null)
		{
			GameObject obj3 = nextUpgrade.gameObject;
			objectExactMatch3 = Util.GetObjectExactMatch(obj3, "artContainer");
			RemoveClones(objectExactMatch3);
			DuplicateChild(objectExactMatch3, "star", stars, value);
		}
		if (nextUpgradeBorder != null)
		{
			GameObject obj4 = nextUpgradeBorder.gameObject;
			objectExactMatch3 = Util.GetObjectExactMatch(obj4, "artContainer");
			RemoveClones(objectExactMatch3);
			DuplicateChild(objectExactMatch3, "star", stars, value);
		}
		Vector3 localPosition = originalPosition;
		localPosition.x -= vector.x / 2f;
		base.transform.localPosition = localPosition;
		UIPanel[] array = Util.FindAllComponents<UIPanel>(base.gameObject);
		foreach (UIPanel uIPanel in array)
		{
			Vector4 clipRange = uIPanel.clipRange;
			clipRange.z = vector.x;
			clipRange.w = vector.y;
			clipRange.x = vector.x / 2f;
			clipRange.y = (0f - objectExactMatch2.transform.localScale.y) / 2f;
			uIPanel.clipRange = clipRange;
		}
		ClippedProgressBar[] array2 = Util.FindAllComponents<ClippedProgressBar>(base.gameObject);
		foreach (ClippedProgressBar clippedProgressBar in array2)
		{
			clippedProgressBar.UpdateClipRange();
		}
	}

	private void RemoveClones(GameObject container)
	{
		GameObject[] objects = Util.GetObjects(container, "(Clone)");
		GameObject[] array = objects;
		foreach (GameObject obj in array)
		{
			Object.Destroy(obj);
		}
	}

	private Vector2 DuplicateChild(GameObject container, string childName, int count, float scale)
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(container, childName);
		float x = objectExactMatch.transform.localPosition.x + objectExactMatch.transform.localScale.x;
		float y = objectExactMatch.transform.localScale.y;
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = NGUITools.AddChild(container, objectExactMatch);
			Vector3 localPosition = objectExactMatch.transform.localPosition;
			Vector3 localScale = objectExactMatch.transform.localScale * scale;
			localPosition.x += objectExactMatch.transform.localScale.x * scale * (float)i;
			gameObject.transform.localPosition = localPosition;
			gameObject.transform.localScale = localScale;
			x = gameObject.transform.localPosition.x + gameObject.transform.localScale.x;
			y = gameObject.transform.localScale.y;
			gameObject.SetActive(true);
		}
		objectExactMatch.active = false;
		return new Vector2(x, y);
	}
}
