using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum Kind { Crab, Seagull }
    enum State { Approaching, Waiting, Attacking, Eating, Fleeing }

    [Header("Type (decides which scare sound plays)")]
    public Kind kind = Kind.Crab;

    [Header("Timing")]
    public float lingerTime = 10f;       // seconds it waits beside the egg
    public float afterAttackPause = 0.8f;

    [Header("Movement")]
    public float approachSpeed = 1.5f;
    public float attackSpeed = 2f;
    public float fleeSpeed = 5f;
    public float standOffDistance = 0.9f; // how far from the egg it waits
    public float spawnYRange = 0f;        // random height offset when arriving (seagull)
    public float fleeUpward = 0f;         // upward drift when leaving (seagull)
    public bool rotateToFaceMovement = false; // on for the seagull

    State state = State.Approaching;
    Egg egg;
    int side;
    Vector3 standPoint;
    Vector3 fleeDir;
    float timer;
    float left, right;

    public void Init(Egg targetEgg, int arriveFromSide)
    {
        egg = targetEgg;
        side = arriveFromSide; // -1 left, 1 right

        Camera cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        left = cam.transform.position.x - halfW;
        right = cam.transform.position.x + halfW;

        Vector3 eggPos = egg.transform.position;
        float startX = side < 0 ? left - 1.5f : right + 1.5f;
        float startY = eggPos.y + Random.Range(-spawnYRange, spawnYRange);
        transform.position = new Vector3(startX, startY, 0f);

        standPoint = new Vector3(eggPos.x + side * standOffDistance, eggPos.y, 0f);
    }

    void Update()
    {
        // if egg disappears, leave
        if (egg == null && state != State.Fleeing) StartFleeing();

        switch (state)
        {
            case State.Approaching:
                if (MoveToward(standPoint, approachSpeed))
                {
                    state = State.Waiting;
                    timer = lingerTime;
                }
                break;

            case State.Waiting:
                timer -= Time.deltaTime;
                if (timer <= 0f) state = State.Attacking;
                break;

            case State.Attacking:
                if (MoveToward(egg.transform.position, attackSpeed))
                {
                    egg.Break();
                    state = State.Eating;
                    timer = afterAttackPause;
                }
                break;

            case State.Eating:
                timer -= Time.deltaTime;
                if (timer <= 0f) StartFleeing();
                break;

            case State.Fleeing:
                transform.position += fleeDir * fleeSpeed * Time.deltaTime;
                float x = transform.position.x;
                if (x < left - 2f || x > right + 2f) Destroy(gameObject);
                break;
        }
    }

    // called by ClickManager when the player clicks this enemy
    public void Scare()
    {
        if (state == State.Fleeing || state == State.Eating) return;

        AudioManager.PlayScare(kind == Kind.Seagull);

        if (egg != null)
        {
            // the egg hatches when the enemy's wait ends, even if player swats it early
            float hatchDelay = 0.5f;
            if (state == State.Waiting) hatchDelay = timer;
            else if (state == State.Approaching) hatchDelay = lingerTime;

            egg.EnemyScared(Mathf.Max(hatchDelay, 0.5f));
        }
        StartFleeing();
    }

    void StartFleeing()
    {
        state = State.Fleeing;
        fleeDir = new Vector3(side, fleeUpward, 0f).normalized;
        if (rotateToFaceMovement) transform.up = fleeDir;
    }

    // moves to a point, returns true when arrives
    bool MoveToward(Vector3 target, float speed)
    {
        Vector3 dir = target - transform.position;
        dir.z = 0f;
        float step = speed * Time.deltaTime;

        if (dir.magnitude <= step)
        {
            transform.position = new Vector3(target.x, target.y, 0f);
            return true;
        }

        dir.Normalize();
        transform.position += dir * step;
        if (rotateToFaceMovement) transform.up = dir;
        return false;
    }
}