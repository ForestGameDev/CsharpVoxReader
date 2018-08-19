using System;
using System.IO;
using System.Linq;

namespace CsharpVoxReader
{
    public class VoxReader
    {
        private const Int32 FILE_FORMAT_VERSION = 150;

        protected string Path { get; set; }
        protected IVoxLoader Loader { get; set; }

        public VoxReader(string path, IVoxLoader loader)
        {
            if ( ! File.Exists(path)) throw new FileNotFoundException("Can't open vox file : file not found", path);
            if (loader == null) throw new ArgumentNullException("loader"); //changed nameof for direct variable name for Unity compatibility

            Path = path;
            Loader = loader;
        }

        private VoxReader()
        {
        }

        public Chunk Read()
        {
            using (FileStream fs = File.OpenRead(Path))
            using(BinaryReader br = new BinaryReader(fs))
            {
                char[] magicNumber = br.ReadChars(4);
                if( ! magicNumber.SequenceEqual("VOX ".ToCharArray()))
                {
                    throw new IOException("Can't read VOX file : invalid vox signature"); //changed InvalidDataException for IOException for Unity compatibility
                }

                Int32 version = br.ReadInt32();
                if(version > FILE_FORMAT_VERSION)
                {
                    throw new IOException("Can't read VOX file : file format version ("+version+") is newer than reader version ("+FILE_FORMAT_VERSION+")"); //changed InvalidDataException for IOException for Unity compatibility
                }

                string id = Chunk.ReadChunkId(br);
                if(id != Chunks.Main.ID)
                {
                    throw new IOException("Can't read VOX file : MAIN chunk expected (was "+id+")"); //changed InvalidDataException for IOException for Unity compatibility
                }

                Chunk main = Chunk.CreateChunk(id);
                main.Read(br, Loader);

                return main;
            }
        }
    }
}
