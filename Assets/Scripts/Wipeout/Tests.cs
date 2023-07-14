using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

namespace Wipeout
{
    [BurstCompile]
    internal unsafe class Tests : EditorWindow
    {
        private void OnGUI()
        {
            if (GUILayout.Button("Test"))
            {
                Test();
            }
        }

        [MenuItem("Tests/Tests")]
        public static void Initialize()
        {
            GetWindow<Tests>();
        }

        private static void Test()
        {
            var floats = new[]
            {
                new[] { 1.0f, 2.0f, 3.0f },
                new[] { 4.0f, 5.0f, 6.0f },
                new[] { 7.0f, 8.0f, 9.0f }
            };

            var buffer = UnsafeBufferUtility.Allocate(floats);

            for (var i = 0; i < buffer.Count; i++)
            {
                for (var j = 0; j < buffer[i].Count; j++)
                {
                    var f = buffer[i][j];

                    Debug.Log($"{i}, {j}, {f}");
                }
            }

            foreach (var unsafeBuffer in buffer)
            {
                foreach (var f in unsafeBuffer)
                {
                    Debug.Log(f);
                }
            }

            buffer.Dispose();
            return;

            for (var i = 0; i < buffer.Count; i++)
            {
                UnsafeUtility.Free(buffer[i], Allocator.Persistent);
            }

            UnsafeUtility.Free(buffer, Allocator.Persistent);
        }

        [BurstCompile]
        public static void TestBurst(ref NativeFilter buffer)
        {
        }

        internal struct NativeFilter
        {
            public UnsafeBuffer<UnsafeBuffer<float>> Coefficients;
            public UnsafeBuffer<UnsafeBuffer<float>> DelayLines;
            public UnsafeBuffer<UnsafeBuffer<int>>   Positions;
            public UnsafeBuffer<UnsafeBuffer<int>>   Taps;
        }
    }
}