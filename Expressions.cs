using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Policy;

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

            if (value is IClass)
            {
                Console.WriteLine("Я НЕ УВЕРЕН ВООБЩЕ ЧТО ЭТО НЕ БУДЕТ ИМЕТЬ НЕ НУДНЫХ ОШИБОК НО ЭТО ТАК ЧТО БЫ БЫЛО ДА");
                IClass valueObject = value as IClass;
                string method;
                switch (Operation.Type)
                {
                    case TokenType.PLUS:
                        method = "_плюс";
                        break;
                    case TokenType.MINUS:
                        method = "_минус";
                        break;
                    case TokenType.NOT:
                        method = "_не";
                        break;
                    default:
                        throw new Exception($"НЕПОДДЕРЖИВАЕМАЯ БИНАРНАЯ ОПЕРАЦИЯ ДЛЯ ДАННЫХ ОБЪЕКТОВ: <{valueObject}> <{Operation.Type}> | <{Value}> <{Operation}>");
                }
                object got;
                if (valueObject.ContainsAttribute(method))
                    got = valueObject.GetAttribute(method);
                else
                    throw new Exception($"В ОБЪЕКТЕ <{valueObject}> НЕТУ МЕТОДА <{method}>");
                if (got is IClass)
                {
                    IClass meth = got as IClass;
                    if (meth.Body is UserFunction)
                    {
                        UserFunction userF = meth.Body as UserFunction;
                        if (userF.ArgsCount() < 1)
                            throw new Exception($"ДЛЯ ДАННОГО МАГИЧАСКОГО МЕТОДА <УМНОЖЕНИЕ> НУЖЕН ХОТЯ БЫ ОДИН АРГУМЕНТ");
                        Objects.Push();
                        // attrs
                        foreach (var attribute in valueObject.Attributes)
                            Objects.AddVariable(attribute.Key, attribute.Value);
                        // execute
                        object result = userF.Execute();
                        // restore
                        foreach (var variable in Objects.Variables)
                            if (valueObject.ContainsAttribute(variable.Key))
                                valueObject.AddAttribute(variable.Key, variable.Value);
                        Objects.Pop();
                        return result;
                    }
                    throw new Exception("СОННА ВАКЕ НАЙ");
                    // return meth.Execute(new object[] { rght });
                }
                throw new Exception($"МЕТОД <{valueObject}> ОКАЗАЛСЯ НЕ МЕТОДОМ А <{got}>");
            }
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
                    if (value is string)
                        return !(Convert.ToString(value).Length != 0);
                    if (value is long || value is double)
                        return !(Convert.ToDouble(value) != 0);
                    if (value is List<object>)
                        return !(((List<object>)value).Count != 0);
                    
                    throw new Exception($"НЕВОЗМОЖНЫЙ ОБЪЕКТ <{value}> ДЛЯ ОПЕРАЦИИ <{Operation.Type}>");
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"<{Value.Evaluated()}> <{Value}> <{Operation}> <{Operation.View}>");
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
            if (lft is IClass || rght is IClass)
            {
                if (!(lft is IClass) || !(rght is IClass))
                    throw new Exception($"ДВОИЧНЫЕ ДЕЙСТВИЯ МОЖНО ДЕЛАТЬ ТОЛЬКО МЕЖДУ ДВУМЯ ОБЪЕКТАМИ А НЕ ОБЪЕКТОМ И ЧЕМ ЛИБО ЕЩЕ\n{lft}\n{rght}");
                IClass leftObject = lft as IClass;
                string method;
                switch (Operation.Type)
                {
                    case TokenType.PLUS:
                        method = "_плюс";
                        break;
                    case TokenType.MINUS:
                        method = "_минус";
                        break;
                    case TokenType.MULTIPLICATION:
                        method = "_умножить";
                        break;
                    case TokenType.DIVISION:
                        method = "_делить";
                        break;
                    case TokenType.POWER:
                        method = "_степень";
                        break;
                    case TokenType.MOD:
                        method = "_остаток";
                        break;
                    case TokenType.DIV:
                        method = "_безостаточный";
                        break;
                    default:
                        throw new Exception($"НЕПОДДЕРЖИВАЕМАЯ БИНАРНАЯ ОПЕРАЦИЯ ДЛЯ ДАННЫХ ОБЪЕКТОВ: {lft} {Operation.Type} {rght} | {Left} {Operation} {Right}");
                }
                object got;
                if (leftObject.ContainsAttribute(method))
                    got = leftObject.GetAttribute(method);
                else
                    throw new Exception($"В ОБЪЕКТЕ <{Left}> НЕТУ МЕТОДА <{method}>");
                if (got is IClass)
                {
                    IClass meth = got as IClass;
                    if (meth.Body is UserFunction)
                    { 
                        UserFunction userF = meth.Body as UserFunction;
                        if (userF.ArgsCount() < 1)
                            throw new Exception($"ДЛЯ ДАННОГО МАГИЧАСКОГО МЕТОДА <УМНОЖЕНИЕ> НУЖЕН ХОТЯ БЫ ОДИН АРГУМЕНТ");
                        Objects.Push();
                        // attrs
                        foreach (var attribute in leftObject.Attributes)
                            Objects.AddVariable(attribute.Key, attribute.Value);
                        // arg
                        Objects.AddVariable(userF.Args[0].View, rght);
                        // execute
                        object result = userF.Execute();
                        // restore
                        foreach (var variable in Objects.Variables)
                            if (leftObject.ContainsAttribute(variable.Key))
                                leftObject.AddAttribute(variable.Key, variable.Value);
                        Objects.Pop();
                        return result;
                    }
                    throw new Exception("СОННА ВАКЕ НАЙ");
                    // return meth.Execute(new object[] { rght });
                }
                throw new Exception($"МЕТОД <{Left}> ОКАЗАЛСЯ НЕ МЕТОДОМ А <{got}>");
            }
            if (lft is string && rght is long)
            {
                string str = Convert.ToString(lft);
                switch (Operation.Type)
                {
                    case TokenType.MULTIPLICATION:
                        string ret = "";
                        for (long i = 0; i < (long)rght; i++)
                            ret += str;
                        return ret;
                    case TokenType.PLUS:
                        return (string)lft + (long)rght;
                    default:
                        throw new Exception($"НЕПОДДЕРЖИВАЕМАЯЯ ОПЕРАЦИЯ <{Operation}> МЕЖДУ <{lft}> И <{rght}>");
                }
            }
            if (lft is long && rght is string)
            {
                string str = Convert.ToString(rght);
                switch (Operation.Type)
                {
                    case TokenType.MULTIPLICATION:
                        string ret = "";
                        for (long i = 0; i < (long)lft; i++)
                            ret += str;
                        return ret;
                    case TokenType.PLUS:
                        return (long)lft + (string)rght;
                    default:
                        throw new Exception($"НЕПОДДЕРЖИВАЕМАЯЯ ОПЕРАЦИЯ <{Operation}> МЕЖДУ <{lft}> И <{rght}>");
                }
            }
            if (lft is string && rght is string)
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

        public object Evaluated() => Objects.GetVariable(Name.View);

        public override string ToString()
        {
            if (Objects.ContainsVariable(Name.View))
            {
                object value = Objects.GetVariable(Name.View);
                if (value is List<object>)
                    return $"{Name} ИМЕЕТ ЗНАЧЕНИЕ {PrintStatement.ListString((List<object>)value)}";
                return Convert.ToString(value);
            }
            throw new Exception("ДАННОЙ ПЕРЕМЕННОЙ ПОКА НЕТУ ????? ЭТО ОШИБКА В ВЫРАЖЕНИИ ПЕРЕМЕННОЙ");
          //  return $"{Objects.NOTHING} ИМЕЕТ ЗНАЧЕНИЕ {Objects.NOTHING}";
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
            object value = Objects.GetVariable(name);
            if (value is long || value is bool)
            {
                long temp = value is bool ? Convert.ToBoolean(value) ? 1 : 0 : Convert.ToInt64(value);
                switch (Operation.Type)
                {
                    case TokenType.PLUSPLUS:
                        Objects.AddVariable(name, ++temp);
                        return temp;
                    case TokenType.MINUSMINUS:
                        Objects.AddVariable(name, --temp);
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
                        Objects.AddVariable(name, ++temp);
                        return temp;
                    case TokenType.MINUSMINUS:
                        Objects.AddVariable(name, --temp);
                        return temp;
                    default:
                        throw new Exception("НЕВОЗМОЖНО");
                }
            }
            throw new Exception($"С ДАННЫМ ЗНАЧЕНИЕМ {value} ДАННОЕ ДЕЙСТВИЕ ({Operation.View}) НЕВОЗМОЖНО");
        }

        public void Execute() => Evaluated();

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
                IClass function = null;
                if (Objects.GetVariable(Name.View) is IClass)
                    function = Objects.GetVariable(Name.View) as IClass;
                else
                    throw new Exception($"ДАННЫЙ ОБЬЕКТ НЕ ЯВЛЯЕТСЯ ФУНКЦИЕЙ <{Name.View}>");
                if (function.Body is UserFunction)
                {
                    UserFunction userFunction = function.Body as UserFunction;
                    if (argov != userFunction.ArgsCount())
                        throw new Exception($"НЕВЕРНОЕ КОЛИЧЕСТВО АРГУМЕНТОВ: БЫЛО<{argov}> ОЖИДАЛОСЬ<{userFunction.ArgsCount()}>");
                    Objects.Push();
                    for (int i = 0; i < argov; i++)
                        Objects.AddVariable(userFunction.GetArgName(i), args[i]);
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
                throw new Exception($"НЕСУЩЕСТВУЮЩАЯ ФУНКЦИЯ ХОТЯ БЫ СЕЙЧАС: <{Name.View}>\n{this}");
               // throw new NotImplementedException("СДЕЛАЙ МЕТОД а вообще это не реально");
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

    public sealed class ListExpression : IExpression
    {
        public List<IExpression> Items;

        public ListExpression(List<IExpression> items) => Items = items;

        public IExpression Clon() => new ListExpression(Items.Select(i => i.Clon()).ToList());

        public object Evaluated() => Items.Select(i => i.Evaluated()).ToList();

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
        IExpression Value;
        Token AttributeName;

        public AttributeExpression(IExpression value, Token attributeName)
        {
            Value = value;
            AttributeName = attributeName;
        }

        public IExpression Clon() => new AttributeExpression(Value.Clon(), AttributeName.Clone());

        public object Evaluated()
        {
            object value = Value.Evaluated();
            if (value is IClass)
            {
                IClass classObject = value as IClass;
                object attribute = classObject.GetAttribute(AttributeName.View);
                return attribute;
            }
            
            if (value is long)
            {
                IClass IInt = Objects.IInteger.Clone();
                return IInt.GetAttribute(AttributeName.View);
            }
            if (value is string)
            {
                IClass IStr = Objects.IString.Clone();
                return IStr.GetAttribute(AttributeName.View);
            }
            if (value is double)
            {
                IClass IFlt = Objects.IFloat.Clone();
                return IFlt.GetAttribute(AttributeName.View);
            }
            if (value is bool)
            {
                IClass IBol = Objects.IBool.Clone();
                return IBol.GetAttribute(AttributeName.View);
            }
            if (value is List<object>)
            {
                IClass ILst = Objects.IList.Clone();
                return ILst.GetAttribute(AttributeName.View);
            }
            throw new Exception($"КАК ПОЧСИТАЛ ЭТО ЭТИМ: <{value}>");
        }

        public override string ToString() => $"{Value}.{AttributeName}";
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
            IClass classObject;
            object got = Objects.GetVariable(ClassName.View);
            if (got is IClass)
                classObject = ((IClass)got).Clone();
            else
                throw new Exception($"ПЕРЕМЕННАЯ <{got} НЕ ЯВЛЯЕТСЯ КЛАССОМ ИЛИ ОБЪЕКТОМ>");

            foreach (IStatement assignment in Assignments)
            {
                if (assignment is AssignStatement)
                {
                    AssignStatement assign = assignment as AssignStatement;
                    object result = assign.Expression.Evaluated();
                    classObject.AddAttribute(assign.Variable.View, result);
                    continue;
                }
                if (assignment is DeclareFunctionStatement)
                {
                    DeclareFunctionStatement method = assignment as DeclareFunctionStatement;
                    method.Execute();
                    UserFunction function = ((IClass)Objects.GetVariable(method.Name.View)).Clone().Body as UserFunction;
                    Objects.DeleteVariable(method.Name.View);
                    classObject.AddAttribute(method.Name.View, new IClass(method.Name.View, new Dictionary<string, object>(), function));
                    continue;
                }
                assignment.Execute();
            }
            return classObject;
        }

        public override string ToString() => $"НОВЫЙ {ClassName}({PrintStatement.ListString(Assignments.Select(a => (object)a.ToString()).ToList())})";
    }

    public sealed class LambdaExpression : IExpression
    {
        Token[] Args;
        IExpression Ret;

        public LambdaExpression(Token[] args, IExpression ret)
        {
            Args = args;
            Ret = ret;
        }

        public IExpression Clon() => new LambdaExpression(Args.Select(a => a.Clone()).ToArray(), Ret.Clon());

        public object Evaluated() => new IClass("лямбдой_был_создан", new Dictionary<string, object>(), new UserFunction(Args, new ReturnStatement(Ret)));

        public override string ToString() => $"ЛЯМБДА {PrintStatement.ListString(Args.Select(a => (object)a).ToList())}: {Ret}";
    }

    public sealed class SliceExpression : IExpression 
    {
        IExpression Taken;
        IExpression From;
        IExpression To;
        IExpression Step;

        public SliceExpression(IExpression taken, IExpression from = null, IExpression to = null, IExpression step = null)
        {
            Taken = taken;
            From = from;
            To = to;
            Step = step;
        }

        public IExpression Clon() => new SliceExpression(Taken.Clon(),
                                                         From.Clon() is null ? null : From.Clon(), 
                                                         To is null ? null : To.Clon(),
                                                         Step is null ? null : Step.Clon());

        public static int Len(object taken) => taken is string? Convert.ToString(taken).Length : ((List<object>)taken).Count;

        public static int[] Sliced(object slice, int from, int to, object To)
        {
            int length = Len(slice);
            try
            {
                if (from < 0)
                    from = length + from + 1;
                if (To is null)
                    to = length;
                if (to != from)
                {
                    if (to < 0)
                        to = length + to;
                    return Enumerable.Range(from, to - from).ToArray();
                }
                return new int[1] { from };
            }
            catch (Exception)
            {
                throw new Exception($"НЕКОРРЕКТНЫЕ ИНДЕКСЫ: ОТ <{from}> ДО <{to}> С ДЛИНОЙ <{to - from}> КОГДА У ОБЪЕКТА <" + (slice is List<object> ? PrintStatement.ListString((List<object>)slice) : slice) + $"> ДЛИНА <{length}>");
            }
        }

        public static int[] SelectStepped(List<int> indeces, int step)
        {
            if (step < 0)
                indeces.Reverse();
            List<int> newArr = new List<int>();
            for (int i = 0; i < indeces.Count; i++)
                if (i % step == 0)
                    newArr.Add(indeces[i]);
            return newArr.ToArray();
        }

        public static int DetermineIndex(IExpression index, int common = 0)
        {
            object got = index is null ? null : index.Evaluated();
            if (!(got is null))
                if (got is long)
                    if (Convert.ToInt64(got) > int.MaxValue || Convert.ToInt64(got) < int.MinValue)
                        throw new Exception($"ЧИСЛО <{got}> БЫЛО СЛИШКОМ БОЛЬШИМ ИЛИ МАЛЕНЬКИМ ДЛЯ ИНДЕКСА");
                    else
                        return Convert.ToInt32(got);
                else
                    throw new Exception($"ЧИСЛО <{got} ВОВСЕ И НЕ ЧИСЛО");
            return common;
        }

        public static List<object> Obj2List(object taken) => taken is string ?
                                          Convert.ToString(taken).ToCharArray().Select(c => (object)Convert.ToString(c)).ToList() :
                                          (List<object>)taken;

        public object Evaluated()
        {
            if (!(To is null) && To.Evaluated() is string)
            {
                object took = Taken.Evaluated();
                if (took is List<object>)
                    return ((List<object>)took)[DetermineIndex(From)];
                else
                    return Convert.ToString((Convert.ToString(took))[DetermineIndex(From)]);
            }

            object taken = Taken.Evaluated();
            int length = Len(taken);

            int from = DetermineIndex(From);
            int to = To is null ? DetermineIndex(To, length) : To.Evaluated() is string ? from + 1 : DetermineIndex(To);
            int step = DetermineIndex(Step, 1);

            if (from == to)
                if (taken is List<object>)
                    return new List<object>();
                else
                    return "";

            int[] indeces = taken is string || taken is long || taken is double ? Sliced(Convert.ToString(taken), from, to, To) :
                            taken is List<object> ? Sliced(taken, from, to, To) :
                            throw new Exception($"<{Taken}> НЕ БЫЛ ЛИСТОМ ИЛИ СТРОКОЙ, А <{taken}>");
            indeces = SelectStepped(indeces.ToList(), step);

            if (indeces.Length == 1)
                taken = taken is string ? ((string)taken)[indeces[0]] : taken is List<object> ? taken : new List<object>() { taken };
            
            List<object> beforeStep = Obj2List(taken);
            //Console.WriteLine("indeces " + PrintStatement.ListString(indeces.Select(i => (object)i).ToList()));
            //Console.WriteLine("items   " + PrintStatement.ListString(beforeStep.Select(i => (object)i).ToList()));
            List<object> newArr = new List<object>();
            foreach (int index in indeces)
                newArr.Add(beforeStep[index]);

            return newArr.All(b => b is string) ? (object)string.Join("", newArr) : newArr;
        }

        public override string ToString() => $"{Taken}[{From}:" + To ?? "" + ":" + Step ?? "" + "]";
    }

    public sealed class RangeExpression : IExpression
    {
        IExpression From;
        IExpression Till;
        IExpression Step;

        public RangeExpression(IExpression from, IExpression till, IExpression step = null)
        {
            From = from;
            Till = till;
            Step = step;
        }

        public IExpression Clon() => new RangeExpression(From.Clon(), Till.Clon(), Step is null ? null : Step.Clon());

        public object Evaluated()
        {
            object from = From.Evaluated();
            object till = Till.Evaluated();
            int step = 1;
            if (!(Step is null))
            {
                object got = Step.Evaluated();
                if (got is long || got is double)
                    step = Convert.ToInt32(got);
                else
                    throw new Exception($"ШАГ БЫЛ НЕ ЧИСЛОМ, А: <{got}>");
            }

            if (from is long || from is double && till is long || till is double)
            {
                int ot = Convert.ToInt32(from);
                int to = Convert.ToInt32(till);

                List<int> ret = null;
                if (ot > to)
                {
                    ot += to;
                    to = ot - to;
                    ot -= to;
                    ret = Enumerable.Range(ot, to - ot + 1).Reverse().ToList();
                }
                else
                    ret = Enumerable.Range(ot,  to - ot + 1).ToList();
                return SliceExpression.SelectStepped(ret, step).Select(r => (object)Convert.ToInt64(r)).ToList();
            }
            if (from is string && till is string)
            {
                string fromed = from as string;
                string tilled = till as string;
                if (fromed.Length == 0 || tilled.Length == 0)
                    throw new Exception($"СТРОКИ БЫЛИ НЕДОСТАТОЧНЙ ДЛИНЫ В: ОТ <{fromed}> ДО <{tilled}>");

                int ot = fromed[0];
                int to = tilled[0];
                List<int> ret = null;
                if (ot > to)
                {
                    ot += to;
                    to = ot - to;
                    ot -= to;
                    ret = Enumerable.Range(ot, to - ot + 1).Reverse().ToList();
                }
                else
                    ret = Enumerable.Range(ot, to - ot + 1).ToList();
                return string.Join("", SliceExpression.SelectStepped(ret, step).Select(s => (char)s));
            }
            throw new Exception($"ЗНАЧЕНИЯ <{from}> И <{till}> ИЗ <{From}> И <{Till}> ОКАЗАЛИСЬ НЕ ЧИСЛАМИ ИЛИ СТРОКАМИ");
        }

        public override string ToString() => $"ОТ {From} ДО {Till}";
    }
}
