using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Bakes a runtime NavMesh from the scene's physics colliders.
/// Runs before other Awakes so NavMeshAgents can bind to the mesh at startup.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class PrototypeNavMeshBaker : MonoBehaviour
{
    void Awake()
    {
        try
        {
            var settings = NavMesh.GetSettingsByIndex(0);
            var bounds = new Bounds(Vector3.zero, new Vector3(200f, 40f, 200f));
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();
            NavMeshBuilder.CollectSources(bounds, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);

            if (sources.Count == 0)
            {
                Debug.LogWarning("PrototypeNavMeshBaker: no sources collected.");
                return;
            }

            var data = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);
            if (data == null)
            {
                Debug.LogWarning("PrototypeNavMeshBaker: bake failed.");
                return;
            }

            NavMesh.AddNavMeshData(data);
            Debug.Log($"PrototypeNavMeshBaker: baked {sources.Count} sources.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"PrototypeNavMeshBaker: {e.Message}");
        }
    }
}