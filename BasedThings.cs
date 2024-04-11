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
        [StringValue("ТОЧКАТОЧКА")]
        DOTDOT,
        [StringValue("..=")]
        DOTDOTEQ,
        [StringValue("ЗНАК ВОПРОСА")]
        QUESTION,
        [StringValue("ПАЛКА | ")]
        STICK,

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
        [StringValue("ТОЧНО")]
        EXACTLY,
        [StringValue("ЗАПОЛНИТЬ")]
        FILL,
        [StringValue("КОТОРЫЙ/АЯ/ОЕ")]
        WHICH,
        [StringValue("ВКЛЮЧИТЬ")]
        IMPORT,
        [StringValue("НАСЛЕДУЕТ")]
        SON,
        [StringValue("БРОСИТЬ")]
        THROW,
        [StringValue("ПОПРОБОВАТЬ")]
        TRY,
        [StringValue("ПОЙМАЙТЬ")]
        CATCH,

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
        [StringValue("ДО")]
        TILL, 
        [StringValue("ШАГ")]
        STEP,

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

    public class Location
    {
        public int Line { get; set; }
        public int Letter { get; set; }

        public Location(int line, int letter)
        {
            Line = line;
            Letter = letter;
        }

        public override string ToString() => $"<СТРОКА {Line}, СИМВОЛ {Letter}>";
    }

    public class Token
    {
        public string View { get; set; }
        public object Value { get; set; }
        public TokenType Type { get; set; }

        public Location Location { get; set; }

        public Token Clone() => new Token() { Value = Value, View = View, Type = Type, Location = Location };

        public override string ToString() => $"<{View}> <{Convert.ToString(Value)}> <{Type.GetStringValue()}> <{Location}>";
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
        public object Execute(params object[] obj) 
        { 
            if (Body is null)
            {
                object got = GetAttribute("_вызов");
                if (got is IClass)
                {
                    IClass method = got as IClass;
                    if (method.Body is null)
                        throw new Exception($"НЕ ЯВЛЯЕТСЯ ОБЬЕКТОМ ДЛЯ ВЫЗОВА: <{method}>");
                    return method.Execute(obj);
                   // return new MethodExpression(new VariableExpression(new Token() { View = Name }), new Token() { View = "_вызов" }, ).Evaluated();
                }
                throw new Exception($"МЕТОД <_вызов> ОКАЗАЛСЯ НЕ МЕТОДОМ А <{got}>");
            }
            return Body.Execute(obj);
        }
        
        public IClass Clone() => new IClass(Name, new Dictionary<string, object>(Attributes), Body is null ? null : Body.Cloned());

        public IFunction Cloned() => Clone();
        
        public bool ContainsAttribute(string key) => Attributes.ContainsKey(key);

        public object GetAttribute(string key) => ContainsAttribute(key) ? Attributes[key] : throw new Exception($"НЕТУ ТАКОГО АТРИБУТА: <{key}> В <{this}>");// Objects.NOTHING;

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
            switch (Name)
            {
                case "ЯЧисло":
                    return $"<ЯЧисло>";
                case "ЯСтрока":
                    return $"<ЯСтрока>";
                case "ЯТочка":
                    return $"<ЯТочка>";
                case "ЯЛист":
                    return $"<ЯЛист>";
                case "ЯПравда":
                    return $"<ЯПравда>";
                default:
                    if (ContainsAttribute("строкой"))
                    {
                        object strokoi = GetAttribute("строкой");
                        if (strokoi is IClass)
                            if (!(((IClass)strokoi).Body is null))
                                return Convert.ToString(((IClass)strokoi).Body.Execute());
                    }
                    return $"<ОБЬЕКТ КЛАССА {Name}>";
            }
        }
    }

    public static partial class Objects
    {

        /*          METHODS          */

        // system
        public static IClass WriteWithoutSlashN = new IClass("очерк", new Dictionary<string, object>(), new WriteNotLn());
        public static IClass Input = new IClass("ввод", new Dictionary<string, object>(), new InputFunction());
        public static IClass ToDate = new IClass("датой", new Dictionary<string, object>(), new ToDateFunction());

        // math
        public static IClass Sinus = new IClass("синус", new Dictionary<string, object>(), new Sinus());
        public static IClass Cosinus = new IClass("косинус", new Dictionary<string, object>(), new Cosinus());
        public static IClass Ceiling = new IClass("потолок", new Dictionary<string, object>(), new Ceiling());
        public static IClass Floor = new IClass("пол", new Dictionary<string, object>(), new Floor());
        public static IClass Tan = new IClass("тангенс", new Dictionary<string, object>(), new Tan());
        public static IClass Max = new IClass("максимум", new Dictionary<string, object>(), new Max());
        public static IClass Min = new IClass("минимум", new Dictionary<string, object>(), new Min());
        public static IClass Square = new IClass("корень", new Dictionary<string, object>(), new Square());
        public static IClass Randomed = new IClass("случайный", new Dictionary<string, object>(), new RandomFunction());
        public static IClass Bin = new IClass("двоичным", new Dictionary<string, object>(), new BinFunction());
        public static IClass BinPlus = new IClass("двоич_плюс", new Dictionary<string, object>(), new BinPlusFunction());

        // io
        public static IClass ReadAllFile = new IClass("вычитать", new Dictionary<string, object>(), new ReadAllFileFunction());
        public static IClass WritingFile = new IClass("писать", new Dictionary<string, object>(), new WritingFileFunction());
        
        // methods
        public static IClass Dir = new IClass("власть", new Dictionary<string, object>(), new DirFunction());

        public static IClass Length = new IClass("длина", new Dictionary<string, object>(), new LenghtFunction());
        public static IClass Split = new IClass("раздел", new Dictionary<string, object>(), new SplitFunction());
        public static IClass Map = new IClass("перебор", new Dictionary<string, object>(), new MapFunction());
        public static IClass Filter = new IClass("фильтр", new Dictionary<string, object>(), new FilterFunction());
        public static IClass Append = new IClass("добавить", new Dictionary<string, object>(), new AppendFunction());
        public static IClass DeleteItem = new IClass("удалить", new Dictionary<string, object>(), new DeleteItemFunction());
        public static IClass JoinedBy = new IClass("соединен", new Dictionary<string, object>(), new JoinedByFunction());
        public static IClass Sort = new IClass("порядком", new Dictionary<string, object>(), new SortFunction());
        public static IClass Contains = new IClass("содержит", new Dictionary<string, object>(), new ContainsFunction());
        public static IClass Reverse = new IClass("обратно", new Dictionary<string, object>(), new ReverseFunction());
        public static IClass Sum = new IClass("суммой", new Dictionary<string, object>(), new SumFunction());

        public static IClass ASCIICode = new IClass("чаркод", new Dictionary<string, object>(), new ASCIICodeFunction());
        public static IClass FromASCIICode = new IClass("символом", new Dictionary<string, object>(), new FromASCIICodeFunction());
        public static IClass IsUpper = new IClass("высок", new Dictionary<string, object>(), new IsUpperFunction());
        public static IClass IsLower = new IClass("низок", new Dictionary<string, object>(), new IsLowerFunction());
        public static IClass ToUpper = new IClass("высоким", new Dictionary<string, object>(), new ToUpperFunction());
        public static IClass ToLower = new IClass("низким", new Dictionary<string, object>(), new ToLowerFunction());
        public static IClass Joined = new IClass("соединить", new Dictionary<string, object>(), new JoinedFunction());
        public static IClass Replace = new IClass("замена", new Dictionary<string, object>(), new ReplaceFunction());
        public static IClass Trim = new IClass("обрез", new Dictionary<string, object>(), new TrimFunction());
        public static IClass TrimStart = new IClass("обрез_лев", new Dictionary<string, object>(), new TrimStartFunction());
        public static IClass TrimEnd = new IClass("обрез_прав", new Dictionary<string, object>(), new TrimEndFunction());
        public static IClass Count = new IClass("счет", new Dictionary<string, object>(), new CountFunction());
        
        // to type
        public static IClass Stringing = new IClass("строчить", new Dictionary<string, object>(), new StringingFunction());
        public static IClass Inting = new IClass("числить", new Dictionary<string, object>(), new IntingFunction());
        public static IClass Doubling = new IClass("точить", new Dictionary<string, object>(), new DoublingFunction());
        public static IClass Listing = new IClass("листить", new Dictionary<string, object>(), new ListingFunction());

        /*           TYPES           */

        public static IClass IInteger = new IClass("ЯЧисло", new Dictionary<string, object>
        {
            { "числом", new IClass("_числом", new Dictionary<string, object>(), Inting.Cloned()) },
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "точкой", new IClass("_точкой", new Dictionary<string, object>(), Doubling.Cloned()) },
            { "потолок", new IClass("_потолок", new Dictionary<string, object>(), Ceiling.Cloned()) },
            { "пол", new IClass("_пол", new Dictionary<string, object>(), Floor.Cloned()) },
            { "длина", new IClass("_длина", new Dictionary<string, object>(), Length.Cloned()) },
            { "чаркод", new IClass("_чаркод", new Dictionary<string, object>(), ASCIICode.Cloned()) },
            { "символом", new IClass("_символом", new Dictionary<string, object>(), FromASCIICode.Cloned()) },
            { "чаром", new IClass("_символом", new Dictionary<string, object>(), FromASCIICode.Cloned()) },
            { "мин", long.MinValue },
            { "макс", long.MaxValue },
            { "владеет", new IClass("_владеет", new Dictionary<string, object>(), Dir.Cloned()) },
            { "дир", new IClass("_владеет", new Dictionary<string, object>(), Dir.Cloned()) },
            { "двоичным", new IClass("_двоичным", new Dictionary<string, object>(), Bin.Cloned()) },
        });

        public static IClass IFloat = new IClass("ЯТочка", new Dictionary<string, object>
        {
            { "числом", new IClass("_числом", new Dictionary<string, object>(), Inting.Cloned()) },
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "точкой", new IClass("_точкой", new Dictionary<string, object>(), Doubling.Cloned()) },
            { "потолок", new IClass("_потолок", new Dictionary<string, object>(), Ceiling.Cloned()) },
            { "пол", new IClass("_пол", new Dictionary<string, object>(), Floor.Cloned()) },
            { "длина", new IClass("_длина", new Dictionary<string, object>(), Length.Cloned()) },
            { "чаркод", new IClass("_чаркод", new Dictionary<string, object>(), ASCIICode.Cloned()) },
            { "символом", new IClass("_символом", new Dictionary<string, object>(), FromASCIICode.Cloned()) },
            { "чаром", new IClass("_символом", new Dictionary<string, object>(), FromASCIICode.Cloned()) },
            { "мин", double.MinValue },
            { "макс", double.MaxValue },
            { "владеет", new IClass("_владеет", new Dictionary<string, object>(), Dir.Cloned()) },
            { "дир", new IClass("_владеет", new Dictionary<string, object>(), Dir.Cloned()) },
        });

        public static IClass IString = new IClass("ЯСтрока", new Dictionary<string, object>
        {
            { "числом", new IClass("_числом", new Dictionary<string, object>(), Inting.Cloned()) },
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "точкой", new IClass("_точкой", new Dictionary<string, object>(), Doubling.Cloned()) },
            { "длина", new IClass("_длина", new Dictionary<string, object>(), Length.Cloned()) },
            { "чаркод", new IClass("_чаркод", new Dictionary<string, object>(), ASCIICode.Cloned()) },
            { "символом", new IClass("_символом", new Dictionary<string, object>(), FromASCIICode.Cloned()) },
            { "чаром", new IClass("_символом", new Dictionary<string, object>(), FromASCIICode.Cloned()) },
            { "высок", new IClass("_высок", new Dictionary<string, object>(), IsUpper.Cloned()) },
            { "низок", new IClass("_низок", new Dictionary<string, object>(), IsLower.Cloned()) },
            { "высоким", new IClass("_высоким", new Dictionary<string, object>(), ToUpper.Cloned()) },
            { "большим", new IClass("_высоким", new Dictionary<string, object>(), ToUpper.Cloned()) },
            { "низким", new IClass("_низким", new Dictionary<string, object>(), ToLower.Cloned()) },
            { "перебор", new IClass("_перебор", new Dictionary<string, object>(), Map.Cloned()) },
            { "фильтр", new IClass("_фильтр", new Dictionary<string, object>(), Filter.Cloned()) },
            { "листом", new IClass("_листом", new Dictionary<string, object>(), Listing.Cloned()) },
            { "удали", new IClass("_удалить", new Dictionary<string, object>(), DeleteItem.Cloned()) },
            { "удалить", new IClass("_удалить", new Dictionary<string, object>(), DeleteItem.Cloned()) },
            { "соедини", new IClass("_соединить", new Dictionary<string, object>(), Joined.Cloned()) },
            { "соединить", new IClass("_соединить", new Dictionary<string, object>(), Joined.Cloned()) },
            { "обратно", new IClass("_обратно", new Dictionary<string, object>(), Reverse.Cloned()) },
            { "порядком", new IClass("_порядком", new Dictionary<string, object>(), Sort.Cloned()) },
            { "порядок", new IClass("_порядком", new Dictionary<string, object>(), Sort.Cloned()) },
            { "сорт", new IClass("_порядком", new Dictionary<string, object>(), Sort.Cloned()) },
            { "содержит", new IClass("_содержит", new Dictionary<string, object>(), Contains.Cloned()) },
            { "имеет", new IClass("_содержит", new Dictionary<string, object>(), Contains.Cloned()) },
            { "раздел", new IClass("_раздел", new Dictionary<string, object>(), Split.Cloned()) },
            { "сплит", new IClass("_сплит", new Dictionary<string, object>(), Split.Cloned()) },
            { "владеет", new IClass("_владеет", new Dictionary<string, object>(), Dir.Cloned()) },
            { "дир", new IClass("_владеет", new Dictionary<string, object>(), Dir.Cloned()) },
            { "замена", new IClass("_замена", new Dictionary<string, object>(), Replace.Cloned()) },
            { "замени", new IClass("_замена", new Dictionary<string, object>(), Replace.Cloned()) },
            { "обрез", new IClass("_обрез", new Dictionary<string, object>(), Trim.Cloned()) },
            { "обрез_лев", new IClass("_обрез_лев", new Dictionary<string, object>(), TrimStart.Cloned()) },
            { "обрез_прав", new IClass("_обрез_прав", new Dictionary<string, object>(), TrimEnd.Cloned()) },
            { "счёт", new IClass("_счет", new Dictionary<string, object>(), Count.Cloned()) },
            { "счет", new IClass("_счет", new Dictionary<string, object>(), Count.Cloned()) },
        });

        public static IClass IBool = new IClass("ЯПравда", new Dictionary<string, object>
        {
            { "числом", new IClass("_числом", new Dictionary<string, object>(), Inting.Cloned()) },
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "точкой", new IClass("_точкой", new Dictionary<string, object>(), Doubling.Cloned()) },
            { "длина", new IClass("_длина", new Dictionary<string, object>(), Length.Cloned()) },
            { "чаркод", new IClass("_чаркод", new Dictionary<string, object>(), ASCIICode.Cloned()) },
            { "символом", new IClass("_символом", new Dictionary<string, object>(), FromASCIICode.Cloned()) },
            { "чаром", new IClass("_символом", new Dictionary<string, object>(), FromASCIICode.Cloned()) },
            { "владеет", new IClass("_владеет", new Dictionary<string, object>(), Dir.Cloned()) },
            { "дир", new IClass("_владеет", new Dictionary<string, object>(), Dir.Cloned()) },
        });

        public static IClass IList = new IClass("ЯЛист", new Dictionary<string, object>
        {
            { "строкой", new IClass("_строкой", new Dictionary<string, object>(), Stringing.Cloned()) },
            { "мин", new IClass("_минимум", new Dictionary<string, object>(), Min.Cloned()) },
            { "макс", new IClass("_максимум", new Dictionary<string, object>(), Max.Cloned()) },
            { "длина", new IClass("_длина", new Dictionary<string, object>(), Length.Cloned()) },
            { "символом", new IClass("_символом", new Dictionary<string, object>(), FromASCIICode.Cloned()) },
            { "чаром", new IClass("_символом", new Dictionary<string, object>(), FromASCIICode.Cloned()) },
            { "перебор", new IClass("_перебор", new Dictionary<string, object>(), Map.Cloned()) },
            { "фильтр", new IClass("_фильтр", new Dictionary<string, object>(), Filter.Cloned()) },
            { "добавь", new IClass("_добавить", new Dictionary<string, object>(), Append.Cloned()) },
            { "добавить", new IClass("_добавить", new Dictionary<string, object>(), Append.Cloned()) },
            { "удали", new IClass("_удалить", new Dictionary<string, object>(), DeleteItem.Cloned()) },
            { "удалить", new IClass("_удалить", new Dictionary<string, object>(), DeleteItem.Cloned()) },
            { "соединен", new IClass("_соединен", new Dictionary<string, object>(), JoinedBy.Cloned()) },
            { "соединён", new IClass("_соединен", new Dictionary<string, object>(), JoinedBy.Cloned()) },
            { "обратно", new IClass("_обратно", new Dictionary<string, object>(), Reverse.Cloned()) },
            { "порядком", new IClass("_порядком", new Dictionary<string, object>(), Sort.Cloned()) },
            { "порядок", new IClass("_порядком", new Dictionary<string, object>(), Sort.Cloned()) },
            { "сорт", new IClass("_порядком", new Dictionary<string, object>(), Sort.Cloned()) },
            { "содержит", new IClass("_содержит", new Dictionary<string, object>(), Contains.Cloned()) },
            { "имеет", new IClass("_содержит", new Dictionary<string, object>(), Contains.Cloned()) },
            { "владеет", new IClass("_владеет", new Dictionary<string, object>(), Dir.Cloned()) },
            { "дир", new IClass("_владеет", new Dictionary<string, object>(), Dir.Cloned()) },
            { "суммой", new IClass("_суммой", new Dictionary<string, object>(), Sum.Cloned()) },
        });

        /*        VARIABLES          */

        public static object NOTHING => Convert.ToInt64(0); // need improving i believe
        public static Stack<Dictionary<string, object>> Registers = new Stack<Dictionary<string, object>>();
        public static Dictionary<string, object> Variables = new Dictionary<string, object>()
        {
            { "Пустой" , new IClass("пустой", new Dictionary<string, object>(), null).Clone() },
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
            { "случайный",  Randomed },
            { "датой",  ToDate },

            { "ввод",  Input.Clone() },
            { "хартия",  Input.Clone() },
            { "ввести",  Input.Clone() },
            { "харатья",  Input.Clone() },

            { "раздел",  Split },
            { "строчить",  Stringing },
            { "числить",  Inting },
            { "точить",  Doubling },
            { "длина", Length },
            { "чаркод", ASCIICode },
            { "символом", FromASCIICode.Clone() },
            { "чаром", FromASCIICode.Clone() },
            { "высок", IsUpper },
            { "низок", IsLower },
            { "высоким", ToUpper },
            { "низким", ToLower },
            { "перебор", Map },
            { "фильтр", Filter },
            { "листить", Listing },
            { "добавить", Append },
            { "удалить", DeleteItem },
            { "соедини", Joined.Clone() },
            { "соединить", Joined.Clone() },
            { "соединен", JoinedBy.Clone() },
            { "соединён", JoinedBy.Clone() },
            { "обратно", Reverse },
            { "порядком", Sort },
            { "иеет", Contains.Clone() },
            { "содержит", Contains.Clone() },
            { "владеет", Dir },
            { "замена", Replace },
            { "обрез", Trim },
            { "обрез_лев", TrimStart },
            { "обрез_прав", TrimEnd },
            { "суммой", Sum },
            { "счет", Count },
            { "двоичным", Bin },
            { "двоич_плюс", BinPlus },

            { "вычитать",  ReadAllFile },
            { "писать",  WritingFile.Clone() },
            { "летописить",  WritingFile.Clone() },
        };

        public static bool ContainsVariable(string key) => Variables.ContainsKey(key);

        public static object GetVariable(string key) => ContainsVariable(key) ? Variables[key] : throw new Exception($"НЕТУ ТАКОЙ ПЕРЕМЕННОЙ: <{key}>");

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
