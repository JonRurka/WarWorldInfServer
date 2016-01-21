using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.Shared;
using Structure = WarWorldInfinity.Structures.Structure;

namespace WarWorldInfinity.Units {
    public class Squad {
        public struct SquadSave {
            public string name;
            public string ident;
            public Vector2Int location;
            public TravelData data;
            public bool atStructure;

            public SquadSave(string name, string ident, Vector2Int location, TravelData data, bool atStructure) {
                this.name = name;
                this.ident = ident;
                this.location = location;
                this.data = data;
                this.atStructure = atStructure;
            }
        }

        public class TravelData {
            public Vector2Int start;
            public Vector2Int end;
            public Vector2Int current;
            public int tickProgress;
            public int maxTicks;
            public bool traveling;
            public bool reverse;

            public TravelData(Vector2Int start, Vector2Int end) {
                this.start = start;
                this.end = end;
                tickProgress = 0;
                maxTicks = Vector2Int.Distance(start, end) / 50;
                traveling = true;
                reverse = false;
                current = start;
            }

            public TravelData(TravelData origin) {
                start = origin.start;
                end = origin.end;
                current = origin.current;
                tickProgress = origin.tickProgress;
                maxTicks = origin.maxTicks;
                traveling = origin.traveling;
                reverse = origin.reverse;
            }

            public Vector2Int UpdateLocation() {
                if (traveling) {
                    if (!reverse)
                        tickProgress++;
                    else
                        tickProgress--;
                    float alpha = Scale(tickProgress, 0, maxTicks, 0, 1);
                    current = Vector2Int.Interpolate(start, end, alpha);
                    traveling = tickProgress >= maxTicks || tickProgress == 0;
                }
                return current;
            }

            public void Reverse() {
                reverse = true;
            }

            private float Scale(float value, float oldMin, float oldMax, float newMin, float newMax) {
                return newMin + (value - oldMin) * (newMax - newMin) / (oldMax - oldMin);
            }
        }

        public string name { get; private set; }
        public string ident { get; private set; }
        public Vector2Int location { get; private set; }
        public TravelData data { get; private set; }
        public Structure defendedStructure { get; private set; }
        public User owner { get; private set; }
        public bool atStructure { get; private set; }

        public Squad(User owner, string name, Structure structure) {
            this.owner = owner;
            this.name = name;
            ident = owner.Name + "_" + name;
            atStructure = true;
            defendedStructure = structure;
            location = defendedStructure.Location;
        }

        public Squad(User owner, SquadSave save) {
            this.owner = owner;
            name = save.name;
            ident = save.ident;
            location = save.location;
            atStructure = save.atStructure;
            if (atStructure)
                if (GameServer.Instance.Structures.OpExists(location))
                    defendedStructure = GameServer.Instance.Structures.GetStructure(location);
                else
                    atStructure = false;
            if (!atStructure && save.data != null) {
                data = new TravelData(save.data);
            }
        }

        public SquadSave Save() {
            return new SquadSave(name, ident, location, new TravelData(data), atStructure);
        }

        public virtual void TickUpdate() {
            if (data != null) {
                location = data.UpdateLocation();
                if (!data.traveling) {
                    data = null;
                    if (GameServer.Instance.Structures.OpExists(location))
                        defendedStructure = GameServer.Instance.Structures.GetStructure(location);
                    else
                        defendedStructure = owner.CreateStructure(location, Structure.StructureType.None, true);
                }
                
            }
        }

        public void Reverse() {
            if (data != null)
                data.Reverse();
        }

        public virtual void Deploy(Vector2Int start, Vector2Int finish) {
            data = new TravelData(start, finish);
        }
    }
}
