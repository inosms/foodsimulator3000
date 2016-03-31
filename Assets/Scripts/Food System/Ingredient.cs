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
[RequireComponent(typeof(MassSpringSystem))]
public class Ingredient : MonoBehaviour 
{
    //references to other components
    private Collider2D trigger;
    private MassSpringSystem massSpring;

    //Flavour system
    public List<Flavour> flavour = new List<Flavour>(3);

    //chain/connection system
    public List<Ingredient> connected = new List<Ingredient>(3);
    private List<int> sharedFlavour = new List<int>(3);//flavour (index) over which ingredient i is connected

    public List<bool> flavourConnected = new List<bool>(3);//is flavour i connected?
    
    private List<float> connectDistances = new List<float>(3);
    public float distanceTolerance = 1.0f;

    public static float stillVelocity = 1.0f;


	//Monobehaviour (Events)
	void Start () 
    {
        //get references to other components
        trigger = GetComponent<Collider2D>();
        massSpring = GetComponent<MassSpringSystem>();

        generateRandomFlavours(UnityEngine.Random.Range(1, 4));
	
	}
	
	void Update () 
    {
        drawDebugFlavour();
        drawDebugConnections();

        disconnectFarAway();//disconnects Ingredients that were previously connected but are too far away now

        //check whether chain should be destroyed
        if (flavourConnected.TrueForAll(x => { return x == true; }))
        {
            StartCoroutine("destroyChain_when_still");
        }
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        
        Ingredient ingr = other.GetComponentInParent<Ingredient>();

        
        if (ingr != null && ingr != this && !connected.Contains(ingr))//a new, different ingredient could connect to this one
        {
            
            //check if they can connect over a shared flavour
            for(int i = 0; i < flavour.Count; i++)
            {
                if (!flavourConnected[i])//is the flavour unconnected?
                {
                    int flavourB_index = ingr.flavour.FindIndex(flavourB => { return flavourB == flavour[i]; });//find a flavour to connect to

                    //if the flavour has been found and can be connected to
                    if (flavourB_index != -1 && !ingr.flavourConnected[flavourB_index])
                    {
                        //connect other
                        ingr.connect(this, flavour[i]);

                        //connect self
                        connect(ingr, flavour[i]);

                        return;//the ingredients have been connected
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

    public void connect(Ingredient i, Flavour f)
    {
        connected.Add(i);
        connectDistances.Add(Vector3.Distance(transform.position, i.transform.position));

        //close slot
        int slot = flavour.FindIndex(f2 => { return f2 == f; });
        sharedFlavour.Add(slot);
        flavourConnected[slot] = true;
    }

    public void disconnect(Ingredient i)
    {
        //find ingredient and remove associated data
        int index = connected.FindIndex(ingr => { return i == ingr; });
        connected.RemoveAt(index);
        connectDistances.RemoveAt(index);

        //open up slot
        flavourConnected[sharedFlavour[index]] = false;
        sharedFlavour.RemoveAt(index);
    }

    public void getChain(ref List<Ingredient> chain)
    {
        if (chain.Contains(this)) { return; }

        chain.Add(this);

        foreach (Ingredient i in connected)
        {
            i.getChain(ref chain);
        }
 
    }

    private void destroyChain()
    {
        //get full chain
        List<Ingredient> chain = new List<Ingredient>();
        getChain(ref chain);

        //destroy every ingredient object in it
        foreach(Ingredient i in chain)
        {
            Destroy(i.gameObject);
        }

    }

    private IEnumerator destroyChainDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        destroyChain();
    }

    private IEnumerator destroyChain_when_still()
    {
        //get full chain
        List<Ingredient> chain = new List<Ingredient>();
        getChain(ref chain);

        //wait 'til chain does not move (much)
        bool chainStill = false;

        while (!chainStill)
        {
            yield return new WaitForSeconds(0.5f);
            chainStill = chain.TrueForAll(i => { return (i.massSpring.getOverallVelocity().magnitude < stillVelocity); });
        }

        //destroy every ingredient object in it
        foreach (Ingredient i in chain)
        {
            Destroy(i.gameObject);
        }
    }

    //flavour methods
    public static Color typeColor(Flavour f)//associates flavour with colour
    {
        Color result = Color.black;


        switch (f)
        {
            case Flavour.sweet:
                {
                    result = new Color(0.0f, 255.0f, 0.0f);
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
        for (int i = 0; i < connected.Count; i++ )
        {
            Color drawCol = typeColor(flavour[sharedFlavour[i]]);//choose the colour according to the shared flavour of the connection
            Debug.DrawLine(transform.position + new Vector3(0, 0, -5), connected[i].transform.position + new Vector3(0, 0, -5), drawCol);
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

            if (!flavour.Contains(randomTaste)) { flavour.Add(randomTaste); flavourConnected.Add(false); }
        }
    }
}
