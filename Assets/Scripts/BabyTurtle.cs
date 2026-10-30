using UnityEngine;

public class BabyTurtle : MonoBehaviour
{
    public float speed = 1.2f;
    public float shorelineY = 12f;     // approx sand line
    public float fadeDistance = 1.5f;  // how far into the it fades out
    public float swayAmount = 0.08f;
    public float swaySpeed = 8f;
    public Color waterTint = new Color(0.55f, 0.8f, 1f);

    SpriteRenderer sr;
    float startX;
    float swayOffset;
    bool splashed;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        startX = transform.position.x;
        swayOffset = Random.value * 10f;
    }

    void Update()
    {
        // waddle to the water
        Vector3 pos = transform.position;
        pos.y += speed * Time.deltaTime;
        pos.x = startX + Mathf.Sin(Time.time * swaySpeed + swayOffset) * swayAmount;
        transform.position = pos;

        // 0 on the sand, 1 once it is fully into the water
        float intoWater = Mathf.Clamp01((pos.y - shorelineY) / fadeDistance);

        // splash once
        if (!splashed && intoWater > 0.1f)
        {
            splashed = true;
            AudioManager.PlaySplash();
        }

        Color c = Color.Lerp(Color.white, waterTint, intoWater);
        c.a = 1f - intoWater;
        sr.color = c;
        transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.8f, intoWater);

        if (intoWater >= 1f) Destroy(gameObject);
    }
}