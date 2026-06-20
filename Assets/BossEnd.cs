using UnityEngine;

public class BossEnd : MonoBehaviour
{
    private void OnDestroy()
    {
        // 보스가 파괴될 때 게임 종료
        GameManager.Instance.GameOver("드이어 탈출이다!!!");
    }
}
