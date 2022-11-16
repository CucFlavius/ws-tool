using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
using ProjectWS.Engine.Rendering;
using System;
using System.Collections.Generic;
using static ProjectWS.Engine.Data.Area;

namespace ProjectWS.Editor.Tools
{
    public class TerrainSculpt
    {
        Engine.Engine engine;
        WorldRenderer worldRenderer;
        Editor editor;

        public TerrainSculpt(Engine.Engine engine, Editor editor, WorldRenderer world)
        {
            this.editor = editor;
            this.engine = engine;
            this.worldRenderer = world;
        }

        public void Update(float deltaTime)
        {
            if (this.worldRenderer == null)
                return;

            if (this.worldRenderer.world == null)
                return;

            if (this.worldRenderer.mousePick == null)
                return;

            if (this.engine.input.LMB && this.editor.keyboardFocused && this.worldRenderer.brushParameters.isEnabled)
            {
                var brushSize = this.worldRenderer.brushParameters.size;
                var hitPoint = this.worldRenderer.mousePick.terrainHitPoint;
                var terrainAreaHit = this.worldRenderer.mousePick.areaHit;
                var terrainSubchunkHit = this.worldRenderer.mousePick.terrainSubchunkHit;
                var subchunkLen = 3;//(int)System.Math.Ceiling(brushSize / Engine.World.World.SUBCHUNK_SIZE / 2.0f);
                //Console.WriteLine(subchunkLen);
                if (this.worldRenderer.world.chunks.TryGetValue(terrainAreaHit, out var chunk))
                {
                    var areaPos = chunk.worldMatrix.ExtractPosition().Xz;

                    for (int i = 0; i < chunk.area.subChunks.Count; i++)
                    {
                        var scDist = Vector2.Distance(chunk.area.subChunks[i].centerPosition.Xz, hitPoint.Xz);
                        if (scDist > brushSize * 2)
                            continue;

                        var sc = chunk.area.subChunks[i];
                        var subPos = new Vector2(sc.X * 32f, sc.Y * 32f) + areaPos;

                        if (sc.X < 0 || sc.X > 15 || sc.Y < 0 || sc.Y > 15)
                            continue;   // Handle painting adjacent area

                        var subchunkIndex = i;
                        var subchunk = chunk.area.subChunks[subchunkIndex];
                        for (int v = 0; v < subchunk.mesh.vertices.Length; v++)
                        {
                            float dist = Vector2.Distance(subchunk.mesh.vertices[v].position.Xz + subPos, hitPoint.Xz);
                            float brush = 1.0f - Math.Clamp(dist * (1.0f / brushSize), 0.0f, 1.0f);
                            subchunk.mesh.vertices[v].position.Y += brush;
                        }

                        subchunk.mesh.ReBuild();
                    }

                    /*
                    for (int x = -subchunkLen; x <= subchunkLen; x++)
                    {
                        for (int y = -subchunkLen; y <= subchunkLen; y++)
                        {
                            var sc = new Vector2i(terrainSubchunkHit.X + x, terrainSubchunkHit.Y + y);
                            var subPos = new Vector2(sc.X * 32f, sc.Y * 32f) + areaPos;

                            if (sc.X < 0 || sc.X > 15 || sc.Y < 0 || sc.Y > 15)
                                continue;   // Handle painting adjacent area

                            var subchunkIndex = ((sc.Y * 16) + sc.X);
                            var subchunk = chunk.area.subChunks[subchunkIndex];
                            for (int v = 0; v < subchunk.mesh.vertices.Length; v++)
                            {
                                float dist = Vector2.Distance(subchunk.mesh.vertices[v].position.Xz + subPos, hitPoint.Xz);
                                float brush = 1.0f - Math.Clamp(dist * (1.0f / brushSize), 0.0f, 1.0f);
                                subchunk.mesh.vertices[v].position.Y += brush;
                            }

                            subchunk.mesh.ReBuild();
                        }
                    }
                    */
                }
            }
        }
    }
}
