using MathUtils;
using ProjectWS.Engine.Material;
using ProjectWS.Engine.World;

namespace ProjectWS.Engine.Objects
{
    public class M3Model : GameObject
    {
        Engine engine;
        public FileFormats.M3.File data;
        public readonly Matrix4 decompressMat = Matrix4.CreateScale(1.0f / 1024.0f);

        public M3Geometry[] geometries;
        public M3Material[] materials;
        private bool isBuilt;

        public M3Model(string path, Vector3 position, Engine engine) : base()
        {
            this.engine = engine;
            this.data = new FileFormats.M3.File(path);
            this.data.modelID = 4;
            using (MemoryStream str = engine.data.GetFileData(path))
            {
                this.data.Read(str);
            }

            if (this.data.failedReading)
            {
                Debug.Log("M3 Failed Reading");
                return;
            }

            Build();
            this.transform.SetPosition(position);
        }

        public override void Build()
        {
            this.materials = new M3Material[this.data.materials.Length];
            for (int i = 0; i < this.materials.Length; i++)
            {
                this.materials[i] = new M3Material(this.data.materials[i], this.data, this.engine.resourceManager);
                this.materials[i].Build();
            }

            this.geometries = new M3Geometry[this.data.geometries.Length];

            for (int i = 0; i < this.geometries.Length; i++)
            {
                this.geometries[i] = new M3Geometry(this.data.geometries[i]);
                this.geometries[i].Build(this.data.modelID);
            }

            this.isBuilt = true;
        }

        public override void Render(Matrix4 model, Shader shader)
        {
            if (!this.isBuilt) return;
            if (this.geometries == null) return;

            for (int g = 0; g < this.geometries.Length; g++)
            {
                if (this.geometries[g] != null && this.geometries[g].meshes != null)
                {
                    for (int i = 0; i < this.geometries[g].meshes.Length; i++)
                    {
                        var mesh = this.geometries[g].meshes[i];

                        if (!mesh.isBuilt || mesh.data == null) continue;

                        int matSelector = mesh.data.materialSelector;

                        var mat = model;

                        if (mesh.positionCompressed)
                            mat = this.decompressMat * model;

                        shader.SetMat4("model", ref mat);

                        this.materials[matSelector].SetToShader(shader);

                        mesh.Draw();
                    }
                }
            }
        }
    }
}
