using System;
using WarWorldInfinity.Shared;
//using System.Drawing;

namespace WarWorldInfinity.LibNoise
{
	public class Gradient
	{
		public GradientKey[] colorKeys;
		private Random rand;

		public Gradient ()
		{
			SetKeys (new GradientKey[0]);
			rand = new Random ();
		}

		public Color Evaluate(int locX, int locY, float time)
		{
			if (time < 0)
				time = 0;
			if (time > 1)
				time = 1;
			
			GradientKey lowerKey = new GradientKey();
			GradientKey upperKey = new GradientKey();
			for (int i = 0; i < colorKeys.Length; i++) {
				float bottom = colorKeys[i].time;
				float top = colorKeys[i + 1].time;
				if (time >= bottom && time <= top){
					lowerKey = colorKeys[i];
					upperKey = colorKeys[i + 1];
					break;
				}
			}
			Color lowColor = new Color ();
			Color upperColor = new Color ();
			int lowIndex = rand.Next (0, lowerKey.colors.Count);
			int upIndex = rand.Next (0, upperKey.colors.Count);
			if (lowerKey.hasImage)
				lowColor = lowerKey.GetPixel (lowIndex, locX, locY);
			else
				lowColor = lowerKey.color;

			if (upperKey.hasImage)
				upperColor = upperKey.GetPixel (upIndex, locX, locY);
			else
				upperColor = upperKey.color;

			float t = GetPercent(lowerKey.time, time, upperKey.time);
			int r = lerp(lowColor.R, upperColor.R, t);
			int g = lerp(lowColor.G, upperColor.G, t);
			int b = lerp(lowColor.B, upperColor.B, t);
			return new Color(255, r, g, b);

			return new Color ();
		}

		private float GetPercent(float bottom, float middle, float top){
			float range = top - bottom;
			float bottomRange = middle - bottom;
			return bottomRange / range;
		}

		private int lerp(int a, int b, float f){
			return (int)(a + f * (b - a));
		}

		public void SetKeys(GradientKey[] colors)
		{
			colorKeys = colors;
		}
	}
}

