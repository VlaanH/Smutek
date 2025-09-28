using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Object = System.Object;

public static class MovingObjectsStaticData
{
    public static DateTime TakeTime { get; set; } 
    public static bool GlobalTaked { get; set; }
}

public class MovingObjects : MonoBehaviour
{
    public bool useControlRotation = false;
    public bool useAForce = false;
    public int delay = 1;
    [FormerlySerializedAs("ps")] public Transform UserBox;


    private int upAndDownValue = 0;
    private Rigidbody rb;
    private float distanceDrop = 1.5f;
    private int movementMultiplier = 3;
    private bool taked = false;

    private void Start()
    {
        MovingObjectsStaticData.TakeTime = DateTime.Now;
        rb = GetComponent<Rigidbody>();
        
    }

    public bool ObjectIsTaken()
    {
        return taked;
    }

    public void ThrowAway()
    {
      
        MovingObjectsStaticData.TakeTime = DateTime.Now.AddSeconds(delay);
        MovingObjectsStaticData.GlobalTaked = false;
        
        taked = false;
        
        rb.drag = 0;
        rb.velocity = Vector3.zero;

        upAndDownValue = 0;
        
        if (useAForce==false)
        {
            rb.isKinematic = false;
        }
      
    }

    private void ForceTack ()
    {
        if (taked == true)
        {
            var distanceBetweenObjects = UserBox.position - rb.position;

            Rigidbody rigidbodyObj = rb.GetComponent<Rigidbody>();
            
            if (distanceBetweenObjects.x >= 0)
            {
                rigidbodyObj.AddForce(0.1f*movementMultiplier,0,0, ForceMode.Impulse);
            }
            if (distanceBetweenObjects.y >= 0)
            {
                rigidbodyObj.AddForce(0,0.1f*3,0, ForceMode.Impulse);
            }
            if (distanceBetweenObjects.z >= 0)
            {
                rigidbodyObj.AddForce(0,0,0.1f*movementMultiplier, ForceMode.Impulse);
            }
            
            
            if (distanceBetweenObjects.x <= 0)
            {
                rigidbodyObj.AddForce(-0.1f * movementMultiplier,0,0, ForceMode.Impulse);
            }
            if (distanceBetweenObjects.y <= 0)
            {
                rigidbodyObj.AddForce(0,-0.1f * 3,0, ForceMode.Impulse);
            }
            if (distanceBetweenObjects.z <= 0)
            {
                rigidbodyObj.AddForce(0,0,-0.1f * movementMultiplier, ForceMode.Impulse);
            }
            
            rb.MoveRotation(UserBox.rotation);
            
           
            
            if (distanceBetweenObjects.z>distanceDrop | distanceBetweenObjects.z<-distanceDrop |
                distanceBetweenObjects.x>distanceDrop | distanceBetweenObjects.x<-distanceDrop)
            {
                ThrowAway();
            }
        
           
        }
    }

    private void Tack()
    {
        if (taked == true)
        {
            rb.MovePosition(UserBox.position);
            
            
            rb.MoveRotation(UserBox.rotation);
        }

        
        
    }
    private void FixedUpdate()
    {

        if (useAForce)
        {
            ForceTack();
        }
        else
        {
            Tack(); 
        }
    

        

    }



    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.name!="FirstPersonController")
        {
            ThrowAway();
        }
        
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            ThrowAway();
        }
    }


    public void TakeObject()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if ( MovingObjectsStaticData.GlobalTaked == false && taked == false &&  MovingObjectsStaticData.TakeTime < DateTime.Now)
            {
                MovingObjectsStaticData.TakeTime = DateTime.Now.AddSeconds(delay);
                MovingObjectsStaticData.GlobalTaked = true;
                taked = true;

                if (useControlRotation)
                {
                    UserBox.rotation = rb.rotation;
                }
                else
                {
                    gameObject.transform.rotation = default;
                }
                
                rb.position = UserBox.position;

                var normalPositin = UserBox.position;
              
                
                
                rb.MovePosition(normalPositin);
                
                rb.drag = 5;

                if (useAForce==false)
                {
                    rb.isKinematic = true;
                }
            }
          

        }
    }
}
