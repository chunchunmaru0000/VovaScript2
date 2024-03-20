using Microsoft.Win32;
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

        public override string ToString()
        {
            return $"<{View}> <{Convert.ToString(Value)}> <{Type.GetStringValue()}>";
        }
    }

    public interface IExpression
    {
        object Evaluated();

        string ToString();
    }

    public interface IStatement
    {
        void Execute();

        string ToString();
    }

    public interface IFunction
    {
        object Execute(params object[] obj);
    }

    public sealed class IClass
    {
        public string Name;
        public Dictionary<string, object> Attributes = new Dictionary<string, object>();
        public Dictionary<string, UserFunction> Methods = new Dictionary<string, UserFunction>();
        public Dictionary<string, IClass> ClassObjects = new Dictionary<string, IClass>();
        public Stack<Dictionary<string, object>> Registers = new Stack<Dictionary<string, object>>();
        public Stack<Dictionary<string, UserFunction>> RegistersF = new Stack<Dictionary<string, UserFunction>>();
        public Stack<Dictionary<string, IClass>> RegistersO = new Stack<Dictionary<string, IClass>>();

        public IClass(string name, Dictionary<string, object> attributes, Dictionary<string, UserFunction> methods)
        {
            Name = name;
            Attributes = attributes;
            Methods = methods;
        }

        public IClass Clone() => new IClass(Name, new Dictionary<string, object>(Attributes), new Dictionary<string, UserFunction>(Methods));

        public bool ContainsAttribute(string key) => Attributes.ContainsKey(key);

        public object GetAttribute(string key) => ContainsAttribute(key) ? Attributes[key] : Objects.NOTHING;

        public void AddAttribute(string key, object value)
        {
            if (Attributes.ContainsKey(key))
                Attributes[key] = value;
            else
                Attributes.Add(key, value);
        }

        public void Push()
        {
            Registers.Push(new Dictionary<string, object>(Attributes));
            RegistersF.Push(new Dictionary<string, UserFunction>(Methods));
            RegistersO.Push(new Dictionary<string, IClass>(ClassObjects));
        }

        public void Pop()
        {
            Attributes = Registers.Pop();
            Methods = RegistersF.Pop();
            ClassObjects = RegistersO.Pop();
        }
        //metods
        public bool ContainsMethod(string key) => Methods.ContainsKey(key);

        public UserFunction GetMethod(string key) => ContainsMethod(key) ? Methods[key] : throw new Exception($"НЕТУ ТАКОГО МЕТОДА В КЛАССЕ ДАННОГО ОБЬЕКТА: <{Name}>");

        public void AddMethod(string key, UserFunction value)
        {
            if (Methods.ContainsKey(key))
                Methods[key] = value;
            else
                Methods.Add(key, value);
        }
        //objs
        public bool ContainsClassObject(string key) => ClassObjects.ContainsKey(key);

        public IClass GetClassObject(string key) => ContainsClassObject(key) ? ClassObjects[key] : ClassObjects["ФЕДКИН"];

        public void AddClassObject(string key, IClass value)
        {
            if (ClassObjects.ContainsKey(key))
                ClassObjects[key] = value;
            else
                ClassObjects.Add(key, value);
        }

        public override string ToString() => $"<ОБЬЕКТ КЛАССА {Name}>";
    }

    public static class Objects
    {
        /*        VARIABLES          */

        public static object NOTHING = (long)0; // need improving i believe
        public static Stack<Dictionary<string, object>> Registers = new Stack<Dictionary<string, object>>();
        public static Stack<Dictionary<string, IFunction>> RegistersF = new Stack<Dictionary<string, IFunction>>();
        public static Stack<Dictionary<string, IClass>> RegistersO = new Stack<Dictionary<string, IClass>>();
        public static Dictionary<string, object> Variables = new Dictionary<string, object>()
        {
            { "ПИ", Math.PI },
            { "Е", Math.E },
            { "ИСПБД", "негр" }
        };

        public static bool ContainsVariable(string key) => Variables.ContainsKey(key);

        public static object GetVariable(string key) => ContainsVariable(key) ? Variables[key] : NOTHING;

        public static void AddVariable(string key, object value)
        {
            if (Variables.ContainsKey(key))
                Variables[key] = value;
            else
                Variables.Add(key, value);
        }

        public static void DeleteVariable(string key) => Variables.Remove(key);

        public static void Push()
        {
            Registers.Push(new Dictionary<string, object>(Variables));
            RegistersF.Push(new Dictionary<string, IFunction>(Functions));
            RegistersO.Push(new Dictionary<string, IClass>(ClassObjects));
        }

        public static void Pop()
        {
            Variables = Registers.Pop();
            Functions = RegistersF.Pop();
            ClassObjects = RegistersO.Pop();
        }

        /*        FUNCTIONS          */

        public static IFunction DO_NOTHING;
        public static IFunction Sinus = new Sinus();
        public static IFunction Cosinus = new Cosinus();
        public static IFunction Ceiling = new Ceiling();
        public static IFunction Floor = new Floor();
        public static IFunction Tan = new Tan();
        public static IFunction Max = new Max();
        public static IFunction Min = new Min();
        public static IFunction Square = new Square();
        public static IFunction ReadAll = new ReadAllFileFunction();
        public static IFunction Split = new SplitFunction();
        public static IFunction Input = new InputFunction();
        public static IFunction Stringing = new StringingFunction();
        public static IFunction Inting = new IntingFunction();
        public static IFunction Doubling = new DoublingFunction();
        public static IFunction Writing = new WritingFileFunction();

        public static Dictionary<string, IFunction> Functions = new Dictionary<string, IFunction>()
        {
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
            { "летописить",  Writing },
        };

        public static bool ContainsFunction(string key) => Functions.ContainsKey(key);

        public static IFunction GetFunction(string key) => ContainsFunction(key) ? Functions[key] : DO_NOTHING;

        public static void AddFunction(string key, IFunction value)
        {
            if (Functions.ContainsKey(key))
                Functions[key] = value;
            else
                Functions.Add(key, value);
        }

        /*           CLASSES         */

        public static IClass Fedkin = new IClass("Федкин", new Dictionary<string, object>(), new Dictionary<string, UserFunction>());

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

        /*        CLASS OBJECTS      */

        public static Dictionary<string, IClass> ClassObjects = new Dictionary<string, IClass>() 
        {
            { "ФЕДКИН", Fedkin }
        };

        public static bool ContainsClassObject(string key) => ClassObjects.ContainsKey(key);

        public static IClass GetClassObject(string key) => ContainsClassObject(key) ? ClassObjects[key] : ClassObjects["ФЕДКИН"];

        public static void AddClassObject(string key, IClass value)
        {
            if (ClassObjects.ContainsKey(key))
                ClassObjects[key] = value;
            else
                ClassObjects.Add(key, value);
        }

        public static void DeleteClassObject(string key) => ClassObjects.Remove(key);
    }
}
