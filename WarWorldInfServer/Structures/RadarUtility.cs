using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.Shared;

namespace WarWorldInfinity.Structures {    
    public static class RadarUtility {
        public struct RadarData {
            public Vector2Int position;
            public int radius;
            public RadarData(Vector2Int position, int radius) {
                this.position = position;
                this.radius = radius;
            }
        }
        /*public static Vector2Int[] GetVisibleObjects(Vector2Int center, Vector2Int topLeft, Vector2Int bottomRight, float radius) {
            for(int x = topLeft.x; x < bottomRight.x; x++) {
                for (int y = topLeft.y; y > bottomRight.y; y--) {
                    
                }
            }
        }*/

        public static Vector2Int[] GetVisibleObjects(RadarData[] radars, Vector2Int[] points) {
            List<Vector2Int> seenPoints = new List<Vector2Int>();
            for(int i = 0; i < points.Length; i++) {
                if (CanSeePoint(radars, points[i])) {
                    seenPoints.Add(points[i]);
                }
            }
            return seenPoints.ToArray();
        }

        public static Vector2Int[] GetVisibleObjects(RadarData radar, Vector2Int[] points) {
            List<Vector2Int> seenPoints = new List<Vector2Int>();
            for (int i = 0; i < points.Length; i++) {
                if (CanSeePoint(radar, points[i])) {
                    seenPoints.Add(points[i]);
                }
            }
            return seenPoints.ToArray();
        }

        public static bool CanSeePoint(RadarData[] radar, Vector2Int point) {
            for (int i = 0; i < radar.Length; i++) {
                if (CanSeePoint(radar[i], point))
                    return true;
            }
            return false;
        }

        public static bool CanSeePoint(RadarData radar, Vector2Int point) {
            Vector2Int center = radar.position;
            int radius = radar.radius;
            return (System.Math.Sqrt(System.Math.Pow(point.x - center.x, 2) - System.Math.Pow(point.y - center.y, 2))) <= radius;
        }
    }
}
