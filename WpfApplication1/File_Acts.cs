using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace WpfApplication1
{
    /// <summary>
    /// Пара время-значение. Или одна точка на графике для параметра. Логика: создаем структуру для хранения точки, затем создадим список точек.
    /// Список точек, информацию о параметре в классе храним.
    /// </summary>
    public struct Time_and_Value
    {
        private DateTime time;
        /// <summary>
        /// Время точки.
        /// </summary>
        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }
        // public DateTime Time { get; set; }
        /// <summary>
        /// Значение точки.
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// Признак достоверности
        /// </summary>
        public string IsOk { get; set; }
    }
    /// <summary>
    /// Список точек, информацию о параметре в классе храним. 
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Список всех для параметра струтур Time_and_Value – времени и значения (список всех точек данных параметра).
        /// </summary>
        public List<Time_and_Value> Time_and_Value_List = new List<Time_and_Value>();
        /// <summary>
        /// KKS параметра. 
        /// </summary>
        public string KKS { get; set; }
        /// <summary>
        /// Описание параметра. 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Размерность. 
        /// </summary>
        public string Dimention { get; set; }
        /// <summary>
        /// Дополнительная информация (к примеру, обозначить где какие группы и реактивности). 
        /// </summary>
        public string Add_Info { get; set; }
    }
    /// <summary>
    /// Хранит имена открытых файлов
    /// </summary>
    public class OneFile
    {
        /// <summary>
        /// Имя файла
        /// </summary>
        public string filename;
        public List<Parameter> ParametersOF = new List<Parameter>();
    }

    /// <summary>
    /// File_Acts – это действия с файлом. Файл у нас – файл данных, снятый, например, с АПИКа после испытаний.
    /// Содержит список параметров, public методы работы с данными из файла. 
    /// </summary>
    public class File_Acts
    {
        //  /// <summary>
        //  /// Для сдвига по времени организовал такой отдельный список.
        //  /// </summary>
        //public List<OneFile> ListFiles = new List<OneFile>();
        /// <summary>
        /// Прочитался ли файл
        /// </summary>
        public string read_satus = "no";
        /// <summary>
        /// Список классов Parameter. (Список параметров с данными их.)
        /// </summary>
        public List<Parameter> Parameters = new List<Parameter>();

        /// <summary>
        /// Считать файл (создать список Parameters).
        /// </summary>
        /// <param name="File_Full_Name">имя файла, включая полный путь</param>
        public void Read_File(string File_Full_Name)
        {
            //1
            if (File_Full_Name.Contains("apik"))
            {
                //try
                //{
                Parameters.AddRange(this.Load_Apik(File_Full_Name));
                return;
                // }
                // catch (Exception) { MessageBox.Show("Формат данных не соответствует файлу АПИК!"); return; }
            }

            //2
            if (File_Full_Name.Contains("prov"))
            {
                try
                {
                    Parameters.AddRange(this.Load_PROV(File_Full_Name));
                    return;
                }
                catch (Exception) { MessageBox.Show("Формат данных не соответствует файлу PROV!"); return; }
            }










            //
            try
            {
                bool reable;//Нормально ли читается
                Parameters.AddRange(this.Load_SvbuNVAES(File_Full_Name, out reable));
                if (reable)
                    return;
            }
            catch (Exception) { };
            try
            {
                bool reable;
                Parameters.AddRange(this.Load_SvbuNVAES_7(File_Full_Name, out reable));
                if (reable)
                    return;
            }
            catch (Exception) { };
            try
            {
                Parameters.AddRange(this.Load_SvrkNvaes(File_Full_Name));
                return;
            }
            catch (Exception) { };
            try
            {
                Parameters.AddRange(this.Load_AOP_Sum_Nvaes(File_Full_Name));
                return;
            }
            catch (Exception) { };
            try
            {
                Parameters.AddRange(this.Load_AOP_Nvaes(File_Full_Name));
                return;
            }
            catch (Exception) { };
            try
            {
                Parameters.AddRange(this.Load_SVRKtxt_Nvaes(File_Full_Name));
                return;
            }
            catch (Exception) { };
            try
            {
                Parameters.AddRange(this.Load_CSV(File_Full_Name));
                return;
            }
            catch (Exception) { };
            try
            {
                Parameters.AddRange(this.Load_Korona(File_Full_Name));
                return;
            }
            catch (Exception) { };
            //try
            //{
            Parameters.AddRange(this.Load_ProgFile(File_Full_Name));
            return;
            //}
            //catch ( Exception ) {} ;


            MessageBox.Show("Чтоб открыть файл АПИК в имя файла данных добавьте\n" + "APIK\n" +
                "Чтоб открыть файл для программы PROV в имя файла данных добавьте\n" + "PROV\n" +
                "\nДозагрузить обозначения параметров СВРК: для этого снимите с СВРК файл списка параметров");
        }



        /// <summary>
        /// Метод, производящий обработку файла данных, снятого с АПИК НВАЭС-6
        /// </summary>
        private List<Parameter> Load_Apik(string File_Full_Name)
        {
            //Список считываемых параметров. Для заполнения
            List<Parameter> Parameters0 = new List<Parameter>();

            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead = new StreamReader(File_Full_Name, Encoding.GetEncoding("Windows-1251"));
            //Список. Хранит разбитую линию файла. А первая строка – имена параметров.
            List<string> File_line_Slices = new List<string>();
            //Считать линию из файла.
            string File_line = FileRead.ReadLine();
            //Записываем разбитую линию в список по кускам. Это имена параметров
            File_line_Slices = File_line.Split('\t').ToList();

            //==============================Этап 1. Составить список параметров.=====================================
            //Записываем все параметры в список. Время в этот список не записываем. А элементы 0 , 1 есть время в АПИКе.
            for (int i = 2; i < File_line_Slices.Count(); i++)
            {
                //Заполняемый параметр
                Parameter param = new Parameter();
                //Пишем в KKS все полностью о параметре
                param.KKS = File_line_Slices[i];
                param.Description = " ";
                param.Dimention = "-";
                Parameters0.Add(param);
            }

            //========================Первую строку с названиями считали. ЭТАП 2=================

            //Массив временный, хранящий значения параметров в строке.
            double[] tempdouble = new double[File_line_Slices.Count];
            //Считываем файл. Признак конца файла – пустая строка.
            File_line = FileRead.ReadLine();
            while (File_line != null && File_line.Length > 5)
            {
                tempdouble = CustomConvert(File_line).Split('\t').Select(n => double.Parse(n)).ToArray();
                for (int i = 2; i < tempdouble.Length; i++)
                {
                    Time_and_Value TAD = new Time_and_Value();
                    TAD.Time = DateTime.FromOADate(tempdouble[0]);
                    TAD.Value = tempdouble[i];
                    Parameters0[i - 2].Time_and_Value_List.Add(TAD);
                }
                //Линия из файла очередная.
                File_line = FileRead.ReadLine();
            }

            //Закрытие потока
            FileRead.Close();
            //ДОБАВЛЕНИЕ

            foreach (Parameter item in Parameters)
            {
                foreach (Parameter item0 in Parameters0)
                {
                    if (item.KKS == item0.KKS)
                    {
                        item0.KKS += "_Copy";
                    }
                }
            }
            return Parameters0;
        }

        /// <summary>
        /// Метод, производящий обработку файла данных, записанного для PROV
        /// </summary>
        private List<Parameter> Load_PROV(string File_Full_Name)
        {
            //Список считываемых параметров. Для заполнения
            List<Parameter> Parameters0 = new List<Parameter>();

            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead = new StreamReader(File_Full_Name);
            //Список. Хранит разбитую линию файла. А первая строка – имена параметров.
            List<string> File_line_Slices = new List<string>();
            //Считать линию из файла.
            string File_line = FileRead.ReadLine();
            //Записываем разбитую линию в список по кускам. Это имена параметров
            File_line_Slices = File_line.Split('\t').ToList();

            //==============================Этап 1. Составить список параметров.=====================================
            //Записываем все параметры в список. Время в этот список не записываем. А элемент 0 есть время в PROV-файле.
            for (int i = 1; i < File_line_Slices.Count(); i++)
            {
                //Заполняемый параметр
                Parameter param = new Parameter();
                //Пишем в KKS все полностью о параметре
                param.KKS = File_line_Slices[i];
                param.Description = " ";
                param.Dimention = "-";
                Parameters0.Add(param);
            }

            //========================Первую строку с названиями считали. ЭТАП 2=================

            //Массив временный, хранящий значения параметров в строке.
            double[] tempdouble = new double[File_line_Slices.Count];
            //Начало отсчета времени
            DateTime firstDate = new DateTime(2000, 01, 01);
            //Считываем файл. Признак конца файла – пустая строка.
            File_line = FileRead.ReadLine();
            while (File_line != null && File_line.Length > 5)
            {
                string[] tempdouble_string = CustomConvert(File_line).Split('\t');
                tempdouble_string[tempdouble_string.Length - 1] = "0";

                tempdouble = tempdouble_string.Select(n => double.Parse(n)).ToArray();
                for (int i = 1; i < tempdouble.Length; i++)
                {
                    Time_and_Value TAD = new Time_and_Value();
                    //TAD.Time = DateTime.FromOADate(tempdouble[0]);
                    TimeSpan time = TimeSpan.FromSeconds(tempdouble[0]);
                    TAD.Time = firstDate + time;
                    TAD.Value = tempdouble[i];
                    Parameters0[i - 1].Time_and_Value_List.Add(TAD);
                }
                //Линия из файла очередная.
                File_line = FileRead.ReadLine();
            }

            //Закрытие потока
            FileRead.Close();
            //ДОБАВЛЕНИЕ

            foreach (Parameter item in Parameters)
            {
                foreach (Parameter item0 in Parameters0)
                {
                    if (item.KKS == item0.KKS)
                    {
                        item0.KKS += "_Copy";
                    }
                }
            }
            return Parameters0;
        }







        /// <summary>
        /// Метод, производящий обработку файла данных, записанного прогой
        /// </summary>
        private List<Parameter> Load_ProgFile(string File_Full_Name)
        {
            List<Parameter> Parameters0 = new List<Parameter>();

            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead = new StreamReader(File_Full_Name, Encoding.GetEncoding("Windows-1251"));
            //Список. Хранит разбитую линию файла. А первая строка в АПИКе Бушерском – заголовки к параметрам.
            List<string> File_line_Slices = new List<string>();
            //Считать линию из файла.
            string File_line = FileRead.ReadLine();
            //Записываем разбитую линию в список по кускам.
            File_line_Slices = File_line.Split('\t').ToList();

            //==============================Этап 1. Составить список параметров.=====================================
            //Записываем все параметры в список. Время в этот список не записываем. А элемент 0  есть время в АПИКе.
            for (int i = 1; i < File_line_Slices.Count(); i++)
            {
                //Записываемый параметр
                Parameter param = new Parameter();
                //Название так выглядит: Номер+ +KKS+' '+описание'Бывает параметр без KKS.
                param.KKS = File_line_Slices[i];
                param.Description = " ";
                param.Dimention = "-";
                //if (File_line_Slices[i].Contains('|'))
                //{
                //  string[] slice_i = File_line_Slices[i].Split('|').ToArray();
                //  param.KKS = slice_i[0];
                //  param.Description = slice_i[1];
                //  param.Dimention = "-";
                //}
                Parameters0.Add(param);
            }
            //Первую строку с названиями считали!
            //========================ЭТАП 2=================

            //Массив временный, хранящий значения параметров в строке.
            double[] tempdouble = new double[File_line_Slices.Count];
            //Считываем файл. Записываем в List параметры (создаем Parametrs). Признак конца файла – пустая строка.
            File_line = FileRead.ReadLine();
            while (File_line != null && File_line.Length > 5)
            {

                if (File_line != null)
                {
                    string[] splitMassString = CustomConvert(File_line).Split('\t');
                    for (int i = 0; i < splitMassString.Count(); i++)
                    {
                        double slise;//Кусочек в доубл
                        bool isParsing = double.TryParse(splitMassString[i], out slise);
                        if (isParsing)
                            tempdouble[i] = slise;
                        else
                            tempdouble[i] = 0;
                    }
                    //ПРоход по массиву double из строки
                    for (int i = 1; i < tempdouble.Length; i++)
                    {
                        Time_and_Value TAD = new Time_and_Value();
                        int seconds = (int)tempdouble[0];
                        TimeSpan fileTime = new TimeSpan(0, 0, 0, seconds, (int)(tempdouble[0] - seconds) * 1000);
                        TAD.Time = DateTime.FromOADate(0) + fileTime;
                        TAD.Value = tempdouble[i];
                        Parameters0[i - 1].Time_and_Value_List.Add(TAD);
                    }
                }
                //Линия из файла очередная.
                File_line = FileRead.ReadLine();
            }
            //Закрытие потока
            FileRead.Close();
            //ДОБАВЛЕНИЕ

            foreach (Parameter item in Parameters)
            {
                foreach (Parameter item0 in Parameters0)
                {
                    if (item.KKS == item0.KKS)
                    {
                        item0.KKS += "_Copy";
                    }
                }
            }
            return Parameters0;

        }


        /// <summary>
        /// Метод, производящий обработку файла данных, снятого с СВБУ НВАЭС-6
        /// </summary>
        private List<Parameter> Load_SvbuNVAES(string File_Full_Name, out bool Readble)
        {
            List<Parameter> Parameters0 = new List<Parameter>();
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead = new StreamReader(File_Full_Name);
            //Список. Хранит разбитую линию файла. 
            List<string> File_line_Slices = new List<string>();
            //Линия файла
            string File_line = "";

            //Переходим к данным в файле
            int j = 0;
            while (j < 5)
            {
                File_line = FileRead.ReadLine();
                j++;
            }
            //Линиия разбивается на кусочки.
            File_line_Slices = File_line.Split('\t').ToList();
            //Если встретили время первый раз, составим список параметров
            DateTime MyTime;
            if (DateTime.TryParse(File_line_Slices[0], out MyTime))
            {
                Parameter param = new Parameter();
                param.KKS = File_line_Slices[1];
                param.Description = File_line_Slices[5];
                param.Dimention = File_line_Slices[3];
                Parameters0.Add(param);
                //Переходим на след. строку, дозаполняем список параметров
                File_line = FileRead.ReadLine();
                while (!DateTime.TryParse(File_line.Split('\t').ToList()[0], out MyTime))
                {
                    Parameter paramAdd = new Parameter();
                    paramAdd.KKS = File_line.Split('\t').ToList()[1];
                    paramAdd.Description = File_line.Split('\t').ToList()[5];
                    paramAdd.Dimention = File_line.Split('\t').ToList()[3];
                    Parameters0.Add(paramAdd);

                    File_line = FileRead.ReadLine();
                }
            }
            //Закрытие потока
            FileRead.Close();

            //Заполняем значения
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead2 = new StreamReader(File_Full_Name);
            //Линия файла
            string File_line2 = FileRead2.ReadLine();
            //Читаем все строки файла
            while (File_line2 != null)
            {
                DateTime CurrTime;
                if (DateTime.TryParse(File_line2.Split('\t').ToList()[0], out CurrTime))
                {
                    //К параметрам подбираем значение
                    int paramcount = 0;

                    paramcount = Parameters.Count;

                    paramcount = Parameters0.Count;
                    for (int i = 0; i < paramcount; i++)
                    {
                        Time_and_Value TAD = new Time_and_Value();
                        TAD.Time = CurrTime;
                        TAD.Value = double.Parse(CustomConvert(File_line2.Split('\t').ToList()[2]));
                        TAD.IsOk = File_line2.Split('\t').ToList()[4];
                        Parameters0[i].Time_and_Value_List.Add(TAD);
                        //Переход на строку ниже, столько раз сколько параметров.
                        File_line2 = FileRead2.ReadLine();
                    }
                }
                File_line2 = FileRead2.ReadLine();
            }
            FileRead2.Close();

            foreach (Parameter item in Parameters)
            {
                foreach (Parameter item0 in Parameters0)
                {
                    if (item.KKS == item0.KKS)
                    {
                        item0.KKS += "SvbuNVAES_Copy";
                    }
                }
            }
            Readble = false;
            if (Parameters0.Count > 0)
                Readble = true;
            return Parameters0;

        }

        /// <summary>
        /// Метод, производящий обработку файла данных, снятого с СВБУ НВАЭС-7
        /// </summary>
        private List<Parameter> Load_SvbuNVAES_7(string File_Full_Name, out bool Readble)
        {
            //Создаем потому что  добавлять будем файл. 
            List<Parameter> Parameters0 = new List<Parameter>();
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead = new StreamReader(File_Full_Name, Encoding.UTF8);
            //Список. Хранит разбитую линию файла. 
            List<string> File_line_Slices = new List<string>();
            //Линия файла. 
            string File_line = FileRead.ReadLine();

            //===============================Этап 1: составим список параметоров.  ========================================      
            //Переходим к данным в файле
            int j = 0;
            while (j < 5)
            {
                File_line = FileRead.ReadLine();
                j++;
            }

            //Список параметров
            while (File_line != null)
            {
                //Линиия разбивается на кусочки.
                File_line_Slices = File_line.Split('\t').ToList();

                //ПРоверим, имеется ли уже параметр
                bool isIn = false;
                foreach (Parameter item in Parameters0)
                {
                    if (item.KKS == File_line_Slices[1])
                    {
                        isIn = true;
                    }
                }
                //Если не было , добавим
                if (!isIn)
                {
                    Parameters0.Add(new Parameter { KKS = File_line_Slices[1], Description = File_line_Slices[5], Dimention = File_line_Slices[3] });
                }

                File_line = FileRead.ReadLine();
            }

            //Закрытие потока
            FileRead.Close();

            //--------------------------------Этап 2. К параметрам добавим значения.===============================
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead2 = new StreamReader(File_Full_Name, Encoding.UTF8);
            //Линия файла. Переходим на 5.
            string File_line2 = FileRead2.ReadLine();

            int f = 0;
            while (f < 4)
            {
                File_line2 = FileRead2.ReadLine();
                f++;
            }

            //Читаем все строки файла
            while (File_line2 != null)
            {
                //Линиия разбивается на кусочки.
                File_line_Slices = File_line2.Split('\t').ToList();

                //К параметрам--время.
                foreach (Parameter item in Parameters0)
                {
                    if (item.KKS == File_line_Slices[1])
                    {

                        List<string> Str = File_line_Slices[0].Split(' ').ToList();
                        //Время получаем
                        DateTime Dt = DateTime.Parse(Str[0]) + TimeSpan.Parse(CustomConvert(Str[1]));



                        item.Time_and_Value_List.Add(new Time_and_Value { Time = Dt, Value = Convert.ToDouble(CustomConvert(File_line_Slices[2])), IsOk = File_line_Slices[4] });
                    }
                }



                //К сделующей строке
                File_line2 = FileRead2.ReadLine();
            }
            FileRead2.Close();


            foreach (Parameter item in Parameters)
            {
                foreach (Parameter item0 in Parameters0)
                {
                    if (item.KKS == item0.KKS)
                    {
                        item0.KKS += "AOPNvaes_Copy";
                    }
                }
            }
            Readble = false;
            if (Parameters0.Count > 0)
                Readble = true;
            return Parameters0;


        }

        //Чтение СВРК с НВАЭС-6
        /// <summary>
        /// Метод, производящий обработку файла данных, снятого с СВРК НВАЭС
        /// </summary>
        private List<Parameter> Load_SvrkNvaes(string File_Full_Name)
        {
            List<Parameter> Parameters0 = new List<Parameter>();
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead = new StreamReader(File_Full_Name, Encoding.GetEncoding("Windows-1251"));
            //Список. Хранит разбитую линию файла. Заголовки к параметрам в 19 строке.
            List<string> File_line_Slices = new List<string>();
            //Считать линию 19 из файла. Заводим счетчик и строковую переменную.
            int j = 0;
            string File_line = "";
            while (j < 19)
            {
                File_line = FileRead.ReadLine();
                j++;
            }

            //Записываем разбитую линию в список по кускам. Напоминание: здесь названия параметров.
            File_line_Slices = File_line.Split(';').ToList();
            //Первый элемент не как другие, содержит лишнюю часть до KKS.
            File_line_Slices[0] = File_line_Slices[0].Split('=').ToList()[1];

            //Записываем все параметры в список. "-1" потому что в конце линии пробел стоит, который записался как элемент таблицы.
            for (int i = 0; i < File_line_Slices.Count() - 1; i++)
            {
                //Записываемый параметр
                Parameter param = new Parameter();
                param.KKS = File_line_Slices[i];
                param.Description = "-";
                param.Dimention = "-";
                //Добавляем.
                Parameters0.Add(param);

            }

            //Первую строку файла с названиями считали! Дочитаем до 28 строки, где мы встанем перед началом данных
            int k = 0;
            while (k < 9)
            {
                File_line = FileRead.ReadLine();
                k++;
            }

            //Считываем файл. Записываем в List параметры (создаем Parametrs). Признак конца файла – пустая строка.
            while (File_line != null)
            {
                //Линия из файла очередная.
                File_line = FileRead.ReadLine();
                //Необходимо, т.к. последняя строка с Null будет считана "заранее" с помощью FileRead.
                if (File_line != null)
                {
                    //Как видно по файлу rsa, линию с параметров вначале стоит отделит до знака равно и после:
                    string[] tempdouble0 = File_line.Split('=').ToArray();
                    string[] tempdouble1 = tempdouble0[1].Split(';').ToArray();//.Select(n => double.Parse(n)).ToArray(); 

                    //Length-1 потому как последняя величина в файле -- пробел.
                    for (int i = 1; i < tempdouble1.Length - 1; i++)
                    {
                        Time_and_Value TAD = new Time_and_Value();
                        TAD.Time = DateTime.Parse(tempdouble1[0]);
                        //Теперь элемент массивва , к которому мы пришли, содержит вначале порядковый номер и при 1 встрече ошибочную цифру, так что начиная с конца считаем нужное.
                        string[] tempdouble2 = tempdouble1[i].Split(' ').ToArray();
                        if (Convert.ToInt32(tempdouble2[tempdouble2.Count() - 1]) == 0)
                        {
                            TAD.IsOk = "Yes";
                        }
                        if (Convert.ToInt32(CustomConvert(tempdouble2[tempdouble2.Count() - 1])) > 0)
                        {
                            TAD.IsOk = "Нет, код: " + tempdouble2[tempdouble2.Count() - 1];
                        }

                        TAD.Value = Double.Parse(CustomConvert(tempdouble2[tempdouble2.Count() - 2]));
                        Parameters0[i - 1].Time_and_Value_List.Add(TAD);

                    }
                }
            }
            //Закрытие потока
            FileRead.Close();

            foreach (Parameter item in Parameters)
            {
                foreach (Parameter item0 in Parameters0)
                {
                    if (item.KKS == item0.KKS)
                    {
                        item0.KKS += "_SvrkNvaes_Copy";
                    }
                }
            }
            return Parameters0;

        }

        /// <summary>
        /// Парс АОП , который с нескольких стоек одновременно.
        /// </summary>
        /// <param name="File_Full_Name"></param>
        /// <param name="IsFirst"></param>
        private List<Parameter> Load_AOP_Sum_Nvaes(string File_Full_Name)
        {
            //Создаем потому что может быть и так что добавлять будем файл. 
            List<Parameter> Parameters0 = new List<Parameter>();
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead = new StreamReader(File_Full_Name);
            //Список. Хранит разбитую линию файла. 
            List<string> File_line_Slices = new List<string>();
            //Линия файла. На вторую надо перейти
            string File_line = FileRead.ReadLine();
            File_line = FileRead.ReadLine();

            //Этап 1: составим список параметоров.        
            while (File_line != null)
            {

                //Линиия разбивается на кусочки.
                File_line_Slices = File_line.Split(';').ToList();
                // составим список параметров
                //ПРоверим, имеется ли уже параметр
                bool isIn = false;
                foreach (Parameter item in Parameters0)
                {
                    if (item.KKS == File_line_Slices[2])
                    {
                        isIn = true;
                    }
                }
                //Если не было , добавим
                if (!isIn)
                {
                    Parameters0.Add(new Parameter { KKS = File_line_Slices[2], Description = File_line_Slices[3] });
                }
                ////Если открытие файла, а не добавление еще одного файла, заполним огинальный список.
                //  if (IsFirst)
                //  {
                //    Parameters=Parameters0;
                //  }


                File_line = FileRead.ReadLine();
            }
            //Закрытие потока
            FileRead.Close();

            //--------------------------------Этап 2. К параметрам добавим значения.===============================
            //Заполняем значения
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead2 = new StreamReader(File_Full_Name);
            //Линия файла
            string File_line2 = FileRead2.ReadLine();
            File_line2 = FileRead2.ReadLine();

            //Читаем все строки файла
            while (File_line2 != null)
            {
                //Линиия разбивается на кусочки.
                File_line_Slices = File_line2.Split(';').ToList();
                //Время получаем
                DateTime CurrTime = DateTime.Parse(File_line_Slices[0]) + TimeSpan.Parse(CustomConvert(File_line_Slices[1]));
                //ПО параметрам прошлись
                foreach (Parameter item in Parameters0)
                {
                    if (File_line_Slices[2] == item.KKS)
                    {
                        //В этом файле плохо записаны дискреты
                        double rezpars;
                        if (double.TryParse(CustomConvert(File_line_Slices[4]), out rezpars))
                            item.Time_and_Value_List.Add(new Time_and_Value { Time = CurrTime, Value = rezpars });
                        else
                            //1->0 В файле записано.
                            item.Time_and_Value_List.Add(new Time_and_Value { Time = CurrTime, Value = Convert.ToDouble(File_line_Slices[4].Split('>').ToList()[1]) });
                    }
                }
                //К сделующей строке
                File_line2 = FileRead2.ReadLine();
            }
            FileRead2.Close();


            foreach (Parameter item in Parameters)
            {
                foreach (Parameter item0 in Parameters0)
                {
                    if (item.KKS == item0.KKS)
                    {
                        item0.KKS += "AOPNvaes_Sum_Copy";
                    }
                }
            }
            return Parameters0;


        }

        /// <summary>
        /// Парс АОП , который с програмки СКУатома
        /// </summary>
        /// <param name="File_Full_Name"></param>
        /// <param name="IsFirst"></param>
        private List<Parameter> Load_AOP_Nvaes(string File_Full_Name)
        {
            //Создаем потому что  добавлять будем файл. 
            List<Parameter> Parameters0 = new List<Parameter>();
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead = new StreamReader(File_Full_Name, Encoding.GetEncoding("Windows-1251"));
            //Список. Хранит разбитую линию файла. 
            List<string> File_line_Slices = new List<string>();
            //Линия файла. 
            string File_line = FileRead.ReadLine();

            //===============================Этап 1: составим список параметоров.  ========================================      
            //Линиия разбивается на кусочки.
            File_line_Slices = File_line.Split(' ').ToList();
            // составим список параметров
            for (int i = 2; i < File_line_Slices.Count; i++)
            {

                Parameters0.Add(new Parameter { KKS = File_line_Slices[i], Description = "-", Dimention = "-" });
            }
            //Закрытие потока
            FileRead.Close();

            //--------------------------------Этап 2. К параметрам добавим значения.===============================
            //Заполняем значения
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead2 = new StreamReader(File_Full_Name);
            //Линия файла. Переходим на вторую.
            string File_line2 = FileRead2.ReadLine();
            File_line2 = FileRead2.ReadLine();

            //Читаем все строки файла
            while (File_line2 != null)
            {
                //Линиия разбивается на кусочки.
                File_line_Slices = File_line2.Split(' ').ToList();
                //Время получаем
                DateTime CurrTime = DateTime.Parse(File_line_Slices[0]) + TimeSpan.Parse(CustomConvert(File_line_Slices[1]));
                //Добавляем значения
                for (int i = 2; i < File_line_Slices.Count; i++)
                {
                    Parameters0[i - 2].Time_and_Value_List.Add(new Time_and_Value { Time = CurrTime, Value = Convert.ToDouble(CustomConvert(File_line_Slices[i])) });
                }
                //К сделующей строке
                File_line2 = FileRead2.ReadLine();
            }
            FileRead2.Close();

            foreach (Parameter item in Parameters)
            {
                foreach (Parameter item0 in Parameters0)
                {
                    if (item.KKS == item0.KKS)
                    {
                        item0.KKS += "AOPNvaes_Copy";
                    }
                }
            }
            return Parameters0;
        }


        /// <summary>
        /// Парс СВРК txt
        /// </summary>
        /// <param name="File_Full_Name"></param>
        /// <param name="IsFirst"></param>
        private List<Parameter> Load_SVRKtxt_Nvaes(string File_Full_Name)
        {
            //Создаем потому что  добавлять будем файл. 
            List<Parameter> Parameters0 = new List<Parameter>();
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead = new StreamReader(File_Full_Name, Encoding.GetEncoding("Windows-1251"));
            //Список. Хранит разбитую линию файла. 
            List<string> File_line_Slices = new List<string>();
            //Линия файла. Надо на 5
            string File_line = FileRead.ReadLine();
            for (int i = 0; i < 4; i++)
            {
                File_line = FileRead.ReadLine();
            }



            //===============================Этап 1: составим список параметоров.  ========================================      
            //Линиия разбивается на кусочки.
            File_line_Slices = File_line.Split('|').ToList();
            // составим список параметров
            for (int i = 1; i < File_line_Slices.Count - 1; i++)
            {

                Parameters0.Add(new Parameter { KKS = File_line_Slices[i], Description = "-", Dimention = "-" });
            }
            //Закрытие потока
            FileRead.Close();

            //--------------------------------Этап 2. К параметрам добавим значения.===============================
            //Заполняем значения
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead2 = new StreamReader(File_Full_Name);
            //Линия файла. Переходим на 6-ю.
            string File_line2 = FileRead2.ReadLine();
            for (int i = 0; i < 5; i++)
            {
                File_line2 = FileRead2.ReadLine();
            }

            //Читаем все строки файла
            while (File_line2 != null)
            {
                //Линиия разбивается на кусочки.
                File_line_Slices = File_line2.Split('|').ToList();
                //Время получаем
                DateTime CurrTime = DateTime.Parse(File_line_Slices[0]);
                //Добавляем значения
                for (int i = 1; i < File_line_Slices.Count - 1; i++)
                {
                    //Если дискрет, то вкл выкл обозначается. 1 и 0 заменим. Для этого сии действия:
                    double Value_ = 777;
                    Double.TryParse(CustomConvert(File_line_Slices[i]), out Value_);
                    if (Value_ == 0)
                    {
                        if (File_line_Slices[i] == "Вкл")
                        {
                            Value_ = 1;
                        }
                        if (File_line_Slices[i] == "Выкл")
                        {
                            Value_ = 0;
                        }
                    }

                    Parameters0[i - 1].Time_and_Value_List.Add(new Time_and_Value { Time = CurrTime, Value = Value_ });
                }
                //К сделующей строке
                File_line2 = FileRead2.ReadLine();
            }
            FileRead2.Close();


            //Если повторяющиеся параметры на другой время, к примеру.
            foreach (Parameter item in Parameters)
            {
                foreach (Parameter item0 in Parameters0)
                {
                    if (item.KKS == item0.KKS)
                    {
                        item0.KKS += "AOPNvaes_Copy";
                    }
                }
            }
            return Parameters0;
        }

        /// <summary>
        /// Парс csv
        /// </summary>
        /// <param name="File_Full_Name"></param>
        /// <param name="IsFirst"></param>
        private List<Parameter> Load_CSV(string File_Full_Name)
        {
            //Создаем потому что  добавлять будем файл. 
            List<Parameter> Parameters0 = new List<Parameter>();
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead = new StreamReader(File_Full_Name, Encoding.GetEncoding("Windows-1251"));
            //Список. Хранит разбитую линию файла. 
            List<string> File_line_Slices = new List<string>();

            //Линия файла. Надо на 1, к списку параметров
            string File_line = FileRead.ReadLine();




            //===============================Этап 1: составим список параметоров.  ========================================      
            //Линиия разбивается на кусочки.
            File_line_Slices = File_line.Split(';').ToList();
            // составим список параметров
            for (int i = 1; i < File_line_Slices.Count; i++)
            {

                Parameters0.Add(new Parameter { KKS = File_line_Slices[i], Description = "-", Dimention = "-" });
            }
            //Закрытие потока
            FileRead.Close();

            //--------------------------------Этап 2. К параметрам добавим значения.===============================
            //Заполняем значения
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead2 = new StreamReader(File_Full_Name);
            //Линия файла. Переходим на 2-ю.
            string File_line2 = FileRead2.ReadLine();
            for (int i = 0; i < 2; i++)
            {
                File_line2 = FileRead2.ReadLine();
            }

            //Читаем все строки файла
            while (File_line2 != null)
            {
                //Линиия разбивается на кусочки.
                File_line_Slices = File_line2.Split(';').ToList();
                //Время получаем
                DateTime CurrTime = DateTime.Parse(File_line_Slices[0]);
                //Добавляем значения
                for (int i = 1; i < File_line_Slices.Count; i++)
                {
                    //Если дискрет, то вкл выкл обозначается. 1 и 0 заменим. Для этого сии действия:
                    double Value_ = 777;
                    Double.TryParse(CustomConvert(File_line_Slices[i]), out Value_);
                    //Здесь нуль значит , согласно, методу трайпарс, означает неуспешность преобразования
                    if (Value_ == 0)
                    {
                        if (File_line_Slices[i] == "Вкл")
                        {
                            Value_ = 1;
                        }
                        if (File_line_Slices[i] == "Выкл")
                        {
                            Value_ = 0;
                        }
                    }

                    Parameters0[i - 1].Time_and_Value_List.Add(new Time_and_Value { Time = CurrTime, Value = Value_ });
                }
                //К сделующей строке
                File_line2 = FileRead2.ReadLine();
            }
            FileRead2.Close();


            //Если повторяющиеся параметры на другой время, к примеру.
            foreach (Parameter item in Parameters)
            {
                foreach (Parameter item0 in Parameters0)
                {
                    if (item.KKS == item0.KKS)
                    {
                        item0.KKS += "AOPNvaes_Copy";
                    }
                }
            }
            return Parameters0;
        }


        /// <summary>
        /// Парс короны
        /// </summary>
        /// <param name="File_Full_Name"></param>
        /// <param name="IsFirst"></param>
        private List<Parameter> Load_Korona(string File_Full_Name)
        {
            //Создаем потому что  добавлять будем файл. 
            List<Parameter> Parameters0 = new List<Parameter>();
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead = new StreamReader(File_Full_Name, Encoding.GetEncoding("Windows-1251"));
            //Список. Хранит разбитую линию файла. 
            List<string> File_line_Slices = new List<string>();

            //Линия файла. Надо на 1, к списку параметров
            string File_line = FileRead.ReadLine();




            //===============================Этап 1: составим список параметоров.  ========================================      
            //Линиия разбивается на кусочки.
            File_line_Slices = File_line.Split('\t').ToList();
            // составим список параметров
            for (int i = 1; i < File_line_Slices.Count; i++)
            {

                Parameters0.Add(new Parameter { KKS = File_line_Slices[i], Description = "-", Dimention = "-" });
            }
            //Закрытие потока
            FileRead.Close();

            //--------------------------------Этап 2. К параметрам добавим значения.===============================
            //Заполняем значения
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead2 = new StreamReader(File_Full_Name);
            //Линия файла. Переходим на 2-ю.
            string File_line2 = FileRead2.ReadLine();
            for (int i = 0; i < 2; i++)
            {
                File_line2 = FileRead2.ReadLine();
            }

            //Читаем все строки файла
            while (File_line2 != null)
            {
                //Линиия разбивается на кусочки.
                File_line_Slices = File_line2.Split('\t').ToList();
                //Время получаем
                DateTime CurrTime = DateTime.Parse("10.10.2000 0:00:00.000") + TimeSpan.FromSeconds(Convert.ToDouble(File_line_Slices[0]));
                //Добавляем значения
                for (int i = 1; i < File_line_Slices.Count; i++)
                {
                    //Если дискрет, то вкл выкл обозначается. 1 и 0 заменим. Для этого сии действия:
                    double Value_ = 777;
                    Double.TryParse(CustomConvert(File_line_Slices[i]), out Value_);
                    //Здесь нуль значит , согласно, методу трайпарс, означает неуспешность преобразования
                    if (Value_ == 0)
                    {
                        if (File_line_Slices[i] == "Вкл")
                        {
                            Value_ = 1;
                        }
                        if (File_line_Slices[i] == "Выкл")
                        {
                            Value_ = 0;
                        }
                    }

                    Parameters0[i - 1].Time_and_Value_List.Add(new Time_and_Value { Time = CurrTime, Value = Value_ });
                }
                //К сделующей строке
                File_line2 = FileRead2.ReadLine();
            }
            FileRead2.Close();


            //Если повторяющиеся параметры на другой время, к примеру.
            foreach (Parameter item in Parameters)
            {
                foreach (Parameter item0 in Parameters0)
                {
                    if (item.KKS == item0.KKS)
                    {
                        item0.KKS += "AOPNvaes_Copy";
                    }
                }
            }
            return Parameters0;
        }








        //Чтение фала, создаваемого самой программой
        /// <summary>
        /// Метод, производящий обработку файла данных, создаваемого самой программой
        /// </summary>
        private Parameter Load_PragramFile(string File_Full_Name)
        {

            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead = new StreamReader(File_Full_Name, Encoding.GetEncoding("Windows-1251"));
            //Список. Хранит разбитую линию файла.
            List<string> File_line_Slices = new List<string>();
            //Считать линию 1 из файла. 
            string File_line = "";
            File_line = FileRead.ReadLine();

            //Записываем разбитую линию в список по кускам. Напоминание: здесь названия параметров.
            File_line_Slices = File_line.Split(';').ToList();
            //Записываем параметр в список. 

            Parameter param = new Parameter();
            param.KKS = File_line_Slices[0];
            param.Description = "-";
            param.Dimention = "-";

            //Первую строку файла с названием считали! Дочитаем до 2, где мы встанем перед началом данных
            File_line = FileRead.ReadLine();

            //Считываем файл. Записываем данные. Признак конца файла – пустая строка.
            while (File_line != null)
            {
                string[] tempdouble = File_line.Split(';').ToArray();

                Time_and_Value TAD = new Time_and_Value();
                TAD.Time = DateTime.Parse(tempdouble[0]);
                TAD.IsOk = tempdouble[2];
                TAD.Value = Double.Parse(tempdouble[1]);

                param.Time_and_Value_List.Add(TAD);



                //Линия из файла очередная.
                File_line = FileRead.ReadLine();
            }
            //Закрытие потока
            FileRead.Close();


            foreach (Parameter item in Parameters)
            {

                if (item.KKS == param.KKS)
                {
                    param.KKS += "Prog_Copy";
                }

            }
            return param;

        }


        public void Load_SvrkNvaesNames(string File_Full_Name)
        {
            //Считать файл. Саздаем поток чтения. Применяем правильную кодировку.
            StreamReader FileRead = new StreamReader(File_Full_Name, Encoding.GetEncoding("Windows-1251"));
            //Список. Хранит разбитую линию файла. Интересное нам-- в 6 сроке начинается.
            List<string> File_line_Slices = new List<string>();
            //Перейти к линии 5. Заводим счетчик и строковую переменную.
            int j = 0;
            string File_line = "";
            while (j < 5)
            {
                File_line = FileRead.ReadLine();
                j++;
            }

            //Parameters уже заполнены, тут лишь 2 поля там дописываем: Discription и Demention.
            //Считываем файл. Записываем в List параметры (создаем Parametrs). Признак конца файла – пустая строка.
            while (File_line != null)
            {
                //Линия из файла очередная.
                File_line = FileRead.ReadLine();
                //Необходимо, т.к. последняя строка с Null будет считана "заранее" с помощью FileRead.
                if (File_line != null)
                {
                    //Как видно по файлу rsa, линию с параметров вначале стоит отделит до знака равно и после:
                    string[] tempdouble = File_line.Split('|').ToArray();
                    //.Select(n => double.Parse(n)).ToArray(); 
                    foreach (Parameter item in Parameters)
                    {
                        if (tempdouble[0].Contains(item.KKS))
                        {
                            item.Description = tempdouble[1];
                            item.Dimention = tempdouble[3];
                            continue;
                        }
                    }
                }
            }

            //Закрытие потока
            FileRead.Close();
        }
        /// <summary>
        /// Даст Parameter по его KKS . Так же, по KKS с номером повторно построенных линий этого KKS.
        /// </summary>
        /// <param name="Discription_"></param>
        /// <returns></returns>
        public Parameter Find_Parametr(string Text)
        {
            //Ище как сам KKS, так и KKS с добавленным _номером.
            if (Text.Contains("_"))
            {
                Text = Text.Substring(0, Text.LastIndexOf("_"));
            }
            //ПРостой поиск
            foreach (Parameter one in Parameters)
            {
                if (one.KKS.Contains(Text))
                    return one;
            }
            MessageBox.Show("Была запущена процедура поиска параметра Find_Parametr, но параметр не был обнаружен!");
            return null;
        }
        /// <summary>
        /// Конвектор точек и запятых.
        /// </summary>
        /// <param name="s">Изменяемая строка</param>
        /// <returns>Измененная строка</returns>
        public static string CustomConvert(string s)
        {
            s = s.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            s = s.Replace(",", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            return s;
        }

        ///// <summary>
        ///// Сдвинуть на время
        ///// </summary>
        ///// <param name="Pars"></param>
        ///// <param name="secs"></param>
        //public void ShiftIt(string FileName, int secs)
        //{
        //  //Найти какой из файлов сдвинуть
        //  List<Parameter> Chosen_Par = new List<Parameter>();
        //  foreach (var item in ListFiles)
        //  {
        //    if (item.filename==FileName)
        //    {
        //      Chosen_Par = item.ParametersOF;
        //    }
        //  }
        //  ////Определим, куда сдвигать
        //  //TimeSpan TimeShift= new TimeSpan();
        //  //if (secs>0)
        //  //{
        //  //  TimeShift=TimeSpan.FromSeconds(secs);
        //  //}
        //  //else TimeShift = -TimeSpan.FromSeconds(-secs);
        //  //Сдвинуть
        //  for (int j=0;j< Chosen_Par.Count;j++)
        //  {
        //    for (int i = 0; i < Chosen_Par[j].Time_and_Value_List.Count; i++)
        //       {

        //            //Структуру менять как класс не получится. Ссылки не так работают
        //            Time_and_Value Tad=new Time_and_Value();//Создали
        //            Tad=Chosen_Par[j].Time_and_Value_List[i];//Копия

        //            Tad.Time = Chosen_Par[j].Time_and_Value_List[i].Time + TimeSpan.FromSeconds(secs);//Значение копии, сам СВДИГ
        //            Chosen_Par[j].Time_and_Value_List[i]=Tad;//Запись

        //       }
        //  }
        //  Parameters.Clear();
        //  //Переписали параметры.
        //  foreach (var item in ListFiles)
        //  {
        //    Parameters.AddRange(item.ParametersOF);
        //  }

        //}


    }
}
