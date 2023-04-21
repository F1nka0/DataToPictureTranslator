using System;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.IO;
using System.Text;

namespace DTV {
    class Translator {
        private readonly int width;
        private readonly int heighth;
        private Bitmap canvas;
        private readonly int pixelSize;
        private int byteCount=2;
        /// <param name="byteCount">Amount of data stored per one pixelSize, has to lie in the range of 1 to 3</param>
        /// <param name="h">BMP canvas height</param>
        /// <param name="w">BMP canvas width</param>
        /// <param name="pixelSize">Size of one data storage unit</param>
        public Translator(int byteCount, int w, int h, int pixelSize)
        {
            if (byteCount<1|| byteCount > 3) { throw new Exception("byteCount must be within range of 1 to 3"); }
            if (h<=0|| w <= 0) { throw new Exception("heighth and width must be positive"); }
            if (pixelSize < 1|| pixelSize>w) { throw new Exception("pixelSize must be greater than 0 and less than the width of BMP canvas"); }
            this.pixelSize = pixelSize;
            this.byteCount = byteCount;
            canvas = new Bitmap(w, h);
            width = w;
            heighth = h;
        }
        private List<string> ProvideBinaryData(string pathToSource) {
            if (!File.Exists(pathToSource)) { throw new FileNotFoundException(); }
            BinaryReader reader = new BinaryReader(File.OpenRead(pathToSource));
            List<string> data = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            FileInfo file = new FileInfo(pathToSource);
            data.Add($"#{BitConverter.ToString(Encoding.Default.GetBytes(file.Name.Substring(file.Name.IndexOf(".") + 1))).Replace("-", "")}");
            for(int amountOfBytes = 0;amountOfBytes<16; amountOfBytes+=2) {
                data.Add($"#{reader.BaseStream.Length.ToString().PadLeft(16, '0').Substring(amountOfBytes,2).PadRight(4,'0')}");
            }
            while (reader.BaseStream.Position < reader.BaseStream.Length) {

                stringBuilder.Append('#');
                for (long curByte = 0; (reader.BaseStream.Position < reader.BaseStream.Length) && (curByte < byteCount); curByte++)
                {
                    stringBuilder.Append(reader.ReadByte().ToString("X").PadLeft(2, '0'));
                }
                //Console.WriteLine(stringBuilder.ToString());
                data.Add(stringBuilder.ToString());
                stringBuilder.Clear();
            }
            reader.Close();
            return data;
        }
        /// <summary>
        /// Encode file to BMP
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        /// <param name="saveBMP">Path to the directory, you want to save encoded data into</param>
        /// <param name="source">Full name of data file,you want to encode</param>
        public void Encode(string source, string saveBMP) {
            if (!File.Exists(source)) {throw new FileNotFoundException();}
            var binData = ProvideBinaryData(source);
            int count = -1;
            for (int i = 0; i < heighth - pixelSize; i+=pixelSize)
            {
                for (int j = 0; j < width - pixelSize; j+= pixelSize)
                {
                    count++;
                    if (count < binData.Count) {
                        SetChunkOfPixelsToColor(i, j, ColorTranslator.FromHtml(binData[count].PadRight(7, '0')));
                    }
                }
            }
            Directory.CreateDirectory($"{saveBMP}\\EncodedBMPs");
            canvas.Save($"{saveBMP}\\EncodedBMPs\\{DateTime.Now.ToString("HH-mm-ss-ffff")}.bmp");
        }
        private void SetChunkOfPixelsToColor(int h,int w,Color color) {
            for (int a = h; a < h + pixelSize; a++) {
                for (int b = w; b < w+pixelSize; b++)
                {
                    canvas.SetPixel(b,a,color);
                }    
            }
        }

        private byte[] StringToByteArray(string hex)
        {
            List<byte> bytes = new List<byte>();  
            for (int i = 0; i < byteCount*2; i+=2)
            {
                bytes.Add(Convert.ToByte(hex.Substring(i,2), 16));
            }
            return bytes.ToArray();
        }
        private string GetExtention(Bitmap bmp) { 
            Color extPixel = bmp.GetPixel(0, 0); 
            return new string(new byte[] { extPixel.R, extPixel.G, extPixel.B }.Select(it => (char)it).ToArray());}
        private long RetrieveAmountOfBytes(Bitmap bmp) {
            string temp = "";
                for (int j = pixelSize; j < 9* pixelSize&& j<width; j += pixelSize)
                {
                    temp += bmp.GetPixel(j, 0).R.ToString("X").PadLeft(2, '0');
                }
            return long.Parse(temp);
        }
        /// <summary>
        /// Decodes data, previously encoded to BMP file
        /// </summary>
        /// <param name="pathToSaveDecodedData">Directory to save decoded data to</param>
        public void Decode(string BMPfile,string pathToSaveDecodedData) {
            if (!File.Exists(BMPfile)) { throw new FileNotFoundException(); }
            var saveDir = Directory.CreateDirectory($"{pathToSaveDecodedData}\\Decoded");
            Bitmap bitmap = new Bitmap(BMPfile);
            StreamWriter writer = new StreamWriter(File.Open($"{saveDir.FullName}\\{new FileInfo(BMPfile).Name} - {DateTime.Now.ToString("HH-mm-ss-ffff")}.{GetExtention(bitmap)}", FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite));
            Color color;
            string bytesToWriteAsString="";
            StringBuilder SB = new StringBuilder();
            int countOfWrittenBytes = 0;
            long countOfBytesToRead = RetrieveAmountOfBytes(bitmap);
            int i = 9* pixelSize;
            for (int j = 0; j < heighth - pixelSize; j += pixelSize)
            {
                for (; i < width - pixelSize; i += pixelSize)
                {
                    if (countOfWrittenBytes < countOfBytesToRead) {
                        color = bitmap.GetPixel(i, j);
                        bytesToWriteAsString += color.R.ToString("X2");
                        if (byteCount == 2|| byteCount == 3) {
                            bytesToWriteAsString += color.G.ToString("X2");
                        }
                        if (byteCount == 3) {
                            bytesToWriteAsString += color.B.ToString("X2");
                        }
                        foreach (byte b in StringToByteArray(bytesToWriteAsString)) {
                            SB.Append((char)b);
                        }
                        bytesToWriteAsString = "";
                        countOfWrittenBytes+=byteCount;
                    }
                }
                i = 0;
            }
            writer.Write(SB.ToString());
            writer.Close();
        }
    }
    class Program {
        public static void Main(string[] args) {
            Translator translator = new Translator(2,1920,1080,10);
        }
    }
}
