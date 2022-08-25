using System;
using System.Collections.Generic;
using NLog;
using SqlServer.Helpers;

namespace SqlServer.Logging
{
    internal class NLogBeginScopeParser
    {
        private readonly NLogOptions _options;

        //private readonly ExtractorDictionary _scopeStateExtractors = new ExtractorDictionary();

        public NLogBeginScopeParser(NLogOptions options)
        {
            _options = options;
        }

        public IDisposable ParseBeginScope<T>(T state)
        {
            if (_options.CaptureMessageProperties)
            {
                if (state is IList<KeyValuePair<string, object>>)
                {
                    return ScopeProperties.CaptureScopeProperties((IList<KeyValuePair<string, object>>)state, false);
                }

                if (!(state is string))
                {
                    return ScopeProperties.CaptureScopeProperty(state);
                    //return ScopeProperties.CaptureScopeProperty(state, _scopeStateExtractors);
                }
            }

            return NestedDiagnosticsLogicalContext.Push(state);
        }

        private sealed class ScopeProperties : IDisposable
        {
            private readonly IDisposable _mldcScope;
            private readonly IDisposable _ndlcScope;
            private ScopeProperties(IDisposable ndlcScope, IDisposable mldcScope)
            {
                _ndlcScope = ndlcScope;
                _mldcScope = mldcScope;
            }

