using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public float sensX = 2f;
    public float sensY = 2f;

    private Vector3 rotation;

    private List<GameObject> highlightableItems;
    private GameObject activeItem;
    private GameObject[] enemies;

    public GameObject player;
    public GameObject credits;

    public SettingsMenu settings;

    public bool beingChased = false;

    // Start is called before the first frame update
    void Start()
    {
        settings.LoadSettings();

        activeItem = null;
        var highlightables = FindObjectsOfType<Highlight>();
        highlightableItems = new List<GameObject>();
        foreach(var highlightable in highlightables)
        {
            highlightableItems.Add(highlightable.gameObject);
        }
        

        Cursor.lockState = CursorLockMode.Locked;
        rotation = new Vector3(0, 0, 0);


        enemies = GameObject.FindGameObjectsWithTag("EnemyParents");
    }

    // Update is called once per frame
    void Update()
    {
        if(!player.GetComponent<PlayerController>().isPaused && !player.GetComponent<PlayerController>().isDead)
        {
            var mouseX = Input.GetAxisRaw("Mouse X");
            var mouseY = Input.GetAxisRaw("Mouse Y");

            rotation.x += -mouseY * sensX;
            rotation.y += mouseX * sensY;

            rotation.x = Mathf.Clamp(rotation.x, -90f, 90f);
            rotation.y = Mathf.Clamp(rotation.y, -90f, 90f);
            transform.localRotation = Quaternion.Euler(rotation);



            RaycastHit hit;
            foreach (var item in highlightableItems)
            {
                item.GetComponent<Highlight>().DisableOutline();
            }
            GameObject highlightedItem = null;



            int layermask = 1 << 6;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 15, layermask))
            {
                GameObject hitObject = hit.collider.transform.gameObject;
                if (hitObject.GetComponent<Highlight>() != null)
                {
                    highlightedItem = hitObject;
                    highlightedItem.GetComponent<Highlight>().EnableOutline();
                }
            }



            if (Input.GetMouseButtonDown(0))
            {
                if (activeItem != null)
                {
                    if (highlightedItem != null && highlightedItem.GetComponent<Obstacle>() != null && activeItem == highlightedItem.GetComponent<Obstacle>().pairedObject)
                    {
                        highlightedItem.GetComponent<Obstacle>().Unlock();
                        highlightableItems.Remove(highlightedItem);
                    }
                    activeItem.GetComponent<Highlight>().SetIsActive(false);
                    activeItem.GetComponent<Highlight>().DisableOutline();
                    activeItem = null;
                }
                if (highlightedItem != null)
                {

                    if (highlightedItem.GetComponent<Lilypad>() != null)
                    {
                        AddItemToInventory(highlightedItem.GetComponent<Lilypad>());
                    }
                    if (highlightedItem.GetComponent<InventoryItem>() != null)
                    {
                        SetActiveItem(highlightedItem);
                    }
                    if (highlightedItem.GetComponent<Dog>() != null)
                    {
                        Cursor.lockState = CursorLockMode.None;
                        credits.SetActive(true);
                    }
                }
            }
        } else if(player.GetComponent<PlayerController>().isDead)
        {
            Transform tMin = null;
            float minDist = Mathf.Infinity;
            GameObject[] faceLocations = GameObject.FindGameObjectsWithTag("Enemy");
            foreach(GameObject face in faceLocations)
            {
                float dist = Vector3.Distance(face.transform.position, transform.position);
                if(dist < minDist)
                {
                    tMin = face.transform;
                    minDist = dist;
                }
            }


            transform.LookAt(tMin);
        }

        beingChased = false;
        foreach(GameObject enemy in enemies)
        {
            if(enemy.gameObject.GetComponent<EnemyAI>().isChasing)
            {
                beingChased = true;
            }
        }

        var chasingEnemy = GameObject.FindGameObjectWithTag("ChasingEnemy");

        if (chasingEnemy.gameObject.GetComponent<EnemyAI>().isChasing)
        {
            beingChased = true;
        }



        if (beingChased)
        {
            GameObject camera = GameObject.FindGameObjectWithTag("ChaseMusic");
            camera.GetComponent<AudioSource>().volume = 0.17f;
        } else
        {
            GameObject camera = GameObject.FindGameObjectWithTag("ChaseMusic");
            camera.GetComponent<AudioSource>().volume = 0f;
        }
    }

    private void SetActiveItem(GameObject item)
    {
        foreach(var hItem in highlightableItems)
        {
            hItem.GetComponent<Highlight>().SetIsActive(hItem == item);
        }
        activeItem = item;

    }

    private void AddItemToInventory(Lilypad lilypad)
    {
        if(lilypad.inventoryItem.name == "key")
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("EnemyParents");

            foreach(GameObject enemy in enemies)
            {
                enemy.GetComponentInChildren<Renderer>().enabled = true;
                enemy.GetComponent<EnemyAI>().SetIsActive(true);
            }
        }

        lilypad.inventoryItem.GetComponent<Renderer>().enabled = true;
        highlightableItems.Remove(lilypad.gameObject);
        Destroy(lilypad.gameObject);
    }

    public void ChaseMusicEnabled(bool enabled)
    {
        this.beingChased = enabled;
    }
}
