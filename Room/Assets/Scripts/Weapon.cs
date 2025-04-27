using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    Collider2D colli;
    public float swing_casting = 0.2f ;
    public float swing_casting_speed = 1;
    float timer = 1f;

    float angle ;
    // Start is called before the first frame update
    void Start()
    {
        colli = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {



        if(Input.GetMouseButtonDown(0) && timer > swing_casting) {
            //-22.5~22.5 방향의 휘두르기
            timer = 0;  
        }

        if ((timer < swing_casting))
        {
            this.tag = "Weapon";
            colli.enabled = true;
            Swing();
            
        }
    }   

    void FixedUpdate(){
        timer += Time.deltaTime;
    }

    void LateUpdate(){
        this.transform.position = GetComponentInParent<WeaponPos>().weaponPos.transform.position;

        if (timer > swing_casting)
        {
            this.tag = "Untagged";
            colli.enabled = false;
            Lookmouse();
        }
        
        
    }

    void Lookmouse(){
        
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,Input.mousePosition.z));
        Vector2 playerPos = transform.parent.position;
        

        angle = Mathf.Atan2(mousePos.y - playerPos.y, mousePos.x - playerPos.x) * Mathf.Rad2Deg;
        
        // +45는 기본적으로 곡괭이 스프라이트가 기울어서 보정한 것
        transform.rotation = Quaternion.AngleAxis(angle-90, Vector3.forward);
        
    }

    void Swing(){
        

        transform.rotation = Quaternion.AngleAxis(Mathf.Lerp(angle+60-90,angle-60-90,timer*swing_casting_speed), Vector3.forward);

        //(Mathf.Lerp(angle-45,angle,Time.deltaTime))


        Debug.Log("공격");
    }
}
