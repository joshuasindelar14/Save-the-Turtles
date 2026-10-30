using UnityEngine;

public class Turtle : MonoBehaviour
{
    [Header("Egg laying")]
    public GameObject eggPrefab;      // the spawner fills this in
    public float layDuration = 1.2f;  // seconds the turtle pauses to lay
    public float eggBehindOffset = 0.6f;

    float speed;
    int direction;      // 1 right, -1 left
    float eggX;         // where this turtle stops to lay its egg
    float despawnX;     // past the far edge of the screen
    bool laidEgg;
    float layTimer;
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Init(int dir, float walkSpeed, float eggXPos, float despawnXPos)
    {
        direction = dir;
        speed = walkSpeed;
        eggX = eggXPos;
        despawnX = despawnXPos;

        // the sprite faces up
        transform.rotation = Quaternion.Euler(0f, 0f, -90f * direction);
    }

    void Update()
    {
        // pause while laying
        if (layTimer > 0f)
        {
            layTimer -= Time.deltaTime;
            if (layTimer <= 0f && animator != null) animator.speed = 1f;
            return;
        }

        // walk
        transform.position += Vector3.right * direction * speed * Time.deltaTime;

        // reached the egg?
        float x = transform.position.x;
        bool reachedEggSpot = (direction == 1 && x >= eggX) || (direction == -1 && x <= eggX);
        if (!laidEgg && reachedEggSpot)
        {
            LayEgg();
        }

        // walked off the side?
        bool offScreen = (direction == 1 && x >= despawnX) || (direction == -1 && x <= despawnX);
        if (offScreen) Destroy(gameObject);
    }

    void LayEgg()
    {
        laidEgg = true;
        layTimer = layDuration;
        if (animator != null) animator.speed = 0f; // freeze the flippers

        Vector3 pos = transform.position;
        pos.x -= direction * eggBehindOffset; // egg appears behind the turtle
        pos.z = 0f;
        Instantiate(eggPrefab, pos, Quaternion.identity);
    }
}