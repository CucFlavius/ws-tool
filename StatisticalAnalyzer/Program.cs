using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using ProjectWS;
using ProjectWS.Engine;
using ProjectWS.Engine.Data;

namespace StatisticalAnalyzer
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            CheckM3Files(@"G:\Reverse Engineering\GameData\Wildstar 1.7.8.16042 Extracted\AIDX\Art\");
            //CheckAreaFiles(@"G:\Reverse Engineering\GameData\Wildstar 1.7.8.16042 Extracted\AIDX\Map");
            //ExtractShaders(@"G:\Reverse Engineering\GameData\Wildstar 1.7.8.16042 Extracted\AIDX\Shaders", @"G:\Reverse Engineering\WildStar\DecompiledShaders");
            //CheckSkyFiles(@"G:\Reverse Engineering\GameData\Wildstar 1.7.8.16042 Extracted\AIDX\Sky");

            //var area = new ProjectWS.Engine.Data.Area(@"G:\Reverse Engineering\GameData\Wildstar 1.7.8.16042 Extracted\AIDX\Map\BattlegroundHallsoftheBloodsworn\BattlegroundHallsoftheBloodsworn.3f3d.area");
            //area.Read();

            //CalculateSkyCoeffs(@"G:\Reverse Engineering\GameData\Wildstar 1.7.8.16042\Data\Sky\Adventure_Galeras3.sky");
        }




        static float[][] coeffs = new float[9][];

        static void CalculateSkyCoeffs(string path)
        {
            for (int i = 0; i < 9; i++)
            {
                coeffs[i] = new float[3];
            }

            Sky sky = new Sky(path);
            sky.Read();

            Vector4[] colors = new Vector4[]
            {
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
            };

            Vector4[] angles = new Vector4[]
            {
                new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0, 0, 0.0f, 0.0f),
                new Vector4(0, 0, 0.0f, 0.0f),
                new Vector4(0, 0, 0.0f, 0.0f),
                new Vector4(0, 0, 0.0f, 0.0f),
            };

            //for (int i = 0; i < sky.skyDataBlock0.angleAndColorUnk0.Length; i++)
            for (int i = 0; i < 6; i++)
            {
                Vector4 angle = angles[i];
                float[] hdr = new float[] { colors[i].X, colors[i].Y, colors[i].Z };
                float yaw = angle.X;
                float pitch = angle.Y;
                /*
                var aandc = sky.skyDataBlock0.angleAndColorUnk0[i];
                var data = aandc.data[0];
                float mult = 1.0f;
                float[] hdr = new float[] { data.color.X * mult, data.color.Y * mult, data.color.Z * mult };
                //Console.WriteLine("Angle " + data.angle.X + " " + data.angle.Y + " " + data.angle.Z);
                Console.WriteLine("Color " + data.color.X + " " + data.color.Y + " " + data.color.Z);
                float yaw = data.angle.X;
                float pitch = data.angle.Y;
                */

                //float x = MathF.Cos(yaw) * MathF.Cos(pitch);
                //float y = MathF.Sin(yaw) * MathF.Cos(pitch);
                //float z = MathF.Sin(pitch);

                float phi = yaw * MathF.PI / 180f;
                float theta = pitch * MathF.PI / 180f;

                float x = MathF.Sin(theta) * MathF.Cos(phi);         /* Cartesian components */
                float y = MathF.Sin(theta) * MathF.Sin(phi);
                float z = MathF.Cos(theta);

                //float x = MathF.Sin(yaw);
                //float y = -(MathF.Sin(pitch) * MathF.Cos(yaw));
                //float z = -(MathF.Cos(pitch) * MathF.Cos(yaw));

                float domega = 1.0f;// MathF.Sin(theta);
                updatecoeffs(hdr, domega, x, y, z);
            }

            for (int i = 0; i < 9; i++)
            {
                //Console.WriteLine($"vec3({coeffs[i][0]}, {coeffs[i][1]}, {coeffs[i][2]}),");
            }

            Console.WriteLine($"cb1_0 = float4({coeffs[0][0]}, {coeffs[0][1]}, {coeffs[0][2]}, {coeffs[1][0]});");
            Console.WriteLine($"cb1_1 = float4({coeffs[1][1]}, {coeffs[1][2]}, {coeffs[2][0]}, {coeffs[2][1]});");
            Console.WriteLine($"cb1_2 = float4({coeffs[2][2]}, {coeffs[3][0]}, {coeffs[3][1]}, {coeffs[3][2]});");

            Console.WriteLine($"cb1_3 = float4({coeffs[4][0]}, {coeffs[4][1]}, {coeffs[4][2]}, {coeffs[5][0]});");
            Console.WriteLine($"cb1_4 = float4({coeffs[5][1]}, {coeffs[5][2]}, {coeffs[6][0]}, {coeffs[6][1]});");
            Console.WriteLine($"cb1_5 = float4({coeffs[6][2]}, {coeffs[7][0]}, {coeffs[7][1]}, {coeffs[7][2]});");

            Console.WriteLine($"cb1_6 = float4({coeffs[8][0]}, {coeffs[8][1]}, {coeffs[8][2]}, 0.00);");



        }


        static void updatecoeffs(float[] hdr, float domega, float x, float y, float z)
        {
            /****************************************************************** 
             Update the coefficients (i.e. compute the next term in the
             integral) based on the lighting value hdr[3], the differential
             solid angle domega and cartesian components of surface normal x,y,z

             Inputs:  hdr = L(x,y,z) [note that x^2+y^2+z^2 = 1]
                      i.e. the illumination at position (x,y,z)

                      domega = The solid angle at the pixel corresponding to 
                  (x,y,z).  For these light probes, this is given by 

                  x,y,z  = Cartesian components of surface normal

             Notes:   Of course, there are better numerical methods to do
                      integration, but this naive approach is sufficient for our
                  purpose.

            *********************************************************************/

            int col;
            for (col = 0; col < 3; col++)
            {
                float c; /* A different constant for each coefficient */

                /* L_{00}.  Note that Y_{00} = 0.282095 */
                c = 0.282095f;
                coeffs[0][col] += hdr[col] * c * domega;

                /* L_{1m}. -1 <= m <= 1.  The linear terms */
                //c = 0.488603f;
                c = 0.30765f;
                coeffs[1][col] += hdr[col] * (c * y) * domega;   /* Y_{1-1} = 0.488603 y  */
                coeffs[2][col] += hdr[col] * (c * z) * domega;   /* Y_{10}  = 0.488603 z  */
                coeffs[3][col] += hdr[col] * (c * x) * domega;   /* Y_{11}  = 0.488603 x  */

                /* The Quadratic terms, L_{2m} -2 <= m <= 2 */

                /* First, L_{2-2}, L_{2-1}, L_{21} corresponding to xy,yz,xz */
                //c = 1.092548f;
                coeffs[4][col] += hdr[col] * (c * x * y) * domega; /* Y_{2-2} = 1.092548 xy */
                coeffs[5][col] += hdr[col] * (c * y * z) * domega; /* Y_{2-1} = 1.092548 yz */
                coeffs[7][col] += hdr[col] * (c * x * z) * domega; /* Y_{21}  = 1.092548 xz */

                /* L_{20}.  Note that Y_{20} = 0.315392 (3z^2 - 1) */
                //c = 0.315392f;
                coeffs[6][col] += hdr[col] * (c * (3 * z * z - 1)) * domega;

                /* L_{22}.  Note that Y_{22} = 0.546274 (x^2 - y^2) */
                //c = 0.546274f;
                coeffs[8][col] += hdr[col] * (c * (x * x - y * y)) * domega;
            }
        }


        static void CheckM3Files(string location)
        {
            string[] m3Files = Directory.GetFiles(location, "*.m3", SearchOption.AllDirectories);

            HashSet<long> collected = new HashSet<long>();

            foreach (var filePath in m3Files)
            {
                var m3 = new M3(filePath);
                m3.Read();

                if (m3.failedReading)
                    continue;

                for (int g = 0; g < m3.materials.Length; g++)
                {
                    var val = m3.materials[g].unk7;

                    if (!collected.Contains(val))
                    {
                        collected.Add(val);
                        //Console.WriteLine(filePath);
                    }
                }

                //if (!collected.Contains(m3.unk1a))
                //    collected.Add(m3.unk1a);

                /*
                if (m3.unk100 != null && m3.unk100.data.Length > 0)
                {
                    Console.WriteLine(filePath);
                }
                */
            }

            foreach (var item in collected)
            {
                Console.WriteLine(item);
            }
        }

        static void CheckAreaFiles(string location)
        {
            string[] areaFiles = Directory.GetFiles(location, "*.area", SearchOption.AllDirectories);

            HashSet<long> collected = new HashSet<long>();

            foreach (var filePath in areaFiles)
            {
                var area = new Area(filePath);
                area.Read();
                /*
                for (int i = 0; i < area.subChunks.Count; i++)
                {
                    var check = area.subChunks[i].
                    //Console.WriteLine(filePath);

                    if (!collected.Contains(check))
                    {
                        collected.Add(check);
                    }
                }
                */
            }

            foreach (var item in collected)
            {
                Console.WriteLine(item);
            }
        }

        static void CheckSkyFiles(string location)
        {
            // G:\Reverse Engineering\GameData\Wildstar 1.7.8.16042 Extracted\AIDX\Sky\Arcterra_EXT_Bones.sky
            string[] areaFiles = Directory.GetFiles(location, "*.sky", SearchOption.AllDirectories);

            HashSet<float> collected = new HashSet<float>();

            foreach (var filePath in areaFiles)
            {
                var sky = new ProjectWS.Engine.Data.Sky(filePath);
                sky.Read();

                if (sky.fogSettings.timestamps.Length > 0)
                {
                    Console.WriteLine(filePath);
                }

                /*
                if (sky.sunLightColor.timestamps.Length > 0)
                {
                    var check = sky.sunLightColor.timestamps[0];

                    if (!collected.Contains(check))
                    {
                        collected.Add(check);
                    }
                }
                */
            }

            foreach (var item in collected)
            {
                Console.WriteLine(item);
            }
        }

        static void ExtractShaders(string shoPath, string outputPath)
        {
            string[] metafiles = Directory.GetFiles(shoPath, "*_0*");

            for (int i = 0; i < metafiles.Length; i++)
            {
                var fileName = Path.GetFileNameWithoutExtension(metafiles[i]);
                Console.WriteLine(fileName);

                try
                {
                    Sho sho = new Sho(metafiles[i]);
                    sho.Read();
                    var folder = $"{outputPath}/{fileName}";

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    if (sho.variants != null)
                    {
                        for (int v = 0; v < sho.variants.Length; v++)
                        {
                            if (sho.variants[v] != null)
                            {
                                var dxbcPath = $"{folder}/{fileName}_{v:D3}.dxbc";
                                var glslPath = $"{folder}/{fileName}_{v:D3}.glsl";
                                if (sho.variants.Length == 1)
                                {
                                    dxbcPath = $"{folder}/{fileName}.dxbc";
                                    glslPath = $"{folder}/{fileName}.glsl";
                                }

                                if (sho.variants[v].data != null)
                                {
                                    File.WriteAllBytes(dxbcPath, sho.variants[v].data);
                                    DXBC2HLSL(dxbcPath, glslPath);
                                    //File.Delete(dxbcPath);
                                }
                            }
                        }
                    }

                    if (IsDirectoryEmpty(folder))
                        Directory.Delete(folder);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine(metafiles[i]);
                    Console.ResetColor();
                    return;
                }
            }
        }

        static void DXBC2HLSL(string inputFile, string outputFile)
        {
            string execPath = AppDomain.CurrentDomain.BaseDirectory;
            string dxilSpirv = execPath + @"/DXBCDecompile/dxil-spirv.exe";

            var startInfo = new ProcessStartInfo
            {
                FileName = dxilSpirv,
                Arguments = $"\"{inputFile}\" --glsl --output \"{outputFile}\"",
                //UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);

            Stopwatch timer = new Stopwatch();
            timer.Start();

            while (true)
            {
                if (process != null)
                {
                    if (!process.StandardOutput.EndOfStream)
                    {
                        //string line = process.StandardOutput.ReadLine();
                    }

                    if (timer.Elapsed.TotalSeconds >= 60) // 1 minute
                    {
                        process.CloseMainWindow();
                        process.Close();
                        break;
                    }

                    if (process.HasExited)
                        break;
                }
            }

            timer.Stop();
        }

        static bool IsDirectoryEmpty(string path)
        {
            IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
            using (IEnumerator<string> en = items.GetEnumerator())
            {
                return !en.MoveNext();
            }
        }

    }
}