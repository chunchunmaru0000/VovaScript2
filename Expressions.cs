using System;
using System.Collections.Generic;
using System.Linq;

namespace VovaScript
{
    public sealed class NumExpression : IExpression
    {
        public Token Value;

        public NumExpression(Token value) => Value = value;

        public NumExpression(object value) => Value = new Token() { Value = value };

        public object Evaluated() => Value.Value;

        public IExpression Clon() => new NumExpression(Value.Clone());

        public override string ToString()
        {
            object value = Value.Value;
            if (value is List<object>)
                return PrintStatement.ListString((List<object>)value);
            if (value is bool)
                return (bool)value ? "Истина" : "Ложь";
            return Convert.ToString(Value);
        }
    }

    public sealed class UnaryExpression : IExpression
    {
        public Token Operation;
        public IExpression Value;
        
        public UnaryExpression(Token operation, IExpression value)
        {
            Operation = operation;
            Value = value;
        }

        public IExpression Clon() => new UnaryExpression(Operation.Clone(), Value.Clon());

        public object Evaluated()
        {
            object value = Value.Evaluated();
            switch (Operation.Type) 
            {
                case TokenType.PLUS:
                    return value;
                case TokenType.MINUS:
                    if (value is long)
                        return -Convert.ToInt64(value);
                    else 
                        return -(double)value;
                case TokenType.NOT:
                    return !Convert.ToBoolean(value);
                default:
                    Console.WriteLine($"<{Value.Evaluated()}> <{Value}> <{Operation.Type}> <{Operation.View}>");
                    throw new Exception("ДА КАК ТАК ВООБЩЕ ВОЗМОЖНО ЧТО ЛИБО ПОСТАВИТЬ КРОМЕ + ИЛИ - ПЕРЕД ЧИСЛОМ");
            }
        }

        public override string ToString() => Operation.View + ' ' + Value.ToString();
    }

    public sealed class BinExpression : IExpression 
    {
        public IExpression Left;
        public Token Operation;
        public IExpression Right;

        public BinExpression(IExpression left, Token operation, IExpression right)
        {
            Left = left;
            Operation = operation;
            Right = right;
        }

        public IExpression Clon() => new BinExpression(Left.Clon(), Operation.Clone(), Right.Clon());

        public object Evaluated()
        {
            object lft = Left.Evaluated();
            object rght = Right.Evaluated();
            if (lft is string || rght is string)
            {
                string slft = lft is bool ? (bool)lft ? "Истина" : "Ложь" : Convert.ToString(lft);
                string srght = rght is bool ? (bool)rght ? "Истина" : "Ложь" : Convert.ToString(rght);
                switch (Operation.Type)
                {
                    case TokenType.PLUS:
                        return slft + srght;
                    case TokenType.MINUS:
                        string str = slft;
                        return str.Replace(srght, "");
                    case TokenType.MULTIPLICATION:
                        string result = "";
                        for (int i = 0; i < srght.Length; i++)
                            result += slft + srght;
                        return result;
                    default:
                        throw new Exception($"НЕПОДДЕРЖИВАЕМАЯ БИНАРНАЯ ОПЕРАЦИЯ ДЛЯ СТРОКИ: {lft} {Operation.Type} {rght} | {Left} {Operation} {Right}");
                }
            }
            else switch (Operation.Type)
            {
                case TokenType.PLUS:
                    if (lft is double || rght is double) 
                        return Convert.ToDouble(lft) + Convert.ToDouble(rght);
                    if (lft is List<object>)
                        if (rght is List<object>)
                        {
                            ((List<object>)lft).AddRange((List<object>)rght);
                            return lft;
                        }
                        else
                        {
                            ((List<object>)lft).Add(rght);
                            return lft;
                        }
                    return Convert.ToInt64(lft) + Convert.ToInt64(rght);
                case TokenType.MINUS:
                    if (lft is double || rght is double)
                        return Convert.ToDouble(lft) - Convert.ToDouble(rght);
                    return Convert.ToInt64(lft) - Convert.ToInt64(rght);
                case TokenType.MULTIPLICATION:
                    if (lft is double || rght is double)
                        return Convert.ToDouble(lft) * Convert.ToDouble(rght);
                    return Convert.ToInt64(lft) * Convert.ToInt64(rght);
                case TokenType.DIVISION:
                    if (Convert.ToDouble(rght) != 0)
                    {
                        if (lft is double || rght is double)
                            return Convert.ToDouble(lft) / Convert.ToDouble(rght);
                        return Convert.ToInt64(lft) / Convert.ToInt64(rght);
                    }
                    throw new Exception("ЧЕРТИЛА НА 0 ДЕЛИШЬ");
                case TokenType.POWER:
                    if (lft is double || rght is double)
                        if (Convert.ToDouble(lft) < 0 && rght is double)
                            throw new Exception($"НЕЛЬЗЯ ПРИ ВОЗВЕДЕНИИ В СТЕПЕНЬ ОРИЦАТЕЛЬНОГО ЧИСЛА ИСПОЛЬЗОВАТЬ В СТЕПЕНИ НЕ ЦЕЛОЕ ЧИСЛО:\n{lft}/{Operation.Type}/{rght}/{Left}/{Operation}/{Right}");
                        else
                            return Math.Pow(Convert.ToDouble(lft), Convert.ToDouble(rght));
                    return Convert.ToInt64(Math.Pow(Convert.ToDouble(lft), Convert.ToDouble(rght)));
                case TokenType.MOD:
                    if (lft is double || rght is double)
                        return Convert.ToDouble(lft) % Convert.ToDouble(rght);
                    return Convert.ToInt64(lft) % Convert.ToInt64(rght);
                case TokenType.DIV:
                    if (lft is double || rght is double)
                        return Convert.ToDouble(lft) / Convert.ToDouble(rght);
                    return Convert.ToInt64(lft) / Convert.ToInt64(rght);
                default:
                    throw new Exception($"НЕПОДДЕРЖИВАЕМАЯ БИНАРНАЯ ОПЕРАЦИЯ: <{lft}> <{Operation.Type.GetStringValue()}> <{rght}> | <{Left}> <{Operation}> <{Right}>");
            }
        }

