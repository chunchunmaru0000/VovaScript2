using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace VovaScript
{
    public partial class Parser
    {
        private IExpression FuncParsy()
        {
            Token name = Consume(TokenType.FUNCTION, TokenType.VARIABLE);
            Consume(TokenType.LEFTSCOB);
            FunctionExpression function = new FunctionExpression(name);
            while (!Match(TokenType.RIGHTSCOB))
            {
                function.AddArg(Expression());
                Sep();
            }
            return function;
        }

        private IExpression Methody()
        {
            Token objectName = Consume(TokenType.VARIABLE);
            Consume(TokenType.DOT);
            Token methodName = Current;
            IExpression borrow = FuncParsy();
            return new MethodExpression(objectName, methodName, borrow);
        }

        private IExpression Attributy()
        {
            Token objectName = Consume(TokenType.VARIABLE);
            Consume(TokenType.DOT);
            Token attributeName = Consume(TokenType.VARIABLE);
            return new AttributeExpression(objectName, attributeName);
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

        private IExpression Slicy()
        {
            Token sliced = Current;
            Consume(TokenType.VARIABLE);
            Consume(TokenType.LCUBSCOB, TokenType.LEFTSCOB);
            IExpression from = Expression();
            if (Match(TokenType.COLON))
            {
                IExpression to = Expression();
                Consume(TokenType.RCUBSCOB);
                return new ListTakeExpression(sliced, from, to);
            }
        /*    if (Match(TokenType.COMMA, TokenType.SEMICOLON))
            {
                List<IExpression> indices = new List<IExpression>();
                Consume(TokenType.COMMA, TokenType.SEMICOLON);
                while(Current.Type != TokenType.RCUBSCOB)
                {
                    indices.Add(Expression());
                }

            }*/
            Consume(TokenType.RCUBSCOB);
            return new ListTakeExpression(sliced, from, null);
        }

        private IExpression Listy()
        {
            Consume(TokenType.LCUBSCOB, TokenType.LEFTSCOB);
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
            throw new NotImplementedException($"НЕВЕРНЫЙ СИНТАКСИС ГДЕ-ТО РЯДОМ С: {result}");
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

        private IExpression Primary()
        {
            Token current = Current;
            Token next = Get(1);

            if (Match(TokenType.NEW))
                return Newy();

            if (current.Type == TokenType.STRING)
            {
                if (next.Type == TokenType.LCUBSCOB)
                   return Slicy();
            }

            if (current.Type == TokenType.VARIABLE)
            {
                if (next.Type == TokenType.LCUBSCOB)
                    return Slicy();

                if (next.Type == TokenType.LEFTSCOB )
                    return FuncParsy();

                /*
                if (next.Type == TokenType.DOT)
                    if (Get(2).Type == TokenType.VARIABLE)
                        if (Get(3).Type == TokenType.LEFTSCOB)
                            return Methody();
                        else
                            return Attributy();
                    else
                        throw new Exception($"НЕДОПУСТИМОЕ СЛОВО ДЛЯ МЕТОДА ИЛИ АТРИБУТА: <{Get(2)}>");
                */
            }

            if (Match(TokenType.SELECT))
                return SQLy();

            if (Match(TokenType.ALL))
                return All;

            if (current.Type == TokenType.FUNCTION)
                return FuncParsy();

            if (current.Type == TokenType.LCUBSCOB)
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

            if (Match(TokenType.PLUSPLUS, TokenType.MINUSMINUS))
            {
                Token name = Current;
                IExpression result = new IncDecBeforeExpression(current, name);
                Consume(TokenType.VARIABLE);
                return result;
            }
            return (IExpression)Statement();
            throw new Exception($"НЕВОЗМОЖНОЕ МАТЕМАТИЧЕСКОЕ ВЫРАЖЕНИЕ: <{current}>\nПОЗИЦИЯ: ЛИНИЯ<{line}> СИМВОЛ<{position}>");
        }

        private IExpression Doty()
        {
            IExpression result = Primary();
            while (true)
            {
                if (Match(TokenType.DOT))
                {
                    Token attr = Consume(Current.Type);
                    if (Match(TokenType.LEFTSCOB))
                    {
                        throw new Exception("ТЫ ДАУН ЕЩЕ НЕ СДЕЛАЛ ПАРСИНГ МЕТОДОВ");
                        //method = чететототам
                        continue;
                    }
                    result = new AttributeExpression(result, attr);
                    continue;
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
                return sign < 0 ? new UnaryExpression(current, Doty()) : Doty();
            }
            if (Match(TokenType.MINUS, TokenType.PLUS))
                return new UnaryExpression(current, Doty());
            return Doty();
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
