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
            Task[] getImgTasks = new Task[allFiles.Length];
            Task[] getFilePathTasks = new Task[allFiles.Length];
            Task[] resizeTasks = new Task[allFiles.Length];
            Image[] imgPhotos = new Image[allFiles.Length];
            string[] imgNames = new string[allFiles.Length];
            for (int i = 0; i < allFiles.Length; i++)
            {
                int index = i;
                getImgTasks[i] = Task.Run(() => {
                    imgPhotos[index] = Image.FromFile(allFiles[index]);
                });
                getFilePathTasks[i] = Task.Run(() => {
                    imgNames[index] = Path.GetFileNameWithoutExtension(allFiles[index]);
                });
            }
            await Task.WhenAll(getImgTasks.Concat(getFilePathTasks));

            imgPhotos.AsParallel().Select((imgPhoto,index)=>new {imgPhoto,index }).ForAll(async x =>
            {
                int sourceWidth = x.imgPhoto.Width;
                int sourceHeight = x.imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);

                Bitmap processedImage = await processBitmapAsync((Bitmap)x.imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight);

                string destFile = Path.Combine(destPath, imgNames[x.index] + ".jpg");
                processedImage.Save(destFile, ImageFormat.Jpeg);
            });
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
            var fileInfos = di.GetFiles().AsParallel().Where(f => f.Extension == ".png" || f.Extension == ".jpg" || f.Extension == ".jpeg").Select(f=>f.FullName);
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
        async Task<Bitmap> processBitmapAsync(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            return await Task.Run(() => 
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
            });
        }

        void getImageFromFile(Image[] imgArray, string[] filePathArray, int index)
        {
            imgArray[index] = Image.FromFile(filePathArray[index]);
        }
    }
}
