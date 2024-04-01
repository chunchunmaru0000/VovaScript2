using System;
using System.Linq;

namespace VovaScript
{
    public partial class Parser
    {
        public Token[] tokens;
        public int lenght;
        public int position;
        public int line = 0;
        public static Token Mul = new Token() { View = "*", Value = null, Type = TokenType.MULTIPLICATION };
        public static Token AllToken = new Token () { View = "всё", Value = "всё", Type = TokenType.ALL };
        public static NumExpression All = new NumExpression(AllToken);
        public static IStatement Nothing = new NothingStatement();
        public static IExpression Nothingness = new NothingExpression();

        public Parser(Token[] tokens) 
        {
            this.tokens = tokens;
            lenght = tokens.Length;
            position = 0;
        }

        private Token Get(int offset)
        {
            if (position + offset < lenght && position + offset > -1)
                return tokens[position + offset];
            return tokens.Last();
        }

        private Token Current => Get(0);

        private string Near(int range)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(string.Join("|", Enumerable.Range(-range, range).Select(g => Get(g).View)));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write('|' + Current.View + '|');
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(string.Join("|", Enumerable.Range(1, range).Select(g => Get(g).View)));
            Console.ResetColor();
            return "";
        }

        private Token Consume(TokenType type)
        {
            Token current = Current;
            if (Current.Type != type)
                throw new Exception($"{Near(6)}" + 
                                    $"СЛОВО НЕ СОВПАДАЕТ С ОЖИДАЕМЫМ\n" +
                                    $"ОЖИДАЛСЯ: <{type.GetStringValue()}>\n" +
                                    $"ТЕКУЩИЙ: <{Current.Type.GetStringValue()}>\n" +
                                    $"ПОЗИЦИЯ: КОМАНДА<{line}> СЛОВО<{position}>");
            position++;
            return current;
        }

        private Token Consume(TokenType type0, TokenType type1)
        {
            Token current = Current;
            if (Current.Type != type0 && Current.Type != type1)
                throw new Exception($"{Near(6)}" + 
                                    $"СЛОВО НЕ СОВПАДАЕТ С ОЖИДАЕМЫМ\n" +
                                    $"ОЖИДАЛСЯ: <{type0.GetStringValue()}> ИЛИ <{type1.GetStringValue()}>\n" +
                                    $"ТЕКУЩИЙ: <{Current.Type.GetStringValue()}>\n" +
                                    $"ПОЗИЦИЯ: КОМАНДА<{line}> СЛОВО<{position}>");
            position++;
            return current;
        }

        private bool Printble(TokenType type)
        {
            return type == TokenType.INTEGER    ||
                   type == TokenType.DOUBLE     ||
                   type == TokenType.VARIABLE   ||
                   type == TokenType.WORD_TRUE  ||
                   type == TokenType.WORD_FALSE ||
                   type == TokenType.PLUS       ||
                   type == TokenType.MINUS      ||
                   type == TokenType.NOT        ||
                   type == TokenType.STRING     ||
                   type == TokenType.LEFTSCOB   ||
                   type == TokenType.FUNCTION   ||
                   type == TokenType.NOW        ||
                   type == TokenType.LCUBSCOB   ||
                   type == TokenType.SELECT     ||
                   type == TokenType.NEW        || 
                   type == TokenType.AT         ||
                   type == TokenType.STICK      ;
        }

        private bool Match(TokenType type)
        {
            if (Current.Type != type)
                return false;
            position++;
            return true;
        }

        private bool Match(TokenType type0, TokenType type1)
        {
            if (Current.Type != type0 && Current.Type != type1)
                return false;
            position++;
            return true;
        }

        private bool Match(TokenType type0, TokenType type1, TokenType type2)
        {
            if (Current.Type != type0 && Current.Type != type1 && Current.Type != type2)
                return false;
            position++;
            return true;
        }

