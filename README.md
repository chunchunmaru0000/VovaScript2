# ВоваСкрипт

Является библиотекой-интерпертируемым языком.

Сам интерпретатор языка может выглядеть примерно как:

```
using VovaScript;

namespace Negr 
{ 
    class Program
    {
        static void Main(string[] args)
        {                   // само имя файла в args[0], а остальное это мне для тестов
            string filename = args.Length > 0 ? args[0] : string.Join("\\", Environment.CurrentDirectory.Split('\\').Take(6)) + "\\test.pyclan";
            Console.WriteLine(filename);
            if (File.Exists(filename))
            {
                string code = File.ReadAllText(filename);
                VovaScript2.PycOnceLoad(code, filename);
            }
            VovaScript2.Pyc();
        }
    }
}
```
## Компиляция

Также может быть и скомпилирован в исполнимый файл компилятором C#, если вызвать его инлайн через

```
VovaScript2.PycOnceLoad("код", filename);
```
