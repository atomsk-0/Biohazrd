﻿using ClangSharp;
using ClangSharp.Interop;

namespace Biohazrd.Transformation.Common
{
    public sealed class TypeReductionTransformation : TypeTransformationBase
    {
        protected override TypeTransformationResult TransformClangTypeReference(TypeTransformationContext context, ClangTypeReference type)
        {
            switch (type.ClangType)
            {
                // Void type
                case BuiltinType { Kind: CXTypeKind.CXType_Void }:
                {
                    return VoidTypeReference.Instance;
                }
                // Elaborated types are namespace-qualified types like physx::PxU32 instead of just PxU32.
                case ElaboratedType elaboratedType:
                {
                    return new ClangTypeReference(elaboratedType.NamedType);
                }
                // We don't care that `auto` or `decltype` was used for a type, so we just replace them with their canonical form
                case AutoType:
                case DecltypeType:
                {
                    return new ClangTypeReference(type.ClangType.CanonicalType);
                }
                // If a typedef is mapped to a translated declaration, it becomes a translated type reference.
                // Otherwise we eliminate the typedef
                case TypedefType typedefType:
                {
                    if (context.Library.TryFindTranslation(typedefType.Decl) is not null)
                    { return new TranslatedTypeReference(typedefType.Decl); }
                    else
                    { return new ClangTypeReference(typedefType.CanonicalType); }
                }
                case PointerType pointerType:
                {
                    ClangTypeReference inner = new(pointerType.PointeeType);
                    return new PointerTypeReference(inner);
                }
                case ReferenceType referenceType:
                {
                    ClangTypeReference inner = new(referenceType.PointeeType);
                    TypeTransformationResult result = new PointerTypeReference(inner)
                    {
                        WasReference = true
                    };

                    if (referenceType is RValueReferenceType)
                    { result = result.AddDiagnostic(Severity.Warning, "Found RValue reference type. This type may not be translated correctly."); }

                    return result;
                }
                // Constant arrays passed as parameters are translated as pointers
                case ConstantArrayType constantArrayType when context.ParentDeclaration is TranslatedParameter:
                {
                    ClangTypeReference inner = new(constantArrayType.ElementType);
                    PointerTypeReference result = new(inner);
                    return new TypeTransformationResult(result, Severity.Warning, $"The size of this constant-sized array parameter was discarded.");
                }
                // Incomplete arrays are translated as pointers
                case IncompleteArrayType incompleteArrayType when context.ParentDeclaration is TranslatedParameter:
                {
                    ClangTypeReference inner = new(incompleteArrayType.ElementType);
                    return new PointerTypeReference(inner);
                }
                case FunctionProtoType functionProtoType:
                {
                    return new FunctionPointerTypeReference(functionProtoType);
                }
                case EnumType enumType:
                {
                    return new TranslatedTypeReference(enumType.Decl);
                }
                case RecordType recordType:
                {
                    return new TranslatedTypeReference(recordType.Decl);
                }
                // Don't know how to reduce this type
                default:
                {
                    return type;
                }
            }
        }
    }
}