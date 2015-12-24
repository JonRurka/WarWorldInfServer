using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using WarWorldInfinity.LibNoise;

namespace WarWorldInfinity
{
	public class GradiantPresetLoader
	{
		public struct PresetSerializer{
			public string name;
			public List<GradientPresets.GradientKeyData> preset;

			public PresetSerializer( string name, List<GradientPresets.GradientKeyData> preset){
				this.name = name;
				this.preset = new List<GradientPresets.GradientKeyData>(preset);
			}
		}

		private Dictionary<string, List<GradientPresets.GradientKeyData>> _presets;

		public GradiantPresetLoader (string folder)
		{
			_presets = new Dictionary<string, List<GradientPresets.GradientKeyData>> ();
			if (!Directory.Exists (folder)) {
				Directory.CreateDirectory(folder);
				return;
			}

			string[] presetFiles = Directory.GetFiles (folder, "*.json");
			for (int i = 0; i < presetFiles.Length; i++) {
				PresetSerializer preset = FileManager.LoadObject<PresetSerializer>(presetFiles[i], false);
				_presets.Add(preset.name, preset.preset);
			}
		}

		public List<GradientPresets.GradientKeyData> GetPreset(string name){
			if (_presets.ContainsKey (name)) {
				List<GradientPresets.GradientKeyData> preset = _presets [name];
				return preset;
			}
			return null;
		}
	}
}

