using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class DragRigidbody : MonoBehaviour 
{
    //components
    private Rigidbody2D rigidBody;

    //drag & drop
    private bool beingDragged = false;
    private float radius = 5.0f;
    private Vector2 dragOffset;
    public float followSpeed = 40.0f;

	// Use this for initialization
	void Start () 
    {
        rigidBody = GetComponent<Rigidbody2D>();
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //mouse has been released
        if (Input.GetButtonUp("Player 1 Click"))
        {
            beingDragged = false;
            rigidBody.isKinematic = false;
            return;
        }

        //if clicking on inside the radius
        if (Input.GetButtonDown("Player 1 Click") &&
            Vector2.Distance(rigidBody.position, mousePos) < radius * transform.localScale.x)
        {
            beingDragged = true;
            rigidBody.isKinematic = true;
            dragOffset = mousePos - rigidBody.position;
        }

        //actual dragging
        if (beingDragged)
        {
            rigidBody.MovePosition(Vector2.Lerp(rigidBody.position, mousePos - dragOffset, Time.deltaTime * followSpeed));
        }
	
	}
}
