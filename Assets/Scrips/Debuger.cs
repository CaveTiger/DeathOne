using UnityEngine;
using UnityEngine.TextCore.Text;

public class Debuger : MonoBehaviour
{
        public string characterID = "000001";         // ������ ĳ���� ID
        public Vector3 spawnPosition;      // ���� ��ġ
        public int count = 1;              // ���� �� (1 �̻�)
    private void OnMouseDown()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = transform.position + new Vector3(i * 1.5f, 0, 0);
            SpawnManager.Instance.SpawnCharacter(characterID, pos);
        }
    }
}
