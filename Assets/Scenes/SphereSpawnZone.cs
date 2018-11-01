using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SphereSpawnZone : SpawnZone {

    [SerializeField]
    bool surfaceOnly;
    
    public override Vector3 SpawnPoint {
        get
        {
            return transform.TransformPoint(
                surfaceOnly ? Random.onUnitSphere : Random.insideUnitSphere
            );
        }
    }
    
    /**
     * gets called once each time a window is drawn
     */
    void OnDrawGizmos () {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, 1f);
    }
}