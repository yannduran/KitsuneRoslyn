﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace TestDiagnosticsUnitTests.Helpers
{
    static class DiagnosticTestHelpers
    {
        public static ImmutableList<Diagnostic> GetDiagnosticsInSimpleCode(
            this ISyntaxNodeAnalyzer<SyntaxKind> analyzer, string code)
        {
            var fullCode = string.Format("using System; namespace TestNamespace {{ public class TestClass {{ "
                + "public static void TestMethod() {{ {0} }} }} }}", code);

            return GetDiagnostics(analyzer, fullCode);
        }

        public static CSharpCompilation CreateCompilation(string code)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(code);
            var compilation = CSharpCompilation.Create(null,
                syntaxTrees: ImmutableArray.Create(tree),
                references: new[]
                {
                    new MetadataFileReference(typeof(object).Assembly.Location),
                });

            return compilation;
        }

        public static ImmutableList<Diagnostic> GetDiagnostics(
            this ISyntaxNodeAnalyzer<SyntaxKind> analyzer, string code)
        {
            var compilation = CreateCompilation(code);
            var tree = compilation.SyntaxTrees.Single();
            var root = tree.GetRoot();

            var diagnostics = ImmutableList<Diagnostic>.Empty;
            AnalyzeTree(analyzer, tree, compilation, d => diagnostics = diagnostics.Add(d));
            return diagnostics;
        }

        public static void AnalyzeTree(this ISyntaxNodeAnalyzer<SyntaxKind> analyzer,
            SyntaxTree tree, Compilation compilation, Action<Diagnostic> addDiagnostic)
        {
            var kinds = analyzer.SyntaxKindsOfInterest;

            var matchingNodes =
                from node in RecursiveGetChild(tree.GetRoot())
                where kinds.Any(k => node.CSharpKind() == k)
                select node;

            foreach (var node in matchingNodes)
            {
                analyzer.AnalyzeNode(
                    node,
                    compilation.GetSemanticModel(tree),
                    addDiagnostic,
                    CancellationToken.None);
            }
        }

        private static IEnumerable<SyntaxNode> RecursiveGetChild(SyntaxNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            Stack<SyntaxNode> x = new Stack<SyntaxNode>();
            x.Push(node);

            while (x.Count != 0)
            {
                var currentNode = x.Pop();

                yield return currentNode;

                foreach (var childNode in currentNode.ChildNodes())
                {
                    x.Push(childNode);
                }
            }
        }
    }
}