using BenchmarkDotNet.Running;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer
{
    class Program
    {
        static void Main(string[] args)
        {
            

            //ImageProcess imageProcess = new ImageProcess();


            ////調整前:3572ms
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //await imageProcess.ResizeImagesAsync(sourcePath, destinationPath, 2.0);
            //sw.Stop();

            //Console.WriteLine($"調整圖片花費時間: {sw.ElapsedMilliseconds} ms");
            //Console.WriteLine($"press any key to continue...");

            var summary = BenchmarkRunner.Run<ImageProcess>();

            Console.ReadKey();
        }
    }
}
