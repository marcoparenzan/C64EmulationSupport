using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace D64ReaderWriter
{
    class D64
    {
        private static uint __size = 174848;

        private byte[] _bytes = new byte[__size];

        public static readonly D64TrackDescriptor[] Tracks = new D64TrackDescriptor[] {
            new D64TrackDescriptor { Size=21, Offset=0, Address=0x00000 },
            new D64TrackDescriptor { Size=21, Offset=21, Address=0x01500 },
            new D64TrackDescriptor { Size=21, Offset=42, Address=0x02A00 },
            new D64TrackDescriptor { Size=21, Offset=63, Address=0x03F00 },
            new D64TrackDescriptor { Size=21, Offset=84, Address=0x05400 },
            new D64TrackDescriptor { Size=21, Offset=105, Address=0x06900 },
            new D64TrackDescriptor { Size=21, Offset=126, Address=0x07E00 },
            new D64TrackDescriptor { Size=21, Offset=147, Address=0x09300 },
            new D64TrackDescriptor { Size=21, Offset=168, Address=0x0A800 },
            new D64TrackDescriptor { Size=21, Offset=189, Address=0x0BD00 },
            new D64TrackDescriptor { Size=21, Offset=210, Address=0x0D200 },
            new D64TrackDescriptor { Size=21, Offset=231, Address=0x0E700 },
            new D64TrackDescriptor { Size=21, Offset=252, Address=0x0FC00 },
            new D64TrackDescriptor { Size=21, Offset=273, Address=0x11100 },
            new D64TrackDescriptor { Size=21, Offset=294, Address=0x12600 },
            new D64TrackDescriptor { Size=21, Offset=315, Address=0x13B00 },
            new D64TrackDescriptor { Size=21, Offset=336, Address=0x15000 },
            new D64TrackDescriptor { Size=19, Offset=357, Address=0x16500 },
            new D64TrackDescriptor { Size=19, Offset=376, Address=0x17800 },
            new D64TrackDescriptor { Size=19, Offset=395, Address=0x18B00 },
            new D64TrackDescriptor { Size=19, Offset=414, Address=0x19E00 },
            new D64TrackDescriptor { Size=19, Offset=433, Address=0x1B100 },
            new D64TrackDescriptor { Size=19, Offset=452, Address=0x1C400 },
            new D64TrackDescriptor { Size=19, Offset=471, Address=0x1D700 },
            new D64TrackDescriptor { Size=18, Offset=490, Address=0x1EA00 },
            new D64TrackDescriptor { Size=18, Offset=508, Address=0x1FC00 },
            new D64TrackDescriptor { Size=18, Offset=526, Address=0x20E00 },
            new D64TrackDescriptor { Size=18, Offset=544, Address=0x22000 },
            new D64TrackDescriptor { Size=18, Offset=562, Address=0x23200 },
            new D64TrackDescriptor { Size=18, Offset=580, Address=0x24400 },
            new D64TrackDescriptor { Size=17, Offset=598, Address=0x25600 },
            new D64TrackDescriptor { Size=17, Offset=615, Address=0x26700 },
            new D64TrackDescriptor { Size=17, Offset=632, Address=0x27800 },
            new D64TrackDescriptor { Size=17, Offset=649, Address=0x28900 },
            new D64TrackDescriptor { Size=17, Offset=666, Address=0x29A00 },
            new D64TrackDescriptor { Size=17, Offset=683, Address=0x2AB00 },
            new D64TrackDescriptor { Size=17, Offset=700, Address=0x2BC00 },
            new D64TrackDescriptor { Size=17, Offset=717, Address=0x2CD00 },
            new D64TrackDescriptor { Size=17, Offset=734, Address=0x2DE00 },
            new D64TrackDescriptor { Size=17, Offset=751, Address=0x2EF00 },
            new D64TrackDescriptor { Size=17, Offset=768, Address=0x30000 },
            new D64TrackDescriptor { Size=17, Offset=785, Address=0x31100 }
        };

        private D64() { }

        public static async Task<D64> Empty()
        {
            var disk = new D64();
            disk._bytes = await File.ReadAllBytesAsync("empty.d64");

            return disk;
        }


        public static async Task<D64> FromStream(Stream stream)
        {
            var disk = new D64();
            var read = await stream.ReadAsync(disk._bytes, 0, (int) __size);
            if (read != __size) throw new ArgumentException("Stream is not a d64 disk");

            return disk;
        }

        public async Task WriteTo(Stream streamD64)
        {
            await streamD64.WriteAsync(_bytes, 0, _bytes.Length);
        }


        public D64 DiskName(string diskName)
        {
            var bytes = diskName.PadRight(16, (char)0xa0);
            Array.Copy(_bytes, BAMAddress, _bytes, 0x90, 0x10);
            return this;
        }

        public D64 DosType(string dosType)
        {
            var bytes = dosType.PadRight(2 , (char)0xa0);
            Array.Copy(_bytes, BAMAddress, _bytes, 0x90, 0x02);

            return this;
        }

        public D64BAMInfo BAM()
        {
            var disk = new D64BAMInfo();

            disk.firstDirectoryTrack = _bytes[BAMAddress + 0x00];
            disk.firstSectorTrack = _bytes[BAMAddress + 0x01];
            disk.diskDosVersionType = (char)_bytes[BAMAddress + 0x02];
            disk.diskName = Encoding.ASCII.GetString(_bytes, (int)BAMAddress + 0x90, 16).Trim('\x00', '\xa0');
            disk.diskId = (ushort)(_bytes[BAMAddress + 0xa2] + (_bytes[BAMAddress + 0xa3] << 8));
            disk.dosType = Encoding.ASCII.GetString(_bytes, (int)BAMAddress + 0xa5, 2).Trim('\x00');

            return disk;
        }

        public D64DirectoryEntry[] Directory()
        {
            byte trackEntry = 18;
            byte sectorEntry = 1;

            var list = new List<D64DirectoryEntry>();
            while (true)
            {
                uint idir = Tracks[trackEntry-1].SectorAddress(sectorEntry);

                // sector
                for (var x = 0; x < 8; x++)
                {
                    var entry = new D64DirectoryEntry();
                    entry.nextTrackEntry = _bytes[idir + 0x00];
                    entry.nextSectorEntry = _bytes[idir + 0x01];
                    if (x == 0)
                    {
                        trackEntry = entry.nextTrackEntry;
                        sectorEntry = entry.nextSectorEntry;
                    }
                    entry.fileType = _bytes[idir + 0x02];
                    entry.actualFileType = (D64FileType)(entry.fileType & 0xf);
                    entry.locked = (entry.fileType & 0b01000000) == 0b01000000;
                    entry.closed = (entry.fileType & 0b10000000) == 0b10000000;
                    entry.firstFileSectorTrack = _bytes[idir + 0x03];
                    entry.firstFileSectorSector = _bytes[idir + 0x04];
                    entry.filename = Encoding.ASCII.GetString(_bytes, (int) idir + 0x05, 16).Trim('\x00');
                    entry.firstBlockTrack = _bytes[idir + 0x15];
                    entry.firstSectorSector = _bytes[idir + 0x16];
                    entry.relRecordLength = _bytes[idir + 0x17];
                    entry.fileSizeInSectors = (ushort)(_bytes[idir + 0x1e] + (_bytes[idir + 0x1f] << 8));

                    if (entry.firstFileSectorTrack>0) list.Add(entry);

                    idir += 0x20;
                }

                if (trackEntry == 0) break;
            }
            return list.ToArray();
        }

        static uint BAMAddress = Tracks[18 - 1].SectorAddress(0);
        static uint T19Address = Tracks[19-1].SectorAddress(0);

        public D64 Write(D64FileType requestedFileType, string filename, byte[] source)
        {
            // session pointers
            uint source_ptr = 0;
            ushort sector_count = 0;
            
            // first: find a dir entry 
            // no sector-based approach
            uint dirEntryAddress = Tracks[18-1].SectorAddress(1);
            while (true)
            {
                var actualFileType = (D64FileType)(_bytes[dirEntryAddress + 0x02] & 0x0f);
                if (actualFileType != D64FileType.DEL)
                {
                    dirEntryAddress += 0x20;
                    if (dirEntryAddress >= T19Address) throw new ArgumentException("disk full (no dir entries available)");
                    continue;
                }

                // now idir contains current dir offset
                break;
            }

            // now scan for free sectors with BAM
            byte currentBamTrackOffset = 0x04;
            byte currentTrack = 1;
            byte currentSector = 0;

            // then find first bam entry
            uint currentTSChain = (uint)(dirEntryAddress + 0x03); // in the directory entry, the position of T/S of the first sector
            while (true)
            {
                // if no free space in track
                if (_bytes[BAMAddress + currentBamTrackOffset] == 0)
                {
                    // next track
                    currentBamTrackOffset += 0x04; // next track in BAM
                    // beginning of disk name in BAM; just after BAM entries
                    if (currentBamTrackOffset >= 0x90) throw new ArgumentException("disk full (no BAM entries available)");
                    // no support for special D64 formats with more than 35 tracks...

                    currentTrack += 1;
                    currentSector = 0;
                    if (currentTrack == 0x12) // track 18..directory...advance again
                    {
                        currentBamTrackOffset += 0x04;
                        currentTrack += 1;
                        currentSector = 0;
                    }
                    continue;
                }

                byte sectorMapEntryOffset = 0x01;
                byte sectorMapMask = 0x01;
                while (true)
                {
                    if ((_bytes[BAMAddress + currentBamTrackOffset + sectorMapEntryOffset] & sectorMapMask) == sectorMapMask) // free
                    {
                        // update session
                        sector_count += 1;

                        // mark as full
                        _bytes[BAMAddress + currentBamTrackOffset] -= 1; // one less sector available
                        _bytes[BAMAddress + currentBamTrackOffset + sectorMapEntryOffset] &= (byte)(0xff-sectorMapMask); // the mask is useful also for setting the bit

                        // at this time address contain the last location, useful to chain sectors
                        // the first time it will be the dir entry, else the last sector
                        _bytes[currentTSChain + 0x00] = currentTrack;
                        _bytes[currentTSChain + 0x01] = currentSector;

                        currentTSChain = Tracks[currentTrack - 1].Address + (uint)(currentSector << 8); // a sector is 256 bytes wide
                        // fill nexy sector pointer to zero if it becomes the last one
                        _bytes[currentTSChain + 0x00] = 0;
                        _bytes[currentTSChain + 0x01] = 0;

                        // the copy loop
                        Copy(source, source_ptr, 254, currentTSChain + 0x02, 0x00);
                        if (source_ptr >= source.Length) break; // finished
                        source_ptr += 254; // not finished ==> advance ptr
                    }

                    currentSector += 1;
                    // track is full
                    if (currentSector == Tracks[currentTrack - 1].Size)
                        break;

                    sectorMapMask <<= 1;
                    if (sectorMapMask == 0x00)
                    {
                        sectorMapEntryOffset += 1;
                        // the boundary control is not useful as currentSector finish it before...always
                        sectorMapMask = 0x01;
                    }
                }

                // if out the loop check if finished...
                if (source_ptr >= source.Length) break; // finished
            }

            // at the end, update dir entry
            // set the file type
            _bytes[dirEntryAddress + 0x02] = (byte)((_bytes[dirEntryAddress + 0x02] & 0b11110000) | (byte)requestedFileType);
            _bytes[dirEntryAddress + 0x02] = (byte)((_bytes[dirEntryAddress + 0x02] & 0b00111111) | (byte)0b10000000);
            // copy the filename
            Copy(Encoding.UTF8.GetBytes(filename), 0, 0x10, (uint)(dirEntryAddress + 5), 0xa0);
            // update sector count
            _bytes[dirEntryAddress + 0x1e] = (byte) (sector_count & 0x00ff);
            _bytes[dirEntryAddress + 0x1f] = (byte)(sector_count >> 8);

            return this;
        }

        private bool Copy(byte[] source, uint source_ptr, uint length, uint target_ptr, byte filler)
        {
            while (true)
            {
                if (length == 0) break;
                if (source_ptr < source.Length)
                {
                    _bytes[target_ptr] = source[source_ptr];
                }
                else
                {
                    // padding
                    _bytes[target_ptr] = filler;
                }
                source_ptr += 1;
                target_ptr += 1;
                length -= 1;
            }
            return source_ptr >= source.Length;
        }
    }

    class D64TrackDescriptor
    {
        public byte Size { get; internal set; }
        public uint Offset { get; internal set; }
        public uint Address { get; internal set; }

        public uint SectorAddress(byte sectorEntry) => (uint)(Address + sectorEntry * 256);
    }

    class D64DirectoryEntry
    {
        public byte nextTrackEntry { get; set; }
        public byte nextSectorEntry { get; set; }
        public byte fileType { get; set; }
        public D64FileType actualFileType { get; set; }
        public bool locked { get; set; }
        public bool closed { get; set; }
        public byte firstFileSectorTrack { get; set; }
        public byte firstFileSectorSector { get; set; }
        public string filename { get; set; }
        public byte firstBlockTrack { get; set; }
        public byte firstSectorSector { get; set; }
        public byte relRecordLength { get; set; }
        public ushort fileSizeInSectors { get; set; }

        public override string ToString()
        {
            return $"[{fileSizeInSectors}]{filename}[{actualFileType}]";
        }
    }

    public class D64BAMInfo
    {
        public object firstDirectoryTrack { get; internal set; }
        public object firstSectorTrack { get; internal set; }
        public char diskDosVersionType { get; internal set; }
        public string diskName { get; internal set; }
        public ushort diskId { get; internal set; }
        public string dosType { get; internal set; }
    }

    enum D64FileType : byte
    {
        DEL = 0b0000,
        SEQ = 0b0001,
        PRG = 0b0010,
        USR = 0b0011,
        REL = 0b0100
    }
}
