using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace RefactoringEssentials.Tests
{
    public abstract class CodeFixTestBase
    {
        //[SetUp]
        //public virtual void SetUp()
        //{
        //}

        internal static string HomogenizeEol(string str)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                var possibleNewline = NewLine.GetDelimiterLength(ch, i + 1 < str.Length ? str[i + 1] : '\0');
                if (possibleNewline > 0)
                {
                    sb.AppendLine();
                    if (possibleNewline == 2) {
                        i++;
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        protected static string ParseText(string input, out TextSpan selectedSpan)
        {
            int start = -1, end = -1;
            var result = new StringBuilder(input.Length);
            int upper = input.Length;
            for (int i = 0; i < upper; i++)
            {
                var ch = input[i];
                if (ch == '$' && start < 0)
                {
                    start = i;
                    continue;
                }
                else if (ch == '$' && end < 0)
                {
                    end = i - 1;
                    continue;
                }
                result.Append(ch);
            }

            if (start < 0)
            {
                selectedSpan = TextSpan.FromBounds(0, 0);
            }
            else
            {
                selectedSpan = TextSpan.FromBounds(start, end);
            }
            return result.ToString();
        }

        //		protected List<CodeAction> GetActions<T> (string input) where T : CodeActionProvider, new ()
        //		{
        //			var ctx = TestRefactoringContext.Create(input);
        //			ctx.FormattingOptions = formattingOptions;
        //			return new T().GetActions(ctx).ToList();
        //		}
        //
        //		protected void TestActionDescriptions (CodeActionProvider provider, string input, params string[] expected)
        //		{
        //			var ctx = TestRefactoringContext.Create(input);
        //			ctx.FormattingOptions = formattingOptions;
        //			var actions = provider.GetActions(ctx).ToList();
        //			Assert.AreEqual(
        //				expected,
        //				actions.Select(a => a.Description).ToArray());
        //		}
    }
}
