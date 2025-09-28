using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class FanController : MonoBehaviour
{
    private Rigidbody fan;
    
    private float torqueStrength = 1f; // Сила вращения
    private float slowdownTime = 4f; // Время замедления
    private bool isStopping = true;
    private void Start()
    {
        fan = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isStopping==false)
        {
            fan.AddTorque(Vector3.forward * torqueStrength);
        }
        
    }

    public void OnFan()
    {
        isStopping = false;
    }


    public void HardStop()
    {
        isStopping = true;
        fan.velocity = Vector3.zero;
    }

    public IEnumerator SlowDownFan()
    {
        yield return new WaitForSeconds(new Random().Next(0,60) * 0.1f);
        isStopping = true;
        
        //fan.velocity = Vector3.zero;
        float elapsedTime = 0f;
        float currentTorque = torqueStrength;
        
        
        for (int i = 0; i < slowdownTime*10; i++)
        {
            fan.AddTorque(Vector3.forward * torqueStrength);
            yield return new WaitForSeconds(i*0.01f);
        }
        
         
    }
}
