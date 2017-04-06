using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RaW5
{
    class Program
    {
        static int iteration = 0, NI = 80;
        static Semaphore e, r, w;
        static int nr, nw, dr , dw;
        static int NR = 3, NW = 2;
        static bool verbose = false;

        static void Main(string[] args)
        {
            e = new Semaphore(1, 1);
            r = new Semaphore(1, 1);
            w = new Semaphore(1, 1);

            r.WaitOne();
            w.WaitOne();
            nr = nw = dr = dw = 0;
            for (int i = 1; i <= NR; i++)
            {
                Thread Reader = new Thread(new ParameterizedThreadStart(ToRead));
                Reader.Name = "Reader " + i;
                Reader.Start(i);

            }//for
            for (int i = 1; i <= NW; i++)
            {
                Thread Writer = new Thread(new ParameterizedThreadStart(ToWrite));
                Writer.Name = "Writer " + i;
                Writer.Start(i);
            }//for
        }//Main

        static void do_somethink(int len)
        {
            double b = 1;
            for (int i = 0; i < len * 1123456; i++)
                b = 1 / b + i * 1.1;
        }

        static void signal()
        {
            if (verbose) Console.WriteLine("Signal work");
            if (nw == 0 && dr > 0)
            {
                dr--;
                r.Release(1);
            }
            else if (nr == 0 && nw == 0 && dw > 0)
            {
                dw--;
                w.Release(1);
            }
            else e.Release(1);
        }
        static void ToRead(object num)
        {
            while (iteration < NI)
            {
                e.WaitOne();
                if (verbose) Console.WriteLine("Reader {0} begins", num);
                if (nw > 0)
                {
                    dr++;
                    e.Release(1);
                    r.WaitOne();
                }
                if (verbose) Console.WriteLine("Reader {0} in rw1", num);
                nr++;
                signal();
                do_somethink(7);
                iteration++;

                if (verbose) Console.WriteLine("Reader {0} goes out", num);
                e.WaitOne();
                nr--;
                signal();
                Thread.Sleep(100);
            }//while

            Console.WriteLine("Reader {0} STOP", num);
            Console.WriteLine(iteration);
        }//Reader

        static void ToWrite(object num)
        {
            Random rnd = new Random();
            while (iteration < NI)
            {
                if (verbose) Console.WriteLine("Writer {0} begins", num);
                e.WaitOne();
                if (nr > 0 || nw > 0)
                {
                    dw++;
                    e.Release(1);
                    w.WaitOne();
                }
                nw++;
                if (verbose) Console.WriteLine("Writer {0} in rw1", num);
                signal();
                do_somethink(15);
                iteration++;

                e.WaitOne();
                nw--;
                signal();
                if (verbose) Console.WriteLine("Writer {0} goes out", num);
                Thread.Sleep(rnd.Next(50, 700));
            }//while
            Console.WriteLine("Writer {0} STOP", num);
            Console.WriteLine(iteration);
        }//Writer

    }
}
