using System;
using UnityEngine;

namespace Obsolete
{
    public unsafe class PointerSerialization : MonoBehaviour, ISerializationCallbackReceiver
    {
        public Test     Test;
        public TestData Data;

        private void Start()
        {
            Test.Number  = 1;
            Test.Pointer = (float*)0x4D2;
        }

        public void OnBeforeSerialize()
        {
            Data.Number  = Test.Number;
            Data.Pointer = new IntPtr(Test.Pointer).ToInt64();
        }

        public void OnAfterDeserialize()
        {
            Test.Number  = Data.Number;
            Test.Pointer = (float*)new IntPtr(Data.Pointer).ToPointer();
        }
    }

    [Serializable]
    public struct Test
    {
        public        int    Number;
        public unsafe float* Pointer;
    }

    [Serializable]
    public struct TestData
    {
        public int  Number;
        public long Pointer;
    }
}