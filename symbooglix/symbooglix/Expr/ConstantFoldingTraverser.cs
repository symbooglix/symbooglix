using System;
using Microsoft.Boogie;
using System.Diagnostics;
using Action = symbooglix.Traverser.Action;
using System.Numerics;
using Microsoft.Basetypes;

namespace symbooglix
{
    // Clients should use this.
    public class ConstantFoldingTraverser : DFSPostOrderTraverser
    {
        public ConstantFoldingTraverser() : base( new ConstantFoldingVisitor() ) { }
    }

    public class ConstantFoldingVisitor : IExprVisitor
    {
        public Action VisitLiteralExpr(LiteralExpr e)
        {
            // Can't constant fold a literal
            return Traverser.Action.ContinueTraversal(e);
        }

        public Action VisitIdentifierExpr(IdentifierExpr e)
        {
            // This is a symbolic variable so we can't constant fold
            return Traverser.Action.ContinueTraversal(e);
        }

        public Action VisitOldExpr(OldExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitCodeExpr(CodeExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitBvExtractExpr(BvExtractExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitBvConcatExpr(BvConcatExpr e)
        {
            if (e.E0 is LiteralExpr && e.E1 is LiteralExpr)
            {
                var MSB = e.E0 as LiteralExpr;
                var LSB = e.E1 as LiteralExpr;
                Debug.Assert(MSB.Val is BvConst, "MSB is not a BvConst");
                Debug.Assert(LSB.Val is BvConst, "LSB is not a BvConst");
                Debug.WriteLine("Constant folding BvConcatExpr");

                var MSBBV = MSB.Val as BvConst;
                var LSBBV = LSB.Val as BvConst;

                // Compute concatentation
                // Note BigInteger is immutable
                BigInteger MSBShifted = MSBBV.Value.ToBigInteger << LSBBV.Bits;
                BigInteger resultAsBigInt = MSBShifted + LSBBV.Value.ToBigInteger;
                BigNum resultAsBigNum = BigNum.FromBigInt(resultAsBigInt);
                LiteralExpr result = new LiteralExpr(Token.NoToken, resultAsBigNum, MSBBV.Bits + LSBBV.Bits);
                return Traverser.Action.ContinueTraversal(result);
            }
            else
                return Traverser.Action.ContinueTraversal(e);
        }

        public Action VisitForallExpr(ForallExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitExistExpr(ExistsExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitLambdaExpr(LambdaExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitNot(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitNeg(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitAdd(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitSub(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitMul(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitDiv(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitMod(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitRealDiv(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitEq(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitNeq(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitGt(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitGe(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitLt(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitLe(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitAnd(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitOr(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitImp(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitIff(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitSubType(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitMapStore(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitMapSelect(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitIfThenElse(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitFunctionCall(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitTypeCoercion(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action VisitArithmeticCoercion(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvadd(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvsub(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvmul(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvudiv(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvurem(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvsdiv(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvsrem(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvsmod(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_sign_extend(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_zero_extend(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvneg(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvand(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvor(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvnot(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvxor(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvshl(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvlshr(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvashr(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvult(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvule(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvugt(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvuge(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvslt(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvsle(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvsgt(NAryExpr e)
        {
            throw new NotImplementedException();
        }

        public Action Visit_bvsge(NAryExpr e)
        {
            throw new NotImplementedException();
        }
    }
}
