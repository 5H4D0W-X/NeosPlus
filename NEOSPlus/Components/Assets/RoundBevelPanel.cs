using BaseX;
using System;
using System.Collections.Generic;

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
            PrimaryBevel = MathX.Min(primarybevel, Size.X / 2, Size.Y / 2);
            SecondaryBevel = MathX.Min(secondarybevel, Thickness / 2, PrimaryBevel);
            Iterations = iterations;

            mesh.Clear();
            mesh.SetVertexCount((Iterations + 1) * 16 + 4); // 4 vertices per segment per corner + 16

            List<int> FrontSideEdge = new List<int>();
            List<int> BackSideEdge = new List<int>();
            

            //define "last" (but technically first) edge, this is the same as the end of the last corner (same positions but not same vertices)

            mesh.SetVertex(0, new float3(Size.X / 2 - SecondaryBevel, Size.Y / -2 + PrimaryBevel, Thickness / 2));
            mesh.SetVertex(1, new float3(Size.X / 2, Size.Y / -2 + PrimaryBevel, Thickness / 2 - SecondaryBevel));
            mesh.SetVertex(2, new float3(Size.X / 2, Size.Y / -2 + PrimaryBevel, Thickness / -2 + SecondaryBevel));
            mesh.SetVertex(3, new float3(Size.X / 2 - SecondaryBevel, Size.Y / -2 + PrimaryBevel, Thickness / -2));


            float2[] PositionMultipliers = {new float2(1f, 1f), new float2(-1f, 1f), new float2(-1f, -1f), new float2(1f, -1f)};

            //calculate corner meshes, add edge vertices to corresponding list
            for (int corner=0; corner < 4; corner++)
            {

                //set positional multiplier
                float CornerAngle = corner * 0.5f * MathX.PI;

                float2 PositionMultiplier = PositionMultipliers[corner];

                float2 Offset = PositionMultiplier * (Size / 2 - new float2(PrimaryBevel, PrimaryBevel));

                for (int segment=0; segment <= Iterations; segment++)
                {
                    int SegmentIndex = (corner * (Iterations + 1) * 4) + (segment * 4) + 4;

                    //the order of the vertices is always from front to back, and the edge is done CCW

                    float CornerLerp = (float)segment / (float)Iterations; //goes from 0 to 1 throughout each corner, 17 passes in total leading to [iterations] corner segments and 1 between each corner, which works as a flat side

                    var v1 = new float3(Offset + MathX.Rotate(GetSquirclePoint(PrimaryBevel, CornerLerp, SecondaryBevel), CornerAngle), Thickness / 2);
                    var v2 = new float3(Offset + MathX.Rotate(GetSquirclePoint(PrimaryBevel, CornerLerp), CornerAngle), Thickness / 2 - SecondaryBevel);
                    var v3 = new float3(Offset + MathX.Rotate(GetSquirclePoint(PrimaryBevel, CornerLerp), CornerAngle), Thickness / -2 + SecondaryBevel);
                    var v4 = new float3(Offset + MathX.Rotate(GetSquirclePoint(PrimaryBevel, CornerLerp, SecondaryBevel), CornerAngle), Thickness / -2);

                    mesh.SetVertex(SegmentIndex, v1);
                    mesh.SetVertex(SegmentIndex + 1, v2);
                    mesh.SetVertex(SegmentIndex + 2, v3);
                    mesh.SetVertex(SegmentIndex + 3, v4);


                    mesh.AddQuadAsTriangles(SegmentIndex - 4, SegmentIndex - 3, SegmentIndex + 1, SegmentIndex );
                    mesh.AddQuadAsTriangles(SegmentIndex - 3, SegmentIndex - 2, SegmentIndex + 2, SegmentIndex + 1);
                    mesh.AddQuadAsTriangles(SegmentIndex - 2, SegmentIndex - 1, SegmentIndex + 3, SegmentIndex + 2);

                    FrontSideEdge.Add(SegmentIndex);
                    BackSideEdge.Add(SegmentIndex + 3);
                }
            }

            

            mesh.AddTriangleFan(FrontSideEdge);
            mesh.AddTriangleFan(BackSideEdge, true);
            mesh.GetMergedDoubles(0.0025);
            mesh.ConvertToFlatShading();
        }

        public override void Update()
        {
        }

        private float2 GetSquirclePoint(float radius, float lerp, float offset = 0)
        {
            //I'm using superellipses instead of circular corners because it looks better, but it's making bevelling more difficult
            float2 Point;

            switch (lerp)
            {
                case 0:
                    Point = new float2(radius - offset, 0); break;

                case 1:
                    Point = new float2(0, radius - offset); break;

                default:
                    float Lerp = lerp * 0.5f * MathX.PI;

                    float2 Normal = new float2(0.75f * MathX.Cos(Lerp) * NPow(MathX.Sin(Lerp), -0.25f), 0.75f * MathX.Sin(Lerp) * NPow(MathX.Cos(Lerp), -0.25f));

                    Normal = Normal.Normalized;

                    Point = new float2(radius * MathX.Pow(MathX.Cos(Lerp), 0.75f), radius * MathX.Pow(MathX.Sin(Lerp), 0.75f)) - (Normal * offset);

                    break;
            }
            return Point;
        }

        private float NPow(float num, float exponent)
        {
            float result = 1 / MathX.Pow((float)num, MathX.Abs(exponent));

            return result;
        }
    }
}
