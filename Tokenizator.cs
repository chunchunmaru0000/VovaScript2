using System;
using System.Collections.Generic;

namespace VovaScript
{
    public class Tokenizator
    {
        private string code;
        private int position;
        private int line;
        private int location;
        private int startLine;
        private bool commented = false;
        private static Token Nothing = new Token() { View = "", Value = null, Type = TokenType.WHITESPACE, Location = new Location(-1, -1) };

        public Tokenizator(string code) 
        {
            this.code = code;
            position = 0;
            line = 1;
            location = position;
            startLine = 0;
        }

        private char Current
        {
            get
            {
                if (position < code.Length)
                    return code[position];
                return '\0';
            }
        }

        private void Next() 
        { 
            position++;
            if (Current == '\n')
            {
                Next();
                startLine = position;
                line++;
                location = 0;
            }
            location = position - startLine;
        }

        private Location Loc() => new Location(line, location);

        private Token NextToken()
        {
            if (Current == '\0')
                return new Token() { View = null, Value = null, Type = TokenType.EOF, Location = Loc() };
            if (char.IsWhiteSpace(Current))
                while (char.IsWhiteSpace(Current))
                    Next();
            if (Current == '"' || Current == '\'')
            {
                if (!commented) {
                    Next();
                    string buffer = "";
                    while (Current != '"' || Current != '\'')
                    {
                        while (true) { 
                            if (Current == '\\')
                            {
                                Next();
                                switch (Current)
                                {
                                    case 'н':
                                        Next();
                                        buffer += '\n';
                                        break;
                                    case 'т':
                                        Next();
                                        buffer += '\t';
                                        break;
                                    case '\\':
                                        Next();
                                        buffer += '\\';
                                        break;
                                    default:
                                        Next();
                                        break;
                                }
                                continue;
                            }
                            break; 
                        }
                        if (Current == '"' || Current == '\'')
                            break;

                        buffer += Current;
                        Next();

                        if (Current == '\0')
                            throw new Exception($"НЕЗАКОНЧЕНА СТРОКА: позиция<{position}> буфер<{buffer}>");
                    }
                    Next();
                    return new Token() { View = buffer, Value = buffer, Type = TokenType.STRING, Location = Loc() };
                }
                else
                {
                    Next();
                    return Nothing;
                }
            }
            if (char.IsDigit(Current))
            {
                int start = position;
                int dots = 0;
                while (char.IsDigit(Current) || Current == '.')
                {
                    if (Current == '.')
                        dots++;
                    if (dots > 1)
                    {
                        dots--;
                        break;
                    }
                    Next();
                }
                string word = code.Substring(start, position - start).Replace('.', ',');
                if (dots == 0)
                {
                    long res;
                    if (long.TryParse(word, out res))
                        return new Token() { View = word, Value = res, Type = TokenType.INTEGER, Location = Loc() };
                    throw new Exception($"ЧИСЛО <{word}> БЫЛО СЛИШКОМ ВЕЛИКО ИЛИ МАЛО ДЛЯ ПОДДЕРЖИВАЕМЫХ СЕЙЧАС ЧИСЕЛ");
                }
                if (dots == 1)
                {
                    double res;
                    if (double.TryParse(word, out res))
                        return new Token() { View = word, Value = res, Type = TokenType.DOUBLE, Location = Loc() };
                    throw new Exception($"ЧИСЛО <{word}> БЫЛО СЛИШКОМ ВЕЛИКО ИЛИ МАЛО ДЛЯ ПОДДЕРЖИВАЕМЫХ СЕЙЧАС ЧИСЕЛ");
                }
                throw new Exception("МНОГА ТОЧЕК ДЛЯ ЧИСЛА");
            }
            if (PycTools.Usable(Current))
            {
                int start = position;
                while (PycTools.Usable(Current))
                    Next();
                string word = code.Substring(start, position - start);
                return Worder.Wordizator(new Token() { View = word, Value = null, Type = TokenType.WORD, Location = Loc() });
            }
            switch (Current)
            {
                case '=':
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        if (Current == '=')
                        {
                            Next();
                            return new Token() { View = "===", Value = null, Type = TokenType.ARROW, Location = Loc() };
                        }
                        return new Token() { View = "==", Value = null, Type = TokenType.EQUALITY, Location = Loc() };
                    }
                    if (Current == '>')
                    {
                        Next();
                        return new Token() { View = "=>", Value = null, Type = TokenType.ARROW, Location = Loc() };
                    }
                    return new Token() { View = "=", Value = null, Type = TokenType.DO_EQUAL, Location = Loc() };
                case '/':
                    Next();
                    if (Current == '/')
                    {
                        Next();
                        return new Token() { View = "//", Value = null, Type = TokenType.DIV, Location = Loc() };
                    }
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = "/=", Value = null, Type = TokenType.DIVEQ, Location = Loc() };
                    }
                    return new Token() { View = "/", Value = null, Type = TokenType.DIVISION, Location = Loc() };
                case '!':
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = "!=", Value = null, Type = TokenType.NOTEQUALITY, Location = Loc() };
                    }
                    return new Token() { View = "!", Value = null, Type = TokenType.NOT, Location = Loc() };
                case '*':
                    Next();
                    if (Current == '*')
                    {
                        Next();
                        return new Token() { View = "**", Value = null, Type = TokenType.POWER, Location = Loc() };
                    }
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = "*=", Value = null, Type = TokenType.MULEQ, Location = Loc() };
                    }
                    return new Token() { View = "*", Value = null, Type = TokenType.MULTIPLICATION, Location = Loc() };
                case '+':
                    Next();
                    if (Current == '+')
                    {
                        Next();
                        return new Token() { View = "++", Value = null, Type = TokenType.PLUSPLUS, Location = Loc() };
                    }
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = "+=", Value = null, Type = TokenType.PLUSEQ, Location = Loc() };
                    }
                    return new Token() { View = "+", Value = null, Type = TokenType.PLUS, Location = Loc() };
                case '-':
                    Next();
                    if (Current == '-')
                    {
                        Next();
                        return new Token() { View = "--", Value = null, Type = TokenType.MINUSMINUS, Location = Loc() };
                    }
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = "-=", Value = null, Type = TokenType.MINUSEQ, Location = Loc() };
                    }
                    return new Token() { View = "-", Value = null, Type = TokenType.MINUS, Location = Loc() };
                case '<':
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = "<=", Value = null, Type = TokenType.LESSEQ, Location = Loc() };
                    }
                    return new Token() { View = "<", Value = null, Type = TokenType.LESS, Location = Loc() };
                case '>':
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = ">=", Value = null, Type = TokenType.MOREEQ, Location = Loc() };
                    }
                    return new Token() { View = ">", Value = null, Type = TokenType.MORE, Location = Loc() };
                case '@':
                    Next();
                    return new Token() { View = "@", Value = null, Type = TokenType.DOG, Location = Loc() };
                case ';':
                    Next();
                    return new Token() { View = ";", Value = null, Type = TokenType.SEMICOLON, Location = Loc() };
                case '(':
                    Next();
                    return new Token() { View = "(", Value = null, Type = TokenType.LEFTSCOB, Location = Loc() };
                case ')':
                    Next();
                    return new Token() { View = ")", Value = null, Type = TokenType.RIGHTSCOB, Location = Loc() };
                case '[':
                    Next();
                    return new Token() { View = "[", Value = null, Type = TokenType.LCUBSCOB, Location = Loc() };
                case ']':
                    Next();
                    return new Token() { View = "]", Value = null, Type = TokenType.RCUBSCOB, Location = Loc() };
                case '{':
                    Next();
                    return new Token() { View = "{", Value = null, Type = TokenType.LTRISCOB, Location = Loc() };
                case '}':
                    Next();
                    return new Token() { View = "}", Value = null, Type = TokenType.RTRISCOB, Location = Loc() };
                case '%':
                    Next();
                    return new Token() { View = "%", Value = null, Type = TokenType.MOD, Location = Loc() };
                case '.':
                    Next();
                    if (Current == '.')
                    {
                        Next();
                        if (Current == '=')
                        {
                            Next();
                            return new Token() { View = "..=", Value = null, Type = TokenType.DOTDOTEQ, Location = Loc() };
                        }
                        return new Token() { View = "..", Value = null, Type = TokenType.DOTDOT, Location = Loc() };
                    }
                    return new Token() { View = ".", Value = null, Type = TokenType.DOT, Location = Loc() };
                case ',':
                    Next();
                    return new Token() { View = ",", Value = null, Type = TokenType.COMMA, Location = Loc() };
                case 'Ё':
                    Next();
                    commented = !commented;
                    return new Token() { View = "Ё", Value = null, Type = TokenType.COMMENTO, Location = Loc() };
                case '\n':
                    Next();
                    return new Token() { View = "\n", Value = null, Type = TokenType.SLASH_N, Location = Loc() };
                case ':':
                    Next();
                    return new Token() { View = ":", Value = null, Type = TokenType.COLON, Location = Loc() };
                case '?':
                    Next();
                    return new Token() { View = "?", Value = null, Type = TokenType.QUESTION, Location = Loc() };
                case '|':
                    Next();
                    return new Token() { View = "|", Value = null, Type = TokenType.STICK, Location = Loc() };
                case '\0':
                    return new Token() { View = null, Value = null, Type = TokenType.EOF, Location = Loc() };
                default:
                    throw new Exception($"{Loc()}\nНЕ СУЩЕСТВУЮЩИЙ СИМВОЛ В ДАННОМ ЯЗЫКЕ <{Current}> <{(int)Current}>");
            }
        }

        public Token[] Tokenize()
        {
            List<Token> tokens = new List<Token>();
            while (true)
            {
                Token token = NextToken();
                if (token.Type == TokenType.COMMENTO)
                    while (true)
                    {
                        token = NextToken();
                        if (token.Type == TokenType.EOF || token.Type == TokenType.COMMENTO)
                        {
                            token = NextToken();
                            break;
                        }
                    }
                if (token.Type != TokenType.WHITESPACE)
                    tokens.Add(token);
                if (token.Type == TokenType.EOF)
                    break;
            }
            return tokens.ToArray();
        }
    }
}
