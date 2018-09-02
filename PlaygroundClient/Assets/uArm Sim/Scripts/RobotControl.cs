using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class RobotControl : MonoBehaviour
{
    // Script Inputs
    public float MaxSpring = 100f;
    public int speed = 1;
    
    // Robot Parts and State
    private HingeJoint _rotHinge;
    private HingeJoint _strHinge;
    private HingeJoint _hgtHinge;
    private HingeJoint _endHinge;
    private GameObject _sucker;

    // Pick and Place Variables
    private bool _pumpStatus;
    private GameObject _touchedObject = null;
    private FixedJoint _pumpJoint = null;

    // Keep track of the starting angles of each servo, so as to be able to reset
    private float _rotStartAngle;
    private float _strStartAngle;
    private float _hgtStartAngle;
    private float _endStartAngle;
        
    // Monobehaviour Functions
    private void Awake()
    {
        // Get any GameObjects that are needed (or whose components are needed)
        GameObject baseTop        = transform.Find("Base Top").gameObject;
        GameObject linkageBottom  = transform.Find("Linkage Bottom").gameObject;
        GameObject linkageBottom1 = transform.Find("Linkage Bottom 1").gameObject;
        GameObject effector       = transform.Find("Effector End").gameObject;
        _sucker                    = transform.Find("Effector End/Effector Sucker").gameObject;

        // Find the "servo" joints
        _rotHinge = baseTop.GetComponent<HingeJoint>();
        _strHinge = linkageBottom.GetComponent<HingeJoint>();
        _hgtHinge = linkageBottom1.GetComponent<HingeJoint>();
        _endHinge = effector.GetComponent<HingeJoint>();

        _rotStartAngle = _rotHinge.angle;
        _strStartAngle = _strHinge.angle;
        _hgtStartAngle = _hgtHinge.angle;
        _endStartAngle = _endHinge.angle;
        
        AttachServos();
    }
    
    private void Update()
    {
        // Grab objects if the pump is currently on. Let go of objects if it's not
        // GrabUpdate();

    }
    
    public void ProcessCommand(int command)
    {

        Debug.Log("Action: " + command);
        // Perform the command
        if (command == 0)
            SetServoAngle(_rotHinge, _rotHinge.angle + speed);
        if (command == 1)
            SetServoAngle(_rotHinge, _rotHinge.angle - speed);
        
        if (command == 2)
            SetServoAngle(_strHinge, _strHinge.angle + speed);
        if (command == 3)
            SetServoAngle(_strHinge, _strHinge.angle - speed);
        
        if (command == 4)
            SetServoAngle(_hgtHinge, _hgtHinge.angle + speed);
        if (command == 5)
            SetServoAngle(_hgtHinge, _hgtHinge.angle - speed);
        
        if (command > 5 || command < 0)
            Debug.Log("Unknown command sent to Robot! Command: " + command);
    }


    public void GrabUpdate()
    {
        // If the pump is on and touching an object, but no Joint has been attached yet
        if(_pumpStatus && _touchedObject != null && _pumpJoint == null){
            // Attach the object to the pump
            Debug.Log("Attaching " + _touchedObject);
            _pumpJoint                     = _touchedObject.AddComponent<FixedJoint>();
            _pumpJoint.connectedBody       = _sucker.GetComponent<Rigidbody>();
            _pumpJoint.breakForce          = 400;
            _pumpJoint.breakTorque         = 100;
            _pumpJoint.enableCollision     = true;
            _pumpJoint.enablePreprocessing = false;
        }


        // If the pump is off and there is still an object attached
        if (!_pumpStatus && _touchedObject != null && _pumpJoint != null)
        {
            // Detach the object from the pump
            Debug.Log("Detaching " + _touchedObject);

            Destroy(_pumpJoint);
            _pumpJoint = null;
        }

            //// If the pump is on and a there is an object currently being touched
            //if(pumpStatus && touchedObject != null)
            //{
            //    Vector3 direction = (touchedPoint.point - sucker.transform.position).normalized;
            //    touchedObject.GetComponent<Rigidbody>().AddForceAtPosition(direction * 10, sucker.transform.position);

            //    Debug.Log("normalized" + direction + "    touched" + touchedPoint.point + "    sucker" + sucker.transform.position);
            //    // Vector3 offset = new Vector3(0, 0, 1);
            //}
        }

    
    
    // Robot or Controller Events
    public void OnEffectorCollisionEnter(Collision col)
    {
         
        GameObject colObj = col.gameObject;

        if(colObj.GetComponent<Rigidbody>() != null) {
            _touchedObject = col.gameObject;
        }
    }

    public void OnEffectorCollisionExit(Collision col)
    {
        _touchedObject = null;
        if(_pumpJoint != null)
        {
            Debug.Log("Detaching " + _touchedObject + " because collision exit");
            Destroy(_pumpJoint);
        }

    }


    // Control Robot
    public void ResetState()
    {
        AttachServos();
        SetServoAngle(_rotHinge, _rotStartAngle);
        SetServoAngle(_strHinge, _strStartAngle);
        SetServoAngle(_hgtHinge, _hgtStartAngle);
        SetServoAngle(_endHinge, _endStartAngle);
    }

    public void AttachServos()
    {
        _rotHinge.useSpring = true;
        _strHinge.useSpring = true;
        _hgtHinge.useSpring = true;
        _endHinge.useSpring = true;

        SetServoAngle(_rotHinge, _rotHinge.angle);
        SetServoAngle(_strHinge, _strHinge.angle);
        SetServoAngle(_hgtHinge, _hgtHinge.angle);
        SetServoAngle(_endHinge, _endHinge.angle);
    }

    public void DetachServos()
    {
        _rotHinge.useSpring = false;
        _strHinge.useSpring = false;
        _hgtHinge.useSpring = false;
        _endHinge.useSpring = false;
    }

    private void SetServoAngle(HingeJoint servo, float angle)
    {
        JointSpring hingeSpring = servo.spring;
        hingeSpring.spring = MaxSpring;
        hingeSpring.damper = 3;
        hingeSpring.targetPosition = angle;
        servo.spring = hingeSpring;
        servo.useSpring = true;
    }
    
    public void SetPump(bool status)
    {
        _pumpStatus = status;         
    }
}
