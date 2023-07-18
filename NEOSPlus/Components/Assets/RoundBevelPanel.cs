using BaseX;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace FrooxEngine
{
    public class RoundBevelPanel : MeshXShape
    {
        public float2 Size;
        public float Thickness;
        public float PrimaryBevel;
        public float SecondaryBevel;
        public int Iterations;

        public RoundBevelPanel(MeshX mesh, float2 size, float thickness, float primarybevel, float secondarybevel, int iterations) : base(mesh)
        {
            Size = size;
            Thickness = thickness;
            PrimaryBevel = primarybevel;
            SecondaryBevel = secondarybevel;
            Iterations = iterations;




            mesh.Clear();
            mesh.SetVertexCount(Iterations * 16 + 18); // 4 vertices per segment per corner + 4 

            List<int> FrontSideEdge = new List<int>();
            List<int> BackSideEdge = new List<int>();

            int index = 0;

            //calculate corner meshes, add edge vertices to corresponding list
            for (int corner=0; corner < 4; corner++)
            {
                float2 CornerMultiplier = float2.Zero;

                //set positional multiplier
                switch(corner)
                {
                    case 0: CornerMultiplier = new float2(1, 1); break;
                    case 1: CornerMultiplier = new float2(-1, 1); break;
                    case 2: CornerMultiplier = new float2(-1, -1); break;
                    case 3: CornerMultiplier = new float2(1, 1); break;
                }

                for (int segment=0; segment < iterations; segment++)
                {
                    //first front and back vertices
                    var PointAF = new float3(GetSquirclePoint(0.1f, segment/(Iterations + 1)) + Size / new float2(2, 2) * CornerMultiplier, thickness / 2 - secondarybevel);
                    var PointBF = new float3(GetSquirclePoint(0.1f, segment/(iterations + 1)) + Size / new float2(2, 2) * CornerMultiplier, thickness / -2 + secondarybevel);
                    //second front and back vertices (going ccw)
                    var PointAB = new float3(GetSquirclePoint(0.1f, (segment + 1)/(iterations + 1)) + Size / new float2(2, 2) * CornerMultiplier, thickness / 2 - secondarybevel);
                    var PointBB = new float3(GetSquirclePoint(0.1f, (segment + 1)/(iterations + 1)) + Size / new float2(2, 2) * CornerMultiplier, thickness / -2 + secondarybevel);

                    mesh.SetVertex(index, PointAF);
                    mesh.SetVertex(index+1, PointBF);
                    mesh.SetVertex(index+2, PointAB);
                    mesh.SetVertex(index+3, PointBB);

                    mesh.AddQuadAsTriangles(index, index + 1, index + 2, index + 3);

                    index += 2; // every corner point gets set twice, I hope it's not a problem but I haven't tested it

                }
            }
        }

        public override void Update()
        { 
        }

        private float2 GetSquirclePoint(float radius, float lerp)
        {
            //I'm using superellipses instead of circular corners because it looks better, but it's making bevelling more difficult

            float Lerp = lerp * 4 * MathX.PI;

            float PointX = radius * MathX.Sign(MathX.Cos(Lerp)) * MathX.Pow(MathX.Abs(MathX.Cos(Lerp)), 0.75f);
            float PointY = radius * MathX.Sign(MathX.Sin(Lerp)) * MathX.Pow(MathX.Abs(MathX.Sin(Lerp)), 0.75f);

            float2 Point = new float2(PointX, PointY);

            return Point;
        }
    }
}
