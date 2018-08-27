using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D64ReaderWriter
{
    class T64
    {
        private byte[] _bytes;

        private T64() { }

        public static byte ileType { get; private set; }
        public string tapeDescription { get; private set; }
        public int directoryEntries { get; private set; }
        public int usedEntries { get; private set; }
        public string userDescription { get; private set; }
        public List<T64FileEntry> files { get; private set; }

        public static async Task<T64> FromStream(Stream stream)
        {
            var tape = new T64();

            var tapeRecordBytes = new byte[64];
            await stream.ReadAsync(tapeRecordBytes, 0, tapeRecordBytes.Length);
            tape.tapeDescription = Encoding.ASCII.GetString(tapeRecordBytes, 0, 32).Trim(' ', '\x0', '\xa0');
            tape.directoryEntries = tapeRecordBytes[34] + (tapeRecordBytes[35] << 8);
            tape.usedEntries = tapeRecordBytes[36] + (tapeRecordBytes[37] << 8);
            tape.userDescription = Encoding.ASCII.GetString(tapeRecordBytes, 40, 24).Trim(' ', '\x0', '\xa0');

            tape.files = new List<T64FileEntry>();

            for (var i = 0; i < tape.directoryEntries; i++)
            {
                var fileEntry = new T64FileEntry();

                var fileRecordBytes = new byte[32];
                await stream.ReadAsync(fileRecordBytes, 0, fileRecordBytes.Length);
                fileEntry.entryType = fileRecordBytes[0];
                fileEntry.C64FileType = fileRecordBytes[1];
                fileEntry.startAddress = fileRecordBytes[2] + (fileRecordBytes[3] << 8);
                fileEntry.endAddress = fileRecordBytes[4] + (fileRecordBytes[5] << 8);
                fileEntry.offsetFile = fileRecordBytes[8] + (fileRecordBytes[9] << 8) + (fileRecordBytes[10] << 16) + (fileRecordBytes[11] << 24);
                fileEntry.c64FileName = Encoding.ASCII.GetString(fileRecordBytes, 16, 16).Trim(' ', '\x0', '\xa0');

                tape.files.Add(fileEntry);
            }

            foreach (var file in tape.files.OrderBy(xx => xx.offsetFile))
            {
                stream.Seek(file.offsetFile, SeekOrigin.Begin);
                file.bytes = new byte[file.endAddress - file.startAddress];
                await stream.ReadAsync(file.bytes, 0, file.bytes.Length);
            }


            return tape;
        }
    }
}
