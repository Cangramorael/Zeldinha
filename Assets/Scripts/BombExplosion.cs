using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombExplosion : MonoBehaviour
{
    public float fade = 1f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeAway(fade));
    }

    private IEnumerator FadeAway(float delay) {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
