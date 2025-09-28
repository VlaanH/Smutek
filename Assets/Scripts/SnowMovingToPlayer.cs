using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowMovingToPlayer : MonoBehaviour
{
    public Transform Player;
    public int Speed = 3;
    private Transform Snow;
    

    private void Start()
    {
        Snow = this.GetComponent<Transform>();
    }


    void Update()
    {
        var newSnowPosition = new Vector3() { x = Player.position.x, y = Snow.position.y, z = Player.position.z };
        
        Snow.position = Vector3.MoveTowards(Snow.position,newSnowPosition,Speed*Time.deltaTime);

    }
}
