using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using P8LuaGrammar;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSharp
{
    public class LuaListener
    {
        private ICharStream input;
        private string source;

        public LuaListener(ILuaListener listener, ICharStream input, string source)
            : base()
        {
            this.input = input;
            this.source = source;
            Listener = listener;
            Listener.Parent = this;
        }

        public string ReplaceAll([NotNull] ParserRuleContext context)
        {
            GetReplacements(context);

            var newSource = source;
            while (Replacements.Count > 0)
            {
                var replacement = Replacements.Pop();
                newSource = replacement.Replace(newSource);
            }
            return newSource;
        }

        public void GetReplacements([NotNull] ParserRuleContext context)
        {
            Replacements = new Stack<Replacement>();
            ParseTreeWalker.Default.Walk((IParseTreeListener)Listener, context);
        }

        private string GetText([NotNull] IParseTree context)
        {
            if (context is ParserRuleContext)
            {
                int a = ((ParserRuleContext)context).Start.StartIndex;
                int b = ((ParserRuleContext)context).Stop.StopIndex;
                Interval interval = new Interval(a, b);
                return input.GetText(interval);
            }
            return context.GetText();
        }

        public virtual void EnterCompoundStatement([NotNull] ParserRuleContext context)
        {
            var lhs = GetText(context.children[0]);
            var opAssign = GetText(context.children[1]);
            var rhs = GetText(context.children[2]);

            string op;

            switch (opAssign[0])
            {
                default:
                    throw new InvalidOperationException(
                        string.Format("Invalid operator in compound statement '{0}' on line {1}", opAssign, context.Start.Line)
                    );
                case '+':
                case '-':
                case '*':
                case '/':
                case '%':
                    op = opAssign.Substring(0, 1);
                    break;
            }

            var node = context.children[2];
            while (node.ChildCount == 1)
            {
                node = node.GetChild(0);
            }
            var isLeaf = node.ChildCount == 0;

            Replacements.Push(
                new Replacement(
                    context.Start.StartIndex,
                    context.Stop.StopIndex,
                    string.Format(isLeaf ? "{0} = {0} {1} {2}" : "{0} = {0} {1} ({2})", lhs, op, rhs)
                )
            );
        }

        public virtual void EnterOperatorComparison([NotNull] ParserRuleContext context)
        {
            if (context.GetText() == "!=")
            {
                Replacements.Push(
                    new Replacement(
                        context.Start.StartIndex,
                        context.Stop.StopIndex,
                        "~="
                    )
                );
            }
        }

        public virtual void EnterOperatorMulDivMod([NotNull] ParserRuleContext context)
        {
            if (context.GetText() == "\\")
            {
                Replacements.Push(
                    new Replacement(
                        context.Start.StartIndex,
                        context.Stop.StopIndex,
                        "//"
                    )
                );
            }
        }

        public void EnterNumber([NotNull] ParserRuleContext context)
        {
            var literal = context.GetText().ToLowerInvariant();
            if (literal.StartsWith("0b"))
            {
                var tokens = literal.Substring(2).Split(new char[] { '.' }, StringSplitOptions.None);
                var integerStr = tokens[0];
                var fractionStr = tokens.Length > 1 ? tokens[1] : "";

                while (integerStr.Length % 4 != 0)
                {
                    integerStr = "0" + integerStr;
                }
                while (fractionStr.Length % 4 != 0)
                {
                    fractionStr += "0";
                }

                var str = new StringBuilder("0x");

                for (int i = 0; i < integerStr.Length; i += 4)
                {
                    var num =
                        (integerStr[i + 3] == '1' ? 1 : 0) |
                        (integerStr[i + 2] == '1' ? 2 : 0) |
                        (integerStr[i + 1] == '1' ? 4 : 0) |
                        (integerStr[i + 0] == '1' ? 8 : 0)
                        ;
                    str.Append(num.ToString("x1", CultureInfo.InvariantCulture));
                }

                if (fractionStr.Length > 0)
                {
                    str.Append(".");

                    for (int i = 0; i < fractionStr.Length; i += 4)
                    {
                        var num =
                            (fractionStr[i + 3] == '1' ? 1 : 0) |
                            (fractionStr[i + 2] == '1' ? 2 : 0) |
                            (fractionStr[i + 1] == '1' ? 4 : 0) |
                            (fractionStr[i + 0] == '1' ? 8 : 0)
                            ;
                        str.Append(num.ToString("x1", CultureInfo.InvariantCulture));
                    }
                }

                Replacements.Push(
                    new Replacement(
                        context.Start.StartIndex,
                        context.Stop.StopIndex,
                        str.ToString()
                    )
                );
            }
        }

        public void EnterFunctioncall([NotNull] ParserRuleContext context)
        {
            /* var func = GetText(context.children[0]);

            if (func == "foreach")
            {
                Replacements.Push(
                    new Replacement(
                        ((ParserRuleContext)context.children[0]).start.StartIndex,
                        ((ParserRuleContext)context.children[0]).stop.StopIndex,
                        string.Format("_foreach")
                    )
                );
            } */
        }

        public Stack<Replacement> Replacements { get; set; }
        public ILuaListener Listener { get; set; }
    }
}
