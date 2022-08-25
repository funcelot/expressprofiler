using System;
using System.IO;
using System.Reflection;
using SqlServer.Helpers;

namespace SqlServer.Resources
{
    public class ExpressResourceManager
    {
        private readonly Assembly _assembly;
        private readonly string _prefix;

        public ExpressResourceManager(Assembly assembly, string prefix = null)
        {
            _assembly = assembly;
            _prefix = prefix;
        }

        private string GetFullName(string name)
        {
            if (_prefix == null)
            {
                return name;
            }

            return _prefix + "." + name;
        }

        private Stream ReadStream(string fileName)
        {
            var fullName = GetFullName(fileName);
            var stream = _assembly.GetManifestResourceStream(fullName);
            if (stream == null)
            {
                if (File.Exists(fileName))
                {
                    return new MemoryStream(File.ReadAllBytes(fileName));
                }
                //
                if (!Path.IsPathRooted(fileName))
                {
                    var fullPath = DirectoryHelper.GetResourceFileFullName(fileName);
                    if (File.Exists(fullPath))
                    {
                        return new MemoryStream(File.ReadAllBytes(fullPath));
                    }
                }
                throw new InvalidOperationException(string.Format("Not found resource by full name '{0}'.", fullName));
            }
            return stream;
        }

        public string Read(string name)
        {
            using (var stream = ReadStream(name))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    return result;
                }
            }
        }

        public static ExpressResourceManager Create<T>(string prefix = null)
        {
            var type = typeof(T);
            return new ExpressResourceManager(type.Assembly, prefix ?? type.Namespace);
        }

        public static ExpressResourceManager CreateDefault<T>()
        {
            var type = typeof(T);
            return new ExpressResourceManager(type.Assembly, type.Namespace + ".Resources");
        }


        public static string ReadCommon(string key)
        {
            var manager = Create<ExpressResourceManager>();

            return manager.Read(key);
        }
    }
}
