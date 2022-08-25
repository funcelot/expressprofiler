﻿using System;
using System.Collections;
using System.Collections.Generic;
using NLog.MessageTemplates;

namespace SqlServer.Logging
{
    internal class NLogMessageParameterList : IList<MessageTemplateParameter>
    {
        private readonly IList<KeyValuePair<string, object>> _parameterList;
        private static readonly NLogMessageParameterList EmptyList = new NLogMessageParameterList(new KeyValuePair<string, object>[0]);
        private static readonly NLogMessageParameterList OriginalMessageList = new NLogMessageParameterList(new[] { new KeyValuePair<string, object>(NLogLogger.OriginalFormatPropertyName, string.Empty) });

        public bool HasOriginalMessage { get { return _originalMessageIndex.HasValue; } }
        private readonly int? _originalMessageIndex;

        public bool HasComplexParameters { get { return _hasMessageTemplateCapture || _isMixedPositional; } }
        private readonly bool _hasMessageTemplateCapture;
        private readonly bool _isMixedPositional;

        public bool IsPositional { get { return _isPositional; } }
        private readonly bool _isPositional;

        public NLogMessageParameterList(IList<KeyValuePair<string, object>> parameterList)
        {
            if (IsValidParameterList(parameterList, out _originalMessageIndex, out _hasMessageTemplateCapture, out _isMixedPositional, out _isPositional))
            {
                _parameterList = parameterList;
            }
            else
            {
                _parameterList = CreateValidParameterList(parameterList);
            }
        }

        /// <summary>
        /// Create a <see cref="NLogMessageParameterList"/> if <paramref name="parameterList"/> has values, otherwise <c>null</c>
        /// </summary>
        /// <remarks>
        /// The LogMessageParameterList-constructor initiates all the parsing/scanning
        /// </remarks>
        public static NLogMessageParameterList TryParse(IList<KeyValuePair<string, object>> parameterList)
        {
            if (parameterList.Count > 1 || (parameterList.Count == 1 && !NLogLogger.OriginalFormatPropertyName.Equals(parameterList[0].Key)))
            {
                return new NLogMessageParameterList(parameterList);
            }
            else if (parameterList.Count == 1)
            {
                return OriginalMessageList; // Skip allocation
            }
            else
            {
                return EmptyList;           // Skip allocation
            }
        }

        public bool HasMessageTemplateSyntax(bool parseMessageTemplates)
        {
            return HasOriginalMessage && (HasComplexParameters || (parseMessageTemplates && Count > 0));
        }

        public string GetOriginalMessage(IList<KeyValuePair<string, object>> messageProperties)
        {
            if (messageProperties != null && _originalMessageIndex < messageProperties.Count)
            {
                return messageProperties[_originalMessageIndex.Value].Value as string;
            }
            return null;
        }

        /// <summary>
        /// Verify that the input parameterList contains non-empty key-values and the original-format-property at the end
        /// </summary>
        private static bool IsValidParameterList(IList<KeyValuePair<string, object>> parameterList, out int? originalMessageIndex, out bool hasMessageTemplateCapture, out bool isMixedPositional, out bool isPositional)
        {
            hasMessageTemplateCapture = false;
            isMixedPositional = false;
            originalMessageIndex = null;
            isPositional = false;
            string parameterName;

            for (int i = 0; i < parameterList.Count; ++i)
            {
                if (!TryGetParameterName(parameterList, i, ref originalMessageIndex, out parameterName))
                {
                    return false;
                }

                char firstChar = parameterName[0];
                if (firstChar >= '0' && firstChar <= '9')
                {
                    if (!isPositional)
                        isMixedPositional = i != 0;
                    isPositional = true;
                }
                else
                {
                    isMixedPositional = isPositional;
                    hasMessageTemplateCapture |= GetCaptureType(firstChar) != CaptureType.Normal;
                }
            }

            isPositional = isPositional && !isMixedPositional;
            return true;
        }

        private static bool TryGetParameterName(IList<KeyValuePair<string, object>> parameterList, int i, ref int? originalMessageIndex, out string parameterKey)
        {
            try
            {
                parameterKey = parameterList[i].Key;
            }
            catch (IndexOutOfRangeException ex)
            {
                // Catch a issue in MEL
                throw new FormatException(string.Format("Invalid format string. Expected {0} format parameters, but failed to lookup parameter index {1}", parameterList.Count - 1, i), ex);
            }

            if (string.IsNullOrEmpty(parameterKey))
            {
                originalMessageIndex = null;
                return false;
            }

            if (NLogLogger.OriginalFormatPropertyName.Equals(parameterKey))
            {
                if (originalMessageIndex.HasValue)
                {
                    originalMessageIndex = null;
                    return false;
                }

                originalMessageIndex = i;
            }

            return true;
        }

        /// <summary>
        /// Extract all valid properties from the input parameterList, and return them in a newly allocated list
        /// </summary>
        private static IList<KeyValuePair<string, object>> CreateValidParameterList(IList<KeyValuePair<string, object>> parameterList)
        {
            var validParameterList = new List<KeyValuePair<string, object>>(parameterList.Count);
            for (int i = 0; i < parameterList.Count; ++i)
            {
                var paramPair = parameterList[i];
                if (string.IsNullOrEmpty(paramPair.Key))
                    continue;

                if (NLogLogger.OriginalFormatPropertyName.Equals(paramPair.Key))
                {
                    continue;
                }

                validParameterList.Add(parameterList[i]);
            }
            return validParameterList;
        }

        public MessageTemplateParameter this[int index]
        {
            get
            {
                if (index >= _originalMessageIndex)
                    index += 1;

                var parameter = _parameterList[index];
                var parameterName = parameter.Key;
                var capture = GetCaptureType(parameterName[0]);
                if (capture != CaptureType.Normal)
                    parameterName = parameterName.Substring(1);
                return new MessageTemplateParameter(parameterName, parameter.Value, null, capture);
            }
            set { throw new NotSupportedException(); }
        }

        private static CaptureType GetCaptureType(char firstChar)
        {
            if (firstChar == '@')
                return CaptureType.Serialize;
            else if (firstChar == '$')
                return CaptureType.Stringify;
            else
                return CaptureType.Normal;
        }


        public int Count { get { return _parameterList.Count - (_originalMessageIndex.HasValue ? 1 : 0); } }

        public bool IsReadOnly { get { return true; } }

        public void Add(MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(MessageTemplateParameter[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; ++i)
                array[i + arrayIndex] = this[i];
        }

        public IEnumerator<MessageTemplateParameter> GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
                yield return this[i];
        }

        public int IndexOf(MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
