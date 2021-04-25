﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpGen.Model;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SharpGen.Generator.Marshallers
{
    internal class BoolToIntArrayMarshaller : MarshallerBase, IMarshaller
    {
        private static readonly SyntaxToken PtrIdentifier = Identifier("__ptr");

        public BoolToIntArrayMarshaller(GlobalNamespaceProvider globalNamespace) : base(globalNamespace)
        {
        }

        public bool CanMarshal(CsMarshalBase csElement) => csElement.IsBoolToInt && csElement.IsArray;

        public ArgumentSyntax GenerateManagedArgument(CsParameter csElement) =>
            Argument(IdentifierName(csElement.Name));

        public ParameterSyntax GenerateManagedParameter(CsParameter csElement) =>
            GenerateManagedArrayParameter(csElement);

        public StatementSyntax GenerateManagedToNative(CsMarshalBase csElement, bool singleStackFrame)
        {
            var marshalStorage = GetMarshalStorageLocation(csElement);

            // TODO: Reverse-callback support?
            StatementSyntax value;
            if (singleStackFrame)
                value = EmitConvertToIntArray(marshalStorage);
            else
            {
                marshalStorage = PrefixUnaryExpression(SyntaxKind.AddressOfExpression, marshalStorage);

                value = FixedStatement(
                    VariableDeclaration(
                        GetMarshalTypeSyntax(csElement),
                        SingletonSeparatedList(
                            VariableDeclarator(PtrIdentifier, default, EqualsValueClause(marshalStorage))
                        )
                    ),
                    EmitConvertToIntArray(IdentifierName(PtrIdentifier))
                );
            }

            return GenerateNullCheckIfNeeded(csElement, value);

            ExpressionStatementSyntax EmitConvertToIntArray(ExpressionSyntax destination) => ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        GlobalNamespace.GetTypeNameSyntax(WellKnownName.BooleanHelpers),
                        IdentifierName("ConvertToIntArray")
                    ),
                    ArgumentList(
                        SeparatedList(
                            new[]
                            {
                                Argument(IdentifierName(csElement.Name)),
                                Argument(destination)
                            }
                        )
                    )
                )
            );
        }

        public IEnumerable<StatementSyntax> GenerateManagedToNativeProlog(CsMarshalCallableBase csElement)
        {
            yield return LocalDeclarationStatement(
                VariableDeclaration(
                    GetMarshalTypeSyntax(csElement),
                    SingletonSeparatedList(VariableDeclarator(GetMarshalStorageLocationIdentifier(csElement)))
                )
            );
            yield return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(GetMarshalStorageLocationIdentifier(csElement)),
                    CastExpression(
                        GetMarshalTypeSyntax(csElement),
                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))
                    )
                )
            );
            yield return GenerateNullCheckIfNeeded(
                csElement,
                Block(
                    LocalDeclarationStatement(
                        VariableDeclaration(
                            GetMarshalTypeSyntax(csElement),
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier(csElement.IntermediateMarshalName),
                                    null,
                                    EqualsValueClause(
                                        StackAllocArrayCreationExpression(
                                            ArrayType(
                                                GetMarshalElementTypeSyntax(csElement),
                                                SingletonList(
                                                    ArrayRankSpecifier(
                                                        SingletonSeparatedList<ExpressionSyntax>(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName(csElement.Name),
                                                                IdentifierName("Length")
                                                            ))))))))))),
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(GetMarshalStorageLocationIdentifier(csElement)),
                            IdentifierName(csElement.IntermediateMarshalName)
                        )
                    )
                )
            );
        }

        public ArgumentSyntax GenerateNativeArgument(CsMarshalCallableBase csElement) =>
            Argument(GetMarshalStorageLocation(csElement));

        public StatementSyntax GenerateNativeCleanup(CsMarshalBase csElement, bool singleStackFrame) => null;

        public StatementSyntax GenerateNativeToManaged(CsMarshalBase csElement, bool singleStackFrame)
        {
            var marshalStorage = GetMarshalStorageLocation(csElement);

            StatementSyntax value;
            if (singleStackFrame)
            {
                value = EmitConvertToBoolArray(marshalStorage);
            }
            else if (csElement is CsField)
            {
                marshalStorage = PrefixUnaryExpression(SyntaxKind.AddressOfExpression, marshalStorage);

                value = FixedStatement(
                    VariableDeclaration(
                        GetMarshalTypeSyntax(csElement),
                        SingletonSeparatedList(
                            VariableDeclarator(PtrIdentifier, default, EqualsValueClause(marshalStorage))
                        )
                    ),
                    EmitConvertToBoolArray(IdentifierName(PtrIdentifier))
                );
            }
            else // Reverse-callbacks
            {
                value = EmitConvertToBoolArray(marshalStorage);
            }

            return GenerateNullCheckIfNeeded(csElement, value);

            ExpressionStatementSyntax EmitConvertToBoolArray(ExpressionSyntax storage) => ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        GlobalNamespace.GetTypeNameSyntax(WellKnownName.BooleanHelpers),
                        IdentifierName("ConvertToBoolArray")
                    ),
                    ArgumentList(
                        SeparatedList(
                            new[]
                            {
                                Argument(storage),
                                Argument(IdentifierName(csElement.Name))
                            }
                        )
                    )
                )
            );
        }

        public IEnumerable<StatementSyntax> GenerateNativeToManagedExtendedProlog(CsMarshalCallableBase csElement)
        {
            yield return GenerateArrayNativeToManagedExtendedProlog(csElement);
        }

        public FixedStatementSyntax GeneratePin(CsParameter csElement) => null;

        public bool GeneratesMarshalVariable(CsMarshalCallableBase csElement) => true;

        private static TypeSyntax GetMarshalElementTypeSyntax(CsMarshalBase csElement) =>
            ParseTypeName(csElement.MarshalType.QualifiedName); 

        public TypeSyntax GetMarshalTypeSyntax(CsMarshalBase csElement) =>
            PointerType(GetMarshalElementTypeSyntax(csElement));
    }
}
