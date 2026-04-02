using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotPop : MonoBehaviour
{
    public GameObject parent;
    public Vector3 offset;
    public void Init(Pot pot)
    {
        parent = pot.plantRoot.gameObject;
    }
    private void LateUpdate()
    {
        if (parent == null) return;
        Vector3 position = Camera.main.WorldToScreenPoint(parent.transform.position + offset);
        transform.position = position;
    }
}
