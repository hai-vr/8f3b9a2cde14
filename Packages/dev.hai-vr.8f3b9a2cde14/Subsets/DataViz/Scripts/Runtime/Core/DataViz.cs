using UnityEngine;

namespace Resilience.Visualize
{
    /// A helper class to draw debugging visual.
    /// These debugging visuals are visible in VR, so it doesn't use Unity Gizmos and Handles.
    ///
    /// Calls to this class are relayed to a MonoBehaviour that contains the renderers.
    ///
    /// To facilitate the use of this class anywhere, the caller doesn't need to locate the actual MonoBehaviour in the scene.
    /// This is (sort of) exposed as if this were a singleton, but the singleton aspect is not enforced.
    ///
    /// (Limit the use of singletons in the projects to exceptional cases like this.
    /// "Single instances" classes are normal, but as much as possible these single instances should be injected manually
    /// into their dependents.)
    public class DataViz
    {
        public static DataViz Instance { get; private set; }
        public static DataVizLifecycle Lifecycle { get; private set; }

        public static void DeclareInstance(DataViz viz, DataVizLifecycle lifecycle)
        {
            Instance = viz;
            Lifecycle = lifecycle;
        }

        public void DrawLine(Vector3[] positions, Color start, Color end)
        {
            Lifecycle.Line(positions, start, end, 1f);
        }

        public void DrawLine(Vector3[] positions, Color start, Color end, float relativeWidth)
        {
            Lifecycle.Line(positions, start, end, relativeWidth);
        }

        public void DrawLine(Vector3 startPos, Vector3 endPos, Color start, Color end)
        {
            Lifecycle.Line(new []{startPos, endPos}, start, end, 1f);
        }

        public void DrawLine(Vector3 startPos, Vector3 endPos, Color color)
        {
            Lifecycle.Line(new[] { startPos, endPos }, color, color, 1f);
        }

        public void DrawNormal(Vector3 pos, Vector3 normal, Color color)
        {
            Lifecycle.Line(new[] { pos, pos + normal }, color, color, 1f);
        }

        public void DrawLine(Vector3 startPos, Vector3 endPos, Color color, float relativeWidth)
        {
            Lifecycle.Line(new []{startPos, endPos}, color, color, relativeWidth);
        }

        public void DrawGizmo(Vector3 position, Quaternion rotation, float sizeInMeters)
        {
            DataViz.Instance.DrawLine(position, position + rotation * Vector3.right * sizeInMeters, Color.red);
            DataViz.Instance.DrawLine(position, position + rotation * Vector3.up * sizeInMeters, Color.green);
            DataViz.Instance.DrawLine(position, position + rotation * Vector3.forward * sizeInMeters, Color.blue);
        }

        public void DrawGizmoSpecial(Vector3 position, Quaternion rotation, float sizeInMeters)
        {
            DataViz.Instance.DrawLine(position, position + rotation * Vector3.right * sizeInMeters, Color.cyan);
            DataViz.Instance.DrawLine(position, position + rotation * Vector3.up * sizeInMeters, Color.magenta);
            DataViz.Instance.DrawLine(position, position + rotation * Vector3.forward * sizeInMeters, Color.yellow);
        }

        public void DrawText(Vector3 pos, Color color, string message)
        {
            // FIXME: We just disabled text temporaily
            // Lifecycle.Text(pos, color, message);
        }

        public void DrawQuaternionInWorldSpace(Quaternion quaternion, Vector3 position, float sizeInMeters, Color color)
        {
            var halfSize = sizeInMeters * 0.5f;

            quaternion.ToAngleAxis(out var angle, out var axis);
            // angle = Mathf.DeltaAngle(0, angle);
            DataViz.Instance.DrawLine(position + axis * halfSize, position - axis * halfSize, color, 0.5f);

            // Probably inefficient
            var initCross = Vector3.Cross(axis, Vector3.up + Vector3.right * 0.001f).normalized * halfSize;
            var pointers = new Vector3[10];
            for (var index = 0; index < pointers.Length; index++)
            {
                var amount = Mathf.Lerp(0, angle, index / (pointers.Length - 1f));
                pointers[index] = position + Quaternion.AngleAxis(amount, axis) * initCross;
            }

            DataViz.Instance.DrawLine(pointers, color, Color.white);
        }

        public void DrawCircle(Vector3 position, Vector3 normal, float radius, Color color)
        {
            var pointers = new Vector3[10];
            for (var index = 0; index < pointers.Length; index++)
            {
                var amount = Mathf.Lerp(0, 360, index / (pointers.Length - 1f));
                pointers[index] = position + Quaternion.AngleAxis(amount, normal) * Vector3.forward * radius;
            }

            DataViz.Instance.DrawLine(pointers, color, Color.white);
        }

        public void DrawHighDefCircle(Vector3 position, Vector3 normal, float radius, Color color)
        {
            var pointers = new Vector3[50];
            for (var index = 0; index < pointers.Length; index++)
            {
                var amount = Mathf.Lerp(0, 360, index / (pointers.Length - 1f));
                pointers[index] = position + Quaternion.AngleAxis(amount, normal) * Vector3.forward * radius;
            }

            DataViz.Instance.DrawLine(pointers, color, color, 0.5f);
        }
    }
}
