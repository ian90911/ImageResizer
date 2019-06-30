using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ImageProcess
    {
        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(destPath);
                var files = di.GetFiles();
                Stopwatch sw = new Stopwatch();
                sw.Start();
                files.AsParallel().ForAll((f) => f.Delete());
                sw.Stop();
                Console.WriteLine($"刪檔花費時間: {sw.ElapsedMilliseconds} ms");
            }
        }

        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public async Task ResizeImages(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            Image[] imgPhotos = new Image[allFiles.Length];
            string[] imgNames = new string[allFiles.Length];

            var files = allFiles.Select((f, index) => new { file = f, index }).AsParallel();
            files.ForAll(f => 
            {
                imgPhotos[f.index] = Image.FromFile(allFiles[f.index]);
                imgNames[f.index] = Path.GetFileNameWithoutExtension(allFiles[f.index]);
            });

            Tuple<int, int, int, int>[] specs = new Tuple<int, int, int, int>[allFiles.Length];
            var photos = imgPhotos.Select((imgPhoto, index) => new { imgPhoto, index }).ToArray();
            Bitmap[] pImgs = new Bitmap[photos.Length];

            photos.AsParallel().ForAll(x =>
            {
                int sourceWidth = x.imgPhoto.Width;
                int sourceHeight = x.imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);
                specs[x.index] = new Tuple<int, int, int, int>(sourceWidth, sourceHeight, destionatonWidth, destionatonHeight);
                pImgs[x.index] = processBitmapAsync((Bitmap)photos[x.index].imgPhoto,
                   specs[x.index].Item1, specs[x.index].Item2,
                   specs[x.index].Item3, specs[x.index].Item4);
            });
            
            Task[] saveTasks = new Task[photos.Length];
            for (int i = 0; i < photos.Count(); i++)
            {
                int index = i;
                saveTasks[i] = Task.Run(() => 
                {
                    string destFile = Path.Combine(destPath, imgNames[photos[index].index] + ".jpg");
                    photos[index].imgPhoto.Save(destFile, ImageFormat.Jpeg);
                });
            }
            await Task.WhenAll(saveTasks);
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public string[] FindImages(string srcPath)
        {
            List<string> files = new List<string>();
            DirectoryInfo di = new DirectoryInfo(srcPath);
            var fileInfos = di.GetFiles().Where(f => f.Extension == ".png" || f.Extension == ".jpg" || f.Extension == ".jpeg").Select(f=>f.FullName).AsParallel();
            files.AddRange(fileInfos);
            return files.ToArray();
        }

        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        Bitmap processBitmapAsync(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, newWidth, newHeight),
                new Rectangle(0, 0, srcWidth, srcHeight),
                GraphicsUnit.Pixel);
            return resizedbitmap;
        }
    }
}
