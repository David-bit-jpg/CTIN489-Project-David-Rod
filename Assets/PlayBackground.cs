using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayBackground : MonoBehaviour
{
    public AudioSource AudioSource;
    [SerializeField] private AudioClip Audio;
    [SerializeField] public float volume = 1f;
    private float nextPlayTime = 0f;

    private void Awake()
    {
        AudioSource = gameObject.AddComponent<AudioSource>();
        AudioSource.clip = Audio;
        AudioSource.loop = false;
        AudioSource.volume = volume;

    }
    void Update()
    {
        if (Time.time >= nextPlayTime)
        {
            AudioSource.Play();
            nextPlayTime = Time.time + Random.Range(1f, 2.5f);
        }
        AudioSource.volume = volume;
    }
}
