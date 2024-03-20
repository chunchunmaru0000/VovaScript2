using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public int ArgsCount()
        {
            return Args.Length;
        }

        public string GetArgName(int i)
        {
            if (i < 0 || i >= ArgsCount())
                return "";
            return Args[i].View;
        }

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
            return Objects.NOTHING;
        }

        public override string ToString()
        {
            return $"({string.Join(", ", Args.Select(a => a.View))}){{{Body}}}";
        }
    }

    public sealed class MethodExpression : IExpression
    {
        public Token ObjectName;
        public Token MethodName;
        public IExpression Borrow;

        public MethodExpression(Token objectName, Token methodName, IExpression borrow)
        {
            ObjectName = objectName;
            MethodName = methodName;
            Borrow = borrow;
        }

        public object Evaluated()
        {
            IClass classObject = Objects.GetClassObject(ObjectName.View);
            UserFunction method = classObject.GetMethod(MethodName.View);
            FunctionExpression borrow = Borrow as FunctionExpression;
            object[] args = borrow.Args.Select(a => a.Evaluated()).ToArray();
            if (args.Length < method.ArgsCount())
                throw new Exception($"НЕВЕРНОЕ КОЛИЧЕСТВО АРГУМЕНТОВ: БЫЛО<{args.Length}> ОЖИДАЛОСЬ<{method.ArgsCount()}>");
            Objects.Push();
            //object things
            foreach (var attribute in classObject.Attributes)
                Objects.AddVariable(attribute.Key, attribute.Value);
            foreach (var methode in classObject.Methods)
                Objects.AddFunction(methode.Key, methode.Value);
            foreach (var obj in classObject.ClassObjects)
                Objects.AddClassObject(obj.Key, obj.Value);
            //attrs
            for (int i = 0; i < method.ArgsCount(); i++)
                Objects.AddVariable(method.GetArgName(i), args[i]);
            //execute
            object result = method.Execute();
            //restore or update
            foreach (var variable in Objects.Variables)
                if (classObject.ContainsAttribute(variable.Key))
                    classObject.AddAttribute(variable.Key, variable.Value);
            foreach (var obj in Objects.ClassObjects)
                if (classObject.ContainsClassObject(obj.Key))
                    classObject.AddClassObject(obj.Key, obj.Value);
            try
            {
                foreach (var function in Objects.Functions)
                    if (classObject.ContainsMethod(function.Key))
                        classObject.AddMethod(function.Key, (UserFunction)function.Value);
            } catch { }

            Objects.Pop();
            return result;
        }

        public override string ToString() => $"{ObjectName}.{Borrow}";
    }

    public sealed class Sinus : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ, БЫЛО: <{x.Length}>");
            return Math.Sin(Convert.ToDouble(x[0]));
        }

        public override string ToString() 
        {
            return $"СИНУС(<>)";
        }
    }

    public sealed class Cosinus : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ, БЫЛО: <{x.Length}>");
            return Math.Cos(Convert.ToDouble(x[0]));
        }

        public override string ToString()
        {
            return $"КОСИНУС(<>)";
        }
    }

    public sealed class Ceiling : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ, БЫЛО: <{x.Length}>");
            return Math.Ceiling(Convert.ToDouble(x[0]));
        }

        public override string ToString()
        {
            return $"ПОТОЛОК(<>)";
        }
    }

    public sealed class Floor : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ, БЫЛО: <{x.Length}>");
            return Math.Floor(Convert.ToDouble(x[0]));
        }

        public override string ToString()
        {
            return $"ЗАЗЕМЬ(<>)";
        }
    }

    public sealed class Tan : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ, БЫЛО: <{x.Length}>");
            return Math.Tan(Convert.ToDouble(x[0]));
        }

        public override string ToString()
        {
            return $"ТАНГЕНС(<>)";
        }
    }

    public sealed class Max : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ, БЫЛО: <{x.Length}>");
            return MoreMax(x);
            //throw new Exception($"С ДАННЫМИ ТИПАМИ ПЕРЕМЕННЫХ ДАННАЯ ФЕНКЦИЯ НЕВОЗМОЖНА: <{this}> <>");
        }

        private object MoreMax(object[] x)
        {
            int I = 0;
            double max = Convert.ToDouble(x[I]);
            for (int i = 0; i < x.Length; i++)
            {
                double iterable;
                if (x[i] is string)
                    iterable = ((string)x[i]).Length;
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
            if (x[I] is string)
                return Convert.ToInt64(((string)x[I]).Length);
            if (x[I] is bool)
                return (bool)x[I] ? 1 : 0;
            throw new Exception($"ЭТОГО ПРОСТО НЕ МОЖЕТ БЫТЬ: <{x[I]}> <{TypePrint.Pyc(x[I])}>");
        }

        public override string ToString()
        {
            return $"НАИБОЛЬШЕЕ(<>)";
        }
    }

    public sealed class Min : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ, БЫЛО: <{x.Length}>");
            return LessMax(x);
            //throw new Exception($"С ДАННЫМИ ТИПАМИ ПЕРЕМЕННЫХ ДАННАЯ ФЕНКЦИЯ НЕВОЗМОЖНА: <{this}> <>");
        }

        private object LessMax(object[] x)
        {
            int I = 0;
            double min = Convert.ToDouble(x[I]);
            for (int i = 0; i < x.Length; i++)
            {
                double iterable;
                if (x[i] is string)
                    iterable = ((string)x[i]).Length;
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
            if (x[I] is string || x[I] is bool)
                return x[I];
            throw new Exception($"ЭТОГО ПРОСТО НЕ МОЖЕТ БЫТЬ: <{x[I]}> <{TypePrint.Pyc(x[I])}>");
        }

        public override string ToString()
        {
            return $"НАИМЕНЬШЕЕ(<>)";
        }
    }

    public sealed class Square : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ, БЫЛО: <{x.Length}>");
            return Math.Sqrt(Convert.ToDouble(x[0]));
        }

        public override string ToString()
        {
            return $"КОРЕНЬ(<>)";
        }
    }

    public sealed class ReadAllFileFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ, БЫЛО: <{x.Length}>");
            string file = Convert.ToString(x[0]);

            if (x.Length >= 2 && Convert.ToString(x[1]) == "линии")
            {
                try
                {
                    return File.ReadAllLines(file, System.Text.Encoding.UTF8).Select(s => (object)s).ToList();
                }
                catch (IOException)
                {
                    throw new Exception("НЕ ПОЛУЧИЛОСЬ ПРОЧИТАТЬ ФАЙЛ В: " + file);

                }
            }
            else
            {
                try
                {
                    return File.ReadAllText(file, System.Text.Encoding.UTF8);
                }
                catch (IOException)
                {
                    throw new Exception("НЕ ПОЛУЧИЛОСЬ ПРОЧИТАТЬ ФАЙЛ В: " + file);
                }
            }
        }

        public override string ToString()
        {
            return $"ВЫЧИТАТЬ(<>)";
        }
    }

    public sealed class SplitFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length == 0)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ, БЫЛО: <{x.Length}>");
            string stroka = Convert.ToString(x[0]);

            if (x.Length == 1) 
                return stroka.Split('\n').Select(s => (object)s).ToList();
            else
            {
                string sep = Convert.ToString(x[1]);
                char separator = sep[0]; //(sep == "\\n") ? '\n' : (sep == "\\t") ? '\t' : sep[0];
                return stroka.Split(separator).Select(s => (object)s).ToList();
            }
        }

        public override string ToString()
        {
            return $"РАЗДЕЛ(<>)";
        }
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

        public override string ToString()
        {
            return $"ВВОД(<>)";
        }
    }

    public sealed class StringingFunction : IFunction
    {
        public object Execute(object[] x)
        {
            switch (x.Length)
            {
                case 0:
                    return "";
                case 1:
                    return x[0] is bool ? (bool)x[0] ? "Истина" : "Ложь" : Convert.ToString(x[0]);
                default:
                    return x.Select(s => s is bool ? (bool)s ? (object)"Истина" : (object)"Ложь" : (object)Convert.ToString(s)).ToList();
            }
        }

        public override string ToString()
        {
            return "СТРОЧИТЬ(<>)";
        }
    }

    public sealed class IntingFunction : IFunction
    {
        public object Execute(object[] x)
        {
            try
            {
                switch (x.Length)
                {
                    case 0:
                        return 0;
                    case 1:
                        return Convert.ToInt64(x[0]);
                    default:
                        return x.Select(s => (object)Convert.ToInt64(s)).ToList();
                }
            }
            catch (Exception) { throw new Exception("КОНВЕРТАЦИЯ НЕ УДАЛАСЬ"); }
        }

        public override string ToString()
        {
            return "ЧИСЛИТЬ(<>)";
        }
    }

    public sealed class DoublingFunction : IFunction
    {
        public object Execute(object[] x)
        {
            try
            {
                switch (x.Length)
                {
                    case 0:
                        return 0;
                    case 1:
                        return Convert.ToDouble(x[0]);
                    default:
                        return x.Select(s => (object)Convert.ToDouble(s)).ToList();
                }
            }
            catch (Exception) { throw new Exception("КОНВЕРТАЦИЯ НЕ УДАЛАСЬ"); }
        }

        public override string ToString()
        {
            return "ТОЧИТЬ(<>)";
        }
    }

    public sealed class WritingFileFunction : IFunction
    {
        public object Execute(object[] x)
        {
            if (x.Length < 3)
                throw new Exception($"НЕДОСТАТОЧНО АРГУМЕНТОВ, БЫЛО: <{x.Length}>");
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
                                throw new Exception("НЕСУЩЕСТВУЮЩИЙ РЕЖИМ ЗАПИСИ: " + file);
                        }
            }
            catch (IOException)
            {
                throw new Exception("НЕ ПОЛУЧИЛОСЬ ЗАПИСАТЬ В ФАЙЛ: " + file);
            }

            return x;
        }

        public override string ToString()
        {
            return "ЛЕТОПИСИТЬ(<>)";
        }
    }
}
