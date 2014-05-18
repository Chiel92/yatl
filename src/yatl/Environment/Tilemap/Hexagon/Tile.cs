using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace yatl.Environment.Tilemap.Hexagon
{
    interface ITile
    {
        int X { get; }
        int Y { get; }
    }

    struct Tile<TTileInfo> : ITile, IEquatable<Tile<TTileInfo>>
    {
        private readonly Tilemap<TTileInfo> tilemap;

        private readonly int x;
        private readonly int y;

        public int X { get { return this.x; } }
        public int Y { get { return this.y; } }

        public Tile(Tilemap<TTileInfo> tilemap, int x, int y)
        {
            if (tilemap == null)
                throw new ArgumentNullException("tilemap");

            this.tilemap = tilemap;
            this.x = x;
            this.y = y;
        }

        public Vector2 Xy { get { return new Vector2(this.x, this.y); } }

        public TTileInfo Info
        {
            get { return this.tilemap[this]; }
        }

        public bool IsValid
        {
            get { return this.tilemap.IsValidTile(this); }
        }

        public Tile<TTileInfo> Neighbour(Direction direction)
        {
            return this.Neighbour(direction.Step());
        }

        public Tile<TTileInfo> Neighbour(Step step)
        {
            return new Tile<TTileInfo>(
                this.tilemap,
                this.x + step.X,
                this.y + step.Y
                );
        }

        public IEnumerable<Tile<TTileInfo>> Neighbours
        {
            get { return this.PossibleNeighbours().Where(t => t.IsValid); }
        }

        public bool Equals(Tile<TTileInfo> other)
        {
            return this.x == other.x && this.y == other.y && this.tilemap == other.tilemap;
        }
    }
}