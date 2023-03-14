using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NReco.VideoConverter;
using TagLib.Riff;
using File = System.IO.File;

namespace AudioCutter
{
    class Program
    {
        static void Main(string[] args)
        {
            var folderPath = Path.Combine("E:\\spareFolder");
            var ignoredExtenstions = new List<string> {".mp4", ".mkv"};
            Directory.GetFiles(folderPath).Where(x=> ignoredExtenstions.Contains(Path.GetExtension(x))).ToList().ForEach(fileName =>
            {
                ;
                // CutMp3(fileName, "E:\\outputAudios\\" + Path.GetFileName(fileName));
                ConvertVideoToMp3(fileName, "E:\\outputAudios\\" + Path.GetFileName(fileName));
                ChangeExtension(fileName);
            });
        }
        public static void CutMp3(string inputFilePath, string outputFilePath)
        {
            // open the input MP3 file
            using (var reader = new Mp3FileReader(inputFilePath))
            {
                // create a new MP3 file writer for the output file
                using (var writer = File.Create(outputFilePath))
                {
                    // create a new instance of the WaveFileWriter
                    using (var waveWriter = new WaveFileWriter(writer, reader.WaveFormat))
                    {
                        // skip the first 3 seconds of audio
                        reader.CurrentTime = TimeSpan.FromSeconds(3);
                        var outFormat = new Mp3WaveFormat(128000, 2, 44100, 128000);

                        // copy the rest of the audio to the output file
                        var buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            waveWriter.Write(buffer, 0, bytesRead);
                        }
                    }
                }
            }
        }

        public static bool ConvertVideoToMp3(string inputFilePath, string outputFilePath)
        {
            try
            {
                // Set the conversion options
                var conversionOptions = new ConvertSettings
                {
                    AudioCodec = "libmp3lame",
                    AudioSampleRate = 128000,
                    CustomInputArgs = "-vn", // extract audio only
                    CustomOutputArgs = "-map_metadata 0 -id3v2_version 3" // set metadata and ID3v2 version
                };
                //conversionOptions. = 128000; // 128 kbps

                // Create an instance of the converter
                var converter = new FFMpegConverter();

                // Convert the video file to MP3
                converter.ConvertMedia(inputFilePath, outputFilePath, "mp3");

                return true; // Conversion succeeded
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting video to MP3: {ex.Message}");
                return false; // Conversion failed
            }
        }

        public static void ChangeExtension(string inputFilePath)
        {

            // Set the output file path with the same name but different extension
            string outputFilePath = Path.ChangeExtension(inputFilePath, "mp3");

            // Delete the existing file if it exists
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            // Rename the original file to the new extension
            File.Move(inputFilePath, outputFilePath);
        }

    }
}
