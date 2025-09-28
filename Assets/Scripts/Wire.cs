using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class Wire : MonoBehaviour
{
    
    [FormerlySerializedAs("StartPoint")] public Transform startObject;
    [FormerlySerializedAs("EndPoint")] public Transform endObject;
    public float sagFactor = 1f;
    public int numPoints = 10;
    public float lineWidth = 0.09f;
    public bool isActiveUpdate = false; 

    private LineRenderer lineRenderer;

    private Vector3 _start;
    private Vector3 _end;
    void Start()
    {
        _start = startObject.position;
        _end = endObject.position;
        _start += new Vector3() {y=0.12f};
        _end += new Vector3() {y=0.12f};
        
        
        lineRenderer = this.GetComponent<LineRenderer>();
        
        lineRenderer.receiveShadows = true;
        lineRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;
        lineRenderer.shadowBias = 0.3f;
        
        lineRenderer.widthMultiplier = lineWidth;
        lineRenderer.startColor = Color.black;
        lineRenderer.positionCount = numPoints;
    }

    private bool _protect = false; 
    void Update()
    {
        if (isActiveUpdate || _protect==false)
        {
           
            _protect = true;
            Vector3 direction = _end - _start;
            Vector3 perpendicularDirection = Vector3.Cross(direction, Vector3.forward).normalized;
            Vector3 sagOffset = perpendicularDirection * sagFactor;

            float segmentLength = direction.magnitude / (numPoints - 1);

            for (int i = 0; i < numPoints; i++)
            {
                Vector3 pointPosition = _start + direction.normalized * segmentLength * i;
                pointPosition += sagOffset * Mathf.Sin((float)i / (numPoints - 1) * Mathf.PI);

                lineRenderer.SetPosition(i, pointPosition);
            }
        }
       
    }
}