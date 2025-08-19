using NbtToolkit;

namespace SchemAnalyser;


public interface ISchematicVisitor
{
    void Visit(Schematic schematic);
    void VisitShipGrid(ShipGrid ships);
}

public interface IShipVisitor
{
    void Visit(ShipGrid ship);
    void VisitBlock(ShipGrid.BlockEntry entry);
}

public interface ICopyCatVisitor
{
    void VisitCopyCat(ShipGrid.BlockEntry entry, BlockState block, TagCompound compound);
}

