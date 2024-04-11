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
            object value = got;
            if (value is IClass)
            {
                IClass classObject = value as IClass;
                got = classObject.GetAttribute(MethodName.View);
            }
            else if (value is long)
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

        public override string ToString() => $"{Pool}.{Borrow}";
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
            string file = HelpMe.GiveMeSafeStr(x[0]);

            if (x.Length >= 2 && HelpMe.GiveMeSafeStr(x[1]) == "линии")
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
            string stroka = HelpMe.GiveMeSafeStr(x[0]);

            if (x.Length == 1) 
                return stroka.Split().Select(s => (object)s).ToList();
            else
            {
                string sep = HelpMe.GiveMeSafeStr(x[1]);
                int len = sep.Length;

                if (sep.Length == 1)
                    return stroka.Split(sep[0]).Select(s => (object)s).ToList();
                else
                {
                    List<object> list = new List<object>();
                    string buffer = "";
                    foreach(char letter in stroka)
                    {
                        buffer += letter;
                        if (buffer.EndsWith(sep))
                        {
                            list.Add(buffer.Substring(0, buffer.Length - len));
                            buffer = "";
                        }
                    }
                    if (buffer != "")
                        list.Add(buffer);
                    return list;
                }
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
            Console.Write(string.Join(" ", x.Select(h => HelpMe.GiveMeSafeStr(h))));
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
                Console.Write(HelpMe.GiveMeSafeStr(x[0]));
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
                    return HelpMe.GiveMeSafeStr(x[0]);
                default:
                    return x.Select(s => HelpMe.GiveMeSafeStr(s)).ToList();
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
                        return x[0] is bool ? (bool)x[0] ? (object)1 : (object)0 : Int64.Parse(HelpMe.GiveMeSafeStr(x[0]));
                    default:
                        return x.Select(s => s is string ? (object)Int64.Parse((string)s) 
                                           : s is bool ? (bool)s ? (object)1 : (object)0
                                           : (object)Int64.Parse(HelpMe.GiveMeSafeStr(s))).ToList();
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
            string file = HelpMe.GiveMeSafeStr(x[0]);
            string mode = HelpMe.GiveMeSafeStr(x[1]);
            string data = HelpMe.GiveMeSafeStr(x[2]);

            try
            {
                if (!File.Exists(file))
                    using (FileStream fs = File.Create(file)) { }

                if (mode == "пере")
                {
                    using (StreamWriter writer = new StreamWriter(file, false, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine(data); 
                    }
                }
                else
                {
                    using (StreamWriter writer2 = new StreamWriter(file, true, System.Text.Encoding.UTF8))
                    {
                        switch (mode)
                        {
                            case "до":
                                writer2.Write(data);
                                break;
                            case "линию":
                                writer2.WriteLine(data);
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"НЕСУЩЕСТВУЮЩИЙ РЕЖИМ ЗАПИСИ <{mode}>: " + file);
                                throw new Exception($"НЕСУЩЕСТВУЮЩИЙ РЕЖИМ ЗАПИСИ <{mode}>: " + file);
                        }
                    }
                }
            }
            catch
            {
                throw new Exception($"НЕ ПОЛУЧИЛОСЬ ЗАПИСАТЬ В ФАЙЛ: <{file}> <{mode}> <{data}>");
            }
            return x[0];
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
            return Convert.ToDouble(SliceExpression.Len(x[0]));
            //throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[0]}> ДЛЯ <{this}>");
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
            return Convert.ToInt64((int)(HelpMe.GiveMeSafeStr(x[0])[0]));
            //throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[0]}> ДЛЯ <{this}>");
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
                string took = HelpMe.GiveMeSafeStr(x[0]);
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
                string took = HelpMe.GiveMeSafeStr(x[0]);
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
                string took = HelpMe.GiveMeSafeStr(x[0]);
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
                string took = HelpMe.GiveMeSafeStr(x[0]);
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
                        //check if size of list was changed and will change it online in перебор
                        if (list.Count < listed.Count)
                            list.AddRange(listed.Skip(list.Count));
                        // i dont thing it will work properly always but i dunno
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
            if (x[0] is string || x[0] is double || x[0] is long || x[0] is bool)
                return HelpMe.GiveMeSafeStr(x[0]).ToCharArray().Select(c => (object)Convert.ToString(c)).ToList();
            throw new Exception($"КОНВЕРТАЦИЯ НЕ УДАЛАСЬ: <{x[0]}>");
        }

        public IFunction Cloned() => new ListingFunction();

        public override string ToString() => "ЛИСТОМ(<>)";
    }

    public sealed class AppendFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 2)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is List<object>)
            {
                if (x.Length == 2)
                    ((List<object>)x[0]).Add(x[1]);
                else
                    ((List<object>)x[0]).AddRange(x.Skip(1));
                
                return x[0];
            }
            throw new Exception($"НЕВЕРНЫЙ ОБЪЕКТ <{x[0]}> ДЛЯ <{this}>");
        }

        public IFunction Cloned() => new AppendFunction();

        public override string ToString() => "ДОБАВИТЬ(<>)";
    }

    public sealed class DeleteItemFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 2)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is List<object>)
            {
                try
                {
                    if (x.Length == 2)
                        ((List<object>)x[0]).RemoveAt(HelpMe.GiveMeSafeInt(x[1]));
                    else
                        ((List<object>)x[0]).RemoveRange(
                            HelpMe.GiveMeSafeInt(x[1]),
                            HelpMe.GiveMeSafeInt(x[2]));
                    return x[0];
                }
                catch(ArgumentException)
                {
                    throw new Exception("ВИДИМО БЫЛО УДАЛЕНО СЛИШКОМ МНОГО ЭЛЕМЕНТОВ");
                }
            }

            List<object> list = SliceExpression.Obj2List(x[0]);
            try
            {
                bool wasString = x[0] is string;
                if (x.Length == 2)
                    list.RemoveAt(HelpMe.GiveMeSafeInt(x[1]));
                else
                    list.RemoveRange(
                        HelpMe.GiveMeSafeInt(x[1]),
                        HelpMe.GiveMeSafeInt(x[2]));
                return wasString ? (object)string.Join("", list) : list;
            }
            catch (ArgumentException)
            {
                throw new Exception("ВИДИМО БЫЛО УДАЛЕНО СЛИШКОМ МНОГО ЭЛЕМЕНТОВ");
            }
        }

        public IFunction Cloned() => new DeleteItemFunction();

        public override string ToString() => "УДАЛИТЬ(<>)";
    }

    public sealed class JoinedFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 2)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            string joined = "";
            joined = HelpMe.GiveMeSafeStr(x[0]);

            if (x.Length == 2)
                if (x[1] is List<object>)
                    return string.Join(joined, (List<object>)x[1]);
                else
                    throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[1]}> ДЛЯ <{this}>");
            else
                return string.Join(joined, x.Skip(1).Select(s => HelpMe.GiveMeSafeStr(s)));

            throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[0]}> ДЛЯ <{this}>");
        }

        public IFunction Cloned() => new JoinedFunction();

        public override string ToString() => "СОЕДИНИТЬ(<>)";
    }

    public sealed class JoinedByFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            List<object> list = SliceExpression.Obj2List(x[0]);

            string joined = x.Length == 1 ? "" : HelpMe.GiveMeSafeStr(x[1]);
            //else throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[1]}> ДЛЯ <{this}>");
            return string.Join(joined, list);
        }

        public IFunction Cloned() => new JoinedByFunction();

        public override string ToString() => "СОЕДИНЁН(<>)";
    }

    public sealed class ReverseFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x[0] is string || x[0] is long || x[0] is double || x[0] is bool)
                return string.Join("", HelpMe.GiveMeSafeStr(x[0]).Reverse());
            if (x[0] is List<object>)
            {
                List<object> temp = x[0] as List<object>;
                temp.Reverse();
                return temp;
            }
            throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[0]}> ДЛЯ <{this}>");
        }

        public IFunction Cloned() => new ReverseFunction();

        public override string ToString() => "ОБРАТНО(<>)";
    }

    public sealed class SortFunction : IFunction
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
                    List<object[]> resulted = new List<object[]>();

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
                        resulted.Add(new object[] { i, lambda.Execute() });
                    }
                    Objects.Pop();
                    resulted = resulted.OrderBy(r => r[1]).ToList();
                    List<object> list = new List<object>(listed);

                    for (int i = 0; i < listed.Count; i++)
                        list[i] = listed[Convert.ToInt32(resulted[i][0])];

                    return wasString ? (object)string.Join("", list) : list;
                }
                throw new Exception($"<{classObject}> НЕ ДОПУСТИМАЯ ФУНКЦИЯ ДЛЯ ИСПОЛЬЗОВАНИЯ В <{this}>");
            }
            throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[1]}> ДЛЯ <{this}>");
        }

        public IFunction Cloned() => new SortFunction();

        public override string ToString() => $"ПОРЯДКОМ(<>)";
    }

    public sealed class ContainsFunction : IFunction
    {
        public static bool CompareListOfLists(List<object> first, List<object> second) 
        {
            if (first.Count != second.Count)
                return false;
            for (int i = 0; i < first.Count; i++)
            {
                object f = first[i];
                object s = second[i];

                if (f is List<object>)
                {
                    if (s is List<object> == false)
                        return false;
                    if (!CompareListOfLists((List<object>)f, (List<object>)s))
                        return false;
                    continue;
                }
                if (f.GetType() == s.GetType())
                {
                    if (f is long)
                        if (Convert.ToInt64(f) == Convert.ToInt64(s))
                            continue;
                        else
                            return false;
                    if (f is double)
                        if (Convert.ToDouble(f) == Convert.ToDouble(s))
                            continue;
                        else
                            return false;
                    if (f is bool)
                        if (Convert.ToBoolean(f) == Convert.ToBoolean(s))
                            continue;
                        else
                            return false;
                    if (f is string)
                        if (Convert.ToString(f) == Convert.ToString(s))
                            continue;
                        else
                            return false;
                    throw new Exception($"ЧТО ЗА ТИП ТАКОЙ ЭЭЭ <{f.GetType()}> У <{f}> И <{s}>");
                }
                return false;
            }
            return true;
        }

        public object Execute(object[] x)
        {
            if (x.Length < 2)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            List<object> list = SliceExpression.Obj2List(x[0]);
            if (x.Length == 2)
                return x[1] is List<object> ? list.Where(l => l is List<object>).Any(l => CompareListOfLists((List<object>)l, (List<object>)x[1])) : list.Contains(x[1]);
            else
                return x.Skip(1).All(l => l is List<object> ? list.Where(ll => ll is List<object>).Any(ll => CompareListOfLists(((List<object>)l), (List<object>)ll)) : list.Contains(l));

        }

        public IFunction Cloned() => new ContainsFunction();

        public override string ToString() => "СОДЕРЖИТ(<>)";
    }

    public sealed class DirFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            object value = x[0];
            if (value is IClass)
            {
                IClass classObject = value as IClass;
                return classObject.Attributes.Select(a => (object)new List<object> { a.Key, a.Value }).ToList();
            }
            else if (value is long)
            {
                IClass IInt = Objects.IInteger.Clone();
                return IInt.Attributes.Select(a => (object)new List<object> { a.Key, a.Value }).ToList();
            }
            else if (value is string)
            {
                IClass IStr = Objects.IString.Clone();
                return IStr.Attributes.Select(a => (object)new List<object> { a.Key, a.Value }).ToList();
            }
            else if (value is double)
            {
                IClass IFlt = Objects.IFloat.Clone();
                return IFlt.Attributes.Select(a => (object)new List<object> { a.Key, a.Value }).ToList();
            }
            else if (value is bool)
            {
                IClass IBol = Objects.IBool.Clone();
                return IBol.Attributes.Select(a => (object)new List<object> { a.Key, a.Value }).ToList();
            }
            else if (value is List<object>)
            {
                IClass ILst = Objects.IList.Clone();
                return ILst.Attributes.Select(a => (object)new List<object> { a.Key, a.Value }).ToList();
            }
            throw new Exception($"ЧО ЗА ТИП ТАКОЙ ЭЭЭ <{value.GetType()}> У <{value}>");
        }

        public IFunction Cloned() => new DirFunction();

        public override string ToString() => "ВЛАДЕЕТ(<>)";
    }

    public sealed class ReplaceFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 3)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            string stroka = HelpMe.GiveMeSafeStr(x[0]);
            string toRep = HelpMe.GiveMeSafeStr(x[1]);
            string byRep = HelpMe.GiveMeSafeStr(x[2]);
            return stroka.Replace(toRep, byRep);
        }

        public IFunction Cloned() => new ReplaceFunction();

        public override string ToString() => $"ЗАМЕНА(<>)";
    }

    public sealed class TrimFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            string stroka = HelpMe.GiveMeSafeStr(x[0]);

            if (x.Length == 1)
                return stroka.Trim();
            string trimBy = HelpMe.GiveMeSafeStr(x[1]);
            return stroka.Trim(trimBy.ToCharArray());
        }

        public IFunction Cloned() => new TrimFunction();

        public override string ToString() => $"ОБРЕЗ(<>)";
    }

    public sealed class TrimStartFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            string stroka = HelpMe.GiveMeSafeStr(x[0]);

            if (x.Length == 1)
                return stroka.TrimStart();
            string trimBy = HelpMe.GiveMeSafeStr(x[1]);
            return stroka.TrimStart(trimBy.ToCharArray());
        }

        public IFunction Cloned() => new TrimStartFunction();

        public override string ToString() => $"ОБРЕЗ_ЛЕВ(<>)";
    }
    
    public sealed class TrimEndFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            string stroka = HelpMe.GiveMeSafeStr(x[0]);

            if (x.Length == 1)
                return stroka.TrimEnd();
            string trimBy = HelpMe.GiveMeSafeStr(x[1]);
            return stroka.TrimEnd(trimBy.ToCharArray());
        }

        public IFunction Cloned() => new TrimEndFunction();

        public override string ToString() => $"ОБРЕЗ_ПРАВ(<>)";
    }

    public sealed class SumFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            if (x.Length > 1)
            {
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

                        List<object> listed = x[0] is string || x[0] is List<object> ? SliceExpression.Obj2List(x[0]) :
                                throw new Exception($"<{x[0]}> НЕ БЫЛ ЛИСТОМ ИЛИ СТРОКОЙ, А <{x[0]}>");

                        double sumFloat = 0;

                        if (listed.Count == 0)
                            return (double)0;

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

                            if (result is double || result is long)
                            {
                                sumFloat += Convert.ToDouble(result);
                                continue;
                            }
                            if (result is string)
                            {
                                sumFloat += Convert.ToString(result).Length;
                                continue;
                            }
                            if (result is bool)
                            {
                                sumFloat += (bool)result ? 1 : 0;
                                continue;
                            }
                            if (result is List<object>)
                            {
                                sumFloat += ((List<object>)result).Count;
                                continue;
                            }
                            if (result is IClass)
                                throw new Exception($"НЕ ПОДДЕРЖИВАЕТ СУММУ ОБЪЕКТОВ <{result}>, А ТОЛЬКО ЧИСЕЛ");
                        }
                        Objects.Pop();

                        return sumFloat;
                    }
                    throw new Exception($"<{classObject}> НЕ ДОПУСТИМАЯ ФУНКЦИЯ ДЛЯ ИСПОЛЬЗОВАНИЯ В <{this}>");
                }
                throw new Exception($"НЕДОПУСТИМЫЙ ТИП ОБЪЕКТА <{x[1]}> ДЛЯ <{this}>");
            }

            List<object> list = x[0] is string || x[0] is List<object> ? SliceExpression.Obj2List(x[0]) :
                                throw new Exception($"<{x[0]}> НЕ БЫЛ ЛИСТОМ ИЛИ СТРОКОЙ, А <{x[0]}>");
            return list.Sum(l => HelpMe.GiveMeSafeDouble(l));
        }

        public IFunction Cloned() => new SumFunction();

        public override string ToString() => $"СУММОЙ(<>)";
    }

    public sealed class CountFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 2)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            string stroka = HelpMe.GiveMeSafeStr(x[0]);
            string counter = HelpMe.GiveMeSafeStr(x[1]);
            int clen = counter.Length;

            if (stroka.Length < clen)
                return (long)0;
            if (stroka == counter)
                return (long)1;

            string buffer;
            long count = 0;
            int len = stroka.Length - clen + 1;
           
            for (int i = 0; i < len; i++)
            {
                buffer = stroka.Substring(i, clen);
                if (buffer == counter)
                    count++;
            }

            return count;
        }

        public IFunction Cloned() => new CountFunction();

        public override string ToString() => $"СЧЕТ(<>)";
    }

    public sealed class RandomFunction : IFunction
    {
        public static Random rnd = new Random();

        public object Execute(object[] x)
        {
            if (x.Length == 0)
                return rnd.NextDouble();

            if (x.Length < 2)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");

            int from = HelpMe.GiveMeSafeInt(x[0]);
            int to = HelpMe.GiveMeSafeInt(x[1]);
            return Convert.ToInt64(rnd.Next(from, to));
        }

        public IFunction Cloned() => new RandomFunction();

        public override string ToString() => $"СЛУЧАЙНЫЙ(<>)";
    }

    public sealed class ToDateFunction : IFunction
    {
        public static Random rnd = new Random();

        public object Execute(object[] x)
        {
            if (x.Length < 1)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");

            long time = HelpMe.GiveMeSafeLong(x[0]);
            DateTime date = DateTime.FromBinary(time);
            return date.TimeOfDay.ToString();
        }

        public IFunction Cloned() => new RandomFunction();

        public override string ToString() => $"ДАТОЙ(<>)";
    }

    sealed class BinFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 2)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");

            long number = HelpMe.GiveMeSafeLong(x[0]);
            int lenght = HelpMe.GiveMeSafeInt(x[1]);

            string result = Convert.ToString(number, 2);
            while (result.Length < lenght)
                result = "0" + result;

            return result;
        }

        public IFunction Cloned() => new BinFunction();

        public override string ToString() => $"ДВОИЧНЫМ(<>)";
    }

    sealed class BinPlusFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 2)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ ДЛЯ <{this}>, БЫЛО: <{x.Length}>");
            try
            {
                string str = HelpMe.GiveMeSafeStr(x[0]);
                long left = Convert.ToInt64(str, 2);
                long right = HelpMe.GiveMeSafeLong(x[1]);
                //Console.WriteLine(left + " | " + right);
                string result = Convert.ToString(left + right, 2);
                while (result.Length < str.Length)
                    result = "0" + result;

                return result;
            }
            catch (OverflowException) 
                  { throw new Exception($"ЧИСЛО <{x[0]}> БЫЛО СЛИШКОМ ВЕЛИКО ИЛИ МАЛО ДЛЯ ЧИЕЛ ПОДДЕРЖИВАЕМЫХ НА ДАННЫЙ МОМЕНТ"); }
            catch { throw new Exception($"ЧТО-ТО ИЗ <{HelpMe.GiveMeSafeStr(x[0])}> ИЛИ <{HelpMe.GiveMeSafeStr(x[1])}> НЕ БЫЛО КОРРЕКТНЫМ ПАРАМЕТРОМ ДЛЯ <{this}>"); }
        }

        public IFunction Cloned() => new BinPlusFunction();

        public override string ToString() => $"ДВОИЧ_ПЛЮС(<>)";
    }
}