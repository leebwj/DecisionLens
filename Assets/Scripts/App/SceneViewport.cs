using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace DecisionLens {

    // the middle of the window, where the scene camera draws and clicks pick objects
    public class SceneViewport {

        private readonly VisualElement area;
        private readonly Camera camera;

        public event Action<GameObject> Clicked;

        public SceneViewport(VisualElement area, Camera camera) {
            this.area = area;
            this.camera = camera;

            // layout may already be done, and then no geometry event comes
            FitCamera();
            area.RegisterCallback<GeometryChangedEvent>(evt => FitCamera());
            area.RegisterCallback<PointerDownEvent>(evt => Clicked?.Invoke(ObjectAt(evt.localPosition)));
        }

        // null over empty ground
        private GameObject ObjectAt(Vector2 point) {
            Rect size = area.contentRect;
            Ray ray = camera.ViewportPointToRay(new Vector3(point.x / size.width, 1f - point.y / size.height, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit)) {
                return hit.collider.transform.root.gameObject;
            }
            return null;
        }

        // keeps the 3D view inside this area instead of behind the whole window
        private void FitCamera() {
            if (area.panel == null) {
                return;
            }

            Rect view = area.worldBound;
            Rect window = area.panel.visualTree.worldBound;

            // sizes are NaN before the first layout, and NaN > 0 is false
            bool laidOut = view.width > 0f && view.height > 0f;
            if (!laidOut) {
                return;
            }

            // ui y goes down, camera y goes up
            camera.rect = new Rect(
                view.xMin / window.width,
                1f - view.yMax / window.height,
                view.width / window.width,
                view.height / window.height);
        }
    }
}
