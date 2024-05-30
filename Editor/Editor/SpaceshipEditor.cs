using Sleep0.Logic.Game;
using UnityEditor;

namespace Sleep0.EditorExtensions
{
    [CustomEditor(typeof(Spaceship), true)]
    public class SpaceshipEditor : CustomEditorOverlayBase<Spaceship, SpaceshipOverlay>
    {
        //protected override void InitializeOverlay()
        //{
        //    //throw new System.NotImplementedException();
        //}
    }
}