using UnityEngine;
using UnityEngine.TextCore.Text;

public class Debuger : MonoBehaviour
{
        public string characterID = "000001";         // 생성할 캐릭터 ID
        public Vector3 spawnPosition;      // 생성 위치
        public int count = 1;              // 생성 수 (1 이상)
    private void OnMouseDown()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = transform.position + new Vector3(i * 1.5f, 0, 0);
            SpawnManager.Instance.SpawnCharacter(characterID, pos);
        }
    }
}
