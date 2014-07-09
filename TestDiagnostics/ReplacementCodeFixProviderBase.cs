﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using System.Threading;
using System.Threading.Tasks;
using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions;
using System.Collections.Immutable;

namespace BlackFox.Roslyn.TestDiagnostics
{
    public abstract class ReplacementCodeFixProviderBase : CodeFixProviderBase
    {
        protected virtual bool Simplify { get { return false; } }
        protected virtual bool Format { get { return false; } }

        public ReplacementCodeFixProviderBase(string diagnosticId, string fixDescription)
            : base(diagnosticId, fixDescription)
        {
        }

        protected override async Task<Document> GetUpdatedDocumentAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var replacementNode = await GetReplacementNodeAsync(document, semanticModel, root, nodeToFix,
                diagnosticId, cancellationToken).ConfigureAwait(false);

            if (replacementNode == nodeToFix)
            {
                return document;
            }

            replacementNode = replacementNode.WithAdditionalAnnotations(GetAnnotations());

            document = await document.ReplaceNodeAsync(nodeToFix, replacementNode);
            
            if (Simplify)
            {
                var simplificationTask = Simplifier.ReduceAsync(
                    document,
                    Simplifier.Annotation,
                    cancellationToken: cancellationToken);

                document = await simplificationTask.ConfigureAwait(false);
            }

            if (Format)
            {
                var formattingTask = Formatter.FormatAsync(
                    document,
                    Formatter.Annotation,
                    cancellationToken: cancellationToken);

                document = await formattingTask.ConfigureAwait(false);
            }

            return document;
        }

        private ImmutableList<SyntaxAnnotation> GetAnnotations()
        {
            var annotations = ImmutableList<SyntaxAnnotation>.Empty;

            if (Simplify)
            {
                annotations = annotations.Add(Simplifier.Annotation);
            }
            if (Format)
            {
                annotations = annotations.Add(Formatter.Annotation);
            }

            return annotations;
        }

        protected abstract Task<SyntaxNode> GetReplacementNodeAsync(Document document, SemanticModel model,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken);
    }
}