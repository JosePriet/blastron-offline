namespace EB.Sparx
{
	public struct Tracker
	{
		public int counter;

		public int total;

		public float interval;

		public float rate;

		public void Add(int v)
		{
			counter += v;
		}

		public void Update(float dT)
		{
			interval += dT;
			if (interval > 1f)
			{
				rate = (float)counter / interval;
				counter = 0;
				interval = 0f;
			}
		}
	}
}
