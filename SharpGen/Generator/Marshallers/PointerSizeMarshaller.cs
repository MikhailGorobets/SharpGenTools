﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SharpGen.Model;
using Microsoft.CodeAnalysis.CSharp;
using System;


namespace SharpGen.Generator.Marshallers
{
    class PointerSizeMarshaller : MarshallerBase, IMarshaller
    {
        public PointerSizeMarshaller(GlobalNamespaceProvider globalNamespace) : base(globalNamespace)
        {
        }

        public bool CanMarshal(CsMarshalBase csElement)
        {
            return (csElement.PublicType.QualifiedName == globalNamespace.GetTypeName(WellKnownName.PointerSize)
                    || (csElement.PublicType is CsFundamentalType fundamental && fundamental.Type == typeof(IntPtr)))
                && !csElement.IsArray
                && ((csElement is CsParameter param && param.IsIn) || csElement is CsReturnValue);
        }

        public ArgumentSyntax GenerateManagedArgument(CsParameter csElement)
        {
            return GenerateManagedValueTypeArgument(csElement);
        }

        public ParameterSyntax GenerateManagedParameter(CsParameter csElement)
        {
            return GenerateManagedValueTypeParameter(csElement);
        }

        public StatementSyntax GenerateManagedToNative(CsMarshalBase csElement, bool singleStackFrame)
        {
            return null;
        }

        public ArgumentSyntax GenerateNativeArgument(CsMarshalCallableBase csElement)
        {
            return Argument(CastExpression(
                PointerType(PredefinedType(Token(SyntaxKind.VoidKeyword))),
                IdentifierName(csElement.Name)));
        }

        public StatementSyntax GenerateNativeCleanup(CsMarshalBase csElement, bool singleStackFrame)
        {
            return null;
        }

        public StatementSyntax GenerateNativeToManaged(CsMarshalBase csElement, bool singleStackFrame)
        {
            return null;
        }

        public FixedStatementSyntax GeneratePin(CsParameter csElement)
        {
            return null;
        }
    }
}
