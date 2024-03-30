using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace VovaScript
{
    public sealed class UserFunction : IFunction
    {
        public Token[] Args;
        public IStatement Body;

        public UserFunction(Token[] args, IStatement body)
        {
            Args = args;
            Body = body;
        }

        public int ArgsCount() => Args.Length;

        public string GetArgName(int i) => i < 0 || i >= ArgsCount() ? "" : Args[i].View;

        public object Execute(params object[] args)
        {
            try
            {
                Body.Execute();
            }
            catch (ReturnStatement result)
            {
                return result.GetResult();
            }
            return (long)0;
        }

        public IFunction Cloned()
        {
            Token[] args = new Token[ArgsCount()];
            Array.Copy(Args, args, ArgsCount());
            IFunction clone = new UserFunction(args, Body.Clone());
            return clone;
        }

        public override string ToString() => $"({string.Join(", ", Args.Select(a => a.View))}){{{Body}}}";
    }

    public sealed class MethodExpression : IExpression
    {
        public Token ObjectName;
        public Token MethodName;
        public IExpression Borrow;
        public IExpression Pool;

        public MethodExpression(IExpression pool, Token methodName, IExpression borrow)
        {
            Pool = pool;
            MethodName = methodName;
            Borrow = borrow;
        }

        public IExpression Clon() => new MethodExpression(Pool.Clon(), MethodName.Clone(), Borrow.Clon());

        public object Evaluated()
        {
            object got = Pool.Evaluated();
            FunctionExpression borrow = Borrow as FunctionExpression;
            if (got is IClass)
            {
                IClass classObject = got as IClass;
                // proceed
                got = classObject.GetAttribute(MethodName.View);
                UserFunction method = null;
                if (got is IClass)
                    method = ((IClass)got).Body as UserFunction;
                else
                {
                    Console.WriteLine("ДА ПОЧЕМУ ОНО СЮДА ВЕДЕТ");
                    throw new Exception($"НЕ ЯВЛЯЕТСЯ МЕТОДОМ: <{got}> С ИМЕНЕМ <{MethodName.View}>");
                }

                object[] args = borrow.Args.Select(a => a.Evaluated()).ToArray();
                if (args.Length < method.ArgsCount())
                    throw new Exception($"НЕВЕРНОЕ КОЛИЧЕСТВО АРГУМЕНТОВ: БЫЛО<{args.Length}> ОЖИДАЛОСЬ<{method.ArgsCount()}>");
                Objects.Push();
                // attrs
                foreach (var attribute in classObject.Attributes)
                    Objects.AddVariable(attribute.Key, attribute.Value);
                // args
                for (int i = 0; i < method.ArgsCount(); i++)
                {
                    string arg = method.GetArgName(i);
                    Objects.AddVariable(arg, args[i]);
                }
                // execute
                object result = method.Execute();
                // restore or update
                foreach (var variable in Objects.Variables)
                    if (classObject.ContainsAttribute(variable.Key))
                        classObject.AddAttribute(variable.Key, variable.Value);
                Objects.Pop();
                return result;
            }
            object value = got;

            if (value is long)
            {
                IClass IInt = Objects.IInteger.Clone();
                got = IInt.GetAttribute(MethodName.View);
            }
            else if (value is string)
            {
                IClass IStr = Objects.IString.Clone();
                got = IStr.GetAttribute(MethodName.View);
            }
            else if (value is double)
            {
                IClass IFlt = Objects.IFloat.Clone();
                got = IFlt.GetAttribute(MethodName.View);
            }
            else if (value is bool)
            {
                IClass IBol = Objects.IBool.Clone();
                got = IBol.GetAttribute(MethodName.View);
            }
            else if (value is List<object>)
            {
                IClass ILst = Objects.IList.Clone();
                got = ILst.GetAttribute(MethodName.View);
            }
            else
                throw new Exception($"НЕ ЯВЛЯЕТСЯ МЕТОДОМ: <{got}> С ИМЕНЕМ <{MethodName.View}>");

            if (got is IClass)
            {
                IClass meth = got as IClass;
                while (meth.Body is IClass)
                    meth = meth.Body as IClass;
                if (meth.Body is UserFunction)
                {
                    UserFunction userF = meth.Body as UserFunction;
                    Objects.Push();
                    List<object> args = borrow.Args.Select(a => a.Evaluated()).ToList();
                    args.Insert(0, value);

                    if (args.Count < userF.ArgsCount())
                        throw new Exception($"НЕВЕРНОЕ КОЛИЧЕСТВО АРГУМЕНТОВ: БЫЛО<{args.Count}> ОЖИДАЛОСЬ<{userF.ArgsCount()}>");

                    for (int i = 0; i < userF.ArgsCount(); i++)
                    {
                        string arg = userF.GetArgName(i);
                        Objects.AddVariable(arg, args[i]);
                    }
                    object result = userF.Execute();
                    Objects.Pop();
                    return result;
                }
                List<object> arges = new List<object> { value };
                arges.AddRange(borrow.Args.Select(a => a.Evaluated()).ToList());
                return meth.Execute(arges.ToArray());
            }
            throw new Exception($"МЕТОД <{MethodName}> ОКАЗАЛСЯ НЕ МЕТОДОМ А <{got}>");
        }

        public override string ToString() => $"{ObjectName}.{Borrow}";
    }

    public sealed class Sinus : IFunction
    {
        public object Execute(object[] x) => x.Length == 0 ? throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ, БЫЛО: <{x.Length}>") 
                                                           : Math.Sin(Convert.ToDouble(x[0]));

        public IFunction Cloned() => new Sinus();

        public override string ToString() => $"СИНУС(<>)";
    }

    public sealed class Cosinus : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            return Math.Cos(Convert.ToDouble(x[0]));
        }

        public IFunction Cloned() => new Cosinus();

        public override string ToString() => $"КОСИНУС(<>)";
    }

    public sealed class Ceiling : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            return Math.Ceiling(Convert.ToDouble(x[0]));
        }

        public IFunction Cloned() => new Ceiling();

        public override string ToString() => $"ПОТОЛОК(<>)";
    }

    public sealed class Floor : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            return Math.Floor(Convert.ToDouble(x[0]));
        }

        public IFunction Cloned() => new Floor();

        public override string ToString() => $"ЗАЗЕМЬ(<>)";
    }

    public sealed class Tan : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            return Math.Tan(Convert.ToDouble(x[0]));
        }

        public IFunction Cloned() => new Tan();

        public override string ToString() => $"ТАНГЕНС(<>)";
    }

    public sealed class Max : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            return MoreMax(x);
            //throw new Exception($"С ДАННЫМИ ТИПАМИ ПЕРЕМЕННЫХ ДАННАЯ ФЕНКЦИЯ НЕВОЗМОЖНА: <{this}> <>");
        }

        public IFunction Cloned() => new Max();

        public static object MoreMax(object[] x)
        {
            int I = 0;
            if (x[0] is List<object>)
                x = ((List<object>)x[0]).ToArray();
            double max = Convert.ToDouble(x[I]);

            for (int i = 0; i < x.Length; i++)
            {
                double iterable;
                if (x[i] is string)
                    iterable = ((string)x[i]).Length;
                else if (x[i] is List<object>)
                    iterable = ((List<object>)x[i]).Count;
                else if (x[i] is bool)
                    iterable = (bool)x[i] ? 1 : 0;
                else
                    iterable = Convert.ToDouble(x[i]);
                if (max < iterable)
                {
                    max = iterable;
                    I = i;
                }
            }
            if (x[I] is double)
                return max;
            if (x[I] is long)
                return Convert.ToInt64(max);
            if (x[I] is List<object>)
                return Convert.ToInt64(((List<object>)x[I]).Count);
            if (x[I] is string || x[I] is bool)
                return x[I];
            throw new Exception($"ЭТОГО ПРОСТО НЕ МОЖЕТ БЫТЬ: <{x[I]}> <{TypePrint.Pyc(x[I])}>");
        }

        public override string ToString() => $"НАИБОЛЬШЕЕ(<>)";
    }

    public sealed class Min : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            return LessMax(x);
            //throw new Exception($"С ДАННЫМИ ТИПАМИ ПЕРЕМЕННЫХ ДАННАЯ ФЕНКЦИЯ НЕВОЗМОЖНА: <{this}> <>");
        }

        public IFunction Cloned() => new Min();

        private object LessMax(object[] x)
        {
            int I = 0;
            if (x[0] is List<object>)
                x = ((List<object>)x[0]).ToArray();
            double min = Convert.ToDouble(x[I]);

            for (int i = 0; i < x.Length; i++)
            {
                double iterable;
                if (x[i] is string)
                    iterable = ((string)x[i]).Length;
                else if (x[i] is List<object>)
                    iterable = ((List<object>)x[i]).Count;
                else if (x[i] is bool)
                    iterable = (bool)x[i] ? 1 : 0;
                else
                    iterable = Convert.ToDouble(x[i]);
                if (min > iterable)
                {
                    min = iterable;
                    I = i;
                }
            }
            if (x[I] is double)
                return min;
            if (x[I] is long)
                return Convert.ToInt64(min);
            if (x[I] is List<object>)
                return Convert.ToInt64(((List<object>)x[I]).Count);
            if (x[I] is string || x[I] is bool)
                return x[I];
            throw new Exception($"ЭТОГО ПРОСТО НЕ МОЖЕТ БЫТЬ: <{x[I]}> <{TypePrint.Pyc(x[I])}>");
        }

        public override string ToString() => $"НАИМЕНЬШЕЕ(<>)";
    }

    public sealed class Square : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            return Math.Sqrt(Convert.ToDouble(x[0]));
        }

        public IFunction Cloned() => new Square();

        public override string ToString() => $"КОРЕНЬ(<>)";
    }

    public sealed class ReadAllFileFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            string file = Convert.ToString(x[0]);

            if (x.Length >= 2 && Convert.ToString(x[1]) == "линии")
                try
                {
                    return File.ReadAllLines(file, System.Text.Encoding.UTF8).Select(s => (object)s).ToList();
                }
                catch (IOException)
                {
                    throw new Exception("НЕ ПОЛУЧИЛОСЬ ПРОЧИТАТЬ ФАЙЛ В: " + file);

                }
            else
                try
                {
                    return File.ReadAllText(file, System.Text.Encoding.UTF8);
                }
                catch (IOException)
                {
                    throw new Exception("НЕ ПОЛУЧИЛОСЬ ПРОЧИТАТЬ ФАЙЛ В: " + file);
                }
        }

        public IFunction Cloned() => new ReadAllFileFunction();

        public override string ToString() => $"ВЫЧИТАТЬ(<>)";
    }

    public sealed class SplitFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            string stroka = Convert.ToString(x[0]);

            if (stroka.Length == 1) 
                return stroka.Split('\n').Select(s => (object)s).ToList();
            else
            {
                string sep = Convert.ToString(x[1]);
                char separator = sep[0]; //(sep == "\\n") ? '\n' : (sep == "\\t") ? '\t' : sep[0];
                return stroka.Split(separator).Select(s => (object)s).ToList();
            }
        }

        public IFunction Cloned() => new SplitFunction();

        public override string ToString() => $"РАЗДЕЛ(<>)";
    }

    public sealed class WriteNotLn : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            Console.Write(string.Join(" ", x));
            return Objects.NOTHING;
        }

        public IFunction Cloned() => new WriteNotLn();

        public override string ToString() => $"ВВОД(<>)";
    }

    public sealed class InputFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length > 0)
            {
                string message = Convert.ToString(x[0]);
                Console.Write(message);
            }
            return Console.ReadLine();
        }

        public IFunction Cloned() => new InputFunction();

        public override string ToString() => $"ВВОД(<>)";
    }

    public sealed class StringingFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is string)
                return x[0];
            switch (x.Length)
            {
                case 0:
                    return "";
                case 1:
                    return x[0] is bool ? (bool)x[0] ? "Истина" : "Ложь"
                         : x[0] is List<object> ? (object)PrintStatement.ListString((List<object>)x[0])
                         : Convert.ToString(x[0]);
                default:
                    return x.Select(s => s is bool ? (bool)s ? (object)"Истина" : (object)"Ложь" 
                                       : s is List<object> ? (object)PrintStatement.ListString((List<object>)s) 
                                       : (object)Convert.ToString(s)).ToList();
            }
        }

        public IFunction Cloned() => new StringingFunction();

        public override string ToString() => "СТРОЧИТЬ(<>)";
    }

    public sealed class IntingFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is long)
                return x[0];
            try
            {
                switch (x.Length)
                {
                    case 1:
                        return x[0] is bool ? (bool)x[0] ? (object)1 : (object)0 : Int64.Parse(Convert.ToString(x[0]));
                    default:
                        return x.Select(s => s is string ? (object)Int64.Parse((string)s) 
                                           : s is bool ? (bool)s ? (object)1 : (object)0
                                           : (object)Int64.Parse(Convert.ToString(s))).ToList();
                }
            }
            catch (Exception) { throw new Exception($"КОНВЕРТАЦИЯ НЕ УДАЛАСЬ: <{x[0]}>"); }
        }

        public IFunction Cloned() => new IntingFunction();

        public override string ToString() => "ЧИСЛИТЬ(<>)";
    }

    public sealed class DoublingFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is double)
                return x[0];
            try
            {
                switch (x.Length)
                {
                    case 0:
                        return 0;
                    case 1:
                        return x[0] is bool ? (bool)x[0] ? 1 : 0 : Convert.ToDouble(x[0]);
                    default:
                        return x.Select(s => s is string ? (object)double.Parse((string)s)
                                           : s is bool ? (bool)s ? (object)(double)1 : (object)(double)0
                                           : (object)Convert.ToDouble(s)).ToList();
                }
            }
            catch (Exception) { throw new Exception($"КОНВЕРТАЦИЯ НЕ УДАЛАСЬ: <{x[0]}>"); }
        }

        public IFunction Cloned() => new DoublingFunction();

        public override string ToString() => "ТОЧИТЬ(<>)";
    }

    public sealed class WritingFileFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 3)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            string file = Convert.ToString(x[0]);
            string mode = Convert.ToString(x[1]);
            string data = Convert.ToString(x[2]);

            try
            {
                if (mode.ToLower() == "пере")
                    using (StreamWriter writer = new StreamWriter(file, false, System.Text.Encoding.UTF8))
                        writer.WriteLine(data); 
                else
                    using (StreamWriter writer = new StreamWriter(file, true, System.Text.Encoding.UTF8))
                        switch (mode.ToLower())
                        {
                            case "до":
                                writer.Write(data);
                                break;
                            case "линию":
                                writer.WriteLine(data);
                                break;
                            default:
                                throw new Exception($"НЕСУЩЕСТВУЮЩИЙ РЕЖИМ ЗАПИСИ <{mode}>: " + file);
                        }
            }
            catch (IOException)
            {
                throw new Exception("НЕ ПОЛУЧИЛОСЬ ЗАПИСАТЬ В ФАЙЛ: " + file);
            }

            return x;
        }

        public IFunction Cloned() => new WritingFileFunction();

        public override string ToString() => "ЛЕТОПИСИТЬ(<>)";
    }

    public sealed class LenghtFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is string)
                return ((string)x[0]).Length;
            if (x[0] is long || x[0] is double)
                return Convert.ToString(x[0]).Length;
            if (x[0] is bool)
                return (bool)x[0] ? 1 : 0;
            if (x[0] is List<object>)
                return ((List<object>)x[0]).Count;
            throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[0]}> ДЛЯ <{this}>");
        }

        public IFunction Cloned() => new LenghtFunction();

        public override string ToString() => "ДЛИНА(<>)";
    }

    public sealed class ASCIICodeFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is string)
                return Convert.ToInt64((int)(((string)x[0])[0]));
            if (x[0] is long || x[0] is double)
                return Convert.ToInt64((int)(Convert.ToString(x[0])[0]));
            if (x[0] is bool)
                return (bool)x[0] ? Convert.ToInt64(61) : Convert.ToInt64(61);
            throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[0]}> ДЛЯ <{this}>");
        }

        public IFunction Cloned() => new ASCIICodeFunction();

        public override string ToString() => "СИМВОЛОМ(<>)";
    }

    public sealed class FromASCIICodeFunction : IFunction
    {
        public static string GetFromASCII(object x)
        {
            if (x is bool)
                x = (bool)x ? Convert.ToInt64(1) : Convert.ToInt64(0);
            if (x is long || x is double)
                return Convert.ToString((char)Convert.ToInt64(x));
            if (x is string)
                return Convert.ToString(x);
            throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x}> ДЛЯ <{new FromASCIICodeFunction()}>");
        }

        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is List<object>)
                if (((List<object>)x[0]).All(i => i is bool || i is double || i is long))
                    return string.Join("", ((List<object>)x[0]).Select(i => GetFromASCII(i)));
            return GetFromASCII(x[0]);
        }

        public IFunction Cloned() => new FromASCIICodeFunction();

        public override string ToString() => "СИМВОЛОМ(<>)";
    }

    public sealed class IsUpperFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is string)
            {
                string took = Convert.ToString(x[0]);
                return took.All(got => char.IsUpper(got) || (got > 1039 && got < 1072 || got == 1025));
            }
            throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[0]}> ДЛЯ <{this}>");
        }

        public IFunction Cloned() => new IsUpperFunction();

        public override string ToString() => "ВЫСОК(<>)";
    }

    public sealed class IsLowerFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is string)
            {
                string took = Convert.ToString(x[0]);
                return took.All(got => char.IsLower(got) || (got > 1071 && got < 1104 || got == 1105));
            }
            throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[0]}> ДЛЯ <{this}>");
        }

        public IFunction Cloned() => new IsLowerFunction();

        public override string ToString() => "НИЗОК(<>)";
    }

    public sealed class ToUpperFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is string)
            {
                string took = Convert.ToString(x[0]);
                return string.Join("", took.Select(got => char.IsLower(got) ?
                                              char.ToUpper(got) :
                                          got > 1039 && got < 1072 ?
                                              (char)(got + 32) : 
                                          got == 1025 ? 'Ё': got));
            }
            throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[0]}> ДЛЯ <{this}>");
        }

        public IFunction Cloned() => new ToUpperFunction();

        public override string ToString() => "ВЫСОКИМ(<>)";
    }

    public sealed class ToLowerFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is string)
            {
                string took = Convert.ToString(x[0]);
                CultureInfo info = new CultureInfo("ru-RU");
                return string.Join("", took.Select(got => char.IsUpper(got) || got > 1071 && got < 1104 || got == 1105 ? char.ToLower(got, info) : got));
            }
            throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[0]}> ДЛЯ <{this}>");
        }

        public IFunction Cloned() => new ToLowerFunction();

        public override string ToString() => "НИЗКИМ(<>)";
    }

    public sealed class MapFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 2)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            object got = x[1];
            if (got is IClass)
            {
                IClass classObject = got as IClass;
                if (classObject.Body is null)
                    throw new Exception($"<{x[1]}> НЕ ЯВЛЯЛСЯ ОБЪЕКТОМ ФУНКЦИИ");
                if (classObject.Body is UserFunction)
                {
                    UserFunction lambda = classObject.Body as UserFunction;
                    int argov = lambda.ArgsCount();
                    if (argov < 1)
                        throw new Exception(
                            $"<{lambda}> ИМЕЛ НЕДОСТАТОЧНО АРГУМЕНТОВ\n" +
                            $"ВОЗМОЖНЫЙ ПОРЯДОК АРГУМЕНТОВ: элемент, индекс, лист");
                    bool index = lambda.ArgsCount() > 1;
                    bool array = lambda.ArgsCount() > 2;
                    bool wasString = x[0] is string;

                    List<object> listed = x[0] is string || x[0] is List<object> ? SliceExpression.Obj2List(x[0]) : 
                            throw new Exception($"<{x[0]}> НЕ БЫЛ ЛИСТОМ ИЛИ СТРОКОЙ, А <{x[0]}>");
                    List<object> list = new List<object>(listed);

                    Objects.Push();
                    for (int i = 0; i < list.Count; i++)
                    {
                        Objects.AddVariable(lambda.GetArgName(0), list[i]);
                        if (index)
                        {
                            Objects.AddVariable(lambda.GetArgName(1), Convert.ToInt64(i));
                            if (array)
                                Objects.AddVariable(lambda.GetArgName(2), list);
                        }
                        object result = lambda.Execute();
                        list[i] = result is bool && wasString ? (bool)result ? "Истина" : "Ложь" : result;
                    }
                    Objects.Pop();

                    return wasString ? (object)string.Join("", list) : list;
                }
                throw new Exception($"<{classObject}> НЕ ДОПУСТИМАЯ ФУНКЦИЯ ДЛЯ ИСПОЛЬЗОВАНИЯ В <{this}>");
            }
            throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[1]}> ДЛЯ <{this}>");
        }

        public IFunction Cloned() => new MapFunction();

        public override string ToString() => $"ПЕРЕБОР(<>)";
    }

    public sealed class FilterFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 2)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            object got = x[1];
            if (got is IClass)
            {
                IClass classObject = got as IClass;
                if (classObject.Body is null)
                    throw new Exception($"<{x[1]}> НЕ ЯВЛЯЛСЯ ОБЪЕКТОМ ФУНКЦИИ");
                if (classObject.Body is UserFunction)
                {
                    UserFunction lambda = classObject.Body as UserFunction;
                    int argov = lambda.ArgsCount();
                    if (argov < 1)
                        throw new Exception(
                            $"<{lambda}> ИМЕЛ НЕДОСТАТОЧНО АРГУМЕНТОВ\n" +
                            $"ВОЗМОЖНЫЙ ПОРЯДОК АРГУМЕНТОВ: элемент, индекс, лист");
                    bool index = lambda.ArgsCount() > 1;
                    bool array = lambda.ArgsCount() > 2;
                    bool wasString = x[0] is string;

                    List<object> listed = x[0] is string || x[0] is List<object> ? SliceExpression.Obj2List(x[0]) :
                            throw new Exception($"<{x[0]}> НЕ БЫЛ ЛИСТОМ ИЛИ СТРОКОЙ, А <{x[0]}>");
                    List<object> list = new List<object>();

                    Objects.Push();
                    for (int i = 0; i < listed.Count; i++)
                    {
                        Objects.AddVariable(lambda.GetArgName(0), listed[i]);
                        if (index)
                        {
                            Objects.AddVariable(lambda.GetArgName(1), Convert.ToInt64(i));
                            if (array)
                                Objects.AddVariable(lambda.GetArgName(2), listed);
                        }
                        object result = lambda.Execute();

                        bool getting;
                        if (result is bool)
                            getting = (bool)result;
                        else if (result is string)
                            getting = ((string)result).Length != 0;
                        else if (result is List<object>)
                            getting = ((List<object>)result).Count != 0;
                        else if (result is long)
                            getting = (long)result != 0;
                        else if (result is double)
                            getting = (double)result != 0;
                        else
                            getting = result is IClass;

                        if (getting)
                            list.Add(listed[i]);
                    }
                    Objects.Pop();

                    return wasString ? (object)string.Join("", list) : list;
                }
                throw new Exception($"<{classObject}> НЕ ДОПУСТИМАЯ ФУНКЦИЯ ДЛЯ ИСПОЛЬЗОВАНИЯ В <{this}>");
            }
            throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[1]}> ДЛЯ <{this}>");
        }

        public IFunction Cloned() => new FilterFunction();

        public override string ToString() => $"ФИЛЬТР(<>)";
    }

    public sealed class ListingFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is List<object>)
                return x[0];
            if (x[0] is string || x[0] is double || x[0] is long)
                return Convert.ToString(x[0]).ToCharArray().Select(c => (object)Convert.ToString(c)).ToList();
            if (x[0] is bool)
                return (((bool)x[0]) ? "Истина" : "Ложь").ToCharArray().Select(c => (object)Convert.ToString(c)).ToList();
            throw new Exception($"КОНВЕРТАЦИЯ НЕ УДАЛАСЬ: <{x[0]}>");
        }

        public IFunction Cloned() => new ListingFunction();

        public override string ToString() => "ЛИСТОМ(<>)";
    }
}
