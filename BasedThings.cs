using System;
using System.Collections.Generic;
using System.Linq;

namespace VovaScript
{
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
        [StringValue("ЛЯМБДА")]
        LAMBDA,

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
        [StringValue("ЦИКЛ")]
        LOOP,
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

    public class IClass : IFunction
    {
        public string Name;
        public IFunction Body;
        public static Dictionary<string, object> HOLLOW = new Dictionary<string, object>();
        public Dictionary<string, object> Attributes = new Dictionary<string, object>();
        public Stack<Dictionary<string, object>> Registers = new Stack<Dictionary<string, object>>();

        public IClass(string name, Dictionary<string, object> attributes, IFunction body = null)
        {
            Name = name;
            Attributes = attributes;
            Body = body;
        }
        public object Execute(params object[] obj) => Body is null ? throw new Exception($"НЕ ЯВЛЯЕТСЯ ОБЬЕКТОМ ДЛЯ ВЫЗОВА: <{Name}>") : Body.Execute(obj);
        
        public IClass Clone() => new IClass(Name, new Dictionary<string, object>(Attributes), Body is null ? null : Body.Cloned());

        public IFunction Cloned() => Clone();
        
        public bool ContainsAttribute(string key) => Attributes.ContainsKey(key);

        public object GetAttribute(string key) => ContainsAttribute(key) ? Attributes[key] : Objects.NOTHING;

        public void AddAttribute(string key, object value)
        {
            if (Attributes.ContainsKey(key))
                Attributes[key] = value;
            else
                Attributes.Add(key, value);
        }

        public void Push() => Registers.Push(new Dictionary<string, object>(Attributes));

        public void Pop() => Attributes = Registers.Pop();

        public override string ToString() 
        {
            if (ContainsAttribute("строкой"))
            {
                object strokoi = GetAttribute("строкой");
                if (strokoi is IClass)
                    if (!(((IClass)strokoi).Body is null))
                        return Convert.ToString(((IClass)strokoi).Body.Execute());
           //         else
           //             return $"<ОБЬЕКТ КЛАССА {Name}>";
           //     else
           //         return $"<ОБЬЕКТ КЛАССА {Name}>";
            }
          //  else
                return $"<ОБЬЕКТ КЛАССА {Name}>";
        }
    }

    public static partial class Objects
    {

        /*           BASED           */

        // system
        public static IClass WriteWithoutSlashN = new IClass("очерк", new Dictionary<string, object>(), new WriteNotLn());
        public static IClass Input = new IClass("ввод", new Dictionary<string, object>(), new InputFunction());
        // math
        public static IClass Sinus = new IClass("синус", new Dictionary<string, object>(), new Sinus());
        public static IClass Cosinus = new IClass("косинус", new Dictionary<string, object>(), new Cosinus());
        public static IClass Ceiling = new IClass("потолок", new Dictionary<string, object>(), new Ceiling());
        public static IClass Floor = new IClass("пол", new Dictionary<string, object>(), new Floor());
        public static IClass Tan = new IClass("тангенс", new Dictionary<string, object>(), new Tan());
        public static IClass Max = new IClass("максимум", new Dictionary<string, object>(), new Max());
        public static IClass Min = new IClass("минимум", new Dictionary<string, object>(), new Min());
        public static IClass Square = new IClass("корень", new Dictionary<string, object>(), new Square());
        // io
        public static IClass ReadAllFile = new IClass("вычитать", new Dictionary<string, object>(), new ReadAllFileFunction());
        public static IClass WritingFile = new IClass("писать", new Dictionary<string, object>(), new WritingFileFunction());
        // methods
        public static IClass Split = new IClass("раздел", new Dictionary<string, object>(), new SplitFunction());
        // to type
        public static IClass Stringing = new IClass("строчить", new Dictionary<string, object>(), new StringingFunction());
        public static IClass Inting = new IClass("числить", new Dictionary<string, object>(), new IntingFunction());
        public static IClass Doubling = new IClass("точить", new Dictionary<string, object>(), new DoublingFunction());

