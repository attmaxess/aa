using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Space(10)] public List<AudioClip> listMusicGamePlayNormal;
    public List<AudioClip> listMusicGamePlayBoss;
    public List<AudioClip> listMusicMainMenu;
    public List<AudioClip> listMusicSelectLevel;
    [SerializeField] List<AudioClip> listDialogWin = null;
    [SerializeField] List<AudioClip> listDialogLose = null;

    [Space(10), SerializeField]
    AudioSource audioSource = null;
    [SerializeField] AudioSource audioSourceLoop = null;
    [SerializeField] AudioSource audioSourceForEnemy = null;
    [SerializeField] AudioSource audioSourceForCharactor = null;
    [SerializeField] AudioSource audioSourceForBoss = null;
    [SerializeField] AudioSource backgroundSound = null;
    [SerializeField] AudioSource nextPageLevelAudioSource = null;
    [SerializeField] AudioSource audioSourceForTrap = null;

    [Space(20)]
    public AudioClip mainButton;
    public AudioClip playButton;
    public AudioClip choiceLevelButton;

    [Header("GamePlay"), Space(10)]
    public AudioClip crowdBattle;
    public AudioClip duplicate;
    public AudioClip lineWin;
    public AudioClip step;
    public AudioClip charactorFail;
    public AudioClip rescuePrincess;
    public AudioClip hotWater;
    public AudioClip punjiAndStoneTrap;
    public AudioClip trapHit;
    public AudioClip bonusX2Army;

    [Header("Boss"), Space(10)]
    public List<AudioClip> bossAttack;
    public AudioClip bossDie;
    public AudioClip wolfAttack;

    [Header("Weapon Charactor"), Space(10)]
    public AudioClip maceMc;
    public AudioClip archeryMc;
    public AudioClip swordMc;
    public AudioClip spearMc;
    public AudioClip shieldMc;
    public AudioClip swordShieldMc;

    [Header("Weapon Enemy"), Space(10)]
    public AudioClip maceEn;
    public AudioClip archeryEn;
    public AudioClip swordEn;
    public AudioClip spearEn;
    public AudioClip shieldEn;
    public AudioClip swordShieldEn;

    private float volSound;
    private int index;
    private bool soundEnable;

    public bool SoundEnable
    {
        get
        {
            return soundEnable;
        }
        set
        {
            soundEnable = value;
            SectionSettings.SoundEnable = value;
        }
    }

    // Use this for initialization
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }

        soundEnable = SectionSettings.SoundEnable;
    }

    void Start()
    {
        UpdateVolume();
    }

    public void UpdateVolume()
    {
        volSound = 1;
        SoundBackground();
    }
    public void UpdateSoundBG()
    {
        if (soundEnable)
        {
            PlayBackgroundSound(listMusicMainMenu);
        }
        else
        {
            StopMusicSound();
        }
    }

    #region PlaySound
    public void PlayWin()
    {
        PlayRandomAudioClip(listDialogWin);
    }
    public void PlayLose()
    {
        PlayRandomAudioClip(listDialogLose);
    }
    public void PlayNextPageLevel()
    {
        if (soundEnable && !nextPageLevelAudioSource.isPlaying)
        {
            nextPageLevelAudioSource.Play();
        }
    }
    public void PlayMoving()
    {
        if (soundEnable)
        {
            audioSourceLoop.clip = step;
            audioSourceLoop.Play();
        }
    }
    public void StopMoving()
    {
        if (soundEnable)
        {
            audioSourceLoop.Stop();
            audioSourceLoop.clip = null;
        }
    }
    public void PlayCrownBattle()
    {
        if (soundEnable)
        {
            audioSourceLoop.clip = crowdBattle;
            audioSourceLoop.Play();
        }
    }
    public void StopCrownBattle()
    {
        if (soundEnable)
        {
            audioSourceLoop.Stop();
            audioSourceLoop.clip = null;
        }
    }
    #endregion    

    public void SoundBackground()
    {
        backgroundSound.volume = volSound;
    }

    AudioClip currentPlayAudioClip;

    public void PlayAudioClip(AudioClip audio)
    {
        if (soundEnable)
        {
            audioSource.PlayOneShot(audio, volSound);
            currentPlayAudioClip = audio;
        }
    }
    public void PlayAudioClipCharactor(AudioClip audio)
    {
        if (soundEnable)
        {
            audioSourceForCharactor.PlayOneShot(audio, volSound);
            currentPlayAudioClip = audio;
        }
    }
    public void PlayAudioClipEnemy(AudioClip audio)
    {
        if (soundEnable)
        {
            audioSourceForEnemy.PlayOneShot(audio, volSound);
            currentPlayAudioClip = audio;
        }
    }

    public void PlayAudioClipForTrap(AudioClip clip)
    {
        if (soundEnable)
        {
            audioSourceForTrap.PlayOneShot(clip, volSound);
            currentPlayAudioClip = clip;
        }
    }

    public void PlayAudioClipBoss(AudioClip audio)
    {
        if (soundEnable)
        {
            audioSourceForBoss.PlayOneShot(audio, volSound);
            currentPlayAudioClip = audio;
        }
    }
    public void PlayRandomAudioClipBoss(List<AudioClip> listAudioClip)
    {
        if (soundEnable)
        {
            AudioClip playClip = listAudioClip[Random.Range(0, listAudioClip.Count)];
            audioSourceForBoss.PlayOneShot(playClip, volSound);
            currentPlayAudioClip = playClip;
        }
    }
    public void PlayRandomAudioClip(List<AudioClip> listAudioClip)
    {
        if (soundEnable)
        {
            AudioClip playClip = listAudioClip[Random.Range(0, listAudioClip.Count)];
            audioSource.PlayOneShot(playClip, volSound);
            currentPlayAudioClip = playClip;
        }
    }

    #region BackgroundSound

    public void PlayBackgroundSound(List<AudioClip> listBg)
    {
        if (soundEnable)
        {
            if (backgroundSound.clip != null)
            {
                if (coFadeBackgroundSound != null)
                {
                    StopCoroutine(coFadeBackgroundSound);
                    coFadeBackgroundSound = null;
                }

                coFadeBackgroundSound = StartCoroutine(IEFadeInSound(backgroundSound, 2f, delegate
                {
                    backgroundSound.clip = listBg[Random.Range(0, listBg.Count)];
                    backgroundSound.Play();
                }));
            }
            else
            {
                backgroundSound.clip = listBg[Random.Range(0, listBg.Count)];
                backgroundSound.Play();
            }
        }
    }

    public void StopMusicSound()
    {
        if (coFadeBackgroundSound != null)
        {
            StopCoroutine(coFadeBackgroundSound);
            coFadeBackgroundSound = null;
        }

        coFadeBackgroundSound = StartCoroutine(IEFadeInSound(backgroundSound, 2f, delegate
        {
            backgroundSound.Stop();
            backgroundSound.clip = null;
        }));
    }

    Coroutine coFadeBackgroundSound;

    #endregion

    IEnumerator PlaySoundDelay(float timeDelay, AudioClip audio)
    {
        yield return new WaitForSeconds(timeDelay);
        PlayAudioClip(audio);
    }

    IEnumerator IEFadeInSound(AudioSource audioSource, float duration, Callback callback)
    {
        float half = duration / 2f;
        float t = 0;
        while (t < half)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(volSound, 0, t / half);
            yield return null;
        }

        audioSource.volume = 0;
        if (callback != null)
        {
            callback.Invoke();
        }

        t = 0;
        while (t < half)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, volSound, t / half);
            yield return null;
        }
        audioSource.volume = 1;
    }
}
