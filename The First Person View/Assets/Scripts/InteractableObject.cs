using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InteractableObject : MonoBehaviour
{
    public PickItem requiredItem; //상호작용 할 아이템
    public Inventory playerInventory;

    public GameObject newObject;
    void Start()
    {
        gameObject.tag = "Interactable";
    }
    public void TryInteract()
    {
        if (playerInventory.HasItem(requiredItem))
        {
            Debug.Log("Interact Success");
            Instantiate(newObject, transform.position, transform.rotation);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Interact Fail");
        }
    }
}
