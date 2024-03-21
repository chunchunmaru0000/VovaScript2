using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VovaScript
{
    public sealed class SQLCreateDatabaseStatement : IStatement, IExpression
    {
        public IExpression Database;

        public SQLCreateDatabaseStatement(IExpression database)
        {
            Database = database;
        }

        public IStatement Clone() => new SQLCreateDatabaseStatement(Database.Clon());

        public IExpression Clon() => new SQLCreateDatabaseStatement(Database.Clon());

        public void Execute() 
        {
            string name = Convert.ToString(Database.Evaluated());
            var database = new { бд = name };
            string json = JsonConvert.SerializeObject(database);
            File.WriteAllText($"{name}.pycdb", json, System.Text.Encoding.UTF8);
        }

        public object Evaluated()
        {
            Execute();
            return Database.Evaluated();
        }

        public override string ToString() => $"СОЗДАТЬ БАЗУ ДАННЫХ <{Database}>";
    }

    public sealed class SQLCreateTableStatement : IStatement, IExpression
    {
        public IExpression TableName;
        public Token[] Types;
        public Token[] Names;
        public object Result;

        public SQLCreateTableStatement(IExpression tableName, Token[] types, Token[] names)
        {
            TableName = tableName;
            Types = types;
            Names = names;
        }

        public IStatement Clone() => new SQLCreateTableStatement(TableName.Clon(), Types.Select(t => t.Clone()).ToArray(), Names.Select(n => n.Clone()).ToArray());

        public IExpression Clon() => new SQLCreateTableStatement(TableName.Clon(), Types.Select(t => t.Clone()).ToArray(), Names.Select(n => n.Clone()).ToArray());

        public void Execute()
        {
            string database = Convert.ToString(Objects.GetVariable("ИСПБД")) + ".pycdb";
            string data = File.ReadAllText(database, System.Text.Encoding.UTF8);
            dynamic jsonData = JsonConvert.DeserializeObject(data);

            string tableName = Convert.ToString(TableName.Evaluated());
            try
            {
                JObject jObj = jsonData as JObject;
                jObj.Add(tableName, new JObject());

                JObject tableJobj = jObj[tableName] as JObject;
                string[] types = Types.Select(t => t.View).ToArray();
                string[] names = Names.Select(n => n.View).ToArray();

                tableJobj.Add("колонки", new JObject());
                JObject colonJobj = tableJobj["колонки"] as JObject;
                tableJobj.Add("колонок", new JArray());
                JArray colonsJobj = (JArray)tableJobj["колонок"];

                for (int i = 0; i < types.Length; i++)
                    colonJobj.Add(names[i], types[i]);

                for (int i = 0; i < types.Length; i++)
                    colonsJobj.Add(names[i]);

                tableJobj.Add("значения", new JArray());

                File.WriteAllText(database, JsonConvert.SerializeObject(jObj), System.Text.Encoding.UTF8);
                Result = jObj.ToString();
            } catch (ArgumentException) {
                Console.ForegroundColor = ConsoleColor.Red;
                Result = $"ТАБЛИЦА С ИМЕНЕНМ <{tableName}> УЖЕ СУЩЕСТВУЕТ";
                Console.WriteLine($"ТАБЛИЦА С ИМЕНЕНМ <{tableName}> УЖЕ СУЩЕСТВУЕТ");
                Console.ResetColor();
              //  throw new Exception($"ТАБЛИЦА С ИМЕНЕНМ <{tableName}> УЖЕ СУЩЕСТВУЕТ"); 
            }
        }

        public object Evaluated()
        {
            Execute();
            return Result;
        }

        public override string ToString() => $" СОЗДАТЬ ТАБЛИЦУ {TableName} {{ТИПЫ: {string.Join(", ", Types.Select(t => t.ToString()))};\n НАЗВАНИЯ: {string.Join(", ", Names.Select(n => n.ToString()))};}}";
    }

    public sealed class SQLInsertStatement : IStatement, IExpression
    {
        public IExpression TableName;
        public IExpression[] Colons;
        public IExpression[] Values;
        public object Result;

        public SQLInsertStatement(IExpression tableName, IExpression[] colons, IExpression[] values)
        {
            TableName = tableName;
            Colons = colons;
            Values = values;
        }

        public IStatement Clone() => new SQLInsertStatement(TableName.Clon(), Colons.Select(c => c.Clon()).ToArray(), Values.Select(v => v.Clon()).ToArray());

        public IExpression Clon() => new SQLInsertStatement(TableName.Clon(), Colons.Select(c => c.Clon()).ToArray(), Values.Select(v => v.Clon()).ToArray());

        public void Execute()
        {
            string database = Convert.ToString(Objects.GetVariable("ИСПБД")) + ".pycdb";
            string data = File.ReadAllText(database, System.Text.Encoding.UTF8);
            dynamic jsonData = JsonConvert.DeserializeObject(data);

            string tableName = Convert.ToString(TableName.Evaluated());

            try
            {
                JObject jObj = jsonData as JObject;
                JObject tableJobj = jObj[tableName] as JObject;
                JObject colonsJobj = tableJobj["колонки"] as JObject;
                JArray valuesJobj = tableJobj["значения"] as JArray;

                string[] colonsNames = colonsJobj.Properties().Select(n => n.Name).ToArray();
                string[] colonsTypes = colonsJobj.Properties().Select(n => n.Value.ToString()).ToArray();

                string[] colonsReaded = Colons.Select(c => Convert.ToString(c.Evaluated())).ToArray();
                object[] valuesReaded = Values.Select(v => v.Evaluated()).ToArray();

                int value = 0;
                JArray toBeAdded = new JArray();
                if (colonsReaded.Length > 0)
                    for (int i = 0; i < colonsNames.Length; i++)
                    {
                        JObject temp = new JObject();
                        temp.Add("колонка", colonsNames[i]);
                        temp.Add("значение", JToken.FromObject(colonsReaded.Contains(colonsNames[i]) ? valuesReaded[value++] is bool ? (bool)valuesReaded[value - 1] ? "Истина" : "Ложь" : valuesReaded[value - 1] : "НИЧЕГО"));
                        toBeAdded.Add(temp);
                    }
                else
                    for (int i = 0; i < colonsNames.Length; i++)
                    {
                        // Console.WriteLine(PrintStatement.ListString(colonsNames.Select(c => (object)c).ToList())); Console.WriteLine(PrintStatement.ListString(valuesReaded.Select(c => c).ToList())); Console.WriteLine(i); Console.WriteLine(value); Console.WriteLine(colonsNames[i]); Console.WriteLine(valuesReaded[value]);
                        JObject temp = new JObject();
                        temp.Add("колонка", colonsNames[i]);
                        temp.Add("значение", JToken.FromObject(valuesReaded[value++] is bool ? (bool)valuesReaded[value-1] ? "Истина" : "Ложь" : valuesReaded[value-1]));
                        toBeAdded.Add(temp);
                    }

                valuesJobj.Add(toBeAdded);
                File.WriteAllText(database, JsonConvert.SerializeObject(jObj), System.Text.Encoding.UTF8);
                Result = jObj.ToString();
            } catch (Exception e)  {
                Result = "СДЕЛАЙ НОРМАЛЬНУЮ ОБРАБОТКУ ОШИБОК В ИНСЕРТ СКЛ";
                Console.ForegroundColor = ConsoleColor.Red; 
                Console.WriteLine(e); 
                Console.ResetColor(); 
            }
        }

        public object Evaluated()
        {
            Execute();
            return Result;
        }

        public override string ToString() => $"ДОБАВИТЬ В {TableName} КОЛОНКИ ({string.Join(", ", Colons.Select(c => c.ToString()))})\nЗНАЧЕНИЯ({string.Join(", ", Values.Select(v => v.ToString()))})";
    }

    public sealed class SQLConditionExpression : IExpression 
    {
        Token[] Condition;
        List<object> Data;
        int position;

        public SQLConditionExpression(Token[] condition, List<object> data)
        {
            Condition = condition;
            Data = data;
            position = 0;
        }

        public IExpression Clon() => new SQLConditionExpression(Condition.Select(c => c.Clone()).ToArray(), Data);

        public object Parser()
        {
            Console.WriteLine(position);
            Console.WriteLine(PrintStatement.ListString(Data));
            Console.WriteLine(PrintStatement.ListString(Condition.Select(c => (object)c).ToList()));
            throw new Exception("ТЫ ДАУН НЕ СДЕЛАЛ ЕЩЕ ЭТО УСЛОВИЯ В СКЛ");
        }

        public object Evaluated() => throw new Exception("ЧЕЛ");
    }

    public struct SelectedColumn
    {
        public string Selection { get; set; }
        public string Alias { get; set; }
        public string From { get; set; }
    }

    public sealed class SQLSelectExpression : IExpression
    {
        List<IExpression> Selections;
        List<IExpression> Ats;
        List<IExpression> Aliases;
        List<IExpression> Froms;
        Token[] Condition;

        public SQLSelectExpression(List<IExpression> selections, List<IExpression> ats, List<IExpression> aliases, List<IExpression> froms, Token[] condition)
        {
            Selections = selections;
            Ats = ats;
            Aliases = aliases;
            Froms = froms;
            Condition = condition;
        }

        public IExpression Clon() => new SQLSelectExpression(
            Selections.Select(s => s.Clon()).ToList(),
            Ats.Select(s => s.Clon()).ToList(),
            Aliases.Select(s => s.Clon()).ToList(),
            Froms.Select(s => s.Clon()).ToList(),
            Condition.Select(c => c.Clone()).ToArray()
            );

        public object Evaluated()
        {
            // begin setup vars
            string database = Convert.ToString(Objects.GetVariable("ИСПБД")) + ".pycdb";
            string data = File.ReadAllText(database, System.Text.Encoding.UTF8);
            dynamic jsonData = JsonConvert.DeserializeObject(data);
            JObject jObj = jsonData as JObject;
            //seslections
            string[] selections = Selections.Select(s => Convert.ToString(s.Evaluated())).ToArray();
            // ats
            string[] ats = new string[selections.Length];
            // aliases
            string[] aliases = new string[selections.Length];
            for (int i = 0; i < selections.Length; i++)
            {
                ats[i] = Convert.ToString(Ats[i].Evaluated());
                aliases[i] = Aliases[i] == Parser.Nothingness ? selections[i] : Convert.ToString(Aliases[i].Evaluated());
            }
            // froms
            string[] froms = Froms.Select(f => Convert.ToString(f.Evaluated())).ToArray();
            // others
            JObject[] tableJobj = froms.Select(f => jObj[f] as JObject).ToArray();
            JArray[] values = tableJobj.Select(t => t["значения"] as JArray).ToArray();
            Dictionary<string, JArray> fromToValues = new Dictionary<string, JArray>();
            for (int i = 0; i < froms.Length; i++)
                fromToValues.Add(froms[i], values[i]);
            // selected columns finally
            List<SelectedColumn> columns = new List<SelectedColumn>();
            for (int select = 0, column = 0; select < selections.Length; select++, column++)
                if (selections[select] == "всё")
                {
                    JObject table = jObj[Ats[select] == Parser.Nothingness ? froms[0] : ats[select]] as JObject;
                    string[] columnsNames = table["колонок"].ToObject<string[]>();
                    foreach(string name in columnsNames)
                    {
                        string from = Ats[select] == Parser.Nothingness ? froms[0] : ats[select];
                        columns.Add(new SelectedColumn() { Selection = name, Alias = name + " от " + from, From = from });
                    }
                }
                else
                    columns.Add (new SelectedColumn() { Selection = selections[select], Alias = aliases[select], From = Ats[select] == Parser.Nothingness ? froms[0] : ats[select] });
            // selected "dicts"
            object[] selected = columns.Select(a => new List<object> { a.Alias, null }).ToArray();
            // select of all
            try
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    JToken[][] valuesArray = fromToValues[columns[i].From].Select(v => v.ToArray()).ToArray();
                    List<object> toBeAdded = new List<object>();
                    foreach (JToken[] value in valuesArray)
                    {
                        foreach (JToken token in value)
                            if ((string)token["колонка"] == columns[i].Selection)
                                toBeAdded.Add(token["значение"]);
                        ((List<object>)selected[i])[1] = toBeAdded;
                    }
                }
            }
            catch (KeyNotFoundException) { throw new Exception($"НЕ СУЩЕСТВУЕТ СТОЛБЦА С НЕКОТОРЫМ НАЗВАНИЕМ\nЭТО МОЖЕТ БЫТЬ ПО ПРИЧИНЕ НЕ УКАЗАНИЯ ИСТОЧНИКА ПРИ ВЫБОРЕ ИЗ НЕСКОЛЬКИХ ТАБЛИЦ"); }
            // selection of what is need based on condition
            if (Condition == null)
                return selected.ToList();
            else
            {

                throw new NotImplementedException("ДАУН4");
            }
        }

        public override string ToString() => $"ВЫБРАТЬ {string.Join(", ", Selections)} ИЗ {string.Join(", ", Froms)}" + (Condition != null ? "ГДЕ {Condition}" : "");
    }
}
