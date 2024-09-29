using MapEditor.Utility;

namespace MapEditor.Features.Abstract;

public interface IKeyboardTransformer
{
    void TransformBegin();
    void TransformComplete();
    void Transform(float delta, KeyboardTransformDirection direction);
}