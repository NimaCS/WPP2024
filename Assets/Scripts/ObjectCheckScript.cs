using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCheckScript : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToCheck;
    [SerializeField] private List<GameObject> objectsInArea;
    public bool allObjectsInArea;

    void Update()
    {
        if (objectsInArea.Count == objectsToCheck.Count)
            allObjectsInArea = true;
        else
            allObjectsInArea = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (objectsToCheck.Contains(other.gameObject))
        {
            objectsInArea.Add(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (objectsToCheck.Contains(other.gameObject) && objectsInArea.Contains(other.gameObject))
        {
            objectsInArea.Remove(other.gameObject);
        }
    }
}
