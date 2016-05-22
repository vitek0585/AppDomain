using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using Program.Proxy.Extractor.Extend;

namespace Program.Proxy.Extractor
{
    public class ExtractorsContainer : IExtractorsContainer, IDisposable
    {
        private string _path;
   
        private AggregateCatalog _aggregateCatalog;
        private CompositionContainer _container;

        [ImportMany(RequiredCreationPolicy = CreationPolicy.NonShared)]
        private IEnumerable<IExtractor> _extractors;

        public ExtractorsContainer(string path)
        {
            _path = path;
        }

        public void Load()
        {
            if (_container == null)
            {
                var di = new DirectoryInfo(_path);
                if (!di.Exists)
                    throw new DirectoryNotFoundException($"Directory {_path} not found");

                var dlls = di.GetFileSystemInfos("*.dll");
                _aggregateCatalog = new AggregateCatalog();

                foreach (var fi in dlls)
                {
                    try
                    {
                        var ac = new AssemblyCatalog(fi.FullName);
                        var parts = ac.Parts.ToArray();
                        _aggregateCatalog.Catalogs.Add(ac);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        //TODO LOG
                        //LogHelper.Log($"Extractor dll - {fi.FullName} is failed", ex);
                    }
                }

                _container = new CompositionContainer(_aggregateCatalog);

            }
        }
        public IEnumerable<IExtractor> GetAllExtractors()
        {
            _container.SatisfyImportsOnce(this);
            return _extractors.Distinct(new ExtractorComparer());
        }
        public IEnumerable<IExtractorExtend> GetAllExtractorsExtend()
        {
            _container.SatisfyImportsOnce(this);

            return _extractors.Select(extract => new ExtractorExtend(extract, Path.GetFileName(extract.GetType().Assembly.Location)))
                .Where(extract => extract != null).Distinct(new ExtractorExtendComparer());
        }
        #region Dispose

        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed && _aggregateCatalog != null)
            {
                _aggregateCatalog.Dispose();
                _container.Dispose();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        ~ExtractorsContainer()
        {
            Dispose();
        }

        #endregion


    }

    public class ExtractorComparer : IEqualityComparer<IExtractor>
    {
        public bool Equals(IExtractor x, IExtractor y)
        {
            return string.Equals(x.Identifier, y.Identifier, StringComparison.InvariantCulture);
        }

        public int GetHashCode(IExtractor obj)
        {
            return obj.Identifier.GetHashCode();
        }
    }
    public class ExtractorExtendComparer : IEqualityComparer<IExtractorExtend>
    {
        public bool Equals(IExtractorExtend x, IExtractorExtend y)
        {
            return string.Equals(x.Identifier, y.Identifier, StringComparison.InvariantCulture);
        }

        public int GetHashCode(IExtractorExtend obj)
        {
            return obj.Identifier.GetHashCode();
        }
    }
}