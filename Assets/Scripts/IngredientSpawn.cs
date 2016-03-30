using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IngredientSpawn : MonoBehaviour 
{
    public float spawnInterval = 1.5f;
    public Rect spawnArea;
    public GameObject[] ingredients = new GameObject[4];

	// Use this for initialization
	void Start () 
    {
        StartCoroutine("spawnIngredients");
	
	}

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(spawnArea.center + (Vector2)transform.position, spawnArea.size);
    }


    IEnumerator spawnIngredients()
    {
        while (this.gameObject.active)
        {
            Vector2 spawnPos = new Vector2();
            spawnPos.x = Random.value * spawnArea.width + spawnArea.x + transform.position.x;
            spawnPos.y = Random.value * spawnArea.height + spawnArea.y + transform.position.y;

            int ingredientToSpawn = Random.Range(0, ingredients.Length);

            Instantiate(ingredients[ingredientToSpawn], spawnPos, Quaternion.identity);

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
