using UnityEngine;

public class BossEnd : MonoBehaviour
{
    private void OnDestroy()
    {
        // 보스가 파괴될 때 게임 종료
        GameManager.Instance.GameOver("you got the boss\n");
    }
}
