using System;
using System.Collections.Generic;
using System.Drawing;

namespace LibNoise
{
	/// <summary>
	/// Provides a series of gradient presets
	/// </summary>
	public static class GradientPresets
	{
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
			var grayscaleColorKeys = new List<GradientColorKey>
			{
				new GradientColorKey(Color.Black, 0),
				new GradientColorKey(Color.White, 1)
			};
			
			// RGB gradient color keys
			var rgbColorKeys = new List<GradientColorKey>
			{
				new GradientColorKey(Color.Red, 0),
				new GradientColorKey(Color.Green, 0.5f),
				new GradientColorKey(Color.Blue, 1)
			};
			
			// RGBA gradient color keys
			var rgbaColorKeys = new List<GradientColorKey>
			{
				new GradientColorKey(Color.Red, 0),
				new GradientColorKey(Color.Green, 1 / 3f),
				new GradientColorKey(Color.Blue, 2 / 3f),
				new GradientColorKey(Color.Black, 1)
			};
			
			// RGBA gradient alpha keys
			var rgbaAlphaKeys = new List<GradientAlphaKey> {new GradientAlphaKey(0, 2 / 3f), new GradientAlphaKey(1, 1)};
			
			// Terrain gradient color keys
			var terrainColorKeys = new List<GradientColorKey>
			{
				new GradientColorKey(Color.FromArgb(255, 0, 0, 128), 0),
				new GradientColorKey(Color.FromArgb(255, 32, 64, 128), 0.4f),
				new GradientColorKey(Color.FromArgb(255, 64, 96, 191), 0.48f),
				new GradientColorKey(Color.FromArgb(255, 0, 191, 0), 0.5f),
				new GradientColorKey(Color.FromArgb(255, 191, 191, 0), 0.625f),
				new GradientColorKey(Color.FromArgb(255, 159, 96, 64), 0.85f),
				new GradientColorKey(Color.FromArgb(255, 128, 255, 255), 0.98f),
				new GradientColorKey(Color.FromArgb(255, 240, 240, 250), 1)
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
		
		#endregion
	}
}

