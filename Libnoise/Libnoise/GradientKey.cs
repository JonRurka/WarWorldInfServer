using System;
using System.Collections.Generic;
using System.Drawing;

namespace LibNoise
{
	public struct GradientKey
	{

		public Color color;
		public List<Color[]> colors;
		public float time;
		public bool hasImage;
		public int width;
		public int height;

		public GradientKey (Color col, float time)
		{
			this.color = col;
			this.colors = new List<Color[]>();
			this.time = time;
			hasImage = false;
			width = 1;
			height = 1;
		}

		public GradientKey(List<Color[]> cols, int width, int height, float time){
			this.colors = new List<Color[]>(cols);
			this.color = new Color ();
			this.time = time;
			this.hasImage = true;
			this.width = width;
			this.height = height;
		}

		public Color GetPixel(int index, int x, int y){
			if (index < 0) {
				return Color.Black;
			}

			if (x < 0) {
				return Color.Black;
			}

			if (y < 0) {
				return Color.Black;
			}
			return colors[index] [x + y * width];
		}
	}
}

