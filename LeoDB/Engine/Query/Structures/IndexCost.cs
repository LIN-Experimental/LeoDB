﻿using System.Linq;
using static LeoDB.Constants;

namespace LeoDB.Engine
{
    /// <summary>
    /// Calculate index cost based on expression/collection index. 
    /// Lower cost is better - lowest will be selected
    /// </summary>
    internal class IndexCost
    {
        public uint Cost { get; }

        /// <summary>
        /// Get filtered expression: "$._id = 10"
        /// </summary>
        public BsonExpression Expression { get; }

        /// <summary>
        /// Get index expression only: "$._id"
        /// </summary>
        public string IndexExpression { get; }

        /// <summary>
        /// Get created Index instance used on query
        /// </summary>
        public Index Index { get; }

        public IndexCost(CollectionIndex index, BsonExpression expr, BsonExpression value, Collation collation)
        {
            this.IndexExpression = index.Expression;
            this.Expression = expr;

            var exprType = expr.Type;

            // if the expression constant is in the left, invert expression type to "normalize" it
            if (expr.Left.IsValue)
            {
                switch (expr.Type)
                {
                    case BsonExpressionType.GreaterThan:
                        exprType = BsonExpressionType.LessThan;
                        break;
                    case BsonExpressionType.GreaterThanOrEqual:
                        exprType = BsonExpressionType.LessThanOrEqual;
                        break;
                    case BsonExpressionType.LessThan:
                        exprType = BsonExpressionType.GreaterThan;
                        break;
                    case BsonExpressionType.LessThanOrEqual:
                        exprType = BsonExpressionType.GreaterThanOrEqual;
                        break;
                    default:
                        break;
                }
            }

            // create index instance
            this.Index = value.Execute(collation).Select(x => this.CreateIndex(exprType, index.Name, x)).FirstOrDefault();

            ENSURE(this.Index != null, "index must be not null");

            // calcs index cost
            this.Cost = this.Index.GetCost(index);
        }

        // used when full index search
        public IndexCost(CollectionIndex index)
        {
            this.Expression = BsonExpression.Create(index.Expression);
            this.Index = new IndexAll(index.Name, Query.Ascending);
            this.Cost = this.Index.GetCost(index);
            this.IndexExpression = index.Expression;
        }

        /// <summary>
        /// Create index based on expression predicate
        /// </summary>
        private Index CreateIndex(BsonExpressionType type, string name, BsonValue value)
        {
            return type switch
            {
                BsonExpressionType.Equal => new IndexEquals(name, value),
                BsonExpressionType.Between => new IndexRange(name, value.AsArray[0], value.AsArray[1], true, true, Query.Ascending),
                BsonExpressionType.Like => new IndexLike(name, value.AsString, Query.Ascending),
                BsonExpressionType.GreaterThan => new IndexRange(name, value, BsonValue.MaxValue, false, true, Query.Ascending),
                BsonExpressionType.GreaterThanOrEqual => new IndexRange(name, value, BsonValue.MaxValue, true, true, Query.Ascending),
                BsonExpressionType.LessThan => new IndexRange(name, BsonValue.MinValue, value, true, false, Query.Ascending),
                BsonExpressionType.LessThanOrEqual => new IndexRange(name, BsonValue.MinValue, value, true, true, Query.Ascending),
                BsonExpressionType.NotEqual => new IndexScan(name, x => x.CompareTo(value) != 0, Query.Ascending),
                BsonExpressionType.In => value.IsArray ?
                                        (Index)new IndexIn(name, value.AsArray, Query.Ascending) :
                                        (Index)new IndexEquals(name, value),
                _ => null,
            };
        }
    }
}