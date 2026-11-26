using UnityEngine;

namespace DecisionLens {

    // something a decision maker can pick as its target
    public class DecisionCandidate : MonoBehaviour {

        [SerializeField] private int priority = 1;
        [SerializeField] private float sightHeight = 1f;

        public int Priority {
            get => priority;
            set => priority = value;
        }

        // the point a decision maker looks at
        public Vector3 SightPoint => transform.position + Vector3.up * sightHeight;
    }
}
