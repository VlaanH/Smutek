using System;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public HingeJoint doorJoint;         // HingeJoint двери
    public float openAngle = 90f;        // угол открытой двери
    public float closedAngle = 0f;       // угол закрытой двери
    public float springForce = 20f;      // сила пружины
    public float springDamper = 2f;      // демпфер
    public float speed = 90f;            // скорость открывания (градусов в секунду)

    private bool isOpen = false;         // состояние двери
    private float targetAngle;           // куда стремится дверь

    void Start()
    {
        if (doorJoint == null)
            doorJoint = GetComponent<HingeJoint>();

        // Инициализируем пружину
        JointSpring spring = doorJoint.spring;
        spring.spring = springForce;
        spring.damper = springDamper;
        spring.targetPosition = closedAngle;
        doorJoint.spring = spring;
        doorJoint.useSpring = true;

        targetAngle = closedAngle;
    }

    private void Update()
    {
        // Плавное движение к целевому углу
        JointSpring spring = doorJoint.spring;
        spring.targetPosition = Mathf.MoveTowards(spring.targetPosition, targetAngle, speed * Time.deltaTime);
        doorJoint.spring = spring;
    }
    
    public bool IsOpen()
    {
        return isOpen;
    }

    public void ChangeDoorPosition()
    {
        if (Input.GetKeyDown(Settings.SelectedSettings.InteractionKeyCode))
        {
            isOpen = !isOpen;
            targetAngle = isOpen ? openAngle : closedAngle;
        }
        
    }

    public void ForcedlyChangeDoorPosition()
    {
        isOpen = !isOpen;
        targetAngle = isOpen ? openAngle : closedAngle;
    }


}