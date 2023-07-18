using BaseX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrooxEngine
{
    [@Category(new string[] { "Assets/Procedural Meshes" })]
    public class RoundBevelPanelMesh : ProceduralMesh
    {
        [Range(1, 64)] public readonly Sync<int> Sides;
        public readonly Sync<float2> Size;
        [Range(0.01f, 0.1f)]public readonly Sync<float> Thickness;

        private RoundBevelPanel panel;
        private int _sides;
        private float2 _size;
        private float _thickness;

        protected override void OnAwake()
        {
            base.OnAwake();
            Sides.Value = 16;
            Size.Value = new float2(0.5f, 0.5f);
            Thickness.Value = 0.025f;
        }

        protected override void PrepareAssetUpdateData()
        {
            _sides = Sides.Value;
            _size = Size.Value;
            _thickness = Thickness.Value;
        }

        protected override void ClearMeshData()
        {
            panel = null;
        }

        protected override void UpdateMeshData(MeshX meshx)
        {
            bool check = false;
            if (panel == null || panel.Size != _size || panel.Thickness != _thickness)
            {
                panel?.Remove();
                panel = new RoundBevelPanel(meshx, _size, _thickness, 0.1f, 0.01f, _sides);
                check = true;
            }

            panel.Size = _size;
            panel.Thickness = _thickness;
            panel.Iterations = _sides;
            panel.PrimaryBevel = 0.1f;
            panel.SecondaryBevel = 0.01f;
            panel.Update();
            uploadHint[MeshUploadHint.Flag.Geometry] = check;
        }
    }
}
