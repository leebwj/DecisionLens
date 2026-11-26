using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DecisionLens {

    // ties the panels, the scene view and the gizmos together, and has every decision maker decide each frame
    [RequireComponent(typeof(UIDocument))]
    public class LensApp : MonoBehaviour {

        [SerializeField] private Camera sceneCamera;
        [SerializeField] private DecisionGizmos gizmos;

        private HierarchyPanel hierarchy;
        private DecisionPanel decisionPanel;
        private SceneViewport viewport;
        private List<DecisionMaker> makers;
        private List<DecisionCandidate> candidates;
        private List<SightObstacle> obstacles;

        public GameObject Selected { get; private set; }

        // one per decision maker, remade every frame
        public List<Decision> Decisions { get; } = new List<Decision>();

        void Start() {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            makers = FindInScene<DecisionMaker>();
            candidates = FindInScene<DecisionCandidate>();
            obstacles = FindInScene<SightObstacle>();

            hierarchy = new HierarchyPanel(root);
            hierarchy.Show(gameObject.scene.name, SceneObjects());
            hierarchy.Clicked += Select;

            decisionPanel = new DecisionPanel(root);

            viewport = new SceneViewport(root.Q("viewport"), sceneCamera);
            viewport.Clicked += Select;
        }

        // null clears the selection
        public void Select(GameObject target) {
            Selected = target;
            hierarchy.SetSelected(target);
        }

        // LateUpdate so anything moved this frame is judged where it ended up
        void LateUpdate() {
            Decisions.Clear();
            foreach (DecisionMaker maker in makers) {
                Decisions.Add(maker.Decide(candidates, obstacles));
            }

            decisionPanel.Show(Selected, Decisions);
            gizmos.Draw(Selected, Decisions);
        }

        // everything in the scene except this app object
        private List<GameObject> SceneObjects() {
            List<GameObject> objects = new List<GameObject>(gameObject.scene.GetRootGameObjects());
            objects.Remove(gameObject);
            return objects;
        }

        // in hierarchy order, which FindObjectsByType doesn't promise
        private List<T> FindInScene<T>() where T : Component {
            List<T> found = new List<T>();
            foreach (GameObject sceneObject in gameObject.scene.GetRootGameObjects()) {
                found.AddRange(sceneObject.GetComponentsInChildren<T>());
            }
            return found;
        }
    }
}
