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
        static async Task Main(string[] args)
        {
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;

            ImageProcess imageProcess = new ImageProcess();

            imageProcess.Clean(destinationPath);

            //調整前:3572ms
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await imageProcess.ResizeImagesAsync(sourcePath, destinationPath, 2.0);
            sw.Stop();

            Console.WriteLine($"調整圖片花費時間: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"press any key to continue...");
            Console.ReadKey();
        }
    }
}
