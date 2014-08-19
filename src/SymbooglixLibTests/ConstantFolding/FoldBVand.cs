﻿using NUnit.Framework;
using System;
using Symbooglix;
using Microsoft.Boogie;
using Microsoft.Basetypes;

namespace ConstantFoldingTests
{
    [TestFixture()]
    public class FoldBVAnd : TestBase
    {
        [Test()]
        public void Disjoint()
        {
            helper(5, 10, 0);
        }

        [Test()]
        public void OneSharedBit()
        {
            helper(3, 2, 2);
        }

        [Test()]
        public void MaskOffOdd()
        {
            helper(11, 14, 10);
        }

        private void helper(int value0, int value1, int expectedValue)
        {
            var simple = builder.ConstantBV(value0, 4);
            var simple2 = builder.ConstantBV(value1, 4);
            var expr = builder.BVAND(simple, simple2);
            expr.Typecheck(new TypecheckingContext(this));
            var CFT = new ConstantFoldingTraverser();
            var result = CFT.Traverse(expr);

            Assert.IsInstanceOfType(typeof(LiteralExpr), result);
            Assert.IsTrue(( result as LiteralExpr ).isBvConst);
            Assert.AreEqual(BigNum.FromInt(expectedValue), ( result as LiteralExpr ).asBvConst.Value);
        }
    }
}

