namespace D64ReaderWriter
{
    internal class T64FileEntry
    {
        internal byte C64FileType;

        public byte entryType { get; internal set; }
        public int startAddress { get; internal set; }
        public int endAddress { get; internal set; }
        public string c64FileName { get; internal set; }
        public int offsetFile { get; internal set; }
        public byte[] bytes { get; internal set; }
    }
}