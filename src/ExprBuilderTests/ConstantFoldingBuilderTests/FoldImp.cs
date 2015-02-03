﻿using System;
using NUnit.Framework;
using Symbooglix;
using Microsoft.Boogie;

namespace ExprBuilderTests.ConstantFoldingTests
{
    [TestFixture()]
    public class FoldImp : ConstantFoldingExprBuilderTests
    {
        [Test()]
        public void TrueAntecedent()
        {
            var cfb = GetConstantFoldingBuilder();
            var variable = GetVarAndIdExpr("foo", BasicType.Bool).Item2;
            var result = cfb.Imp(cfb.True, variable);
            Assert.AreSame(variable, result);
        }

        [Test()]
        public void FalseAntecedent()
        {
            var cfb = GetConstantFoldingBuilder();
            var variable = GetVarAndIdExpr("foo", BasicType.Bool).Item2;
            var result = cfb.Imp(cfb.False, variable);
            Assert.IsTrue(ExprUtil.IsTrue(result));
        }

        [Test()]
        public void NoFold()
        {
            var pair = GetSimpleAndConstantFoldingBuilder();
            var sb = pair.Item1;
            var cfb = pair.Item2;
            var variable0 = GetVarAndIdExpr("foo", BasicType.Bool).Item2;
            var variable1 = GetVarAndIdExpr("foo2", BasicType.Bool).Item2;
            var foldedResult = cfb.Imp(variable0, variable1);
            var simpleResult = sb.Imp(variable0, variable1);
            Assert.AreEqual(simpleResult, foldedResult);
            Assert.IsNotNull(ExprUtil.AsImp(foldedResult));
        }
    }
}

