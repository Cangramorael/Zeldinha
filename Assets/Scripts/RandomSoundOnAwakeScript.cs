using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSoundOnAwakeScript : MonoBehaviour
{
    public List<AudioClip> audioClips;

    private AudioSource thisAudioSource;

    void Awake() {
        // Get Component
        thisAudioSource = GetComponent<AudioSource>();
    }
    // Start is called before the first frame update
    void Start()
    {
        // Play random clip
        AudioClip audioClip = audioClips[Random.Range(0, audioClips.Count)];
        thisAudioSource.PlayOneShot(audioClip);
    }
}
