using ork.ast;
using ork.builtins;
using System.Collections.Immutable;

namespace ork;

using static Object;

public sealed class TreeWalkingInterpreter
{
    private bool ret;

    public object? Eval(Program program, Environment env)
    {
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
    }

    private object? Eval(Node node, Environment env)
    {
        start:
        switch (node.Tag)
        {
            case AstTag.ExpressionStatement:
                ExpressionStatement expressionStatement = (ExpressionStatement)node;
                if (expressionStatement.Expression is null)
                    return null;
                node = expressionStatement.Expression;
                goto start;
            case AstTag.LetStatement:
                LetStatement letStatement = (LetStatement)node;
                var value = Eval(letStatement.Expression, env);
                env.Set(letStatement.Name.TokenLiteral, value);
                break;
            case AstTag.Identifier:
                Identifier id = (Identifier)node;
                if (env.TryGet(id.TokenLiteral, out object? idval))
                {
                    return idval;
                } else if (Builtin.Functions.TryGetValue(id.TokenLiteral, out var f))
                {
                    return f;
                }
                throw new OrkRuntimeException($"identifier not found: {id.TokenLiteral}");
            case AstTag.PrefixExpression:
                PrefixExpression prefixExpression = (PrefixExpression)node;
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
                    }
                    else if (prefixExpression.TokenLiteral == "-")
                    {
                        return rhs switch
                        {
                            Int64 ival => -ival,
                            _ => throw new OrkRuntimeException(
                                $"unknown operator: {prefixExpression.TokenLiteral}{TypeName(rhs)}"),
                        };
                    }
                    throw new OrkRuntimeException($"unknown operator: {prefixExpression.TokenLiteral}{TypeName(rhs)}");
                }
            case AstTag.InfixExpression:
                InfixExpression infixExpression = (InfixExpression)node;
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
                    }
                    else if (lhs is string s1 && rhs is string s2)
                    {
                        return infixExpression.TokenLiteral switch
                        {
                            "+" => s1 + s2,
                            "==" => s1 == s2,
                            "!=" => s1 != s2,
                            _ => throw new OrkRuntimeException(
                                $"unknown operator: {TypeName(s1)} {infixExpression.TokenLiteral} {TypeName(s2)}"),
                        };
                    }
                    else if (lhs is bool b1 && rhs is bool b2)
                    {
                        return infixExpression.TokenLiteral switch
                        {
                            "==" => b1 == b2,
                            "!=" => b1 != b2,
                            _ => throw new OrkRuntimeException(
                                $"unknown operator: {TypeName(lhs)} {infixExpression.TokenLiteral} {TypeName(rhs)}"),
                        };
                    }
                    else if (lhs?.GetType() != rhs?.GetType())
                    {
                        throw new OrkRuntimeException(
                            $"type mismatch: {TypeName(lhs)} {infixExpression.TokenLiteral} {TypeName(rhs)}");
                    }

                    throw new OrkRuntimeException(
                            $"unknown operator: {TypeName(lhs)} {infixExpression.TokenLiteral} {TypeName(rhs)}");
                }
            case AstTag.IfExpression:
                IfExpression ifExpression = (IfExpression)node;
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
            case AstTag.BlockStatement:
                BlockStatement blockStatement = (BlockStatement)node;
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
            case AstTag.ReturnStatement:
                ReturnStatement returnStatement = (ReturnStatement)node;
                object? returnValue = null;
                if (returnStatement.Expression is not null)
                {
                    returnValue = Eval(returnStatement.Expression, env);
                }
                ret = true;
                
                return returnValue;
            case AstTag.FunctionLiteral:
                FunctionLiteral functionLiteral = (FunctionLiteral)node;
                return new Function(functionLiteral.Parameters, functionLiteral.Body, env);
            case AstTag.CallExpression:
                CallExpression callExpression = (CallExpression)node;
                var fn = Eval(callExpression.Function, env);
                switch (fn)
                {
                    case Function f:
                        Environment fenv = new Environment(f.Environment);
                        if (callExpression.Arguments.Count != f.Parameters.Count)
                        {
                            throw new OrkRuntimeException($"invalid number of arguments: expected {f.Parameters.Count} but {callExpression.Arguments.Count} were given");
                        }
                        int index1 = 0;
                        foreach (var v in callExpression.Arguments)
                        {
                            fenv.Set(f.Parameters[index1].TokenLiteral, Eval(v, env));
                            index1++;
                        }
                        var br = Eval(f.Body, fenv);
                        if (ret)
                            ret = false;
                        return br;
                    case Func<object?[], object?> b:
                        return b(callExpression.Arguments.Select(a => Eval(a, env)).ToArray());
                    default:
                        throw new OrkRuntimeException($"not a function: {TypeName(fn)}");
                }
            case AstTag.IndexExpression:
                IndexExpression indexExpression = (IndexExpression)node;
                var left = Eval(indexExpression.Left, env);
                var index = Eval(indexExpression.Index, env);
                if (left is ImmutableArray<object?> l && index is long i)
                {
                    if (i < 0 || i > l.Length - 1)
                        return null;
                    return l[(int)i]; // TODO
                }
                else if (left is ImmutableDictionary<object, object?> d && index is not null)
                {
                    return !d.TryGetValue(index, out var dval) ? null : dval;
                } 
                else if (left is string s && index is long a)
                {
                    if (a < 0 || a > s.Length - 1)
                        return null;
                    return s[(int)a].ToString(); // TODO
                }
                throw new OrkRuntimeException($"index operator not supported: {TypeName(left)}");
            case AstTag.HashLiteral:
                HashLiteral hashLiteral = (HashLiteral)node;
                Dictionary<object, object?> pairs = new();
                foreach (var (k, v) in hashLiteral.Pairs)
                {
                    var key = Eval(k, env);
                    if (key is null)
                        throw new OrkRuntimeException($"a key may not be NIL");
                    if (key is not long && key is not string && key is not bool)
                        throw new OrkRuntimeException($"unusable as hash key: {TypeName(key)}");
                    var dvalue = v is not null ? Eval(v, env) : null;
                    pairs[key] = dvalue;
                }
                return pairs.ToImmutableDictionary();
            case AstTag.IntegerLiteral:
                IntegerLiteral val = (IntegerLiteral)node; 
                return val.Value;
            case AstTag.TrueLiteral:
                return true;
            case AstTag.FalseLiteral:
                return false;
            case AstTag.StringLiteral:
                StringLiteral stringLiteral = (StringLiteral)node;
                return stringLiteral.TokenLiteral;
            case AstTag.ArrayLiteral:
                ArrayLiteral arrayLiteral = (ArrayLiteral)node;
                return arrayLiteral.Elements.Select(a => Eval(a, env)).ToImmutableArray();
        }
        return null;
    }
}