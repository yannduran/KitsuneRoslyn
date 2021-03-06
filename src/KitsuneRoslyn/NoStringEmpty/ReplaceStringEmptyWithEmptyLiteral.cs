﻿// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using BlackFox.Roslyn.Diagnostics.RoslynExtensions.SyntaxFactoryAdditions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

namespace BlackFox.Roslyn.Diagnostics.NoStringEmpty
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class ReplaceStringEmptyWithEmptyLiteral : ReplacementNodeCodeFixProviderBase
    {
        public const string Id = "BlackFox.ReplaceStringEmptyWithEmptyLiteral";

        static readonly LiteralExpressionSyntax emptyStringLiteralExpression
            = StringLiteralExpression("");

        public ReplaceStringEmptyWithEmptyLiteral()
                    : base(NoStringEmptyAnalyzer.Id, "Use \"\"")
        {
        }

        protected override async Task<SyntaxNode> GetReplacementNodeAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var stringEmptyExpression = (MemberAccessExpressionSyntax)nodeToFix;

            return emptyStringLiteralExpression.WithSameTriviaAs(stringEmptyExpression);
        }
    }
}