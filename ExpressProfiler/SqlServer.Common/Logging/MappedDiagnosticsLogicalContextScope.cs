using System;
using System.Collections.Generic;
using System.Threading;
using NLog;

namespace Express.Logging
{
    internal class MappedDiagnosticsLogicalContextScope : IDisposable
    {
        private int _disposed;
        private IDisposable[] _disposables;

        public MappedDiagnosticsLogicalContextScope(IList<KeyValuePair<string, object>> propertyList)
        {
            _disposables = new IDisposable[propertyList.Count];
            for (var index = 0; index < propertyList.Count; index++)
            {
                var pair = propertyList[index];
                _disposables[index] = MappedDiagnosticsLogicalContext.SetScoped(pair.Key, pair.Value);
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                if (_disposables != null)
                {
                    for (var index = _disposables.Length-1; index >=0; index--)
                    {
                        var disposable = _disposables[index];
                        if (disposable != null)
                        {
                            disposable.Dispose();
                            _disposables[index] = null;
                        }
                    }
                    _disposables = null;
                }
            }
        }
    }
}
