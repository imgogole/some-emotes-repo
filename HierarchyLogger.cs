using System.Text;
using UnityEngine;

namespace SomeEmotesREPO
{
    public static class HierarchyLogger
    {
        public static void LogFullHierarchy(GameObject go)
        {
            Transform root = go.transform;
            while (root.parent != null)
                root = root.parent;

            var sb = new StringBuilder();
            BuildHierarchyString(root, 0, sb, go.transform);
            SomeEmotesREPO.Logger.LogInfo(sb.ToString());
        }

        private static void BuildHierarchyString(Transform t, int indent, StringBuilder sb, Transform initial)
        {
            sb.Append(' ', indent * 2);
            string is_initial = (t == initial) ? " [INITIAL GAME OBJECT]" : "";
            sb.AppendLine($"- {t.name}" + is_initial);
            foreach (Transform child in t)
                BuildHierarchyString(child, indent + 1, sb, initial);
        }

        public static void LogMeshHierarchy(GameObject go)
        {
            Transform root = go.transform;
            while (root.parent != null)
                root = root.parent;

            var sb = new StringBuilder();
            BuildMeshHierarchyString(root, 0, sb);
            SomeEmotesREPO.Logger.LogInfo(sb.ToString());
        }

        private static void BuildMeshHierarchyString(Transform t, int indent, StringBuilder sb)
        {
            GameObject go = t.gameObject;
            bool isActive = go.activeInHierarchy;
            bool hasMeshRenderer = go.GetComponent<MeshRenderer>() != null;
            bool hasSkinnedMeshRenderer = go.GetComponent<SkinnedMeshRenderer>() != null;
            bool hasRenderer = hasMeshRenderer || hasSkinnedMeshRenderer;

            string meshName = "N/A";

            if (hasMeshRenderer)
            {
                MeshFilter filter = go.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                    meshName = filter.sharedMesh.name;
            }
            else if (hasSkinnedMeshRenderer)
            {
                var smr = go.GetComponent<SkinnedMeshRenderer>();
                if (smr.sharedMesh != null)
                    meshName = smr.sharedMesh.name;
            }

            Vector3 pos = t.localPosition;
            Vector3 euler = t.localEulerAngles;
            Vector3 scale = t.localScale;

            sb.Append(' ', indent * 2);
            sb.AppendLine($"- {t.name} | Active: {isActive} | Renderer: {hasRenderer} | Pos: {pos}, Rot : {euler}, Scale : {scale} | Mesh: {meshName}");

            foreach (Transform child in t)
                BuildMeshHierarchyString(child, indent + 1, sb);
        }
    }
}
