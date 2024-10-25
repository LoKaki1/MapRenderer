using System.Numerics;
using System.Runtime.InteropServices;
using MapRenderer.Common.Helpers.Memory;

namespace MapRenderer.Common.Models.Shaders;

public unsafe class ShaderValue<T>
{
    public int Location { get; set; }
    private readonly Matrix4x4* m_CacheMatrixSingle;
    private static readonly IDictionary<Type, Action<int, T>> m_Setters;
    private readonly Action<int, T> m_Setter;

    // No need to init this for every value only ones
    static ShaderValue()
    {
        m_Setters = new Dictionary<Type, Action<int, T>>()
        {
            {typeof(int), (location, value) => Gl.Uniform1(location, (value is int p) ? p : 0)},
            {typeof(double), (location, value) => Gl.Uniform1(location, (value is double p) ? p : 0)},
            {typeof(float), (location, value) => Gl.Uniform1(location, (value is float p) ? p : 0)},
            {typeof(short), (location, value) => Gl.Uniform1(location, (value is short p) ? p : 0)},
            // etc
            {
                typeof(Vector2),
                (location, value) =>
                    {
                        if (value is Vector2 v2)
                        {
                            Gl.Uniform2(location, ref v2);
                        }
                    }
            },
            {
                typeof(Vector3),
                (location, value) =>
                    {
                        if (value is Vector3 v3)
                        {
                            Gl.Uniform3(location, ref v3);
                        }
                    }
            },
            {typeof(bool), (location, value) => Gl.Uniform1(location, (value is bool p) ? p ? 1 : 0  : 0)},
            {
                typeof(System.Numerics.Matrix4x4),
                (location, value) =>
                    {
                       float* fixedData = (float*)&value;;
                       Gl.UniformMatrix4(location, 1, false, fixedData);
                    }
            }
        };


    }

    public ShaderValue(ShaderProgram shader, string name)
    {
        Location = shader.GetUniformLocation(name);
        m_CacheMatrixSingle
                 = (Matrix4x4*)Allocator.AllocZeroed(Marshal.SizeOf<Matrix4x4>());

        if (Location < 0)
            Console.WriteLine($"The {name} uniform is not used in the shader");
        var t = typeof(T);
        m_Setter = m_Setters[typeof(T)];

    }

    public void Set(T value)
    {
        // TODO We probably need some giant switch case  
        m_Setter.Invoke(Location, value);
    }
        
    // }
    // // Boolean
    // public void Set(bool v)
    // {
    //     Gl.Uniform1(Location, v ? 1 : 0);
    // }

    //TODO: generic this please
    // Matrix4    
    // public void Set(Matrix4x4 v)
    // {
    //     *m_CacheMatrixSingle = v;
    //     Gl.UniformMatrix4(Location, 1, false, (float*)m_CacheMatrixSingle);
    // }
}