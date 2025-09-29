using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlatformColor : MonoBehaviour
{
    public ColorType color;

    public Color red    = new Color(1f, 0.23f, 0.19f);
    public Color green  = new Color(0.20f, 0.78f, 0.35f);
    public Color blue   = new Color(0.00f, 0.48f, 1f);
    public Color yellow = new Color(1f, 0.80f, 0.00f);

    void OnValidate() => Apply();
    void Start()      => Apply();

    void Apply()
    {
        var sr = GetComponent<SpriteRenderer>();
        sr.color = color switch
        {
            ColorType.Red    => red,
            ColorType.Green  => green,
            ColorType.Blue   => blue,
            ColorType.Yellow => yellow,
            _ => Color.white
        };
    }
}
