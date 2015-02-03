using System;
using Microsoft.Boogie;
using Microsoft.Basetypes;

namespace Symbooglix
{   
    namespace Solver
    {
        /// <summary>
        /// This solver considers every query to "resultIsAlways"
        /// with the same assignment for everything.
        /// 
        /// It is meant for testing only.
        /// </summary>
        public class DummySolver : ISolverImpl
        {
            private Result ResultIsAlways;

            public DummySolver(Solver.Result resultIsAlways = Result.SAT)
            {
                this.ResultIsAlways = resultIsAlways;
            }

            public void SetConstraints(IConstraintManager cm)
            {
                // The dummy solver doesn't care about these 
            }

            public void SetTimeout(int seconds)
            {
                // The dummy solver doesn't care about this
            }
            public void Interrupt()
            {
                // Dummy solver doesn't need to do anything
            }

            public Tuple<Result, IAssignment> ComputeSatisfiability(Expr queryExpr, bool computeAssignment)
            {
                // Dummy Solver thinks everything is satisfiable
                if (computeAssignment)
                    return Tuple.Create(ResultIsAlways, new DummyAssignment(0) as IAssignment);
                else
                    return Tuple.Create(ResultIsAlways, null as IAssignment);
            }

            public void Dispose()
            {
                // Dummy solver doesn't need to clean up anything
                return;
            }

            public ISolverImplStatistics Statistics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
                
            private class DummyAssignment : IAssignment
            {
                private int defaultValue;
                public DummyAssignment(int defaultValue)
                {
                    this.defaultValue = defaultValue;
                }

                public Microsoft.Boogie.LiteralExpr GetAssignment(SymbolicVariable SV)
                {
                    if (SV.TypedIdent.Type.IsBv)
                    {
                        int width = SV.TypedIdent.Type.BvBits;
                        return new LiteralExpr(Token.NoToken, BigNum.FromInt(defaultValue), width);
                    }
                    else if (SV.TypedIdent.Type.IsBool)
                    {
                        return defaultValue > 0 ? Expr.True : Expr.False;
                    }
                    else if (SV.TypedIdent.Type.IsInt)
                    {
                        return new LiteralExpr(Token.NoToken, BigNum.FromInt(defaultValue));
                    }
                    else if (SV.TypedIdent.Type.IsReal)
                    {
                        return new LiteralExpr(Token.NoToken, BigDec.FromInt(defaultValue));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }



    }
}

