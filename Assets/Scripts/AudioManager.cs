using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource AudioSource;
    public Scrollbar VolumeScrollbar;
    public AudioResource[] audioClips;

    public void ChangeVolume()
    {
        if (VolumeScrollbar == null)
        {
            return;
        }
        float volume = VolumeScrollbar.value;
        AudioSource.volume = volume;
    }

    public void PlayAudio(int index)
    {
        AudioSource.resource = audioClips[index];
        AudioSource.Play(); 
    }
    public void PlayTwoAudios(int index)
    {
        StartCoroutine(PlaySequential(index));
    }

    private IEnumerator PlaySequential(int index)
    {
        AudioSource.resource = audioClips[index];
        AudioSource.Play();
        yield return new WaitWhile(() => AudioSource.isPlaying);
        index += 1;
        if (index < audioClips.Length)
        {
            AudioSource.resource = audioClips[index];
            AudioSource.Play();
        }
    }
}