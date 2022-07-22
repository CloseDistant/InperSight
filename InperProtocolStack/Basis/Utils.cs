using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace InperProtocolStack.Basis
{
    public class Utils
    {
        public static List<byte> StructToBytes<T>(T obj)
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr bufferPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(obj, bufferPtr, false);
                byte[] bytes = new byte[size];
                Marshal.Copy(bufferPtr, bytes, 0, size);
                return bytes.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error in StructToBytes ! " + ex.Message);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferPtr);
            }
        }


        public static T BytesToStruct<T>(byte[] arr, int length = 0)
        {
            int size = length <= 0 ? arr.Length : length;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            T stru = default(T);
            try
            {
                Marshal.Copy(arr, 0, ptr, size);
                stru = (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return stru;
        }
    }
}
