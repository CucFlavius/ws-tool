using OpenTK.Mathematics;

namespace ProjectWS.Engine.Objects
{
    public class M3Model : GameObject
    {
        public Data.M3 m3Data;
        public readonly Matrix4 decompressMat = Matrix4.CreateScale(1.0f / 1024.0f);

        public M3Model(string path, Vector3 position, Engine engine) : base()
        {
            this.m3Data = new Data.M3(path, engine.data);
            this.m3Data.modelID = 4;
            this.m3Data.Read();

            if (this.m3Data.failedReading)
            {
                Debug.Log("M3 Failed Reading");
                return;
            }

            this.m3Data.Build();

            this.transform.SetPosition(position);
        }

        public override void Render(Matrix4 model, Shader shader)
        {
            if (!this.m3Data.isBuilt) return;

            for (int i = 0; i < this.m3Data.geometries[0].submeshes.Length; i++)
            {
                var submesh = this.m3Data.geometries[0].submeshes[i];

                if (!submesh.isBuilt) continue;
                int matSelector = submesh.materialSelector;

                if (submesh.positionCompressed)
                    shader.SetMat4("model", this.decompressMat * model);
                else
                    shader.SetMat4("model", model);

                this.m3Data.materials[matSelector].mat.Set(shader);

                submesh.Draw();
            }
        }
    }
}
