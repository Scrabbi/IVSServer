using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace WpfApplication1
{
    public  class DataOnlineBuilder
    {
        public  Single currentTok1;
        public Single currentTok2;

        public  void DoCurrTok_1(CancellationToken token, int milisecond, File_Acts file_Acts)
        {
            int i= 1;
            Parameter J1 = file_Acts.Find_Parametr("J1");
            while (!token.IsCancellationRequested)
            {
                
                //currentTok = (Single) Math.Sin(argument);
                currentTok1 = (Single) ((1e+10)* J1.Time_and_Value_List[i].Value)*25000;
                i+=1;
                int delTime = (int)J1.Time_and_Value_List[i].Time.Subtract(J1.Time_and_Value_List[i - 1].Time).TotalMilliseconds;
                Thread.Sleep(delTime);
            }
        }
        public  void DoCurrTok_2(CancellationToken token, int milisecond, File_Acts file_Acts)
        {
            int i = 0;
            Parameter J2 = file_Acts.Find_Parametr("J2");
            while (!token.IsCancellationRequested)
            {

                //currentTok = (Single) Math.Sin(argument);
                currentTok2 = (Single) ((1e+10) * J2.Time_and_Value_List[i].Value)*25000;
                i+=1;
                int delTime = (int)J2.Time_and_Value_List[i].Time.Subtract(J2.Time_and_Value_List[i - 1].Time).TotalMilliseconds;
                Thread.Sleep(delTime);
            }
        }
    }
}
