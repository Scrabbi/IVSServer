using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WpfApplication1
{
    public class Network
    {
        Random rand = new Random();
        /// <summary>
        /// Ожидание запроса на трансляцию 
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="Port"></param>
        public Socket WaitRequest(string IP, string Port)
        {
            //Локальную конечную точку
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(IP), Convert.ToInt32(Port));
            // Создаем сокет Tcp/Ip
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Связываем сокет с локальной точкой, по которой будем принимать данные
            listenSocket.Bind(ipPoint);
            // Начинаем прослушивание
            listenSocket.Listen(1000);
            return listenSocket.Accept();
        }
        /// <summary>
        /// Бесконечный цикл передачи данных.
        /// </summary>
        /// <param name="handler"></param>
        public void BeginBroadcasting(Socket handler, CancellationToken token, DataOnlineBuilder dataOnlineBuilder, MainWindow MV)
        {
            // Буфер для получаемых данных -- ключа. Получаемые байты
            byte[] key = new byte[1];
            // Готовим ответ. Всего должно 43 байта посылаться в форме массива.
            byte[] dataSent = new byte[43];
            // Заполним Fcurrent1 -- это типа частота от прибора, дающего ток.
            byte[] Fcurrent1 = new byte[4];
            //Заполним для ворго канала все то же. Канал 1 отошлет от 6 до 9 число 
            byte[] Fcurrent2 = new byte[4];

            bool cancel = false;
            while (!cancel)
            {
                // Ловля ошибки или остановки приложения
                token.ThrowIfCancellationRequested();
                try
                {
                    int bytes = handler.Receive(key);// Количество полученных байтов
                }
                catch (System.Net.Sockets.SocketException)
                {
                    cancel = true;
                    key[0] = 0;
                    //MV.Start_Serv.IsEnabled = true; Второстепенный данный поток не может получить доступ к главному
                }


                if (key[0] == 224)
                {
                    // Канал 1 отошлет от 1 до 5 число 
                    Single d1 = (Single)dataOnlineBuilder.currentTok1;
                    //d1 *= 25000; // Хотим в итоге видеть цифры тока
                    Fcurrent1 = BitConverter.GetBytes(d1);
                    //Записываем в посылку Fcurrent1
                    for (int i = 0; i < Fcurrent1.Length; i++)
                    {
                        dataSent[i + 1] = Fcurrent1[i];
                    }
                    //Заполним Power1 -- степень тока
                    dataSent[9] = 10; //1..5*10-8

                    Single d2 = (Single)dataOnlineBuilder.currentTok2;
                    //d2 *= 25000;//Хотим в итоге видеть 11..20*10-8 ток
                    Fcurrent2 = BitConverter.GetBytes(d2);
                    for (int i = 0; i < Fcurrent2.Length; i++)
                    {
                        dataSent[i + 15] = Fcurrent2[i];
                    }
                    //Заполним Power2
                    dataSent[23] = 10;

                    //Послыаем
                    handler.Send(dataSent);
                }
            }
        }

    }
}
