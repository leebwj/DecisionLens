using System.Collections.Generic;
using UnityEngine;

namespace DecisionLens {

    // every sentence the lens shows, in one place so the wording is easy to change
    public static class Explanation {

        public static string NotPartOfAnything(GameObject thing) {
            return $"{thing.name} does not make or take part in any decisions.";
        }

        // how something fits into someone else's decision, null if it isn't part of it
        public static string For(Decision decision, GameObject thing) {
            CandidateResult result = decision.results.Find(r => r.candidate.gameObject == thing);
            if (result != null) {
                return ForCandidate(decision, result);
            }
            if (thing.GetComponent<SightObstacle>() != null) {
                return ForObstacle(decision, thing);
            }
            return null;
        }

        private static string ForCandidate(Decision decision, CandidateResult result) {
            string maker = decision.maker.name;
            string name = result.candidate.name;
            CandidateResult target = decision.target;

            if (result == target) {
                return $"{maker} picked {name} as its target.";
            }
            if (!result.inRange) {
                return $"{maker} rejected {name}: it is out of range.";
            }
            if (!result.CanSee) {
                return $"{maker} rejected {name}: {result.blockedBy.name} blocks the line of sight.";
            }
            if (target.candidate.Priority > result.candidate.Priority) {
                return $"{maker} rejected {name}: {target.candidate.name} has a higher priority.";
            }
            return $"{maker} rejected {name}: {target.candidate.name} is closer.";
        }

        private static string ForObstacle(Decision decision, GameObject obstacle) {
            List<string> hidden = new List<string>();
            foreach (CandidateResult result in decision.results) {
                if (result.IsBlockedBy(obstacle)) {
                    hidden.Add(result.candidate.name);
                }
            }

            if (hidden.Count == 0) {
                return $"{obstacle.name} is not blocking {decision.maker.name}'s view of anything.";
            }
            return $"{obstacle.name} blocks {decision.maker.name}'s view of {JoinNames(hidden)}.";
        }

        // "A", "A and B", "A, B and C"
        private static string JoinNames(List<string> names) {
            if (names.Count == 1) {
                return names[0];
            }
            return string.Join(", ", names.GetRange(0, names.Count - 1)) + " and " + names[names.Count - 1];
        }
    }
}
