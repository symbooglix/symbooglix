using NUnit.Framework;
using System;
using Microsoft.Boogie;
using Symbooglix;
using System.Collections.Generic;

namespace ConstantFoldingTests
{
    [TestFixture()]
    public class FoldExistsExpr : TestBase
    {
        public FoldExistsExpr()
        {
            // Boogie hits NullPtrException if the cmdline parser
            // isn't setup when printing forallExpr. This is sooo annoying!
            SymbooglixLibTests.SymbooglixTest.setupCmdLineParser();
        }

        [Test()]
        public void isTrue()
        {
            testTruth(Expr.True);
        }

        [Test()]
        public void isFalse()
        {
            testTruth(Expr.False);
        }

        public void testTruth(Expr constantBool)
        {
            Assert.IsInstanceOfType(typeof(LiteralExpr), constantBool);
            var boundVars = new List<Variable>();
            boundVars.Add(new BoundVariable(Token.NoToken, new TypedIdent(Token.NoToken, "bool", constantBool.Type)));
            Expr e = new ExistsExpr(Token.NoToken, boundVars, constantBool);
            e.Typecheck(new TypecheckingContext(this));
            var CFT = new ConstantFoldingTraverser();
            e = CFT.Traverse(e);
            Assert.AreSame(e, constantBool);
            e.Typecheck(new TypecheckingContext(this));
        }

        // FIXME: Write test trying more complicated Expr. Is there a way to generate an Expr Tree from a string
        // that would make tests easier to write.
    }
}
