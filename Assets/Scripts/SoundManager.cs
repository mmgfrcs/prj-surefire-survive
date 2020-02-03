using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] internal float BGMFadeTime = 1;
    [SerializeField] internal AudioClip chestOpenClip, keyCollectClip, goblinAttackClip, goblinLaughClip, trollAttackClip, trollRoarClip, doorRumbleClip;
    [SerializeField] internal AudioSource normalSource, hordeSource;

    bool normalMode = true;
    static SoundManager instance;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    internal static void PlaySound(AudioSource source, SoundType type)
    {
        AudioClip toPlay = SelectSound(type);

        print($"Playing {toPlay.name} ({toPlay.length.ToString("n3")}s) on {source.gameObject.name}");
        source.PlayOneShot(toPlay, type == SoundType.DoorRumble ? 2f : 0.8f);
    }

    internal static void PlaySound(SoundType type)
    {
        AudioClip toPlay = SelectSound(type);

        print($"Playing {toPlay.name} ({toPlay.length.ToString("n3")}s) on Player");
        AudioSource.PlayClipAtPoint(toPlay, GameManager.Instance.PlayerObject.transform.position, type == SoundType.DoorRumble ? 2f : 1f);
    }

    static AudioClip SelectSound(SoundType type)
    {
        if (type == SoundType.ChestOpen) return instance.chestOpenClip;
        else if (type == SoundType.KeyCollect) return instance.keyCollectClip;
        else if (type == SoundType.GoblinAttack) return instance.goblinAttackClip;
        else if (type == SoundType.GoblinLaugh) return instance.goblinLaughClip;
        else if (type == SoundType.TrollAttack) return instance.trollAttackClip;
        else if (type == SoundType.DoorRumble) return instance.doorRumbleClip;
        else return instance.trollRoarClip;
    }

    internal static void CrossfadeBGM()
    {
        instance.normalMode = !instance.normalMode;
        instance.StartCoroutine(instance.BGMSwitch());
    }

    IEnumerator BGMSwitch()
    {
        if(!normalMode)
        {
            hordeSource.Play();
            while (normalSource.volume > 0 && hordeSource.volume < 1)
            {
                normalSource.volume -= Time.deltaTime / BGMFadeTime;
                hordeSource.volume += Time.deltaTime / BGMFadeTime;
                yield return null;
            }
            normalSource.Pause();
        }
        else
        {
            normalSource.Play();
            while (normalSource.volume < 1 && hordeSource.volume > 0)
            {
                normalSource.volume += Time.deltaTime / BGMFadeTime;
                hordeSource.volume -= Time.deltaTime / BGMFadeTime;
                yield return null;
            }
            hordeSource.Pause();
        }
    }

    public enum SoundType
    {
        ChestOpen, KeyCollect, GoblinAttack, GoblinLaugh, TrollAttack, TrollRoar, DoorRumble
    }
}