        public override string ToString() => $"{Left} {Operation.View} {Right}";
    }

    public sealed class CmpExpression : IExpression
    {
        public IExpression Left;
        public Token Comparation;
        public IExpression Right;

        public CmpExpression(IExpression left, Token comparation, IExpression right)
        {
            Left = left;
            Comparation = comparation;
            Right = right;
        }

        public IExpression Clon() => new CmpExpression(Left.Clon(), Comparation.Clone(), Right.Clon());

        public object Evaluated()
        {
            object olft = Left.Evaluated();
            object orght = Right.Evaluated();
            if (olft is string || orght is string) 
            {
                string slft = Convert.ToString(olft);
                string srght = Convert.ToString(orght);
                int slftl = slft.Length;
                int srghtl = srght.Length;
                switch (Comparation.Type)
                {
                    case TokenType.EQUALITY:
                        return slft == srght;
                    case TokenType.NOTEQUALITY:
                        return slft != srght;
                    case TokenType.LESS:
                        return slftl < srghtl;
                    case TokenType.LESSEQ:
                        return slftl <= srghtl;
                    case TokenType.MORE:
                        return slftl > srghtl;
                    case TokenType.MOREEQ:
                        return slftl >= srghtl;
                    case TokenType.AND:
                        return slftl > 0 && srghtl > 0;
                    case TokenType.OR:
                        return slftl > 0 || srghtl > 0;
                    default:
                        throw new Exception($"ТАК НЕЛЬЗЯ СРАВНИВАТЬ СТРОКИ: <{Left}> <{Comparation}> <{Right}>");
                }
            }
            if (!(olft is bool) && !(orght is bool))
            {
                double lft = Convert.ToDouble(olft);
                double rght = Convert.ToDouble(orght);
                switch (Comparation.Type)
                {
                    case TokenType.EQUALITY:
                        return lft == rght;
                    case TokenType.NOTEQUALITY:
                        return lft != rght;
                    case TokenType.LESS:
                        return lft < rght;
                    case TokenType.LESSEQ:
                        return lft <= rght;
                    case TokenType.MORE:
                        return lft > rght;
                    case TokenType.MOREEQ:
                        return lft >= rght;
                    case TokenType.AND:
                        return lft != 0 && rght != 0;
                    case TokenType.OR:
                        return lft != 0 || rght != 0;
                    default:
                        throw new Exception($"НЕСРАВНЕННЫЕ ЧИСЛА: <{lft}> <{Comparation.Type.GetStringValue()}> <{rght}> | <{Left}> <{Comparation}> <{Right}>");
                }
            }
            else if (olft is bool && orght is bool)
            {
                bool lft = Convert.ToBoolean(olft);
                bool rght = Convert.ToBoolean(orght);
                switch (Comparation.Type)
                {
                    case TokenType.EQUALITY:
                        return lft == rght;
                    case TokenType.NOTEQUALITY:
                        return lft != rght;
                    case TokenType.AND:
                        return lft && rght;
                    case TokenType.OR:
                        return lft || rght;
                    default:
                        throw new Exception("НЕСРАВНЕННЫЕ УСЛОВИЯ: <" + (lft ? "Истина" : "Ложь") + $"> <{Comparation.Type.GetStringValue()}> <" + (rght ? "Истина" : "Ложь") + $"> | <{Left}> <{Comparation}> <{Right}>");
                }
            }
            throw new Exception($"НЕЛЬЗЯ СРАВНИВАТЬ РАЗНЫЕ ТИПЫ: <{Left}> <{Comparation}> <{Right}>");
        }

