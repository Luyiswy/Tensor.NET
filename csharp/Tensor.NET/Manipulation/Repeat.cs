using Tensornet.Common;
using Tensornet.Native;
using Tensornet.Exceptions;
using Tensornet.Native.Param;

namespace Tensornet{
    public static class RepeatExtension{
        /// <summary>
        /// Repeat elements of a tensor.
        /// For details, please refer to https://numpy.org/doc/stable/reference/generated/numpy.repeat.html?highlight=repeat
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="repeats"> The number of repetitions for each element. repeats is broadcasted to fit the shape of the given axis. </param>
        /// <param name="axis"> The axis along which to repeat values. </param>
        /// <returns></returns>
        public static Tensor<T> Repeat<T>(this Tensor<T> src, int repeats, int axis) where T : struct, IEquatable<T>, IConvertible
        {
            Tensor<T> res = new Tensor<T>(DeduceLayout(src.TLayout, repeats, axis));
            res.TLayout.InitContiguousLayout();
            RepeatInternal(src, res, repeats, axis);
            return res;
        }
        private unsafe static void RepeatInternal<T>(Tensor<T> src, Tensor<T> dst, int repeats, int axis) where T : struct, IEquatable<T>, IConvertible{
            RepeatParam param = new RepeatParam() { repeats = repeats, axis = axis };
            IntPtr status = NativeExecutor.Execute(NativeApi.Repeat, src.TMemory, dst.TMemory, src.TLayout, dst.TLayout, new IntPtr(&param), Tensor<T>.Provider);
            NativeStatus.AssertOK(status);
        }
        private static TensorLayout DeduceLayout(TensorLayout src, int repeats, int axis){
            TensorLayout res = new TensorLayout();
            if(repeats <= 0){
                throw new InvalidParamException("The repeats count must be a positive number.");
            }
            if (axis < 0 || axis >= src.NDim) {
                throw new InvalidParamException("Invalid param for repeat.");
            }
            res.DType = src.DType;
            res.NDim = src.NDim;
            for (int i = 0; i < src.NDim; i++) {
                res.Shape[i] = src.Shape[i];
            }
            res.Shape[axis] = src.Shape[axis] * repeats;
            return res;
        }
    }

    public static partial class Tensor{
        /// <summary>
        /// Repeat elements of a tensor.
        /// For details, please refer to https://numpy.org/doc/stable/reference/generated/numpy.repeat.html?highlight=repeat
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="repeats"> The number of repetitions for each element. repeats is broadcasted to fit the shape of the given axis. </param>
        /// <param name="axis"> The axis along which to repeat values. </param>
        /// <returns></returns>
        public static Tensor<T> Repeat<T>(Tensor<T> src, int repeats, int axis) where T : struct, IEquatable<T>, IConvertible{
            return src.Repeat(repeats, axis);
        }
    }
}