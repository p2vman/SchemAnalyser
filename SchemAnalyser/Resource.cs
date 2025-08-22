using Microsoft.Win32.SafeHandles;

namespace SchemAnalyser;

public interface IResource
{
    Stream OpenStream();
}

public class ResourceLocation
{
    public string Namespace {get; private set;}
    public string Patch {get; private set;}

    public ResourceLocation StartPrefix(string path)
    {
        return new ResourceLocation { Namespace = Namespace, Patch = path+Patch };
    }

    public ResourceLocation EndPrefix(string path)
    {
        return new ResourceLocation { Namespace = Namespace, Patch = Patch+ path };
    }

    public static ResourceLocation operator +(ResourceLocation left, string right)
    {
        return left.EndPrefix(right);;
    }
    
    public static ResourceLocation operator -(ResourceLocation left, string right)
    {
        return left.StartPrefix(right);
    }
    
    public static implicit operator ResourceLocation(string location) => ParseOrThrow(location);

    public static ResourceLocation Parse(string location)
    {
        var split = location.Split(":");
        if (split.Length == 2)
        {
            return new ResourceLocation()
            {
                Namespace = split[0],
                Patch = split[1]
            };
        }
        return null;
    }
    
    public static ResourceLocation ParseOrThrow(string location)
    {
        var split = location.Split(":");
        if (split.Length == 2)
        {
            return new ResourceLocation()
            {
                Namespace = split[0],
                Patch = split[1]
            };
        }

        throw new Exception();
    }
}

public interface IResourceMannager
{
    IResource? GetResource(string? name);
    IResource? GetResource(ResourceLocation location);
    IResource GetResourceOrThrow(string? name);
    IResource GetResourceOrThrow(ResourceLocation? location);
    string? ReadToEnd(ResourceLocation? resource);
    string? ReadToEnd(string? resource);
    string ReadToEndOrThrow(ResourceLocation? resource);
    string ReadToEndOrThrow(string? resource);
    string? ReadToEnd(IResource? resource);
    string ReadToEndOrThrow(IResource? resource);
    
    IResource? this[ResourceLocation? location] { get; }
    //IResource? this[string? location] { get; }
}

public class ResourceNotfoundException : Exception
{
    public ResourceNotfoundException(string message) : base(message)
    {
    
    }
    
    public ResourceNotfoundException() : base()
    {
    
    }
}

public class DevResourceMannager : IResourceMannager
{
    private string root;
    public DevResourceMannager(string root)
    {
        this.root = root;
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);
    }
    
    public IResource? GetResource(string? name)
    {
        if (name == null) return null;
        var path =  Path.Combine(root, name);
        if (!File.Exists(path)) return null;
        return new DevResource()
        {
            Path = path,
        };
    }

    public IResource? GetResource(ResourceLocation? location)
    {
        if (location == null) return null;
        var path = Path.Combine(root, location.Namespace, location.Patch);
        if (!File.Exists(path)) return null;
        return new DevResource()
        {
            Path = path,
        };
    }
    
    public IResource GetResourceOrThrow(string? name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var path =  Path.Combine(root, name);
        if (!File.Exists(path)) 
        {
            throw new ResourceNotfoundException(path);
        };
        return new DevResource()
        {
            Path = path,
        };
    }
    
    public IResource GetResourceOrThrow(ResourceLocation? location)
    {
        ArgumentNullException.ThrowIfNull(location);
        var path = Path.Combine(root, location.Namespace, location.Patch);
        if (!File.Exists(path)) 
        {
            throw new ResourceNotfoundException(path);
        };
        return new DevResource()
        {
            Path = path,
        };
    }

    public IResource? this[ResourceLocation? location] => GetResource(location);

    public string? ReadToEnd(ResourceLocation? resource)
    {
        return ReadToEnd(GetResource(resource));
    }
    
    public string? ReadToEnd(string? resource)
    {
        return ReadToEnd(GetResource(resource));
    }

    public string ReadToEndOrThrow(ResourceLocation? resource)
    {
        return ReadToEndOrThrow(GetResourceOrThrow(resource));
    }

    public string ReadToEndOrThrow(string? resource)
    {
        return ReadToEndOrThrow(GetResourceOrThrow(resource));
    }
    
    

    public string? ReadToEnd(IResource? resource)
    {
        if (resource == null) return null;
        using var reader = new StreamReader(resource.OpenStream());
        return reader.ReadToEnd();
    }
    
    public string ReadToEndOrThrow(IResource? resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        using var reader = new StreamReader(resource.OpenStream());
        return reader.ReadToEnd();
    }
    
    public class DevResource : IResource
    {
        public string Path {get; set;}
        
        public Stream OpenStream()
        {
            return File.Open(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}