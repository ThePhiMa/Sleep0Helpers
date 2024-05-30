using Sleep0.Logic;
using UnityEditor;

namespace Sleep0.EditorExtensions
{
    [CustomEditor(typeof(PIDAgent))]
    public class PIDAgentEditor : CustomEditorOverlayBase<PIDAgent, PIDAgentOverlay>
    { }
}