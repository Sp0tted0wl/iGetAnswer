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
using System.Security.Cryptography;
using System.Collections;
using System.Text.RegularExpressions;

namespace iGetAnswer
{
    class AnswerBruter
    {
        private string _courseFileText { get; set; }
        
        private AnswerBruter() {}
        public AnswerBruter(string courseFileText)
        {
            _courseFileText = courseFileText;
        }

        /// <summary>
        /// Возвращает таблицу правильных ответов по заданному идентификатору задачи
        /// которая содержит идентификатор правильного ответа и его текст
        /// </summary>
        /// <param name="TaskID">Идентификатор вопроса</param>
        /// <returns></returns>
        public Hashtable GetTrueAnswers(int TaskID)
        {
            List<Answers> answers = Brute(TaskID);
            Hashtable TrueAnswers = new Hashtable();

            foreach (Answers item in answers)
            {
                TrueAnswers.Add(item.AID, item.AnswerText);
            }

            return TrueAnswers;
        }

        /// <summary>
        /// Возвращает таблицу вопросов по заданному номеру лекции
        /// </summary>
        /// <param name="LectureNum">Номер лекции</param>
        /// <returns></returns>
        public Hashtable GetTaskList(int LectureNum)
        {
            int LectureID = GetLectureID(LectureNum);
            Lectures Lecture = GetLectureFromCourse(LectureID);
            List<Tasks> LectureTasks = GetTasksFromLecture(Lecture.TasksRaw);

            Hashtable TaskList = new Hashtable();

            foreach (var item in LectureTasks)
            {
                TaskList.Add(item.TID, item.TaskText);
            }

            return TaskList;
        }

        private int GetLectureID(int LectureNum)
        {

            MatchCollection lectureHash = Regex.Matches(_courseFileText, @"(?<lnum>\d+)\s=>\s(?<lid>\d+)",
                                                                         RegexOptions.Singleline);

            foreach (Match item in lectureHash)
            {
                int CurrentNum = Convert.ToInt32(item.Groups["lnum"].Value);

                if (CurrentNum == LectureNum)
                {
                    return Convert.ToInt32(item.Groups["lid"].Value);
                }
            }

            return 0;
        }

        private Lectures GetLectureFromCourse(int LectureID)
        {
            string pattern = @"(?<lid>";
            pattern += LectureID.ToString();
            pattern += @")\s=>\s(?<tasks>\[.*?(?:\],){7})";

            Match lectures = Regex.Match(_courseFileText, pattern, RegexOptions.Singleline);

            Lectures CurrentLecture = new Lectures();

            CurrentLecture.LID = Convert.ToInt32(lectures.Groups["lid"].Value);
            CurrentLecture.TasksRaw = lectures.Groups["tasks"].Value;

            return CurrentLecture;
        }

        private List<Tasks> GetTasksFromLecture(string RawTasks)
        {
            MatchCollection tasks = Regex.Matches(RawTasks, @"\['(?<tid>\d+)','\d+'(?:,'\d'){2},""(?<ttext>.*?)"",""(?<ahash>[a-f0-9]*)"",""[a-f0-9]*"",(?<avars>\[{2}.*?(?:,\]){2})", 
                                                             RegexOptions.Singleline);

            List<Tasks> TaskList = new List<Tasks>();

            foreach (Match item in tasks)
            {
                Tasks NewTask = new Tasks();

                NewTask.TID = Convert.ToInt32(item.Groups["tid"].Value);
                NewTask.TaskText = item.Groups["ttext"].Value.Trim();
                NewTask.TrueAnswerHash = item.Groups["ahash"].Value.Trim();
                NewTask.AnswerVariantsRaw = item.Groups["avars"].Value;
                
                TaskList.Add(NewTask);
            }

            return TaskList;
        }

        private Tasks GetTaskFromCourse(int TaskID)
        {
            string pattern = @"\['(?<tid>";
            pattern += TaskID.ToString();
            pattern += @")','\d+'(?:,'\d'){2},""(?<ttext>.*?)"",""";
            pattern += @"(?<ahash>[a-f0-9]*)"",""[a-f0-9]*"",(?<avars>\[{2}.*?(?:,\]){2})";

            Match task = Regex.Match(_courseFileText, pattern, RegexOptions.Singleline);

            Tasks CurrentTask = new Tasks();
            CurrentTask.TID = Convert.ToInt32(task.Groups["tid"].Value);
            CurrentTask.TaskText = task.Groups["ttext"].Value.Trim();
            CurrentTask.TrueAnswerHash = task.Groups["ahash"].Value.Trim();
            CurrentTask.AnswerVariantsRaw = task.Groups["avars"].Value;

            return CurrentTask;
        }
        
        private List<Answers> GetAnswersFromTask(string RawVariants)
        {
            MatchCollection answers = Regex.Matches(RawVariants, @"'(?<aid>\d+)'.*?""(?<atext>.*?)""", RegexOptions.Singleline);

            List<Answers> AnswerList = new List<Answers>();

            foreach (Match item in answers)
            {
                Answers NewAnswer = new Answers();

                NewAnswer.AID = Convert.ToInt32(item.Groups["aid"].Value);
                NewAnswer.AnswerText = item.Groups["atext"].Value.Trim();

                AnswerList.Add(NewAnswer);
            }

            return AnswerList;
        }

        private string GetMD5Hash(string text)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.Default.GetBytes(text));

                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                    sBuilder.Append(data[i].ToString("x2"));

                return sBuilder.ToString();
            }
        }

        private List<Answers> Brute(int TaskID)
        {
            Tasks task = GetTaskFromCourse(TaskID);
            List<Answers> answers = GetAnswersFromTask(task.AnswerVariantsRaw);
            
            int SetPower = (int)Math.Pow(2, answers.Count);

            for (int i = 1; i < SetPower; i++)
            {
                string aStr = "";
                answers.ForEach(x => x.Valid = false);

                for (int j = 0; j < answers.Count; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        aStr += "*a*" + answers[j].AID;
                        answers[j].Valid = true;
                    }
                }
                string answerString = "asdc*a*" + task.TID + aStr;

                if (GetMD5Hash(answerString) == task.TrueAnswerHash)
                {
                    return answers.Where(w => w.Valid == true).ToList();
                }
            }

            return new List<Answers>();
        }

        #region Вспомогательные типы
        private class Lectures
        {
            /// <summary>
            /// Идентификатор лекции
            /// </summary>
            public int LID { get; set; }

            public string TasksRaw { get; set; }
        }

        private class Tasks
        {
            /// <summary>
            /// Идентификатор вопроса
            /// </summary>
            public int TID { get; set; }

            public string TaskText { get; set; }

            public string TrueAnswerHash { get; set; }

            public string AnswerVariantsRaw { get; set; }
        }

        private class Answers
        {
            /// <summary>
            /// Идентификатор варианта ответа
            /// </summary>
            public int AID { get; set; }

            public string AnswerText { get; set; }

            /// <summary>
            /// Признак правильного ответа
            /// </summary>
            public bool Valid { get; set; }
        }
        #endregion
    }
}
