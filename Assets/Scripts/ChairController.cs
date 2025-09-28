using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ChairController : MonoBehaviour
{
    public static ChairController sittingOn;



    public bool isSit = false;
    
        
    public FirstPersonController PersonController;
    public GetUpTypePosition getUpTypePosition;
    public Transform GetSittingTargetPosition;
    public HintsBox HintsBox;
    
    public int SizeHeight = 2;

    private int forceMult = 90;
    
    private Rigidbody _rigidbody;
    private bool _isGetSittingTargetPositionNotNull;
    private bool _isGetSittingTargetPositionNull;

    private void Start()
    {
        _isGetSittingTargetPositionNull = GetSittingTargetPosition==null;
        _isGetSittingTargetPositionNotNull = GetSittingTargetPosition!=null;
        
        _rigidbody = this.GetComponent<Rigidbody>();
    }

    public enum GetUpTypePosition
    {
        X, Z,
        MinX,MinZ
    }

    private void AddForceToSwing()
    {
        if (Input.GetKeyDown(Settings.SelectedSettings.BeforeKeyCode))
        {
            _rigidbody.AddForce(forceMult*5,forceMult*5,0, ForceMode.Force);
        }
        
    }

    private void Update()
    {

        if (PersonController.IsFreezing==false)
        {
            if (Input.GetKeyDown(Settings.SelectedSettings.ExitFromInteraction))
            {
                var chairPosition = GetChairPosition();
                GetUp();
            }

            if (isSit)
            {
                if (_isGetSittingTargetPositionNotNull)
                {
                    AddForceToSwing();
                    PersonController.gameObject.transform.position = GetSittingTargetPosition.position;
                }

            
                DistanceGetUp();
            } 
        }
       
        
    }
    
    public static ChairController GetChairAtPosition(Vector3 position)
    {

        var allChair = FindObjectsOfType<ChairController>().ToList();

        foreach (var chair in allChair)
        {
            
            chair._isGetSittingTargetPositionNull = chair.GetSittingTargetPosition==null;
            chair._isGetSittingTargetPositionNotNull =  chair.GetSittingTargetPosition!=null;
            
            if (chair.GetChairPosition()==position)
            {
                
                
                return chair;
            }
        }

        return GameObject.Find("chochelyCharObj").GetComponent<ChairController>();
    }

    public Vector3 GetChairPosition()
    {
        if (_isGetSittingTargetPositionNull)
        {
            return this.transform.position; 
        }
        else
        {
            return GetSittingTargetPosition.transform.position;
        }
      
    }

    public void DistanceGetUp()
    {
        var chairPosition = GetChairPosition();
        var distanceBetweenObjects = chairPosition - PersonController.transform.position;
        
        if (PersonController.isSit == true)
        {
            if (  distanceBetweenObjects.x>1 || distanceBetweenObjects.x<-1 || distanceBetweenObjects.z>1 || distanceBetweenObjects.z<-1 ||
                  distanceBetweenObjects.y>2.3 || distanceBetweenObjects.y<-2.3)
            {
                GetUp();
            }
           
        }
        
    }

    
    
    public void GetUp()
    {
        var chairPosition = GetChairPosition();
        
        if (PersonController.isSit == true && isSit == true)
        {
            if (getUpTypePosition == GetUpTypePosition.X)
            {
                PersonController.gameObject.transform.position = chairPosition += new Vector3(2,1,0);
            }
            else if(getUpTypePosition == GetUpTypePosition.Z)
            {
                PersonController.gameObject.transform.position = chairPosition += new Vector3(0,1,2);
            }
            else if(getUpTypePosition == GetUpTypePosition.MinX)
            {
                PersonController.gameObject.transform.position = chairPosition += new Vector3(-2,1,0);
            }
            else if(getUpTypePosition == GetUpTypePosition.MinZ)
            {
                PersonController.gameObject.transform.position = chairPosition += new Vector3(0,1,-2);
            }
            
            PersonController.Sit();
            
            isSit = false;
           
        }
        
    }


    public void Sit()
    {
        if (PersonController.isSit == false && isSit == false)
        {
            ChairController.sittingOn = this;
                
            isSit = true;
            var chairPosition = GetChairPosition();

            PersonController.Sit();

                
            if (_isGetSittingTargetPositionNotNull)
            {
                HintsBox.SetHintData(0);
            }
            else
            {
                HintsBox.SetHintData(1); 
            }

            PersonController.gameObject.transform.position = chairPosition += new Vector3(0,SizeHeight,0);
        }
    }

    public void SitOnChairForKey()
    {
        if (Input.GetKeyDown(Settings.SelectedSettings.InteractionKeyCode))
        {
            Sit();

        }
    }

}
