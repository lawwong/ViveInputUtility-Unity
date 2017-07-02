//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using System;
using UnityEngine;

namespace HTC.UnityPlugin.Utility
{
    public class OrderedEnumAttribute : PropertyAttribute
    {
        public Type overrideEnumType { get; private set; }

        public OrderedEnumAttribute(Type overrideEnumType = null)
        {
            this.overrideEnumType = overrideEnumType;
        }
    }
}