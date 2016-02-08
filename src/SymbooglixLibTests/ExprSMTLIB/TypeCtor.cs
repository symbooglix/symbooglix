//------------------------------------------------------------------------------
//                                  Symbooglix
//
//
// Copyright 2014-2015 Daniel Liew
//
// This file is licensed under the 2-Clause BSD license.
// See LICENSE for details.
//------------------------------------------------------------------------------
using Microsoft.Boogie;
using NUnit.Framework;
using Symbooglix;
using Symbooglix.Annotation;
using System;
using System.Collections.Generic;
using System.IO;


namespace ExprSMTLIBTest
{
    [TestFixture()]
    public class TypeConstructor : ExprSMTLIBTestBase
    {
        [Test()]
        public void NoArguments()
        {
            var tcDecl = new TypeCtorDecl(Token.NoToken, "fox", 0);
            var tc = new CtorType(Token.NoToken, tcDecl, new List <Microsoft.Boogie.Type>());
            var tcTypeIdent = new TypedIdent(Token.NoToken, "fox", tc);
            var gv = new GlobalVariable(Token.NoToken, tcTypeIdent);

            // FIXME: The Symbolic constructor shouldn't really need the program location
            gv.SetMetadata<ProgramLocation>((int) AnnotationIndex.PROGRAM_LOCATION, new ProgramLocation(gv));
            var sym = new SymbolicVariable("y", gv);

            var builder = new SimpleExprBuilder(/*immutable=*/ true);
            var eq = builder.Eq(sym.Expr, sym.Expr);
            Assert.IsNotInstanceOf<LiteralExpr>(eq); // Check that it wasn't constant folded

            using (var writer = new StringWriter())
            {
                var printer = GetPrinter(writer);
                printer.AddDeclarations(eq);
                printer.PrintSortDeclarations();
                Assert.AreEqual("(declare-sort @fox)", writer.ToString().Trim());
            }
        }

        [Test(),ExpectedException(typeof(NotSupportedException))]
        public void Arguments()
        {
            var tcDecl = new TypeCtorDecl(Token.NoToken, "fox", 1);
            var tc = new CtorType(Token.NoToken, tcDecl, new List <Microsoft.Boogie.Type>() { Microsoft.Boogie.Type.Bool});
            var tcTypeIdent = new TypedIdent(Token.NoToken, "fox", tc);
            var gv = new GlobalVariable(Token.NoToken, tcTypeIdent);

            // FIXME: The Symbolic constructor shouldn't really need the program location
            gv.SetMetadata<ProgramLocation>((int) AnnotationIndex.PROGRAM_LOCATION, new ProgramLocation(gv));
            var sym = new SymbolicVariable("y", gv);

            var builder = new SimpleExprBuilder(/*immutable=*/ true);
            var eq = builder.Eq(sym.Expr, sym.Expr);
            Assert.IsNotInstanceOf<LiteralExpr>(eq); // Check that it wasn't constant folded

            using (var writer = new StringWriter())
            {
                var printer = GetPrinter(writer);
                printer.AddDeclarations(eq);
                printer.PrintSortDeclarations();
                Assert.AreEqual("(declare-sort @fox)\n", writer.ToString());
            }
        }
    }
}

