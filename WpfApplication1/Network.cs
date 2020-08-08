using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Threading;
using System . Net;
using System . Net . Sockets;

namespace WpfApplication1
{
    public  class Network
    {
        /// <summary>
        /// Ожидание запроса на трансляцию 
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="Port"></param>
        public  Socket WaitRequest ( string IP , string Port )
        {
            //Локальную конечную точку
            IPEndPoint ipPoint = new IPEndPoint ( IPAddress . Parse ( IP ) , Convert . ToInt32 ( Port ) );
            // Создаем сокет Tcp/Ip
            Socket listenSocket = new Socket ( AddressFamily . InterNetwork , SocketType . Stream , ProtocolType . Tcp );
            // Связываем сокет с локальной точкой, по которой будем принимать данные
            listenSocket .Bind ( ipPoint );
            // Начинаем прослушивание
            listenSocket . Listen (1000 );   
           return listenSocket . Accept ( ) ;
        }
        /// <summary>
        /// Бесконечный цикл передачи данных.
        /// </summary>
        /// <param name="handler"></param>
        public  void BeginBroadcasting ( Socket handler , CancellationToken token )
        {
            byte[] key = new byte [ 1 ]; // Буфер для получаемых данных -- ключа. Получаемые байты
            int bytes = handler . Receive ( key );// Количество полученных байтов

            if ( key [ 0 ] == 224 )
            while(true)
            { 
                        // Ловля ошибки или остановки приложения
                     token . ThrowIfCancellationRequested ( );
                        // Готовим ответ. Всего должно 43 байта посылаться в форме массива.
                    byte[] dataSent = new byte [ 43] ;
                        // Заполним Fcurrent1 -- это типа частота от прибора, дающего ток.
                    byte[] Fcurrent1 = new byte [ 4 ];

                        // Канал 1 отошлет от 1 до 5 число 
                    Random rand = new Random ( );
                    Single d1 = rand . Next ( 1 , 5 );
                    d1 *=  25000; // Хотим в итоге видеть цифры тока
                    Fcurrent1 = BitConverter . GetBytes ( d1 );
                    //Записываем в посылку Fcurrent1
                    for ( int i = 0 ; i < Fcurrent1 . Length ; i++ )
                    {
                        dataSent [ i + 1 ] = Fcurrent1 [ i ];
                    }
                        //Заполним Power1 -- степень тока

                    dataSent [ 9 ] = 8; //1..5*10-8


                         //Заполним для ворго канала все то же. Канал 1 отошлет от 6 до 9 число 
                    byte[] Fcurrent2 = new byte [ 4 ];
                    Single d2 = rand . Next ( 6 , 9 );
                    d2 *=  25000;//Хотим в итоге видеть 11..20*10-8 ток
                    Fcurrent2 = BitConverter . GetBytes ( d2 );
                    for ( int i = 0 ; i < Fcurrent2 . Length ; i++ )
                    {
                        dataSent [ i + 15 ] = Fcurrent2 [ i ];
                    }
                        //Заполним Power2
                    dataSent [ 23 ] = 8;

                        //Послыаем

                    handler . Send ( dataSent );
                            //Задержка отправки
                    //Thread . Sleep ( 1000 );
            }
        }
        
    }
}
