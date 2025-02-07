namespace UnBox3D.Controls.States
{
    public interface IState
    {
        void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e);
        void OnMouseMove(System.Windows.Input.MouseEventArgs e);
        void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e);
    }
}