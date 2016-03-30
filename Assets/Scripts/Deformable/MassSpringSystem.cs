using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MassSpringSystem : MonoBehaviour 
{
    //parameters
    public Transform[] MassPoints;
    public float frequency = 1;
    public float dampingRatio;
    public float mass = 10;
    public float deformationThreshold = 5.0f;

    //spring-related
    private EdgeCollider2D[] colliders;
    private List<SpringJoint2D> springs = new List<SpringJoint2D>();
    private List<Rigidbody2D> pointRigidBodies = new List<Rigidbody2D>();
    private float[] original_distances;

    //components
    //private PolygonCollider2D polygonCollider;

    //mesh related
    //private int[] indexBuffer;
    //Vector2[] uv;


	// Use this for initialization
	void Start ()
    {
        //polygonCollider = GetComponent<PolygonCollider2D>();
        colliders = new EdgeCollider2D[MassPoints.Length];
        original_distances = new float[MassPoints.Length];


        //Set up Springs:
        for (int i = 0; i < MassPoints.Length; i++)
        {
            //add an edge collider to every mass point
            colliders[i] = MassPoints[i].gameObject.AddComponent<EdgeCollider2D>();

            //and store its distance from the centre
            original_distances[i] = MassPoints[i].localPosition.magnitude;

            //add spring joints between all pairs of different points
            for (int j = 0; j < MassPoints.Length; j++)
            {
                if (j != i)
                {
                    SpringJoint2D spring = MassPoints[i].gameObject.AddComponent<SpringJoint2D>();

                    spring.connectedBody = MassPoints[j].GetComponent<Rigidbody2D>();//connect with the other rigidbody
                    Vector2 originalDistVector = MassPoints[i].position - MassPoints[j].position;
                    spring.distance = originalDistVector.magnitude;

                    springs.Add(spring);
                }
            }
        }

        //Store references to all rigidbodies of masspoints
        for (int i = 0; i < MassPoints.Length; i++)
        {
            pointRigidBodies.Add(MassPoints[i].GetComponent<Rigidbody2D>());
        }

        //disable collisions with MassPoints from the same system
        for (int i = 0; i < MassPoints.Length; i++)
        {
            //Physics2D.IgnoreCollision(colliders[i], this.polygonCollider);//ignore Collisions between child mass points and parent polygon collider
            //Physics2D.IgnoreCollision(colliders[i].gameObject.GetComponent<CircleCollider2D>(), this.polygonCollider);

            for (int j = MassPoints.Length - 1; j > i; j--)
            {
                Physics2D.IgnoreCollision(colliders[i], colliders[j]);
                Physics2D.IgnoreCollision(colliders[i], colliders[j].gameObject.GetComponent<CircleCollider2D>());
                Physics2D.IgnoreCollision(colliders[j], colliders[i].gameObject.GetComponent<CircleCollider2D>());
                Physics2D.IgnoreCollision(colliders[i].gameObject.GetComponent<CircleCollider2D>(), colliders[j].gameObject.GetComponent<CircleCollider2D>());
            }
        }

        //Initialize parameters
        updateSpringParameters();
        updateRigidBodyParameters();

    }
	
	// Update is called once per frame
	void Update () 
    {
        //updateSpringParameters();
        //updateRigidBodyParameters();
        adjustCollider();
        moveToMidpoint();

        checkDeformation();
	}


    private void adjustCollider()
    {
        if (MassPoints.Length <= 0) return;

        //for all edge colliders
        for(int i = 0; i < colliders.Length; i++)
        {
            Vector2[] edgePoints = new Vector2[3];

            Vector2 ownPos = MassPoints[i].position;
            Vector2 previousPos = MassPoints[((i - 1) + MassPoints.Length) % MassPoints.Length].position;
            Vector2 nextPos = MassPoints[((i + 1) + MassPoints.Length) % MassPoints.Length].position;

            //edgePoints are in local space --> transform
            edgePoints[0] = MassPoints[i].InverseTransformPoint(ownPos + 0.5f * (previousPos - ownPos));
            edgePoints[1] = MassPoints[i].InverseTransformPoint(ownPos);
            edgePoints[2] = MassPoints[i].InverseTransformPoint(ownPos + 0.5f * (nextPos - ownPos));


            colliders[i].points = edgePoints;

        }

        
        /*
        //adjust polygon collider according to points
        Vector2[] pointPositions = new Vector2[MassPoints.Length];

        for (int i = 0; i < MassPoints.Length; i++)
        {
            pointPositions[i] = MassPoints[i].localPosition;
        }

        polygonCollider.points = pointPositions;*/


    }

    private void moveToMidpoint()
    {
        Vector3 avgPos = Vector3.zero;

        for (int i = 0; i < MassPoints.Length; i++)
        {
            avgPos += MassPoints[i].position;
        }

        avgPos /= MassPoints.Length;

        Vector3 displacement = avgPos - transform.position;//calculate how the parent is being moved

        //move all children in opposite direction
        for (int i = 0; i < MassPoints.Length; i++)
        {
            MassPoints[i].position = MassPoints[i].position - displacement;
        }

        transform.position = avgPos;

    }

    private void updateSpringParameters()
    {
        for (int i = 0; i < springs.Count; i++)
        {
            springs[i].frequency = frequency;
            springs[i].dampingRatio = dampingRatio;
 
        }
    }

    private void updateRigidBodyParameters()
    {
        for (int i = 0; i < pointRigidBodies.Count; i++)
        {
            pointRigidBodies[i].mass = mass / MassPoints.Length;
        }
    }

    private void checkDeformation()
    {
        for (int i = 0; i < MassPoints.Length; i++)
        {
            if (MassPoints[i].localPosition.magnitude > deformationThreshold * original_distances[i])
            {
                //destroy mass spring system if deformation too large

                /*
                foreach(Transform child in transform)
                {
                    Destroy(child.gameObject);
                }*/
                
                Destroy(this.gameObject);
            }
        }
 
    }


    public void addForce(Vector2 force)
    {
        for (int i = 0; i < pointRigidBodies.Count; i++)
        {
            pointRigidBodies[i].AddForce(force);
        }

    }

    public Vector2 getOverallVelocity()
    {
        Vector2 result = Vector2.zero;
        for (int i = 0; i < pointRigidBodies.Count; i++)
        {
            result += pointRigidBodies[i].velocity;
        }

        result /= pointRigidBodies.Count;

        return result;
    }
}