        /*           TYPES           */

        public static IClass IInteger = new IClass("ЯЧисло", new Dictionary<string, object>
        {
            { "числом", new IClass("_числом", new Dictionary<string, object>(), Inting.Cloned()) },
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "точкой", new IClass("_точкой", new Dictionary<string, object>(), Doubling.Cloned()) },
            { "потолок", new IClass("_потолок", new Dictionary<string, object>(), Ceiling.Cloned()) },
            { "пол", new IClass("_пол", new Dictionary<string, object>(), Floor.Cloned()) },
        });

        public static IClass IFloat = new IClass("ЯТочка", new Dictionary<string, object>
        {
            { "числом", new IClass("_числом", new Dictionary<string, object>(), Inting.Cloned()) },
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "точкой", new IClass("_точкой", new Dictionary<string, object>(), Doubling.Cloned()) },
            { "потолок", new IClass("_потолок", new Dictionary<string, object>(), Ceiling.Cloned()) },
            { "пол", new IClass("_пол", new Dictionary<string, object>(), Floor.Cloned()) },
        });

        public static IClass IString = new IClass("ЯСтрока", new Dictionary<string, object>
        {
            { "числом", new IClass("_числом", new Dictionary<string, object>(), Inting.Cloned()) },
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "точкой", new IClass("_точкой", new Dictionary<string, object>(), Doubling.Cloned()) },
        });

        public static IClass IBool = new IClass("ЯПравда", new Dictionary<string, object>
        {
            { "числом", new IClass("_числом", new Dictionary<string, object>(), Inting.Cloned()) },
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "точкой", new IClass("_точкой", new Dictionary<string, object>(), Doubling.Cloned()) },
        });

        public static IClass IList = new IClass("ЯЛист", new Dictionary<string, object>
        {
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "мин", new IClass("_минимум", new Dictionary<string, object>(), Min.Cloned()) },
            { "макс", new IClass("_максимум", new Dictionary<string, object>(), Max.Cloned()) },
        });

        /*        VARIABLES          */

        public static object NOTHING = (long)0; // need improving i believe
        public static Stack<Dictionary<string, object>> Registers = new Stack<Dictionary<string, object>>();
        public static  Dictionary<string, object> Variables = new Dictionary<string, object>()
        {
            { "ЯЧисло", IInteger },
            { "ЯСтрока", IString },
            { "ЯТочка", IFloat },
            { "ЯПравда", IBool },
            { "ЯЛист", IList },

            { "очерк", WriteWithoutSlashN },

            { "ПИ", Math.PI },
            { "Е", Math.E },
            { "ИСПБД", "дада" },
            { "синус", Sinus },
            { "косинус", Cosinus },
            { "потолок", Ceiling },
            { "заземь", Floor.Clone() },
            { "пол", Floor.Clone() },
            { "тангенс", Tan },
            { "макс",  Max.Clone() },
            { "большее",  Max.Clone() },
            { "максимум",  Max.Clone() },
            { "наибольшее",  Max.Clone() },
            { "мин",  Min.Clone() },
            { "меньшее",  Min.Clone() },
            { "минимум",  Min.Clone() },
            { "наименьшее",  Min.Clone() },
            { "корень",  Square },

            { "ввод",  Input.Clone() },
            { "хартия",  Input.Clone() },
            { "ввести",  Input.Clone() },
            { "харатья",  Input.Clone() },

            { "раздел",  Split },
            { "строчить",  Stringing },
            { "числить",  Inting },
            { "точить",  Doubling },

            { "вычитать",  ReadAllFile },
            { "писать",  WritingFile.Clone() },
            { "летописить",  WritingFile.Clone() },
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

        public static void Push() => Registers.Push(new Dictionary<string, object>(Variables));

        public static void Pop() => Variables = Registers.Pop();
    }
}
