using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace DecisionLens {

    // the Decision Lens panel on the right and the target bar under the scene, always about the selected object
    public class DecisionPanel {

        private readonly VisualElement content;
        private readonly VisualElement hint;
        private readonly Label selectedName;
        private readonly VisualElement notes;
        private readonly VisualElement table;
        private readonly VisualElement rows;
        private readonly VisualElement targetInfo;
        private readonly Label targetName;

        public DecisionPanel(VisualElement root) {
            content = root.Q("lens-content");
            hint = root.Q("lens-empty");
            selectedName = root.Q<Label>("selected-name");
            notes = root.Q("lens-notes");
            table = root.Q("decision-table");
            rows = root.Q("candidate-rows");
            targetInfo = root.Q("target-info");
            targetName = root.Q<Label>("target-name");
        }

        public void Show(GameObject selected, List<Decision> decisions) {
            notes.Clear();
            rows.Clear();
            Decision own = selected != null ? decisions.Find(d => d.maker.gameObject == selected) : null;

            content.EnableInClassList("is-hidden", selected == null);
            hint.EnableInClassList("is-hidden", selected != null);
            table.EnableInClassList("is-hidden", own == null);
            targetInfo.EnableInClassList("is-blank", own == null);

            if (selected == null) {
                return;
            }

            selectedName.text = selected.name;
            if (own != null) {
                ShowTable(own);
            } else {
                ShowNotes(selected, decisions);
            }
        }

        // a decision maker gets its candidates, target first
        private void ShowTable(Decision decision) {
            targetName.text = decision.target != null ? decision.target.candidate.name : "None";

            if (decision.target != null) {
                AddRow(decision.target, true);
            }
            foreach (CandidateResult result in decision.results) {
                if (result != decision.target) {
                    AddRow(result, false);
                }
            }
        }

        // anything else gets a line for each decision it is part of
        private void ShowNotes(GameObject selected, List<Decision> decisions) {
            foreach (Decision decision in decisions) {
                string line = Explanation.For(decision, selected);
                if (line != null) {
                    AddNote(line);
                }
            }
            if (notes.childCount == 0) {
                AddNote(Explanation.NotPartOfAnything(selected));
            }
        }

        private void AddNote(string text) {
            Label note = new Label(text);
            note.AddToClassList("lens__note");
            notes.Add(note);
        }

        private void AddRow(CandidateResult result, bool isTarget) {
            VisualElement row = new VisualElement();
            row.AddToClassList("table__row");
            row.EnableInClassList("table__row--target", isTarget);

            row.Add(Cell("candidate", result.candidate.name));
            row.Add(Cell("distance", result.distance.ToString("0.0", CultureInfo.InvariantCulture)));
            row.Add(Cell("sight", result.CanSee ? "Clear" : "Blocked"));
            row.Add(Cell("priority", result.candidate.Priority.ToString(CultureInfo.InvariantCulture)));
            row.Add(Cell("result", isTarget ? "Target" : "Rejected"));
            rows.Add(row);
        }

        private static Label Cell(string column, string text) {
            Label cell = new Label(text);
            cell.AddToClassList("table__cell");
            cell.AddToClassList("table__cell--" + column);
            return cell;
        }
    }
}
