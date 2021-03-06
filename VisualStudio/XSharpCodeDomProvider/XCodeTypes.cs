﻿//
// Copyright (c) XSharp B.V.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.
// See License.txt in the project root for license information.
//
using System.CodeDom;
using System.Diagnostics;
using XSharpModel;

namespace XSharp.CodeDom
{
    /// <summary>
    /// Enhanced Type reference with System.Type property, since CodeTypeReference does not hold on to the type
    /// </summary>
    [DebuggerDisplay("{_typeName,nq}")]
    internal class XCodeTypeReference : CodeTypeReference
    {

        internal XCodeTypeReference(string typeName) : base(typeName)
        {
        }
        internal XCodeTypeReference(IXType type) : base(type.FullName)
        {
        }
    }

    [DebuggerDisplay("{Name,nq}")]
    internal class XCodeTypeReferenceExpression : CodeTypeReferenceExpression
    {
        internal string Name { get; private set; }
        internal XCodeTypeReferenceExpression(System.Type type) : base(type)
        {
            Name = type.FullName;
        }
        internal XCodeTypeReferenceExpression(string type) : base(type)
        {
            Name = type;
            if (Name.StartsWith("global::"))
            {
                var typeName= Name.Substring(8);
                typeName = typeName.Replace(".Properties.Resources",".Resources");
                this.Type.BaseType = typeName;
                this.Type.Options = CodeTypeReferenceOptions.GlobalReference;
            }
        }
        internal XCodeTypeReferenceExpression(CodeTypeReference type) : base(type)
        {
            Name = type.BaseType;
        }
    }


    [DebuggerDisplay("{Name,nq}")]
    internal class XCodeMemberField : CodeMemberField
    {
        internal XCodeMemberField() : base()
        {

        }
    }
    [DebuggerDisplay("{Name,nq}")]
    internal class XCodeNamespace : CodeNamespace
    {
        internal XCodeNamespace(string name) : base(name)
        {

        }
    }


    [DebuggerDisplay("{Name,nq}")]
    internal class XCodeTypeDeclaration : CodeTypeDeclaration
    {
        internal XCodeTypeDeclaration(string name) : base(name)
        {

        }
    }
    [DebuggerDisplay("{Name,nq}")]
    internal class XCodeMemberMethod : CodeMemberMethod
    {
        internal XCodeMemberMethod() : base()
        {

        }
    }
    internal class XCodeCompileUnit : CodeCompileUnit
    {
        internal XCodeCompileUnit() : base()
        {

        }
    }
    [DebuggerDisplay("{Name,nq}")]
    internal class XCodeMemberEvent : CodeMemberEvent
    {
        internal XCodeMemberEvent() : base()
        {

        }
    }
    [DebuggerDisplay("{Name,nq}")]
    internal class XCodeConstructor : CodeConstructor
    {
        internal XCodeConstructor() : base()
        {

        }
    }
    [DebuggerDisplay("{Name,nq}")]
    internal class XCodeNamespaceImport : CodeNamespaceImport
    {
        internal XCodeNamespaceImport(string name) : base(name)
        {

        }
    }
    [DebuggerDisplay("{FieldName,nq}")]
    internal class XCodeFieldReferenceExpression : CodeFieldReferenceExpression
    {
        internal XCodeFieldReferenceExpression(CodeExpression lhs, string name) : base(lhs, name)
        {

        }
    }

    internal class XCodeSnippetTypeMember : CodeSnippetTypeMember
    {
        internal XCodeSnippetTypeMember(string text) : base(text)
        {

        }
    }

    [DebuggerDisplay("{PropertyName,nq}")]
    internal class XCodePropertyReferenceExpression : CodePropertyReferenceExpression
    {
        internal XCodePropertyReferenceExpression(CodeExpression lhs, string name) : base(lhs, name)
        {

        }
    }

    [DebuggerDisplay("{MethodName,nq}")]
    internal class XCodeMethodReferenceExpression : CodeMethodReferenceExpression
    {
        internal XCodeMethodReferenceExpression(CodeExpression lhs, string name) : base(lhs, name)
        {
        }
    }

    [DebuggerDisplay("{VariableName,nq}")]
    internal class XCodeVariableReferenceExpression : CodeVariableReferenceExpression
    {
        internal XCodeVariableReferenceExpression(string name) : base(name)
        {
        }
    }

    [DebuggerDisplay("{EventName,nq}")]
    internal class XCodeEventReferenceExpression : CodeEventReferenceExpression
    {
        internal XCodeEventReferenceExpression(CodeExpression lhs, string name) : base(lhs, name)
        {
        }
    }
    [DebuggerDisplay("SUPER")]
    internal class XCodeBaseReferenceExpression : CodeBaseReferenceExpression
    {
        internal XCodeBaseReferenceExpression() : base()
        {
        }
    }

    [DebuggerDisplay("SELF")]
    internal class XCodeThisReferenceExpression : CodeThisReferenceExpression
    {
        internal XCodeThisReferenceExpression() : base()
        {
        }
    }
}
