using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeController : MonoBehaviour
{
    public CapsuleCollider2D col;
    public Transform head;
    public LineRenderer lineRender;
    public Vector3 growRate;
    private List<RaycastHit2D> results;
    public ContactFilter2D filter;
    void Start()
    {
        results = new List<RaycastHit2D>();
    }

    // Update is called once per frame
    void Update()
    {
        lineRender.SetPosition(0, head.transform.position);
        lineRender.SetPosition(1, transform.position);
        if (Physics2D.Raycast(head.transform.position, transform.up, filter, results, 0.5f) == 0)
        {
            col.transform.localPosition = head.localPosition * 0.5f;
            head.localPosition += growRate * Time.deltaTime;
            col.size += (Vector2)growRate * Time.deltaTime;
        }
    }
}
