using System.IO;

namespace Hai.Project12.TF.Editor
{
    internal class H12TFWriter
    {
        private const int IndentationSpaces = 4;
        private readonly StringWriter _writer;
        private readonly TFRoot _tfRoot;

        internal H12TFWriter(StringWriter writer, TFRoot tfRoot)
        {
            _writer = writer;
            _tfRoot = tfRoot;
        }

        internal string WriteAll()
        {
            var indentationLevel = 0;

            foreach (var tfElt in _tfRoot.Elements)
            {
                PrintElement(tfElt, ref indentationLevel);
            }

            return _writer.ToString();
        }

        private void PrintElement(ITFElt tfElt, ref int indentationLevel)
        {
            switch (tfElt)
            {
                case TFBrace brace:
                {
                    if (!string.IsNullOrWhiteSpace(brace.HeaderNullableOrEmpty))
                    {
                        PrintIndentation(indentationLevel);
                        _writer.WriteLine(brace.HeaderNullableOrEmpty);
                    }
                    PrintIndentation(indentationLevel);
                    _writer.WriteLine("{");

                    indentationLevel++;
                    foreach (var treeElement in brace.Elements)
                    {
                        PrintElement(treeElement, ref indentationLevel);
                    }
                    indentationLevel--;

                    PrintIndentation(indentationLevel);
                    _writer.WriteLine("}");
                    break;
                }
                case TFLine line:
                    PrintIndentation(indentationLevel);
                    _writer.WriteLine(line.Line);
                    break;
                case TFEmptyLine:
                    _writer.WriteLine("");
                    break;
                case TFSection section:
                {
                    foreach (var treeElement in section.Elements)
                    {
                        PrintElement(treeElement, ref indentationLevel);
                    }

                    break;
                }
                case TFUnindentedLine unindentedLine:
                {
                    _writer.WriteLine(unindentedLine.Line);
                    break;
                }
            }
        }

        private void PrintIndentation(int indentationLevel)
        {
            _writer.Write(new string(' ', indentationLevel * IndentationSpaces));
        }
    }
}
