using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using LibNoise;
using LibNoise.Models;
using LibNoise.Modifiers;

namespace WarWorldInfServer
{
	public class TerrainBuilder
	{
		public class TerrainSettings
		{
			public int width;
			public int height;
			public int seed;
			public string preset;
			public string imageFile;
			public string moduleFile;
			
			public TerrainSettings(int width, int height, int seed, string preset, string imageFile, string moduleFile){
				this.width = width;
				this.height = height;
				this.seed = seed;
				this.preset = preset;
				this.imageFile = imageFile;
				this.moduleFile = moduleFile;
			}
		}

		private GradiantPresetLoader _presetLoader;

		public int Seed { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public IModule NoiseModule { get; private set; }
		public Noise2D NoiseMap { get; private set; }
		public System.Drawing.Color[] ColorMap { get; private set; }
		public TerrainSettings Settings { get; private set;}
		public List<GradientPresets.GradientKeyData> GradientPreset { get; private set;}
		public Gradient Gradient { get; private set; }

		// use when creating
		public TerrainBuilder (int width, int height, int seed)
		{
			Seed = seed;
			Width = width;
			Height = height;
			_presetLoader = new GradiantPresetLoader (GameServer.Instance.AppDirectory + "GradientPresets" + GameServer.sepChar);
			Settings = new TerrainSettings (width, height, seed, string.Empty, string.Empty, string.Empty);
		}

		// use when loading
		public TerrainBuilder(TerrainSettings settings){
			Settings = settings;
			Seed = settings.seed;
			Width = settings.width;
			Height = settings.height;
			NoiseModule = FileManager.LoadObject<IModule> (settings.moduleFile, true);;
			_presetLoader = new GradiantPresetLoader (GameServer.Instance.AppDirectory + "GradientPresets" + GameServer.sepChar);
			GradientPreset = _presetLoader.GetPreset (settings.preset);
			Gradient = GradientCreator.CreateGradientServer (new List<GradientPresets.GradientKeyData>(GradientPreset));
			LoadMap (settings.imageFile);
		}

		// use when loading. Never needs to be called.
		public void Generate(){
			Generate (NoiseModule, Settings.preset);
		}

		// use when creating
		public bool Generate(IModule module, string preset){
			NoiseModule = module;
			GradientPreset = _presetLoader.GetPreset(preset);
			if (GradientPreset != null && module != null) {
				NoiseMap = new Noise2D (Width, Height, NoiseModule);
				//_noiseMap.GeneratePlanar (0, 4, 0, 2);
				NoiseMap.GenerateSpherical (-90, 90, -180, 180);
				Logger.Log ("terrain generated.");
				Gradient = GradientCreator.CreateGradientServer (new List<GradientPresets.GradientKeyData>(GradientPreset));
				ColorMap = ColorConvert.SysColList(NoiseMap.GetTexture (Gradient));
				Logger.Log ("bitmap generated.");
				Settings.preset = preset;
				return true;
			}
			else
				Logger.LogError("Generate: Gradient Preset is null!");
			return false;
		}

		public void Save(string file){
			SaveBmp (ColorMap, file);
			Settings.imageFile = file;
			Logger.Log ("Bitmap saved.");
		}

		public void SaveModule(string file){
			Settings.moduleFile = file;
			FileManager.SaveConfigFile (file, NoiseModule, true);
		}

		public string GetColorString(){
			return GetColorString (ColorMap);
		}

		public void SaveMapStr(string file){
			SaveMapStr (GetColorString (), file);
		}

		public void LoadMap(string file){
			Settings.imageFile = file;
			Image img = Bitmap.FromFile (file);
			Width = img.Width;
			Height = img.Height;
			Bitmap bmap = new Bitmap (img, img.Width, img.Height);
			ColorMap = new System.Drawing.Color[Width * Height];
			for (int x = 0; x < Width; x++) {
				for (int y = 0; y < Height; y++) {
					ColorMap[x + y * Width] = bmap.GetPixel(x, y);
				}
			}
			img.Dispose ();
		}

		public System.Drawing.Color GetColor(int x, int y){
			return ColorMap [x + y * Width];
		}

		public Bitmap GetBitmap(){
			return GetBitmap (ColorMap);
		}

		public Bitmap GetBitmap(System.Drawing.Color[] colors){
			Bitmap map = new Bitmap (Width, Height);
			for (int x = 0; x < Width; x++) {
				for (int y = 0; y < Height; y++) {
					map.SetPixel(x, y, colors[x + y * Width]);
				}
			}
			return map;
		}

		private string GetColorString(System.Drawing.Color[] colors){
			string colorStr = string.Empty;
			for (int i = 0; i < ColorMap.Length; i++) {
				colorStr += colors[i].R + "," + colors[i].G + "," + colors[i].B + "&";
			}
			return colorStr;
		}
		
		private void SaveMapStr(string colorStr, string file){
			StreamWriter stream = File.CreateText (file);
			stream.WriteLine (colorStr);
			stream.Close ();
		}
		
		private System.Drawing.Color[] GetColorsFromString(string colorStr)
		{
			string[] colors = colorStr.Split('&');
            System.Drawing.Color[] colorArr = new System.Drawing.Color[colors.Length];
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					string[] RGB = colors[x + y * Width].Split(',');
					int R = int.Parse(RGB[0]);
					int G = int.Parse(RGB[1]);
					int B = int.Parse(RGB[2]);
					colorArr[x + y * Width] = System.Drawing.Color.FromArgb(255, R, G, B);
				}
			}
			return colorArr;
		}
		
		private void SaveBmp(System.Drawing.Color[] colors, string file){
			Bitmap map = GetBitmap (colors);
			map.Save(file);
			map.Dispose();
		}
	}
}

