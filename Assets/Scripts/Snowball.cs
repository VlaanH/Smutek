using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snowball : MonoBehaviour
{
    public FirstPersonController _firstPersonController;
  

    void TeleportToStandardSnowballPosition()
    {
        Vector3 tpPosition = new Vector3(287,2.5f,-91);
        Quaternion playerRotation = new Quaternion(0,0,0,0);
        

        var transformPlayer = _firstPersonController.transform;
        
        if (_firstPersonController.transform.position.z<-100)
        {
            transformPlayer.position = tpPosition;
            transformPlayer.rotation = playerRotation;
        }

        if (_firstPersonController.transform.position.z>650)
        {
           
            transformPlayer.position = tpPosition;
            transformPlayer.rotation = playerRotation;
        }

        if (_firstPersonController.transform.position.x<-120)
        {
            transformPlayer.position = tpPosition;
            transformPlayer.rotation = playerRotation;
        }
        
        if (_firstPersonController.transform.position.x>550)
        {
            transformPlayer.position = tpPosition;
            transformPlayer.rotation = playerRotation;
        }
    }

    void Update()
    {
       // Debug.Log("X="+_firstPersonController.transform.position.x+" | Z=" + _firstPersonController.transform.position.z + " | Y="+_firstPersonController.transform.position.y);

        TeleportToStandardSnowballPosition();
    }
}
