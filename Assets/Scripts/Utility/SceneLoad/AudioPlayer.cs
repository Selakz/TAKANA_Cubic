using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    // Serializable and Public
    public AudioSource audioSource;
    public AudioClip[] clipSequence;

    // Private
    private int p = 0;

    // Static

    // Defined Function
    public void AudioPlay()
    {
        if (p >= clipSequence.Length) return;
        audioSource.clip = clipSequence[p];
        audioSource.Play();
        p++;
    }

    // System Function
}
