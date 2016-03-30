using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//What flavours exist in the game?
public enum Flavour
{
    sweet,
    sour,
    spicy,
    salty,
    umami,
    bitter
}


[RequireComponent(typeof(Collider2D))]
public class Ingredient : MonoBehaviour 
{
    //references to other components
    private Collider2D trigger;

    //Flavour system
    public List<Flavour> flavour = new List<Flavour>(3);
    private List<Ingredient> connected = new List<Ingredient>();


	// Use this for initialization
	void Start () 
    {
        //get references to other components
        trigger = GetComponent<Collider2D>();

        generateRandomFlavours();
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        drawDebugFlavour();
	}


    //flavour methods
    public static Color typeColor(Flavour f)//associates flavour with colour
    {
        Color result = Color.black;


        switch (f)
        {
            case Flavour.sweet:
                {
                    result = new Color(1.0f, 204.0f / 255.0f, 1.0f);
                    break;
                }
            case Flavour.sour:
                {
                    result = new Color(1.0f, 1.0f, 0.0f);
                    break;
                }
            case Flavour.spicy:
                {
                    result = new Color(1.0f, 0.0f, 0.0f);
                    break;
                }
            case Flavour.salty:
                {
                    result = new Color(1.0f, 1.0f, 1.0f);
                    break;
                }
            case Flavour.umami:
                {
                    result = new Color(145.0f / 255.0f, 47.0f / 255.0f, 4.0f / 255.0f);
                    break;
                }
            case Flavour.bitter:
                {
                    result = new Color(25.0f / 255.0f, 38.0f / 255.0f, 212.0f / 255.0f);
                    break;
                }
            default:
                {
                    break;
                }
        }

        return result;
    }


    //Debug / Test methods
    private void drawDebugFlavour()
    {
        Vector3 a, b, c;//points of triangle

        a = transform.position + new Vector3(0, transform.localScale.y*0.5f, -5);
        b = transform.position + new Vector3(-transform.localScale.x*0.5f, -transform.localScale.y * 0.25f, -5);
        c = transform.position + new Vector3(transform.localScale.x*0.5f, -transform.localScale.y * 0.25f, -5);

        if (flavour.Count == 3)
        {
            Debug.DrawLine(a, b, typeColor(flavour[0]));
            Debug.DrawLine(b, c, typeColor(flavour[1]));
            Debug.DrawLine(c, a, typeColor(flavour[2]));
        }
    }

    private void generateRandomFlavours()
    {
        //choose three different flavours for the ingredient randomly
        Array possibleTastes = Enum.GetValues(typeof(Flavour));

        while (flavour.Count < 3)
        {
            Flavour randomTaste = (Flavour)UnityEngine.Random.Range(0, possibleTastes.Length);

            if (!flavour.Contains(randomTaste)) { flavour.Add(randomTaste); }
        }
    }
}
