// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !NET10_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.InteropServices
{
    /// <summary>
    /// Backport of PinnedGCHandle for target frameworks where the BCL type is not available.
    /// </summary>
    public struct PinnedGCHandle<T> : IEquatable<PinnedGCHandle<T>>, IDisposable
        where T : class?
    {
        private GCHandle _handle;

        public PinnedGCHandle(T target)
        {
            _handle = GCHandle.Alloc(target, GCHandleType.Pinned);
        }

        public bool IsAllocated => _handle.IsAllocated;

        public T Target
        {
            get => (T)_handle.Target!;
            set
            {
                if (_handle.IsAllocated)
                {
                    _handle.Free();
                }

                _handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            }
        }

        public void Dispose()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is PinnedGCHandle<T> other && Equals(other);

        public readonly bool Equals(PinnedGCHandle<T> other) => _handle.Equals(other._handle);

        public override readonly int GetHashCode() => _handle.GetHashCode();
    }
}
#endif
