using Numnet.Exceptions;

namespace Numnet.Native.Param{
    internal struct MatmulParam{
        
    }
    internal struct PermuteParam{
        internal IntPtr dims;
    }
    internal struct TransposeParam{
        internal int dimA;
        internal int dimB;
    }
    internal struct TypeConvertParam{
        internal DType targetType;
    }
}