using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Scrolling Settings")]
    [SerializeField] private float scrollSpeed = 5f;
    [SerializeField] private float chunkLength = 20f;
    private float despawnY;
    private float camBottomY;

    [Header("Map Prefabs")]
    [SerializeField] private GameObject[] startChunks;       // 처음 맵
    [SerializeField] private GameObject[] stage1Chunks;      // 1단계 맵
    [SerializeField] private GameObject transition1to2;      // 1->2단계 전환 맵 (1개)
    [SerializeField] private GameObject[] stage2Chunks;      // 2단계 맵
    [SerializeField] private GameObject transition2to3;      // 2->3단계 전환 맵 (1개)
    [SerializeField] private GameObject[] stage3Chunks;      // 3단계 맵

    // 맵의 진행 상태를 관리하는 열거형(Enum)
    private enum MapPhase { Start, Stage1, Trans1to2, Stage2, Trans2to3, Stage3 }
    private MapPhase currentPhase = MapPhase.Start;

    // 프리팹 원본을 Key로, 해당 프리팹으로 만들어진 오브젝트들의 Queue를 Value로 가지는 풀링 딕셔너리
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    // 현재 게임 화면(및 위아래 대기열)에 활성화된 3개의 맵 청크
    private List<GameObject> activeChunks = new List<GameObject>();

    void Start()
    {
        Camera cam = Camera.main;
        float halfH = cam.orthographic
            ? cam.orthographicSize
            : Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * Mathf.Abs(cam.transform.position.z);
        float camBottom = cam.transform.position.y - halfH;
        float camTop    = cam.transform.position.y + halfH;
        camBottomY = camBottom;
        despawnY = camBottom - chunkLength;

        Debug.Log($"[MapManager] 화면 Y: {camBottom:F2}~{camTop:F2} | chunkLength: {chunkLength}");

        // StartMap 1개 — 하단 기준으로 생성
        SpawnChunk(camBottom);
        currentPhase = MapPhase.Stage1;

        // 화면 상단 + 여유분까지 Stage1 청크를 실제 bounds 기준으로 채움
        float fillY = GetTopEdge(activeChunks.Count > 0 ? activeChunks[activeChunks.Count - 1] : null, camBottom + chunkLength);
        while (fillY <= camTop + chunkLength)
        {
            SpawnChunk(fillY);
            float newTop = GetTopEdge(activeChunks.Count > 0 ? activeChunks[activeChunks.Count - 1] : null, fillY + chunkLength);
            if (newTop <= fillY) break;
            fillY = newTop;
        }
    }

    void Update()
    {
        MoveChunks();
    }

    private void MoveChunks()
    {
        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            GameObject chunk = activeChunks[i];
            chunk.transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);

            // 청크 상단이 카메라 하단 아래로 완전히 벗어나면 디스폰
            float topEdge = GetTopEdge(chunk, chunk.transform.position.y + chunkLength);
            if (topEdge <= camBottomY)
            {
                // 가장 위 청크의 상단(= 새 청크의 하단)
                float spawnBottomY = GetTopEdge(activeChunks[activeChunks.Count - 1], activeChunks[activeChunks.Count - 1].transform.position.y + chunkLength);

                ReturnToPool(chunk);
                activeChunks.RemoveAt(i);
                SpawnChunk(spawnBottomY);
            }
        }
    }

    // bottomY: 이 청크의 하단(Renderer bounds 기준)이 위치할 Y좌표
    private void SpawnChunk(float bottomY)
    {
        GameObject prefabToSpawn = GetNextChunkPrefab();

        if (prefabToSpawn == null)
        {
            Debug.LogError("[MapManager] SpawnChunk: prefab이 null입니다. Inspector에서 프리팹 연결을 확인하세요.");
            return;
        }

        GameObject newChunk = GetFromPool(prefabToSpawn);
        // 임시로 원점에 배치 후 실제 bounds를 측정해 하단이 bottomY에 오도록 보정
        newChunk.transform.position = Vector3.zero;
        newChunk.SetActive(true);

        Renderer r = newChunk.GetComponentInChildren<Renderer>();
        float pivotY = r != null ? bottomY + (0f - r.bounds.min.y) : bottomY;
        newChunk.transform.position = new Vector3(0, pivotY, 0);

        activeChunks.Add(newChunk);
        Debug.Log($"[MapManager] Spawning: {prefabToSpawn.name} | bottomY={bottomY:F2} pivotY={pivotY:F2} topY={GetTopEdge(newChunk, pivotY + chunkLength):F2}");
    }

    private float GetTopEdge(GameObject chunk, float fallback)
    {
        if (chunk == null) return fallback;
        Renderer r = chunk.GetComponentInChildren<Renderer>();
        return r != null ? r.bounds.max.y : fallback;
    }

    // --- Object Pooling Logic ---
    private GameObject GetFromPool(GameObject prefab)
    {
        // 딕셔너리에 해당 프리팹의 큐가 없다면 새로 만들어줍니다.
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        // 큐에 재활용 가능한 객체가 있다면 꺼내줍니다.
        if (poolDictionary[prefab].Count > 0)
        {
            return poolDictionary[prefab].Dequeue();
        }
        // 없다면 새로 Instantiate로 찍어냅니다.
        else
        {
            GameObject newObj = Instantiate(prefab, transform);

            // 나중에 풀로 돌아올 때 자기가 어떤 큐로 들어가야 하는지 알게 하기 위해 헬퍼 컴포넌트를 붙입니다.
            MapChunk chunkInfo = newObj.AddComponent<MapChunk>();
            chunkInfo.originalPrefab = prefab;

            return newObj;
        }
    }

    private void ReturnToPool(GameObject chunk)
    {
        chunk.SetActive(false); // 오브젝트 끄기 (최적화 핵심)

        MapChunk chunkInfo = chunk.GetComponent<MapChunk>();
        if (chunkInfo != null && chunkInfo.originalPrefab != null)
        {
            // 원본 프리팹 정보에 맞는 큐에 다시 집어넣습니다.
            poolDictionary[chunkInfo.originalPrefab].Enqueue(chunk);
        }
        else
        {
            Destroy(chunk); // 예외 처리 (헬퍼가 파괴되었을 경우)
        }
    }

    // --- Phase & Transition Logic ---
    private GameObject GetNextChunkPrefab()
    {
        switch (currentPhase)
        {
            case MapPhase.Start:
                return GetRandomPrefab(startChunks);

            case MapPhase.Stage1:
                return GetRandomPrefab(stage1Chunks);

            case MapPhase.Trans1to2:
                if (transition1to2 == null)
                {
                    Debug.LogError("[MapManager] transition1to2 프리팹이 null입니다. Inspector를 확인하세요.");
                    currentPhase = MapPhase.Stage2;
                    return null;
                }
                currentPhase = MapPhase.Stage2;
                return transition1to2;

            case MapPhase.Stage2:
                return GetRandomPrefab(stage2Chunks);

            case MapPhase.Trans2to3:
                if (transition2to3 == null)
                {
                    Debug.LogError("[MapManager] transition2to3 프리팹이 null입니다. Inspector를 확인하세요.");
                    currentPhase = MapPhase.Stage3;
                    return null;
                }
                currentPhase = MapPhase.Stage3;
                return transition2to3;

            case MapPhase.Stage3:
                return GetRandomPrefab(stage3Chunks);

            default:
                return GetRandomPrefab(stage1Chunks);
        }
    }

    private GameObject GetRandomPrefab(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0) return null;
        return prefabs[Random.Range(0, prefabs.Length)];
    }

    // 외부에서 보스 처치나 스코어 도달 시 호출하는 난이도 상승 함수
    public void UpgradeStage()
    {
        if (currentPhase == MapPhase.Stage1)
        {
            currentPhase = MapPhase.Trans1to2;
            Debug.Log("2단계로 진입합니다!");
        }
        else if (currentPhase == MapPhase.Stage2)
        {
            currentPhase = MapPhase.Trans2to3;
            Debug.Log("3단계로 진입합니다!");
        }
    }
}

/// <summary>
/// 동적으로 생성된 오브젝트가 자신의 원본 프리팹을 기억하게 만드는 헬퍼 클래스입니다.
/// MapManager와 동일한 스크립트 파일 하단에 두거나, 별도의 C# 파일로 분리해도 됩니다.
/// </summary>
public class MapChunk : MonoBehaviour
{
    public GameObject originalPrefab;
}