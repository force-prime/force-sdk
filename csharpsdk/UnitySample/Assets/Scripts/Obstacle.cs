using System;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    static private float LEFT_BORDER = -10;

    static public event Action<GameObject> OnTriggerCollision;

    static public int Count { get; private set; }
    static public Obstacle Last {get; private set;}

    static public void Generate(GameObject parent, GameObject prefab, float minX, float maxX)
    {
        var lastX = Last != null ? Last.transform.localPosition.x : 0f;
        var o = Instantiate(prefab, parent.transform);
        float deltaX = UnityEngine.Random.Range(minX, maxX);
        float deltaY = UnityEngine.Random.Range(1.5f, 4f);
        if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
            deltaY = -deltaY;
        o.transform.localPosition = new Vector3(lastX + deltaX, deltaY, 0);
    }

    static public void ClearAll()
    {
        foreach (var o in GameObject.FindObjectsOfType<Obstacle>(true))
            Destroy(o.gameObject);

        Count = 0;
        Last = null;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        OnTriggerCollision?.Invoke(col.gameObject);
    }

    private void Awake()
    {
        Count++;
        Last = this;
    }

    private void OnDestroy()
    {
        Count--;
    }

    private void Update()
    {
        if (transform.position.x < LEFT_BORDER)
            Destroy(gameObject);
    }
}
