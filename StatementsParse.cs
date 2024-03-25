using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace VovaScript
{
    public partial class Parser
    {
        private IStatement OneOrBlock()
        {
            if (Match(TokenType.LTRISCOB, TokenType.LEFTSCOB))
                return Block();
            else
            {
                Match(TokenType.COLON);
                return Statement();
            }
        }

        private IStatement Assigny()
        {
            Match(TokenType.THIS);
            Token current = Consume(TokenType.VARIABLE);
            Consume(TokenType.DO_EQUAL);
            IExpression expression = Expression();
            IStatement result = new AssignStatement(current, expression);
            Sep();
            return result;
        }

        private IStatement OpAssigny()
        {
            Token variable = Current;
            Consume(TokenType.VARIABLE);
            Token operation = Current;
            Consume(operation.Type);
            IExpression expression = Expression();
            Sep();
            return new OperationAssignStatement(variable, operation, expression);
        }

        private IStatement ItemAssigny()
        {
            Token variable = Current;
            Consume(TokenType.VARIABLE);

            Consume(TokenType.LCUBSCOB, TokenType.LEFTSCOB);
            IExpression index = Expression();
            Consume(TokenType.RCUBSCOB);

            if (Match(TokenType.DO_EQUAL))
            {
                IExpression value = Expression();
                Sep();
                return new ItemAssignStatement(new NumExpression(variable), index, value);
            }
            return new PrintStatement(new ListTakeExpression(variable, index, null));
        }

        private IStatement AttMethody()
        {
            Token objName = Consume(TokenType.VARIABLE);
            Consume(TokenType.DOT);
            List<Token> attrs = new List<Token>();
            //Token thingName = Consume(TokenType.VARIABLE);
            while (Current.Type != TokenType.ARROW && Current.Type != TokenType.DO_EQUAL)
            {
                attrs.Add(Consume(TokenType.VARIABLE));
                if (!Match(TokenType.DOT))
                    if (Current.Type == TokenType.ARROW || Current.Type == TokenType.DO_EQUAL)
                        break;
                    else
                        throw new Exception($"ОШИБКА СИНТАКИСА РЯДОМ С: {Near(6)}");
            }

            if (Match(TokenType.DO_EQUAL))
            {
                IExpression value = Expression();
                Sep();
                return new AttributeAssignStatement(objName, attrs.ToArray(), value);
            }
            if (Match(TokenType.ARROW))
            {
                List<Token> args = new List<Token>();
                Consume(TokenType.LEFTSCOB);
                while (!Match(TokenType.RIGHTSCOB))
                {
                    args.Add(Consume(TokenType.VARIABLE));
                    Sep();
                }
                IStatement body = OneOrBlock();
                return new MethodAssignStatement(objName, attrs.ToArray(), args.ToArray(), body);
            }
            throw new Exception($"ДАННОЕ СЛОВО НЕ МОЖЕТ БЫТЬ ИСПОЛЬЗОВАННО В КАЧЕСТВЕ НАЗВАНИЯ ДЛЯ МЕТОДА ИЛИ АТТРИБУТА: {objName}.<{string.Join(".", attrs)}>");
        }

        private IStatement Printy()
        {
            IStatement print = new PrintStatement(Expression());
            Sep();
            return print;
        }

        private IStatement IfElsy()
        {
            IExpression condition = Expression();
            IStatement ifStatements = OneOrBlock();
            IStatement elseStatements;
            if (Match(TokenType.WORD_ELSE))
                elseStatements = OneOrBlock();
            else
                elseStatements = null;
            return new IfStatement(condition, ifStatements, elseStatements);
        }

        private IStatement Whily()
        {
            IExpression condition = Expression();
            IStatement statement = OneOrBlock();
            return new WhileStatement(condition, statement);
        }

        private IStatement Fory()
        {
            IStatement definition = Assigny();

            IExpression condition = Expression();
            Sep();

            Token current = Current;
            IStatement alter = null;
            if (Match(TokenType.VARIABLE))
            {
                Consume(TokenType.DO_EQUAL);
                alter = new AssignStatement(current, Expression());
            }
            else if (Current.Type == TokenType.PLUSPLUS || Current.Type == TokenType.MINUSMINUS)
            {
                Token operation = Current;
                Consume(TokenType.PLUSPLUS, TokenType.MINUSMINUS);
                Token name = Current;
                Consume(TokenType.VARIABLE);
                alter = new IncDecBeforeExpression(operation, name);
            }

            IStatement body = OneOrBlock();
            return new ForStatement(definition, condition, alter, body);
        }

        public IStatement BeforeIncDecy()
        {
            Token current = Current;
            Consume(TokenType.PLUSPLUS, TokenType.MINUSMINUS);
            Token name = Current;
            Consume(TokenType.VARIABLE);
            IStatement statement = new IncDecBeforeExpression(current, name);
            Sep();
            return statement;
        }

        public IStatement Breaky()
        {
            IStatement statement = new BreakStatement();
            Sep();
            return statement;
        }

        public IStatement Continuy()
        {
            IStatement statement = new ContinueStatement();
            Sep();
            return statement;
        }

        public IStatement Returny() 
        {
            IExpression expression = Expression();
            Sep();
            return new ReturnStatement(expression);
        }

        public IStatement Functiony()
        {
            Token name = Current;
            Consume(TokenType.VARIABLE);
            List<Token> args = new List<Token>();
            Consume(TokenType.ARROW);
            Consume(TokenType.LEFTSCOB);
            while (!Match(TokenType.RIGHTSCOB))
            {
                args.Add(Consume(TokenType.VARIABLE));
                Sep();
            }
            IStatement body = OneOrBlock();
            return new DeclareFunctionStatement(name, args.ToArray(), body);
        }

        public IStatement Procedury()
        {
            IExpression expression = Expression();
            Sep();
            return new ProcedureStatement(expression);
        }

        public IStatement Cleary()
        {
            Sep();
            return new ClearStatement();
        }

        public IStatement Sleepy()
        {
            Consume(TokenType.LEFTSCOB);
            IExpression ms = Expression();
            Consume(TokenType.RIGHTSCOB);
            Sep();
            return new SleepStatement(ms);
        }

        public IStatement Pycy()
        {
            IExpression program = Expression();
            Sep();
            return new ProgramStatement(program);
        }

        public IStatement Classy()
        {
            Token className = Consume(TokenType.VARIABLE);
            if (Current.Type == TokenType.LTRISCOB || Current.Type == TokenType.LEFTSCOB)
            {
                IStatement body = OneOrBlock();
                return new DeclareClassStatement(className, body);
            }
            throw new Exception("ОБЬЯВЛЕНИЕ ТЕЛА КЛАССА МОЖЕТ БЫТЬ ТОЛЬКО В СКОБКАХ");
        }

        public IStatement SQLCreateDatabasy()
        {
            Consume(TokenType.CREATE);
            Consume(TokenType.DATABASE);
            IExpression database = Expression();
            Sep();
            return new SQLCreateDatabaseStatement(database);
        }

        public IStatement SQLCreateTably()
        {
            Consume(TokenType.CREATE);
            Consume(TokenType.TABLE);
            IExpression tableName = Expression();

            Match(TokenType.LTRISCOB, TokenType.LEFTSCOB);
            List<Token> types = new List<Token>();
            List<Token> names = new List<Token>();
            while (!Match(TokenType.RTRISCOB, TokenType.LEFTSCOB))
            {
                Token current = Current;
                if (Match(TokenType.COMMA))
                    continue;
                if (current.Type == TokenType.STROKE || current.Type == TokenType.NUMBER || current.Type == TokenType.FNUMBER || current.Type == TokenType.BUL)
                {
                    Match(current.Type);
                    types.Add(current);
                    continue;
                }
                if (Match(TokenType.VARIABLE, TokenType.STRING))
                {
                    names.Add(current);
                    continue;
                }
            }
            return new SQLCreateTableStatement(tableName, types.ToArray(), names.ToArray());
        }

        public IStatement SQLInserty()
        {
            Consume(TokenType.IN);
            IExpression table = Expression();

            List<IExpression> colons = new List<IExpression>();
            if (Match(TokenType.COLONS))
            {
                Consume(TokenType.LEFTSCOB);
                while (!Match(TokenType.RIGHTSCOB, TokenType.EOF))
                {
                    colons.Add(Expression());
                    Sep();
                }
            }

            List<IExpression> values = new List<IExpression>();
            if (Match(TokenType.VALUES))
            {
                Consume(TokenType.LEFTSCOB);
                while (!Match(TokenType.RIGHTSCOB, TokenType.EOF))
                {
                    values.Add(Expression());
                    Sep();
                }
            }

            Sep();
            return new SQLInsertStatement(table, colons.ToArray(), values.ToArray());
        }
    }
}
