﻿using LeoDB.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using static LeoDB.Constants;

namespace LeoDB
{
    internal class ExpressionContext
    {
        public ExpressionContext()
        {
            this.Source = Expression.Parameter(typeof(IEnumerable<BsonDocument>), "source");
            this.Root = Expression.Parameter(typeof(BsonDocument), "root");
            this.Current = Expression.Parameter(typeof(BsonValue), "current");
            this.Collation = Expression.Parameter(typeof(Collation), "collation");
            this.Parameters = Expression.Parameter(typeof(BsonDocument), "parameters");
        }

        public ParameterExpression Source { get; }
        public ParameterExpression Root { get; }
        public ParameterExpression Current { get; }
        public ParameterExpression Collation { get; }
        public ParameterExpression Parameters { get; }
    }
}
