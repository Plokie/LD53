using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Ring : MonoBehaviour {
    public float precision = 0.01f;
    public float radius = 3f;
    public Color color = Color.white;
    public float thickness = .5f;
    public LineRenderer lr;
    public bool draw = false;
    public float axisOffset = 0;

    void Awake() {
        if(lr==null) lr = GetComponent<LineRenderer>();

        // OnValidate();
    }

    void Update() {
        lr.enabled = draw;
        Draw();
    }

    public void Draw() {
        float theta = 0f;
        int size = (int)((1f / precision) + 1f);
        lr.positionCount = size;
        for (int i = 0; i < size; i++) {
            theta += (2.0f * Mathf.PI * precision);
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            lr.SetPosition(i, new Vector3(x, axisOffset, y) + transform.position);
        }
    }

    void OnValidate() {
        UpdateVars();
    }

    public void UpdateVars() {
        if(lr==null) return;

        lr.startColor = color;
        lr.endColor = color;

        lr.startWidth = thickness;
        lr.endWidth=thickness;
    }
}