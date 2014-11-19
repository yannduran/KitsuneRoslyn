﻿// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    class PotentialTernaryOperator
    {
        public static PotentialTernaryOperator NoReplacement { get; }
            = new PotentialTernaryOperator(
                    PotentialTernaryOperatorClassification.NoReplacement,
                    null, 0);

        public PotentialTernaryOperatorClassification Classification { get; private set; }
        public Optional<SyntaxNode> NodeToRemove { get; private set; }
        public ImmutableList<StatementSyntax> Replacements { get; private set; }
        public int TernaryOperatorCount { get; private set; }

        PotentialTernaryOperator(PotentialTernaryOperatorClassification classification,
            ImmutableList<StatementSyntax> replacements, int ternaryOperatorCount)
        {
            Classification = classification;
            NodeToRemove = new Optional<SyntaxNode>();
            Replacements = replacements;
            TernaryOperatorCount = ternaryOperatorCount;
        }


        static ExpressionSyntax ParenthesesAroundConditionalExpression(ExpressionSyntax expression)
        {
            var conditional = expression as ConditionalExpressionSyntax;
            return conditional != null
                ? conditional.WithParentheses()
                : expression;
        }

        public static ConditionalExpressionSyntax BuildReplacement(ExpressionSyntax condition, ExpressionSyntax whenTrue,
            ExpressionSyntax whenFalse)
        {
            var conditional = ConditionalExpression(
                condition,
                ParenthesesAroundConditionalExpression(whenTrue),
                ParenthesesAroundConditionalExpression(whenFalse));

            return conditional
                .WithAdditionalAnnotations(SyntaxAnnotation.ElasticAnnotation, Formatter.Annotation);
        }

        public static PotentialTernaryOperator Create(IfStatementSyntax ifStatement)
        {
            if (ifStatement.Else == null)
            {
                return NoReplacement;
            }

            var replaceable = TernaryReplaceable.Find(ifStatement.Statement, ifStatement.Else.Statement);

            if (!replaceable.IsReplaceable)
            {
                return NoReplacement;
            }

            var toTrack = replaceable.Differences.Select(t => t.Item1);
            var replacement = ifStatement.Statement.TrackNodes(toTrack);

            foreach (var difference in replaceable.Differences)
            {
                var conditionalReplacement = BuildReplacement(ifStatement.Condition, difference.Item1, difference.Item2);
                replacement = replacement.ReplaceNode(replacement.GetCurrentNode(difference.Item1), conditionalReplacement);
            }

            return new PotentialTernaryOperator(PotentialTernaryOperatorClassification.ReplacementPossible,
                BlockContent(replacement), replaceable.Differences.Count);
        }

        static ImmutableList<StatementSyntax> BlockContent(StatementSyntax node)
        {
            var block = node as BlockSyntax;
            if (block != null)
            {
                return block.Statements.ToImmutableList();
            }
            else
            {
                return ImmutableList<StatementSyntax>.Empty.Add(node);
            }
        } 
    }
}