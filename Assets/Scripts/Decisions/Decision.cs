using System.Collections.Generic;
using UnityEngine;

namespace DecisionLens {

    // what one decision maker decided, remade every frame
    public class Decision {
        public DecisionMaker maker;

        // one per candidate, in the order they were given
        public List<CandidateResult> results = new List<CandidateResult>();

        // null when no candidate passes
        public CandidateResult target;
    }

    // how one candidate did against the decision maker's rules
    public class CandidateResult {
        public DecisionCandidate candidate;
        public float distance;
        public bool inRange;
        public SightObstacle blockedBy;
        public Vector3 blockedAt;

        public bool CanSee => blockedBy == null;
        public bool Passes => inRange && CanSee;

        public bool IsBlockedBy(GameObject thing) => blockedBy != null && blockedBy.gameObject == thing;
        public bool Involves(GameObject thing) => candidate.gameObject == thing || IsBlockedBy(thing);
    }
}
