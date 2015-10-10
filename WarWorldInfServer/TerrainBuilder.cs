using System;
using System.Drawing;
using System.IO;
using LibNoise;
using LibNoise.Models;
using LibNoise.Modifiers;

namespace WarWorldInfServer
{
	public class TerrainBuilder
	{
		private int _seed;
		private int _width;
		private int _height;
		private IModule _noiseModule;
		private Noise2D _noiseMap;
		private Color[] _map;

		public int Seed { get { return _seed; } }
		public int Width { get { return _width; } }
		public int Height { get { return _height; } }
		public IModule NoiseModule { get { return _noiseModule; } }
		public Noise2D NoiseMap { get { return _noiseMap; } }
		public Color[] ColorMap { get { return _map; } }

		public TerrainBuilder (int width, int height, int seed)
		{
			_seed = seed;
			_width = width;
			_height = height;
		}

		public TerrainBuilder (string file){
			LoadMap (file);
		}

		public void Generate(Gradient preset){
			_noiseModule = new Perlin ();
			((Perlin)_noiseModule).OctaveCount = 16;
			((Perlin)_noiseModule).Seed = _seed;
			_noiseMap = new Noise2D (_width, _height, _noiseModule);
			_noiseMap.GeneratePlanar (0, 4, 0, 2);
			Logger.Log ("terrain generated.");
			_map = _noiseMap.GetTexture (preset);
			Logger.Log ("bitmap generated.");
		}

		public void Save(string file){
			SaveBmp (_map, file);
			Logger.Log ("Bitmap saved.");
		}

		public string GetColorString(){
			return GetColorString (_map);
		}

		public void SaveMapStr(string file){
			SaveMapStr (GetColorString (), file);
		}

		public void LoadMap(string file){
			Image img = Bitmap.FromFile (file);
			_width = img.Width;
			_height = img.Height;
			Bitmap bmap = new Bitmap (img, img.Width, img.Height);
			_map = new Color[_width * _height];
			for (int x = 0; x < _width; x++) {
				for (int y = 0; y < _height; y++) {
					_map[x + y * _width] = bmap.GetPixel(x, y);
				}
			}
			img.Dispose ();
		}

		public Color GetColor(int x, int y){
			return _map [x + y * _width];
		}

		public Bitmap GetBitmap(){
			return GetBitmap (_map);
		}

		public Bitmap GetBitmap(Color[] colors){
			Bitmap map = new Bitmap (_width, _height);
			for (int x = 0; x < _width; x++) {
				for (int y = 0; y < _height; y++) {
					map.SetPixel(x, y, colors[x + y * _width]);
				}
			}
			return map;
		}

		private string GetColorString(Color[] colors){
			string colorStr = string.Empty;
			for (int i = 0; i < _map.Length; i++) {
				colorStr += colors[i].R + "," + colors[i].G + "," + colors[i].B + "&";
			}
			return colorStr;
		}
		
		private void SaveMapStr(string colorStr, string file){
			StreamWriter stream = File.CreateText (file);
			stream.WriteLine (colorStr);
			stream.Close ();
		}
		
		private Color[] GetColorsFromString(string colorStr)
		{
			string[] colors = colorStr.Split('&');
			Color[] colorArr = new Color[colors.Length];
			for (int x = 0; x < _width; x++)
			{
				for (int y = 0; y < _height; y++)
				{
					string[] RGB = colors[x + y * _width].Split(',');
					int R = int.Parse(RGB[0]);
					int G = int.Parse(RGB[1]);
					int B = int.Parse(RGB[2]);
					colorArr[x + y * _width] = Color.FromArgb(255, R, G, B);
				}
			}
			return colorArr;
		}
		
		private void SaveBmp(Color[] colors, string file){
			Bitmap map = GetBitmap (colors);
			map.Save(file);
			map.Dispose();
		}
	}
}

