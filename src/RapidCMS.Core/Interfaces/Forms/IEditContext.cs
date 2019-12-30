﻿using System;
using System.Linq.Expressions;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Interfaces.Data;

namespace RapidCMS.Core.Interfaces.Forms
{
    public interface IEditContext<TEntity> where TEntity : IEntity
    {
        UsageType UsageType { get; }
        EntityState EntityState { get; }

        TEntity Entity { get; }
        IParent? Parent { get; }

        IRelationContainer GetRelationContainer();
        bool? IsModified<TValue>(Expression<Func<TEntity, TValue>> property);
        bool? IsModified(string propertyName);

        bool? IsValid<TValue>(Expression<Func<TEntity, TValue>> property);
        bool? IsValid(string propertyName);

        bool? WasValidated<TValue>(Expression<Func<TEntity, TValue>> property);
        bool? WasValidated(string propertyName);
    }
}
