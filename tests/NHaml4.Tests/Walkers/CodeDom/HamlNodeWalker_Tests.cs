﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NHaml4.Compilers;
using NHaml4.Parser;
using NHaml4.Walkers.CodeDom;
using NUnit.Framework;
using NHaml4.IO;
using NHaml4.Parser.Rules;
using NHaml4.Tests.Mocks;

namespace NHaml4.Tests.Walkers.CodeDom
{
    internal class HamlNodeWalker_Tests
    {
        private ClassBuilderMock _classBuilderMock;
        private DummyWalker _walker;

        private class DummyWalker : HamlNodeWalker
        {
            public DummyWalker(ITemplateClassBuilder classBuilder, HamlOptions options)
                : base(classBuilder, options)
            { }
        }

        [SetUp]
        public void SetUp()
        {
            _classBuilderMock = new ClassBuilderMock();
            _walker = new DummyWalker(_classBuilderMock, new HamlOptions());
        }

        [Test]
        public void WalkChildren_TextNode_WalksTextNode()
        {
            const string testText = "Hello world";
            var document = new HamlDocument { new HamlNodeText(new HamlLine(testText)) };
            _walker.Walk(document);

            Assert.That(_classBuilderMock.Build(""), Is.StringContaining(testText));
        }

        [Test]
        public void WalkChildren_TagNode_WalksTagNode()
        {
            const string tagName = "div";
            var document = new HamlDocument { new HamlNodeTag(new HamlLine(tagName)) };
            _walker.Walk(document);

            Assert.That(_classBuilderMock.Build(""), Is.StringContaining(tagName));
        }

        [Test]
        public void WalkChildren_HtmlCommentNode_WalksHtmlCommentNode()
        {
            const string comment = "test";
            var document = new HamlDocument {
                new HamlNodeHtmlComment(new HamlLine(comment))
            };
            _walker.Walk(document);

            Assert.That(_classBuilderMock.Build(""), Is.StringContaining(comment));
        }
    }
}
