using UnityEngine;
using System.Collections.Generic;

// MeshFilter�� MeshRenderer ������Ʈ�� �ʼ������� �ʿ���
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshMirrorCustom : MonoBehaviour
{
    // �̷����� ���� �����ϴ� ������
    public enum MirrorAxis
    {
        X,
        Y,
        Z,
        Custom  // Ŀ���� �� �߰�
    }

    [SerializeField] private MirrorAxis mirrorAxis = MirrorAxis.X;        // �̷��� ���� �� (�⺻��: X��)
    [SerializeField] private Vector3 customMirrorAxis = Vector3.right;    // Ŀ���� �̷� �� ���� (�⺻��: ������ ����)
    [SerializeField] private bool createMirrorOnStart = true;             // ���� �� �ڵ� �̷��� ���� ����
    [SerializeField] private bool updateMirrorInRealtime = false;         // �ǽð� �̷��� ������Ʈ ����
    [SerializeField] private bool normalizeCustomAxis = true;             // Ŀ���� �� ����ȭ ����

    private GameObject mirroredObject;         // �̷����� ������Ʈ�� �����ϴ� ����
    private MeshFilter originalMeshFilter;     // ���� ������Ʈ�� MeshFilter
    private MeshRenderer originalMeshRenderer; // ���� ������Ʈ�� MeshRenderer
    private MeshFilter mirroredMeshFilter;     // �̷����� ������Ʈ�� MeshFilter
    private MeshRenderer mirroredMeshRenderer; // �̷����� ������Ʈ�� MeshRenderer

    private void Start()
    {
        // ���� ������Ʈ ���� ��������
        originalMeshFilter = GetComponent<MeshFilter>();
        originalMeshRenderer = GetComponent<MeshRenderer>();

        // ������ ���� ���� �� �̷����� �޽� ����
        if (createMirrorOnStart)
        {
            CreateMirroredMesh();
        }
    }

    private void Update()
    {
        // �ǽð� ������Ʈ �ɼ��� ���� �ְ� �̷� ������Ʈ�� �����ϸ� �� ������ ������Ʈ
        if (updateMirrorInRealtime && mirroredObject != null)
        {
            UpdateMirroredMesh();
        }
    }

    // ���ؽ�Ʈ �޴��� ��� �߰� (Inspector���� ������Ʈ ��Ŭ�� �� �޴��� ǥ�õ�)
    [ContextMenu("Create Mirrored Mesh")]
    public void CreateMirroredMesh()
    {
        // �̹� �̷����� ������Ʈ�� �ִٸ� ����
        if (mirroredObject != null)
        {
            DestroyImmediate(mirroredObject);
        }

        // �̷����� �޽��� ���� �� ���� ������Ʈ ����
        mirroredObject = new GameObject($"{gameObject.name}_Mirrored");
        mirroredObject.transform.parent = transform.parent;

        // �ʿ��� ������Ʈ �߰�
        mirroredMeshFilter = mirroredObject.AddComponent<MeshFilter>();
        mirroredMeshRenderer = mirroredObject.AddComponent<MeshRenderer>();

        // ���� ������Ʈ�� ��Ƽ������ �̷����� ������Ʈ�� ����
        mirroredMeshRenderer.sharedMaterials = originalMeshRenderer.sharedMaterials;

        // �̷����� �޽� ���� �� ������Ʈ
        UpdateMirroredMesh();
    }

    // �̷����� �޽� ������Ʈ �޼ҵ�
    [ContextMenu("Update Mirrored Mesh")]
    public void UpdateMirroredMesh()
    {
        // �̷� ������Ʈ�� ������ ���� ����
        if (mirroredObject == null)
        {
            CreateMirroredMesh();
            return;
        }

        // ���� �޽� ��������
        Mesh originalMesh = originalMeshFilter.sharedMesh;

        // �̷����� �޽��� ���� �� �޽� ��ü ����
        Mesh mirroredMesh = new Mesh();
        mirroredMesh.name = $"{originalMesh.name}_Mirrored";

        // ���� �޽��� ������ ��������
        Vector3[] originalVertices = originalMesh.vertices;    // ���� �迭
        int[] originalTriangles = originalMesh.triangles;      // �ﰢ�� �ε��� �迭
        Vector3[] originalNormals = originalMesh.normals;      // ���� ���� �迭
        Vector2[] originalUVs = originalMesh.uv;               // UV ��ǥ �迭

        // �̷����� �޽� �����͸� ���� �迭 ����
        Vector3[] mirroredVertices = new Vector3[originalVertices.Length];
        int[] mirroredTriangles = new int[originalTriangles.Length];
        Vector3[] mirroredNormals = new Vector3[originalNormals.Length];

        // Ŀ���� �� ����ȭ (���̰� 1�� �ǵ���)
        Vector3 axisNormalized = customMirrorAxis;
        if (mirrorAxis == MirrorAxis.Custom && normalizeCustomAxis)
        {
            axisNormalized = customMirrorAxis.normalized;
        }

        // ���õ� �࿡ ���� ������ ���� ���� �̷���
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];
            Vector3 normal = originalNormals[i];

            switch (mirrorAxis)
            {
                case MirrorAxis.X:
                    vertex.x = -vertex.x;  // X�� ���� ���� ����
                    normal.x = -normal.x;  // X�� ���� ���� ���� ����
                    break;
                case MirrorAxis.Y:
                    vertex.y = -vertex.y;  // Y�� ���� ���� ����
                    normal.y = -normal.y;  // Y�� ���� ���� ���� ����
                    break;
                case MirrorAxis.Z:
                    vertex.z = -vertex.z;  // Z�� ���� ���� ����
                    normal.z = -normal.z;  // Z�� ���� ���� ���� ����
                    break;
                case MirrorAxis.Custom:
                    // Ŀ���� �� ���� �̷��� (�ݻ� ��ȯ)
                    // ����: v' = v - 2 * (v��n) * n, ���⼭ n�� ����ȭ�� �� ����
                    float dot = Vector3.Dot(vertex, axisNormalized);
                    vertex = vertex - 2 * dot * axisNormalized;

                    // ���� ���͵� ������ ������� �ݻ�
                    dot = Vector3.Dot(normal, axisNormalized);
                    normal = normal - 2 * dot * axisNormalized;
                    break;
            }

            mirroredVertices[i] = vertex;
            mirroredNormals[i] = normal;
        }

        // �ﰢ�� �ε��� ���� ������ (�� ���� ������ ���� �ʼ�)
        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            mirroredTriangles[i] = originalTriangles[i];          // ù ��° ���� ����
            mirroredTriangles[i + 1] = originalTriangles[i + 2];  // �� ��°�� �� ��° ���� ���� �ٲ�
            mirroredTriangles[i + 2] = originalTriangles[i + 1];  // �̷��� �ϸ� �� ������ �ùٸ��� ������
        }

        // �̷����� �޽��� ������ ����
        mirroredMesh.vertices = mirroredVertices;
        mirroredMesh.triangles = mirroredTriangles;
        mirroredMesh.normals = mirroredNormals;
        mirroredMesh.uv = originalUVs;  // UV�� �̷����� �ʿ� ����
        mirroredMesh.RecalculateBounds();  // �޽� ��� ����

        // ������ �޽��� �̷����� ������Ʈ�� �Ҵ�
        mirroredMeshFilter.sharedMesh = mirroredMesh;

        // �̷����� ������Ʈ ��ġ ������Ʈ
        UpdateMirroredPosition();
    }

    // �̷����� ������Ʈ�� ��ġ ������Ʈ
    private void UpdateMirroredPosition()
    {
        // ȸ���� ũ��� ������ �����ϰ� ����
        mirroredObject.transform.rotation = transform.rotation;
        mirroredObject.transform.localScale = transform.localScale;

        // ���õ� �࿡ ���� �̷����� ��ġ ���
        Vector3 mirroredPosition = transform.position;
        Vector3 axisNormalized = customMirrorAxis;

        if (mirrorAxis == MirrorAxis.Custom && normalizeCustomAxis)
        {
            axisNormalized = customMirrorAxis.normalized;
        }

        switch (mirrorAxis)
        {
            case MirrorAxis.X:
                mirroredPosition.x = -mirroredPosition.x;  // X ��ġ ����
                break;
            case MirrorAxis.Y:
                mirroredPosition.y = -mirroredPosition.y;  // Y ��ġ ����
                break;
            case MirrorAxis.Z:
                mirroredPosition.z = -mirroredPosition.z;  // Z ��ġ ����
                break;
            case MirrorAxis.Custom:
                // Ŀ���� �� ���� ��ġ �̷���
                float dot = Vector3.Dot(mirroredPosition, axisNormalized);
                mirroredPosition = mirroredPosition - 2 * dot * axisNormalized;
                break;
        }

        // ���� ��ġ ����
        mirroredObject.transform.position = mirroredPosition;
    }

    // ��Ÿ�ӿ� �̷��� �� ���� �޼ҵ�
    public void SetMirrorAxis(MirrorAxis newAxis)
    {
        // ���� ����� ��쿡�� ������Ʈ
        if (mirrorAxis != newAxis)
        {
            mirrorAxis = newAxis;
            if (mirroredObject != null)
            {
                UpdateMirroredMesh();
            }
        }
    }

    // ���� �ε����� �̷��� �� ���� (UI �����)
    public void SetMirrorAxis(int axisIndex)
    {
        if (axisIndex >= 0 && axisIndex <= (int)MirrorAxis.Custom)
        {
            SetMirrorAxis((MirrorAxis)axisIndex);
        }
    }

    // Ŀ���� �̷��� �� ���� �޼ҵ�
    public void SetCustomMirrorAxis(Vector3 newAxis)
    {
        if (customMirrorAxis != newAxis)
        {
            customMirrorAxis = newAxis;
            if (mirrorAxis == MirrorAxis.Custom && mirroredObject != null)
            {
                UpdateMirroredMesh();
            }
        }
    }

    // ���� �޼ҵ�: �̷��� ��� ���� (���� ���ͷ� ����)
    public void SetMirrorPlane(Vector3 planeNormal)
    {
        SetMirrorAxis(MirrorAxis.Custom);
        SetCustomMirrorAxis(planeNormal);
    }

    // ���� �޼ҵ�: �� �� ������ �߰� ������� �̷���
    public void SetMirrorPlaneBetweenPoints(Vector3 pointA, Vector3 pointB)
    {
        Vector3 midpoint = (pointA + pointB) * 0.5f;
        Vector3 direction = (pointB - pointA).normalized;

        // �� ������Ʈ�� ��ġ�� ����� ����� �̷� ��� ����
        transform.position = midpoint;
        SetMirrorPlane(direction);
    }

    // ���� ������Ʈ�� ������ �� �̷� ������Ʈ�� �Բ� ����
    private void OnDestroy()
    {
        if (mirroredObject != null)
        {
            DestroyImmediate(mirroredObject);
        }
    }

