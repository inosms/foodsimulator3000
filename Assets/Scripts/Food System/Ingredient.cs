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

    //chain/connection system
    public List<Ingredient> connected = new List<Ingredient>(3);
    private List<int> associatedSlot = new List<int>(3);//slot index (in flavour) of connection i

    public List<bool> slotConnected = new List<bool>(3);//is flavour i connected?
    
    private List<float> connectDistances = new List<float>(3);
    public float distanceTolerance = 1.0f;
    
    
    


	// Use this for initialization
	void Start () 
    {
        //get references to other components
        trigger = GetComponent<Collider2D>();

        generateRandomFlavours(3);
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        drawDebugFlavour();
        drawDebugConnections();

        disconnectFarAway();//disconnects Ingredients that were previously connected but are too far away now
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        
        Ingredient ingr = other.GetComponentInParent<Ingredient>();

        
        if (ingr != null && ingr != this && !connected.Contains(ingr))//a new, different ingredient could connect to this one
        {
            
            //check if they can connect over a shared flavour
            for(int i = 0; i < flavour.Count; i++)
            {
                if (!slotConnected[i])//is the flavour unconnected?
                {
                    int flavourB_index = ingr.flavour.FindIndex(flavourB => { return flavourB == flavour[i]; });//find a flavour to connect to

                    //if the flavour has been found and can be connected to
                    if (flavourB_index != -1 && !ingr.slotConnected[flavourB_index])
                    {
                        //connect other
                        ingr.connect(this, flavour[i]);

                        //connect self
                        connect(ingr, flavour[i]);
                    }
                }


            }
        }
    }

    //Connection methods
    void disconnectFarAway()
    {
        for (int i = 0; i < connected.Count; )
        {
            float dist = Vector3.Distance(connected[i].transform.position, transform.position);

            //if ingredient too far away, disconnect from chain
            if (dist > connectDistances[i] + distanceTolerance)
            {
                //disconnect other
                connected[i].disconnect(this);

                //disconnect self
                disconnect(connected[i]);
            }
            else
            {
                i++;
            }
        }
    }

    public void connect(Ingredient i, Flavour sharedFlavour)
    {
        connected.Add(i);
        connectDistances.Add(Vector3.Distance(transform.position, i.transform.position));

        //close slot
        int slot = flavour.FindIndex(f => { return f == sharedFlavour; });
        associatedSlot.Add(slot);
        slotConnected[slot] = true;
    }

    public void disconnect(Ingredient i)
    {
        //find ingredient and remove associated data
        int index = connected.FindIndex(ingr => { return i == ingr; });
        connected.RemoveAt(index);
        connectDistances.RemoveAt(index);

        //open up slot
        slotConnected[associatedSlot[index]] = false;
        associatedSlot.RemoveAt(index);
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

        if (flavour.Count > 0) { Debug.DrawLine(a, b, typeColor(flavour[0])); }
        if (flavour.Count > 1) { Debug.DrawLine(b, c, typeColor(flavour[1])); }
        if (flavour.Count > 2) { Debug.DrawLine(c, a, typeColor(flavour[2])); }
    }

    private void drawDebugConnections()
    {
        foreach (Ingredient i in connected)
        {
            Debug.DrawLine(transform.position + new Vector3(0, 0, -5), i.transform.position + new Vector3(0, 0, -5), Color.green);
        }
    }

    private void generateRandomFlavours(int num)
    {
        //choose three different flavours for the ingredient randomly
        Array possibleTastes = Enum.GetValues(typeof(Flavour));

        if (possibleTastes.Length < num) { num = possibleTastes.Length; }//ensure there is no endless while loop

        while (flavour.Count < num)
        {
            Flavour randomTaste = (Flavour)UnityEngine.Random.Range(0, possibleTastes.Length);

            if (!flavour.Contains(randomTaste)) { flavour.Add(randomTaste); slotConnected.Add(false); }
        }
    }
}
