using System;

namespace Mongo.Context.Internal
{
    public class PropertyGetter<TEntity, TValue> : IPropertyGetter
    {
        private readonly Func<TEntity, TValue> _getter;

        public PropertyGetter(Func<TEntity, TValue> getter)
        {
            _getter = getter;
        }

        public virtual object GetValue(object instance)
        {
            return _getter((TEntity)instance);
        }
    }
}
