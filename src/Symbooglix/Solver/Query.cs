﻿using System;
using Microsoft.Boogie;

namespace Symbooglix
{
    namespace Solver
    {
        public class Query
        {
            public IConstraintManager Constraints { get; private set;}
            public Constraint QueryExpr { get; private set;}
            public Query(IConstraintManager constraints, Constraint queryExpr)
            {
                this.Constraints = constraints;
                this.QueryExpr = queryExpr;
            }

            public Query WithNegatedQueryExpr(IExprBuilder builder)
            {
                var that = (Query) this.MemberwiseClone();
                that.QueryExpr = this.QueryExpr.GetNegatedConstraint(builder);
                return that;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                var other = obj as Query;
                if (other == null)
                    return false;

                if (Constraints.Count != other.Constraints.Count)
                    return false;

                return this.QueryExpr.Equals(other.QueryExpr) && this.Constraints.Equals(other.Constraints);
            }

            public override int GetHashCode()
            {
                return 33 * Constraints.GetHashCode() + QueryExpr.GetHashCode();
            }
        }
    }
}

