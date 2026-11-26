using System.Collections.Generic;
using UnityEngine;

namespace DecisionLens {

    // picks a target: it has to be in range and in sight, then the highest priority wins, the nearer one on a tie
    public class DecisionMaker : MonoBehaviour {

        [SerializeField] private float detectionRadius = 15f;
        [SerializeField] private float eyeHeight = 1f;

        public float DetectionRadius => detectionRadius;
        public Vector3 Eye => transform.position + Vector3.up * eyeHeight;

        public Decision Decide(List<DecisionCandidate> candidates, List<SightObstacle> obstacles) {
            // colliders don't follow objects moved this frame until synced
            Physics.SyncTransforms();

            Decision decision = new Decision();
            decision.maker = this;

            foreach (DecisionCandidate candidate in candidates) {
                CandidateResult result = Check(candidate, obstacles);
                decision.results.Add(result);

                if (result.Passes && (decision.target == null || Beats(result, decision.target))) {
                    decision.target = result;
                }
            }
            return decision;
        }

        private CandidateResult Check(DecisionCandidate candidate, List<SightObstacle> obstacles) {
            CandidateResult result = new CandidateResult();
            result.candidate = candidate;
            result.distance = Vector3.Distance(transform.position, candidate.transform.position);
            result.inRange = result.distance <= detectionRadius;

            Vector3 toCandidate = candidate.SightPoint - Eye;
            Ray sight = new Ray(Eye, toCandidate);
            float nearestHit = toCandidate.magnitude;

            // each hit shortens the ray, so the nearest obstacle ends up as the blocker
            foreach (SightObstacle obstacle in obstacles) {
                if (obstacle.Blocks(sight, nearestHit, out float hitDistance)) {
                    nearestHit = hitDistance;
                    result.blockedBy = obstacle;
                    result.blockedAt = sight.GetPoint(hitDistance);
                }
            }
            return result;
        }

        private static bool Beats(CandidateResult a, CandidateResult b) {
            if (a.candidate.Priority != b.candidate.Priority) {
                return a.candidate.Priority > b.candidate.Priority;
            }
            return a.distance < b.distance;
        }
    }
}
