#include "core/base/include/layout.h"

#include <cstring>
#include <string>

namespace nncore {
Shape::Shape(const std::vector<size_t> &init_shape) {
  nn_assert(init_shape.size() <= MAX_NDIM,
            "The shape you specified has too many dims, which is %lu, the "
            "max is %lu\n",
            init_shape.size(), MAX_NDIM);
  ndim = init_shape.size();
  for (int i = 0; i < ndim; i++) {
    shape[i] = init_shape[ndim - 1 - i];
  }
}

Shape::Shape(const std::initializer_list<size_t> &init_shape)
    : Shape(std::vector<size_t>{init_shape}) {}

bool Shape::is_scalar() const { return ndim == 1 && shape[0] == 1; }

bool Shape::is_empty() const {
  if (ndim == 0) return true;
  for (size_t i = 0; i < ndim; i++) {
    if (shape[i] == 0) return true;
  }
  return false;
}

bool Shape::is_shape(const Shape &rhs) const {
  if (ndim != rhs.ndim) return false;
  for (size_t i = 0; i < ndim; i++) {
    if (shape[i] != rhs.shape[i]) return false;
  }
  return true;
}

bool Shape::is_equivalent_shape(const Shape &rhs) const {
  size_t min_ndim = ndim > rhs.ndim ? rhs.ndim : ndim;
  for (size_t i = 0; i < min_ndim; i++) {
    if (shape[i] != rhs.shape[i]) return false;
  }
  for (size_t i = min_ndim; i < ndim; i++) {
    if (shape[i] != 1) return false;
  }
  for (size_t i = min_ndim; i < rhs.ndim; i++) {
    if (rhs.shape[i] != 1) return false;
  }
  return true;
}

size_t Shape::count() const {
  size_t r = 1;
  for (size_t i = 0; i < ndim; i++) r *= shape[i];
  return r;
}

std::string Shape::to_string() const {
  std::string r = "{";
  if (ndim > 0) {
    for (int i = 0; i < ndim; i++) {
      r += std::to_string(shape[ndim - 1 - i]);
      if (i != ndim - 1) r += ", ";
    }
  }
  r += "}";
  return r;
}

Layout::Layout() : dtype(), format(Format::Default) {}

Layout::Layout(const DType &dtype) : dtype(dtype), format(Format::Default) {}

Layout::Layout(const Shape &shape, const DType &dtype)
    : Layout(shape, dtype, Format::Default) {}

Layout::Layout(const DType &dtype, const Format &format)
    : dtype(dtype), format(format) {}

Layout::Layout(const Shape &shape, const DType &dtype, const Format &format)
    : Shape(shape), dtype(dtype), format(format) {
  size_t s = 1;
  for (size_t i = 0; i < ndim; i++) {
    stride[i] = s;
    s *= shape[i];
  }
}

Layout::Layout(const Shape &shape, const std::vector<size_t> &stride,
               const DType &dtype)
    : Layout(shape, stride, dtype, Format::Default) {}
Layout::Layout(const Shape &shape, const std::vector<size_t> &stride,
               const DType &dtype, const Format &format)
    : Shape(shape), dtype(dtype), format(Format::Default) {
  nn_assert(shape.ndim == stride.size(),
            "Size of shape mismatched that of stride.");
  for (int i = 0; i < shape.ndim; i++) this->stride[i] = stride[ndim - i - 1];
}

bool Layout::is_same_layout(const Layout &rhs) const {
  if (dtype != rhs.dtype || format != rhs.format || !is_shape(rhs))
    return false;
  for (int i = 0; i < ndim; i++) {
    if (stride[i] != rhs.stride[i]) return false;
  }
  return true;
}

bool Layout::is_equivalent_layout(const Layout &rhs) const {
  return dtype == rhs.dtype && format == rhs.format && is_equivalent_shape(rhs);
}

std::string Layout::to_string() const {
  std::string r = "(";
  if (!ndim) {
    r += " Scalar";
  } else {
    r += "shape = {";
    for (int i = 0; i < ndim; i++) {
      r += std::to_string(shape[ndim - 1 - i]);
      if (i != ndim - 1) r += ", ";
    }
    r += "}, stride = {";
    for (int i = 0; i < ndim; i++) {
      r += std::to_string(stride[ndim - 1 - i]);
      if (i != ndim - 1) r += ", ";
    }
    r += "}";
  }
  r += ", dtype = ";
  r += dtype.name();
  r += ")";
  return r;
}

size_t Layout::content_bytes() const { return count() * dtype.size(); }

}  // namespace nncore
