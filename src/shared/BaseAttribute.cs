﻿using System;

namespace chia.dotnet.console
{
    public abstract class BaseAttribute : Attribute
    {
        public bool Required { get; set; }

        public object? Default { get; set; }

        public string? Description { get; set; }

        public string? ArgumentHelpName { get; set; }
    }
}
