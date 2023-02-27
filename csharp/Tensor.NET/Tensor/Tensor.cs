using Tensornet.Native;
using System.Text;
using Tensornet.Common;
using Tensornet.Exceptions;
using System.Collections;

namespace Tensornet{
    public partial class Tensor<T> : IEnumerable, IEnumerable<T>, ICloneable where T : struct, IEquatable<T>, IConvertible
    {
        internal TensorLayout TLayout{get; set; }
        internal TensorMemory<T> TMemory{ get; set; }
        public Span<int> Shape{
            get{
                return TLayout.Shape.AsSpan(0, TLayout.NDim);
            }
        }
        public int Dim{get { return TLayout.NDim; } }
        public DType DataType{get { return TLayout.DType; } }
        public static NativeProvider Provider { get; set; } = NativeProvider.Naive;
        internal Tensor(TensorMemory<T> memory, TensorLayout layout){
            TMemory = memory;
            TLayout = layout;
        }
        internal Tensor(TensorLayout layout){
            TLayout = layout;
            TMemory = new TensorMemory<T>(layout.TotalElemCount());
        }
        internal Tensor(TensorShape shape, DType dtype){
            TLayout = new TensorLayout(shape, dtype);
            TMemory = new TensorMemory<T>(shape.TotalElemCount());
        }
        /// <summary>
        /// Only used to generate a scalar.
        /// </summary>
        /// <param name="value"></param>
        internal Tensor(T value){
            TMemory = new TensorMemory<T>(new T[] { value });
            TLayout = new TensorLayout(new int[] { 1 }, TensorTypeInfo.GetTypeInfo(typeof(T))._dtype);
        }
        internal int IndicesToPosition(params int[] indices){
            if(indices.Length != TLayout.NDim){
                throw new InvalidArgumentException($"Index does not have same dims with tensor, " + 
                    $"the index is {indices.Length} dims but the tensor is {TLayout.NDim}.");
            }
            int res = 0;
            for (int i = 0; i < TLayout.NDim; i++) {
                if(indices[i] < -1 || indices[i] >= TLayout.Shape[i]){
                    throw new InvalidArgumentException($"{i}th index exceeds the bound of the shape. Index is {indices[i]}, limit is {TLayout.Shape[i]}.");
                }
                res += indices[i] * TLayout.Stride[i];
            }
            return res + TLayout.Offset;
        }
        public Span<T> AsSpan(){
            return TMemory.AsSpan();
        }

        public T this[params int[] indices]{
            get{
                return AsSpan()[IndicesToPosition(indices)];
            }
            set{
                AsSpan()[IndicesToPosition(indices)] = value;
            }
        }

        /// <summary>
        /// Make the tensor contiguous. Note that if the tensor is already contiguous, the method will return itself.
        /// If the tensor is not contiguous, a new contiguous tensor will be returned.
        /// </summary>
        /// <returns></returns>
        public Tensor<T> ToContiguousTensor(){
            Tensor<T> res = new Tensor<T>(new TensorLayout(TLayout as TensorShape, TLayout.DType));
            this.CopyTo(res);
            return res;
        }

        /// <summary>
        /// Copy the data from one tensor to another tensor.
        /// Please note that it will only check the total element count instead of the whole shape.
        /// </summary>
        /// <param name="target"> A contiguous target tensor</param>
        /// <exception cref="InvalidArgumentException"></exception>
        /// <exception cref="MismatchedShapeException"></exception>
        public void CopyTo(Tensor<T> target){
            if(!target.TLayout.IsContiguous()){
                throw new InvalidArgumentException("Cannot copy the data to a tensor which is not contiguous.");
            }
            if(target.TLayout.TotalElemCount() != TLayout.TotalElemCount()){
                throw new MismatchedShapeException("Cannot copy a tensor to another tensor whose total element count is different.");
            }
            var spanTarget = target.AsSpan();
            int i = 0;
            foreach(var data in this){
                spanTarget[i++] = data;
            }
        }

        // Returns an enumerator for this list with the given
        // permission for removal of elements. If modifications made to the list
        // while an enumeration is in progress, the MoveNext and
        // GetObject methods of the enumerator will throw an exception.
        //
        public TensorEnumerator<T> GetEnumerator()
            => new TensorEnumerator<T>(this);
 
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => new TensorEnumerator<T>(this);
 
        IEnumerator IEnumerable.GetEnumerator()
            => new TensorEnumerator<T>(this);

        public object Clone(){
            if(TLayout.IsContiguous()){
                return new Tensor<T>(new TensorMemory<T>(TMemory.AsSpan()), new TensorLayout(TLayout));
            }
            else{
                return ToContiguousTensor();
            }
        }

        public override string ToString()
        {
            // Somtimes the tensor is not contiguous, so we need to convert the index calculated 
            // by shape to the real index calculated by sride.
            Func<int, int> getRealPos = idx => {
                int res = 0;
                int mod = 1;
                for (int i = TLayout.NDim - 1; i >= 1; i--) mod *= TLayout.Shape[i];
                for (int i = 0; i < TLayout.NDim; i++) {
                    int shape = idx / mod;
                    idx -= shape * mod;
                    res += shape * TLayout.Stride[i];
                    if(i < TLayout.NDim - 1 ) mod /= TLayout.Shape[i + 1];
                }
                return res + TLayout.Offset;
            };

            StringBuilder r = new StringBuilder($"Tensor({TLayout.GetInfoString()}):\n");
            for (int i = 0; i < TLayout.TotalElemCount(); i++) {
                int mod = 1;
                for (int j = TLayout.NDim - 1; j >= 0; j--) {
                    mod *= TLayout.Shape[j];
                    if (i % mod == 0) {
                        r.Append("[");
                    } else {
                        break;
                    }
                }
                r.Append(" ").Append(AsSpan()[getRealPos(i)]);

                if ((i + 1) % TLayout.Shape[TLayout.NDim - 1] != 0) r.Append(",");

                r.Append(" ");
                mod = 1;
                int hit_times = 0;
                for (int j = TLayout.NDim - 1; j >= 0; j--) {
                    mod *= TLayout.Shape[j];
                    if ((i + 1) % mod == 0) {
                        r.Append("]");
                        hit_times++;
                    } else {
                        break;
                    }
                }
                if (hit_times > 0 && hit_times < TLayout.NDim) {
                    r.Append(",\n");
                    for (int j = 0; j < TLayout.NDim - hit_times; j++) {
                        r.Append(" ");
                    }
                }
            }
            // r.Append("\n");
            return r.ToString();
        }

        public static explicit operator Tensor<T>(T value) => new Tensor<T>(value);
        public static implicit operator Tensor<T>(Scalar<T> value) => new Tensor<T>(value.Value);
    }
}