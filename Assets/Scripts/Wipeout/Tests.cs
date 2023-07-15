using Unity.Burst;
using UnityEditor;
using UnityEngine;

namespace Wipeout
{
    [BurstCompile]
    internal class Tests : EditorWindow
    {
        private NativeFilterState NativeFilter1;

        private void OnGUI()
        {
            if (GUILayout.Button("Test2"))
            {
                Test2();
            }

            if (GUILayout.Button("Test3"))
            {
                Test3();
            }
        }

        [MenuItem("Tests/Tests")]
        public static void Initialize()
        {
            GetWindow<Tests>();
        }

        private static void Test2()
        {
            var doubles = new[]
            {
                new[] { 1.0f, 2.0f, 3.0f },
                new[] { 4.0f, 5.0f, 6.0f },
                new[] { 7.0f, 8.0f, 9.0f }
            };

            var doublesBuffer = UnsafeBufferUtility.Allocate(doubles);

            for (var i = 0; i < doublesBuffer.Count; i++)
            {
                for (var j = 0; j < doublesBuffer[i].Count; j++)
                {
                    var f = doublesBuffer[i][j];

                    Debug.Log($"{i}, {j}, {f}");
                }
            }

            foreach (var unsafeBuffer in doublesBuffer)
            {
                foreach (var f in unsafeBuffer)
                {
                    Debug.Log(f);
                }
            }

            doublesBuffer.Dispose();
        }

        private static void Test3()
        {
            var triples = new[]
            {
                new[]
                {
                    new[] { 1.0f, 2.0f, 3.0f },
                    new[] { 4.0f, 5.0f, 6.0f },
                    new[] { 7.0f, 8.0f, 9.0f }
                },
                new[]
                {
                    new[] { 10.0f, 11.0f, 12.0f },
                    new[] { 13.0f, 14.0f, 15.0f },
                    new[] { 16.0f, 17.0f, 18.0f }
                },
                new[]
                {
                    new[] { 19.0f, 20.0f, 21.0f },
                    new[] { 22.0f, 23.0f, 24.0f },
                    new[] { 25.0f, 26.0f, 27.0f }
                }
            };

            var triplesBuffer = UnsafeBufferUtility.Allocate(triples);

            for (var i = 0; i < triplesBuffer.Count; i++)
            {
                for (var j = 0; j < triplesBuffer[i].Count; j++)
                {
                    for (var k = 0; k < triplesBuffer[i][j].Count; k++)
                    {
                        var f = triplesBuffer[i][j][k];

                        Debug.Log($"{i}, {j}, {f}");
                    }
                }
            }

            foreach (var i in triplesBuffer)
            {
                foreach (var j in i)
                {
                    foreach (var f in j)
                    {
                        Debug.Log(f);
                    }
                }
            }

            triplesBuffer.Dispose();
        }
    }
}