using UnityEngine;
using System.Collections.Generic;

namespace MultiMeshMirror
{
    //보너스과제5. 여러 원본 오브젝트의 미러링 처리
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
        // 미러링할 축 선택
        public enum MirrorAxis
        {
            X, Y, Z
        }

        [SerializeField] private MirrorAxis mirrorAxis = MirrorAxis.X;        // 미러링 기준 축 
        [SerializeField] private bool updateMirrorInRealtime = false;         // 실시간 미러링 업데이트 여부
                                                                              
        // 미러링 할 원본 오브젝트 리스트. 인스펙터에서 할당
        [SerializeField] private List<GameObject> targetObjects = new List<GameObject>();

        //원본오브젝트를 미러링할 오브젝트 리스트
        private List<TargetObject> mirroredObjects = new List<TargetObject>();

        private Vector3 mirroredObjectLocalSacle = new Vector3(1, 1, 1); // 자식으로 생긴 오브젝트의 로컬스케일 초기화값

        private void Start()
        {
            CreateAllMirroredMeshes();
        }

        private void Update()
        {
            //보너스과제2. 실시간 업데이트 옵션
            if (updateMirrorInRealtime && mirroredObjects.Count > 0)
            {
                UpdateAllMirroredMeshes();
            }

        }

        // 모든 타겟 오브젝트에 대해 미러링 메쉬 생성
        public void CreateAllMirroredMeshes()
        {

            // 모든 타겟 오브젝트에 대해 미러링 메쉬 생성
            foreach (var targetObj in targetObjects)
            {
                    CreateMirroredMesh(targetObj);
            }
        }

        // 개별 원본 오브젝트에 대한 미러링 메쉬 생성
        public void CreateMirroredMesh(GameObject targetObj)
        {
            // 원본 오브젝트에서 필요한 컴포넌트 가져오기
            MeshFilter originalMeshFilter = targetObj.GetComponent<MeshFilter>();
            MeshRenderer originalMeshRenderer = targetObj.GetComponent<MeshRenderer>();

            // 원본 메쉬 가져오기
            Mesh originalMesh = originalMeshFilter.sharedMesh;

            // 새로운 오브젝트 생성
            TargetObject targetObjectData = new TargetObject();
            targetObjectData.originalObject = targetObj;
            targetObjectData.originalObjectFilter = originalMeshFilter;
            targetObjectData.originalObjectRenderer = originalMeshRenderer;
            targetObjectData.originalMesh = originalMesh;
            targetObjectData.originalUVs = originalMesh.uv;

            // 미러링된 메쉬를 담을 새 오브젝트 생성
            GameObject mirroredObject = new GameObject(targetObj.name + "_Mirrored");
            //보너스과제3. 원본오브젝트에서 자식으로 생성
            mirroredObject.transform.parent = targetObjectData.originalObject.transform; // 부모설정

            // 필요한 컴포넌트 추가
            MeshFilter mirroredMeshFilter = mirroredObject.AddComponent<MeshFilter>();
            MeshRenderer mirroredMeshRenderer = mirroredObject.AddComponent<MeshRenderer>();

            // 원본 오브젝트의 머티리얼을 미러링된 오브젝트에 복사
            mirroredMeshRenderer.sharedMaterials = originalMeshRenderer.sharedMaterials;

            // TargetObject 데이터 완성
            targetObjectData.mirroredObject = mirroredObject;
            targetObjectData.mirroredMeshFilter = mirroredMeshFilter;
            targetObjectData.mirroredMeshRenderer = mirroredMeshRenderer;
            targetObjectData.mirroredMesh = new Mesh();

            // 미러링 객체 리스트에 추가
            mirroredObjects.Add(targetObjectData);

           
        }

        // 모든 미러링된 메쉬 업데이트
        public void UpdateAllMirroredMeshes()
        {
            foreach (var targetObj in mirroredObjects)
            {
                UpdateMirroredMesh(targetObj);
            }
        }

        // foreach 돌릴 개별 미러링 메쉬 업데이트
        public void UpdateMirroredMesh(TargetObject targetObj)
        {
            if (targetObj == null || targetObj.mirroredObject == null)
            {
                return;
            }

            // 원본 메쉬 데이터 가져오기
            Vector3[] originalVertices = targetObj.originalMesh.vertices;
            int[] originalTriangles = targetObj.originalMesh.triangles;
            Vector3[] originalNormals = targetObj.originalMesh.normals;

            // 미러링된 메쉬 데이터를 위한 배열 생성
            Vector3[] mirroredVertices = new Vector3[originalVertices.Length];
            int[] mirroredTriangles = new int[originalTriangles.Length];
            Vector3[] mirroredNormals = new Vector3[originalNormals.Length];

            // 선택된 축에 따라 정점과 법선 벡터 미러링
            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 vertex = originalVertices[i];
                Vector3 normal = originalNormals[i];

                // 각 축마다 버텍스, 법선벡터 반전
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

            // 삼각형 인덱스 순서 뒤집기 
            for (int i = 0; i < originalTriangles.Length; i += 3)
            {
                mirroredTriangles[i] = originalTriangles[i];          // 첫 번째 정점 유지
                mirroredTriangles[i + 1] = originalTriangles[i + 2];  // 두 번째와 세 번째 정점 순서 바꿈
                mirroredTriangles[i + 2] = originalTriangles[i + 1];  // 면 방향이 뒤집힘
            }

            // 미러링된 메쉬에 데이터 입력
            targetObj.mirroredMesh.vertices = mirroredVertices;
            targetObj.mirroredMesh.triangles = mirroredTriangles;
            targetObj.mirroredMesh.normals = mirroredNormals;
            targetObj.mirroredMesh.uv = targetObj.originalUVs;

            // 생성된 메쉬를 미러링된 오브젝트에 할당
            targetObj.mirroredMeshFilter.sharedMesh = targetObj.mirroredMesh;

            //보너스과제1 실시간 업데이트 옵션 미러링된 오브젝트 위치,회전,크기 업데이트
            UpdateMirroredPosition(targetObj);
            UpdateMirroredRotation(targetObj);
            targetObj.mirroredObject.transform.localScale = mirroredObjectLocalSacle;


        }


        private void UpdateMirroredPosition(TargetObject targetObj)
        {

            // 선택된 축에 따라 미러링된 위치 계산
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
            // 회전값은 각 축에대한 xyzw 복합적으로 필요하기때문에 지역변수 할당.
            Quaternion originalRotation = targetObj.originalObject.transform.rotation;
            Quaternion mirroredRotation;

            // 축에 따른 회전 미러링
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