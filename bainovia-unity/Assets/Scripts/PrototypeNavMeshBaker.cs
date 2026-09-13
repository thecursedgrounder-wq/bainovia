using UnityEngine;
using UnityEngine.AI;
using System.Collections;
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

    /// <summary>
    /// Agents whose OnEnable ran before the bake complete (player scene order is
    /// not guaranteed) fail to create. Re-creating them one frame after the mesh
    /// exists makes pathing reliable in the built player.
    /// </summary>
    void Start()
    {
        StartCoroutine(RebindAgents());
    }

    IEnumerator RebindAgents()
    {
        yield return null;

        var agents = Object.FindObjectsByType<NavMeshAgent>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var a in agents)
        {
            if (a == null) continue;
            a.enabled = false;
            a.enabled = true;
        }
        Debug.Log($"PrototypeNavMeshBaker: rebound {agents.Length} agents.");
    }
}