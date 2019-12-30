﻿using System;
using System.Linq.Expressions;

namespace RapidCMS.Core.Interfaces.Metadata
{
    public interface IPropertyMetadata
    {
        Type PropertyType { get; }
        string PropertyName { get; }
        Type ObjectType { get; }
        Func<object, object> Getter { get; }

        string Fingerprint { get; }

        LambdaExpression OriginalExpression { get; }
    }
}
