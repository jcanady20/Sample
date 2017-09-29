﻿using System.Reflection;
using Mongo.Context.Extensions;

namespace Mongo.Context.Internal
{
    public abstract class AccessorFactory<TAccessor>
        where TAccessor : class
    {
        public virtual TAccessor Create(IPropertyBase property)
        {
            return property as TAccessor ?? Create(property.DeclaringEntityType.ClrType.GetAnyProperty(property.Name));
        }

        public abstract TAccessor Create(PropertyInfo property);
    }
}
