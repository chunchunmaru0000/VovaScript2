using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace VovaScript
{
    public sealed class AssignStatement : IStatement, IExpression
    {
        public Token Variable;
        public IExpression Expression;
        public IExpression Slot;
        public object Result;

        public AssignStatement(Token variable, IExpression expression) 
        { 
            Variable = variable;
            Expression = expression;
        }

        public AssignStatement(IExpression slot, IExpression expression)
        {
            Slot = slot;
            Expression = expression;
        }

        public void Execute()
        {
            object Result = Expression.Evaluated();
            Objects.AddVariable(Variable.View, Result);
        }

        public object Evaluated()
        {
            Execute();
            return Result is IClass ? ((IClass)Result).Clone() : Result;
        }

        public IStatement Clone() => new AssignStatement(Variable.Clone(), Expression.Clon());

        public IExpression Clon() => new AssignStatement(Variable.Clone(), Expression.Clon());

        public override string ToString() => $"{Variable} = {Expression};";
    }

    public sealed class PrintStatement : IStatement, IExpression
    {
        public IExpression Expression;
        public object Result;

        public PrintStatement(IExpression expression) => Expression = expression;

        public void Execute()
        {
            object value = Expression.Evaluated();
            if (value is List<object>)
            {
                Result = ListString((List<object>)value);
                Console.WriteLine(Result);
            }
            else if (value is bool)
            {
                Result = (bool)value ? "Истина" : "Ложь";
                Console.WriteLine(Result);
            }
            else if (value is IClass)
            {
                IClass classObject = value as IClass;
                Result = classObject.Clone();
                if (classObject.ContainsAttribute("строкой"))
                {
                    object strokoi = classObject.GetAttribute("строкой");
                    if (strokoi is IClass)
                        if (!(((IClass)strokoi).Body is null))
                            Console.WriteLine(((IClass)strokoi).Body.Execute());
                        else
                            Console.WriteLine(classObject);
                    else
                        Console.WriteLine(classObject);
                }
                else
                    Console.WriteLine(classObject);
            }
            else if (value is string)
            {
                Console.WriteLine('"' + Convert.ToString(value) + '"');
                Result = value;
            }
            else
            {
                Console.WriteLine(value);
                Result = value;
            }
        }

        public object Evaluated()
        {
            Execute();
            return Result;
        }

        public IStatement Clone() => new PrintStatement(Expression.Clon());

        public IExpression Clon() => new PrintStatement(Expression.Clon());

        public static string ListString(List<object> list)
        {
            string text = "[";
            foreach (object item in list)
            {
                if (item is List<object>)
                    text += ListString((List<object>)item);
                else if (item is bool)
                    text += (bool)item ? "Истина" : "Ложь";
                else if (item is string)
                    text += '"' + (string)item + '"';
                else if (item is char)
                    text += '"' + Convert.ToString(item) + '"';
                else if (item is IExpression)
                    text += ((IExpression)item).ToString();
                else
                    text += Convert.ToString(item);

                if (item != list.Last())
                    text += ", ";
            }
            return text + ']';
        }

        public override string ToString() => $"НАЧЕРТАТЬ {Expression};";
    }

    public sealed class IfStatement : IStatement, IExpression
    {
        public IExpression Expression;
        public IStatement IfPart;
        public IStatement ElsePart;
        public IfStatement(IExpression expression, IStatement ifStatement, IStatement elseStatement)
        {
            Expression = expression;
            IfPart = ifStatement;
            ElsePart = elseStatement;
        }

        public IExpression Clon() => new IfStatement(Expression.Clon(), IfPart.Clone(), ElsePart.Clone());

        public IStatement Clone() => new IfStatement(Expression.Clon(), IfPart.Clone(), ElsePart.Clone());

        public void Execute()
        {
            bool result = Convert.ToBoolean(Expression.Evaluated());
            if (result)
                IfPart.Execute();
            else if (ElsePart != null)
                ElsePart.Execute();
        }

        public object Evaluated()
        {
            bool result = Convert.ToBoolean(Expression.Evaluated());
            if (result)
                try
                {
                    IfPart.Execute();
                }
                catch (ReturnStatement ret) { return ret.GetResult(); }
            else if (ElsePart != null)
                try
                {
                    ElsePart.Execute();
                }
                catch (ReturnStatement ret) { return ret.GetResult(); }
            throw new Exception($"ЕСЛИ ИСПОЛЬЗОВАТЬ ТАК СТРУКТУРУ <{this}> ТО НАДО ЧТО-ТО ВЕРНУТЬ");
        }

        public override string ToString() => $"ЕСЛИ {Expression} ТОГДА {{{IfPart}}} ИНАЧЕ {{{ElsePart}}}";
    }

    public sealed class BlockStatement : IStatement, IExpression
    {
        public List<IStatement> Statements;

        public BlockStatement() => Statements = new List<IStatement>();

        public BlockStatement(List<IStatement> statements) => Statements = statements;

        public void Execute()
        {
            foreach (IStatement statement in Statements)
                statement.Execute();
        }

        public object Evaluated()
        {
            try
            {
                foreach (IStatement statement in Statements)
                    statement.Execute();
            }
            catch (ReturnStatement result)
            {
                return result.GetResult();
            }
            throw new Exception($"ЕСЛИ ИСПОЛЬЗОВАТЬ ТАК СТРУКТУРУ <{this}> ТО НАДО ЧТО-ТО ВЕРНУТЬ");
        }

        public IStatement Clone() => new BlockStatement() { Statements = Statements.Select(s => s.Clone()).ToList() };

        public IExpression Clon() => new BlockStatement() { Statements = Statements.Select(s => s.Clone()).ToList() };

        public void AddStatement(IStatement statement) => Statements.Add(statement);

        public override string ToString() => string.Join("|", Statements.Select(s =>'<' + s.ToString() + '>').ToArray());
    }

    public sealed class WhileStatement : IStatement, IExpression
    {
        IExpression Expression;
        IStatement Statement;

        public WhileStatement(IExpression expression, IStatement statement)
        {
            Expression = expression;
            Statement = statement;
        }

        public IStatement Clone() => new WhileStatement(Expression.Clon(), Statement.Clone());

        public IExpression Clon() => new WhileStatement(Expression.Clon(), Statement.Clone());

        public void Execute()
        {
            while(Convert.ToBoolean(Expression.Evaluated()))
            {
                try
                {
                    Statement.Execute();
                }
                catch (BreakStatement)
                {
                    break;
                }
                catch (ContinueStatement)
                {
                    // continue by itself
                }
            }
        }

        public object Evaluated()
        {
            while (Convert.ToBoolean(Expression.Evaluated()))
            {
                try
                {
                    Statement.Execute();
                }
                catch (BreakStatement)
                {
                    break;
                }
                catch (ContinueStatement)
                {
                    // continue by itself
                }
                catch (ReturnStatement result)
                {
                    return result.GetResult();
                }
            }
            throw new Exception($"ЕСЛИ ИСПОЛЬЗОВАТЬ ТАК СТРУКТУРУ <{this}> ТО НАДО ЧТО-ТО ВЕРНУТЬ");
        }

        public override string ToString() => $"{Expression}: {{{Statement}}}";
    }

    public sealed class ForStatement : IStatement, IExpression
    {
        IStatement Definition;
        IExpression Condition;
        IStatement Alter;
        IStatement Statement;

        public ForStatement(IStatement definition, IExpression condition, IStatement alter, IStatement statement)
        {
            Definition = definition;
            Condition = condition;
            Alter = alter;
            Statement = statement;
        }

        public IStatement Clone() => new ForStatement(Definition.Clone(), Condition.Clon(), Alter.Clone(), Statement.Clone());

        public IExpression Clon() => new ForStatement(Definition.Clone(), Condition.Clon(), Alter.Clone(), Statement.Clone());

        public void Execute()
        {
            for (Definition.Execute(); Convert.ToBoolean(Condition.Evaluated()); Alter.Execute())
            {
                try
                {
                    Statement.Execute();
                }
                catch (BreakStatement)
                {
                    break;
                }
                catch (ContinueStatement)
                {
                    // continue by itself
                }
            }
        }

        public object Evaluated()
        {
            for (Definition.Execute(); Convert.ToBoolean(Condition.Evaluated()); Alter.Execute())
            {
                try
                {
                    Statement.Execute();
                }
                catch (BreakStatement)
                {
                    break;
                }
                catch (ContinueStatement)
                {
                    // continue by itself
                }
                catch (ReturnStatement result)
                {
                    return result.GetResult();
                }
            }
            throw new Exception($"ЕСЛИ ИСПОЛЬЗОВАТЬ ТАК СТРУКТУРУ <{this}> ТО НАДО ЧТО-ТО ВЕРНУТЬ");
        }

        public override string ToString() => $"ДЛЯ {Definition} {Condition} {Alter}: {Statement}";
    }

    public sealed class BreakStatement : Exception, IStatement, IExpression
    {
        public void Execute() => throw this;

        public IStatement Clone() => new BreakStatement();

        public IExpression Clon() => new BreakStatement();

        public object Evaluated() => "ВЫЙТИ";

        public override string ToString() => "ВЫЙТИ;";
    }

    public sealed class ContinueStatement : Exception, IStatement, IExpression
    {
        public void Execute() => throw this;

        public IStatement Clone() => new ContinueStatement();

        public IExpression Clon() => new ContinueStatement();

        public object Evaluated() => "ПРОДОЛЖИТЬ";

        public override string ToString() => "ПРОДОЛЖИТЬ;";
    }

    public sealed class ReturnStatement : Exception, IStatement, IExpression
    {
        public IExpression Expression;
        public object Value;

        public ReturnStatement(IExpression expression) => Expression = expression;

        public void Execute()
        {
            Value = Expression.Evaluated();
            throw this;
        }

        public object Evaluated()
        {
            Value = Expression.Evaluated();
            throw this;
        }

        public IStatement Clone() => new ReturnStatement(Expression.Clon());

        public IExpression Clon() => new ReturnStatement(Expression.Clon());

        public object GetResult() => Value;

        public override string ToString() => $"ВЕРНУТЬ {Value};";
    }

    public sealed class DeclareFunctionStatement : IStatement, IExpression
    {
        public Token Name;
        public Token[] Args;
        public IStatement Body;
        public IClass Pool;
        public IClass Function;

        public DeclareFunctionStatement(Token name, Token[] args, IStatement body, IClass pool = null)
        {
            Name = name;
            Args = args;
            Body = body;
            Pool = pool;
        }

        public IStatement Clone() => new DeclareFunctionStatement(Name.Clone(), Args.Select(a => a.Clone()).ToArray(), Body.Clone(), Pool.Clone());

        public IExpression Clon() => new DeclareFunctionStatement(Name.Clone(), Args.Select(a => a.Clone()).ToArray(), Body.Clone(), Pool.Clone());

        public void Execute()
        {
            IClass function = new IClass(Name.View, new Dictionary<string, object>(), new UserFunction(Args, Body));
            Function = function;
            if (Pool is null)
                Objects.AddVariable(Name.View, function);
            else
                Pool.AddAttribute(Name.View, function);
//=> Objects.AddFunction(Name.View, new UserFunction(Args, Body));
        } 

        public object Evaluated()
        {
            Execute();
            return Function.Clone();
        }

        public override string ToString() => $"{Name} => ({string.Join("|", Args.Select(a => a.View))}) {Body};";
    }

    public sealed class ProcedureStatement : IStatement, IExpression
    {
        public IExpression Function;

        public ProcedureStatement(IExpression function) => Function = function;

        public IStatement Clone() => new ProcedureStatement(Function.Clon());

        public IExpression Clon() => new ProcedureStatement(Function.Clon());

        public void Execute() => Function.Evaluated();

        public object Evaluated() => Function.Evaluated();

        public override string ToString() => $"ВЫПОЛНИТЬ ПРЕЦЕДУРУ {Function};";
    }

    public sealed class ClearStatement : IStatement, IExpression
    {
        public void Execute() => Console.Clear();

        public object Evaluated() => "ЧИСТКА КОНСОЛИ";

        public IStatement Clone() => new ClearStatement();

        public IExpression Clon() => new ClearStatement();

        public override string ToString() => "ЧИСТКА КОНСОЛИ;";
    }

    public sealed class SleepStatement : IStatement, IExpression
    {
        public IExpression Ms;

        public SleepStatement(IExpression ms) => Ms = ms;

        public IStatement Clone() => new SleepStatement(Ms.Clon());

        public IExpression Clon() => new ClearStatement();

        public void Execute() => Thread.Sleep(Convert.ToInt32(Ms.Evaluated()));

        public object Evaluated()
        {
            int ms = Convert.ToInt32(Ms.Evaluated());
            Thread.Sleep(ms);
            return Convert.ToInt64(ms);
        }

        public override string ToString() => $"СОН({Ms})";
    }

    public sealed class ItemAssignStatement : IStatement, IExpression
    {
        public IExpression List;
        public IExpression Index;
        public IExpression Expression;
        public IClass Pool;

        public ItemAssignStatement(IExpression list, IExpression index, IExpression expression, IClass pool = null)
        {
            List = list;
            Index = index;
            Expression = expression;
            Pool = pool;
        }

        public IStatement Clone() => new ItemAssignStatement(List.Clon(), Index.Clon(), Expression.Clon(), Pool is null ? null : Pool.Clone());

        public IExpression Clon() => new ItemAssignStatement(List.Clon(), Index.Clon(), Expression.Clon(), Pool is null ? null : Pool.Clone());

        public void Execute()
        {
            ((List<object>)List.Evaluated())[Convert.ToInt32(Index.Evaluated())] = Expression.Evaluated();
            Console.WriteLine("Я НЕ УВЕРЕН ЧТО ОН ПРИСВАИВАЕТ ТАК КАК РАНЬШЕ ОНО ПРИСВАИВАЛЬСЯ ПО ИМЕНИ ПЕРЕМЕННОЙ А СЕЙЧАС Я ХОТЕЛ ПРОВЕРИТЬ ЕСЛИ ОБЬЕКТ БУДЕТ ССЫЛОЧНЫМ И ПОЫЕЗЕТ И ЕГО ЭЛЕМЕНТ ИЗМЕНИТСЯ");
        }

        public object Evaluated()
        {
            Execute();
            return List.Evaluated();
        }

        public override string ToString() => $"{List}[{Index.Evaluated()}] = {Expression.Evaluated()};";
    }

    public sealed class OperationAssignStatement : IStatement, IExpression
    {
        public Token Variable;
        public Token Operation;
        public IExpression Expression;
        public object Result;

        public OperationAssignStatement(Token variable, Token operation, IExpression expression)
        {
            Variable = variable;
            Operation = operation;
            Expression = expression;
        }

        public IStatement Clone() => new OperationAssignStatement(Variable.Clone(), Operation.Clone(), Expression.Clon());

        public IExpression Clon() => new OperationAssignStatement(Variable.Clone(), Operation.Clone(), Expression.Clon());

        public void Execute()
        {
            string name = Variable.View;
            object value = Expression.Evaluated();
            object variable = Objects.GetVariable(name);
            object result = null;
            switch (Operation.Type)
            {
                case TokenType.PLUSEQ:
                    if (value is bool)
                    {
                        if (variable is long)
                            result = Convert.ToInt64(variable) + ((bool)value ? 1 : 0);
                        else if (variable is double)
                            result = Convert.ToDouble(variable) + ((bool)value ? 1 : 0);
                        else if (variable is string)
                            result = Convert.ToString(variable) + ((bool)value ? 1 : 0);
                    }
                    else if(variable is double || value is double)
                        result = Convert.ToDouble(variable) + Convert.ToDouble(value);
                    else if (variable is string || value is string)
                        result = Convert.ToString(variable) + Convert.ToString(value);
                    else if (variable is long)
                        result = Convert.ToInt64(variable) + Convert.ToInt64(value);
                    else if (variable is List<object>)
                    {
                        if (value is List<object>) 
                            ((List<object>)variable).AddRange((List<object>)value);
                        else
                            ((List<object>)variable).Add(value);
                        result = variable;
                    }
                    else throw new Exception($"НЕДОПУСТИМОЕ ДЕЙСТВИЕ МЕЖДУ: <{variable}>({TypePrint.Pyc(variable)}) И <{value}>({TypePrint.Pyc(value)})");
                    break;
                case TokenType.MINUSEQ:
                    if (variable is double || value is double)
                        result = Convert.ToDouble(variable) - Convert.ToDouble(value);
                    else if (variable is string || value is string)
                        result = Convert.ToString(variable).Replace(Convert.ToString(value), "");
                    else if (variable is long)
                        result = Convert.ToInt64(variable) - Convert.ToInt64(value);
                    else throw new Exception($"НЕДОПУСТИМОЕ ДЕЙСТВИЕ МЕЖДУ: <{variable}>({TypePrint.Pyc(variable)}) И <{value}>({TypePrint.Pyc(value)})");
                    break;
                case TokenType.MULEQ:
                    if (variable is double || value is double)
                        result = Convert.ToDouble(variable) * Convert.ToDouble(value);
                    else if (variable is long)
                        result = Convert.ToInt64(variable) * Convert.ToInt64(value);
                    else throw new Exception($"НЕДОПУСТИМОЕ ДЕЙСТВИЕ МЕЖДУ: <{variable}>({TypePrint.Pyc(variable)}) И <{value}>({TypePrint.Pyc(value)})");
                    break;
                case TokenType.DIVEQ:
                    if (variable is double || value is double || variable is long || value is long)
                        result = Convert.ToDouble(variable) / Convert.ToDouble(value);
                    else throw new Exception($"НЕДОПУСТИМОЕ ДЕЙСТВИЕ МЕЖДУ: <{variable}>({TypePrint.Pyc(variable)}) И <{value}>({TypePrint.Pyc(value)})");
                    break;
                default:
                    throw new Exception($"НЕ МОЖЕТ БЫТЬ: <{name}> <{variable}> <{value}> <{Operation.View}>");
            }
            Objects.AddVariable(name, result);
            Result = result;
        }

        public object Evaluated()
        {
            Execute();
            return Result is IClass ? ((IClass)Result).Clone() : Result;
        }

        public override string ToString() => $"{Variable.View} {Operation.View} {Expression}";
    }

    public sealed class ProgramStatement : IStatement, IExpression
    {
        IExpression Program;

        public ProgramStatement(IExpression program) => Program = program;

        public IStatement Clone() => new ProgramStatement(Program.Clon());

        public IExpression Clon() => new ProgramStatement(Program.Clon());

        public void Execute()
        {
            string code = Convert.ToString(Program.Evaluated());
            VovaScript2.PycOnceLoad(code);
        }

        public object Evaluated()
        {
            string code = Convert.ToString(Program.Evaluated());
            try
            {
                VovaScript2.PycOnceLoad(code);
            }
            catch (ReturnStatement result)
            {
                return result.GetResult();
            }
            return code;
        }

        public override string ToString() => "РУСИТЬ " + Program.ToString();
    }

    public sealed class NothingStatement : IStatement, IExpression
    { 
        public void Execute() { }

        public object Evaluated() => Objects.NOTHING;

        public IStatement Clone() => new NothingStatement();

        public IExpression Clon() => new NothingStatement();

        public override string ToString() => "НИЧЕГО"; 
    }

    public sealed class DeclareClassStatement : IStatement, IExpression
    {
        public Token ClassName;
        public IStatement Body;

        public DeclareClassStatement(Token className, IStatement body)
        {
            ClassName = className;
            Body = body;
        }

        public IStatement Clone() => new DeclareClassStatement(ClassName.Clone(), Body.Clone());

        public IExpression Clon() => new DeclareClassStatement(ClassName.Clone(), Body.Clone());

        public void Execute()
        {
            IClass newClass = new IClass(ClassName.View, new Dictionary<string, object>());
            BlockStatement body = Body as BlockStatement;
            foreach (IStatement statement in body.Statements)
            {
                if (statement is AssignStatement)
                {
                    AssignStatement assign = statement as AssignStatement;
                    object result = assign.Expression.Evaluated();
                    newClass.AddAttribute(assign.Variable.View, result);
                    continue;
                }
                if (statement is DeclareFunctionStatement)
                {
                    DeclareFunctionStatement method = statement as DeclareFunctionStatement;
                    method.Execute();
                    newClass.AddAttribute(method.Name.View, new IClass(method.Name.View, new Dictionary<string, object>(), new UserFunction(method.Args, method.Body)));
                    continue;
                }
                throw new Exception($"НЕДОПУСТИМОЕ ВЫРАЖЕНИЕ ДЛЯ ОБЬЯВЛЕНИЯ В КЛАССЕ: <{TypePrint.Pyc(statement)}> С ТЕЛОМ {statement}");
            }
            Objects.AddVariable(newClass.Name, newClass);
        }

        public object Evaluated()
        {
            Execute();
            return new NewObjectExpression(ClassName, new IStatement[0]);
        }

        public override string ToString() => $"КЛАСС {ClassName.View}{{{Body}}}";
    }

    public sealed class AttributeAssignStatement : IStatement, IExpression
    {
        public Token ObjName;
        public Token[] Attributes;
        public IExpression Value;
        public object Result;

        public AttributeAssignStatement(Token objName, Token[] attrs, IExpression value)
        {
            ObjName = objName;
            Attributes = attrs;
            Value = value;
        }

        public IStatement Clone() => new AttributeAssignStatement(ObjName.Clone(), Attributes.Select(a => a.Clone()).ToArray(), Value.Clon());

        public IExpression Clon() => new AttributeAssignStatement(ObjName.Clone(), Attributes.Select(a => a.Clone()).ToArray(), Value.Clon());

        public void Execute() 
        {
            if (Objects.ContainsVariable(ObjName.View))
            {
                object got = Objects.GetVariable(ObjName.View);
                if (got is IClass)
                {
                    IClass classObject = got as IClass;
                    IClass last = null;
                    for (int i = 0; i < Attributes.Length-1; i++)
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
                            {
                                Result = Value.Evaluated();
                                last.AddAttribute(Attributes[i].View, Result);
                                return;
                            }
                            throw new Exception($"НЕ ОБЪЕКТ: <{Attributes[i]}> ГДЕ-ТО В <{ObjName}>");
                        }
                        if (i == Attributes.Length - 1)
                        {
                            Result = Value.Evaluated();
                            classObject.AddAttribute(Attributes[i].View, Result);
                            return;
                        }
                        throw new Exception($"НЕСУЩЕСТВУЮЩИЙ КАК ОБЪЕКТ: <{Attributes[i]}> ГДЕ-ТО В <{ObjName}>");
                    }
                    Result = Value.Evaluated();
                    classObject.AddAttribute(Attributes.Last().View, Result);
                    return;
                }
            }
            throw new Exception($"НЕСУЩЕСТВУЮЩИЙ КАК ОБЪЕКТ: <{ObjName}>");
        }

        public object Evaluated()
        {
            Execute();
            return Result is IClass ? ((IClass)Result).Clone() : Result;
        }
        
        public override string ToString() => $"{ObjName}.{PrintStatement.ListString(Attributes.Select(a => (object)a).ToList())} = {Value}";
    }

    public sealed class MethodAssignStatement : IStatement, IExpression
    {
        public Token ObjName;
        public Token[] Attributes;
        public Token[] Args;
        public IStatement Body;
        public object Result;

        public MethodAssignStatement(Token objectName, Token[] attrs, Token[] args, IStatement body)
        {
            ObjName = objectName;
            Attributes = attrs;
            Args = args;
            Body = body;
        }

        public IStatement Clone() => new MethodAssignStatement(ObjName.Clone(), Attributes.Select(a => a.Clone()).ToArray(), Args.Select(a => a.Clone()).ToArray(), Body.Clone());

        public IExpression Clon() => new MethodAssignStatement(ObjName.Clone(), Attributes.Select(a => a.Clone()).ToArray(), Args.Select(a => a.Clone()).ToArray(), Body.Clone());

        //   public void Execute() => (Objects.GetVariable(ObjectName.View) as IClass).AddAttribute(MethodName.View, new IClass(MethodName.View, new Dictionary<string, object>(), new UserFunction(Args, Body)));

        public void Execute()
        {
            if (Objects.ContainsVariable(ObjName.View))
            {
                object got = Objects.GetVariable(ObjName.View);
                if (got is IClass)
                {
                    IClass classObject = got as IClass;
                    IClass last = null;
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
                            {
                                Result = new IClass(Attributes[i].View, new Dictionary<string, object>(), new UserFunction(Args, Body));
                                last.AddAttribute(Attributes[i].View, Result);
                                return;
                            }
                            throw new Exception($"НЕ ОБЪЕКТ: <{Attributes[i]}> ГДЕ-ТО В <{ObjName}>");
                        }
                        if (i == Attributes.Length - 1)
                        {
                            Result = new IClass(Attributes[i].View, new Dictionary<string, object>(), new UserFunction(Args, Body));
                            classObject.AddAttribute(Attributes[i].View, Result);
                            return;
                        }
                        throw new Exception($"НЕСУЩЕСТВУЮЩИЙ КАК ОБЪЕКТ: <{Attributes[i]}> ГДЕ-ТО В <{ObjName}>");
                    }
                    Result = new IClass(Attributes.Last().View, new Dictionary<string, object>(), new UserFunction(Args, Body));
                    classObject.AddAttribute(Attributes.Last().View, Result);
                    return;
                }
            }
            throw new Exception($"НЕСУЩЕСТВУЮЩИЙ КАК ОБЪЕКТ: <{ObjName}>");
        }

        public object Evaluated()
        {
            Execute();
            return Result is IClass ? ((IClass)Result).Clone() : Result;
        }

        public override string ToString() => $"{ObjName}.{PrintStatement.ListString(Attributes.Select(a => (object)a).ToList())} => ({string.Join("|", Args.Select(a => a.View))}) {Body}";
    }
}