        public override string ToString() => $"{Left} {Comparation.View} {Right}";
    }

    public sealed class ShortIfExpression : IExpression
    {
        IExpression Condition;
        IExpression Pravda;
        IExpression Nepravda;

        public ShortIfExpression(IExpression condition, IExpression pravda, IExpression nepravda)
        {
            Condition = condition;
            Pravda = pravda;
            Nepravda = nepravda;
        }

        public IExpression Clon() => new ShortIfExpression(Condition.Clon(), Pravda.Clon(), Nepravda.Clon());

        public object Evaluated() => Convert.ToBoolean(Condition.Evaluated()) ? Pravda.Evaluated() : Nepravda.Evaluated();

        public override string ToString() => $"({Condition} ? {Pravda} : {Nepravda})";
    }

    public sealed class VariableExpression : IExpression
    {
        public Token Name;

        public VariableExpression(Token varivable) => Name = varivable;

        public IExpression Clon() => new VariableExpression(Name.Clone());

        public object Evaluated() => Objects.GetVariable(Name.View).Evaluated();

        public override string ToString()
        {
            if (Objects.ContainsVariable(Name.View))
            {
                object value = Objects.GetVariable(Name.View).Evaluated();
                if (value is List<object>)
                    return $"{Name} ИМЕЕТ ЗНАЧЕНИЕ {PrintStatement.ListString((List<object>)value)}";
                return value.ToString();
            }
        //    throw new Exception("ДАННОЙ ПЕРЕМЕННОЙ ПОКА НЕТУ ????? ЭТО ОШИБКА В ВЫРАЖЕНИИ ПЕРЕМЕННОЙ");
            return $"{Objects.NOTHING} ИМЕЕТ ЗНАЧЕНИЕ {Objects.NOTHING.Evaluated()}";
        }
    }

    public sealed class IncDecBeforeExpression : IExpression, IStatement
    {
        public Token Operation;
        public Token Name;

        public IncDecBeforeExpression(Token operation, Token name)
        {
            Operation = operation;
            Name = name;
        }

        public IExpression Clon() => new IncDecBeforeExpression(Operation.Clone(), Name.Clone());

        public IStatement Clone() => new IncDecBeforeExpression(Operation.Clone(), Name.Clone());

        public object Evaluated()
        {
            string name = Name.View;
            object value = Objects.GetVariable(name).Evaluated();
            if (value is long || value is bool)
            {
                long temp = value is bool ? Convert.ToBoolean(value) ? 1 : 0 : Convert.ToInt64(value);
                switch (Operation.Type)
                {
                    case TokenType.PLUSPLUS:
                        Objects.AddVariable(name, new IClass(name, ++temp, new Dictionary<string, IClass>()));
                        return temp;
                    case TokenType.MINUSMINUS:
                        Objects.AddVariable(name, new IClass(name, --temp, new Dictionary<string, IClass>()));
                        return temp;
                    default:
                        throw new Exception("НЕВОЗМОЖНО");
                }
            }
            if (value is double)
            {
                double temp = Convert.ToDouble(value);
                switch (Operation.Type)
                {
                    case TokenType.PLUSPLUS:
                        Objects.AddVariable(name, new IClass(name, ++temp, new Dictionary<string, IClass>()));
                        return temp;
                    case TokenType.MINUSMINUS:
                        Objects.AddVariable(name, new IClass(name, --temp, new Dictionary<string, IClass>()));
                        return temp;
                    default:
                        throw new Exception("НЕВОЗМОЖНО");
                }
            }
            throw new Exception($"С ДАННЫМ ЗНАЧЕНИЕМ {value} ДАННОЕ ДЕЙСТВИЕ ({Operation.View}) НЕВОЗМОЖНО");
        }

