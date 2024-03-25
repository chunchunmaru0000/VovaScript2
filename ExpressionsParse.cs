﻿using System;
using System.Collections.Generic;

namespace VovaScript
{
    public partial class Parser
    {
        private static Token Zero = new Token() { Type = TokenType.INTEGER, Value = 0, View = "0" };
        private static Token End = new Token() { Type = TokenType.STRING, Value = "КОНЕЦ", View = "КОНЕЦ" };

        private IExpression FuncParsy()
        {
            Token name = Consume(TokenType.VARIABLE);
            Consume(TokenType.LEFTSCOB);
            FunctionExpression function = new FunctionExpression(name);
            while (!Match(TokenType.RIGHTSCOB))
            {
                function.AddArg(Expression());
                Sep();
            }
            return function;
        }

        private IExpression Newy()
        {
            Token className = Consume(TokenType.VARIABLE);
            Consume(TokenType.LEFTSCOB);
            List<IStatement> assigns = new List<IStatement>();
            while (!Match(TokenType.RIGHTSCOB))
            {
                assigns.Add(Statement());
                Sep();
            }
            return new NewObjectExpression(className, assigns.ToArray());
        }

        private IExpression Listy()
        {
            List<IExpression> items = new List<IExpression>();
            while (!Match(TokenType.RCUBSCOB, TokenType.RIGHTSCOB))
            {
                items.Add(Expression());
                Match(TokenType.COMMA, TokenType.SEMICOLON);
            }
            return new ListExpression(items);
        }

        private IExpression Scoby()
        {
            IExpression result = Expression();
            if (Match(TokenType.RIGHTSCOB))
                return result;
            if (Match(TokenType.QUESTION))
            {
                IExpression pravda = Expression();
                Consume(TokenType.COLON);
                IExpression nepravda = Expression();
                Consume(TokenType.RIGHTSCOB);
                return new ShortIfExpression(result, pravda, nepravda);
            }
            throw new NotImplementedException($"{Near(6)}НЕВЕРНЫЙ СИНТАКСИС ГДЕ-ТО РЯДОМ С: {result}");
        }

        private IExpression SQLy()
        {
            List<IExpression> selections = new List<IExpression>();
            List<IExpression> ats = new List<IExpression>();
            List<IExpression> aliases = new List<IExpression>();
            while (!Match(TokenType.FROM)) 
            { 
                selections.Add(Expression());
                if (Match(TokenType.AT))
                    ats.Add(Expression());
                else
                    ats.Add(Nothingness);
                if (Match(TokenType.AS))
                    aliases.Add(Addity());
                else
                    aliases.Add(Nothingness);
                Match(TokenType.COMMA, TokenType.AND);
            }

            List<IExpression> froms = new List<IExpression>();
            while (!Match(TokenType.SEMICOLON, TokenType.WHERE, TokenType.EOF))
            {
                froms.Add(Expression());
                Match(TokenType.COMMA, TokenType.AND);
            }

            List<Token> condition = null;
            if (Get(-1).Type == TokenType.WHERE)
            {
                condition = new List<Token>();
                while (!Match(TokenType.SEMICOLON, TokenType.WHERE, TokenType.EOF))
                    condition.Add(Consume(Current.Type));
            }

            return new SQLSelectExpression(selections, ats, aliases, froms, condition.ToArray());
        }

        private IExpression Lambdy()
        {
            List<Token> args = new List<Token>();
            while (!Match(TokenType.COLON))
            {
                args.Add(Consume(TokenType.VARIABLE));
                Sep();
            }
            IExpression ret = Expression();
            return new LambdaExpression(args.ToArray(), ret);
        }

        private IExpression Primary()
        {
            Token current = Current;

            if (Match(TokenType.NEW))
                return Newy();

            if (current.Type == TokenType.VARIABLE && Get(1).Type == TokenType.LEFTSCOB)
                return FuncParsy();

            if (Match(TokenType.ARROW, TokenType.LAMBDA))
                return Lambdy();

            if (Match(TokenType.SELECT))
                return SQLy();

            if (Match(TokenType.ALL))
                return All;

            if (Match(TokenType.LCUBSCOB))
                return Listy();

            if (Match(TokenType.NOW))
                return new NowExpression();

            if (Match(TokenType.STRING))
                return new NumExpression(current);

            if (Match(TokenType.WORD_TRUE, TokenType.WORD_FALSE))
                return new NumExpression(current);

            if (Match(TokenType.INTEGER, TokenType.DOUBLE))
                return new NumExpression(current);

            if (Match(TokenType.LEFTSCOB))
                return Scoby();

            if (Match(TokenType.VARIABLE))
                return new VariableExpression(current);
            // need improve
            if (Match(TokenType.PLUSPLUS, TokenType.MINUSMINUS))
            {
                Token name = Current;
                IExpression result = new IncDecBeforeExpression(current, name);
                Consume(TokenType.VARIABLE);
                return result;
            }
            return (IExpression)Statement();
            throw new Exception($"{Near(6)}НЕВОЗМОЖНОЕ МАТЕМАТИЧЕСКОЕ ВЫРАЖЕНИЕ: <{current}>\nПОЗИЦИЯ: ЛИНИЯ<{line}> СИМВОЛ<{position}>");
        }
        // 1000iq
        // can be better if would be like in python [::2] something
        private IExpression Aftery()
        {
            IExpression result = Primary();
            while (true)
            {
                if (Match(TokenType.DOT))
                {
                    Token attr = Consume(Current.Type);
                    if (Match(TokenType.LEFTSCOB))
                    {
                        position -= 2; //  ЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛЧЕЛ
                        IExpression borrow = FuncParsy();
                        result = new MethodExpression(result, attr, borrow);
                        continue;
                    }
                    result = new AttributeExpression(result, attr);
                    continue;
                }
                if (Match(TokenType.LCUBSCOB))
                {
                    IExpression first;
                    if (Current.Type == TokenType.COLON)
                        first = new NumExpression(Zero);
                    else
                        first = Expression();

                    if (Match(TokenType.COLON))
                    {
                        IExpression second;
                        if (Match(TokenType.RCUBSCOB))
                        {
                            result = new SliceExpression(result, first, new NumExpression(End));
                            continue;
                        }
                        else
                            second = Expression();

                        Consume(TokenType.RCUBSCOB);
                        result = new SliceExpression(result, first, second);
                        continue;
                    }

                    if (Match(TokenType.RCUBSCOB))
                    {
                        result = new SliceExpression(result, first);
                        continue;
                    }
                }
                break;
            }
            return result;
        }

