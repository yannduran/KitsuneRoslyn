﻿// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.TestHelpers;
using BlackFox.Roslyn.Diagnostics.TestHelpers.DiagnosticTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.PropertyConversions
{
    [TestClass]
    public class PropertyAnalyzerTests
    {
        [TestMethod]
        public void Get_return_constant()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer, "public int X { get { return 42; } }");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(PropertyAnalyzer.IdToExpression, PropertyAnalyzer.IdToInitializer);
        }

        [TestMethod]
        public void Get_return_expression_static_1()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "public int X { get { return Math.Sin(42); } }");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(PropertyAnalyzer.IdToExpression, PropertyAnalyzer.IdToInitializer);
        }

        [TestMethod]
        public void Get_return_expression_static_2()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "public static int x = 42; public int X { get { return Math.Sin(x); } }");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(PropertyAnalyzer.IdToExpression, PropertyAnalyzer.IdToInitializer);
        }

        [TestMethod]
        public void Get_return_expression_instance()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "public int x = 42; public int X { get { return Math.Sin(x); } }");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(PropertyAnalyzer.IdToExpression);
        }

        [TestMethod]
        public void Get_set_auto_implemented()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer, "public int X { get; set; }");
            Check.That(diagnostics).IsEmpty();
        }

        [TestMethod]
        public void Get_set()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer, "public int X { get { return 42; } set {} }");
            Check.That(diagnostics).IsEmpty();
        }

        [TestMethod]
        public void Expression_constant()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer, "public int X => 42;");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(PropertyAnalyzer.IdToStatement, PropertyAnalyzer.IdToInitializer);
        }

        [TestMethod]
        public void Expression_expression_static_1()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer, "public int X => Math.Sin(42);");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(PropertyAnalyzer.IdToStatement, PropertyAnalyzer.IdToInitializer);
        }

        [TestMethod]
        public void Expression_expression_static_2()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "public static int x = 42; public int SinX => Math.Sin(x);");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(PropertyAnalyzer.IdToStatement, PropertyAnalyzer.IdToInitializer);
        }

        [TestMethod]
        public void Expression_expression_instance()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "public int x = 42; public int SinX => Math.Sin(x);");
            var ids = diagnostics.Select(d => d.Id);

            // Intializer is impossible as no instance reference can exist in their scope
            Check.That(ids).ContainExactlyAnyOrder(PropertyAnalyzer.IdToStatement);
        }

        [TestMethod]
        public void Initializer_constant()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer, "public int X {get;} = 42;");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(PropertyAnalyzer.IdToStatement, PropertyAnalyzer.IdToExpression);
        }

        [TestMethod]
        public void Initializer_expression()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "public int X {get;} = Math.Sin(42);");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(PropertyAnalyzer.IdToStatement, PropertyAnalyzer.IdToExpression);
        }

        [TestMethod]
        public void Initializer_primary_constructor_argument()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnostics(analyzer,
                "class Foo(int x) { public int X {get;} = x;}");
            Check.That(diagnostics).IsEmpty();
        }

        [TestMethod]
        public void Initializer_primary_constructor_argument_deep()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnostics(analyzer,
                "class Foo(int x) { public int X {get;} = (int)System.Math.Sin(x+1);}");
            Check.That(diagnostics).IsEmpty();
        }
    }
}