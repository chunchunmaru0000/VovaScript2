using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace VovaScript
{
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

    public enum TokenType
    {
        //base types
        [StringValue("КОНЕЦ ФАЙЛА")]
        EOF,
        [StringValue("СЛОВО")]
        WORD,
        [StringValue("СТРОКА")]
        STRING,
        [StringValue("ИСТИННОСТЬ")]
        BOOLEAN,
        [StringValue("ЦЕЛОЕ ЧИСЛО64")]
        INTEGER,
        [StringValue("НЕ ЦЕЛОЕ ЧИСЛО64")]
        DOUBLE,
        [StringValue("КЛАСС")]
        CLASS,
        [StringValue("ЭТОТ")]
        THIS,

        //operators
        [StringValue("ПЛЮС")]
        PLUS,
        [StringValue("МИНУС")]
        MINUS,
        [StringValue("УМНОЖЕНИЕ")]
        MULTIPLICATION,
        [StringValue("ДЕЛЕНИЕ")]
        DIVISION,
        [StringValue("СДЕЛАТЬ РАВНЫМ")]
        DO_EQUAL,
        [StringValue("СТРЕЛКА")]
        ARROW,
        [StringValue("БЕЗ ОСТАТКА")]
        DIV,
        [StringValue("ОСТАТОК")]
        MOD,
        [StringValue("СТЕПЕНЬ")]
        POWER,
        [StringValue("+=")]
        PLUSEQ,
        [StringValue("-=")]
        MINUSEQ,
        [StringValue("*=")]
        MULEQ,
        [StringValue("/=")]
        DIVEQ,
        [StringValue("НОВЫЙ")]
        NEW,

        //cmp
        [StringValue("РАВЕН")]
        EQUALITY,
        [StringValue("НЕ РАВЕН")]
        NOTEQUALITY,
        [StringValue(">")]
        MORE,
        [StringValue(">=")]
        MOREEQ,
        [StringValue("<")]
        LESS,
        [StringValue("<=")]
        LESSEQ,
        [StringValue("НЕ")]
        NOT,
        [StringValue("И")]
        AND,
        [StringValue("ИЛИ")]
        OR,

        //other
        [StringValue("ПЕРЕМЕННАЯ")]
        VARIABLE,
        [StringValue("ФУНКЦИЯ")]
        FUNCTION,
        [StringValue(";")]
        SEMICOLON,
        [StringValue(":")]
        COLON,
        [StringValue("++")]
        PLUSPLUS,
        [StringValue("--")]
        MINUSMINUS,
        [StringValue(",")]
        COMMA,

        [StringValue(")")]
        RIGHTSCOB,
        [StringValue("(")]
        LEFTSCOB,
        [StringValue("]")]
        RCUBSCOB,
        [StringValue("[")]
        LCUBSCOB,
        [StringValue("}")]
        RTRISCOB,
        [StringValue("{")]
        LTRISCOB,

        [StringValue("ПЕРЕНОС")]
        SLASH_N,
        [StringValue("ЦИТАТА")]
        COMMENTO,
        [StringValue("ПУСТОТА")]
        WHITESPACE,
        [StringValue("СОБАКА")]
        DOG,
        [StringValue("КАВЫЧКА")]
        QUOTE,
        [StringValue("ТОЧКА")]
        DOT,
        [StringValue("ЗНАК ВОПРОСА")]
        QUESTION,

        //words types
        [StringValue("ЕСЛИ")]
        WORD_IF,
        [StringValue("ИНАЧЕ")]
        WORD_ELSE,
        [StringValue("ИНАЧЕЛИ")]
        WORD_ELIF,
        [StringValue("ПОКА")]
        WORD_WHILE,
        [StringValue("НАЧЕРТАТЬ")]
        WORD_PRINT,
        [StringValue("ДЛЯ")]
        WORD_FOR,
        [StringValue("ИСТИНА")]
        WORD_TRUE,
        [StringValue("ЛОЖЬ")]
        WORD_FALSE,
        [StringValue("ПРОДОЛЖИТЬ")]
        CONTINUE,
        [StringValue("ВЫЙТИ")]
        BREAK,
        [StringValue("ВЕРНУТЬ")]
        RETURN,
        [StringValue("ВЫПОЛНИТЬ ПРОЦЕДУРУ")]
        PROCEDURE,
        [StringValue("СЕЙЧАС")]
        NOW,
        [StringValue("ЧИСТКА")]
        CLEAR,
        [StringValue("СОН")]
        SLEEP,
        [StringValue("РУСИТЬ")]
        VOVASCRIPT,

        //SQL
        [StringValue("СОЗДАТЬ")]
        CREATE,
        [StringValue("БД")]
        DATABASE,
        [StringValue("ТАБЛИЦА")]
        TABLE,
        [StringValue("ДОБАВИТЬ")]
        INSERT,
        [StringValue("В")]
        IN,
        [StringValue("ЗНАЧЕНИЯ")]
        VALUES,
        [StringValue("КОЛОНКИ")]
        COLONS,
        [StringValue("ГДЕ")]
        WHERE,
        [StringValue("ВЫБРАТЬ")]
        SELECT,
        [StringValue("ИЗ")]
        FROM,
        [StringValue("ОТ")]
        AT,
        [StringValue("КАК")]
        AS,

        [StringValue("ЧИСЛО")]
        NUMBER,
        [StringValue("ЧИСЛО С ТОЧКОЙ")]
        FNUMBER,
        [StringValue("СТРОЧКА")]
        STROKE,
        [StringValue("ПРАВДИВОСТЬ")]
        BUL,

        [StringValue("ВСЁ")]
        ALL,
    }

    public class Token
    {
        public string View { get; set; }
        public object Value { get; set; }
        public TokenType Type { get; set; }

        public Token Clone() => new Token() { Value = Value, View = View, Type = Type };

        public override string ToString() => $"<{View}> <{Convert.ToString(Value)}> <{Type.GetStringValue()}>";
    }

    public interface IExpression
    {
        object Evaluated();

        IExpression Clon();
    }

    public interface IStatement
    {
        void Execute();

        IStatement Clone();
    }

    public interface IFunction
    {
        object Execute(params object[] obj);

        IFunction Cloned();
    }

    public class IClass : IExpression, IFunction
    {
        public string Name;
        public object Value;
        public IFunction Body;
        public static Dictionary<string, IClass> HOLLOW = new Dictionary<string, IClass>();
        public Dictionary<string, IClass> Attributes = new Dictionary<string, IClass>();
        public Stack<Dictionary<string, IClass>> Registers = new Stack<Dictionary<string, IClass>>();

        public IClass(string name, object value, Dictionary<string, IClass> attributes, IFunction body = null)
        {
            Name = name;
            Value = value;
            Attributes = attributes;
            Body = body;
            AddAttribute("строкой", new IClass("__строкой__", $"<ОБЬЕКТ КЛАССА {Name}>", new Dictionary<string, IClass>()));

        }

        public IClass(IClass toClone)
        {
            IClass clone = toClone.Clone();
            Name = clone.Name;
            Value = clone.Value;
            Body = clone.Body;
            Attributes = clone.Attributes;
        }

        public IClass(string name, object value)
        {
            Name = name;
            Value = value;
            Body = null;
            Attributes = new Dictionary<string, IClass>();
        }

        public IClass(object value)
        {
            Name = Convert.ToString(value);
            Value = value;
            Body = null;
            Attributes = new Dictionary<string, IClass>();
        }

        public object Evaluated() => Value is IClass ? ((IClass)Value).Evaluated() : Value;

        public object Execute(params object[] obj) => Body is null ? Value : Body.Execute(obj);

        public IExpression Clon() => new NumExpression(Value);
        
        public IClass Clone() => new IClass(Name, Value is IClass ? ((IClass)Value).Clone() : Value, new Dictionary<string, IClass>(Attributes), Body.Cloned());

        public IFunction Cloned() => Body.Cloned();
        
        public bool ContainsAttribute(string key) => Attributes.ContainsKey(key);

        public IClass GetAttribute(string key) => ContainsAttribute(key) ? Attributes[key] : Objects.NOTHING;

        public void AddAttribute(string key, IClass value)
        {
            if (Attributes.ContainsKey(key))
                Attributes[key] = value;
            else
                Attributes.Add(key, value);
        }

        public void Push() => Registers.Push(new Dictionary<string, IClass>(Attributes));

        public void Pop() => Attributes = Registers.Pop();

        public override string ToString() => Convert.ToString(GetAttribute("строкой").Evaluated());
    }

    public static class Objects
    {
        /*        VARIABLES          

        public static IClass DO_NOTHING;
        public static IClass Sinus = new Sinus();
        public static IClass Cosinus = new Cosinus();
        public static IClass Ceiling = new Ceiling();
        public static IClass Floor = new Floor();
        public static IClass Tan = new Tan();
        public static IClass Max = new Max();
        public static IClass Min = new Min();
        public static IClass Square = new Square();
        public static IClass ReadAll = new ReadAllFileFunction();
        public static IClass Split = new SplitFunction();
        public static IClass Input = new InputFunction();
        public static IClass Stringing = new StringingFunction();
        public static IClass Inting = new IntingFunction();
        public static IClass Doubling = new DoublingFunction();
        public static IClass Writing = new WritingFileFunction();
*/
        public static IClass NOTHING = new IClass("НИЧЕГО", (long)0, new Dictionary<string, IClass>()); // need improving i believe
        public static Stack<Dictionary<string, IClass>> Registers = new Stack<Dictionary<string, IClass>>();
        public static Dictionary<string, IClass> Variables = new Dictionary<string, IClass>()
        {/*
            { "ПИ", Math.PI },
            { "Е", Math.E },
            { "ИСПБД", "негр" }
            { "синус", Sinus },
            { "косинус", Cosinus },
            { "потолок", Ceiling },
            { "заземь", Floor },
            { "тангенс", Tan },
            { "макс",  Max },
            { "максимум",  Max },
            { "наибольшее",  Max },
            { "большее",  Max },
            { "меньшее",  Min },
            { "мин",  Min },
            { "корень",  Square },
            { "наименьшее",  Min },
            { "минимум",  Min },
            { "вычитать",  ReadAll },
            { "раздел",  Split },
            { "хартия",  Input },
            { "ввод",  Input },
            { "харатья",  Input },
            { "ввести",  Input },
            { "строчить",  Stringing },
            { "числить",  Inting },
            { "точить",  Doubling },
            { "писать",  Writing },
            { "летописить",  Writing },*/
        };

        public static bool ContainsVariable(string key) => Variables.ContainsKey(key);

        public static IClass GetVariable(string key) => ContainsVariable(key) ? Variables[key] : NOTHING;

        public static void AddVariable(string key, IClass value)
        {
            if (Variables.ContainsKey(key))
                Variables[key] = value;
            else
                Variables.Add(key, value);
        }

        public static void DeleteVariable(string key) => Variables.Remove(key);

        public static void Push()
        {
            Registers.Push(new Dictionary<string, IClass>(Variables));
        }

        public static void Pop()
        {
            Variables = Registers.Pop();
        }

        /*           CLASSES         */

        public static IClass Fedkin = new IClass("Федкин", (long)0, new Dictionary<string, IClass>());

        public static Dictionary<string, IClass> Classes = new Dictionary<string, IClass>()
        {
            { "Федкин", Fedkin }
        };

        public static bool ContainsClass(string key) => Classes.ContainsKey(key);

        public static IClass GetClass(string key) => ContainsClass(key) ? Classes[key] : Classes["ФЕДКИН"];

        public static void AddClass(string key, IClass value)
        {
            if (Classes.ContainsKey(key))
                Classes[key] = value;
            else
                Classes.Add(key, value);
        }
    }
}
