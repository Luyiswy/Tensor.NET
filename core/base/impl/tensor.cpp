#include "core/base/include/tensor.h"

namespace nncore {
void RefPtr::reset(const void* ptr, size_t offset) {
  nn_assert(m_mutable, "this RefPtr can't change.");
  *m_ref = const_cast<void*>(ptr);
  m_offset = offset;
}

void Tensor::reset_ptr(void* ptr, size_t offset) {
  m_ref_ptr.reset(ptr, offset);
}
}  // namespace nncore