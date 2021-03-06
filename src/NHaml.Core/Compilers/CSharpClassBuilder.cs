﻿using System;
using System.Collections.Generic;
using System.Text;
using NHaml.Core.Visitors;
using NHaml.Core.Ast;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Text.RegularExpressions;

namespace NHaml.Core.Compilers
{
    public class CSharpClassBuilder : CodeDomVisitorClassBuilder
    {
        internal class CSharpVisitor : CodeDomVisitor
        {
            private Regex _lambdaRegex;

            protected override CodeObject StartBlock { get { return new CodeSnippetExpression("{//"); } }
            protected override CodeObject EndBlock { get { return new CodeSnippetExpression("}//"); } }
            protected override CodeObject LambdaEndBlock { get { return new CodeSnippetExpression("})"); } }
            protected override bool SupportLambda { get { return true; } }

            protected override Regex LambdaRegex
            {
                get
                {
                    if (_lambdaRegex == null)
                    {
                        _lambdaRegex = new Regex(@"^(.+)(\(.*\))\s*=>\s*$", RegexOptions.Compiled | RegexOptions.Singleline);
                    }
                    return _lambdaRegex;
                }
            }

            protected override string TranslateLambda( string codeLine, Match lambdaMatch )
            {
                var part0 = codeLine.Substring(0, lambdaMatch.Groups[1].Length - 2);
                var part2 = lambdaMatch.Groups[2].Captures[0].Value;
                var part1 = (lambdaMatch.Groups[1].Captures[0].Value.Trim().EndsWith("()", StringComparison.OrdinalIgnoreCase) ? null : ", ");
                return string.Format("{0}{1}delegate{2}{{", part0, part1, part2);
            }

            protected override string Comment
            {
                get { return "//"; }
            }
        }

        CodeDomVisitor _visitor;
        CodeDomProvider _provider;
        CompilerParameters _options;
        CodeGeneratorOptions _generator;

        protected override CodeDomProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    _provider = new CSharpCodeProvider(new Dictionary<string, string>()
                        { 
                            {"CompilerVersion", "v2.0"}
                        }
                    );
                }
                return _provider;
            }
        }

        protected override CodeDomVisitor Visitor
        {
            get {
                if (_visitor == null)
                {
                    _visitor = new CSharpVisitor();
                }
                return _visitor;
            }
        }

        protected override CompilerParameters CompilerOptions
        {
            get {
                if (_options == null)
                {
                    _options = new CompilerParameters();
                }
                return _options;
            }
        }

        protected override CodeGeneratorOptions GeneratorOptions
        {
            get {
                if (_generator == null)
                {
                    _generator = new CodeGeneratorOptions();
                }
                return _generator;
            }
        }

        protected override bool SupportsDebug()
        {
            return true;
        }
    }
}
