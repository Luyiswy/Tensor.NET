using Tensornet.Exceptions;
using Tensornet.Native;
using Tensornet.Native.Param;

namespace Tensornet.Common{
    internal enum InterElemOperationType{
        Add = 1,
        Sub = 2,
        Mul = 3,
        Div = 4,
        Mod = 5,
        And = 6,
        Or = 7,
        Xor = 8
    }
    internal static class InterElemOperation{
        public static unsafe Tensor<T> Execute<T>(Tensor<T> a, Tensor<T> b, InterElemOperationType operationType) 
            where T : struct, IEquatable<T>, IConvertible{
            TensorLayout resLayout = new TensorLayout();
            resLayout.DType = TensorTypeInfo.GetTypeInfo(typeof(T))._dtype;
            resLayout.NDim = System.Math.Max(a.TLayout.NDim, b.TLayout.NDim);
            for (int i = a.TLayout.NDim - 1, j = b.TLayout.NDim - 1, idx = resLayout.NDim - 1; i >= 0 || j >= 0; i--, j--, idx--){
                if(i < 0){
                    resLayout.Shape[idx] = b.TLayout.Shape[j];
                }
                else if(j < 0){
                    resLayout.Shape[idx] = a.TLayout.Shape[i];
                }
                else if(a.TLayout.Shape[i] == b.TLayout.Shape[j]){
                    resLayout.Shape[idx] = a.TLayout.Shape[i];
                }
                else if(a.TLayout.Shape[i] == 1){
                    resLayout.Shape[idx] = b.TLayout.Shape[j];
                }
                else if(b.TLayout.Shape[j] == 1){
                    resLayout.Shape[idx] = a.TLayout.Shape[i];
                }
                else{
                    throw new MismatchedShapeException($"Cannot broadcast between the shape {a.TLayout as TensorShape} and shape {b.TLayout as TensorShape}.");
                }
            }
            resLayout.InitContiguousLayout();
            Tensor<T> tempA = a.Broadcast(resLayout);
            Tensor<T> tempB = b.Broadcast(resLayout);
            Tensor<T> res = new Tensor<T>(resLayout);
            InterelemParam p = new InterelemParam() { operationType = operationType };
            IntPtr status = NativeExecutor.Execute(NativeApi.Interelem, tempA.TMemory, tempB.TMemory, res.TMemory, tempA.TLayout, tempB.TLayout, res.TLayout, new IntPtr(&p), Tensor<T>.Provider);
            NativeStatus.AssertOK(status);
            return res;
        }
        public static unsafe Tensor<TResult> Execute<TA, TB, TResult>(Tensor<TA> a, Tensor<TB> b, InterElemOperationType operationType) 
            where TA : struct, IEquatable<TA>, IConvertible 
            where TB : struct, IEquatable<TB>, IConvertible 
            where TResult : struct, IEquatable<TResult>, IConvertible{
            TensorLayout resLayout = new TensorLayout();
            resLayout.DType = TensorTypeInfo.GetTypeInfo(typeof(TResult))._dtype;
            resLayout.NDim = System.Math.Max(a.TLayout.NDim, b.TLayout.NDim);
            for (int i = a.TLayout.NDim - 1, j = b.TLayout.NDim - 1, idx = resLayout.NDim - 1; i >= 0 || j >= 0; i--, j--, idx--){
                if(i < 0){
                    resLayout.Shape[idx] = b.TLayout.Shape[j];
                }
                else if(j < 0){
                    resLayout.Shape[idx] = a.TLayout.Shape[i];
                }
                else if(a.TLayout.Shape[i] == b.TLayout.Shape[j]){
                    resLayout.Shape[idx] = a.TLayout.Shape[i];
                }
                else if(a.TLayout.Shape[i] == 1){
                    resLayout.Shape[idx] = b.TLayout.Shape[j];
                }
                else if(b.TLayout.Shape[j] == 1){
                    resLayout.Shape[idx] = a.TLayout.Shape[i];
                }
                else{
                    throw new MismatchedShapeException($"Cannot broadcast between the shape {a.TLayout as TensorShape} and shape {b.TLayout as TensorShape}.");
                }
            }
            resLayout.InitContiguousLayout();
            Tensor<TA> tempA = a.Broadcast(resLayout);
            Tensor<TB> tempB = b.Broadcast(resLayout);
            Tensor<TResult> res = new Tensor<TResult>(resLayout);
            InterelemParam p = new InterelemParam() { operationType = operationType };
            IntPtr status = NativeExecutor.Execute(NativeApi.Interelem, tempA.TMemory, tempB.TMemory, res.TMemory, tempA.TLayout, tempB.TLayout, res.TLayout, new IntPtr(&p), Tensor<TResult>.Provider);
            NativeStatus.AssertOK(status);
            return res;
        }
        public static Tensor<TResult> Execute<TA, TB, TResult>(Tensor<TA> a, Tensor<TB> b, Func<TA, TB, TResult> operation) 
            where TA : struct, IEquatable<TA>, IConvertible 
            where TB : struct, IEquatable<TB>, IConvertible 
            where TResult : struct, IEquatable<TResult>, IConvertible{
            TensorLayout resLayout = new TensorLayout();
            resLayout.DType = TensorTypeInfo.GetTypeInfo(typeof(TResult))._dtype;
            resLayout.NDim = System.Math.Max(a.TLayout.NDim, b.TLayout.NDim);
            for (int i = a.TLayout.NDim - 1, j = b.TLayout.NDim - 1, idx = resLayout.NDim - 1; i >= 0 || j >= 0; i--, j--, idx--){
                if(i < 0){
                    resLayout.Shape[idx] = b.TLayout.Shape[j];
                }
                else if(j < 0){
                    resLayout.Shape[idx] = a.TLayout.Shape[i];
                }
                else if(a.TLayout.Shape[i] == b.TLayout.Shape[j]){
                    resLayout.Shape[idx] = a.TLayout.Shape[i];
                }
                else if(a.TLayout.Shape[i] == 1){
                    resLayout.Shape[idx] = b.TLayout.Shape[j];
                }
                else if(b.TLayout.Shape[j] == 1){
                    resLayout.Shape[idx] = a.TLayout.Shape[i];
                }
                else{
                    throw new MismatchedShapeException($"Cannot broadcast between the shape {a.TLayout as TensorShape} and shape {b.TLayout as TensorShape}.");
                }
            }
            resLayout.InitContiguousLayout();
            Tensor<TA> tempA = a.Broadcast(resLayout);
            Tensor<TB> tempB = b.Broadcast(resLayout);
            Tensor<TResult> res = new Tensor<TResult>(resLayout);
            int idxOffset = resLayout.NDim - TensorLayout.MAX_NDIM;
            Span<TResult> spanRes = res.AsSpan();
            Span<TA> spanA = a.AsSpan();
            Span<TB> spanB = b.AsSpan();
            for (int n = 0; n < (idxOffset >= 0 ? resLayout.Shape[idxOffset] : 1); n++){
                int nOffsetA = idxOffset >= 0 ? n * tempA.TLayout.Stride[idxOffset] : 0;
                int nOffsetB = idxOffset >= 0 ? n * tempB.TLayout.Stride[idxOffset] : 0;
                int nOffsetRes = idxOffset >= 0 ? n * resLayout.Stride[idxOffset] : 0;
                for (int c = 0; c < (idxOffset >= -1 ? resLayout.Shape[idxOffset + 1] : 1); c++)
                {
                    int ncOffsetA = (idxOffset >= -1 ? c * tempA.TLayout.Stride[idxOffset + 1] : 0) + nOffsetA;
                    int ncOffsetB = (idxOffset >= -1 ? c * tempB.TLayout.Stride[idxOffset + 1] : 0) + nOffsetB;
                    int ncOffsetRes = (idxOffset >= -1 ? c * resLayout.Stride[idxOffset + 1] : 0) + nOffsetRes;
                    for (int h = 0; h < (idxOffset >= -2 ? resLayout.Shape[idxOffset + 2] : 1); h++){
                        int nchOffsetA = (idxOffset >= -2 ? h * tempA.TLayout.Stride[idxOffset + 2] : 0) + ncOffsetA;
                        int nchOffsetB = (idxOffset >= -2 ? h * tempB.TLayout.Stride[idxOffset + 2] : 0) + ncOffsetB;
                        int nchOffsetRes = (idxOffset >= -2 ? h * resLayout.Stride[idxOffset + 2] : 0) + ncOffsetRes;
                        for (int w = 0; w < (idxOffset >= -3 ? resLayout.Shape[idxOffset + 3] : 1); w++){
                            spanRes[nchOffsetRes + w * resLayout.Stride[idxOffset + 3]] = 
                                operation(spanA[nchOffsetA + w * tempA.TLayout.Stride[idxOffset + 3]], 
                                          spanB[nchOffsetB + w * tempB.TLayout.Stride[idxOffset + 3]]);
                        }
                    }
                }
            }
            return res;
        }
    }
}