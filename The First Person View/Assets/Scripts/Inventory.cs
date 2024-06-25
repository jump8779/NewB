using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Texture crosshairTexture;
    
    public PlayerCtrl playerController;
    [HideInInspector]
    public ItemCombinationList combinationList;
    [HideInInspector]
    public InteractableObject interactableObject;
    public PickItem[] availableItems; //List with Prefabs of all the available items
    
    //Viewing Item
    private PickItem currentItemInstance;
    private bool isViewingItem = false;

    //Available items slots
    int[] itemSlots = new int[12];
    bool showInventory = false;
    float windowAnimation = 1;
    float animationTimer = 0;

    //UI Drag & Drop
    int hoveringOverIndex = -1;
    int itemIndexToDrag = -1;
    Vector2 dragOffset = Vector2.zero;

    //Item Pick up
    PickItem detectedItem;
    int detectedItemIndex;

    //Interactable Item
    [HideInInspector]
    public bool isInteractable = false;

    //Combined Item
    string combineItem;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        combinationList = GetComponent<ItemCombinationList>();
        if (combinationList == null)
        {
            combinationList = this.gameObject.AddComponent<ItemCombinationList>();
        }

        //Initialize Item Slots
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i] = -1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Show/Hide inventory
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            showInventory = !showInventory;
            animationTimer = 0;

            if (showInventory)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        //Interact Item
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isInteractable)
            {
                interactableObject.TryInteract();
            }
        }

        if (animationTimer < 1)
        {
            animationTimer += Time.deltaTime;
        }

        if (showInventory)
        {
            windowAnimation = Mathf.Lerp(windowAnimation, 0, animationTimer);
            playerController.canMove = false;
        }
        else
        {
            windowAnimation = Mathf.Lerp(windowAnimation, 1f, animationTimer);
            playerController.canMove = true;
        }

        //When item viewing
        if (isViewingItem)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel") * 2;
            if (Input.GetMouseButton(0))
            {
                currentItemInstance.transform.Rotate(0f, -Input.GetAxis("Mouse X") * 2, 0f, Space.World);
                currentItemInstance.transform.Rotate(Input.GetAxis("Mouse Y") * 2, 0f, 0f, Space.World);
            }
            if (scroll != 0)
            {
                float Transformedscroll = TransformValue(scroll);
                currentItemInstance.transform.localScale = Vector3.one * Transformedscroll;
            }
            if (Input.GetMouseButtonDown(1))
            {
                CloseItemView();
            }
        }

        //Begin View Item
        if (Input.GetMouseButtonDown(2) && hoveringOverIndex > -1 && itemSlots[hoveringOverIndex] > -1)
        {
            ViewItem(availableItems[itemSlots[hoveringOverIndex]]);
        }

        //Begin item drag
        if (Input.GetMouseButtonDown(0) && hoveringOverIndex > -1 && itemSlots[hoveringOverIndex] > -1)
        {
            itemIndexToDrag = hoveringOverIndex;
        }

        //Release dragged item
        if (Input.GetMouseButtonUp(0) && itemIndexToDrag > -1)
        {
            if (hoveringOverIndex < 0)
            {
                //Drop the item outside
                Instantiate(availableItems[itemSlots[itemIndexToDrag]], playerController.PlayerCamera.transform.position + (playerController.PlayerCamera.transform.forward), Quaternion.identity);
                itemSlots[itemIndexToDrag] = -1;
            }
            else
            {
                if (itemSlots[hoveringOverIndex] > -1)
                {
                    combineItem = ItemCombine(availableItems[itemSlots[itemIndexToDrag]],
                        availableItems[itemSlots[hoveringOverIndex]]);

                    if (combineItem != null)
                    {
                        for (int i = 0; i < availableItems.Length; i++)
                        {
                            if (availableItems[i].itemName == combineItem)
                            {
                                itemSlots[itemIndexToDrag] = -1;
                                itemSlots[hoveringOverIndex] = i;
                                break;
                            }
                        }
                    }
                    else
                    {
                        int itemIndexTmp = itemSlots[itemIndexToDrag];
                        itemSlots[itemIndexToDrag] = itemSlots[hoveringOverIndex];
                        itemSlots[hoveringOverIndex] = itemIndexTmp;
                    }
                }
                else
                {
                    //Switch items between the selected slot and the one we are hovering on
                    int itemIndexTmp = itemSlots[itemIndexToDrag];
                    itemSlots[itemIndexToDrag] = itemSlots[hoveringOverIndex];
                    itemSlots[hoveringOverIndex] = itemIndexTmp;
                }
            }
            itemIndexToDrag = -1;
        }

        //Item pick up
        if (detectedItem && detectedItemIndex > -1)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("KeyInput");
                //Add the item to inventory
                int slotToAddTo = -1;
                for (int i = 0; i < itemSlots.Length; i++)
                {
                    if (itemSlots[i] == -1)
                    {
                        slotToAddTo = i;
                        break;
                    }
                }
                if (slotToAddTo > -1)
                {
                    itemSlots[slotToAddTo] = detectedItemIndex;
                    detectedItem.PickUp();
                }
            }
        }
    }

    void FixedUpdate()
    {
        //Detect if the Player is looking at any item
        RaycastHit hit;
        Ray ray = playerController.PlayerCamera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));

        if (Physics.Raycast(ray, out hit, 500f))//2.5f))
        {
            Transform objectHit = hit.transform;

            if (objectHit.CompareTag("Respawn"))
            {
                Debug.Log("Detected!");
                if ((detectedItem == null || detectedItem.transform != objectHit) && objectHit.GetComponent<PickItem>() != null)
                {
                    PickItem itemTmp = objectHit.GetComponent<PickItem>();

                    //Check if item is in availableItemsList
                    for (int i = 0; i < availableItems.Length; i++)
                    {
                        if (availableItems[i].itemName == itemTmp.itemName)
                        {
                            detectedItem = itemTmp;
                            detectedItemIndex = i;
                        }
                    }
                }
            }
            else
            {
                detectedItem = null;
            }
            if (objectHit.CompareTag("Interactable"))
            {
                Debug.Log("Interactable");
                interactableObject = objectHit.GetComponent<InteractableObject>();
                isInteractable = true;
            }
            else
            {
                isInteractable = false;
            }
        }
        else
        {
            detectedItem = null;
            isInteractable = false;
        }
    }

    public void ViewItem(PickItem item)
    {
        if (currentItemInstance != null)
        {
            currentItemInstance.PickUp();
        }

        currentItemInstance = Instantiate(item, playerController.PlayerCamera.transform.position + (playerController.PlayerCamera.transform.forward), Quaternion.identity);
        currentItemInstance.transform.localScale = Vector3.one * 0.5f;
        Rigidbody rb = currentItemInstance.GetComponent<Rigidbody>();
        if(rb != null )
        {
            Destroy(rb);
        }
        isViewingItem = true;
        playerController.canMove = false;
    }
    public void CloseItemView()
    {
        if (currentItemInstance != null)
        {
            currentItemInstance.PickUp();
        }
        isViewingItem = false;
        playerController.canMove = true;
    }
    //item combine
    public string ItemCombine(PickItem item1, PickItem item2)
    {
        string resultItemName = combinationList.GetCombinedItem(item1.itemName, item2.itemName);

        if (!string.IsNullOrEmpty(resultItemName))
        {
            return resultItemName;
        }
        return null;
    }
    //Item check
    public bool HasItem(PickItem itemName)
    {
        if (isInteractable)
        {
            for (int i = 0; i < availableItems.Length; i++)
            {
                if (availableItems[i].itemName == itemName.itemName)
                {
                    return itemSlots.Contains(i);
                }
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    float TransformValue(float x)
    {
        // 음수일 경우
        if (x < 0)
        {
            return x / (0.5f + Mathf.Abs(x));
        }
        // 양수일 경우
        else
        {
            return 0.5f + x;
        }
    }

    void OnGUI()
    {
        //Inventory UI
        GUI.Label(new Rect(5, 5, 200, 25), "Press 'Tab' to open Inventory");

        //Inventory window
        if (windowAnimation < 1)
        {
            GUILayout.BeginArea(new Rect(10 - (430 * windowAnimation), Screen.height / 2 - 200, 302, 430), GUI.skin.GetStyle("box"));

            GUILayout.Label("Inventory", GUILayout.Height(25));

            GUILayout.BeginVertical();
            for (int i = 0; i < itemSlots.Length; i += 3)
            {
                GUILayout.BeginHorizontal();
                //Display 3 items in a row
                for (int a = 0; a < 3; a++)
                {
                    if (i + a < itemSlots.Length)
                    {
                        if (itemIndexToDrag == i + a || (itemIndexToDrag > -1 && hoveringOverIndex == i + a))
                        {
                            GUI.enabled = false;
                        }

                        if (itemSlots[i + a] > -1)
                        {
                            if (availableItems[itemSlots[i + a]].itemPreview)
                            {
                                GUILayout.Box(availableItems[itemSlots[i + a]].itemPreview, GUILayout.Width(95), GUILayout.Height(95));
                            }
                            else
                            {
                                GUILayout.Box(availableItems[itemSlots[i + a]].itemName, GUILayout.Width(95), GUILayout.Height(95));
                            }
                        }
                        else
                        {
                            //Empty slot
                            GUILayout.Box("", GUILayout.Width(95), GUILayout.Height(95));
                        }

                        //Detect if the mouse cursor is hovering over item
                        Rect lastRect = GUILayoutUtility.GetLastRect();
                        Vector2 eventMousePositon = Event.current.mousePosition;
                        if (Event.current.type == EventType.Repaint && lastRect.Contains(eventMousePositon))
                        {
                            hoveringOverIndex = i + a;
                            if (itemIndexToDrag < 0)
                            {
                                dragOffset = new Vector2(lastRect.x - eventMousePositon.x, lastRect.y - eventMousePositon.y);
                            }
                        }

                        GUI.enabled = true;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint && !GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                hoveringOverIndex = -1;
            }

            GUILayout.EndArea();
        }

        //Item dragging
        if (itemIndexToDrag > -1)
        {
            if (availableItems[itemSlots[itemIndexToDrag]].itemPreview)
            {
                GUI.Box(new Rect(Input.mousePosition.x + dragOffset.x, Screen.height - Input.mousePosition.y + dragOffset.y, 95, 95), availableItems[itemSlots[itemIndexToDrag]].itemPreview);
            }
            else
            {
                GUI.Box(new Rect(Input.mousePosition.x + dragOffset.x, Screen.height - Input.mousePosition.y + dragOffset.y, 95, 95), availableItems[itemSlots[itemIndexToDrag]].itemName);
            }
        }

        //Display item name when hovering over it
        if (hoveringOverIndex > -1 && itemSlots[hoveringOverIndex] > -1 && itemIndexToDrag < 0)
        {
            GUI.Box(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y - 30, 100, 25), availableItems[itemSlots[hoveringOverIndex]].itemName);
        }

        if (!showInventory)
        {
            //Player crosshair
            GUI.color = detectedItem ? Color.green : Color.white;
            GUI.DrawTexture(new Rect(Screen.width / 2 - 4, Screen.height / 2 - 4, 8, 8), crosshairTexture);
            GUI.color = Color.white;

            //Pick up message
            if (detectedItem)
            {
                GUI.color = new Color(0, 0, 0, 0.84f);
                GUI.Label(new Rect(Screen.width / 2 - 75 + 1, Screen.height / 2 - 50 + 1, 150, 20), "Press 'F' to pick '" + detectedItem.itemName + "'");
                GUI.color = Color.green;
                GUI.Label(new Rect(Screen.width / 2 - 75, Screen.height / 2 - 50, 150, 20), "Press 'F' to pick '" + detectedItem.itemName + "'");
            }
        }
    }
}
