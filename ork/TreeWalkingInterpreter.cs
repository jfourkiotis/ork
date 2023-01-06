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
            case PrefixExpression prefixExpression:
            {
                var rhs = Eval(prefixExpression.Rhs);
                if (prefixExpression.TokenLiteral == "!")
                {
                    return rhs switch
                    {
                        true => false,
                        false => true,
                        null => true,
                        _ => false,
                    };
                } else if (prefixExpression.TokenLiteral == "-")
                {
                    return rhs switch
                    {
                        Int64 val => -val,
                        _ => null,
                    };
                }
                break;
            }
            case InfixExpression infixExpression:
            {
                var lhs = Eval(infixExpression.Lhs);
                var rhs = Eval(infixExpression.Rhs);
                if (lhs is long a && rhs is long b)
                {
                    return infixExpression.TokenLiteral switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => a / b,
                        ">" => a > b,
                        "<" => a < b,
                        "==" => a == b,
                        "!=" => a != b,
                        _ => null,
                    };
                } else if (lhs is bool b1 && rhs is bool b2)
                {
                    return infixExpression.TokenLiteral switch
                    {
                        "==" => b1 == b2,
                        "!=" => b1 != b2,
                        _ => null,
                    };
                }
                break;
            }
            case IntegerLiteral val:
                return val.Value;
            case TrueLiteral:
                return true;
            case FalseLiteral:
                return false;
        }
        return null;
    }
}