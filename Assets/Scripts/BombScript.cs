using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombScript : MonoBehaviour
{
    public float ExplosionDelay = 5f;

    public GameObject ExplosionPrefab;

    public GameObject WoodBreakingPrefab;

    public float BlastRadius = 1.5f;

    public float BlastDamage = 10f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ExplosionCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator ExplosionCoroutine(){
        //Wait
        yield return new  WaitForSeconds(ExplosionDelay);

        //Explode
        Explode();
    }

    private void Explode() {
        //Create VFX
        Instantiate(ExplosionPrefab, transform.position, ExplosionPrefab.transform.rotation);

        //Destroy plataforms
        Collider[] colliders = Physics.OverlapSphere(transform.position, BlastRadius);
        foreach(Collider collider in colliders) {
            GameObject hitObject = collider.gameObject;
            if(hitObject.CompareTag("Platform")) {
                LifeScript lifeScript = hitObject.GetComponent<LifeScript>();
                if(lifeScript != null) {
                    float distance = (hitObject.transform.position - transform.position).magnitude;
                    float distanceRate = Mathf.Clamp(distance / BlastRadius, 0, 1);
                    float damageRate = 1f - Mathf.Pow(distanceRate, 4);
                    int damage = (int) Mathf.Ceil(damageRate * BlastDamage);
                    lifeScript.health -= damage;
                    if(lifeScript.health <= 0) {
                        Instantiate(WoodBreakingPrefab, hitObject.transform.position, WoodBreakingPrefab.transform.rotation);
                        Destroy(hitObject);
                    }
                }
            }
        }

        //Create SFX
        //To do

        //Destroy Bomb
        Destroy(gameObject);
    }
}
