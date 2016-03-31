using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MassSpringSystem))]
[RequireComponent(typeof(Collider2D))]
public class MoveFalling : MonoBehaviour 
{
    public static float fallSpeed = 25.0f;//TODO: should be outsourced later
    public static float moveSpeed = 50.0f;

    //movement and physics
    private MassSpringSystem toMove;
    private List<Rigidbody2D> rigidBodies = new List<Rigidbody2D>();
    private Collider2D trigger;

	// Use this for initialization
	void Start () 
    {
        toMove = GetComponent<MassSpringSystem>();
        trigger = GetComponent<Collider2D>();
        rigidBodies.AddRange(toMove.getPointRigidbodies());

        foreach (Rigidbody2D r in rigidBodies)
        {
            r.gravityScale = 0;
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        //move mass spring system according to player input
        Vector2 dirInput = new Vector2(Input.GetAxis("Player 1 Horizontal Keyboard"), Input.GetAxis("Player 1 Vertical Keyboard"));

        Vector2 vel = new Vector2(dirInput.x * moveSpeed, -fallSpeed + dirInput.y * moveSpeed * 0.5f) * Time.deltaTime * 20.0f;

        foreach (Rigidbody2D r in rigidBodies)
        {
            r.velocity = vel;
        }
	}

    void OnTriggerEnter2D(Collider2D other)
    {
		// if the mass spring system collides with something
		// and this something is not itself and not the wall
		if (other.transform.parent != transform && !other.tag.Equals("Wall"))
		{

			//remove this script, giving control to physics simulation
			foreach (Rigidbody2D r in rigidBodies)
			{
				r.gravityScale = 1.0f;
			}

			Destroy(this);          
		}
    }
}
