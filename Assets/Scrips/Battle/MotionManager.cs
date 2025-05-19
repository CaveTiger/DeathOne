using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;

public class MotionManager : MonoBehaviour
{
    private static MotionManager instance;
    public static MotionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MotionManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("MotionManager");
                    instance = obj.AddComponent<MotionManager>();
                }
            }
            return instance;
        }
    }

    private SpriteRenderer spriteRenderer;
    private CharacterStats characterStats;
    private Dictionary<string, Sprite> motionSprites;
    private string characterFolder; // 예: "Player" 또는 "MobA"
    private string characterName;   // 예: "Player" 또는 "MobA"

    // 모드 제작자를 위한 스프라이트 매핑
    [System.Serializable]
    public class MotionSpriteData
    {
        public string motionType;  // 예: "Attack", "Hit", "Stand"
    }

    [SerializeField]
    private List<MotionSpriteData> motionSpriteDataList;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        spriteRenderer = GetComponent<SpriteRenderer>();
        characterStats = GetComponent<CharacterStats>();
        // 예시: 캐릭터 데이터에서 폴더명과 이름을 가져오는 방식
        characterFolder = characterStats.CharacterFolder; // "Player" 또는 "MobA"
        characterName = characterStats.CharacterName;     // "Player" 또는 "MobA"
        InitializeMotionSprites();
    }

    /// <summary>
    /// 모션 스프라이트 초기화
    /// </summary>
    private void InitializeMotionSprites()
    {
        motionSprites = new Dictionary<string, Sprite>();
        foreach (var data in motionSpriteDataList)
        {
            string fullPath = $"UnitSprite/{characterFolder}/{characterName}_{data.motionType}";
            Sprite sprite = Resources.Load<Sprite>(fullPath);
            if (sprite != null)
                motionSprites[data.motionType] = sprite;
            else
                Debug.LogWarning($"스프라이트 로드 실패: {fullPath}");
        }

        // 모드 스프라이트 로드
        LoadModSprites();
    }

    /// <summary>
    /// 모드 스프라이트 로드
    /// </summary>
    private void LoadModSprites()
    {
        string modPath = Path.Combine(Application.dataPath, "Mods");
        if (Directory.Exists(modPath))
        {
            foreach (var modFolder in Directory.GetDirectories(modPath))
            {
                string spritesPath = Path.Combine(modFolder, "Sprites", "Character", characterName);
                if (Directory.Exists(spritesPath))
                {
                    LoadModSpritesFromFolder(spritesPath);
                }
            }
        }
    }

    /// <summary>
    /// 모드 폴더에서 스프라이트 로드
    /// </summary>
    private void LoadModSpritesFromFolder(string folderPath)
    {
        foreach (var file in Directory.GetFiles(folderPath, "*.png"))
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            // 모션 타입만 추출 (예: "_Smash" 부분만)
            string motionType = fileName.Substring(fileName.LastIndexOf('_'));
            
            if (motionSprites.ContainsKey(motionType))
            {
                byte[] fileData = File.ReadAllBytes(file);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                Sprite modSprite = Sprite.Create(texture, 
                    new Rect(0, 0, texture.width, texture.height), 
                    new Vector2(0.5f, 0.5f));
                motionSprites[motionType] = modSprite;
            }
        }
    }

    /// <summary>
    /// 스킬 모션을 실행합니다.
    /// </summary>
    /// <param name="motionType">XML의 Motion 태그 값</param>
    public void PlaySkillMotion(string motionType)
    {
        if (spriteRenderer == null) return;
        if (!characterStats.IsActive) return;
        if (motionSprites.ContainsKey(motionType))
            spriteRenderer.sprite = motionSprites[motionType];
    }

    /// <summary>
    /// 피격 모션을 실행합니다.
    /// </summary>
    /// <param name="damage">받은 데미지</param>
    public void PlayHitMotion(int damage)
    {
        if (spriteRenderer == null) return;

        // 데미지에 따른 피격 모션 분기
        string motionType = damage > characterStats.MaxHp * 0.3f ? "_HeavyHit" : "_Hit";
    }

    /// <summary>
    /// 모션 시퀀스를 재생합니다.
    /// </summary>
    private IEnumerator PlayMotionSequence(string motionType)
    {
        // 현재 스프라이트 저장
        Sprite currentSprite = spriteRenderer.sprite;

        // 모션 스프라이트로 변경
        if (motionSprites.ContainsKey(motionType))
        {
            spriteRenderer.sprite = motionSprites[motionType];
        }

        // 모션 지속 시간
        yield return new WaitForSeconds(0.5f);

        // 원래 스프라이트로 복귀
        spriteRenderer.sprite = currentSprite;
    }
}
