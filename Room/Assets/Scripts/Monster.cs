using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    Rigidbody2D rigid ;
    public Transform target ;
    public float speed = 2;
    public float health = 3;

    Animator anim ;
    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = target.position - this.transform.position ;
        rigid.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D other){
        if(other.gameObject.CompareTag("Weapon")){
            health-- ;
            if(health > 0){
                anim.SetTrigger("Hit");
            }
            else {
                Destroy(this.gameObject);
            }
            

        }
        
    } 
}