            public void Dispose()
            {
                try
                {
                    if (_mldcScope != null)
                    {
                        _mldcScope.Dispose();
                    }

                }
                catch (Exception ex)
                {
                    NLog.Common.InternalLogger.Debug(ex, "Exception in BeginScope dispose MappedDiagnosticsLogicalContext");
                }

                try
                {
                    if (_ndlcScope != null)
                    {
                        _ndlcScope.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    NLog.Common.InternalLogger.Debug(ex, "Exception in BeginScope dispose NestedDiagnosticsLogicalContext");
                }
            }

            private static IDisposable CreateScopeProperties(object scopeObject, IList<KeyValuePair<string, object>> propertyList)
            {
                if (propertyList != null && propertyList.Count > 0)
                {
                    return new ScopeProperties(NestedDiagnosticsLogicalContext.Push(scopeObject), new MappedDiagnosticsLogicalContextScope(propertyList));
                }

                return NestedDiagnosticsLogicalContext.Push(scopeObject);
            }

            public static IDisposable CaptureScopeProperties(IList<KeyValuePair<string, object>> scopePropertyList, bool includeActivtyIdsWithBeginScope)
            {
                object scopeObject = scopePropertyList;

                if (scopePropertyList.Count > 0)
                {
                    if (NLogLogger.OriginalFormatPropertyName.Equals(scopePropertyList[scopePropertyList.Count - 1].Key))
                    {
                        scopePropertyList = ExcludeOriginalFormatProperty(scopePropertyList);
                    }
                    else if (includeActivtyIdsWithBeginScope && "RequestId".Equals(scopePropertyList[0].Key))
                    {
                        scopePropertyList = IncludeActivityIdsProperties(scopePropertyList);
                    }
                }

                return CreateScopeProperties(scopeObject, scopePropertyList);
            }

#if !NET5_0
            private static IList<KeyValuePair<string, object>> IncludeActivityIdsProperties(IList<KeyValuePair<string, object>> scopePropertyList)
            {
                return scopePropertyList;
            }
#else
            private static IReadOnlyList<KeyValuePair<string, object>> IncludeActivityIdsProperties(IReadOnlyList<KeyValuePair<string, object>> scopePropertyList)
            {
                if (scopePropertyList.Count > 1 && "RequestPath".Equals(scopePropertyList[1].Key))
                {
                    var activty = System.Diagnostics.Activity.Current;
                    if (activty != null)
                        return new ScopePropertiesWithActivityIds(scopePropertyList, activty);
                }

                return scopePropertyList;
            }

            private class ScopePropertiesWithActivityIds : IReadOnlyList<KeyValuePair<string, object>>
            {
                private readonly IReadOnlyList<KeyValuePair<string, object>> _originalPropertyList;
                private readonly System.Diagnostics.Activity _currentActivity;

                public ScopePropertiesWithActivityIds(IReadOnlyList<KeyValuePair<string, object>> originalPropertyList, System.Diagnostics.Activity currentActivity)
                {
                    _originalPropertyList = originalPropertyList;
                    _currentActivity = currentActivity;
                }

                public KeyValuePair<string, object> this[int index]
                {
                    get
                    {
                        int offset = index - _originalPropertyList.Count;
                        if (offset < 0)
                        {
                            return _originalPropertyList[index];
                        }
                        else
                        {
                            switch (offset)
                            {
                                case 0: return new KeyValuePair<string, object>(nameof(_currentActivity.SpanId), _currentActivity.GetSpanId());
                                case 1: return new KeyValuePair<string, object>(nameof(_currentActivity.TraceId), _currentActivity.GetTraceId());
                                case 2: return new KeyValuePair<string, object>(nameof(_currentActivity.ParentId), _currentActivity.GetParentId());
                            }
                        }

                        throw new ArgumentOutOfRangeException(nameof(index));
                    }
                }

                public int Count => _originalPropertyList.Count + 3;

                public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
                {
                    for (int i = 0; i < Count; ++i)
                        yield return this[i];
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return ((IEnumerable)_originalPropertyList).GetEnumerator();
                }
            }
#endif

            private static IList<KeyValuePair<string, object>> ExcludeOriginalFormatProperty(IList<KeyValuePair<string, object>> scopePropertyList)
            {
                if (scopePropertyList.Count == 2 && !NLogLogger.OriginalFormatPropertyName.Equals(scopePropertyList[0].Key))
                {
                    scopePropertyList = new[] { scopePropertyList[0] };
                }
                else if (scopePropertyList.Count <= 2)
                {
#if NET451
                    scopePropertyList = new KeyValuePair<string, object>[0];
#else
                    scopePropertyList = ArrayHelper.Empty<KeyValuePair<string, object>>();
#endif
                }
                else
                {
                    var propertyList = new List<KeyValuePair<string, object>>(scopePropertyList.Count - 1);
                    for (var i = 0; i < scopePropertyList.Count; ++i)
                    {
                        var property = scopePropertyList[i];
                        if (i == scopePropertyList.Count - 1 && i > 0 && NLogLogger.OriginalFormatPropertyName.Equals(property.Key))
                        {
                            continue; // Handle BeginScope("Hello {World}", "Earth")
                        }

                        propertyList.Add(property);
                    }
                    scopePropertyList = propertyList;
                }

                return scopePropertyList;
            }

            public static IDisposable CaptureScopeProperty<TState>(TState scopeProperty)
            {
                return NestedDiagnosticsLogicalContext.Push(scopeProperty);
            }

            //public static IDisposable CaptureScopeProperty<TState>(TState scopeProperty, ExtractorDictionary stateExtractor)
            //{
            //    if (!TryLookupExtractor(stateExtractor, scopeProperty.GetType(), out var keyValueExtractor))
            //    {
            //        return NestedDiagnosticsLogicalContext.Push(scopeProperty);
            //    }

            //    var propertyValue = TryParseKeyValueProperty(keyValueExtractor, scopeProperty);
            //    if (!propertyValue.HasValue)
            //    {
            //        return NestedDiagnosticsLogicalContext.Push(scopeProperty);
            //    }

            //    return new ScopeProperties(NestedDiagnosticsLogicalContext.Push(scopeProperty), MappedDiagnosticsLogicalContext.SetScoped(propertyValue.Value.Key, propertyValue.Value.Value));
            //}

            //private static KeyValuePair<string, object>? TryParseKeyValueProperty(KeyValuePair<Func<object, object>, Func<object, object>> keyValueExtractor, object property)
            //{
            //    string propertyName = null;

            //    try
            //    {
            //        var propertyKey = keyValueExtractor.Key.Invoke(property);
            //        propertyName = propertyKey?.ToString() ?? string.Empty;
            //        var propertyValue = keyValueExtractor.Value.Invoke(property);
            //        return new KeyValuePair<string, object>(propertyName, propertyValue);
            //    }
            //    catch (Exception ex)
            //    {
            //        InternalLogger.Debug(ex, "Exception in BeginScope add property {0}", propertyName);
            //        return null;
            //    }
            //}

            //private static bool TryLookupExtractor(ExtractorDictionary stateExtractor, Type propertyType,
            //    out KeyValuePair<Func<object, object>, Func<object, object>> keyValueExtractor)
            //{
            //    if (!stateExtractor.TryGetValue(propertyType, out keyValueExtractor))
            //    {
            //        try
            //        {
            //            return TryBuildExtractor(propertyType, out keyValueExtractor);
            //        }
            //        catch (Exception ex)
            //        {
            //            InternalLogger.Debug(ex, "Exception in BeginScope create property extractor");
            //        }
            //        finally
            //        {
            //            stateExtractor[propertyType] = keyValueExtractor;
            //        }
            //    }

            //    return keyValueExtractor.Key != null;
            //}

            //private static bool TryBuildExtractor(Type propertyType, out KeyValuePair<Func<object, object>, Func<object, object>> keyValueExtractor)
            //{
            //    keyValueExtractor = default;

            //    var itemType = propertyType.GetTypeInfo();
            //    if (!itemType.IsGenericType || itemType.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
            //    {
            //        return false;
            //    }

            //    var keyPropertyInfo = itemType.GetDeclaredProperty("Key");
            //    var valuePropertyInfo = itemType.GetDeclaredProperty("Value");
            //    if (valuePropertyInfo == null || keyPropertyInfo == null)
            //    {
            //        return false;
            //    }

            //    var keyValuePairObjParam = Expression.Parameter(typeof(object), "pair");
            //    var keyValuePairTypeParam = Expression.Convert(keyValuePairObjParam, propertyType);

            //    var propertyKeyAccess = Expression.Property(keyValuePairTypeParam, keyPropertyInfo);
            //    var propertyKeyAccessObj = Expression.Convert(propertyKeyAccess, typeof(object));
            //    var propertyKeyLambda = Expression.Lambda<Func<object, object>>(propertyKeyAccessObj, keyValuePairObjParam).Compile();

            //    var propertyValueAccess = Expression.Property(keyValuePairTypeParam, valuePropertyInfo);
            //    var propertyValueLambda = Expression.Lambda<Func<object, object>>(propertyValueAccess, keyValuePairObjParam).Compile();

            //    keyValueExtractor = new KeyValuePair<Func<object, object>, Func<object, object>>(propertyKeyLambda, propertyValueLambda);
            //    return true;
            //}

            public override string ToString()
            {
                return _ndlcScope != null ? _ndlcScope.ToString() : base.ToString();
            }
        }
    }
}
