using BaseX;
using System.Collections.Generic;
using System.Linq;

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
            mesh.SetVertexCount(Iterations * 16 + 40); // 4 vertices per segment per corner + 16

            List<int> FrontSideEdge = new List<int>();
            List<int> BackSideEdge = new List<int>();

            //define "last" (but technically first) edge, this is the same as the end of the last corner (same positions but not same vertices)

            mesh.SetVertex(0, new float3(Size.X / 2 - SecondaryBevel, Size.Y / -2 - PrimaryBevel, Thickness / 2));
            mesh.SetVertex(1, new float3(Size.X / 2, Size.Y / -2 - PrimaryBevel, Thickness / 2 - SecondaryBevel));
            mesh.SetVertex(2, new float3(Size.X / 2, Size.Y / -2 - PrimaryBevel, Thickness / -2 + SecondaryBevel));
            mesh.SetVertex(3, new float3(Size.X / 2 - SecondaryBevel, Size.Y / -2 - PrimaryBevel, Thickness / -2));

            //FrontSideEdge.Append(0);
            //BackSideEdge.Append(3);

            //calculate corner meshes, add edge vertices to corresponding list
            for (int corner=0; corner < 4; corner++)
            {

                //set positional multiplier
                float CornerAngle = corner * -90;

                float2 InwardsOffset = new float2(PrimaryBevel, PrimaryBevel);

                for (int segment=0; segment < iterations; segment++)
                {
                    int SegmentIndex = corner * iterations * 4 + segment + 4;

                    //the order of the vertices is always from front to back, and the edge is done CCW

                    float3 v1 = new float3(MathX.Rotate(Size / 2 - InwardsOffset + GetSquirclePoint(PrimaryBevel, (segment) / Iterations, SecondaryBevel), CornerAngle), Thickness / 2);
                    float3 v2 = new float3(MathX.Rotate(Size / 2 - InwardsOffset + GetSquirclePoint(PrimaryBevel, (segment) / Iterations), CornerAngle), Thickness / 2 - SecondaryBevel);
                    float3 v3 = new float3(MathX.Rotate(Size / 2 - InwardsOffset + GetSquirclePoint(PrimaryBevel, (segment) / Iterations), CornerAngle), Thickness / -2 + SecondaryBevel);
                    float3 v4 = new float3(MathX.Rotate(Size / 2 - InwardsOffset + GetSquirclePoint(PrimaryBevel, (segment) / Iterations, SecondaryBevel), CornerAngle), Thickness / -2);

                    mesh.SetVertex(SegmentIndex, v1);
                    mesh.SetVertex(SegmentIndex + 1, v2);
                    mesh.SetVertex(SegmentIndex + 2, v3);
                    mesh.SetVertex(SegmentIndex + 3, v4);

                    mesh.AddQuadAsTriangles(SegmentIndex - 4, SegmentIndex - 3, SegmentIndex, SegmentIndex + 1);
                    mesh.AddQuadAsTriangles(SegmentIndex - 3, SegmentIndex - 2, SegmentIndex + 1, SegmentIndex + 2);
                    mesh.AddQuadAsTriangles(SegmentIndex - 2, SegmentIndex - 1, SegmentIndex + 2, SegmentIndex + 3);

                    FrontSideEdge.Append(SegmentIndex);
                    BackSideEdge.Append(SegmentIndex + 3);

                }
            }

            mesh.AddTriangleFan(FrontSideEdge);
            mesh.AddTriangleFan(BackSideEdge);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }

        public override void Update()
        {
        }

        private float2 GetSquirclePoint(float radius, float lerp, float offset = 0)
        {
            //I'm using superellipses instead of circular corners because it looks better, but it's making bevelling more difficult

            float Lerp = lerp * 0.5f * MathX.PI;

            float2 Derivative = new float2(-0.75f * MathX.Sin(Lerp) * MathX.Pow(MathX.Cos(Lerp),-0.25f), 0.75f * MathX.Cos(Lerp) * MathX.Pow(MathX.Sin(Lerp), -0.25f));
            float2 Normal = MathX.Rotate90CCW(Derivative.Normalized);

            float2 Point = new float2(radius * MathX.Pow(MathX.Cos(Lerp), 0.75f), radius * MathX.Pow(MathX.Sin(Lerp), 0.75f)) + Normal * offset;

            return Point;
        }
    }
}
