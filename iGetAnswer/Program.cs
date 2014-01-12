//####################################
//#     Brutforce 4 INTUIT tests     #
//#         Powered by 0wl           #
//# All rights have been violated ;) #
//####################################

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace iGetAnswer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Использование: iGetAnswer.exe <путь к файлу курса *.pm>\n");
            }

            string rawCourse;
            using (var reader = new StreamReader(args[0], Encoding.Default, true))
            {
                rawCourse = reader.ReadToEnd();
            }

            string line1 = "1. Получить список вопросов по номеру лекции\n";
            string line2 = "2. Получить правильные ответы по идентификатору вопроса\n";
            string line3 = "Выход: q/Q\n\n";
            string line4 = ">";

            AnswerBruter bruter = new AnswerBruter(rawCourse);
            var l = bruter.GetTrueAnswers(16544);
            string choice = "";
            do
            {
                Console.Write(line1 + line2 + line3 + line4);

                choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("№ лекции:");
                        int lectureNum = Convert.ToInt32(Console.ReadLine());

                        Hashtable TaskList = bruter.GetTaskList(lectureNum);
                        foreach (DictionaryEntry item in TaskList)
                        {
                            Console.WriteLine("({0}) {1}", item.Key, item.Value);
                        }

                        Console.WriteLine("\n");
                        break;
                    case "2":
                        Console.Write("ID вопроса:");
                        int taskID = Convert.ToInt32(Console.ReadLine());

                        Hashtable TrueAnswerList = bruter.GetTrueAnswers(taskID);
                        foreach (DictionaryEntry item in TrueAnswerList)
                        {
                            Console.WriteLine("({0}) {1}", item.Key, item.Value);
                        }

                        Console.WriteLine("\n");
                        break;
                    default:
                        break;

                }
            }
            while (choice.ToLower() != "q");          
        }
    }
}
