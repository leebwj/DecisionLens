using UnityEngine;

namespace DecisionLens {

    // anything with a collider that blocks the line of sight
    public class SightObstacle : MonoBehaviour {

        public bool Blocks(Ray sight, float maxDistance, out float hitDistance) {
            hitDistance = 0f;
            Collider shape = GetComponentInChildren<Collider>();

            if (shape == null || !shape.Raycast(sight, out RaycastHit hit, maxDistance)) {
                return false;
            }
            hitDistance = hit.distance;
            return true;
        }
    }
}
