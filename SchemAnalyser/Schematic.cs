using System.IO.Compression;
using System.Text;
using g3;
using NbtToolkit;
using NbtToolkit.Binary;

namespace SchemAnalyser
{
    public class Schematic
    {
        public List<TagCompound> ExtraBlockData { get; set; }
        public List<BlockState> BlockPallete { get; set; }
        public List<ShipGrid> ShipGrids { get; set; }

        public void Visit(ISchematicVisitor visitor)
        {
            visitor.Visit(this);
            foreach(var block in ShipGrids) visitor.VisitShipGrid(block);
        }

        public static Schematic FromStream(Stream stream)
        {
            var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            var str = ReadUtf(reader, 400);

            Console.WriteLine(str);

            str = ReadUtf(reader, 400);

            Console.WriteLine(str);
            
            using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                var nbt_reader = new NbtReader(gzipStream);

                var rootTag = nbt_reader.ReadRootTag();

                var blockStates = new List<BlockState>();

                foreach (var entry in rootTag["blockPalette"].AsTagList<TagCompound>())
                {
                    var properties = new Dictionary<string, string>();
                    if (entry.ContainsKey("Properties"))
                    {
                        foreach (var prop in entry["Properties"].AsTagCompound())
                        {
                            properties[prop.Key] = prop.Value.AsString();
                        }
                    }

                    blockStates.Add(new BlockState()
                    {
                        Name = entry["Name"].AsString(),
                        Properties = properties
                    });
                }

                var shipGrids = new List<ShipGrid>();

                foreach (var entry in rootTag["gridData"].AsTagCompound())
                {
                    var blocks = new List<ShipGrid.BlockEntry>();
                    foreach (var data in entry.Value.AsTagList<TagCompound>())
                    {
                        blocks.Add(new ShipGrid.BlockEntry()
                        {
                            x = data["x"].AsInt(),
                            y = data["y"].AsInt(),
                            z = data["z"].AsInt(),
                            pid = data["pid"].AsInt(),
                            edi = data["edi"].AsInt(),
                        });
                    }

                    shipGrids.Add(new ShipGrid() { Id = entry.Key, Blocks = blocks });
                }

                var extraBlocks = new List<TagCompound>();

                foreach (var entry in rootTag["extraBlockData"].AsTagList<TagCompound>())
                {
                    extraBlocks.Add(entry);
                }

                var schema = new Schematic()
                {
                    BlockPallete = blockStates,
                    ShipGrids = shipGrids,
                    ExtraBlockData = extraBlocks
                };
                
                return schema;
            }
        }
        public static int ReadVarInt(BinaryReader reader)
        {
            var numRead = 0;
            var result = 0;
            byte read;
            do
            {
                read = reader.ReadByte();
                var value = (read & 0b01111111);
                result |= (value << (7 * numRead));

                numRead++;
                if (numRead > 5)
                    throw new InvalidDataException("VarInt is too big");
            } while ((read & 0b10000000) != 0);

            return result;
        }

        public static string ReadUtf(BinaryReader reader, int maxLength)
        {
            var j = ReadVarInt(reader);
            var i = GetMaxEncodedUtfLength(maxLength);

            if (j > i)
                throw new InvalidDataException($"Encoded string length {j} > max {i}");
            if (j < 0)
                throw new InvalidDataException("Encoded string length < 0");

            var bytes = reader.ReadBytes(j);
            var s = Encoding.UTF8.GetString(bytes);

            return s.Length > maxLength ? throw new InvalidDataException($"Decoded string length {s.Length} > max {maxLength}") : s;
        }

        private static int GetMaxEncodedUtfLength(int value)
        {
            return (int)Math.Ceiling(value * 3.0);
        }
    }
    
    public class BlockState
    {
        public Dictionary<string, string> Properties { get; set; }
        public string Name { get; set; }

    }
    public class ShipGrid
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
        public AxisAlignedBox3d aabb { get; set; }
    }
    
    
}