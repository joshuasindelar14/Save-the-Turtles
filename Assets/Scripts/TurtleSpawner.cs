using System.Collections;
using UnityEngine;

public class TurtleSpawner : MonoBehaviour
{
    public GameObject turtlePrefab;
    public GameObject eggPrefab;

    [Header("Where turtles walk (world units)")]
    public float minY = 1.5f;      // lowest row a turtle can walk on
    public float maxY = 10.5f;     // highest row
    public float eggMargin = 1.5f; // keeps eggs away from the edges

    [Header("Difficulty ramp")]
    public float rampSeconds = 120f; // time to reach full difficulty
    public float startInterval = 3f; // seconds between turtles at the start
    public float minInterval = 0.5f; // seconds between turtles at full difficulty
    public float startSpeed = 1.5f;  // walk speed at the start
    public float maxSpeed = 4f;    // walk speed at full difficulty

    Camera cam;

    void Start()
    {
        Debug.Log("Spawner Start ran");
        cam = Camera.main;
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            SpawnTurtle();

            float interval = Mathf.Lerp(startInterval, minInterval, Ramp());
            yield return new WaitForSeconds(interval * Random.Range(0.8f, 1.2f));
        }
    }

    // 0 at the start of the game, 1 once rampSeconds have passed
    float Ramp()
    {
        return Mathf.Clamp01(Time.timeSinceLevelLoad / rampSeconds);
    }

    void SpawnTurtle()
    {
        Debug.Log("Spawning a turtle!");
        // work out the screen edges from the camera
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float left = cam.transform.position.x - halfW;
        float right = cam.transform.position.x + halfW;

        int dir = Random.value < 0.5f ? 1 : -1;   // 1 left, -1 right
        float startX = dir == 1 ? left - 1.5f : right + 1.5f;
        float endX = dir == 1 ? right + 1.5f : left - 1.5f;

        float y = Random.Range(minY, maxY);
        float eggX = Random.Range(left + eggMargin, right - eggMargin);
        float speed = Mathf.Lerp(startSpeed, maxSpeed, Ramp()) * Random.Range(0.9f, 1.1f);

        GameObject obj = Instantiate(turtlePrefab, new Vector3(startX, y, 0f), Quaternion.identity);
        Turtle turtle = obj.GetComponent<Turtle>();
        turtle.eggPrefab = eggPrefab;
        turtle.Init(dir, speed, eggX, endX);
    }
}