using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using NHaml.Core.Ast;
using NHaml.Core.Template;

namespace NHaml.Core.Visitors
{
    /// <summary>
    /// Abstract class that creates the proper HTML result of the AST.
    /// </summary>
    public abstract class HtmlVisitor : NodeVisitorBase
    {
        public delegate void AstAction();

        public HtmlVisitor(TemplateOptions options)
        {
            Options = options;
        }

        public HtmlVisitor()
        {
            Options = new TemplateOptions();
        }

        public TemplateOptions Options { get; set; }
        public int Indent { get; set; }
        public string Format { get; set; }

        public override void Visit(DocumentNode node)
        {
            StartVisit(node);
            var first = true;
            foreach(var child in node.Childs)
            {
                if ((!first) && (!(child is CodeBlockNode)))
                    WriteText(System.Environment.NewLine);

                Visit(child);
                if ((!(child is MetaNode)) && (!(child is CodeBlockNode)))
                    first = false;
            }
            EndVisit(node);
        }

        protected virtual void EndVisit(DocumentNode node)
        {
        }

        protected virtual void StartVisit(DocumentNode node)
        {
        }

        public override void Visit(DocTypeNode node)
        {
            WriteText(GetDocType(node.Text));
        }

        private string GetDocType(string id)
        {
            if(string.IsNullOrEmpty(id))
                if(Format == "html4")
                    return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">";
                else if(Format == "html5")
                    return @"<!DOCTYPE html>";
                else
                    return
                        @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">";

            var arr = new List<string>(id.Split(' '));

            switch(arr[0])
            {
                case "1.1":
                {
                    return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">";
                }
                case "frameset":
                {
                    if(Format == "html4")
                        return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 4.01 Frameset//EN"" ""http://www.w3.org/TR/html4/frameset.dtd"">";

                    return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Frameset//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-frameset.dtd"">";
                }
                case "HTML":
                    {
                        if (arr.Count>1) {
                            switch (arr[1]) {
                                case "frameset":
                                    {
                                        return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 4.01 Frameset//EN"" ""http://www.w3.org/TR/html4/frameset.dtd"">";
                                    }
                                case "strict":
                                    {
                                        return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 4.01//EN"" ""http://www.w3.org/TR/html4/strict.dtd"">";
                                    }
                                case "transitional":
                                    {
                                        return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">";
                                    }
                            }
                        }
                        return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">";
                    }
                case "basic":
                {
                    return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML Basic 1.1//EN"" ""http://www.w3.org/TR/xhtml-basic/xhtml-basic11.dtd"">";
                }
                case "XML":
                {
                    if(Format == "html4" || Format == "html5")
                        return string.Empty;
                    var encoding = "utf-8";

                    if (arr.Count > 1)
                    {
                        encoding = arr[1];
                    }

                    return String.Format(@"<?xml version='1.0' encoding='{0}' ?>",encoding);
                }
                case "strict":
                {
                    if (Format == "html4")
                        return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 4.01//EN"" ""http://www.w3.org/TR/html4/strict.dtd"">";

                    return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">";
                }
                case "mobile":
                {
                    return
                        @"<!DOCTYPE html PUBLIC ""-//WAPFORUM//DTD XHTML Mobile 1.2//EN"" ""http://www.openmobilealliance.org/tech/DTD/xhtml-mobile12.dtd"">";
                }
                case "transitional":
                {
                    if (Format=="html4")
                        return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">";
                        
                    return @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">";
                }
                default:
                    throw new Exception("unknown doctype");
            }
        }

        public override void Visit(ChildrenNode node)
        {
            Indent++;

            VisitAndIdentAlways(node);

            Indent--;

            WriteIndent();
        }

