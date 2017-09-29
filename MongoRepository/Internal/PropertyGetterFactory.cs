using System;
using System.Reflection;

namespace Mongo.Context.Internal
{
    public class PropertyGetterFactory : AccessorFactory<IPropertyGetter>
    {
        public override IPropertyGetter Create(PropertyInfo property)
        {
            var types = new[] { property.DeclaringType, property.PropertyType };
            var getterType = typeof(PropertyGetter<,>).MakeGenericType(types);
            var funcType = typeof(Func<,>).MakeGenericType(types);
            return (IPropertyGetter)Activator.CreateInstance(getterType, property.GetMethod.CreateDelegate(funcType));
        }
    }
}
