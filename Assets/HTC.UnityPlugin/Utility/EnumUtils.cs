﻿//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HTC.UnityPlugin.Utility
{
    public static class EnumUtils
    {
        public const int MASK_FIELD_LENGTH = sizeof(int) * 8;

        public class EnumDisplayInfo
        {
            public Type type { get; set; }

            public int minValue { get; set; }
            public int maxValue { get; set; }

            public string[] rawNames { get; set; }
            public int[] rawValues { get; set; }
            public Dictionary<int, int> rawValue2index { get; set; }
            public Dictionary<string, int> rawName2index { get; set; }

            public string[] displayedNames { get; set; }
            public int[] displayedValues { get; set; }
            public Dictionary<int, int> value2displayedIndex { get; set; }
            public Dictionary<string, int> name2displayedIndex { get; set; }

            public string[] displayedMaskNames { get; set; }
            public int[] displayedMaskValues { get; set; }
            public Dictionary<int, int> value2displayedMaskIndex { get; set; }
            public Dictionary<string, int> name2displayedMaskIndex { get; set; }

            public Dictionary<int, uint> value2displayedMaskField { get; set; }
            public List<uint> displayedMaskIndex2realMaskField { get; set; }

            public EnumDisplayInfo(Type type)
            {
                if (type == null) { throw new ArgumentNullException("type"); }
                if (!type.IsEnum) { throw new ArgumentException("Must be enum type", "type"); }

                rawNames = Enum.GetNames(type);
                rawValues = Enum.GetValues(type) as int[];
                rawValue2index = new Dictionary<int, int>();
                rawName2index = new Dictionary<string, int>();
                minValue = int.MaxValue;
                maxValue = int.MinValue;

                {
                    var index = 0;
                    foreach (var value in rawValues)
                    {
                        minValue = Mathf.Min(minValue, value);
                        maxValue = Mathf.Max(maxValue, value);

                        rawName2index[rawNames[index]] = index;

                        if (!rawValue2index.ContainsKey(value)) { rawValue2index[value] = index; }

                        ++index;
                    }
                }

                var displayedNamesList = new List<string>();
                var displayedValuesList = new List<int>();
                value2displayedIndex = new Dictionary<int, int>();
                name2displayedIndex = new Dictionary<string, int>();

                var displayedMaskNamesList = new List<string>();
                var displayedMaskValuesList = new List<int>();
                value2displayedMaskIndex = new Dictionary<int, int>();
                name2displayedMaskIndex = new Dictionary<string, int>();

                value2displayedMaskField = new Dictionary<int, uint>();
                displayedMaskIndex2realMaskField = new List<uint>();

                foreach (FieldInfo fi in type.GetFields()
                                             .Where(fi => fi.IsStatic && fi.GetCustomAttributes(typeof(HideInInspector), true).Length == 0)
                                             .OrderBy(fi => fi.MetadataToken))
                {
                    int index;
                    int priorIndex;
                    var name = fi.Name;
                    var value = (int)fi.GetValue(null);

                    displayedNamesList.Add(name);
                    displayedValuesList.Add(value);
                    index = displayedNamesList.Count - 1;

                    name2displayedIndex[name] = index;

                    if (!value2displayedIndex.TryGetValue(value, out priorIndex))
                    {
                        value2displayedIndex[value] = index;
                    }
                    else
                    {
                        displayedNamesList[index] += " (" + displayedNamesList[priorIndex] + ")";
                        name2displayedIndex[displayedNamesList[index]] = index;
                    }

                    if (value < 0 || value >= MASK_FIELD_LENGTH) { continue; }

                    displayedMaskNamesList.Add(name);
                    displayedMaskValuesList.Add(value);
                    index = displayedMaskNamesList.Count - 1;

                    name2displayedMaskIndex[name] = index;

                    if (!value2displayedMaskIndex.TryGetValue(value, out priorIndex))
                    {
                        value2displayedMaskIndex.Add(value, index);
                        value2displayedMaskField.Add(value, 1u << index);
                    }
                    else
                    {
                        displayedMaskNamesList[index] += " (" + displayedMaskNamesList[priorIndex] + ")";
                        name2displayedMaskIndex[displayedMaskNamesList[index]] = index;
                        value2displayedMaskField[value] |= 1u << index;
                    }

                    displayedMaskIndex2realMaskField.Add(1u << value);
                }

                displayedNames = displayedNamesList.ToArray();
                displayedValues = displayedValuesList.ToArray();
                displayedMaskNames = displayedMaskNamesList.ToArray();
                displayedMaskValues = displayedMaskValuesList.ToArray();
            }

            public int RealToDisplayedMaskField(int realMask)
            {
                var displayedMask = 0u;
                var mask = 1u;

                for (int value = 0; value < MASK_FIELD_LENGTH && realMask != 0; ++value, mask <<= 1)
                {
                    uint mk;
                    if ((realMask & mask) > 0 && value2displayedMaskField.TryGetValue(value, out mk))
                    {
                        displayedMask |= mk;
                    }
                }

                return (int)displayedMask;
            }

            public int DisplayedToRealMaskField(int displayedMask, bool fillUp = true)
            {
                var uDisMask = (uint)displayedMask;
                var realMask = 0u;

                for (int index = 0; index < displayedMaskValues.Length && uDisMask != 0; ++index)
                {
                    var mask = value2displayedMaskField[displayedMaskValues[index]];

                    if (fillUp)
                    {
                        if ((uDisMask & mask) > 0)
                        {
                            realMask |= displayedMaskIndex2realMaskField[index];
                        }
                    }
                    else
                    {
                        if ((uDisMask & mask) == mask)
                        {
                            realMask |= displayedMaskIndex2realMaskField[index];
                        }
                    }
                }

                return (int)realMask;
            }
        }

        private static Dictionary<Type, EnumDisplayInfo> s_enumInfoTable = new Dictionary<Type, EnumDisplayInfo>();

        public static EnumDisplayInfo GetDisplayInfo(Type type)
        {
            EnumDisplayInfo info;
            if (!s_enumInfoTable.TryGetValue(type, out info))
            {
                info = new EnumDisplayInfo(type);
                s_enumInfoTable.Add(type, info);
            }

            return info;
        }

        public static int GetMinValue(Type enumType)
        {
            return GetDisplayInfo(enumType).minValue;
        }

        public static int GetMaxValue(Type enumType)
        {
            return GetDisplayInfo(enumType).maxValue;
        }
    }
}