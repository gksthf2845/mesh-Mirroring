using UnityEngine;
using System.Collections.Generic;

// MeshFilter와 MeshRenderer 컴포넌트가 필수적으로 필요함
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshMirrorCustom : MonoBehaviour
{
    // 미러링할 축을 선택하는 열거형
    public enum MirrorAxis
    {
        X,
        Y,
        Z,
        Custom  // 커스텀 축 추가
    }

    [SerializeField] private MirrorAxis mirrorAxis = MirrorAxis.X;        // 미러링 기준 축 (기본값: X축)
    [SerializeField] private Vector3 customMirrorAxis = Vector3.right;    // 커스텀 미러 축 방향 (기본값: 오른쪽 방향)
    [SerializeField] private bool createMirrorOnStart = true;             // 시작 시 자동 미러링 생성 여부
    [SerializeField] private bool updateMirrorInRealtime = false;         // 실시간 미러링 업데이트 여부
    [SerializeField] private bool normalizeCustomAxis = true;             // 커스텀 축 정규화 여부

    private GameObject mirroredObject;         // 미러링된 오브젝트를 참조하는 변수
    private MeshFilter originalMeshFilter;     // 원본 오브젝트의 MeshFilter
    private MeshRenderer originalMeshRenderer; // 원본 오브젝트의 MeshRenderer
    private MeshFilter mirroredMeshFilter;     // 미러링된 오브젝트의 MeshFilter
    private MeshRenderer mirroredMeshRenderer; // 미러링된 오브젝트의 MeshRenderer

    private void Start()
    {
        // 원본 컴포넌트 참조 가져오기
        originalMeshFilter = GetComponent<MeshFilter>();
        originalMeshRenderer = GetComponent<MeshRenderer>();

        // 설정에 따라 시작 시 미러링된 메쉬 생성
        if (createMirrorOnStart)
        {
            CreateMirroredMesh();
        }
    }

    private void Update()
    {
        // 실시간 업데이트 옵션이 켜져 있고 미러 오브젝트가 존재하면 매 프레임 업데이트
        if (updateMirrorInRealtime && mirroredObject != null)
        {
            UpdateMirroredMesh();
        }
    }

    // 컨텍스트 메뉴에 기능 추가 (Inspector에서 오브젝트 우클릭 시 메뉴에 표시됨)
    [ContextMenu("Create Mirrored Mesh")]
    public void CreateMirroredMesh()
    {
        // 이미 미러링된 오브젝트가 있다면 삭제
        if (mirroredObject != null)
        {
            DestroyImmediate(mirroredObject);
        }

        // 미러링된 메쉬를 담을 새 게임 오브젝트 생성
        mirroredObject = new GameObject($"{gameObject.name}_Mirrored");
        mirroredObject.transform.parent = transform.parent;

        // 필요한 컴포넌트 추가
        mirroredMeshFilter = mirroredObject.AddComponent<MeshFilter>();
        mirroredMeshRenderer = mirroredObject.AddComponent<MeshRenderer>();

        // 원본 오브젝트의 머티리얼을 미러링된 오브젝트에 복사
        mirroredMeshRenderer.sharedMaterials = originalMeshRenderer.sharedMaterials;

        // 미러링된 메쉬 생성 및 업데이트
        UpdateMirroredMesh();
    }

    // 미러링된 메쉬 업데이트 메소드
    [ContextMenu("Update Mirrored Mesh")]
    public void UpdateMirroredMesh()
    {
        // 미러 오브젝트가 없으면 새로 생성
        if (mirroredObject == null)
        {
            CreateMirroredMesh();
            return;
        }

        // 원본 메쉬 가져오기
        Mesh originalMesh = originalMeshFilter.sharedMesh;

        // 미러링된 메쉬를 위한 새 메쉬 객체 생성
        Mesh mirroredMesh = new Mesh();
        mirroredMesh.name = $"{originalMesh.name}_Mirrored";

        // 원본 메쉬의 데이터 가져오기
        Vector3[] originalVertices = originalMesh.vertices;    // 정점 배열
        int[] originalTriangles = originalMesh.triangles;      // 삼각형 인덱스 배열
        Vector3[] originalNormals = originalMesh.normals;      // 법선 벡터 배열
        Vector2[] originalUVs = originalMesh.uv;               // UV 좌표 배열

        // 미러링된 메쉬 데이터를 위한 배열 생성
        Vector3[] mirroredVertices = new Vector3[originalVertices.Length];
        int[] mirroredTriangles = new int[originalTriangles.Length];
        Vector3[] mirroredNormals = new Vector3[originalNormals.Length];

        // 커스텀 축 정규화 (길이가 1이 되도록)
        Vector3 axisNormalized = customMirrorAxis;
        if (mirrorAxis == MirrorAxis.Custom && normalizeCustomAxis)
        {
            axisNormalized = customMirrorAxis.normalized;
        }

        // 선택된 축에 따라 정점과 법선 벡터 미러링
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];
            Vector3 normal = originalNormals[i];

            switch (mirrorAxis)
            {
                case MirrorAxis.X:
                    vertex.x = -vertex.x;  // X축 기준 정점 반전
                    normal.x = -normal.x;  // X축 기준 법선 벡터 반전
                    break;
                case MirrorAxis.Y:
                    vertex.y = -vertex.y;  // Y축 기준 정점 반전
                    normal.y = -normal.y;  // Y축 기준 법선 벡터 반전
                    break;
                case MirrorAxis.Z:
                    vertex.z = -vertex.z;  // Z축 기준 정점 반전
                    normal.z = -normal.z;  // Z축 기준 법선 벡터 반전
                    break;
                case MirrorAxis.Custom:
                    // 커스텀 축 기준 미러링 (반사 변환)
                    // 공식: v' = v - 2 * (v·n) * n, 여기서 n은 정규화된 축 벡터
                    float dot = Vector3.Dot(vertex, axisNormalized);
                    vertex = vertex - 2 * dot * axisNormalized;

                    // 법선 벡터도 동일한 방식으로 반사
                    dot = Vector3.Dot(normal, axisNormalized);
                    normal = normal - 2 * dot * axisNormalized;
                    break;
            }

            mirroredVertices[i] = vertex;
            mirroredNormals[i] = normal;
        }

        // 삼각형 인덱스 순서 뒤집기 (면 방향 유지를 위해 필수)
        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            mirroredTriangles[i] = originalTriangles[i];          // 첫 번째 정점 유지
            mirroredTriangles[i + 1] = originalTriangles[i + 2];  // 두 번째와 세 번째 정점 순서 바꿈
            mirroredTriangles[i + 2] = originalTriangles[i + 1];  // 이렇게 하면 면 방향이 올바르게 유지됨
        }

        // 미러링된 메쉬에 데이터 설정
        mirroredMesh.vertices = mirroredVertices;
        mirroredMesh.triangles = mirroredTriangles;
        mirroredMesh.normals = mirroredNormals;
        mirroredMesh.uv = originalUVs;  // UV는 미러링할 필요 없음
        mirroredMesh.RecalculateBounds();  // 메쉬 경계 재계산

        // 생성된 메쉬를 미러링된 오브젝트에 할당
        mirroredMeshFilter.sharedMesh = mirroredMesh;

        // 미러링된 오브젝트 위치 업데이트
        UpdateMirroredPosition();
    }

    // 미러링된 오브젝트의 위치 업데이트
    private void UpdateMirroredPosition()
    {
        // 회전과 크기는 원본과 동일하게 설정
        mirroredObject.transform.rotation = transform.rotation;
        mirroredObject.transform.localScale = transform.localScale;

        // 선택된 축에 따라 미러링된 위치 계산
        Vector3 mirroredPosition = transform.position;
        Vector3 axisNormalized = customMirrorAxis;

        if (mirrorAxis == MirrorAxis.Custom && normalizeCustomAxis)
        {
            axisNormalized = customMirrorAxis.normalized;
        }

        switch (mirrorAxis)
        {
            case MirrorAxis.X:
                mirroredPosition.x = -mirroredPosition.x;  // X 위치 반전
                break;
            case MirrorAxis.Y:
                mirroredPosition.y = -mirroredPosition.y;  // Y 위치 반전
                break;
            case MirrorAxis.Z:
                mirroredPosition.z = -mirroredPosition.z;  // Z 위치 반전
                break;
            case MirrorAxis.Custom:
                // 커스텀 축 기준 위치 미러링
                float dot = Vector3.Dot(mirroredPosition, axisNormalized);
                mirroredPosition = mirroredPosition - 2 * dot * axisNormalized;
                break;
        }

        // 계산된 위치 적용
        mirroredObject.transform.position = mirroredPosition;
    }

    // 런타임에 미러링 축 변경 메소드
    public void SetMirrorAxis(MirrorAxis newAxis)
    {
        // 축이 변경된 경우에만 업데이트
        if (mirrorAxis != newAxis)
        {
            mirrorAxis = newAxis;
            if (mirroredObject != null)
            {
                UpdateMirroredMesh();
            }
        }
    }

    // 정수 인덱스로 미러링 축 변경 (UI 연결용)
    public void SetMirrorAxis(int axisIndex)
    {
        if (axisIndex >= 0 && axisIndex <= (int)MirrorAxis.Custom)
        {
            SetMirrorAxis((MirrorAxis)axisIndex);
        }
    }

    // 커스텀 미러링 축 설정 메소드
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

    // 편의 메소드: 미러링 평면 설정 (법선 벡터로 정의)
    public void SetMirrorPlane(Vector3 planeNormal)
    {
        SetMirrorAxis(MirrorAxis.Custom);
        SetCustomMirrorAxis(planeNormal);
    }

    // 편의 메소드: 두 점 사이의 중간 평면으로 미러링
    public void SetMirrorPlaneBetweenPoints(Vector3 pointA, Vector3 pointB)
    {
        Vector3 midpoint = (pointA + pointB) * 0.5f;
        Vector3 direction = (pointB - pointA).normalized;

        // 이 오브젝트의 위치를 고려한 상대적 미러 평면 설정
        transform.position = midpoint;
        SetMirrorPlane(direction);
    }

    // 원본 오브젝트가 삭제될 때 미러 오브젝트도 함께 삭제
    private void OnDestroy()
    {
        if (mirroredObject != null)
        {
            DestroyImmediate(mirroredObject);
        }
    }

#if UNITY_EDITOR
    // 에디터에서 시각적으로 미러 축 표시 (Scene 뷰에서만 보임)
    private void OnDrawGizmosSelected()
    {
        // 미러 축 시각화
        Gizmos.color = Color.cyan;

        // 선택된 축에 따라 다른 방식으로 시각화
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

                // 미러 평면 시각화
                Gizmos.color = new Color(0, 1, 1, 0.2f);

                // 평면을 정의하는 두 개의 수직 벡터 찾기
                Vector3 perpendicular1 = Vector3.Cross(axisDir, Vector3.up);
                if (perpendicular1.magnitude < 0.001f)
                {
                    perpendicular1 = Vector3.Cross(axisDir, Vector3.right);
                }
                perpendicular1 = perpendicular1.normalized;
                Vector3 perpendicular2 = Vector3.Cross(axisDir, perpendicular1).normalized;

                // 평면 그리기
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