        public void Execute()
        {
            string name = Name.View;
            object value = Objects.GetVariable(name).Evaluated();
            if (value is long || value is bool)
            {
                long temp = value is bool ? Convert.ToBoolean(value) ? 1 : 0 : Convert.ToInt64(value);
                switch (Operation.Type)
                {
                    case TokenType.PLUSPLUS:
                        Objects.AddVariable(name, new IClass(name, ++temp, new Dictionary<string, IClass>()));
                        return;
                    case TokenType.MINUSMINUS:
                        Objects.AddVariable(name, new IClass(name, --temp, new Dictionary<string, IClass>()));
                        return;
                    default:
                        throw new Exception("НЕВОЗМОЖНО");
                }
            }
            if (value is double)
            {
                double temp = Convert.ToDouble(value);
                switch (Operation.Type)
                {
                    case TokenType.PLUSPLUS:
                        Objects.AddVariable(name, new IClass(name, ++temp, new Dictionary<string, IClass>()));
                        return;
                    case TokenType.MINUSMINUS:
                        Objects.AddVariable(name, new IClass(name, --temp, new Dictionary<string, IClass>()));
                        return;
                    default:
                        throw new Exception("НЕВОЗМОЖНО");
                }
            }
            throw new Exception($"С ДАННЫМ ЗНАЧЕНИЕМ {value} ДАННОЕ ДЕЙСТВИЕ ({Operation.View}) НЕВОЗМОЖНО");
        }

        public override string ToString() => '<' + Operation.ToString() + Name + '>';
    }

    public sealed class FunctionExpression : IExpression 
    {
        public Token Name;
        public List<IExpression> Args;

        public FunctionExpression(Token name)
        {
            Name = name;
            Args = new List<IExpression>();
        }

        public FunctionExpression(Token name, List<IExpression> args) 
        {
            Name = name;
            Args = args;
        }

        public IExpression Clon() => new FunctionExpression(Name.Clone(), Args.Select(a => a.Clon()).ToList());

        public void AddArg(IExpression arg) => Args.Add(arg);

        public object Evaluated()
        {
            int argov = Args.Count;
            object[] args = new object[argov];
            for (int i = 0; i < argov; i++)
                args[i] = Args[i].Evaluated();
            if (Objects.ContainsVariable(Name.View))
            {
                IClass function = Objects.GetVariable(Name.View);
                if (function.Body is UserFunction)
                {
                    UserFunction userFunction = function.Body as UserFunction;
                    if (argov != userFunction.ArgsCount())
                        throw new Exception($"НЕВЕРНОЕ КОЛИЧЕСТВО АРГУМЕНТОВ: БЫЛО<{argov}> ОЖИДАЛОСЬ<{userFunction.ArgsCount()}>");
                    Objects.Push();
                    for (int i = 0; i < argov; i++)
                        Objects.AddVariable(userFunction.GetArgName(i), new IClass(userFunction.GetArgName(i), args[i]));
                    object result = userFunction.Execute();
                    Objects.Pop();
                    return result;
                }
                if (!(function is null))
                    return function.Execute(args);
                throw new Exception($"НЕСУЩЕСТВУЮЩАЯ ФУНКЦИЯ ХОТЯ БЫ СЕЙЧАС: <{Name.View}>");
            }
            else
            {
                throw new Exception($"НЕСУЩЕСТВУЮЩАЯ ФУНКЦИЯ ХОТЯ БЫ СЕЙЧАС: <{Name.View}>");
                throw new NotImplementedException("СДЕЛАЙ МЕТОД а вообще это не реально");
            }
        }

        public override string ToString() => $"ФУНКЦИЯ {Name.View}({string.Join(", ", Args.Select(a => a.ToString()))})";
    }

    public sealed class NowExpression : IExpression
    {
        public double Time;

        public object Evaluated() => (double)DateTime.Now.Ticks / 10000;

        public IExpression Clon() => new NowExpression();

        public override string ToString() => $"СЕЙЧАС<{Time}>";
    }

    public sealed class ListTakeExpression : IExpression
    {
        public Token Arr;
        public IExpression From;
        public IExpression To = null;

        public ListTakeExpression(Token arr, IExpression from, IExpression to)
        {
            Arr = arr;
            From = from;
            To = to;
        }

        public IExpression Clon() => new ListTakeExpression(Arr.Clone(), From.Clon(), To is null ? null : To.Clon());

