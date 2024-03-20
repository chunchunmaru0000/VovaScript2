namespace VovaScript
{
    class PycTools
    {
        public static bool Usable (char c) { 
            return c != '+' && c != '-' && c != '*' && c != '/' && c != '%' &&
                   c != '(' && c != ')' && c != '[' && c != ']' && c != '{' && c != '}' &&
                   c != '@' && c != ';' && c != '.' && c != ',' && c != '"' && c != ':' && c != '?' &&
                   c != 'Ё' && c != '\n' && c != ' ' && 
                   c != '<' && c != '>' && c != '!' && c != '=' &&
                   c != '\0';
        }
    }
}
