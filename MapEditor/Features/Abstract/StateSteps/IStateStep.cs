namespace MapEditor.Features.Abstract.StateSteps;

public interface IStateStep
{
    void Do();
    void Undo();
}