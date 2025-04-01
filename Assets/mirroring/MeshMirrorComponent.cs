using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;


public class MeshMirrorComponent : MonoBehaviour
{
    // �̷����� �� ����
    public enum MirrorAxis
    {
        X, Y, Z
    }

    [SerializeField] private MirrorAxis mirrorAxis = MirrorAxis.X;        // �̷��� ���� �� 
    [SerializeField] private bool updateMirrorInRealtime = false;         // �ǽð� �̷��� ������Ʈ ����

    private GameObject mirroredObject;         // �̷����� ������Ʈ�� �����ϴ� ����
    private MeshFilter originalMeshFilter;     // ���� ������Ʈ�� MeshFilter
    private MeshRenderer originalMeshRenderer; // ���� ������Ʈ�� MeshRenderer
    private MeshFilter mirroredMeshFilter;     // �̷����� ������Ʈ�� MeshFilter
    private MeshRenderer mirroredMeshRenderer; // �̷����� ������Ʈ�� MeshRenderer

    //������Ʈ���� �������� �޸� ���Ҵ��� �������� ĳ�� 
    private Mesh originalMesh; // ���� ������Ʈ�� �޽� ĳ��
    private Mesh mirroredMesh; // ������ �̷����� ������Ʈ �޽� ĳ��
    Vector3 mirroredPosition; // �̷��� ������Ʈ ��ġ


    //�ѹ��� ���� ����.
    private Vector2[] originalUVs;// uv�� ���� ĳ��
    private Vector3 mirroredObjectLocalSacle = new Vector3(1, 1, 1); // �ڽ����� ���� ������Ʈ�� ���ý����� �ʱ�ȭ��

    private void Start()
    {
        // ���� ������Ʈ ���� ��������
        originalMeshFilter = GetComponent<MeshFilter>();
        originalMeshRenderer = GetComponent<MeshRenderer>();

        CreateMirroredMesh();

    }

    private void Update()
    {
        //���ʽ�����2. �ǽð� ������Ʈ �ɼ�
        if (updateMirrorInRealtime && mirroredObject != null)
        {
            UpdateMirroredMesh();
        }
    }

    //ó�� �� �� �޽� ����
    public void CreateMirroredMesh()
    {
        // ���� �޽� ��������
        originalMesh = originalMeshFilter.sharedMesh;
        originalUVs = originalMesh.uv;               



        // �̷����� �޽��� ���� �� ������Ʈ ����
        mirroredObject = new GameObject(gameObject.name + "_Mirrored");
        //���ʽ�����3. ����������Ʈ���� �ڽ����� ����
        mirroredObject.transform.parent = transform;

        // �ʿ��� ������Ʈ �߰�
        mirroredMeshFilter = mirroredObject.AddComponent<MeshFilter>();
        mirroredMeshRenderer = mirroredObject.AddComponent<MeshRenderer>();

        // ���� ������Ʈ�� ��Ƽ������ �̷����� ������Ʈ�� ����
        mirroredMeshRenderer.sharedMaterials = originalMeshRenderer.sharedMaterials;

       
    }

    public void UpdateMirroredMesh()
    {
        if (mirroredObject == null)
        {
            // Debug.Log("�̷��޽� �������� ����");
            return;
        }

        // ���� �̷� �޽� ���� �ϱ� �� ��üũ
        if (mirroredMeshFilter.sharedMesh != null)
        {
            // ���� �޽� ����
            mirroredMesh = mirroredMeshFilter.sharedMesh;
            // Debug.Log("����");
        }
        else
        {
            // ó�� �� ���� �� �޽� ����
            mirroredMesh = new Mesh();
        }


        // ���� �޽��� ������ ��������
        Vector3[] originalVertices = originalMesh.vertices;    // ���� �迭
        int[] originalTriangles = originalMesh.triangles;      // �ﰢ�� �ε��� �迭
        Vector3[] originalNormals = originalMesh.normals;      // ���� ���� �迭


        // �̷����� �޽� �����͸� ���� �迭 ����
        Vector3[] mirroredVertices = new Vector3[originalVertices.Length];
        int[] mirroredTriangles = new int[originalTriangles.Length];
        Vector3[] mirroredNormals = new Vector3[originalNormals.Length];

        // ���õ� �࿡ ���� ������ ���� ���� �̷���
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];
            Vector3 normal = originalNormals[i];

            //�� �ึ�� ���ؽ�,�������� ����
            switch (mirrorAxis)
            {
                case MirrorAxis.X:
                    vertex.x = -vertex.x;
                    normal.x = -normal.x;
                    break;
                case MirrorAxis.Y:
                    vertex.y = -vertex.y;
                    normal.y = -normal.y;
                    break;
                case MirrorAxis.Z:
                    vertex.z = -vertex.z;
                    normal.z = -normal.z;
                    break;

            }

            mirroredVertices[i] = vertex;
            mirroredNormals[i] = normal;
        }

        // �ﰢ�� �ε��� ���� ������ 
        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            mirroredTriangles[i] = originalTriangles[i];          // ù ��° ���� ����
            mirroredTriangles[i + 1] = originalTriangles[i + 2];  // �� ��°�� �� ��° ���� ���� �ٲ�
            mirroredTriangles[i + 2] = originalTriangles[i + 1];  // �� ������ ������
        }

        // �̷����� �޽��� ������ �Է�
        mirroredMesh.vertices = mirroredVertices;
        mirroredMesh.triangles = mirroredTriangles;
        mirroredMesh.normals = mirroredNormals;
        mirroredMesh.uv = originalUVs;
        // ������ �޽��� �̷����� ������Ʈ�� �Ҵ�
        mirroredMeshFilter.sharedMesh = mirroredMesh;

        //���ʽ�����1 �ǽð� ������Ʈ �ɼ� �̷����� ������Ʈ ��ġ,ȸ��,ũ�� ������Ʈ
        UpdateMirroredPosition();
        UpdateMirroredRotation();
        mirroredObject.transform.localScale = mirroredObjectLocalSacle;
    }


    private void UpdateMirroredPosition()
    {

        // ���õ� �࿡ ���� �̷����� ��ġ ���
        mirroredPosition = transform.position;

        switch (mirrorAxis)
        {
            case MirrorAxis.X:
                mirroredPosition.x = -mirroredPosition.x;
                break;
            case MirrorAxis.Y:
                mirroredPosition.y = -mirroredPosition.y;
                break;
            case MirrorAxis.Z:
                mirroredPosition.z = -mirroredPosition.z;
                break;
        }


        mirroredObject.transform.position = mirroredPosition;


    }
    private void UpdateMirroredRotation()
    {
        // ȸ������ �� �࿡���� xyzw ���������� �ʿ��ϱ⶧���� �������� �Ҵ�.
        Quaternion originRotation = transform.rotation;
        Quaternion mirroredRotation;

        // �࿡ ���� ȸ�� �̷���
        switch (mirrorAxis)
        {
            case MirrorAxis.X:

                mirroredRotation = new Quaternion(-originRotation.x, originRotation.y, originRotation.z, -originRotation.w);
                break;
            case MirrorAxis.Y:

                mirroredRotation = new Quaternion(originRotation.x, -originRotation.y, originRotation.z, -originRotation.w);
                break;
            case MirrorAxis.Z:

                mirroredRotation = new Quaternion(originRotation.x, originRotation.y, -originRotation.z, -originRotation.w);
                break;
            default:
                mirroredRotation = originRotation;
                break;
        }

        mirroredObject.transform.rotation = mirroredRotation;
    }
}