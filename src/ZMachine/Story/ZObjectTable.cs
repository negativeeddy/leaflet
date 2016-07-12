﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                ZObject tmp = new ZObject(data, currentAddress + i * OBJECT_DEF_SIZE, i + 1);

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

        /// <summary>
        /// Indicates whether an object is the child of another object
        /// </summary>
        /// <param name="parentID">the parent object's Id</param>
        /// <param name="childID">the child object's Id</param>
        /// <returns></returns>
        public bool IsValidChild(int parentID, int childID)
        {
            return Objects.Any(o => o.ID == childID && o.ParentID == parentID);
        }

        /// <summary>
        /// Returns the Id of an object's child
        /// </summary>
        /// <param name="objectID">the Id of the object whose child to find</param>
        /// <returns>the Id of the child, 0 if there is no child</returns>
        internal int GetChild(int objectID)
        {
            return Objects.FirstOrDefault(o => o.ID == objectID).ChildID;
        }

        /// <summary>
        /// Returns the Id of an object's sibling
        /// </summary>
        /// <param name="objectID">the Id of the object whose child to find</param>
        /// <returns>the Id of the child, 0 if there is no child</returns>
        internal int GetSiblingId(int objectID)
        {
            return Objects.FirstOrDefault(o => o.ID == objectID).SiblingID;
        }

        /// <summary>
        /// Returns the Id of an object's parent
        /// </summary>
        /// <param name="objectID">the Id of the object whose child to find</param>
        /// <returns>the Id of the child, 0 if there is no child</returns>
        internal int GetParent(int objectID)
        {
            return Objects.FirstOrDefault(o => o.ID == objectID).ParentID;
        }

        public ZObject GetObject(int objectID)
        {
            if (objectID == 0)
            {
                return null;
            }
            return Objects.First(o => o.ID == objectID);
        }

        internal void ReparentObject(int objectID, int newParentID)
        {
            Debug.Assert(objectID != 0, "Invalid object ID");
            Debug.Assert(objectID != newParentID, "objectID and newParentID must be different");

            ZObject movingObject = GetObject(objectID);
            ZObject oldParent = GetObject(movingObject.ParentID);
            int oldNextSiblingId = GetSiblingId(objectID);

            if (oldParent?.ChildID == objectID)
            {
                // if the old item was the first child, fix the parent
                oldParent.ChildID = oldNextSiblingId;
            }
            else
            {
                // fix the gap in the old sibling chain
                ZObject oldPrevSibling = Objects.FirstOrDefault(o => o.SiblingID == objectID);
                if (oldPrevSibling != null)
                {
                    oldPrevSibling.SiblingID = oldNextSiblingId;
                }
            }


            // set the object to be the first child of the new parent
            ZObject newParent = GetObject(newParentID);
            newParent.ChildID = objectID;
            // set the object sibling to the new parents previous first child
            int newSiblingID = newParent.ChildID;
            movingObject.SiblingID = newSiblingID;
        }
    }
}
