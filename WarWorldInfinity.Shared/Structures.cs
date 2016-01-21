using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace WarWorldInfinity.Shared {
    public struct Color {
        public byte A;
        public byte R;
        public byte G;
        public byte B;

        public Color(byte a, byte r, byte g, byte b) {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public Color(int a, int r, int g, int b) {
            A = (byte)a;
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
        }
    }

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct Vector2Int : IEquatable<Vector2Int> {
        public int x;
        public int y;
        public Vector2Int(int _x, int _y) {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Vector2Int first, Vector2Int second) {
            return (first.x == second.x && first.y == second.y);
        }

        public static bool operator !=(Vector2Int first, Vector2Int second) {
            return (first.x != second.x || first.y != second.y);
        }

        public static Vector2Int operator +(Vector2Int first, Vector2Int second) {
            Vector2Int result = new Vector2Int();
            result.x = first.x + second.x;
            result.y = first.y + second.y;
            return result;
        }

        public static Vector2Int operator *(Vector2Int first, Vector2Int second) {
            Vector2Int result = new Vector2Int();
            result.x = first.x * second.x;
            result.y = first.y * second.y;
            return result;
        }

        public static int Distance(Vector2Int a, Vector2Int b) {
            int deltaX = b.x - a.x;
            int deltaY = b.y - a.y;
            return (int)Math.Abs(Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY)));
        }

        public static Vector2Int Interpolate(Vector2Int first, Vector2Int second, float alpha) {
            int x = (int)(alpha * second.x + (1 - alpha) * first.x);
            int y = (int)(alpha * second.y + (1 - alpha) * first.y);
            return new Vector2Int(x, y);
        }

        public override bool Equals(object obj) {
            if (obj is Vector2Int)
                return Equals((Vector2Int)obj);
            return false;
        }

        public override int GetHashCode() {
            return (x << 8) ^ y;
        }

        public override string ToString() {
            return string.Format("({0}, {1})", x, y);
        }

        public bool Equals(Vector2Int other) {
            return (x == other.x && y == other.y);
        }
    }
}
