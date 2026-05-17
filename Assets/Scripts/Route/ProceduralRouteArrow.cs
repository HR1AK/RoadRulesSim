using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ProceduralRouteArrow : MonoBehaviour
{
    [Header("Arrow shape")]
    [SerializeField] private float length = 2.0f;
    [SerializeField] private float bodyLength = 1.2f;
    [SerializeField] private float bodyWidth = 0.35f;
    [SerializeField] private float headWidth = 0.9f;

    [Header("Visual")]
    [SerializeField] private Material material;
    [SerializeField] private Color color = new Color(0.1f, 0.8f, 1f, 0.85f);

    private void Awake()
    {
        GenerateArrow();
    }

    private void OnValidate()
    {
        length = Mathf.Max(0.1f, length);
        bodyLength = Mathf.Clamp(bodyLength, 0.05f, length);
        bodyWidth = Mathf.Max(0.05f, bodyWidth);
        headWidth = Mathf.Max(bodyWidth, headWidth);

        GenerateArrow();
    }

    private void GenerateArrow()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.name = "Procedural Route Arrow";

        float halfBody = bodyWidth * 0.5f;
        float halfHead = headWidth * 0.5f;

        float backZ = -length * 0.5f;
        float bodyEndZ = backZ + bodyLength;
        float frontZ = length * 0.5f;

        Vector3[] vertices =
        {
            new Vector3(-halfBody, 0f, backZ),
            new Vector3( halfBody, 0f, backZ),
            new Vector3( halfBody, 0f, bodyEndZ),
            new Vector3( halfHead, 0f, bodyEndZ),
            new Vector3( 0f,      0f, frontZ),
            new Vector3(-halfHead, 0f, bodyEndZ),
            new Vector3(-halfBody, 0f, bodyEndZ)
        };

        int[] triangles =
        {
            0, 1, 2,
            0, 2, 6,

            6, 2, 3,
            6, 3, 5,

            5, 3, 4
        };

        Vector2[] uv =
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0.6f),
            new Vector2(1f, 0.7f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, 0.7f),
            new Vector2(0f, 0.6f)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;

        if (material != null)
        {
            meshRenderer.sharedMaterial = material;
        }
        else
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

            if (shader == null)
                shader = Shader.Find("Unlit/Color");

            if (shader == null)
                shader = Shader.Find("Standard");

            Material generatedMaterial = new Material(shader);
            generatedMaterial.color = color;

            meshRenderer.sharedMaterial = generatedMaterial;
        }
    }
}