using System;
using System.Collections.Generic;
using System.IO;

namespace LibNoise
{
	/// <summary>
	/// Provides a series of gradient presets
	/// </summary>
	public static class GradientPresets
	{
		public class GradientKeyData
		{
			public bool isImage;
			public Color color;
			public List<string> imageFiles;
			public List<Color[]> images;
			public float time;

			public GradientKeyData(){

			}

			public GradientKeyData(Color color, float time){
				this.isImage = false;
				this.color = color;
				this.imageFiles = new List<string>();
				this.time = time;
				this.images = new List<Color[]>();
			}

			public GradientKeyData(List<string> files, float time){
				this.isImage = true;
				this.color = new Color(0,0,0,0);
				this.imageFiles = new List<string>(files);
				this.time = time;
				this.images = new List<Color[]>();
			}
		}

		/*public struct GColor{
			public byte A;
			public byte R;
			public byte G;
			public byte B;
			
			public GColor(byte A, byte R, byte G, byte B){
				this.A = A;
				this.R = R;
				this.G = G;
				this.B = B;
			}

			public static implicit operator Color(GColor original){
				return new Color (original.A, original.R, original.G, original.B);
			}
		}*/

		#region Fields
		
		private static readonly Gradient _empty;
		private static readonly Gradient _grayscale;
		private static readonly Gradient _rgb;
		private static readonly Gradient _rgba;
		private static readonly Gradient _terrain;
		
		#endregion
		
		#region Constructors
		
		/// <summary>
		/// Initializes a new instance of Gradient.
		/// </summary>
		static GradientPresets()
		{
			// Grayscale gradient color keys
			var grayscaleColorKeys = new List<GradientKey>
			{
				new GradientKey(new Color(1, 0, 0, 0), 0),
				new GradientKey(new Color(1,1,1,1), 1)
			};
			
			// RGB gradient color keys
			var rgbColorKeys = new List<GradientKey>
			{
				new GradientKey(new Color(1, 255, 0, 0), 0),
				new GradientKey(new Color(1, 0, 255, 0), 0.5f),
				new GradientKey(new Color(1, 0, 0, 255), 1)
			};
			
			// RGBA gradient color keys
			var rgbaColorKeys = new List<GradientKey>
			{
				new GradientKey(new Color(1, 255, 0, 0), 0),
				new GradientKey(new Color(1, 0, 255, 0), 1 / 3f),
				new GradientKey(new Color(1, 0, 0, 255), 2 / 3f),
				new GradientKey(new Color(1, 0, 0, 0), 1)
			};
			
			// RGBA gradient alpha keys
			var rgbaAlphaKeys = new List<GradientAlphaKey> {new GradientAlphaKey(0, 2 / 3f), new GradientAlphaKey(1, 1)};

			// Terrain gradient color keys
			var terrainColorKeys = new List<GradientKey>
			{
				new GradientKey(new Color(255, 0, 0, 128), 0),
				new GradientKey(new Color(255, 32, 64, 128), 0.4f),
				new GradientKey(new Color(255, 64, 96, 191), 0.48f),
				new GradientKey(new Color(255, 0, 191, 0), 0.5f),
				new GradientKey(new Color(255, 191, 191, 0), 0.625f),
				new GradientKey(new Color(255, 159, 96, 64), 0.85f),
				new GradientKey(new Color(255, 128, 255, 255), 0.98f),
				new GradientKey(new Color(255, 240, 240, 250), 1)
			};
			
			// Generic gradient alpha keys
			var alphaKeys = new List<GradientAlphaKey> {new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1)};
			
			_empty = new Gradient();
			
			_rgb = new Gradient();
			_rgb.SetKeys(rgbColorKeys.ToArray());
			
			_rgba = new Gradient();
			_rgba.SetKeys(rgbaColorKeys.ToArray());
			
			_grayscale = new Gradient();
			_grayscale.SetKeys(grayscaleColorKeys.ToArray());
			
			_terrain = new Gradient();
			_terrain.SetKeys(terrainColorKeys.ToArray());
		}

		public static Gradient CreateGradient(List<GradientKey> keys){
			Gradient grad = new Gradient ();
			grad.SetKeys (keys.ToArray ());
			return grad;
		}
		
		#endregion
		
		#region Properties
		
		/// <summary>
		/// Gets the empty instance of Gradient.
		/// </summary>
		public static Gradient Empty
		{
			get { return _empty; }
		}
		
		/// <summary>
		/// Gets the grayscale instance of Gradient.
		/// </summary>
		public static Gradient Grayscale
		{
			get { return _grayscale; }
		}
		
		/// <summary>
		/// Gets the RGB instance of Gradient.
		/// </summary>
		public static Gradient RGB
		{
			get { return _rgb; }
		}
		
		/// <summary>
		/// Gets the RGBA instance of Gradient.
		/// </summary>
		public static Gradient RGBA
		{
			get { return _rgba; }
		}
		
		/// <summary>
		/// Gets the terrain instance of Gradient.
		/// </summary>
		public static Gradient Terrain
		{
			get { return _terrain; }
		}

		public static string AppDirectory;

		
		
		#endregion
	}
}