        private IExpression Unary()
        {
            Token current = Current;
            int sign = -1;
            if (Match(TokenType.NOT))
            {
                while (true)
                {
                    current = Current;
                    if (Match(TokenType.NOT))
                    {
                        sign *= -1;
                        continue;
                    }
                    break;
                }
                return sign < 0 ? new UnaryExpression(current, Aftery()) : Aftery();
            }
            if (Match(TokenType.MINUS, TokenType.PLUS))
                return new UnaryExpression(current, Aftery());
            return Aftery();
        }

        private IExpression Powy()
        {
            IExpression result = Unary();
            while (true)
            {
                Token current = Current;
                if (Match(TokenType.POWER))
                {
                    result = new BinExpression(result, current, Unary());
                    continue;
                }
                break;
            }
            return result;
        }

        private IExpression Mody()
        {
            IExpression result = Powy();
            while (true)
            {
                Token current = Current;
                if (Match(TokenType.MOD) || Match(TokenType.DIV))
                {
                    result = new BinExpression(result, current, Powy());
                    continue;
                }
                break;
            }
            return result;
        }

        private IExpression Muly()
        {
            IExpression result = Mody();
            while (true)
            {
                Token current = Current;
                if (Match(TokenType.MULTIPLICATION) || Match(TokenType.DIVISION))
                {
                    result = new BinExpression(result, current, Mody());
                    continue;
                }
                if (current.Type == TokenType.INTEGER || current.Type == TokenType.DOUBLE)
                {
                    result = new BinExpression(result, Mul, Mody());
                    continue;
                }
                if (current.Type == TokenType.VARIABLE)
                {
                    result = new BinExpression(result, Mul, Mody());
                    continue;
                }
                if (Match(TokenType.LEFTSCOB))
                {
                    IExpression expression = Expression();
                    Match(TokenType.RIGHTSCOB);
                    result = new BinExpression(result, Mul, expression);
                    continue;
                }
                if (current.Type == TokenType.VARIABLE && Get(1).Type == TokenType.LEFTSCOB || current.Type == TokenType.FUNCTION)
                {
                    result = new BinExpression(result, Mul, FuncParsy());
                    continue;
                }
                break;
            }
            return result;
        }

        private IExpression Addity()
        {
            IExpression result = Muly();
            while (true)
            {
                Token current = Current;
                if (Match(TokenType.PLUS) || Match(TokenType.MINUS))
                {
                    result = new BinExpression(result, current, Muly());
                    continue;
                }
                break;
            }
            return result;
        }

        private IExpression Booly()
        {
            IExpression result = Addity();
            while (true)
            {
                Token current = Current;
                if (Match(TokenType.EQUALITY, TokenType.NOTEQUALITY))
                {
                    result = new CmpExpression(result, current, Addity());
                    continue;
                }
                if (Match(TokenType.MORE, TokenType.MOREEQ))
                {
                    result = new CmpExpression(result, current, Addity());
                    continue;
                }
                if (Match(TokenType.LESS, TokenType.LESSEQ))
                {
                    result = new CmpExpression(result, current, Addity());
                    continue;
                }
                break;
            }
            return result;
        }

        private IExpression Andy()
        {
            IExpression result = Booly();
            while (true)
            {
                Token current = Current;
                if (Match(TokenType.AND))
                {
                    result = new CmpExpression(result, current, Booly());
                    continue;
                }
                break;
            }
            return result;
        }

        private IExpression Ory()
        {
            IExpression result = Andy();
            while (true)
            {
                Token current = Current;
                if (Match(TokenType.OR))
                {
                    result = new CmpExpression(result, current, Andy());
                    continue;
                }
                break;
            }
            return result;
        }
    }
}
