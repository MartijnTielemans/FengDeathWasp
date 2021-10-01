using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Music Track")]
    public AudioSource Music;

    [Space]
    [Header("Menu Sounds")]
    public AudioSource[] menuSounds;

    [Space]
    [Header("Player Sounds")]
    public AudioSource[] playerSounds;

    [Space]
    [Header("Enemy Sounds")]
    public AudioSource[] enemySounds;

    [Space]
    [Header("Other Sounds")]
    public AudioSource[] otherSounds;

    [Space]
    [Header("Settings")]
    [SerializeField] bool playMusicAtStart = true;

    private void Awake()
    {
        tag = "SoundManager";
    }

    private void Start()
    {
        // Plays music upon starting the scene
        if (playMusicAtStart)
            Music.Play();
    }

    public void PlaySound(AudioSource sound)
    {
        sound.Play();
    }

    public void StopSound(AudioSource sound)
    {
        sound.Stop();
    }

    public void StartFadeOut(AudioSource sound, float duration)
    {
        StartCoroutine(FadeOutSound(sound, duration));
    }

    // Fades out a sound over a set duration
    IEnumerator FadeOutSound(AudioSource sound, float duration)
    {
        float currentTime = 0;
        float start = sound.volume;

        while(currentTime < duration)
        {
            currentTime += Time.deltaTime;
            sound.volume = Mathf.Lerp(start, 0, currentTime / duration);
            yield return null;
        }

        yield break;
    }
}
