using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeController : MonoBehaviour
{
    public CapsuleCollider2D col;
    public Transform head;
    public LineRenderer lineRender;
    public Vector3 growRate;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        col.transform.localPosition = head.localPosition * 0.5f;
        head.localPosition += growRate * Time.deltaTime;
        col.size += (Vector2)growRate * Time.deltaTime;
        lineRender.SetPosition(0, head.transform.position);
        lineRender.SetPosition(1, transform.position);
    }
}
