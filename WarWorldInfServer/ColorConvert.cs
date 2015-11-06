using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarWorldInfServer {
    public static class ColorConvert {
        public static System.Drawing.Color ToSysColor(LibNoise.Color inColor) {
            return System.Drawing.Color.FromArgb(inColor.A, inColor.R, inColor.G, inColor.B);
        }

        public static LibNoise.Color ToLibColor(System.Drawing.Color inColor) {
            return new LibNoise.Color(inColor.A, inColor.R, inColor.G, inColor.B);
        }

        public static System.Drawing.Color[] SysColList(LibNoise.Color[] inColors) {
            System.Drawing.Color[] outColors = new System.Drawing.Color[inColors.Length]; 
            for (int i = 0; i < inColors.Length; i++) {
                outColors[i] = ToSysColor(inColors[i]);
            }
            return outColors;
        }

        public static LibNoise.Color[] LibColList(System.Drawing.Color[] inColors) {
            LibNoise.Color[] outColors = new LibNoise.Color[inColors.Length];
            for (int i = 0; i < inColors.Length; i++) {
                outColors[i] = ToLibColor(inColors[i]);
            }
            return outColors;
        }
    }
}
