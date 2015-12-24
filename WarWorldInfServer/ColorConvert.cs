using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.Shared;

namespace WarWorldInfinity {
    public static class ColorConvert {
        public static System.Drawing.Color ToSysColor(Color inColor) {
            return System.Drawing.Color.FromArgb(inColor.A, inColor.R, inColor.G, inColor.B);
        }

        public static Color ToLibColor(System.Drawing.Color inColor) {
            return new Color(inColor.A, inColor.R, inColor.G, inColor.B);
        }

        public static System.Drawing.Color[] SysColList(Color[] inColors) {
            System.Drawing.Color[] outColors = new System.Drawing.Color[inColors.Length]; 
            for (int i = 0; i < inColors.Length; i++) {
                outColors[i] = ToSysColor(inColors[i]);
            }
            return outColors;
        }

        public static Color[] LibColList(System.Drawing.Color[] inColors) {
            Color[] outColors = new Color[inColors.Length];
            for (int i = 0; i < inColors.Length; i++) {
                outColors[i] = ToLibColor(inColors[i]);
            }
            return outColors;
        }
    }
}
