using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace VovaScript
{
    public sealed class AssignStatement : IStatement
    {
        public Token Variable;
        public IExpression Expression;

        public AssignStatement(Token variable, IExpression expression) 
        { 
            Variable = variable;
            Expression = expression;
        }

        public void Execute()
        {
            string name = Variable.View;
            object result = Expression.Evaluated();

            if (Objects.ContainsVariable(name))
                Objects.DeleteVariable(name);
            if (Objects.ContainsClassObject(name))
                Objects.DeleteClassObject(name);

            if (result is IClass)
            {
                IClass classObject = result as IClass;
                Objects.AddClassObject(Variable.View, classObject);
            }
            else
                Objects.AddVariable(name, result);
        }

        public override string ToString() => $"{Variable} = {Expression};";
    }

    public sealed class PrintStatement : IStatement
    {
        public IExpression Expression;

        public PrintStatement(IExpression expression)
        {
            Expression = expression;
        }

        public void Execute()
        {
            object value = Expression.Evaluated();
            if (value is List<object>)
                Console.WriteLine(ListString((List<object>)value));
            else if (value is bool)
                Console.WriteLine((bool)value ? "Истина" : "Ложь");
            else
                Console.WriteLine(value);
        }

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

    public sealed class IfStatement : IStatement
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

        public void Execute()
        {
            bool result = Convert.ToBoolean(Expression.Evaluated());
            if (result)
                IfPart.Execute();
            else if (ElsePart != null)
                ElsePart.Execute();
        }

        public override string ToString() => $"ЕСЛИ {Expression} ТОГДА {{{IfPart}}} ИНАЧЕ {{{ElsePart}}}";
    }

    public sealed class BlockStatement : IStatement
    {
        public List<IStatement> Statements;

        public BlockStatement() => Statements = new List<IStatement>();

        public void Execute()
        {
            foreach (IStatement statement in Statements)
                statement.Execute();
        }

        public void AddStatement(IStatement statement) => Statements.Add(statement);

        public override string ToString() => string.Join("|", Statements.Select(s =>'<' + s.ToString() + '>').ToArray());
    }

    public sealed class WhileStatement : IStatement
    {
        IExpression Expression;
        IStatement Statement;

        public WhileStatement(IExpression expression, IStatement statement)
        {
            Expression = expression;
            Statement = statement;
        }

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
                    // contonue by itself
                }
            }
        }

        public override string ToString() => $"{Expression}: {{{Statement}}}";
    }

    public sealed class ForStatement : IStatement
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

        public override string ToString() => $"ДЛЯ {Definition} {Condition} {Alter}: {Statement}";
    }

    public sealed class BreakStatement : Exception, IStatement
    {
        public void Execute() => throw this;

        public override string ToString() => "ВЫЙТИ;";
    }

    public sealed class ContinueStatement : Exception, IStatement
    {
        public void Execute() => throw this;

        public override string ToString() => "ПРОДОЛЖИТЬ;";
    }

    public sealed class ReturnStatement : Exception, IStatement
    {
        public IExpression Expression;
        public object Value;

        public ReturnStatement(IExpression expression) => Expression = expression;

        public void Execute()
        {
            Value = Expression.Evaluated();
            throw this;
        }

        public object GetResult() => Value;

        public override string ToString() => $"ВЕРНУТЬ {Value};";
    }

    public sealed class DeclareFunctionStatement : IStatement
    {
        public Token Name;
        public Token[] Args;
        public IStatement Body;

        public DeclareFunctionStatement(Token name, Token[] args, IStatement body)
        {
            Name = name;
            Args = args;
            Body = body;
        }

        public void Execute() => Objects.AddFunction(Name.View, new UserFunction(Args, Body));

        public override string ToString() => $"{Name} => ({string.Join("|", Args.Select(a => a.View))}) {Body};";
    }

    public sealed class ProcedureStatement : IStatement
    {
        public IExpression Function;

        public ProcedureStatement(IExpression function) => Function = function;

        public void Execute() => Function.Evaluated();

        public override string ToString() => $"ВЫПОЛНИТЬ ПРЕЦЕДУРУ {Function}";
    }

    public sealed class ClearStatement : IStatement
    {
        public void Execute() => Console.Clear();

        public override string ToString() => "ЧИСТКА КОНСОЛИ";
    }

    public sealed class SleepStatement : IStatement
    {
        public IExpression Ms;

        public SleepStatement(IExpression ms) => Ms = ms;

        public void Execute() => Thread.Sleep(Convert.ToInt32(Ms.Evaluated()));

        public override string ToString() => $"СОН({Ms})";
    }

    public sealed class ItemAssignStatement : IStatement
    {
        public Token Variable;
        public IExpression Index;
        public IExpression Expression;

        public ItemAssignStatement(Token variable, IExpression index, IExpression expression)
        {
            Variable = variable;
            Index = index;
            Expression = expression;
        }

        public void Execute()
        {
            string name = Variable.View;
            int index = Convert.ToInt32(Index.Evaluated());
            object value = Expression.Evaluated();
            List<object> list = (List<object>)Objects.GetVariable(name);
            list[index] = value;
            Objects.AddVariable(name, list);
        }

        public override string ToString() => $"{Variable.View}[{Index.Evaluated()}] = {Expression.Evaluated()};";
    }

    public sealed class OperationAssignStatement : IStatement
    {
        public Token Variable;
        public Token Operation;
        public IExpression Expression;

        public OperationAssignStatement(Token variable, Token operation, IExpression expression)
        {
            Variable = variable;
            Operation = operation;
            Expression = expression;
        }

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
        }

        public override string ToString() => $"{Variable.View} {Operation.View} {Expression}";
    }

    public sealed class ProgramStatement : IStatement
    {
        IExpression Program;

        public ProgramStatement(IExpression program) => Program = program;

        public void Execute()
        {
            string code = Convert.ToString(Program.Evaluated());
            VovaScript2.PycOnceLoad(code);
        }

        public override string ToString() => "РУСИТЬ " + Program.ToString();
    }

    public sealed class NothingStatement : IStatement { public void Execute() { } public override string ToString() => "НИЧЕГО"; }

    public sealed class DeclareClassStatement : IStatement
    {
        public Token ClassName;
        public IStatement Body;

        public DeclareClassStatement(Token className, IStatement body)
        {
            ClassName = className;
            Body = body;
        }

        public void Execute()
        {
            IClass newClass = new IClass(ClassName.View, new Dictionary<string, object>(), new Dictionary<string, UserFunction>());
            BlockStatement body = Body as BlockStatement;
            foreach (IStatement statement in body.Statements)
            {
                if (statement is AssignStatement)
                {
                    AssignStatement assign = statement as AssignStatement;
                    object result = assign.Expression.Evaluated();
                    if (result is IClass)
                        newClass.AddClassObject(assign.Variable.View, (IClass)result);
                    else
                        newClass.AddAttribute(assign.Variable.View, result);
                    continue;
                }
                if (statement is DeclareFunctionStatement)
                {
                    DeclareFunctionStatement method = statement as DeclareFunctionStatement;
                    newClass.AddMethod(method.Name.View, new UserFunction(method.Args, method.Body));
                    continue;
                }
                throw new Exception($"НЕДОПУСТИМОЕ ВЫРАЖЕНИЕ ДЛЯ ОБЬЯВЛЕНИЯ В КЛАССЕ: <{TypePrint.Pyc(statement)}> С ТЕЛОМ {statement}");
            }
            Objects.AddClass(newClass.Name, newClass);
        }

        public override string ToString() => $"КЛАСС {ClassName.View}{{{Body}}}";
    }

    public sealed class AttributeAssignStatement : IStatement
    {
        public Token ObjName;
        public Token AttributeName;
        public IExpression Value;

        public AttributeAssignStatement(Token objName, Token attributeName, IExpression value)
        {
            ObjName = objName;
            AttributeName = attributeName;
            Value = value;
        }

        public void Execute() => Objects.GetClassObject(ObjName.View).AddAttribute(AttributeName.View, Value.Evaluated());

        public override string ToString() => $"{ObjName}.{AttributeName} = {Value};";
    }

    public sealed class MethodAssignStatement : IStatement
    {
        public Token ObjectName;
        public Token MethodName;
        public Token[] Args;
        public IStatement Body;

        public MethodAssignStatement(Token objectName, Token methodName, Token[] args, IStatement body)
        {
            ObjectName = objectName;
            MethodName = methodName;
            Args = args;
            Body = body;
        }

        public void Execute() => Objects.GetClassObject(ObjectName.View).AddMethod(MethodName.View, new UserFunction(Args, Body));

        public override string ToString() => $"{ObjectName}.{MethodName} => ({string.Join("|", Args.Select(a => a.View))}) {Body};";
    }
}
