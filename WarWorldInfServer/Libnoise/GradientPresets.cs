using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using WarWorldInfServer;

namespace LibNoise
{
	/// <summary>
	/// Provides a series of gradient presets
	/// </summary>
	public static class GradientPresets
	{
		public struct GradientKeyData
		{
			public bool isImage;
			public GColor color;
			public List<string> imageFiles;
			public float time;

			public GradientKeyData(GColor color, float time){
				this.isImage = false;
				this.color = color;
				this.imageFiles = new List<string>();
				this.time = time;
			}

			public GradientKeyData(List<string> files, float time){
				this.isImage = true;
				this.color = new GColor(0,0,0,0);
				this.imageFiles = new List<string>(files);
				this.time = time;
			}
		}

		public struct GColor{
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
				return Color.FromArgb (original.A, original.R, original.G, original.B);
			}
		}

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
				new GradientKey(Color.Black, 0),
				new GradientKey(Color.White, 1)
			};
			
			// RGB gradient color keys
			var rgbColorKeys = new List<GradientKey>
			{
				new GradientKey(Color.Red, 0),
				new GradientKey(Color.Green, 0.5f),
				new GradientKey(Color.Blue, 1)
			};
			
			// RGBA gradient color keys
			var rgbaColorKeys = new List<GradientKey>
			{
				new GradientKey(Color.Red, 0),
				new GradientKey(Color.Green, 1 / 3f),
				new GradientKey(Color.Blue, 2 / 3f),
				new GradientKey(Color.Black, 1)
			};
			
			// RGBA gradient alpha keys
			var rgbaAlphaKeys = new List<GradientAlphaKey> {new GradientAlphaKey(0, 2 / 3f), new GradientAlphaKey(1, 1)};

			// Terrain gradient color keys
			var terrainColorKeys = new List<GradientKey>
			{
				new GradientKey(Color.FromArgb(255, 0, 0, 128), 0),
				new GradientKey(Color.FromArgb(255, 32, 64, 128), 0.4f),
				new GradientKey(Color.FromArgb(255, 64, 96, 191), 0.48f),
				new GradientKey(Color.FromArgb(255, 0, 191, 0), 0.5f),
				new GradientKey(Color.FromArgb(255, 191, 191, 0), 0.625f),
				new GradientKey(Color.FromArgb(255, 159, 96, 64), 0.85f),
				new GradientKey(Color.FromArgb(255, 128, 255, 255), 0.98f),
				new GradientKey(Color.FromArgb(255, 240, 240, 250), 1)
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

		public static Gradient CreateGradient(List<GradientKeyData> keyData){
			List<GradientKey> keys = new List<GradientKey> ();
			for (int i = 0; i < keyData.Count; i++) {
				if (keyData[i].isImage){
					List<Color[]> images = new List<Color[]>();
					string[] files = keyData[i].imageFiles.ToArray();
					for (int j = 0; j < files.Length; j++){
						string file = GameServer.Instance.AppDirectory + files[j];
						if (File.Exists(file)){
							Bitmap map =  new Bitmap (Bitmap.FromFile (file));
							//Logger.Log("{0}: {1} {2}", file, map.Width.ToString(), map.Height.ToString());
							Color[] imgColors = new Color[map.Width * map.Height];
							for (int x = 0; x < map.Width; x++) {
								for (int y = 0; y < map.Height; y++){
									imgColors[x + y * map.Width] = map.GetPixel(x, y);
								}
							}
							images.Add(imgColors);
						}
						else
							Logger.LogError("Cannot find texture {0}", file);
					}
					keys.Add(new GradientKey(images, 10, 10, keyData[i].time));
				}
				else
				{

					keys.Add(new GradientKey(keyData[i].color, keyData[i].time));
				}
			}
			return CreateGradient (keys);
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
		
		#endregion
	}
}

