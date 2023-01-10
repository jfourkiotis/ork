using ork.ast;
using System.Text;

namespace ork
{
    public sealed class Function
    {
        public Function(IList<Identifier> parameters, BlockStatement body, Environment env)
        {
            Parameters = parameters;
            Body = body;
            Environment = env;
        }

        public IList<Identifier> Parameters { get; }
        public BlockStatement Body { get; }
        public Environment Environment { get; }
        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append("fn");
            sb.Append('(');
            sb.Append(String.Join(',', Parameters));
            sb.Append(')');
            sb.Append(Body);
            sb.Append('\n'); // this is strange ...
            return sb.ToString();
        }
    }
}
