using System.Collections;
using UnityEngine;

public class Egg : MonoBehaviour
{
    // for GameManager
    public static event System.Action<Egg> EggLost;
    public static event System.Action<Egg> EggHatched;

    [Header("Prefabs")]
    public GameObject crabPrefab;
    public GameObject seagullPrefab;
    public GameObject babyTurtlePrefab;

    [Header("Sprites")]
    public Sprite brokenSprite;   // enemy breaks egg
    public Sprite hatchedSprite;  // egg hatches

    [Header("Timing")]
    public float attackDelay = 5f;  // seconds after being laid before an enemy arrives
    public float shakeTime = 1f;    // wobble before hatching

    public bool IsBroken { get; private set; }
    public bool IsHatched { get; private set; }

    bool hatchStarted;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(attackDelay);

        GameObject prefab = Random.value < 0.5f ? crabPrefab : seagullPrefab;
        int side = Random.value < 0.5f ? -1 : 1;

        GameObject obj = Instantiate(prefab);
        obj.GetComponent<Enemy>().Init(this, side);
    }

    public void Break()
    {
        if (IsBroken || hatchStarted) return;
        IsBroken = true;
        sr.sprite = brokenSprite;
        AudioManager.PlayEggCrush();
        EggLost?.Invoke(this);
    }

    // called when player scares the enemy away
    public void EnemyScared(float hatchDelay)
    {
        if (IsBroken || hatchStarted) return;
        hatchStarted = true;
        StartCoroutine(HatchRoutine(hatchDelay));
    }

    IEnumerator HatchRoutine(float delay)
    {
        // wait out enemy wait period, then wobble
        float shake = Mathf.Min(shakeTime, delay);
        yield return new WaitForSeconds(delay - shake);

        float t = 0f;
        while (t < shake)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 30f) * 8f);
            yield return null;
        }
        transform.rotation = Quaternion.identity;

        // swap to empty shell and release baby
        sr.sprite = hatchedSprite != null ? hatchedSprite : brokenSprite;
        IsHatched = true;
        AudioManager.PlayHatch();

        Vector3 pos = transform.position;
        pos.z = 0f;
        Instantiate(babyTurtlePrefab, pos, Quaternion.identity);

        EggHatched?.Invoke(this);
    }
}