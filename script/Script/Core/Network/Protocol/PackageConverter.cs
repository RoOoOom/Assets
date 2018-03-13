using NetJZWL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetJZWL
{
    /// <summary>
    /// 主要是利用反射，使对象和二进制之间进行互相转换
    /// </summary>
    public class PackageConverter
    {
        public static T PackageToObject<T>(ByteBuffer pkg)
        {
            return (T)PackageToObject(pkg, typeof(T));
        }

        /// <summary>
        /// 利用反射和特性将二进制信息转换成对象
        /// </summary>
        /// <param name="pkg">协议信息</param>
        /// <param name="type">要转换成的对象类型</param>
        /// <returns></returns>
        public static object PackageToObject(ByteBuffer pkg, Type type)
        {
            object t;
            Type elementType = type.GetElementType();
            if (elementType == null)
            {
                t = System.Activator.CreateInstance(type);
                _PackageToObject(pkg, t);
            }
            else
            {
                t = _PackageToArray(pkg, elementType);
            }
            return t;
        }
        
        /// <summary>
        /// 二进制信息转换成对象
        /// </summary>
        /// <param name="pkg"></param>
        /// <param name="o"></param>
        #region ByteBuffer
        private static void _PackageToObject(ByteBuffer pkg, object o)
        {
            try
            {
                FieldInfo[] fields = o.GetType().GetFields();
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(bool))
                    {
                        field.SetValue(o, pkg.PopByte() > 0 ? true : false);
                    }
                    else if (field.FieldType == typeof(sbyte))
                    {
                        field.SetValue(o, pkg.PopSByte());
                    }
                    else if (field.FieldType == typeof(byte))
                    {
                        field.SetValue(o, pkg.PopByte());
                    }
                    else if (field.FieldType == typeof(short))
                    {
                        field.SetValue(o, pkg.PopShort());
                    }
                    else if (field.FieldType == typeof(ushort))
                    {
                        field.SetValue(o, pkg.PopUShort());
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        field.SetValue(o, pkg.PopInt());
                    }
                    else if (field.FieldType == typeof(uint))
                    {
                        field.SetValue(o, pkg.PopUInt());
                    }
                    else if (field.FieldType == typeof(long))
                    {
                        field.SetValue(o, pkg.PopLong());
                    }
                    else if (field.FieldType == typeof(ulong))
                    {
                        field.SetValue(o, pkg.PopULong());
                    }
                    else if (field.FieldType == typeof(string))
                    {
                        field.SetValue(o, pkg.PopString());
                    }
                    else
                    {
                        Type subType = field.FieldType.GetElementType();
                        if (subType == null)
                        {
                            object fieldObject = System.Activator.CreateInstance(field.FieldType);
                            _PackageToObject(pkg, fieldObject);
                            field.SetValue(o, fieldObject);
                        }
                        else
                        {
                            field.SetValue(o, _PackageToArray(pkg, subType));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new UnityEngine.UnityException("Error: PackageToObject error -- " + e.ToString());
            }
        }
        private static bool[] _PkgToBoolArray(ByteBuffer pkg)
        {
            short count = pkg.PopShort();
            bool[] array = new bool[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = pkg.PopByte() > 0 ? true : false;
            }
            return array;
        }

        private static sbyte[] _PkgToSByteArray(ByteBuffer pkg)
        {
            short count = pkg.PopShort();
            sbyte[] array = new sbyte[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = pkg.PopSByte();
            }
            return array;
        }

        private static byte[] _PkgToByteArray(ByteBuffer pkg)
        {
            short count = pkg.PopShort();
            byte[] array = new byte[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = pkg.PopByte();
            }
            return array;
        }

        private static short[] _PkgToShortArray(ByteBuffer pkg)
        {
            short count = pkg.PopShort();
            short[] array = new short[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = pkg.PopShort();
            }
            return array;
        }

        private static ushort[] _PkgToUShortArray(ByteBuffer pkg)
        {
            short count = pkg.PopShort();
            ushort[] array = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = pkg.PopUShort();
            }
            return array;
        }

        private static int[] _PkgToIntArray(ByteBuffer pkg)
        {
            short count = pkg.PopShort();
            int[] array = new int[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = pkg.PopInt();
            }
            return array;
        }

        private static uint[] _PkgToUIntArray(ByteBuffer pkg)
        {
            short count = pkg.PopShort();
            uint[] array = new uint[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = pkg.PopUInt();
            }
            return array;
        }

        private static long[] _PkgToLongArray(ByteBuffer pkg)
        {
            short count = pkg.PopShort();
            long[] array = new long[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = pkg.PopLong();
            }
            return array;
        }

        private static ulong[] _PkgToULongArray(ByteBuffer pkg)
        {
            short count = pkg.PopShort();
            ulong[] array = new ulong[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = pkg.PopULong();
            }
            return array;
        }

        private static string[] _PkgToStringArray(ByteBuffer pkg)
        {
            short count = pkg.PopShort();
            string[] array = new string[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = pkg.PopString();
            }
            return array;
        }

        private static object _PackageToArray(ByteBuffer pkg, Type elementType)
        {
            if (elementType == typeof(bool))
            {
                return _PkgToBoolArray(pkg);
            }
            else if (elementType == typeof(sbyte))
            {
                return _PkgToSByteArray(pkg);
            }
            else if (elementType == typeof(byte))
            {
                return _PkgToByteArray(pkg);
            }
            else if (elementType == typeof(short))
            {
                return _PkgToShortArray(pkg);
            }
            else if (elementType == typeof(ushort))
            {
                return _PkgToUShortArray(pkg);
            }
            else if (elementType == typeof(int))
            {
                return _PkgToIntArray(pkg);
            }
            else if (elementType == typeof(uint))
            {
                return _PkgToUIntArray(pkg);
            }
            else if (elementType == typeof(long))
            {
                return _PkgToLongArray(pkg);
            }
            else if (elementType == typeof(ulong))
            {
                return _PkgToULongArray(pkg);
            }
            else if (elementType == typeof(string))
            {
                return _PkgToStringArray(pkg);
            }
            else
            {
                short count = pkg.PopShort();
                object[] array = Array.CreateInstance(elementType, count) as object[];
                object element;
                for (int i = 0; i < count; i++)
                {
                    element = System.Activator.CreateInstance(elementType);
                    _PackageToObject(pkg, element);
                    array[i] = element;
                }
                return array;
            }
        }
        #endregion
        /// <summary>
        /// 将对象转换为二进制
        /// </summary>
        /// <param name="o"></param>
        /// <param name="pkg"></param>
        /// <returns></returns>
        public static IMsgOutPutByteArray ObjectToPackage(object o, IMsgOutPutByteArray pkg)
        {
            if (o is Array)
            {
                _ArrayToPackage(o as Array, pkg, o.GetType());
            }
            else
            {
                _ObjectToPackage(o, pkg);
            }
            return pkg;
        }

        /// <summary>
        /// 将对象转换为二进制
        /// </summary>
        /// <param name="o"></param>
        /// <param name="pkg"></param>
        private static void _ObjectToPackage(object o, IMsgOutPutByteArray pkg)
        {
            try
            {
                FieldInfo[] fields = o.GetType().GetFields();
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(bool))
                    {
                        pkg.PushByte((bool)field.GetValue(o) == true ? (byte)1 : (byte)0);
                    }
                    else if (field.FieldType == typeof(sbyte))
                    {
                        pkg.PushSByte((sbyte)field.GetValue(o));
                    }
                    else if (field.FieldType == typeof(byte))
                    {
                        pkg.PushByte((byte)field.GetValue(o));
                    }
                    else if (field.FieldType == typeof(short))
                    {
                        pkg.PushShort((short)field.GetValue(o));
                    }
                    else if (field.FieldType == typeof(ushort))
                    {
                        pkg.PushUShort((ushort)field.GetValue(o));
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        pkg.PushInt((int)field.GetValue(o));
                    }
                    else if (field.FieldType == typeof(uint))
                    {
                        pkg.PushUInt((uint)field.GetValue(o));
                    }
                    else if (field.FieldType == typeof(long))
                    {
                        pkg.PushLong((long)field.GetValue(o));
                    }
                    else if (field.FieldType == typeof(ulong))
                    {
                        pkg.PushULong((ulong)field.GetValue(o));
                    }
                    else if (field.FieldType == typeof(string))
                    {
                        pkg.PushString((string)field.GetValue(o));
                    }
                    else
                    {
                        Type subType = field.FieldType.GetElementType();
                        if (subType == null)
                        {
                            _ObjectToPackage(field.GetValue(o), pkg);
                        }
                        else
                        {
                            _ArrayToPackage(field.GetValue(o) as Array, pkg, subType);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new UnityEngine.UnityException("Error: ObjectToPackage error -- " + e.ToString());
            }
        }


        /// <summary>
        /// 将数组转换成二进制
        /// </summary>
        /// <param name="array"></param>
        /// <param name="pkg"></param>
        /// <param name="type"></param>
        private static void _ArrayToPackage(Array array, IMsgOutPutByteArray pkg, Type type)
        {
            short count = (short)array.Length;
            pkg.PushShort(count);
            int i;
            if (type == typeof(bool))
            {
                for (i = 0; i < count; i++)
                {
                    pkg.PushByte((bool)array.GetValue(i) == true ? (byte)1 : (byte)0);
                }
            }
            else if (type == typeof(sbyte))
            {
                for (i = 0; i < count; i++)
                {
                    pkg.PushSByte((sbyte)array.GetValue(i));
                }
            }
            else if (type == typeof(byte))
            {
                for (i = 0; i < count; i++)
                {
                    pkg.PushByte((byte)array.GetValue(i));
                }
            }
            else if (type == typeof(short))
            {
                for (i = 0; i < count; i++)
                {
                    pkg.PushShort((short)array.GetValue(i));
                }
            }
            else if (type == typeof(ushort))
            {
                for (i = 0; i < count; i++)
                {
                    pkg.PushUShort((ushort)array.GetValue(i));
                }
            }
            else if (type == typeof(int))
            {
                for (i = 0; i < count; i++)
                {
                    pkg.PushInt((int)array.GetValue(i));
                }
            }
            else if (type == typeof(uint))
            {
                for (i = 0; i < count; i++)
                {
                    pkg.PushUInt((uint)array.GetValue(i));
                }
            }
            else if (type == typeof(long))
            {
                for (i = 0; i < count; i++)
                {
                    pkg.PushLong((long)array.GetValue(i));
                }
            }
            else if (type == typeof(ulong))
            {
                for (i = 0; i < count; i++)
                {
                    pkg.PushULong((ulong)array.GetValue(i));
                }
            }
            else if (type == typeof(string))
            {
                for (i = 0; i < count; i++)
                {
                    pkg.PushString((string)array.GetValue(i));
                }
            }
            else
            {
                for (i = 0; i < count; i++)
                {
                    _ObjectToPackage(array.GetValue(i), pkg);
                }
            }
        }
    }
}
