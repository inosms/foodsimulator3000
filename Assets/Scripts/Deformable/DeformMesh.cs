using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
public class DeformMesh : MonoBehaviour 
{
    //public parameters
    public Transform[] cagePoints = new Transform[4];//points of the cage that controls deformation
    
    //components
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private Material mat;

    //Variables for passing stuff to GPU
    private ComputeBuffer cagePos_buffer;
    private ComputeBuffer weights_buffer;
    
    private Vector3[] cagePos;
    private float[] weights;

	// Use this for initialization
	void Start () 
    {
        //get component references
        meshRenderer = GetComponent<MeshRenderer>();
        mat = meshRenderer.material;

        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;

        //create CPU arrays
        cagePos = new Vector3[cagePoints.Length];
        weights = new float[cagePoints.Length * mesh.vertices.Length];



        //create buffers
        cagePos_buffer = new ComputeBuffer(cagePos.Length, sizeof(float) * 3);
        weights_buffer = new ComputeBuffer(weights.Length, sizeof(float));

        //bind buffers
        mat.SetBuffer("cagePoints", cagePos_buffer);//bind cagePos_buffer to GPU variable "cagePoints"
        mat.SetBuffer("weights", weights_buffer);//bind weights_buffer to GPU variable "weights"


        //calculate original position of cage points in local space
        for (int i = 0; i < cagePoints.Length; i++)
        {
            cagePos[i] = transform.InverseTransformPoint(cagePoints[i].position);
        }
        cagePos_buffer.SetData(cagePos);//actually set data



        //precompute the mean value weights that will be used to deform the mesh
        calculateMeanValueWeights();

        weights_buffer.SetData(weights);//actually set data

	}

    void LateUpdate()
    {
        //update positions of cage points (relative to origin)
        for (int i = 0; i < cagePoints.Length; i++)
        {
            cagePos[i] = transform.InverseTransformPoint(cagePoints[i].position);
        }

        //create data
        cagePos_buffer.SetData(cagePos);//actually set data
    }

    void OnDestroy()
    {
        if (cagePos_buffer != null && weights_buffer != null)
        {
            //release GPU buffers
            cagePos_buffer.Release();
            weights_buffer.Release();
        }
    }

    private void calculateMeanValueWeights()//calculate the weights of each cage point for every vertex
    {
        Vector3[] vertices = mesh.vertices;
        float[] angles = new float[weights.Length];

        for(int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex_pos = vertices[i];

            //Step 1: calculate angles alpha for every triangle from current Vertex to all cage points
            for (int j = 0; j < cagePos.Length; j++)
            {
                Vector2 dir1 = new Vector2(cagePos[j].x - vertex_pos.x, cagePos[j].y - vertex_pos.y).normalized;
                Vector2 dir2 = new Vector2(cagePos[(j + 1) % cagePos.Length].x - vertex_pos.x, cagePos[(j + 1) % cagePos.Length].y - vertex_pos.y).normalized;

                float angle = Vector2.Angle(dir2, dir1);
                angles[i * cagePos.Length + j] = angle;
            }

            //Step 2: calculate weights
            for (int j = 0; j < cagePos.Length; j++)
            {
                float angle_j = angles[i * cagePos.Length + j];
                float angle_j_minus1 = angles[i * cagePos.Length + ((j - 1 + cagePos.Length) % cagePos.Length)];

                float distance = new Vector2(cagePos[j].x - vertex_pos.x, cagePos[j].y - vertex_pos.y).magnitude;

                float weight_j = (Mathf.Tan(Mathf.Deg2Rad * (angle_j / 2.0f)) + Mathf.Tan(Mathf.Deg2Rad * (angle_j_minus1 / 2.0f))) / distance;
                weights[i * cagePos.Length + j] = weight_j;
            }
            
            //Step 3: normalize weights
            float weight_sum = 0;
            for (int j = 0; j < cagePos.Length; j++)//sum up all weights for this vertex
            {
                weight_sum += weights[i * cagePos.Length + j];
            }

            for (int j = 0; j < cagePos.Length; j++)//divide every weight by sum
            {
                weights[i * cagePos.Length + j] /= weight_sum;
            }
        }
    }
}
