using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AudioManager>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private AudioClip musikMainMenu;
    private AudioClip musikWin;
    private AudioClip musikGameOver;
    private AudioClip musikDead;
    private AudioClip clipJump;
    private AudioClip clipHit;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Inisialisasi AudioSource jika belum ada
        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // Cari AudioSource kedua untuk SFX
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length > 1)
        {
            sfxSource = sources[1];
        }
        else
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // Muat semua klip audio secara dinamis dari folder Resources
        musikMainMenu = Resources.Load<AudioClip>("Music/musik_main_menu");
        musikWin = Resources.Load<AudioClip>("Music/win");
        musikGameOver = Resources.Load<AudioClip>("Music/game_over");
        musikDead = Resources.Load<AudioClip>("Music/dead");
        clipJump = Resources.Load<AudioClip>("Music/jump");
        clipHit = Resources.Load<AudioClip>("Music/hit");
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(musikMainMenu);
    }

    public void PlayWinMusic()
    {
        PlayMusic(musikWin);
    }

    public void PlayGameOverMusic()
    {
        PlayMusic(musikGameOver);
    }

    public void PlayDeadMusic()
    {
        PlayMusic(musikDead);
    }

    public void PlayJumpSFX()
    {
        PlaySFX(clipJump);
    }

    public void PlayHitSFX()
    {
        PlaySFX(clipHit);
    }

    public void PlayGameOverSFX()
    {
        StopMusic();
        PlaySFX(musikGameOver);
    }

    public void PlayDeadSFX()
    {
        StopMusic();
        PlaySFX(musikDead);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