        public override void Visit(TagNode node)
        {
            WriteText(String.Format("<{0}", node.Name));

            foreach(var attribute in SortAndJoinAttributes(node.Attributes))
            {
                WriteText(" ");
                Visit(attribute);
            }

            if(node.Child == null && (Options.IsAutoClosingTag(node.Name) || node.AutoClose))
            {
                if(Format == "html4" || Format == "html5")
                    WriteText(">");
                else
                    WriteText(" />");
                return;
            }

            WriteText(">");

            Visit(node.Child);

            WriteText(String.Format("</{0}>", node.Name));
        }

        public override void Visit(TextNode node)
        {
            foreach(var chunk in node.Chunks)
                Visit(chunk);
        }

        public override void Visit(TextChunk chunk)
        {
            WriteText(chunk.Text);
        }

        public override void Visit(CodeChunk chunk)
        {
            if (chunk.Escape.HasValue)
            {
                WriteCode(chunk.Code.Trim(), chunk.Escape.Value);
            }
            else
            {
                WriteCode(chunk.Code.Trim(), Options.EncodeHtml);
            }
        }

        public override void Visit(AttributeNode node)
        {
            WriteText(node.Name);
            WriteText("='");

            base.Visit(node);

            WriteText("'");
        }

        public override void Visit(CodeBlockNode node)
        {
            if (node.Child != null)
            {
                WriteStartBlock(node.Code.Trim(), true);
                VisitAndIdentAlways(node.Child,true);
                WriteEndBlock();
            }
            else
            {
                WriteStartBlock(node.Code.Trim(), false);
            }
        }

        public override void Visit(FilterNode node)
        {
            switch(node.Name)
            {
                case "javascript":
                {
                    WriteText(@"<script type='text/javascript'>");
                    WriteText(System.Environment.NewLine);
                    Indent++;
                    WriteIndent();
                    WriteText(@"//<![CDATA[");

                    Indent++;
                    VisitAndIdentAlways(node.Child);
                    Indent--;

                    WriteIndent();
                    WriteText(@"//]]>");
                    WriteText(System.Environment.NewLine);

                    Indent--;
                    WriteIndent();
                    WriteText(@"</script>");
                    break;
                }
                default:
                {
                    WriteData(Capture(() => VisitAndIdentOnlyWithMoreChilds(node.Child)), node.Name);
                    break;
                }
            }
        }

        public override void Visit(CommentNode node)
        {
            if(string.IsNullOrEmpty(node.Condition))
            {
                WriteText("<!--");
                var space = !( node.Child is ChildrenNode );
                if(space)
                    WriteText(" ");
                Visit(node.Child);
                if(space)
                    WriteText(" ");
                WriteText("-->");
            }
            else
            {
                WriteText(String.Format("<!--[{0}]>", node.Condition));
                Indent++;
                VisitAndIdentAlways(node.Child);
                Indent--;
                WriteIndent();
                WriteText("<![endif]-->");
            }
        }

        public override void Visit(CodeNode node)
        {
            if (!node.Escape.HasValue)
            {
                WriteCode(node.Code.Trim(), Options.EncodeHtml);
            }
            else
            {
                WriteCode(node.Code.Trim(), node.Escape.Value);
            }
        }

        protected void VisitAndIdentAlways(AstNode node, bool codeblockcaller)
        {
            if (node == null)
                return;

            if (!codeblockcaller)
                WriteText(System.Environment.NewLine);

            if (node is ChildrenNode)
                foreach (var childrenNode in (ChildrenNode)node)
                {
                    if (!(childrenNode is CodeBlockNode) && !(childrenNode is MetaNode))
                    {
                        WriteIndent();
                    }
                    Visit(childrenNode);
                    if (!(childrenNode is CodeBlockNode) && !(childrenNode is MetaNode))
                    {
                        WriteText(System.Environment.NewLine);
                    }
                }
            else
            {
                if (!codeblockcaller)
                {
                    WriteIndent();
                }
                Visit(node);
                if (!codeblockcaller)
                {
                    WriteText(System.Environment.NewLine);
                }
            }
        }

