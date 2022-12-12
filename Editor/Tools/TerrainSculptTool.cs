using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
using ProjectWS.Engine.Rendering;
using System;
using System.Collections.Generic;
using static ProjectWS.Engine.Data.Area;

namespace ProjectWS.Editor.Tools
{
    public class TerrainSculptTool : Tool
    {
        readonly Engine.Engine engine;
        public readonly WorldRenderer worldRenderer;
        readonly Editor editor;

        public Mode mode = Mode.RaiseLower;

        public enum Mode
        {
            RaiseLower,
            Flatten
        }

        public TerrainSculptTool(Engine.Engine engine, Editor editor, WorldRenderer world)
        {
            this.hasBrush = true;
            this.editor = editor;
            this.engine = engine;
            this.worldRenderer = world;
            this.isEnabled = false;
        }

        public override void Enable()
        {
            this.isEnabled = true;
            this.worldRenderer.brushParameters.mode = Engine.Rendering.ShaderParams.BrushParameters.BrushMode.Gradient;
        }

        public override void Disable()
        {
            this.isEnabled = false;
        }

        public override void Update(float deltaTime)
        {
            float flat = ((8400 & 0x7FFF) / 8.0f) - 2048.0f;
            if (this.worldRenderer == null) return;
            if (this.worldRenderer.world == null)  return;
            if (this.worldRenderer.mousePick == null) return;

            // Update brush size
            this.worldRenderer.brushParameters.size += this.engine.input.GetMouseScroll();
            this.worldRenderer.brushParameters.size = (float)Math.Clamp(this.worldRenderer.brushParameters.size, 1.0f, 100f);


            if (this.engine.input.LMB && this.editor.keyboardFocused && this.worldRenderer.brushParameters.isEnabled)
            {
                var brushSize = this.worldRenderer.brushParameters.size;
                var hitPoint = this.worldRenderer.mousePick.terrainHitPoint;
                var terrainAreaHit = this.worldRenderer.mousePick.areaHit;

                for (int a = 0; a < this.areaKernel.Length; a++)
                {
                    if (this.worldRenderer.world.chunks.TryGetValue(terrainAreaHit + this.areaKernel[a], out var chunk))
                    {
                        // TODO : currently just checking all 9 kernels, could optimize this by verifying on which kernel the brush is on
                        // TODO : future, add support for more than 3x3 areas, brush size is now stuck to max 512x512 (although it's probably enough)
                        //var cDist = Vector2.Distance(chunk.worldCoords.Xz + new Vector2(256, 256), hitPoint.Xz);
                        //if (cDist > brushSize * 2 + 256)
                        //    continue;

                        var areaPos = chunk.worldCoords.Xz;

                        for (int s = 0; s < chunk.area.subChunks.Count; s++)
                        {
                            var scDist = Vector2.Distance(chunk.area.subChunks[s].centerPosition.Xz, hitPoint.Xz);
                            // TODO: need to check better if the brush overlaps the subchunks, otherwise might run into gaps again
                            // (do square overlap math between brush square and subchunks squares (check if any of the sc corners are inside br square))
                            // (do ^ same thing to area squares)
                            if (scDist > brushSize * 2)
                                continue;

                            var sc = chunk.area.subChunks[s];
                            var subPos = new Vector2(sc.X * 32f, sc.Y * 32f) + areaPos;

                            if (sc.X < 0 || sc.X > 15 || sc.Y < 0 || sc.Y > 15)
                                continue;   // Handle painting adjacent area

                            var subchunkIndex = s;
                            var subchunk = chunk.area.subChunks[subchunkIndex];
                            for (int v = 0; v < subchunk.mesh.vertices.Length; v++)
                            {
                                float dist = Vector2.Distance(subchunk.mesh.vertices[v].position.Xz + subPos, hitPoint.Xz);
                                float brush = 1.0f - Math.Clamp(dist * (1.0f / brushSize), 0.0f, 1.0f);

                                if (this.mode == Mode.RaiseLower)
                                {
                                    subchunk.mesh.vertices[v].position.Y += brush;
                                }
                                else if (this.mode == Mode.Flatten)
                                {
                                    // float h = ((heightMap[(y + 1) * 19 + x + 1] & 0x7FFF) / 8.0f) - 2048.0f;
                                    if (subchunk.mesh.vertices[v].position.Y > flat)
                                    {
                                        subchunk.mesh.vertices[v].position.Y -= brush;
                                        if (subchunk.mesh.vertices[v].position.Y < flat)
                                            subchunk.mesh.vertices[v].position.Y = flat;
                                    }
                                    else if (subchunk.mesh.vertices[v].position.Y < flat)
                                    {
                                        subchunk.mesh.vertices[v].position.Y += brush;
                                        if (subchunk.mesh.vertices[v].position.Y > flat)
                                            subchunk.mesh.vertices[v].position.Y = flat;
                                    }
                                }
                            }

                            subchunk.mesh.ReBuild();
                        }
                    }
                }
            }
        }
    }
}
