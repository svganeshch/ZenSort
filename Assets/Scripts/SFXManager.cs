using DG.Tweening;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    public AudioClip[] propPickupSounds;
    public AudioClip propMatched;
    public AudioClip starBonus;
    public AudioClip levelDoneSound;
    public AudioClip levelFailedSound;

    public AudioClip buttonTap;

    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    public AudioClip ChooseRandomSFXFromArray(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("ChooseRandomSFXFromArray: Clips array is null or empty.");
            return null;
        }

        int randomIndex = Random.Range(0, clips.Length);

        return clips[randomIndex];
    }

    public void PlaySFX(AudioClip clip, float volume = 1, bool fade = false, float fadeDuration = 0f)
    {
        audioSource.PlayOneShot(clip, volume);
        
        if (fade) audioSource.DOFade(0, fadeDuration).OnComplete(() =>
        {
            audioSource.Stop();
            audioSource.volume = 1;
        });
    }

    public void QuickFade()
    {
        audioSource.DOFade(0, 1.5f).OnComplete(() =>
        {
            audioSource.Stop();
            audioSource.volume = 1;
        });
    }

    public void PlayPropPickedSound()
    {
        PlaySFX(ChooseRandomSFXFromArray(propPickupSounds));
    }

    public void PlayPropMatchedSound()
    {
        PlaySFX(propMatched);
    }

    public void PlayStarBonusSound()
    {
        PlaySFX(starBonus);
    }
}
