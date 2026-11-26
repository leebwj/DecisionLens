using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DecisionLens {

    // dashed scene lines like Unity's gizmos, made from line renderers since real gizmos only draw in the editor
    public class DecisionGizmos : MonoBehaviour {

        private const int CIRCLE_POINTS = 96;
        private const float GROUND_LIFT = 0.03f;
        private const float RING_MARGIN = 0.25f;

        [SerializeField] private Material dashedLine;
        [SerializeField] private float width = 0.05f;

        [Header("Colors")]
        [SerializeField] private Color radiusColor = new Color32(0x7B, 0xAE, 0xFA, 0xFF);
        [SerializeField] private Color clearColor = new Color32(0x9B, 0xC5, 0xA6, 0xFF);
        [SerializeField] private Color blockedColor = new Color32(0xE9, 0xA2, 0x9C, 0xFF);
        [SerializeField] private Color behindWallColor = new Color32(0x8C, 0x8C, 0x8C, 0xFF);
        [SerializeField] private Color selectionColor = new Color32(0xEE, 0xEE, 0xEE, 0xFF);

        private LineRenderer radius;
        private LineRenderer selection;
        private readonly List<LineRenderer> sightLines = new List<LineRenderer>();
        private int sightLinesUsed;

        void Awake() {
            radius = NewCircle("Detection Radius");
            selection = NewCircle("Selection");
        }

        // a decision maker gets its radius and all its sight lines, anything else only the sight lines it is part of
        public void Draw(GameObject selected, List<Decision> decisions) {
            radius.enabled = false;
            sightLinesUsed = 0;

            if (selected != null) {
                foreach (Decision decision in decisions) {
                    DrawDecision(decision, selected);
                }
            }

            for (int i = sightLinesUsed; i < sightLines.Count; i++) {
                sightLines[i].enabled = false;
            }
            DrawSelection(selected);
        }

        private void DrawDecision(Decision decision, GameObject selected) {
            DecisionMaker maker = decision.maker;
            bool ownDecision = maker.gameObject == selected;

            if (ownDecision) {
                radius.enabled = true;
                SetCircle(radius, maker.transform.position, maker.DetectionRadius, radiusColor);
            }

            foreach (CandidateResult result in decision.results) {
                if (ownDecision || result.Involves(selected)) {
                    DrawSight(maker, result);
                }
            }
        }

        // green when clear, red up to the obstacle and grey behind it when blocked
        private void DrawSight(DecisionMaker maker, CandidateResult result) {
            Vector3 end = result.candidate.SightPoint;

            if (result.CanSee) {
                AddLine(maker.Eye, end, clearColor);
            } else {
                AddLine(maker.Eye, result.blockedAt, blockedColor);
                AddLine(result.blockedAt, end, behindWallColor);
            }
        }

        // a ring around the selection, skipped for things like the floor that aren't part of any decision
        private void DrawSelection(GameObject selected) {
            Renderer[] parts = TakesPart(selected) ? selected.GetComponentsInChildren<Renderer>() : new Renderer[0];

            selection.enabled = parts.Length > 0;
            if (parts.Length == 0) {
                return;
            }

            Bounds bounds = parts[0].bounds;
            foreach (Renderer part in parts) {
                bounds.Encapsulate(part.bounds);
            }

            Vector3 center = new Vector3(bounds.center.x, selected.transform.position.y, bounds.center.z);
            float size = new Vector2(bounds.extents.x, bounds.extents.z).magnitude + RING_MARGIN;
            SetCircle(selection, center, size, selectionColor);
        }

        private static bool TakesPart(GameObject thing) {
            return thing != null && (thing.GetComponent<DecisionMaker>() != null
                || thing.GetComponent<DecisionCandidate>() != null
                || thing.GetComponent<SightObstacle>() != null);
        }

        private void AddLine(Vector3 from, Vector3 to, Color color) {
            if (sightLinesUsed == sightLines.Count) {
                sightLines.Add(NewLine("Sight Line"));
            }

            LineRenderer line = sightLines[sightLinesUsed++];
            line.enabled = true;
            line.startColor = color;
            line.endColor = color;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
        }

        // lifted a little so the floor doesn't hide it
        private void SetCircle(LineRenderer circle, Vector3 center, float size, Color color) {
            circle.startColor = color;
            circle.endColor = color;

            for (int i = 0; i < CIRCLE_POINTS; i++) {
                float angle = i * 2f * Mathf.PI / CIRCLE_POINTS;
                circle.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * size, GROUND_LIFT, Mathf.Sin(angle) * size));
            }
        }

        private LineRenderer NewLine(string lineName) {
            GameObject holder = new GameObject(lineName);
            holder.transform.SetParent(transform, false);

            LineRenderer line = holder.AddComponent<LineRenderer>();
            line.sharedMaterial = dashedLine;
            line.textureMode = LineTextureMode.Tile;
            line.widthMultiplier = width;
            line.positionCount = 2;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        // lies flat on the ground instead of turning to face the camera
        private LineRenderer NewCircle(string circleName) {
            LineRenderer circle = NewLine(circleName);
            circle.loop = true;
            circle.positionCount = CIRCLE_POINTS;
            circle.alignment = LineAlignment.TransformZ;
            circle.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            return circle;
        }
    }
}
