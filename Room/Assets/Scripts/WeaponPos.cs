using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPos : MonoBehaviour
{
    public Transform weaponPos;
    public float distance = 1; 
    void Start()
    {
        weaponPos = Instantiate(weaponPos,transform);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,Input.mousePosition.z));
        Vector2 playerPos = transform.position;

        // 마우스가 플레이어와 겹치는 경우도 코딩해 나중에 !!
        weaponPos.transform.position = playerPos + (mousePos - playerPos).normalized * Mathf.Min(Vector2.Distance(mousePos,playerPos), distance);
        //Debug.Log(mousePos + "," + playerPos);
        // float angle = Mathf.Atan2(mousePos.y - playerPos.y, mousePos.x - playerPos.x) * Mathf.Rad2Deg;
        // weapon.transform.rotation = Quaternion.AngleAxis(angle-90+45, Vector3.forward);
        
    }
}
