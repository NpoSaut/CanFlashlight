using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFramesCodegen.CodeGeneration
{
    public class CodeBlock : CodeElement, IEnumerable<CodeElement>
    {
        protected LinkedList<CodeElement> SubElements { get; set; }

        public CodeBlock()
        {
            SubElements = new LinkedList<CodeElement>();
        }

        public void Add(CodeElement element)
        {
            SubElements.AddLast(element);
        }
        public void Add(CodeBlock element)
        {
            SubElements.AddLast(element);
        }
        public void Add(IEnumerable<CodeElement> elements)
        {
            foreach (var e in elements) Add(e);
        }
        public void Add(String line)
        { 
            Add(new CodeLine(line));
        }
        public void Add(String Format, params object[] args)
        {
            Add(new CodeLine(Format, args));
        }
        public override void Echo(StringBuilder sb, int OffsetIndex)
        {
            foreach (var e in SubElements)
                e.Echo(sb, OffsetIndex);
        }

        #region Члены IEnumerable<CodeElement>

        public IEnumerator<CodeElement> GetEnumerator()
        {
            return SubElements.GetEnumerator();
        }

        #endregion

        #region Члены IEnumerable

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return SubElements.GetEnumerator();
        }

        #endregion
    }
}
