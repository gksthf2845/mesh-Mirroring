using UnityEngine;
using System.Collections.Generic;

namespace MultiMeshMirror
{
    //���ʽ�����5. ���� ���� ������Ʈ�� �̷��� ó��
    public class TargetObject
    {
        public GameObject originalObject;
        public MeshFilter originalObjectFilter;
        public MeshRenderer originalObjectRenderer;
        public Mesh originalMesh;
        public Vector2[] originalUVs;

        public GameObject mirroredObject;
        public MeshFilter mirroredMeshFilter;
        public MeshRenderer mirroredMeshRenderer;
        public Mesh mirroredMesh;
        public Vector3 mirroredPosition;
    }

    public class MultiMeshMirrorComponent : MonoBehaviour
    {
        // �̷����� �� ����
        public enum MirrorAxis
        {
            X, Y, Z
        }

        [SerializeField] private MirrorAxis mirrorAxis = MirrorAxis.X;        // �̷��� ���� �� 
        [SerializeField] private bool updateMirrorInRealtime = false;         // �ǽð� �̷��� ������Ʈ ����
                                                                              
        // �̷��� �� ���� ������Ʈ ����Ʈ. �ν����Ϳ��� �Ҵ�
        [SerializeField] private List<GameObject> targetObjects = new List<GameObject>();

        //����������Ʈ�� �̷����� ������Ʈ ����Ʈ
        private List<TargetObject> mirroredObjects = new List<TargetObject>();

        private Vector3 mirroredObjectLocalSacle = new Vector3(1, 1, 1); // �ڽ����� ���� ������Ʈ�� ���ý����� �ʱ�ȭ��

        private void Start()
        {
            CreateAllMirroredMeshes();
        }

        private void Update()
        {
            //���ʽ�����2. �ǽð� ������Ʈ �ɼ�
            if (updateMirrorInRealtime && mirroredObjects.Count > 0)
            {
                UpdateAllMirroredMeshes();
            }

        }

        // ��� Ÿ�� ������Ʈ�� ���� �̷��� �޽� ����
        public void CreateAllMirroredMeshes()
        {

            // ��� Ÿ�� ������Ʈ�� ���� �̷��� �޽� ����
            foreach (var targetObj in targetObjects)
            {
                    CreateMirroredMesh(targetObj);
            }
        }

        // ���� ���� ������Ʈ�� ���� �̷��� �޽� ����
        public void CreateMirroredMesh(GameObject targetObj)
        {
            // ���� ������Ʈ���� �ʿ��� ������Ʈ ��������
            MeshFilter originalMeshFilter = targetObj.GetComponent<MeshFilter>();
            MeshRenderer originalMeshRenderer = targetObj.GetComponent<MeshRenderer>();

            // ���� �޽� ��������
            Mesh originalMesh = originalMeshFilter.sharedMesh;

            // ���ο� ������Ʈ ����
            TargetObject targetObjectData = new TargetObject();
            targetObjectData.originalObject = targetObj;
            targetObjectData.originalObjectFilter = originalMeshFilter;
            targetObjectData.originalObjectRenderer = originalMeshRenderer;
            targetObjectData.originalMesh = originalMesh;
            targetObjectData.originalUVs = originalMesh.uv;

            // �̷����� �޽��� ���� �� ������Ʈ ����
            GameObject mirroredObject = new GameObject(targetObj.name + "_Mirrored");
            //���ʽ�����3. ����������Ʈ���� �ڽ����� ����
            mirroredObject.transform.parent = targetObjectData.originalObject.transform; // �θ���

            // �ʿ��� ������Ʈ �߰�
            MeshFilter mirroredMeshFilter = mirroredObject.AddComponent<MeshFilter>();
            MeshRenderer mirroredMeshRenderer = mirroredObject.AddComponent<MeshRenderer>();

            // ���� ������Ʈ�� ��Ƽ������ �̷����� ������Ʈ�� ����
            mirroredMeshRenderer.sharedMaterials = originalMeshRenderer.sharedMaterials;

            // TargetObject ������ �ϼ�
            targetObjectData.mirroredObject = mirroredObject;
            targetObjectData.mirroredMeshFilter = mirroredMeshFilter;
            targetObjectData.mirroredMeshRenderer = mirroredMeshRenderer;
            targetObjectData.mirroredMesh = new Mesh();

            // �̷��� ��ü ����Ʈ�� �߰�
            mirroredObjects.Add(targetObjectData);

           
        }

