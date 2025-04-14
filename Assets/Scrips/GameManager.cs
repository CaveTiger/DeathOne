using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Debug.Log("GameManager Awake");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameManager 중복! 삭제됨");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CharacterLoader.Instance.LoadAllCharacters();
        if (CharacterData.characterDict != null)
        {
            Debug.Log("캐릭터 데이터 로딩 완료");
        }
    }
    private void Start()
    {
        StageLoader.Instance.LoadStages();
        foreach (var kvp in CharacterData.characterDict)
        {
            var data = kvp.Value;
            Debug.Log($"[딕셔너리 체크] ID: {data.ID}, HP: {data.Hp}, Atk: {data.Atk}, Def: {data.Def}, Speed{data.Speed}, Sprite: {data.Sprite}, EvasionRate{data.EvasionRate }, Accuracy{data.Accuracy}");
        } //스테이지 체크
        foreach (var kvp in StageManager.Instance.stageDict)
        {
            var stage = kvp.Value;
            Debug.Log($"[스테이지] ID: {stage.ID}, Label: {stage.Label}, Block 수: {stage.Blocks.Count}");
        }
        //스테이지 블록 체크
        foreach (var kvp in StageManager.Instance.stageBlockDict)
        {
            var block = kvp.Value;
            Debug.Log($"[블록] StageKey: {block.StageKey}, BlockType: {block.BlockType}, ParentBlockName: {block.ParentBlockName}");

            if (block.EnemyIDs.Count > 0)
                Debug.Log($" ┗ 적 ID: {string.Join(", ", block.EnemyIDs)}");

            if (block.EventList.Count > 0)
                Debug.Log($" ┗ 이벤트: {string.Join(", ", block.EventList)}");

            if (block.SpecialGuests.Count > 0)
                Debug.Log($" ┗ 스페셜 게스트: {string.Join(", ", block.SpecialGuests)}");

            if (!string.IsNullOrEmpty(block.FrontCutID) || !string.IsNullOrEmpty(block.BackCutID))
                Debug.Log($" ┗ 컷신: Front={block.FrontCutID}, Back={block.BackCutID}");
        }
    }
}
