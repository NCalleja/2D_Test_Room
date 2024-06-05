using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImagePool : MonoBehaviour
{

    // Serialize Field adds the below field to the Inspector
    [SerializeField]
    private GameObject afterImagePrefab;

    private Queue<GameObject> availableObjects = new Queue<GameObject>();

    public static PlayerAfterImagePool Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        GrowPool();
    }

    // 10 Game Objects for the Pool
    private void GrowPool()
    {
        for (int i = 0; i < 10; i++)
        {
            // Var to create the Variable but then Instantiate it as the After Image Prefab
            var instanceToAdd = Instantiate(afterImagePrefab);
            instanceToAdd.transform.SetParent(transform);
            AddToPool(instanceToAdd);

            // DEBUG
            Debug.Log("Afterimage added to pool: " + instanceToAdd.name);

        }

        // DEBUG
        Debug.Log("Pool Grown: " + availableObjects.Count + " after images available");

    }

    public void AddToPool(GameObject instance)
    {
        instance.SetActive(false);
        availableObjects.Enqueue(instance);

        // DEBUG
        Debug.Log("Afterimage Added Back to Pool: " + instance.name);
    }

    public GameObject GetFromPool()
    {
        if(availableObjects.Count == 0) 
        {
            GrowPool();
        }

        var instance = availableObjects.Dequeue();
        instance.SetActive(true);

        // DEBUG
        Debug.Log("Afterimage Activated " + instance.name);

        return instance;
    }
}
