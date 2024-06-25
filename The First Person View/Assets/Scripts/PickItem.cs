using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickItem : MonoBehaviour
{
    public string itemName = "Some Item";
    public Texture itemPreview;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.tag = "Respawn";
    }
    
    public void PickUp()
    {
        Destroy(gameObject);
    }
}
