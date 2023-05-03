﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpGen.Logging;
using SharpGen.Model;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SharpGen.Generator;

internal sealed class NativeStructCodeGenerator : MemberMultiCodeGeneratorBase<CsStruct>
{
    private static readonly NameSyntax StructLayoutAttributeName = ParseName("System.Runtime.InteropServices.StructLayoutAttribute");
    private const string StructLayoutKindName = "System.Runtime.InteropServices.LayoutKind.";
    private static readonly AttributeArgumentSyntax StructLayoutExplicit = AttributeArgument(ParseName(StructLayoutKindName + "Explicit"));
    private static readonly AttributeArgumentSyntax StructLayoutSequential = AttributeArgument(ParseName(StructLayoutKindName + "Sequential"));
    private static readonly NameEqualsSyntax StructLayoutPackName = NameEquals(IdentifierName("Pack"));
    private static readonly SyntaxToken MarshalParameterRefName = Identifier("@ref");

    private static readonly AttributeArgumentSyntax StructLayoutCharset = AttributeArgument(
        ParseName("System.Runtime.InteropServices.CharSet.Unicode")
    ).WithNameEquals(NameEquals(IdentifierName("CharSet")));

    public override IEnumerable<MemberDeclarationSyntax> GenerateCode(CsStruct csStruct)
    {
        yield return StructDeclaration("__Native")
                    .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.UnsafeKeyword), Token(SyntaxKind.PartialKeyword)))
                    .WithAttributeLists(SingletonList(GenerateStructLayoutAttribute(csStruct)))
                    .WithMembers(List(csStruct.Fields.SelectMany(GenerateMarshalStructField)));

        if (csStruct.GenerateAsClass)
        {
            var methodName = IdentifierName("__MarshalFrom");
            var marshalArgument = Argument(IdentifierName(MarshalParameterRefName))
               .WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword));

            var invocationExpression = csStruct.IsStaticMarshal
                                           ? InvocationExpression(
                                               methodName,
                                               ArgumentList(
                                                   SeparatedList(
                                                       new[]
                                                       {
                                                           Argument(ThisExpression())
                                                              .WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword)),
                                                           marshalArgument
                                                       }
                                                   )
                                               )
                                           )
                                           : InvocationExpression(
                                               methodName,
                                               ArgumentList(SingletonSeparatedList(marshalArgument))
                                           );

            yield return ConstructorDeclaration(csStruct.Name)
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                        .WithBody(Block());

            yield return ConstructorDeclaration(csStruct.Name)
                        .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword)))
                        .WithParameterList(MarshalParameterListSyntax)
                        .WithBody(Block(ExpressionStatement(invocationExpression)));
        }

        yield return GenerateMarshalFree(csStruct);
        yield return GenerateMarshalFrom(csStruct);
        yield return GenerateMarshalTo(csStruct);

        IEnumerable<MemberDeclarationSyntax> GenerateMarshalStructField(CsField field)
        {
            var fieldDecl = FieldDeclaration(VariableDeclaration(ParseTypeName(field.MarshalType.QualifiedName)))
               .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

            if (csStruct.ExplicitLayout)
                fieldDecl = AddFieldOffsetAttribute(fieldDecl, field.Offset);

            if (field.ArraySpecification is { } arraySpecification)
            {
                FieldDeclarationSyntax ComputeType(ArraySpecification? arraySpec, string typeName, string fieldName, bool hasNativeValueType)
                {
                    var qualifiedName = typeName;
                    if (hasNativeValueType) qualifiedName += ".__Native";
                    if (arraySpec?.Type == ArraySpecificationType.Dynamic) qualifiedName += "*";

                    return fieldDecl.WithDeclaration(VariableDeclaration(ParseTypeName(qualifiedName), SingletonSeparatedList(VariableDeclarator(fieldName))));
                }

                yield return ComputeType(field.ArraySpecification, field.MarshalType.QualifiedName, field.Name, field.HasNativeValueType);

                if (arraySpecification.Dimension is { } dimension)
                    for (var i = 1; i < dimension; i++)
                    {
                        var declaration = ComputeType(field.ArraySpecification, field.MarshalType.QualifiedName, $"__{field.Name}{i}", field.HasNativeValueType);
                         
                        if (csStruct.ExplicitLayout)
                        {
                            var offset = field.Offset + i * field.Size / dimension;
                            declaration = AddFieldOffsetAttribute(declaration, offset, true);
                        }

                        yield return declaration;
                    }
                else
                    Logger.Warning(
                        LoggingCodes.UnknownArrayDimension, "Unknown array dimensions for [{0}]",
                        field.QualifiedName
                    );
            }
            else if (field.HasNativeValueType)
            {
                var qualifiedName = field.MarshalType.QualifiedName;
                qualifiedName += ".__Native";
                if (field.HasPointer) qualifiedName += "*";

                yield return fieldDecl.WithDeclaration(
                    VariableDeclaration(
                        ParseTypeName(qualifiedName),
                        SingletonSeparatedList(VariableDeclarator(field.Name))
                    )
                );
            }
            else
            {
                var qualifiedName = field.MarshalType.QualifiedName;
                if (field.HasPointer && !field.IsInterface && !field.IsString) qualifiedName += "*";

                yield return fieldDecl.WithDeclaration(
                    VariableDeclaration(
                        ParseTypeName(qualifiedName),
                        SingletonSeparatedList(VariableDeclarator(field.Name))
                    )
                ); ;
            }
        }
    }

    internal static AttributeListSyntax GenerateStructLayoutAttribute(CsStruct csElement) => AttributeList(
        SingletonSeparatedList(
            Attribute(
                StructLayoutAttributeName,
                AttributeArgumentList(
                    SeparatedList(
                        new[]
                        {
                            csElement.ExplicitLayout ? StructLayoutExplicit : StructLayoutSequential,
                            AttributeArgument(
                                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(csElement.Align))
                                )
                               .WithNameEquals(StructLayoutPackName),
                            StructLayoutCharset
                        }
                    )
                )
            )
        )
    );

    private MethodDeclarationSyntax GenerateMarshalFree(CsStruct csStruct) => GenerateMarshalMethod(
        "__MarshalFree",
        csStruct.Fields,
        field => GetMarshaller(field)?.GenerateNativeCleanup(field, false)
    );

    private MethodDeclarationSyntax GenerateMarshalTo(CsStruct csStruct)
    {
        IEnumerable<StatementSyntax> FieldMarshallers(CsField field)
        {
            if (field.Relations.Count == 0)
            {
                yield return GetMarshaller(field).GenerateManagedToNative(field, false);
                yield break;
            }

            foreach (var relation in field.Relations)
            {
                var marshaller = GetRelationMarshaller(relation);
                if (relation is not LengthRelation related)
                    yield return marshaller.GenerateManagedToNative(null, field);
            }
        }

        return GenerateMarshalMethod(
            "__MarshalTo",
            csStruct.Fields,
            FieldMarshallers
        );
    }

    private static ParameterListSyntax MarshalParameterListSyntax => ParameterList(
        SingletonSeparatedList(Parameter(MarshalParameterRefName).WithType(RefType(ParseTypeName("__Native"))))
    );

    private MethodDeclarationSyntax GenerateMarshalMethod<T>(string name, IEnumerable<T> source,
                                                             Func<T, StatementSyntax> transform)
        where T : CsMarshalBase
    {
        var list = NewStatementList;
        list.AddRange(source, transform);
        return GenerateMarshalMethod(name, list);
    }

    private MethodDeclarationSyntax GenerateMarshalMethod<T>(string name, IEnumerable<T> source,
                                                             Func<T, IEnumerable<StatementSyntax>> transform)
        where T : CsMarshalBase
    {
        var list = NewStatementList;
        list.AddRange(source, transform);
        return GenerateMarshalMethod(name, list);
    }

    private static MethodDeclarationSyntax GenerateMarshalMethod(string name, StatementSyntaxList body) =>
        MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), name)
           .WithParameterList(MarshalParameterListSyntax)
           .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.UnsafeKeyword)))
           .WithBody(body.ToBlock());

    private MethodDeclarationSyntax GenerateMarshalFrom(CsStruct csStruct)
    {
        IEnumerable<StatementSyntax> FieldMarshallers(CsField field)
        {
            if (field.Relations.Count == 0)
            {
                yield return GetMarshaller(field).GenerateNativeToManaged(field, false);
            }
        }

        return GenerateMarshalMethod(
            "__MarshalFrom",
            csStruct.PublicFields,
            FieldMarshallers);
    }

    public NativeStructCodeGenerator(Ioc ioc) : base(ioc)
    {
    }
}