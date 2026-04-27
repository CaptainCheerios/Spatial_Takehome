using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrabbableObject))]
public class GrabbableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        GrabbableObject grabbable = (GrabbableObject)target;

        if (GUILayout.Button("Fit Box Collider To Mesh Renderers"))
        {
            FitBoxColliderToMeshRenderers(grabbable);
        }
    }

    private void FitBoxColliderToMeshRenderers(GrabbableObject grabbable)
    {
        // 1. Find the first collider on the GameObject.
        Collider collider = grabbable.GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogWarning(
                $"[GrabbableObjectEditor] No Collider found on '{grabbable.name}'.",
                grabbable);
            return;
        }

        BoxCollider box = collider as BoxCollider;
        if (box == null)
        {
            Debug.LogWarning(
                $"[GrabbableObjectEditor] First collider on '{grabbable.name}' is a " +
                $"{collider.GetType().Name}, not a BoxCollider. Only BoxCollider is supported.",
                grabbable);
            return;
        }

        // 2. Read the serialized 'meshRenderers' field.
        SerializedProperty mrProp = serializedObject.FindProperty("meshRenderers");
        if (mrProp == null || !mrProp.isArray)
        {
            Debug.LogWarning(
                "[GrabbableObjectEditor] Could not find a serialized 'meshRenderers' array " +
                "on GrabbableObject. Make sure the field exists and is [SerializeField].",
                grabbable);
            return;
        }

        if (mrProp.arraySize == 0)
        {
            Debug.LogWarning(
                $"[GrabbableObjectEditor] 'meshRenderers' array is empty on '{grabbable.name}'.",
                grabbable);
            return;
        }

        // 3. Compute the AABB of all meshes in the grabbable's local space.
        Transform root = grabbable.transform;
        bool hasBounds = false;
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.zero);

        for (int i = 0; i < mrProp.arraySize; i++)
        {
            MeshRenderer mr = mrProp.GetArrayElementAtIndex(i).objectReferenceValue as MeshRenderer;
            if (mr == null) continue;

            MeshFilter mf = mr.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            Bounds meshLocal = mf.sharedMesh.bounds;
            Vector3 c = meshLocal.center;
            Vector3 e = meshLocal.extents;

            // 8 corners of this mesh's local-space AABB.
            for (int xs = -1; xs <= 1; xs += 2)
            for (int ys = -1; ys <= 1; ys += 2)
            for (int zs = -1; zs <= 1; zs += 2)
            {
                Vector3 corner = c + new Vector3(e.x * xs, e.y * ys, e.z * zs);
                Vector3 world = mf.transform.TransformPoint(corner);
                Vector3 local = root.InverseTransformPoint(world);

                if (!hasBounds)
                {
                    localBounds = new Bounds(local, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    localBounds.Encapsulate(local);
                }
            }
        }

        if (!hasBounds)
        {
            Debug.LogWarning(
                $"[GrabbableObjectEditor] No valid meshes found in 'meshRenderers' on " +
                $"'{grabbable.name}'.",
                grabbable);
            return;
        }

        // 4. Apply, with undo support.
        Undo.RecordObject(box, "Fit Box Collider To Mesh Renderers");
        box.center = localBounds.center;
        box.size = localBounds.size;
        EditorUtility.SetDirty(box);
    }
}