﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.
// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.
// See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    public static class TernaryReplacable
    {
        public static bool TryFind(SyntaxNodeOrToken before,
            SyntaxNodeOrToken after, out ImmutableList<Tuple<SyntaxNode, SyntaxNode>> differences)
        {
            var rawDifferences = SyntaxDifferences.Find(before, after);
            var manageableDifferences = rawDifferences
                .Select(d => NearestReplacableByTernary(d.Item1, d.Item2, before, after))
                .ToImmutableList();

            if (manageableDifferences.Any(d => d == null))
            {
                differences = null;
                return false;
            }

            differences = manageableDifferences;
            return true;
        }

        private static Tuple<SyntaxNode, SyntaxNode> NearestReplacableByTernary(
            SyntaxNode item1, SyntaxNode item2,
            SyntaxNodeOrToken before, SyntaxNodeOrToken after)
        {
            if (item1 == before || item2 == after)
            {
                return null;
            }

            if (IsReplacableByTernary(item1) && IsReplacableByTernary(item2))
            {
                return Tuple.Create(item1, item2);
            }

            return NearestReplacableByTernary(item1.Parent, item2.Parent, before, after);
        }

        static HashSet<SyntaxKind> okParents = new HashSet<SyntaxKind>
        {
            SyntaxKind.Argument,
            SyntaxKind.EqualsValueClause,
            SyntaxKind.AddExpression,
            SyntaxKind.SubtractExpression,
            SyntaxKind.MultiplyExpression,
            SyntaxKind.DivideExpression,
            SyntaxKind.ModuloExpression,
            SyntaxKind.LeftShiftExpression,
            SyntaxKind.RightShiftExpression,
            SyntaxKind.LogicalOrExpression,
            SyntaxKind.LogicalAndExpression,
            SyntaxKind.BitwiseOrExpression,
            SyntaxKind.BitwiseAndExpression,
            SyntaxKind.ExclusiveOrExpression,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression,
            SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression,
        };

        static HashSet<SyntaxKind> rightBinaryOkParents = new HashSet<SyntaxKind>
        {
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.AddAssignmentExpression,
            SyntaxKind.SubtractAssignmentExpression,
            SyntaxKind.MultiplyAssignmentExpression,
            SyntaxKind.DivideAssignmentExpression,
            SyntaxKind.ModuloAssignmentExpression,
            SyntaxKind.AndAssignmentExpression,
            SyntaxKind.ExclusiveOrAssignmentExpression,
            SyntaxKind.OrAssignmentExpression,
            SyntaxKind.LeftShiftAssignmentExpression,
            SyntaxKind.RightShiftAssignmentExpression,
        };

        private static bool IsReplacableByTernary(SyntaxNode node)
        {
            var parentKind = node.Parent.CSharpKind();
            if (okParents.Contains(parentKind))
            {
                return true;
            }
            if (rightBinaryOkParents.Contains(parentKind))
            {
                return ((BinaryExpressionSyntax)node.Parent).Right == node;
            }
            return false;
        }
    }
}