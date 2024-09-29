using System.Linq;
using System.Text;

namespace MapEditor.Features.Abstract.StateSteps;

public sealed record CompoundSteps(params IStateStep[] Steps) : IStateStep
{
    public void Do() {
        foreach (var step in Steps) {
            step.Do();
        }
    }

    public void Undo() {
        foreach (var step in Steps.Reverse()) {
            step.Undo();
        }
    }

#if DEBUG
    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append("CompoundSteps [");
        foreach (var step in Steps) {
            sb.AppendLine();
            sb.Append(step);
        }

        sb.AppendLine();
        sb.Append(']');
        return sb.ToString();
    }
#endif
}
