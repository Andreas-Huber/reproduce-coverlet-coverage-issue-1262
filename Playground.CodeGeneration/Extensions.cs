using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Playground.CodeGeneration
{
    public static class Extensions
    {
        public static bool IsAbstract(this MemberDeclarationSyntax syntax)
            => syntax.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.AbstractKeyword));
        
        public static bool TryFindParent<TParent>(this SyntaxNode node, out TParent parent)
            where TParent : SyntaxNode
        {
            if (node == null)
            {
                parent = null;
                return false;
            }
            else if (node.GetType() == typeof(TParent))
            {
                parent = (TParent)node;
                return true;
            }
            else
            {
                if (node.Parent != null)
                {
                    return node.Parent.TryFindParent(out parent);
                }
                else
                {
                    parent = null;
                    return false;
                }
            }
        }
    }
}