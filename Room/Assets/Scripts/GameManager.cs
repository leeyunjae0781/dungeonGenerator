using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform player ; public Transform monster ; 
    public Cam cam ;
    void Start()
    {
        player = Instantiate(player);
        cam.player = player;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q)){
            Transform monster_spawned = Instantiate(monster,Vector2.zero,Quaternion.identity);
            monster_spawned.GetComponent<Monster>().Setup(player);
            //monster_spawned.GetComponent<Monster>().target = player;
        }
    }
}
