using UnityEngine;

namespace Life
{
	public interface ILifeFn
	{
		void Start(Transform t, float duration, float amplitude, float oscillations);

		bool Update(float elapsed);
	}
}