        public string SliceString(string Slice)
        {
            try
            {
                int from = Convert.ToInt32(From.Evaluated());
                int to = 0;
                if (To != null)
                    to = Convert.ToInt32(To.Evaluated());
                int length = Slice.Length;
                if (from < 0)
                    from = length + from + 1;
                if (To != null)
                {
                    if (to < 0)
                        to = length + to + 1;
                    return Slice.Substring(from, to - from);
                }
                return Slice[from] + "";
            }
            catch (Exception)
            {
                int from = Convert.ToInt32(From.Evaluated());
                int to = Convert.ToInt32(To.Evaluated());
                throw new Exception($"НЕКОРРЕКТНЫЕ ИНДЕКСЫ: ОТ <{from}> ДО <{to}> С ДЛИНОЙ <{to - from}>");
            }
        }

        public object Evaluated()
        {
            if (Arr.Type == TokenType.STRING)
                return SliceString(Arr.View);

            object sliced = Objects.GetVariable(Arr.View);
            if (sliced is string)
                return SliceString(Convert.ToString(sliced));

            int from = Convert.ToInt32(From.Evaluated());
            int to = 0;
            if (To != null)
                to = Convert.ToInt32(To.Evaluated());
            List<object> arr = (List<object>)sliced;

            int length = arr.Count;
            if (from < 0)
                from = length + from + 1;
            if (To != null)
            {
                if (to < 0)
                    to = length + to + 1;
                return arr.Skip(from).Take(to - from).ToList();
            }
            return arr[from];
        }

        public override string ToString()
        {
            int from = Convert.ToInt32(From.Evaluated());
            int to = Convert.ToInt32(To.Evaluated());
            return $"{Arr.View}[{from}" + to??"" + "]";
        }
    }

    public sealed class ListExpression : IExpression
    {
        public List<IExpression> Items;

        public ListExpression(List<IExpression> items) => Items = items;

        public IExpression Clon() => new ListExpression(Items.Select(i => i.Clon()).ToList());

        public object Evaluated()
        {
            List<object> items = new List<object>(Items.Count);
            foreach (IExpression expression in Items)
                items.Add(expression.Evaluated());
            return items;
        }

        public override string ToString() => $"СПИСОК[{PrintStatement.ListString(Items.Select(i => (object)i).ToList())}]";
    }

    public sealed class NothingExpression : IExpression 
    {
        public IExpression Clon() => new NothingExpression();

        public object Evaluated() => (long)0;
        
        public override string ToString() => "НИЧЕГО"; 
    }

    public sealed class AttributeExpression : IExpression
    {
        //Token[] ObjectsStack;    ***********************************************************************************
        Token ObjectName;
        Token AttributeName;

        public AttributeExpression(Token objectName, Token attributeName)
        {
            ObjectName = objectName;
            AttributeName = attributeName;
        }

        public IExpression Clon() => new AttributeExpression(ObjectName.Clone(), AttributeName.Clone());

        public object Evaluated() => Objects.GetVariable(ObjectName.View).GetAttribute(AttributeName.View);

        public override string ToString() => $"{ObjectName}.{AttributeName}";
    }

    public sealed class NewObjectExpression : IExpression
    {
        public Token ClassName;
        public IStatement[] Assignments;

        public NewObjectExpression(Token className, IStatement[] assigns)
        {
            ClassName = className;
            Assignments = assigns;
        }

        public IExpression Clon() => new NewObjectExpression(ClassName.Clone(), Assignments.Select(a => a.Clone()).ToArray());

        public object Evaluated()
        {
            IClass classObject = Objects.GetClass(ClassName.View).Clone();
            foreach (IStatement assignment in Assignments)
            {
                if (assignment is AssignStatement)
                {
                    AssignStatement assign = assignment as AssignStatement;
                    object result = assign.Expression.Evaluated();
                    if (result is IClass)
                        classObject.AddAttribute(assign.Variable.View, (IClass)result);
                    else
                        classObject.AddAttribute(assign.Variable.View, new IClass(assign.Variable.View, result, new Dictionary<string, IClass>()));
                    continue;
                }
                if (assignment is DeclareFunctionStatement)
                {
                    DeclareFunctionStatement method = assignment as DeclareFunctionStatement;
                    Objects.Push();
                    method.Execute();
                    IFunction function = Objects.GetVariable(method.Name.View).Clone();
                    Objects.Pop();
                    classObject.AddAttribute(method.Name.View, new IClass(method.Name.View, IClass.HOLLOW, new Dictionary<string, IClass>(), function));
                    continue;
                }
                assignment.Execute();
            }
            return classObject;
        }

        public override string ToString() => $"НОВЫЙ {ClassName}({PrintStatement.ListString(Assignments.Select(a => (object)a.ToString()).ToList())})";
    }
}
