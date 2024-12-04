using System;
using System.Collections.Generic;
using System.Linq;

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
            Token variable = Consume(TokenType.VARIABLE);
            Token operation = Consume(Current.Type);
            IExpression expression = Expression();
            Token op = new Token() { Location = Current.Location };
            switch (operation.Type)
            {
                case TokenType.PLUSEQ:
                    op.Type = TokenType.PLUS;
                    op.View = "+";
                    break;
                case TokenType.MINUSEQ:
                    op.Type = TokenType.MINUS;
                    op.View = "-";
                    break;
                case TokenType.MULEQ:
                    op.Type = TokenType.MULTIPLICATION;
                    op.View = "*";
                    break;
                case TokenType.DIVEQ:
                    op.Type = TokenType.DIVISION;
                    op.View = "/";
                    break;
                default:
                    break;
            }
            Sep();
            return new AssignStatement(variable, new BinExpression(new VariableExpression(variable), op, expression));
        }

        public static TokenType[] Equalities = new TokenType[] { TokenType.DO_EQUAL, TokenType.PLUSEQ, TokenType.MINUSEQ, TokenType.MULEQ, TokenType.DIVEQ };

        public FullNodeToAssign FullNodeToAssignParse()
        {
            Token objName = Consume(TokenType.VARIABLE);
            List<object> parts = new List<object>();

            Token current = Current;
            if (current.Type == TokenType.DOT || current.Type == TokenType.LCUBSCOB)
            {
                while (true)
                {
                    if (Match(TokenType.DOT))
                    {
                        parts.Add(Consume(TokenType.VARIABLE));
                        continue;
                    }
                    if (Match(TokenType.LCUBSCOB))
                    {
                        IExpression[] slice = new IExpression[3] { null, null, null };

                        if (Current.Type != TokenType.COLON && Current.Type != TokenType.RCUBSCOB)
                            slice[0] = Expression();
                        
                        if (Match(TokenType.RCUBSCOB))
                        {
                            slice[1] = new NumExpression("");
                            parts.Add(slice);
                            continue;
                        }

                        if (Match(TokenType.COLON))
                        {
                            if (Current.Type != TokenType.COLON && Current.Type != TokenType.RCUBSCOB)
                                slice[1] = Expression();
                            
                            if (Match(TokenType.RCUBSCOB))
                            {
                                parts.Add(slice);
                                continue;
                            }

                            if (Match(TokenType.COLON))
                            {
                                if (Current.Type != TokenType.RCUBSCOB)
                                    slice[2] = Expression();

                                Consume(TokenType.RCUBSCOB);
                                parts.Add(slice);
                                continue;
                            }
                        }

                        parts.Add(slice);
                        continue;
                    }
                    break;
                }
            }

            return new FullNodeToAssign() { ObjName = objName, Parts = parts.ToArray()};
        }

        public IStatement WhereFullAssign()
        {
            FullNodeToAssign node = FullNodeToAssignParse();

            bool exactly = Match(TokenType.EXACTLY);
            bool fill = Match(TokenType.FILL);

            Token operation;
            if (Equalities.Contains(Current.Type))
                operation = Consume(Current.Type);
            else
                throw new Exception($"ОЖИДАЛСЯ ОДИН ИЗ <{string.Join(", ", Equalities.Select(e => e.GetStringValue()))}> НО БЫЛ НАЙДЕН <{Current}>");
            
            IExpression assigned = Expression();
            return new WhereFullAssignStatement(node, operation, assigned, exactly, fill);
        }

        // ВЕРОЯЯТНЕЕ ВСЕГО БУДЕТ УДАЛЕНО ПОЛНОСТЬЮ
        // ВЕРОЯЯТНЕЕ ВСЕГО БУДЕТ УДАЛЕНО ПОЛНОСТЬЮ
        // ВЕРОЯЯТНЕЕ ВСЕГО БУДЕТ УДАЛЕНО ПОЛНОСТЬЮ
        public VarAttrSliceNode ParseFullObjNode()
        {
            Token objName = Consume(TokenType.VARIABLE);
            List<Token> attrs = null;
            if (Match(TokenType.DOT))
            {
                attrs = new List<Token>() { Consume(TokenType.VARIABLE) };
                while (Match(TokenType.DOT))
                    attrs.Add(Consume(TokenType.VARIABLE));
            }
            List<IExpression[]> slices = null;
            if (Current.Type == TokenType.LCUBSCOB)
            {
                slices = new List<IExpression[]>();
                int i = -1;
                while (true)
                {
                    i++;
                    slices.Add(new IExpression[3]);
                    if (Match(TokenType.LCUBSCOB))
                    {
                        IExpression first;
                        if (Current.Type == TokenType.COLON)
                            first = null;
                        else
                            first = Expression();

                        if (Match(TokenType.COLON))
                        {
                            IExpression second;
                            if (Match(TokenType.RCUBSCOB))
                            {
                                slices[i] = new IExpression[3] { first, null, null };
                                continue;
                            }
                            else
                                if (Current.Type == TokenType.COLON)
                                second = null;
                            else
                                second = Expression();

                            if (Match(TokenType.COLON))
                            {
                                IExpression third;
                                if (Match(TokenType.RCUBSCOB))
                                {
                                    slices[i] = new IExpression[3] { first, second, null };
                                    continue;
                                }
                                else
                                    third = Expression();

                                Consume(TokenType.RCUBSCOB);
                                slices[i] = new IExpression[3] { first, second, third };
                                continue;
                            }

                            Consume(TokenType.RCUBSCOB);
                            slices[i] = new IExpression[3] { first, second, null };
                            continue;
                        }

                        Consume(TokenType.RCUBSCOB);
                        slices[i] = new IExpression[3] { first, new NumExpression(""), null };
                        continue;
                    }
                    break;
                }
            }
            return new VarAttrSliceNode() { ObjName = objName, Attrs = attrs is null ? null : attrs.ToArray(), Slices = slices is null ? null : slices.ToArray() };
        }

        private IStatement ItemAssigny()
        {
            VarAttrSliceNode parsed = ParseFullObjNode();

            bool exactly = Match(TokenType.EXACTLY);
            bool fill = Match(TokenType.FILL);
            Consume(TokenType.DO_EQUAL, TokenType.ARROW);
            return new SliceAssignStatement(parsed.ObjName, parsed.Slices, parsed.Attrs, Expression(), exactly, fill, Sep());
        }

        private IStatement AttMethody()
        {
            Token objName = Consume(TokenType.VARIABLE);
            Consume(TokenType.DOT);
            List<Token> attrs = new List<Token>();
            while (Current.Type != TokenType.ARROW && Current.Type != TokenType.DO_EQUAL)
            {
                attrs.Add(Consume(TokenType.VARIABLE));
                if (!Match(TokenType.DOT))
                    if (Current.Type == TokenType.ARROW || Current.Type == TokenType.DO_EQUAL)
                        break;
                    else
                        throw new Exception($"{Near(6)}ОШИБКА СИНТАКИСА РЯДОМ С: {Current}");
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
            throw new Exception($"{Near(6)}ДАННОЕ СЛОВО НЕ МОЖЕТ БЫТЬ ИСПОЛЬЗОВАННО В КАЧЕСТВЕ НАЗВАНИЯ ДЛЯ МЕТОДА ИЛИ АТТРИБУТА: {objName}.<{string.Join(".", attrs)}>");
        }
        // ВЕРОЯЯТНЕЕ ВСЕГО БУДЕТ УДАЛЕНО ПОЛНОСТЬЮ
        // ВЕРОЯЯТНЕЕ ВСЕГО БУДЕТ УДАЛЕНО ПОЛНОСТЬЮ
        // ВЕРОЯЯТНЕЕ ВСЕГО БУДЕТ УДАЛЕНО ПОЛНОСТЬЮ

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

            List<IExpression> elifsExps = new List<IExpression>();
            List<IStatement> elifsBlocks = new List<IStatement>();
            while (Match(TokenType.WORD_ELIF))
            {
                elifsExps.Add(Expression());
                elifsBlocks.Add(OneOrBlock());
            }
            if (Match(TokenType.WORD_ELSE))
                elseStatements = OneOrBlock();
            else
                elseStatements = null;
            return new IfStatement(condition, ifStatements, elifsExps.ToArray(), elifsBlocks.ToArray(), elseStatements);
        }

        private IStatement Whily()
        {
            IExpression condition = Expression();
            IStatement statement = OneOrBlock();
            return new WhileStatement(condition, statement);
        }

        public static Token IMPOSSIBLE = new Token() { View = "НЕ ВОЗ МОЖ НО" };

        private IStatement Fory()
        {
            if (Current.Type == TokenType.VARIABLE && Get(1).Type == TokenType.WHICH || Get(1).Type == TokenType.IN)
            {
                Token itterator = Consume(TokenType.VARIABLE);
                VarAttrSliceNode which = new VarAttrSliceNode() { ObjName = IMPOSSIBLE };
                if (Match(TokenType.WHICH))
                    which = ParseFullObjNode();
                Consume(TokenType.IN);
                IExpression ratio = Expression();
                IStatement notBody = OneOrBlock();
                return new ForInStatement(itterator, which, ratio, notBody);
            }

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
            List<Token> inherit = new List<Token>();
            if (Match(TokenType.SON))
                while (Current.Type == TokenType.VARIABLE)
                {
                    inherit.Add(Consume(TokenType.VARIABLE));
                    Sep();
                }
            if (Current.Type == TokenType.LTRISCOB || Current.Type == TokenType.LEFTSCOB)
            {
                IStatement body = OneOrBlock();
                return new DeclareClassStatement(className, inherit.ToArray(), body);
            }
            throw new Exception($"{Near(6)}ОБЬЯВЛЕНИЕ ТЕЛА КЛАССА МОЖЕТ БЫТЬ ТОЛЬКО В СКОБКАХ");
        }

        public IStatement Importy()
        {
            int dots = 0;
            while (Current.Type == TokenType.DOT || Current.Type == TokenType.DOTDOT)
                if (Match(TokenType.DOT))
                    dots++;
                else if (Match(TokenType.DOTDOT))
                    dots += 2;

            List<Token> path = new List<Token>() { Consume(TokenType.VARIABLE) };
            while (Match(TokenType.DOT))
                path.Add(Consume(TokenType.VARIABLE));

            Sep();
            return new ImportStatement(dots, path.ToArray());
        }

        public IStatement Throwy()
        {
            IExpression message = Expression();
            Sep();
            return new ThrowStatement(message);
        }

        public IStatement TryCatchy()
        {
            IStatement tryBlock = OneOrBlock();
            Consume(TokenType.CATCH);
            Token to = IMPOSSIBLE;
            if (Current.Type == TokenType.VARIABLE)
                to = Consume(TokenType.VARIABLE);
            IStatement catchBlock = OneOrBlock();
            return new TryCatchStatement(tryBlock, to, catchBlock);
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