        private bool Sep () => Match(TokenType.SEMICOLON, TokenType.COMMA);

        private IExpression Expression()
        {
            return Ory();
        }

        private IStatement Statement()
        {
            line++;
            Token current = Current;
            Token next = Get(1);

            if (current.Type == TokenType.CREATE)
            {
                if (next.Type == TokenType.DATABASE)
                    return SQLCreateDatabasy();

                if (next.Type == TokenType.TABLE)
                    return SQLCreateTably();
            }

            if (Match(TokenType.INSERT))
                return SQLInserty();

            if (Match(TokenType.THIS))
                return Assigny();

            if (current.Type == TokenType.VARIABLE)
            {
                if (next.Type == TokenType.DO_EQUAL)
                    return Assigny();

                if (next.Type == TokenType.PLUSEQ || next.Type == TokenType.MINUSEQ || next.Type == TokenType.MULEQ || next.Type == TokenType.DIVEQ)
                    return OpAssigny();

                if (next.Type == TokenType.ARROW)
                    return Functiony();
            }

            if (Match(TokenType.IN))
                return AttMethody();

            if (Match(TokenType.WHERE))
                return ItemAssigny();

            if (current.Type == TokenType.PLUSPLUS || current.Type == TokenType.MINUSMINUS && next.Type == TokenType.VARIABLE)
                return BeforeIncDecy();

            if (Match(TokenType.VOVASCRIPT))
                return Pycy();

            if (Match(TokenType.IMPORT))
                return Importy();

            if (Match(TokenType.CLASS))
                return Classy();

            if (Match(TokenType.PROCEDURE))
                return Procedury();

            if (Match(TokenType.WORD_IF))
                return IfElsy();

            if (Match(TokenType.WORD_WHILE))
                return Whily();

            if (Match(TokenType.BREAK))
                return Breaky();

            if (Match(TokenType.CONTINUE))
                return Continuy();

            if (Match(TokenType.RETURN))
                return Returny();

            if (Match(TokenType.WORD_FOR))
                return Fory();

            if (Match(TokenType.LOOP))
                return new LoopStatement(OneOrBlock());

            if (Match(TokenType.CLEAR))
                return Cleary();

            if (Match(TokenType.SLEEP))
                return Sleepy();

            if (Match(TokenType.THROW))
                return Throwy();
            
            if (Match(TokenType.TRY))
                return TryCatchy();

            if (Match(TokenType.WORD_PRINT))
                return Printy();

            if (Printble(current.Type))
                return Printy();

            if (current.Type == TokenType.COLON || current.Type == TokenType.LTRISCOB || current.Type == TokenType.LEFTSCOB)
                return OneOrBlock();

            if (Sep())
                return Nothing;

            try { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"{Get(-1)}; {current}; {next};"); Console.ResetColor(); } catch (Exception) { }
            throw new Exception($"{Near(6)}НЕИЗВЕСТНОЕ ДЕЙСТВИЕ: {current}\nПОЗИЦИЯ: ДЕЙСТВИЕ<{line}> СЛОВО<{position}>");
        }

        private IStatement Block()
        {
            BlockStatement block = new BlockStatement();
            while (!Match(TokenType.RTRISCOB, TokenType.RIGHTSCOB))
                block.AddStatement(Statement());
            return block;
        }

        public IStatement Parse()
        {
            BlockStatement parsed = new BlockStatement();
            while (!Match(TokenType.EOF))
                parsed.Statements.Add(Statement());
            return parsed;
        }

        public void Run(bool debug = false, bool printVariables = false)
        {
            while (!Match(TokenType.EOF))
            {
                IStatement statement = Statement();
                if (debug)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(statement.ToString());
                }
                VovaScript2.PrintVariables(printVariables);

                Console.ForegroundColor = ConsoleColor.Yellow;
                if (!(statement is BlockStatement))
                    statement.Execute();
            }
        }
    }
}
