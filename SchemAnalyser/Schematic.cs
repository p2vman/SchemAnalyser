using System.IO.Compression;
using System.Text;
using NbtToolkit;
using NbtToolkit.Binary;

namespace SchemAnalyser
{
    public class Schematic
    {
        public List<TagCompound> ExtraBlockData { get; set; }
        public List<BlockState> BlockPallete { get; set; }
        public List<Ship> Ships { get; set; }

        public static Schematic FromStream(Stream stream)
        {
            var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            string str = ReadUtf(reader, 400);

            Console.WriteLine(str);

            str = ReadUtf(reader, 400);

            Console.WriteLine(str);
            
            using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                NbtReader nbt_reader = new NbtReader(gzipStream);

                var rootTag = nbt_reader.ReadRootTag();

                List<BlockState> blockStates = new List<BlockState>();

                foreach (var entry in rootTag["blockPalette"].AsTagList<TagCompound>())
                {
                    var Properties = new Dictionary<string, string>();
                    if (entry.ContainsKey("Properties"))
                    {
                        foreach (var prop in entry["Properties"].AsTagCompound())
                        {
                            Properties[prop.Key] = prop.Value.AsString();
                        }
                    }

                    blockStates.Add(new BlockState()
                    {
                        Name = entry["Name"].AsString(),
                        Properties = Properties
                    });
                }

                List<Ship> ships = new List<Ship>();

                foreach (var entry in rootTag["gridData"].AsTagCompound())
                {
                    List<Ship.BlockEntry> blocks = new List<Ship.BlockEntry>();
                    foreach (var data in entry.Value.AsTagList<TagCompound>())
                    {
                        blocks.Add(new Ship.BlockEntry()
                        {
                            x = data["x"].AsInt(),
                            y = data["y"].AsInt(),
                            z = data["z"].AsInt(),
                            pid = data["pid"].AsInt(),
                            edi = data["edi"].AsInt(),
                        });
                    }

                    ships.Add(new Ship() { Id = entry.Key, Blocks = blocks });
                }

                List<TagCompound> extraBlocks = new List<TagCompound>();

                foreach (var entry in rootTag["extraBlockData"].AsTagList<TagCompound>())
                {
                    extraBlocks.Add(entry);
                }

                Schematic schema = new Schematic()
                {
                    BlockPallete = blockStates,
                    Ships = ships,
                    ExtraBlockData = extraBlocks
                };


                return schema;
            }
        }
        
        public static int ReadVarInt(BinaryReader reader)
        {
            int numRead = 0;
            int result = 0;
            byte read;
            do
            {
                read = reader.ReadByte();
                int value = (read & 0b01111111);
                result |= (value << (7 * numRead));

                numRead++;
                if (numRead > 5)
                    throw new InvalidDataException("VarInt is too big");
            } while ((read & 0b10000000) != 0);

            return result;
        }

        public static string ReadUtf(BinaryReader reader, int maxLength)
        {
            int j = ReadVarInt(reader);
            int i = GetMaxEncodedUtfLength(maxLength);

            if (j > i)
                throw new InvalidDataException($"Encoded string length {j} > max {i}");
            if (j < 0)
                throw new InvalidDataException("Encoded string length < 0");

            byte[] bytes = reader.ReadBytes(j);
            string s = Encoding.UTF8.GetString(bytes);

            if (s.Length > maxLength)
                throw new InvalidDataException($"Decoded string length {s.Length} > max {maxLength}");

            return s;
        }

        private static int GetMaxEncodedUtfLength(int value)
        {
            // Minecraft использует этот метод:
            // ceil(value * 3) (максимум UTF-8 байт на символ)
            return (int)Math.Ceiling(value * 3.0);
        }
    }
    
    public class BlockState
    {
        public Dictionary<string, string> Properties { get; set; }
        public string Name { get; set; }

    }
    public class Ship
    {
        public class BlockEntry
        {
            public int x { get; set; }
            public int y { get; set; }
            public int z { get; set; }

            public int pid { get; set; }
            public int edi { get; set; }
        }

        public string Id { get; set; }
        public List<BlockEntry> Blocks { get; set; }
    }
    
    
}