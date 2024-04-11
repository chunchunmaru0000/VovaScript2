namespace VovaScript
{
    static class Worder
    {
        public static Token Wordizator(Token word)
        {
            switch (word.View)
            {
                case "егда":
                    word.Type = TokenType.WORD_IF;
                    return word;
                case "еже":
                    word.Type = TokenType.WORD_IF;
                    return word;
                case "если":
                    word.Type = TokenType.WORD_IF;
                    return word;
                case "коль":
                    word.Type = TokenType.WORD_IF;
                    return word;
                case "ежели":
                    word.Type = TokenType.WORD_IF;
                    return word;

                case "иначе":
                    word.Type = TokenType.WORD_ELSE;
                    return word;
                case "иначели":
                    word.Type = TokenType.WORD_ELIF;
                    return word;
                case "не":
                    word.Type = TokenType.NOT;
                    return word;

                case "класс":
                    word.Type = TokenType.CLASS;
                    return word;
                case "новый":
                    word.Type = TokenType.NEW;
                    return word;
                case "новая":
                    word.Type = TokenType.NEW;
                    return word;
                case "новое":
                    word.Type = TokenType.NEW;
                    return word;
                case "здесь":
                    word.Type = TokenType.THIS;
                    return word;
                case "этот":
                    word.Type = TokenType.THIS;
                    return word;
                case "овый":
                    word.Type = TokenType.THIS;
                    return word;
                case "свой":
                    word.Type = TokenType.THIS;
                    return word;

                case "включить":
                    word.Type = TokenType.IMPORT;
                    return word;
                case "включи":
                    word.Type = TokenType.IMPORT;
                    return word;

                case "пока":
                    word.Type = TokenType.WORD_WHILE;
                    return word;
                case "докамест":
                    word.Type = TokenType.WORD_WHILE;
                    return word;
                case "дондеже":
                    word.Type = TokenType.WORD_WHILE;
                    return word;

                case "для":
                    word.Type = TokenType.WORD_FOR;
                    return word;
                case "цикл":
                    word.Type = TokenType.LOOP;
                    return word;
                case "реквием":
                    word.Type = TokenType.LOOP;
                    return word;

                case "начертать":
                    word.Type = TokenType.WORD_PRINT;
                    return word;
                case "епистолия":
                    word.Type = TokenType.WORD_PRINT;
                    return word;

                case "истина":
                    word.Value = true;
                    word.Type = TokenType.WORD_TRUE;
                    return word;
                case "истину":
                    word.Value = true;
                    word.Type = TokenType.WORD_TRUE;
                    return word;
                case "правда":
                    word.Value = true;
                    word.Type = TokenType.WORD_TRUE;
                    return word;
                case "реснота":
                    word.Value = true;
                    word.Type = TokenType.WORD_TRUE;
                    return word;
                case "аминь":
                    word.Value = true;
                    word.Type = TokenType.WORD_TRUE;
                    return word;

                case "ложь":
                    word.Value = false;
                    word.Type = TokenType.WORD_FALSE;
                    return word;

                case "и":
                    word.Type = TokenType.AND;
                    return word;
                case "&&":
                    word.Type = TokenType.AND;
                    return word;

                case "или":
                    word.Type = TokenType.OR;
                    return word;
                case "||":
                    word.Type = TokenType.OR;
                    return word;

                case "больше":
                    word.Type = TokenType.MORE;
                    return word;
                case "паче":
                    word.Type = TokenType.MORE;
                    return word;
                case "вяще":
                    word.Type = TokenType.MORE;
                    return word;

                case "меньше":
                    word.Type = TokenType.LESS;
                    return word;

                case "продолжить":
                    word.Type = TokenType.CONTINUE;
                    return word;
                case "выйти":
                    word.Type = TokenType.BREAK;
                    return word;

                case "вернуть":
                    word.Type = TokenType.RETURN;
                    return word;
                case "воздать":
                    word.Type = TokenType.RETURN;
                    return word;
                case "пояти":
                    word.Type = TokenType.RETURN;
                    return word;
                case "славить":
                    word.Type = TokenType.RETURN;
                    return word;
                case "яти":
                    word.Type = TokenType.RETURN;
                    return word;
                case "чтить":
                    word.Type = TokenType.RETURN;
                    return word;

                case "деяти":
                    word.Type = TokenType.PROCEDURE;
                    return word;
                case "выполнить":
                    word.Type = TokenType.PROCEDURE;
                    return word;
                case "процедура":
                    word.Type = TokenType.PROCEDURE;
                    return word;
                case "процедуру":
                    word.Type = TokenType.PROCEDURE;
                    return word;

                case "лямбда":
                    word.Type = TokenType.LAMBDA;
                    return word;
                case "функция":
                    word.Type = TokenType.LAMBDA;
                    return word;

                case "сейчас":
                    word.Type = TokenType.NOW;
                    return word;
                case "чистка":
                    word.Type = TokenType.CLEAR;
                    return word;
                case "сон":
                    word.Type = TokenType.SLEEP;
                    return word;
                case "русить":
                    word.Type = TokenType.VOVASCRIPT;
                    return word;
                case "точно":
                    word.Type = TokenType.EXACTLY;
                    return word;
                case "заполнить":
                    word.Type = TokenType.FILL;
                    return word;
                case "заполни":
                    word.Type = TokenType.FILL;
                    return word;

                case "который":
                    word.Type = TokenType.WHICH;
                    return word;
                case "которая":
                    word.Type = TokenType.WHICH;
                    return word;
                case "которое":
                    word.Type = TokenType.WHICH;
                    return word;

                case "сын":
                    word.Type = TokenType.SON;
                    return word;
                case "наследует":
                    word.Type = TokenType.SON;
                    return word;
                case "наследок":
                    word.Type = TokenType.SON;
                    return word;
                case "наследник":
                    word.Type = TokenType.SON;
                    return word;
                case "потомок":
                    word.Type = TokenType.SON;
                    return word;

                case "бросить":
                    word.Type = TokenType.THROW;
                    return word;
                case "брось":
                    word.Type = TokenType.THROW;
                    return word;

                case "попробовать":
                    word.Type = TokenType.TRY;
                    return word;
                case "попробуй":
                    word.Type = TokenType.TRY;
                    return word;

                case "поймать":
                    word.Type = TokenType.CATCH;
                    return word;
                case "поймай":
                    word.Type = TokenType.CATCH;
                    return word;

                /*           SQL           */

                case "создать":
                    word.Type = TokenType.CREATE;
                    return word;
                case "бд":
                    word.Type = TokenType.DATABASE;
                    return word;

                case "таблица":
                    word.Type = TokenType.TABLE;
                    return word;
                case "таблицу":
                    word.Type = TokenType.TABLE;
                    return word;
                case "тб":
                    word.Type = TokenType.TABLE;
                    return word;

                case "строчка_":
                    word.Type = TokenType.STROKE;
                    return word;
                case "число_":
                    word.Type = TokenType.NUMBER;
                    return word;
                case "точка_":
                    word.Type = TokenType.FNUMBER;
                    return word;
                case "обманчивость_":
                    word.Type = TokenType.BUL;
                    return word;

                case "все":
                    word.Type = TokenType.ALL;
                    return word;
                case "всё":
                    word.Type = TokenType.ALL;
                    return word;

                case "добавить":
                    word.Type = TokenType.INSERT;
                    return word;
                case "в":
                    word.Type = TokenType.IN;
                    return word;
                case "значения":
                    word.Type = TokenType.VALUES;
                    return word;
                case "колонки":
                    word.Type = TokenType.COLONS;
                    return word;
                case "выбрать":
                    word.Type = TokenType.SELECT;
                    return word;
                case "из":
                    word.Type = TokenType.FROM;
                    return word;

                case "от":
                    word.Type = TokenType.AT;
                    return word;
                case "до":
                    word.Type = TokenType.TILL;
                    return word;
                case "шаг":
                    word.Type = TokenType.STEP;
                    return word;

                case "как":
                    word.Type = TokenType.AS;
                    return word;
                case "где":
                    word.Type = TokenType.WHERE;
                    return word;
                    
                default:
                    word.Type = TokenType.VARIABLE;
                    return word;
            }
        }
    }
}