#if UNITY_EDITOR
    // �����Ϳ��� �ð������� �̷� �� ǥ�� (Scene �信���� ����)
    private void OnDrawGizmosSelected()
    {
        // �̷� �� �ð�ȭ
        Gizmos.color = Color.cyan;

        // ���õ� �࿡ ���� �ٸ� ������� �ð�ȭ
        switch (mirrorAxis)
        {
            case MirrorAxis.X:
                Gizmos.DrawLine(transform.position - Vector3.right * 2, transform.position + Vector3.right * 2);
                Gizmos.color = new Color(0, 1, 1, 0.2f);
                Gizmos.DrawCube(transform.position, new Vector3(0.05f, 4, 4));
                break;
            case MirrorAxis.Y:
                Gizmos.DrawLine(transform.position - Vector3.up * 2, transform.position + Vector3.up * 2);
                Gizmos.color = new Color(0, 1, 1, 0.2f);
                Gizmos.DrawCube(transform.position, new Vector3(4, 0.05f, 4));
                break;
            case MirrorAxis.Z:
                Gizmos.DrawLine(transform.position - Vector3.forward * 2, transform.position + Vector3.forward * 2);
                Gizmos.color = new Color(0, 1, 1, 0.2f);
                Gizmos.DrawCube(transform.position, new Vector3(4, 4, 0.05f));
                break;
            case MirrorAxis.Custom:
                Vector3 axisDir = normalizeCustomAxis ? customMirrorAxis.normalized : customMirrorAxis;
                Gizmos.DrawLine(transform.position - axisDir * 2, transform.position + axisDir * 2);

                // �̷� ��� �ð�ȭ
                Gizmos.color = new Color(0, 1, 1, 0.2f);

                // ����� �����ϴ� �� ���� ���� ���� ã��
                Vector3 perpendicular1 = Vector3.Cross(axisDir, Vector3.up);
                if (perpendicular1.magnitude < 0.001f)
                {
                    perpendicular1 = Vector3.Cross(axisDir, Vector3.right);
                }
                perpendicular1 = perpendicular1.normalized;
                Vector3 perpendicular2 = Vector3.Cross(axisDir, perpendicular1).normalized;

                // ��� �׸���
                Vector3 size1 = perpendicular1 * 2;
                Vector3 size2 = perpendicular2 * 2;

                Gizmos.DrawLine(transform.position - size1 - size2, transform.position + size1 - size2);
                Gizmos.DrawLine(transform.position + size1 - size2, transform.position + size1 + size2);
                Gizmos.DrawLine(transform.position + size1 + size2, transform.position - size1 + size2);
                Gizmos.DrawLine(transform.position - size1 + size2, transform.position - size1 - size2);
                break;
        }
    }
#endif
}