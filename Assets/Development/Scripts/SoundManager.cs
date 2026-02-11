using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips - BGM")]
    public AudioClip main_Background;
    public AudioClip ingame_Background;

    [Header("Audio Clips - SFX")]
    public AudioClip stageSelect;
    public AudioClip categorySelectButton;
    public AudioClip selectButton;
    public AudioClip resetButton;
    public AudioClip gameStart;

    public AudioClip throwBag;
    public AudioClip hit;


    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 배경음악 재생 메서드
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        
        // PlayOneShot은 기존에 재생 중인 소리를 끊지 않고 덧입혀 재생합니다.
        sfxSource.PlayOneShot(clip);
    }
}