        // ��� �̷����� �޽� ������Ʈ
        public void UpdateAllMirroredMeshes()
        {
            foreach (var targetObj in mirroredObjects)
            {
                UpdateMirroredMesh(targetObj);
            }
        }

        // foreach ���� ���� �̷��� �޽� ������Ʈ
        public void UpdateMirroredMesh(TargetObject targetObj)
        {
            if (targetObj == null || targetObj.mirroredObject == null)
            {
                return;
            }

            // ���� �޽� ������ ��������
            Vector3[] originalVertices = targetObj.originalMesh.vertices;
            int[] originalTriangles = targetObj.originalMesh.triangles;
            Vector3[] originalNormals = targetObj.originalMesh.normals;

            // �̷����� �޽� �����͸� ���� �迭 ����
            Vector3[] mirroredVertices = new Vector3[originalVertices.Length];
            int[] mirroredTriangles = new int[originalTriangles.Length];
            Vector3[] mirroredNormals = new Vector3[originalNormals.Length];

            // ���õ� �࿡ ���� ������ ���� ���� �̷���
            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 vertex = originalVertices[i];
                Vector3 normal = originalNormals[i];

                // �� �ึ�� ���ؽ�, �������� ����
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
            targetObj.mirroredMesh.vertices = mirroredVertices;
            targetObj.mirroredMesh.triangles = mirroredTriangles;
            targetObj.mirroredMesh.normals = mirroredNormals;
            targetObj.mirroredMesh.uv = targetObj.originalUVs;

            // ������ �޽��� �̷����� ������Ʈ�� �Ҵ�
            targetObj.mirroredMeshFilter.sharedMesh = targetObj.mirroredMesh;

            //���ʽ�����1 �ǽð� ������Ʈ �ɼ� �̷����� ������Ʈ ��ġ,ȸ��,ũ�� ������Ʈ
            UpdateMirroredPosition(targetObj);
            UpdateMirroredRotation(targetObj);
            targetObj.mirroredObject.transform.localScale = mirroredObjectLocalSacle;


        }


        private void UpdateMirroredPosition(TargetObject targetObj)
        {

            // ���õ� �࿡ ���� �̷����� ��ġ ���
            targetObj.mirroredPosition = targetObj.originalObject.transform.position;


            switch (mirrorAxis)
            {
                case MirrorAxis.X:
                    targetObj.mirroredPosition.x = -targetObj.mirroredPosition.x;
                    break;
                case MirrorAxis.Y:
                    targetObj.mirroredPosition.y = -targetObj.mirroredPosition.y;
                    break;
                case MirrorAxis.Z:
                    targetObj.mirroredPosition.z = -targetObj.mirroredPosition.z;
                    break;
            }


            targetObj.mirroredObject.transform.position = targetObj.mirroredPosition;


        }
        private void UpdateMirroredRotation(TargetObject targetObj)
        {
            // ȸ������ �� �࿡���� xyzw ���������� �ʿ��ϱ⶧���� �������� �Ҵ�.
            Quaternion originalRotation = targetObj.originalObject.transform.rotation;
            Quaternion mirroredRotation;

            // �࿡ ���� ȸ�� �̷���
            switch (mirrorAxis)
            {
                case MirrorAxis.X:

                    mirroredRotation = new Quaternion(-originalRotation.x, originalRotation.y, originalRotation.z, -originalRotation.w);
                    break;
                case MirrorAxis.Y:

                    mirroredRotation = new Quaternion(originalRotation.x, -originalRotation.y, originalRotation.z, -originalRotation.w);
                    break;
                case MirrorAxis.Z:

                    mirroredRotation = new Quaternion(originalRotation.x, originalRotation.y, -originalRotation.z, -originalRotation.w);
                    break;
                default:
                    mirroredRotation = originalRotation;
                    break;
            }

            targetObj.mirroredObject.transform.rotation = mirroredRotation;
        }
    }


}