﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Playground.CodeGeneration
{
    [Generator]
    public class LoggingAdapterGenerator : ISourceGenerator
    {
        internal int NumCandidatesFound { get; private set; }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                if (context.SyntaxReceiver is AkkaLoggingAdapterReceiver receiver)
                {
                    NumCandidatesFound = receiver.Candidates.Count;
                    foreach (var candidate in receiver.Candidates)
                    {
                        var classDecl = candidate.Class;
                        if (classDecl.TryFindParent<NamespaceDeclarationSyntax>(out var namespaceDeclaration))
                        {
                            var sourceBuilder = new StringBuilder(@"
using System;
using Akka.Event;

namespace {{NAMESPACE}}
{
    /// <summary>
    /// AUTOGENERATED CODE FROM LoggingAdapterGenerator
    /// </summary>
    public partial class {{CLASS}}
    {
        private static ILoggingAdapter Log => Context.GetLogger();
    }
}");

                            var classDeclarationText = $"{classDecl.Identifier.Text}{classDecl.TypeParameterList}";

                            var source = sourceBuilder.ToString()
                                .Replace("{{NAMESPACE}}", namespaceDeclaration.Name.ToString())
                                .Replace("{{CLASS}}", classDeclarationText);

                            // inject the created source into the users compilation
                            context.AddSource($"generatedAkka-LoggingAdapter-{classDecl.Identifier.Text}", source);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"There was an exception in {nameof(LoggingAdapterGenerator)}.{nameof(Execute)}: {ex.Message}\r\n{ex.StackTrace}");
                if (!Debugger.IsAttached)
                {
                    Debugger.Launch();
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new AkkaLoggingAdapterReceiver());
        }
    }

    public class Candidate
    {
        public ClassDeclarationSyntax Class { get; set; }
        public Location Location { get; set; }
    }

    public class AkkaLoggingAdapterReceiver : ISyntaxReceiver
    {
        public List<Candidate> Candidates { get; } = new List<Candidate>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            try
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    if (classDeclarationSyntax.BaseList == null ||
                        classDeclarationSyntax.IsAbstract())
                    {
                        return;
                    }

                    if (classDeclarationSyntax.BaseList.Types.Any(b => b.Type.ToString().EndsWith("Actor")))
                    {
                        if (classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                        {
                            Candidates.Add(new Candidate()
                            {
                                Class = classDeclarationSyntax,
                                Location = classDeclarationSyntax.GetLocation()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"There was an exception in {nameof(AkkaLoggingAdapterReceiver)}.{nameof(OnVisitSyntaxNode)}: {ex.Message}\r\n{ex.StackTrace}");
                if (!Debugger.IsAttached)
                {
                    Debugger.Launch();
                }
            }
        }
        
        
    }
    
}
