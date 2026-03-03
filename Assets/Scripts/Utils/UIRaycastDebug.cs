using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRaycastDebug : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (!EventSystem.current)
        {
            Debug.LogError("❌ Pas d'EventSystem dans la scène.");
            return;
        }

        var ped = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        if (results.Count == 0)
        {
            Debug.LogWarning("⚠️ Clic UI: 0 hit (rien n'est raycasté).");
            return;
        }

        Debug.Log($"✅ Clic UI hits: {results.Count}");
        for (int i = 0; i < Mathf.Min(results.Count, 10); i++)
        {
            var r = results[i];
            Debug.Log($"{i} -> {r.gameObject.name} | module={r.module} | depth={r.depth} | sortingOrder={r.sortingOrder}");
        }
    }
}