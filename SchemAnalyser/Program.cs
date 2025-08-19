using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using NbtToolkit;

namespace SchemAnalyser
{
    public class MaterialUtils
    {
        public static BlockState GetCopyCatMaterial(TagCompound tag)
        {
            return GetBlockState(tag["material"].AsTagCompound());
        }

        public static BlockState GetBlockState(TagCompound tag)
        {
            var properties = new Dictionary<string, string>();
            if (tag.ContainsKey("Properties"))
            {
                foreach (var prop in tag["Properties"].AsTagCompound())
                {
                    properties[prop.Key] = prop.Value.AsString();
                }
            }
            
            return new BlockState()
            {
                Name = tag["Name"].AsString(),
                Properties = properties
            };
        }
        
    }
    public class Program
    {
        public static Mapper mapper = new();
        public static void Main(string[] args)
        {
            //create:copycat_base
            mapper.Put("copycats:multistate_copycat", compound =>
            {
                List<BlockState> materials = [];

                if (compound.ContainsKey("material_data"))
                {
                    var material_data = compound["material_data"].AsTagCompound();

                    foreach (var entry in material_data)
                    {
                        var state = MaterialUtils.GetCopyCatMaterial(entry.Value.AsTagCompound());
                        if (state.Name != "create:copycat_base")
                        {
                            materials.Add(state);
                        }
                    }
                }
                
                return materials;
            });
            
            mapper.Put("copycats:copycat", compound =>
            {
                List<BlockState> materials = [];

                if (compound.ContainsKey("Material"))
                {
                    var state = MaterialUtils.GetBlockState(compound["Material"].AsTagCompound());
                    if (state.Name == "create:copycat_base")
                    {
                        materials.Add(state);
                    }
                }
                
                return materials;
            });
            
            byte[] bytes = File.ReadAllBytes("Rapira 3.04 gray.vschem");

            var stream = new MemoryStream(bytes);
            
            Schematic schematic = Schematic.FromStream(stream);
            //Console.WriteLine(JsonSerializer.Serialize(schematic));
            
            schematic.Visit(new SchematicVisitor());
        }
        
        

        class SchematicVisitor : ISchematicVisitor, IShipVisitor
        {
            public Schematic schematic;
            public void Visit(Schematic schematic)
            {
               this.schematic = schematic;
            }

            public void VisitShipGrid(ShipGrid ships)
            {
                Visit(ships);
                foreach (var block in ships.Blocks)
                {
                    VisitBlock(block);
                }
            }

            public void Visit(ShipGrid ship)
            {
                Console.WriteLine(ship.Id);
            }

            public void VisitBlock(ShipGrid.BlockEntry entry)
            {
                if (entry.edi == -1) return;
                TagCompound root = schematic.ExtraBlockData[entry.edi];
                if (root.ContainsKey("id"))
                {
                    string id = root["id"].AsString();
                    if (id.StartsWith("copycats") ||  id.StartsWith("framedblocks"))
                    {
                        VisitBlock(entry, schematic.BlockPallete[entry.pid], root);
                    }
                }
            }

            public void VisitBlock(ShipGrid.BlockEntry entry, BlockState block, TagCompound compound)
            {
                Console.WriteLine(JsonSerializer.Serialize(mapper.Map<List<BlockState>>(compound)));
            }
        }
    }
}