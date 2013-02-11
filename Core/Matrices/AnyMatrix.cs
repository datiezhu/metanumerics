﻿using System;


namespace Meta.Numerics.Matrices {

    /// <summary>
    /// Describes the form of all matrices.
    /// </summary>
    /// <typeparam name="T">The type of the matrix entries.</typeparam>
    public abstract class AnyMatrix<T> {

        /// <summary>
        /// Gets the number of matrix rows.
        /// </summary>
        public abstract int RowCount { get; }

        /// <summary>
        /// Gets the number of matrix columns.
        /// </summary>
        public abstract int ColumnCount { get; }

        /// <summary>
        /// Gets or sets the value of a matrix entry.
        /// </summary>
        /// <param name="r">The (zero-based) row index.</param>
        /// <param name="c">The (zero-based) column index.</param>
        /// <returns>The value of the <paramref name="r"/>,<paramref name="c"/> matrix entry.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="r"/> or <paramref name="c"/> is
        /// outside the valid range.</exception>
        public abstract T this[int r, int c] { get; set; }

        /// <summary>
        /// Sets all matrix entries according to a supplied fill function.
        /// </summary>
        /// <param name="f">The fill function.</param>
        public virtual void Fill (Func<int, int, T> f) {
            if (f == null) throw new ArgumentNullException("f");
            for (int r = 0; r < this.RowCount; r++) {
                for (int c = 0; c < this.ColumnCount; c++) {
                    this[r, c] = f(r, c);
                }
            }
        }

    }

}