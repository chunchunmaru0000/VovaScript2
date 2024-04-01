using System;
using System.Collections.Generic;

namespace VovaScript
{
    public class Tokenizator
    {
        private string code;
        private int position;
        private bool commented = false;
        private static Token Nothing = new Token() { View = "", Value = null, Type = TokenType.WHITESPACE };

        public Tokenizator(string code) 
        {
            this.code = code;
            position = 0;
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

        private void Next() { position++; }

        private Token NextToken()
        {
            if (Current == '\0')
                return new Token() { View = null, Value = null, Type = TokenType.EOF };
            if (char.IsWhiteSpace(Current))
            {
                int start = position;
                while (char.IsWhiteSpace(Current))
                    Next();
                string word = code.Substring(start, position - start);
                return new Token() { View = word, Value = null, Type = TokenType.WHITESPACE };
            }
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
                    return new Token() { View = buffer, Value = buffer, Type = TokenType.STRING };
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
                    return new Token() { View = word, Value = Convert.ToInt64(word), Type = TokenType.INTEGER };
                if (dots == 1)
                    return new Token() { View = word, Value = Convert.ToDouble(word), Type = TokenType.DOUBLE };
                throw new Exception("МНОГА ТОЧЕК ДЛЯ ЧИСЛА");
            }
            if (PycTools.Usable(Current))
            {
                int start = position;
                while (PycTools.Usable(Current))
                    Next();
                string word = code.Substring(start, position - start);
                return Worder.Wordizator(new Token() { View = word, Value = null, Type = TokenType.WORD });
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
                            return new Token() { View = "===", Value = null, Type = TokenType.ARROW };
                        }
                        return new Token() { View = "==", Value = null, Type = TokenType.EQUALITY };
                    }
                    if (Current == '>')
                    {
                        Next();
                        return new Token() { View = "=>", Value = null, Type = TokenType.ARROW };
                    }
                    return new Token() { View = "=", Value = null, Type = TokenType.DO_EQUAL };
                case '/':
                    Next();
                    if (Current == '/')
                    {
                        Next();
                        return new Token() { View = "//", Value = null, Type = TokenType.DIV };
                    }
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = "/=", Value = null, Type = TokenType.DIVEQ };
                    }
                    return new Token() { View = "/", Value = null, Type = TokenType.DIVISION };
                case '!':
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = "!=", Value = null, Type = TokenType.NOTEQUALITY };
                    }
                    return new Token() { View = "!", Value = null, Type = TokenType.NOT };
                case '*':
                    Next();
                    if (Current == '*')
                    {
                        Next();
                        return new Token() { View = "**", Value = null, Type = TokenType.POWER };
                    }
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = "*=", Value = null, Type = TokenType.MULEQ };
                    }
                    return new Token() { View = "*", Value = null, Type = TokenType.MULTIPLICATION };
                case '+':
                    Next();
                    if (Current == '+')
                    {
                        Next();
                        return new Token() { View = "++", Value = null, Type = TokenType.PLUSPLUS };
                    }
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = "+=", Value = null, Type = TokenType.PLUSEQ };
                    }
                    return new Token() { View = "+", Value = null, Type = TokenType.PLUS };
                case '-':
                    Next();
                    if (Current == '-')
                    {
                        Next();
                        return new Token() { View = "--", Value = null, Type = TokenType.MINUSMINUS };
                    }
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = "-=", Value = null, Type = TokenType.MINUSEQ };
                    }
                    return new Token() { View = "-", Value = null, Type = TokenType.MINUS };
                case '<':
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = "<=", Value = null, Type = TokenType.LESSEQ };
                    }
                    return new Token() { View = "<", Value = null, Type = TokenType.LESS };
                case '>':
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        return new Token() { View = ">=", Value = null, Type = TokenType.MOREEQ };
                    }
                    return new Token() { View = ">", Value = null, Type = TokenType.MORE };
                case '@':
                    Next();
                    return new Token() { View = "@", Value = null, Type = TokenType.DOG };
                case ';':
                    Next();
                    return new Token() { View = ";", Value = null, Type = TokenType.SEMICOLON };
                case '(':
                    Next();
                    return new Token() { View = "(", Value = null, Type = TokenType.LEFTSCOB };
                case ')':
                    Next();
                    return new Token() { View = ")", Value = null, Type = TokenType.RIGHTSCOB };
                case '[':
                    Next();
                    return new Token() { View = "[", Value = null, Type = TokenType.LCUBSCOB };
                case ']':
                    Next();
                    return new Token() { View = "]", Value = null, Type = TokenType.RCUBSCOB };
                case '{':
                    Next();
                    return new Token() { View = "{", Value = null, Type = TokenType.LTRISCOB };
                case '}':
                    Next();
                    return new Token() { View = "}", Value = null, Type = TokenType.RTRISCOB };
                case '%':
                    Next();
                    return new Token() { View = "%", Value = null, Type = TokenType.MOD };
                case '.':
                    Next();
                    if (Current == '.')
                    {
                        Next();
                        if (Current == '=')
                        {
                            Next();
                            return new Token() { View = "..=", Value = null, Type = TokenType.DOTDOTEQ };
                        }
                        return new Token() { View = "..", Value = null, Type = TokenType.DOTDOT };
                    }
                    return new Token() { View = ".", Value = null, Type = TokenType.DOT };
                case ',':
                    Next();
                    return new Token() { View = ",", Value = null, Type = TokenType.COMMA };
                case 'Ё':
                    Next();
                    commented = !commented;
                    return new Token() { View = "Ё", Value = null, Type = TokenType.COMMENTO };
                case '\n':
                    Next();
                    return new Token() { View = "\n", Value = null, Type = TokenType.SLASH_N };
                case ':':
                    Next();
                    return new Token() { View = ":", Value = null, Type = TokenType.COLON };
                case '?':
                    Next();
                    return new Token() { View = "?", Value = null, Type = TokenType.QUESTION };
                case '|':
                    Next();
                    return new Token() { View = "|", Value = null, Type = TokenType.STICK };
                default:
                    throw new Exception("НЕ СУЩЕСТВУЮЩИЙ СИМВОЛ В ДАННОМ ЯЗЫКЕ");
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
