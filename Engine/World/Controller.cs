using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.World
{
    public class Controller
    {
        public bool spawn;
        public Vector2 chunkPosition;
        public Vector3 worldPosition;
        public World world;

        public Controller(World world)
        {
            this.world = world;
        }

        public void Update(Camera camera)
        {
            if (this.world == null) return;
            if (camera == null) return;

            Vector2 newChunkPosition = Utilities.WorldToChunkCoords(camera.transform.GetPosition());

            // Did we move to a different chunk, or we just spawned
            if (spawn || chunkPosition != newChunkPosition)
            {
                chunkPosition = newChunkPosition;

                if (spawn)
                {
                    camera.transform.SetPosition(this.worldPosition);
                    spawn = false;
                }

                Spiral((int)newChunkPosition.X, (int)newChunkPosition.Y);
                DeactivateChunks();
            }

            this.worldPosition = camera.transform.GetPosition();
        }

        void Spiral(int X, int Y)
        {
            Vector2 center = new Vector2(X, Y);
            this.world.activeChunks.Clear();

            int x, y, dx, dy;
            x = y = dx = 0;
            dx = 0;
            dy = -1;
            int maxEdge = World.DRAWDIST2 * 2 + 1;
            int maxI = maxEdge * maxEdge;
            for (int i = 0; i < maxI; i++)
            {
                if (((x + X) > 0) && ((x + X) < World.WORLD_SIZE) && ((y + Y) > 0) && ((y + Y) < World.WORLD_SIZE))
                {
                    int xCoord = x + X;
                    int yCoord = y + Y;
                    int lod = 0;
                    int absx = Math.Abs(x);
                    int absy = Math.Abs(y);
                    if (absx <= World.DRAWDIST0 && absy <= World.DRAWDIST0)
                        lod = 0;
                    else if (absx <= World.DRAWDIST1 && absy <= World.DRAWDIST1)
                        lod = 1;
                    else if (absx <= World.DRAWDIST2 && absy <= World.DRAWDIST2)
                        lod = 2;

                    Vector2 coords = new Vector2(xCoord, yCoord);
                    if (this.world.chunks.TryGetValue(coords, out Chunk chunk))
                    {
                        this.world.activeChunks.Add(coords, chunk);
                        chunk.SetLod(lod, center);
                    }
                }
                if ((x == y) || ((x < 0) && (x == -y)) || ((x > 0) && (x == 1 - y)))
                {
                    maxEdge = dx;
                    dx = -dy;
                    dy = maxEdge;
                }
                x += dx;
                y += dy;
            }
        }

        void DeactivateChunks()
        {
            foreach (KeyValuePair<Vector2, Chunk> item in this.world.chunks)
            {
                if (!this.world.activeChunks.ContainsKey(item.Key))
                {
                    item.Value.SetLod(-1, Vector2.Zero);
                }
            }
        }
    }
}
