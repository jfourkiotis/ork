using ork.ast;
using ork.builtins;

namespace ork;

using static Object;

public sealed class TreeWalkingInterpreter
{
    private bool ret;

    public object? Eval(INode node, Environment env)
    {
        start:
        switch (node)
        {
            case Program program:
                object? result = null;
                foreach (var statement in program.Statements)
                {
                    result = Eval(statement, env);
                    if (ret)
                    {
                        return result;
                    }
                }

                return result;
            case ExpressionStatement expressionStatement:
                if (expressionStatement.Expression is null)
                    return null;
                node = expressionStatement.Expression;
                goto start;
            case LetStatement letStatement:
                var value = Eval(letStatement.Expression, env);
                env.Set(letStatement.Name.TokenLiteral, value);
                break;
            case Identifier id:
                if (env.TryGet(id.TokenLiteral, out object? idval))
                {
                    return idval;
                } else if (Builtin.Functions.TryGetValue(id.TokenLiteral, out var f))
                {
                    return f;
                }
                throw new OrkRuntimeException($"identifier not found: {id.TokenLiteral}");
            case PrefixExpression prefixExpression:
            {
                var rhs = Eval(prefixExpression.Rhs, env);
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
                        _ => throw new OrkRuntimeException(
                            $"unknown operator: {prefixExpression.TokenLiteral}{TypeName(rhs)}"),
                    };
                }
                throw new OrkRuntimeException($"unknown operator: {prefixExpression.TokenLiteral}{TypeName(rhs)}");
                
            }
            case InfixExpression infixExpression:
            {
                var lhs = Eval(infixExpression.Lhs, env);
                var rhs = Eval(infixExpression.Rhs, env);
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
                        _ => throw new OrkRuntimeException(
                            $"unknown operator: {TypeName(a)} {infixExpression.TokenLiteral} {TypeName(b)}"),
                    };
                } else if (lhs is string s1 && rhs is string s2)
                {
                    return infixExpression.TokenLiteral switch
                    {
                        "+" => s1 + s2,
                        _ => throw new OrkRuntimeException(
                            $"unknown operator: {TypeName(s1)} {infixExpression.TokenLiteral} {TypeName(s2)}"),
                    };
                } else if (lhs is bool b1 && rhs is bool b2)
                {
                    return infixExpression.TokenLiteral switch
                    {
                        "==" => b1 == b2,
                        "!=" => b1 != b2,
                        _ => throw new OrkRuntimeException(
                            $"unknown operator: {TypeName(lhs)} {infixExpression.TokenLiteral} {TypeName(rhs)}"),
                    };
                } else if (lhs?.GetType() != rhs?.GetType())
                {
                    throw new OrkRuntimeException(
                        $"type mismatch: {TypeName(lhs)} {infixExpression.TokenLiteral} {TypeName(rhs)}");
                }
  
                throw new OrkRuntimeException(
                        $"unknown operator: {TypeName(lhs)} {infixExpression.TokenLiteral} {TypeName(rhs)}");
                
            }
            case IfExpression ifExpression:
                var conditionResult = Eval(ifExpression.Condition, env);
                if (conditionResult is null or false)
                {
                    if (ifExpression.Else is not null)
                    {
                        node = ifExpression.Else;
                        goto start;
                    }
                }
                else
                {
                    node = ifExpression.Then;
                    goto start;
                }

                break;
            case BlockStatement blockStatement:
                object? blockResult = null;
                foreach (var statement in blockStatement.Statements)
                {
                    blockResult = Eval(statement, env);
                    if (ret)
                    {
                        return blockResult;
                    }
                }

                return blockResult;
            case ReturnStatement returnStatement:
                object? returnValue = null;
                if (returnStatement.Expression is not null)
                {
                    returnValue = Eval(returnStatement.Expression, env);
                }
                ret = true;
                
                return returnValue;
            case FunctionLiteral functionLiteral:
                return new Function(functionLiteral.Parameters, functionLiteral.Body, env);
            case CallExpression callExpression:
                var fn = Eval(callExpression.Function, env);
                switch (fn)
                {
                    case Function f:
                        Environment fenv = new Environment(f.Environment);
                        int index1 = 0;
                        foreach (var v in callExpression.Arguments)
                        {
                            fenv.Set(f.Parameters[index1].TokenLiteral, Eval(v, env));
                            index1++;
                        }
                        node = f.Body;
                        env = fenv;
                        goto start;
                    case Func<object?[], object?> b:
                        return b(callExpression.Arguments.Select(a => Eval(a, env)).ToArray());
                    default:
                        throw new OrkRuntimeException($"not a function: {TypeName(fn)}");
                }
            case IntegerLiteral val:
                return val.Value;
            case TrueLiteral:
                return true;
            case FalseLiteral:
                return false;
            case StringLiteral stringLiteral:
                return stringLiteral.TokenLiteral;
        }
        return null;
    }
}