        protected void VisitAndIdentAlways(AstNode node)
        {
            VisitAndIdentAlways(node, false);
        }

        protected void VisitAndIdentOnlyWithMoreChilds(AstNode node)
        {
            if(node == null)
                return;

            if(node is ChildrenNode)
            {
                var children = ( (ChildrenNode)node );
                if(children.Count > 1)
                    VisitAndIdentAlways(node);
                else
                    foreach(var child in children)
                        node = child;
            }

            Visit(node);
        }

        private bool TryGetSimpleTextNode(TextNode node, out string textvalue)
        {
            textvalue = null;
            bool issimple = ((node.Chunks.Count == 0) || ((node.Chunks.Count == 1) && (node.Chunks[0] is TextChunk)));
            if (issimple)
            {
                textvalue = "";
                if (node.Chunks.Count == 1)
                    textvalue = (node.Chunks[0] as TextChunk).Text;
            }
            return issimple;
        }

        public override void Visit(LateBindingNode node) 
        {
            WriteData(node.Value,null);
        }

        private IEnumerable<AttributeNode> SortAndJoinAttributes(IEnumerable<AttributeNode> inputAttributes)
        {
            var queue = new List<AttributeNode>(inputAttributes);

            var attributes = new List<AttributeNode>();
            while (queue.Count > 0)
            {
                var first = queue[0];
                queue.RemoveAt(0);
                attributes.Add(first);

                var isId = first.Name.Equals("id", StringComparison.InvariantCultureIgnoreCase);

                var value = first.Value;
                if (value == null)
                {
                    value = new TextNode(new TextChunk(""));
                }

                var buf = new List<object> { Capture(value) };

                foreach (var sameAtt in queue.FindAll(a => a.Name == first.Name))
                {
                    queue.Remove(sameAtt);
                    buf.Add(Capture(sameAtt.Value));
                }

                first.Value = DataJoiner(isId ? "_" : " ", buf.ToArray(), !isId);
            }

            attributes.Sort((a1, a2) => a1.Name.CompareTo(a2.Name));

            return attributes;
        }

        public virtual void WriteIndent()
        {
            if (Options.UseTabs)
            {
                WriteText(new String('\t', Options.IndentSize * (Indent + Options.BaseIndent)));
            }
            else
            {
                WriteText(new String(' ', Options.IndentSize * (Indent + Options.BaseIndent)));
            }
        }

        public object Capture(AstNode node)
        {
            if(node == null)
                throw new ArgumentNullException("node");

            return Capture(() => Visit(node));
        }

        public object Capture(AstAction action)
        {
            if(action == null)
                throw new ArgumentNullException("action");

            PushWriter();
            action();
            return PopWriter();
        }

        /// <summary>
        /// Writes simple string to the output
        /// </summary>
        /// <param name="text">The text to write</param>
        protected abstract void WriteText(string text);

        /// <summary>
        /// Writes code fragments to the output
        /// </summary>
        /// <param name="code">The code to write</param>
        protected abstract void WriteCode(string code, bool escapeHtml);

        /// <summary>
        /// Creates a new writer, saves the old one on a stack
        /// </summary>
        protected abstract void PushWriter();

        /// <summary>
        /// Pops the writer returning the associated data
        /// </summary>
        /// <returns>Associated data</returns>
        protected abstract object PopWriter();

        /// <summary>
        /// Starts a new code block with the code fragment given
        /// </summary>
        /// <param name="code"></param>
        protected abstract void WriteStartBlock(string code, bool hasChild);

        /// <summary>
        /// Ends the code block
        /// </summary>
        protected abstract void WriteEndBlock();

        /// <summary>
        /// Write Visitor specific data back
        /// </summary>
        protected abstract void WriteData(object data, string filter);

        /// <summary>
        /// Joins visitor specific data using the specified string sorting them if nescessary
        /// </summary>
        protected abstract LateBindingNode DataJoiner(string joinString, object[] data, bool sort);
    }
}