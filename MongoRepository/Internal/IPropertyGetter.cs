namespace Mongo.Context.Internal
{
    public interface IPropertyGetter
    {
        object GetValue(object instance);
    }
}
