using System;
using System.Linq;

namespace VovaScript
{
    public static class PycTools
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

    public static class TypePrint
    {
        public static string Pyc(object value)
        {
            switch (value.GetType().ToString())
            {
                case "System.String":
                    return "СТРОКА";
                case "System.Int32":
                    return "ЧИСЛО 32 ???";
                case "System.Int64":
                    return "ЧИСЛО 64";
                case "System.Double":
                    return "ЧИСЛО С ТОЧКОЙ 64";
                case "System.Boolean":
                    return "ПРАВДИВОСТЬ";
                case "VovaScript.UserFunction":
                    return "ПОЛЬЗОВАТЕЛЬСКАЯ ФУНКЦИЯ";
                case "VovaScript.Sinus":
                    return "ФУНКЦИЯ СИНУС";
                case "VovaScript.Cosinus":
                    return "ФУНКЦИЯ КОСИНУС";
                case "VovaScript.Ceiling":
                    return "ФУНКЦИЯ ПОТОЛОК";
                case "VovaScript.Floor":
                    return "ФУНКЦИЯ ЗАЗЕМЬ";
                case "VovaScript.Tan":
                    return "ФУНКЦИЯ ТАНГЕНС";
                case "VovaScript.FunctionExpression":
                    return "ФУНКЦИЯ";
                case "VovaScript.Max":
                    return "ФУНКЦИЯ НАИБОЛЬШЕЕ";
                case "VovaScript.Min":
                    return "НАИМЕНЬШЕЕ";
                case "System.Collections.Generic.List`1[System.Object]":
                    return "СПИСОК";
                case "[System.Collections.Generic.List`1[VovaScript.IExpression]]":
                    return "СПИСОК";
                case "System.Object[]":
                    return "СПИСОК";
                case "VovaScript.SQLSelectExpression":
                    return "ВЫБОР ИЗ ТАБЛИЦЫ";
                case "VovaScript.DeclareFunctionStatement":
                    return "НАЗНАЧИТЬ ФУНКЦИЮ";
                case "VovaScript.AssignStatement":
                    return "НАЗНАЧИТЬ ПЕРЕМЕННУЮ";
                case "VovaScript.MethodExpression":
                    return "МЕТОД";
                default:
                    return value.GetType().ToString();
                    //throw new Exception($"НЕ ПОМНЮ ЧТО БЫ ДОБАЛЯЛ ТАКОЙ ТИП: <{value.GetType().Name}> У <{value}>");
            }
        }
    }

    public class StringValueAttribute : Attribute
    {
        public string Value { get; }

        public StringValueAttribute(string value)
        {
            Value = value;
        }
    }

    public static class EnumExtensions
    {
        public static string GetStringValue(this Enum value)
        {
            var type = value.GetType();
            var fieldInfo = type.GetField(value.ToString());
            var attribs = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
            return attribs.Length > 0 ? attribs[0].Value : null;
        }
    }

    public class VovaScriptException : Exception
    {
        public new string Message;

        public VovaScriptException(string message) => Message = message;
    }

    public static class HelpMe
    {
        public static object[] GetAttrAndValue(Token ObjName, Token[] Attributes, IExpression Value)
        {
            if (Objects.ContainsVariable(ObjName.View))
            {
                object got = Objects.GetVariable(ObjName.View);
                if (got is IClass)
                {
                    IClass classObject = got as IClass;
                    IClass last;
                    for (int i = 0; i < Attributes.Length - 1; i++)
                    {
                        if (classObject.ContainsAttribute(Attributes[i].View))
                        {
                            got = classObject.GetAttribute(Attributes[i].View);
                            last = classObject;
                            if (got is IClass)
                            {
                                classObject = got as IClass;
                                continue;
                            }
                            if (i == Attributes.Length - 1)
                                return new object[] { last, Attributes[i].View, Value.Evaluated() };
                            throw new Exception($"НЕ ОБЪЕКТ: <{Attributes[i]}> ГДЕ-ТО В <{ObjName}>");
                        }
                        if (i == Attributes.Length - 1)
                            return new object[] { classObject, Attributes[i].View, Value.Evaluated() };
                        throw new Exception($"НЕСУЩЕСТВУЮЩИЙ КАК ОБЪЕКТ: <{Attributes[i]}> ГДЕ-ТО В <{ObjName}>");
                    }
                    return new object[] { classObject, Attributes.Last().View, Value.Evaluated() };
                }
            }
            throw new Exception($"НЕСУЩЕСТВУЮЩИЙ КАК ОБЪЕКТ: <{ObjName}>");
        }
    }
}
