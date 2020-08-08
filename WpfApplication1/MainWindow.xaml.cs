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
    /// <summary>
    /// Сокет создавайемый, после принятия данных
    /// </summary>
    Socket handler;
    public MainWindow()
    {
      InitializeComponent();
    }
    
    
    private async void  Start_Serv_Click(object sender, RoutedEventArgs e)
    {
            _network=new Network();
                //От перекликивания
            Start_Serv.IsEnabled=false;
            
            _tokenSourse=new CancellationTokenSource();
            CancellationToken token=_tokenSourse.Token;

                              //Стоим на прослушивании
            //try
            //{
                string ip=TexBox_IP . Text ;
                string port=TexBox_port . Text ;

                Task<Socket> task_wait = Task<Socket> . Factory . StartNew ( ( ) => _network . WaitRequest ( ip, port) );
                handler= await task_wait;
            //}
            //catch ( Exception ex )
            //{
                //    MessageBox.Show(string . Format ( "ПРоизошла ошибка: {0}" , ex . Message ));
            //}
            // Мы дождались клиента, пытающегося с нами соединиться. Получаем ключ -- сообщение. Оно у нас посто одно число "224"
                try 
	                {
                       Task task = Task . Factory . StartNew ( ( ) => _network . BeginBroadcasting ( handler , token ) , token );
                        await task;
	                }
	             catch (Exception ex)
	                {
                        MessageBox.Show(string . Format("Произошла ошибка!!!_, {0}", ex));
	                }
        }

    private void End_Serv_Click(object sender, RoutedEventArgs e)
    {
        if ( handler!=null )
        {
            _tokenSourse . Cancel ( );
            handler . Shutdown ( SocketShutdown . Both );
            handler . Close ( );
            Start_Serv . IsEnabled=true;
        }
        else
        MessageBox.Show("Закрывать нечего, не было принятых данных");
   }
  }
}
