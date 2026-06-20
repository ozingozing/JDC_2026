using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Scrolling Settings")]
    [SerializeField] private float scrollSpeed = 5f;
    [SerializeField] private float chunkLength = 20f;
    [SerializeField] private float despawnY = -25f;

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
        // 게임 시작 시 맵 3개를 세로로 이어 붙여 스폰합니다.
        SpawnChunk(0f);
        SpawnChunk(chunkLength);
        SpawnChunk(chunkLength * 2f);

        // 처음 맵 스폰 직후, 다음 스폰부터는 1단계 맵이 나오도록 페이즈를 넘깁니다.
        currentPhase = MapPhase.Stage1;
    }

    void Update()
    {
        MoveChunks();
    }

    private void MoveChunks()
    {
        // 리스트에서 요소를 제거해야 하므로 역순으로 반복문을 돕니다.
        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            GameObject chunk = activeChunks[i];

            // 아래로 이동
            chunk.transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);

            // 카메라 아래로 벗어났다면
            if (chunk.transform.position.y <= despawnY)
            {
                // 새 청크가 스폰될 위치 계산 (가장 위에 있는 맵의 바로 위)
                float spawnY = activeChunks[activeChunks.Count - 1].transform.position.y + chunkLength;

                // 벗어난 청크는 풀에 반납(비활성화)하고 리스트에서 제거
                ReturnToPool(chunk);
                activeChunks.RemoveAt(i);

                // 맨 위에 새로운 청크 스폰
                SpawnChunk(spawnY);
            }
        }
    }

    private void SpawnChunk(float yPosition)
    {
        // 현재 페이즈에 맞는 프리팹을 결정
        GameObject prefabToSpawn = GetNextChunkPrefab();

        // 풀에서 꺼내오거나 새로 생성
        GameObject newChunk = GetFromPool(prefabToSpawn);

        // 위치 설정 및 활성화
        newChunk.transform.position = new Vector3(0, yPosition, 0);
        newChunk.SetActive(true);

        activeChunks.Add(newChunk);
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
                // 트랜지션 맵을 하나 내보내고 나면, 바로 다음 맵부터는 2단계가 나오도록 페이즈를 전환합니다.
                currentPhase = MapPhase.Stage2;
                return transition1to2;

            case MapPhase.Stage2:
                return GetRandomPrefab(stage2Chunks);

            case MapPhase.Trans2to3:
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