﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMachine.Memory;

namespace ZMachine.Story
{
    public class ZObjectTable
    {
        public ZObjectTable(byte[] data, int loadAddress)
        {
            Load(data, loadAddress);
        }

        /// <summary>
        /// Populates the Object collection from memory
        /// </summary>
        /// <param name="data"></param>
        /// <param name="loadAddress"></param>
        private void Load(byte[] data, int loadAddress)
        {
            // v1-3 object size is 9, if v4+ then object size is 14 
            const int OBJECT_DEF_SIZE = 9;
            int currentAddress = loadAddress;

            for (int i = 0; i < 31; i++)
            {
                ZObject.DefaultProperties[i] = data.GetWord(currentAddress + i * 2);
            }
            currentAddress += 2 * 31;
            // load the first object in order to determine how many objects there are
            ZObject first = new ZObject(data, currentAddress, 1);
            Objects.Add(first);

            int firstPropList = first.PropertyAddress;
            int objectCount = -(currentAddress - first.PropertyAddress) / OBJECT_DEF_SIZE;

            for (int i = 1; i < objectCount; i++)
            {
                ZObject tmp = new ZObject(data, currentAddress + i*OBJECT_DEF_SIZE, i+1);

                Objects.Add(tmp);
            }
        }

        /// <summary>
        /// All of the ZObjects in the machine
        /// </summary>
        public List<ZObject> Objects { get; } = new List<ZObject>(256);

        /// <summary>
        /// Creates a string representation of the entire object tree in memory
        /// </summary>
        /// <returns>a string representing the object tree</returns>
        public string DumpObjectTree()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var obj in Objects.Where(zo => zo.ParentID == 0))
            {
                PrintObject(obj, 0, sb);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Prints the specified object and its children to a string builder
        /// </summary>
        /// <param name="current">the object to print</param>
        /// <param name="level">the level of indention to print at</param>
        /// <param name="sb">the string builder object which contains the printed object tree</param>
        private void PrintObject(ZObject current, int level, StringBuilder sb)
        {
            for (int i = 0; i < level; i++) { sb.Append("-----"); }    // do the indention

            sb.AppendLine(current.ToString());

            if (current.ChildID != 0)
            {
                PrintObject(Objects[current.ChildID - 1], level + 1, sb);
            }

            if (current.SiblingID != 0)
            {
                PrintObject(Objects[current.SiblingID - 1], level, sb);
            }
        }
    }
}