using System.Linq.Expressions;

namespace Sakuga;

public class ModelAccessor<TModel>
    where TModel: class
{
    private readonly TModel _model;

    internal ModelAccessor(TModel model)
    {
        _model = model;
    }

    public T Get<T>(Expression<Func<TModel, T>> expression)
    {
        var func = expression.Compile();
        return func(_model);
    }
}
