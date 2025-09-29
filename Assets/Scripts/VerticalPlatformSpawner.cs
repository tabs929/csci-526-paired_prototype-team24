using UnityEngine;

public class VerticalPlatformSpawner : MonoBehaviour
{
    public float firstGapAbovePlayer = 1.0f;
    public float yStep = 2.3f;
    public float minHorizontalSeparation = 1.75f;

    public int maxPickAttempts = 8;

    public Transform cam;
    public GameObject platformPrefab;
    public Vector2 xRange = new(-3.2f, 3.2f);
    public int prewarm = 12;

    public Transform startPlatform;
    public float firstMinHorizontalSeparation = 2.2f;

    float nextY;
    float lastX = float.NaN;                   

    void Start()
    {
        if (!cam) cam = Camera.main.transform;

        if (!startPlatform)
        {
            var sp = GameObject.Find("StartPlatform");
            if (sp) startPlatform = sp.transform;
        }

        if (startPlatform)
            lastX = startPlatform.position.x;

        nextY = cam.position.y + firstGapAbovePlayer - yStep;

        float originalMinSep = minHorizontalSeparation;
        if (!float.IsNaN(lastX))
            minHorizontalSeparation = Mathf.Max(minHorizontalSeparation, firstMinHorizontalSeparation);
        SpawnOne();  
        minHorizontalSeparation = originalMinSep;

        for (int i = 0; i < prewarm - 1; i++)
            SpawnOne();
    }

    void Update()
    {
        while (nextY < cam.position.y + yStep * prewarm)
            SpawnOne();
    }

    void SpawnOne()
    {
        nextY += yStep;
        float x = PickX();
        var go = Instantiate(platformPrefab, new Vector3(x, nextY, 0), Quaternion.identity);

        var col = go.GetComponent<PlatformColor>();
        if (col) col.color = (ColorType)Random.Range(0, 4);

        lastX = x;
    }

    float PickX()
    {
        if (float.IsNaN(lastX))
            return Random.Range(xRange.x, xRange.y);

        for (int i = 0; i < maxPickAttempts; i++)
        {
            float candidate = Random.Range(xRange.x, xRange.y);
            if (Mathf.Abs(candidate - lastX) >= minHorizontalSeparation)
                return candidate;
        }

        float mid = (xRange.x + xRange.y) * 0.5f;
        if (lastX <= mid)
            return Mathf.Clamp(xRange.y - 0.01f, xRange.x, xRange.y);
        else
            return Mathf.Clamp(xRange.x + 0.01f, xRange.x, xRange.y);
    }
}
