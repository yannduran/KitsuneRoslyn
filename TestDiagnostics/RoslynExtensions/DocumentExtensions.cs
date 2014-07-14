﻿// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.TestDiagnostics.RoslynExtensions
{
    public static class DocumentExtensions
    {
        public static async Task<Document> ReplaceNodeAsync<TNode>(this Document document,
            TNode oldNode, TNode newNode, CancellationToken cancellationToken = default(CancellationToken))
            where TNode : SyntaxNode
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(oldNode, newNode);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
