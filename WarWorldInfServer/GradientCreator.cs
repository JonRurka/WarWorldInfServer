using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace WarWorldInfServer {
    class GradientCreator {
        public static Dictionary<string, Color[]> TextureFiles = new Dictionary<string, Color[]>();

        public static LibNoise.Gradient CreateGradientServer(List<LibNoise.GradientPresets.GradientKeyData> keyData) {
            List<LibNoise.GradientKey> keys = new List<LibNoise.GradientKey>();
            for (int i = 0; i < keyData.Count; i++) {
                if (keyData[i].isImage) {
                    List<LibNoise.Color[]> images = new List<LibNoise.Color[]>();
                    string[] files = keyData[i].imageFiles.ToArray();
                    for (int j = 0; j < files.Length; j++) {
                        string file = GameServer.Instance.AppDirectory + "terrainTextures" + Path.DirectorySeparatorChar + files[j];
                        if (File.Exists(file)) {
                            Bitmap map = new Bitmap(Bitmap.FromFile(file));
                            //Logger.Log("{0}: {1} {2}", file, map.Width.ToString(), map.Height.ToString());
                            Color[] imgColors = new Color[map.Width * map.Height];
                            for (int x = 0; x < map.Width; x++) {
                                for (int y = 0; y < map.Height; y++) {
                                    imgColors[x + y * map.Width] = map.GetPixel(x, y);
                                }
                            }
                            if (!TextureFiles.ContainsKey(files[j]))
                                TextureFiles.Add(files[j], imgColors);
                            keyData[i].images.Add(ColorConvert.LibColList(imgColors));
                            images.Add(ColorConvert.LibColList(imgColors));
                        }
                    }
                    keys.Add(new LibNoise.GradientKey(images, 10, 10, keyData[i].time));
                }
                else {

                    keys.Add(new LibNoise.GradientKey(keyData[i].color, keyData[i].time));
                }
            }
            return LibNoise.GradientPresets.CreateGradient(keys);
        }
    }
}
