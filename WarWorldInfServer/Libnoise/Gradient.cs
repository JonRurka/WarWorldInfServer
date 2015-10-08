using System;
using System.Drawing;

namespace LibNoise
{
	public class Gradient
	{
		public GradientColorKey[] colorKeys;

		public Gradient ()
		{
			SetKeys (new GradientColorKey[0]);
		}

		public Color Evaluate(float time)
		{
			if (time < 0)
				time = 0;
			if (time > 1)
				time = 1;
			
			GradientColorKey lowerKey = new GradientColorKey();
			GradientColorKey upperKey = new GradientColorKey();
			for (int i = 0; i < colorKeys.Length; i++) {
				float bottom = colorKeys[i].time;
				float top = colorKeys[i + 1].time;
				if (time >= bottom && time <= top){
					lowerKey = colorKeys[i];
					upperKey = colorKeys[i + 1];
					break;
				}
			}
			
			float t = GetPercent(lowerKey.time, time, upperKey.time);
			int r = lerp(lowerKey.color.R, upperKey.color.R, t);
			int g = lerp(lowerKey.color.G, upperKey.color.G, t);
			int b = lerp(lowerKey.color.B, upperKey.color.B, t);
			return Color.FromArgb(255, r, g, b);

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

		public void SetKeys(GradientColorKey[] colors)
		{
			colorKeys = colors;
		}
	}
}

