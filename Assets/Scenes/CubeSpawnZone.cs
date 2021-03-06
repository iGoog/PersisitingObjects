using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CubeSpawnZone : SpawnZone {

    [SerializeField]
    bool surfaceOnly;
    
    public override Vector3 SpawnPoint {
        get
        {
            Vector3 p;
            p.x = Random.Range(-0.5f, 0.5f);
            p.y = Random.Range(-0.5f, 0.5f);
            p.z = Random.Range(-0.5f, 0.5f);
            if (surfaceOnly) {
                int axis = Random.Range(0, 3);
            }
            return transform.TransformPoint(p);
        }
    }
    
    /**
     * gets called once each time a window is drawn
     */
    void OnDrawGizmos () {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}