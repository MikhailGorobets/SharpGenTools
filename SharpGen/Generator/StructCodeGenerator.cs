﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpGen.Model;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SharpGen.Generator;

internal sealed class StructCodeGenerator : MemberSingleCodeGeneratorBase<CsStruct>
{
    public StructCodeGenerator(Ioc ioc) : base(ioc)
    {
    }

    public override MemberDeclarationSyntax GenerateCode(CsStruct csElement)
    {
        var list = NewMemberList;
        list.AddRange(csElement.InnerStructs, Generators.Struct);
        list.AddRange(csElement.ExpressionConstants, Generators.ExpressionConstant);
        list.AddRange(csElement.GuidConstants, Generators.GuidConstant);
        list.AddRange(csElement.ResultConstants, Generators.ResultConstant);

        var explicitLayout = !csElement.HasMarshalType && csElement.ExplicitLayout;
        list.AddRange(csElement.PublicFields, explicitLayout ? Generators.ExplicitOffsetField : Generators.AutoLayoutField);

        if (!csElement.HasCustomMarshal)
            list.Add(csElement, Generators.DefaultConstructor);

        if (csElement.HasMarshalType && !csElement.HasCustomMarshal)
            list.Add(csElement, Generators.NativeStruct);

        var attributeList = !csElement.HasMarshalType
                                ? SingletonList(NativeStructCodeGenerator.GenerateStructLayoutAttribute(csElement))
                                : default;

        var modifierTokenList = csElement.VisibilityTokenList.Add(Token(SyntaxKind.PartialKeyword));
        var identifier = Identifier(csElement.Name);
        var baseType = csElement.BaseObject != null
            ? BaseList(Token(SyntaxKind.ColonToken),
                SingletonSeparatedList((BaseTypeSyntax) SimpleBaseType(ParseTypeName(csElement.BaseObject.Name))))
            : default;

        MemberDeclarationSyntax declaration = csElement.GenerateAsClass
                                                  ? ClassDeclaration(
                                                      attributeList,
                                                      modifierTokenList,
                                                      identifier,
                                                      default,
                                                      baseType,
                                                      default,
                                                      List(list)
                                                  )
                                                  : StructDeclaration(
                                                      attributeList,
                                                      modifierTokenList,
                                                      identifier,
                                                      default,
                                                      default,
                                                      default,
                                                      List(list)
                                                  );

        return AddDocumentationTrivia(declaration, csElement);
    }
}