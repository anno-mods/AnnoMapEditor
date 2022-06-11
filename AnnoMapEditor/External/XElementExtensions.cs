﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AnnoMapEditor.External
{
    public static class XElementExtensions
    {
        public static string? GetValueFromPath(this XElement node, string path)
        {
            XElement? currentNode = node;
            Queue<string> parts = new Queue<string>(path.Split('/'));

            while (parts.Any() && currentNode is not null)
            {
                string current = parts.Dequeue();
                currentNode = currentNode.Descendants(current)?.FirstOrDefault();
            }

            return currentNode?.Value;
        }
    }
}
