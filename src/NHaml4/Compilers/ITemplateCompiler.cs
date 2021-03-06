using NHaml;
using System.Collections.Generic;
using System;

namespace NHaml4.Compilers
{
    public interface ITemplateFactoryCompiler
    {
        TemplateFactory Compile(string templateCode, string className, IList<Type> references);
    }
}