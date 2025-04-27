using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 1; 
    Rigidbody2D rigid ;
    public GameObject weaponPf; GameObject weapon; 
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        weapon = Instantiate(weaponPf,transform); 
    }

    // Update is called once per frame
    void Update()
    {
        float vertical = Input.GetAxisRaw("Vertical") ;
        float horizontal = Input.GetAxisRaw("Horizontal") ;

        rigid.linearVelocity = new Vector3(horizontal, vertical,0).normalized * speed ;
    }


}
