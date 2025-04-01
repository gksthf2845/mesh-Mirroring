using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;


public class MeshMirrorComponent : MonoBehaviour
{
    // 미러링할 축 선택
    public enum MirrorAxis
    {
        X, Y, Z
    }

    [SerializeField] private MirrorAxis mirrorAxis = MirrorAxis.X;        // 미러링 기준 축 
    [SerializeField] private bool updateMirrorInRealtime = false;         // 실시간 미러링 업데이트 여부

    private GameObject mirroredObject;         // 미러링된 오브젝트를 참조하는 변수
    private MeshFilter originalMeshFilter;     // 원본 오브젝트의 MeshFilter
    private MeshRenderer originalMeshRenderer; // 원본 오브젝트의 MeshRenderer
    private MeshFilter mirroredMeshFilter;     // 미러링된 오브젝트의 MeshFilter
    private MeshRenderer mirroredMeshRenderer; // 미러링된 오브젝트의 MeshRenderer

    //업데이트에서 매프레임 메모리 재할당을 막기위한 캐시 
    private Mesh originalMesh; // 원본 오브젝트의 메쉬 캐시
    private Mesh mirroredMesh; // 재사용할 미러링된 오브젝트 메쉬 캐시
    Vector3 mirroredPosition; // 미러링 오브젝트 위치


    //한번만 쓰일 변수.
    private Vector2[] originalUVs;// uv만 따로 캐시
    private Vector3 mirroredObjectLocalSacle = new Vector3(1, 1, 1); // 자식으로 생긴 오브젝트의 로컬스케일 초기화값

    private void Start()
    {
        // 원본 컴포넌트 참조 가져오기
        originalMeshFilter = GetComponent<MeshFilter>();
        originalMeshRenderer = GetComponent<MeshRenderer>();

        CreateMirroredMesh();

    }

    private void Update()
    {
        //보너스과제2. 실시간 업데이트 옵션
        if (updateMirrorInRealtime && mirroredObject != null)
        {
            UpdateMirroredMesh();
        }
    }

    //처음 한 번 메쉬 생성
    public void CreateMirroredMesh()
    {
        // 원본 메쉬 가져오기
        originalMesh = originalMeshFilter.sharedMesh;
        originalUVs = originalMesh.uv;               



        // 미러링된 메쉬를 담을 새 오브젝트 생성
        mirroredObject = new GameObject(gameObject.name + "_Mirrored");
        //보너스과제3. 원본오브젝트에서 자식으로 생성
        mirroredObject.transform.parent = transform;

        // 필요한 컴포넌트 추가
        mirroredMeshFilter = mirroredObject.AddComponent<MeshFilter>();
        mirroredMeshRenderer = mirroredObject.AddComponent<MeshRenderer>();

        // 원본 오브젝트의 머티리얼을 미러링된 오브젝트에 복사
        mirroredMeshRenderer.sharedMaterials = originalMeshRenderer.sharedMaterials;

       
    }

    public void UpdateMirroredMesh()
    {
        if (mirroredObject == null)
        {
            // Debug.Log("미러메쉬 생성되지 않음");
            return;
        }

        // 기존 미러 메쉬 재사용 하기 전 널체크
        if (mirroredMeshFilter.sharedMesh != null)
        {
            // 기존 메쉬 재사용
            mirroredMesh = mirroredMeshFilter.sharedMesh;
            // Debug.Log("재사용");
        }
        else
        {
            // 처음 한 번만 새 메쉬 생성
            mirroredMesh = new Mesh();
        }


        // 원본 메쉬의 데이터 가져오기
        Vector3[] originalVertices = originalMesh.vertices;    // 정점 배열
        int[] originalTriangles = originalMesh.triangles;      // 삼각형 인덱스 배열
        Vector3[] originalNormals = originalMesh.normals;      // 법선 벡터 배열


        // 미러링된 메쉬 데이터를 위한 배열 생성
        Vector3[] mirroredVertices = new Vector3[originalVertices.Length];
        int[] mirroredTriangles = new int[originalTriangles.Length];
        Vector3[] mirroredNormals = new Vector3[originalNormals.Length];

        // 선택된 축에 따라 정점과 법선 벡터 미러링
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];
            Vector3 normal = originalNormals[i];

            //각 축마다 버텍스,법선벡터 반전
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
        mirroredMesh.vertices = mirroredVertices;
        mirroredMesh.triangles = mirroredTriangles;
        mirroredMesh.normals = mirroredNormals;
        mirroredMesh.uv = originalUVs;
        // 생성된 메쉬를 미러링된 오브젝트에 할당
        mirroredMeshFilter.sharedMesh = mirroredMesh;

        //보너스과제1 실시간 업데이트 옵션 미러링된 오브젝트 위치,회전,크기 업데이트
        UpdateMirroredPosition();
        UpdateMirroredRotation();
        mirroredObject.transform.localScale = mirroredObjectLocalSacle;
    }


    private void UpdateMirroredPosition()
    {

        // 선택된 축에 따라 미러링된 위치 계산
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
        // 회전값은 각 축에대한 xyzw 복합적으로 필요하기때문에 지역변수 할당.
        Quaternion originRotation = transform.rotation;
        Quaternion mirroredRotation;

        // 축에 따른 회전 미러링
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