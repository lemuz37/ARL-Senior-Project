namespace UnBox3D.Commands
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}