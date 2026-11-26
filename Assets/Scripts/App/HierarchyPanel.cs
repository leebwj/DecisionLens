using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DecisionLens {

    // the list of scene objects on the left
    public class HierarchyPanel {

        private readonly Label sceneName;
        private readonly VisualElement rows;

        public event Action<GameObject> Clicked;

        public HierarchyPanel(VisualElement root) {
            sceneName = root.Q<Label>("scene-name");
            rows = root.Q("hierarchy-rows");
        }

        public void Show(string scene, IReadOnlyList<GameObject> objects) {
            sceneName.text = scene;
            rows.Clear();

            foreach (GameObject sceneObject in objects) {
                Label row = new Label(sceneObject.name);
                row.userData = sceneObject;
                row.AddToClassList("hierarchy__row");
                row.RegisterCallback<PointerDownEvent>(evt => Clicked?.Invoke(sceneObject));
                rows.Add(row);
            }
        }

        public void SetSelected(GameObject selected) {
            foreach (VisualElement row in rows.Children()) {
                row.EnableInClassList("hierarchy__row--selected", row.userData == (object)selected);
            }
        }
    }
}
