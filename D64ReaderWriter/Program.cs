using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace D64ReaderWriter
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var filenameT64 = @"E:\Commodore 64\media\zyronesc.T64";
            using (var streamT64 = File.OpenRead(filenameT64))
            {
                var tape = await T64.FromStream(streamT64);

                var filenameD64 = @"E:\Commodore 64\media\zyronesc.D64";
                using (var streamD64 = File.OpenWrite(filenameD64))
                {
                    var disk = await D64.Empty();
                    disk.DiskName(tape.userDescription).DosType("2A");

                    foreach (var file in tape.files.OrderBy(xx => xx.offsetFile))
                    {
                        disk.Write(D64FileType.PRG, file.c64FileName, file.bytes);
                    }

                    await disk.WriteTo(streamD64);
                }
            }
        }
    }
}
