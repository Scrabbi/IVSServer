using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;

namespace WpfApplication1
{
  /// <summary>
  /// Логика взаимодействия для MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
      private CancellationTokenSource _tokenSourse;
      private Network _network;
      public DataOnlineBuilder myDataOnBuilder = new DataOnlineBuilder();
        /// <summary>
        /// Сокет создавайемый, после принятия данных
        /// </summary>
        Socket handler;
        /// <summary>
        /// Экземпляр File_Acts
        /// </summary>
        File_Acts myFileActs = new File_Acts();


    public MainWindow()
    {
      InitializeComponent();


            //Удалить все параметры. Можно заново загружать файл.
            myFileActs.Parameters.Clear();

            myFileActs.Read_File("TOKs_prov.txt");
            //Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog();
            //openFileDialog1.Multiselect = true;
            //if (openFileDialog1.ShowDialog() == true)
            //{
            //    //Лист открывающихся файлов
            //    foreach (var item in openFileDialog1.FileNames)
            //    {
            //        //Считываем файлы
            //        myFileActs.Read_File(item);
            //    }
            //}
    }
    
    
    private async void  Start_Serv_Click(object sender, RoutedEventArgs e)
    {
            _network=new Network();
                //От перекликивания
            Start_Serv.IsEnabled=false;
            
            _tokenSourse=new CancellationTokenSource();
            CancellationToken token=_tokenSourse.Token;

            //Начать процесс транслирования файла с реактивностью
            Task task_1 = Task.Factory.StartNew(() => myDataOnBuilder.DoCurrTok_1( token, 20,myFileActs), token);
            Task task_2 = Task.Factory.StartNew(() => myDataOnBuilder.DoCurrTok_2(token, 20, myFileActs), token);
            //Стоим на прослушивании
            //try
            //{
            string ip=TexBox_IP . Text ;
            string port=TexBox_port . Text ;

            Task<Socket> task_wait = Task<Socket> . Factory . StartNew ( ( ) => _network . WaitRequest (ip, port),token );
            handler= await task_wait;
            
            // Мы дождались клиента, пытающегося с нами соединиться. Получаем ключ -- сообщение. Оно у нас посто одно число "224"
                try 
	                {
                       Task task = Task . Factory . StartNew ( ( ) => _network . BeginBroadcasting ( handler , token, myDataOnBuilder, this) , token );
                        //await task;
	                }
	             catch (Exception ex)
	                {
                        MessageBox.Show(string . Format("Произошла ошибка!!!_, {0}", ex));
	                }
        }

    
  }
}
