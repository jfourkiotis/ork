using ork.ast;

namespace ork;

public sealed class TreeWalkingInterpreter
{
    public object? Eval(INode node)
    {
        start:
        switch (node)
        {
            case Program program:
                object? result = null;
                foreach (var statement in program.Statements)
                {
                    result = Eval(statement);
                }

                return result;
            case ExpressionStatement expressionStatement:
                if (expressionStatement.Expression is null)
                    return null;
                node = expressionStatement.Expression;
                goto start;
            case IntegerLiteral val:
                return val.Value;
        }
        return null;
    }
}