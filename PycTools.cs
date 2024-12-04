using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace VovaScript
{
    public static class PycTools
    {
        public static bool Usable(char c) {
            return c != '+' && c != '-' && c != '*' && c != '/' && c != '%' &&
                   c != '(' && c != ')' && c != '[' && c != ']' && c != '{' && c != '}' && c != '|' &&
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

    public struct FullNodeToAssign
    {
        public Token ObjName;
        public object[] Parts;

        public FullNodeToAssign Clone() => new FullNodeToAssign() { ObjName = ObjName.Clone(), Parts = Parts.Select(p => p is Token ? (object)(p as Token).Clone() : (object)(p as IExpression[]).Select(e => e.Clon()).ToArray()).ToArray() };

        public override string ToString() => ObjName.ToString() + (Parts.Length < 1 ? "" : string.Join("", Parts.Select(p => p )));
    }

    public struct ListAndIndeces
    {
        public List<object> List;
        public int[] Indeces;
    }

    public struct VarAttrSliceNode
    {
        public Token ObjName;
        public Token[] Attrs;
        public IExpression[][] Slices;

        public override string ToString() => ObjName.ToString() + 
                                             (Attrs is null ? "" : string.Join(".", Attrs.Select(a => a.ToString()))) + 
                                             (Slices is null ? "" : "[ИН:ДЕК:СЫ]");
    }

    public struct ParrentAttrValue
    {
        public IClass Parrent;
        public string AttrName;
        public object Value;
    }

    public struct ObjectValue
    {
        public string ObjName;
        public IClass Parrent;
        public object Value;
        public bool IsChild;
        public bool IsItem;
    }

    public struct IndecesValue
    {
        public List<int> AssignIndeces;
        public List<object> Value;
        public bool WasString;
    }

    public static class HelpMe 
    {
        public static ParrentAttrValue GiveMeAttrAndValue(Token ObjName, Token [] Attributes, IExpression Value)
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
                                return new ParrentAttrValue() { Parrent = last, AttrName = Attributes[i].View, Value = Value.Evaluated() };
                            throw new Exception($"НЕ ОБЪЕКТ: <{Attributes[i]}> ГДЕ-ТО В <{ObjName}>");
                        }
                        if (i == Attributes.Length - 1)
                            return new ParrentAttrValue() { Parrent = classObject, AttrName = Attributes[i].View, Value = Value.Evaluated() };
                        throw new Exception($"НЕСУЩЕСТВУЮЩИЙ КАК ОБЪЕКТ: <{Attributes[i]}> ГДЕ-ТО В <{ObjName}>");
                    }
                    return new ParrentAttrValue() { Parrent = classObject, AttrName = Attributes.Last().View, Value = Value.Evaluated() };
                }
            }
            throw new Exception($"НЕСУЩЕСТВУЮЩИЙ КАК ОБЪЕКТ: <{ObjName}>");
        }

        public static IndecesValue GiveMeIndecesAndValue(Token ObjName, IExpression[][] Slices, object taked)
        {
            /*
            if (taked is string)
            {
                /*
                int[] indeces = null;
                List<int> taken = Enumerable.Range(0, taked is List<object> ? ((List<object>)taked).Count : Convert.ToString(taked).Length).Select(e => e).ToList();
                foreach (IExpression[] slice in Slices)
                {
                    taken = Enumerable.Range(0, taked is List<object> ? ((List<object>)taked).Count : Convert.ToString(taked).Length).Select(e => e).ToList();
                    if (slice[0] is null && slice[1] is null && slice[2] is null)
                        continue;
                    int from = SliceExpression.DetermineIndex(slice[0]);

                    if ((!(slice[1] is null) && slice[1].Evaluated() is string) || taken.Count == 1)
                    {
                        /*
                         taken = GiveMeSafeInt(taken);
                        if (taked is List<object>)
                        {
                            List<object> listed = taked as List<object>;
                            taked = listed[(int)taken];
                            wasString = taked is string;
                            taken = Enumerable.Range(0, taked is List<object> ? ((List<object>)taked).Count : Convert.ToString(taked).Length).Select(e => e).ToList();
                            indeces = null;
                            continue;
                        }
                        else if (taked is string)
                        {
                        //}
                        else
                            taken = from;
                        break;
                        int len = SliceExpression.Len(taken);
                        taken = wasString ? string.Join("", Convert.ToString(Convert.ToString(taked)[from < 0 ? len + from : from])) : ((List<object>)taked)[from < 0 ? len + from : from];
                        continue;

                        string stred = taked as string;
                        return new IndecesValue() { AssignIndeces = taken, Value = SliceExpression.Obj2List(stred[taken[0]].ToString()), WasString = true };
                    }

                    int to = slice[1] is null ? SliceExpression.DetermineIndex(slice[1]) : slice[1].Evaluated() is string ? from : SliceExpression.DetermineIndex(slice[1]);
                    int step = SliceExpression.DetermineIndex(slice[2], 1);

                    indeces = SliceExpression.Sliced(taken.Select(t => (object)t).ToList(), from, to, slice[1]);
                    indeces = SliceExpression.SelectStepped(indeces.ToList(), step);

                    List<object> beforeStep = SliceExpression.Obj2List(taked);
                    List<object> newArr = new List<object>();
                    
                    try
                    {
                        foreach (int index in indeces)
                            newArr.Add(beforeStep[index]);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        throw new Exception("ОКАЗАЛСЯ ЗА ГРАНИЦАМИ ЛИСТА");
                    }
                    //taken = newArr.All(b => b is string) ? (object)string.Join("", newArr) : newArr;
                    taked = string.Join("", newArr);
                }
                List<int> assignIndecex = taken;
                List<object> value = SliceExpression.Obj2List(taked);

                return new IndecesValue() { AssignIndeces = assignIndecex, Value = value, WasString = true };
                object taken = Enumerable.Range(0, taked is List<object> ? ((List<object>)taked).Count : Convert.ToString(taked).Length).Select(e => (object)e).ToList();
                int[] indeces = null;
                foreach (IExpression[] slice in Slices)
                {
                    if (slice[0] is null && slice[1] is null && slice[2] is null)
                        continue;
                    int from = SliceExpression.DetermineIndex(slice[0]);
                    int to = slice[1] is null ? SliceExpression.DetermineIndex(slice[1]) : slice[1].Evaluated() is string ? from : SliceExpression.DetermineIndex(slice[1]);
                    int step = SliceExpression.DetermineIndex(slice[2], 1);

                    indeces = taken is string || taken is long || taken is double ? SliceExpression.Sliced(Convert.ToString(taken), from, to, slice[1]) :
                            taken is List<object> ? SliceExpression.Sliced(taken, from, to, slice[1]) :
                            throw new Exception($"<{ObjName.View}> НЕ БЫЛ ЛИСТОМ ИЛИ СТРОКОЙ, А <{taken}>");
                    indeces = SliceExpression.SelectStepped(indeces.ToList(), step);

                    List<object> beforeStep = SliceExpression.Obj2List(taken);
                    List<object> newArr = new List<object>();
                    // а = "0001";где а[1:][1:][-1] = 9;а; где а[1:][1:][-2:-1] = 8;а;
                    try
                    {
                        foreach (int index in indeces)
                            newArr.Add(beforeStep[index]);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        throw new Exception("ОКАЗАЛСЯ ЗА ГРАНИЦАМИ ЛИСТА");
                    }

                    taken = newArr.All(b => b is string) ? (object)string.Join("", newArr) : newArr;
                }
                List<int> assignIndecex = SliceExpression.Obj2List(taken).Select(a => Convert.ToInt32(a)).ToList();
                List<object> value = SliceExpression.Obj2List(taked);

                return new IndecesValue() { AssignIndeces = assignIndecex, Value = value, WasString = true };
            }
            if (taked is List<object>)
            {

            }
            throw new Exception($"ЧТО-ТО В <{ObjName.View}> НЕ БЫЛО ЛИСТОМ ИЛИ СТРОКОЙ, А <{taked}>");*/
            object taken = Enumerable.Range(0, taked is List<object> ? ((List<object>)taked).Count : Convert.ToString(taked).Length).Select(e => (object)e).ToList();
            int[] indeces = null;
            foreach (IExpression[] slice in Slices)
            {
                if (slice[0] is null && slice[1] is null && slice[2] is null)
                    continue;
                int from = SliceExpression.DetermineIndex(slice[0]);
                int to = slice[1] is null ? SliceExpression.DetermineIndex(slice[1]) : slice[1].Evaluated() is string ? from : SliceExpression.DetermineIndex(slice[1]);
                int step = SliceExpression.DetermineIndex(slice[2], 1);

                indeces = taken is string || taken is long || taken is double ? SliceExpression.Sliced(Convert.ToString(taken), from, to, slice[1]) :
                        taken is List<object> ? SliceExpression.Sliced(taken, from, to, slice[1]) :
                        throw new Exception($"<{ObjName.View}> НЕ БЫЛ ЛИСТОМ ИЛИ СТРОКОЙ, А <{taken}>");
                indeces = SliceExpression.SelectStepped(indeces.ToList(), step);

                List<object> beforeStep = SliceExpression.Obj2List(taken);
                List<object> newArr = new List<object>();

                try
                {
                    foreach (int index in indeces)
                        newArr.Add(beforeStep[index]);
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new Exception("ОКАЗАЛСЯ ЗА ГРАНИЦАМИ ЛИСТА");
                }

                taken = newArr.All(b => b is string) ? (object)string.Join("", newArr) : newArr;
            }
            List<int> assignIndecex = SliceExpression.Obj2List(taken).Select(a => Convert.ToInt32(a)).ToList();
            List<object> value = SliceExpression.Obj2List(taked);

            return new IndecesValue() { AssignIndeces = assignIndecex, Value = value };
        }

        public static object GiveMeRetOfListAndIndeces(object got, List<int> assignIndecex, List<object> value, IExpression[][] Slices, bool Exactly = false, bool Fill = true)
        {
            if (!(got is List<object>) && !(got is string))
                got = new List<object>() { got };
            List<object> toAssign = SliceExpression.Obj2List(got);
            bool was = value.All(v => v is string);

            if (Exactly)
            {
                if (toAssign.Count != assignIndecex.Count)
                    throw new Exception($"БЫЛО ИНДЕКСОВ НА НАЗНАЧЕНИЕ <{assignIndecex.Count}> НО В НАЗНАЧЕНИИ ЦЕЛЫХ <{toAssign.Count}>\n{got}");

                for (int i = 0; i < assignIndecex.Count; i++)
                    value[assignIndecex[i]] = toAssign[i];
            }
            else
            {
                if (!Fill)// && Slices.All(s => s[2] is null))
                {
                    if (toAssign.Count == assignIndecex.Count)
                        for (int i = 0; i < assignIndecex.Count; i++)
                            value[assignIndecex[i]] = toAssign[i];
                    else
                    {
                        List<object> begin = value.Take(assignIndecex[0]).ToList();
                        value.RemoveRange(assignIndecex[0], assignIndecex.Count);
                        toAssign.AddRange(value);
                        begin.AddRange(toAssign);
                        value = begin;
                    }
                }
                else
                {
                    if (toAssign.Count == assignIndecex.Count || toAssign.Count > assignIndecex.Count)
                        for (int i = 0; i < assignIndecex.Count; i++)
                            value[assignIndecex[i]] = toAssign[i];
                    else
                        for (int i = 0; i < assignIndecex.Count; i++)
                            value[assignIndecex[i]] = toAssign[i >= toAssign.Count ? toAssign.Count - 1 : i];
                }
            }
            object ret;
            if (was)
                ret = string.Join("", value.Select(v => "" + Convert.ToString(v)[0]).ToArray());
            else
                ret = value;
            return ret;
        }

        public static ObjectValue GiveMeObject(Token ObjName, Token[] Attrs, IExpression[][] Slices)
        {
            if (Attrs is null && Slices is null)
                return new ObjectValue() { 
                    ObjName = ObjName.View, 
                    Parrent = null, 
                    Value = Objects.GetVariable(ObjName.View), 
                    IsChild = false, 
                    IsItem = false 
                };
            if (Attrs is null && !(Slices is null))
                return new ObjectValue()
                {
                    ObjName = ObjName.View,
                    Parrent = null,
                    Value = SliceExpression.Obj2List(Objects.GetVariable(ObjName.View)),
                    IsChild = false,
                    IsItem = true
                };
            if (!(Attrs is null) && Slices is null)
            {
                ParrentAttrValue resultObj = GiveMeAttrAndValue(ObjName, Attrs, new NumExpression(""));
                IClass toSave = resultObj.Parrent;
                string attrName = resultObj.AttrName;
                object taked = toSave.GetAttribute(attrName);
                return new ObjectValue()
                {
                    ObjName = ObjName.View,
                    Parrent = toSave,
                    Value = taked,
                    IsChild = true,
                    IsItem = false
                };
            }
            if (!(Attrs is null) && !(Slices is null))
            {
                ParrentAttrValue resultObj = GiveMeAttrAndValue(ObjName, Attrs, new NumExpression(""));
                IClass toSave = resultObj.Parrent;
                string attrName = resultObj.AttrName;
                object taked = toSave.GetAttribute(attrName);
                IndecesValue result = GiveMeIndecesAndValue(ObjName, Slices, taked);
                return new ObjectValue()
                {
                    ObjName = ObjName.View,
                    Parrent = toSave,
                    Value = result.Value,
                    IsChild = true,
                    IsItem = true
                };
            }
            throw new Exception("ФВАПЫАПРШОЩЖВАПЫОЫВАРАПРВАПРВПАРВПРОВПРОШЩЖВААПРТОЛДЮАПРВАПРЛДЬЖЖВАПРФВОЩЗАПОЫПВАПОЫ");
        }

        public static ListAndIndeces GiveMeListAndIndeces(object obj, int[] indecesToAssign, IExpression[] part)
        {
            List<object> listed = SliceExpression.Obj2List(obj);
            int len = SliceExpression.Len(listed);
            int[] newIndeces; 
            int from = part[0] is null ? 0 : GiveMeSafeInt(part[0].Evaluated());
            int to;
            if (part[1] is null)
                to = len;
            else
            {
                object parted = part[1].Evaluated();
                to = parted is string ? from : GiveMeSafeInt(parted);
            }
            int step = part[2] is null ? 1 : GiveMeSafeInt(part[2].Evaluated());
            if (from < 0)
                from = len + from + 1;

            string message(int[] ind) =>
                $"ОКАЗАЛСЯ ЗА ГРАНИЦАМИ ЛИСТА, ИНДЕКСЫ: ОТ <{from}> ДО <{to}> ШАГ <{step}>\n" +
                $"ДАННЫМ ЗНАЧЕНИЕМ БЫЛО <{GiveMeSafeStr(listed)}>\n" +
                $"ПРИ ПОПЫТКЕ ЗВЯТЬ ИЗ ИНДЕКСОВ <{GiveMeSafeStr(indecesToAssign)}> ИНДЕКСЫ В МЕСТАХ <{ind}>";

            try
            {
                if (to != from)
                {
                    if (to < 0)
                        to = len + to;
                    newIndeces = Enumerable.Range(from, to - from).ToArray();
                }
                else
                    newIndeces = new int[1] { from };
            }
            catch (ArgumentOutOfRangeException) { throw new Exception(message(null)); }
            catch (IndexOutOfRangeException)    { throw new Exception(message(null)); }

            newIndeces = SliceExpression.SelectStepped(newIndeces.ToList(), step);

            if (indecesToAssign is null)
                return new ListAndIndeces() { Indeces = newIndeces, List = listed };

            List<int> newArr = new List<int>();
            try
            {
                foreach (int index in newIndeces)
                    newArr.Add(indecesToAssign[index]);
            }
            catch (ArgumentOutOfRangeException) { throw new Exception(message(newIndeces)); }
            catch (IndexOutOfRangeException)    { throw new Exception(message(newIndeces)); }

            return new ListAndIndeces() { Indeces = newArr.ToArray(), List = listed };
        }

        public static int GiveMeSafeInt(object x)
        {
            if (x is int)
                return Convert.ToInt32(x);
            if (x is long || x is double)
            {
                int res;
                if (int.TryParse(GiveMeSafeStr(x), out res))
                    return res;
                else
                    throw new Exception($"ЗНАЧЕНИЕ <{x}> ВЫЛО СЛИШКОМ ВЕЛИКО ДЛЯ ДАННОГО УПОТРЕБЛЕНИЯ В КАЧЕСТВЕ ЧИСЛА(ВЕРОЯТНО В КАЧЕСТВЕ ИНДЕКСА)");
            }
            throw new Exception($"БЫЛ НЕ ЧИСЛОМ, А <{x}>");
        }

        public static long GiveMeSafeLong(object x) 
        {
            if (x is bool)
                return (bool)x ? 1 : 0;
            if (x is double)
                return Convert.ToInt64(x);

            string word = GiveMeSafeStr(x);
            long res;
            if (long.TryParse(word, out res))
                return res;
            throw new Exception($"ЧИСЛО <{word}> БЫЛО СЛИШКОМ ВЕЛИКО ИЛИ МАЛО ДЛЯ ПОДДЕРЖИВАЕМЫХ СЕЙЧАС ЦЕЛЫХ ЧИСЕЛ");
        }

        public static double GiveMeSafeDouble(object x) => 
            x is long || x is double ? 
                Convert.ToDouble(x) : 
            x is bool ? 
                (bool)x ? 1 : 0 : 
            throw new Exception($"БЫЛ НЕ ЧИСЛОМ, А <{x}>");

        public static string GiveMeSafeStr(object x) =>
            x is bool ? 
                (bool)x ? "Истина" : "Ложь" : 
            x is List<object> ? 
                PrintStatement.ListString((List<object>)x) : 
            Convert.ToString(x);

        public static bool GiveMeSafeBool(object x) => 
            x is bool ? 
                (bool)x : 
            x is string ? 
                Convert.ToString(x).Length != 0 : 
            x is long || x is double ? 
                Convert.ToDouble(x) != 0 : 
            x is List<object> ?
                ((List<object>)x).Count != 0 :
            true;
    }
}