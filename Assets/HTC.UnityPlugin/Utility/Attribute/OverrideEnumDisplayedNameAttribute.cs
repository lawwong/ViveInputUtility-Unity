//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using System;

namespace HTC.UnityPlugin.Utility
{
    public class OverrideEnumDisplayedNameAttribute : Attribute
    {
        public string DisplayedName { get; private set; }

        public OverrideEnumDisplayedNameAttribute(string displayedName)
        {
            DisplayedName = displayedName;
        }
    }
}