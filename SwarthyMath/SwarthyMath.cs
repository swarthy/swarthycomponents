//Author: Alexander Mochalin
//Copyright (c) 2013-2014 All Rights Reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SwarthyMath
{
    #region Делегаты
    /// <summary>
    /// Функция, возвращающая 1 значение, имеющая вектор в качестве параметра
    /// </summary>
    /// <param name="parametres"></param>
    /// <returns></returns>
    public delegate Decimal function1N(SVector parametres);// одно значение / несколько параметров
    /// <summary>
    /// Функция, возвращающая вектор, имеющая вектор в качестве параметра
    /// </summary>
    /// <param name="parametres"></param>
    /// <returns></returns>
    public delegate SVector functionNN(SVector parametres);// несколько значений / несколько параметров
    /// <summary>
    /// Функция возвращающая вектор, имеющая 1 параметр
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public delegate SVector functionN1(Decimal parameter);//  несколько значений / один параметр
    public delegate Decimal funcAB(Decimal alphaPreLast, Decimal betaPreLast); //для прогонки
    #endregion
    #region Матрица
    /// <summary>
    /// Класс, хранящий информацию о размере матриц
    /// </summary>
    public struct SMatrixSize
    {
        int n, m;
        public int N { get { return n; } }
        public int M { get { return m; } }
        public SMatrixSize(int N, int M)
        {
            n = N;
            m = M;
        }
        public override bool Equals(object obj)
        {
            SMatrixSize s = (SMatrixSize)obj;
            return (s.n == n && s.m == m);
        }
        public static bool operator ==(SMatrixSize s1, SMatrixSize s2)
        {
            return s1.Equals(s2);
        }
        public static bool operator !=(SMatrixSize s1, SMatrixSize s2)
        {
            return !s1.Equals(s2);
        }
        public override string ToString()
        {
            return String.Format("[{0}x{1}]", n, m);
        }
    }
    /// <summary>
    /// Класс матрицы
    /// </summary>
    public class SMatrix
    {
        Decimal[,] matr;
        public SMatrixSize Size
        {
            get
            {
                return new SMatrixSize(matr.GetLength(0), matr.GetLength(1));
            }
        }
        /// <summary>
        /// Число строк
        /// </summary>
        public int N
        {
            get
            {
                return matr.GetLength(0);
            }
        }
        /// <summary>
        /// Число столбцов
        /// </summary>
        public int M
        {
            get
            {
                return matr.GetLength(1);
            }
        }
        public SMatrix()
        {
        }
        public static implicit operator SMatrix(Decimal[,] matr)
        {
            return new SMatrix(matr);
        }
        public SMatrix(int size)
        {
            matr = new Decimal[size, size];
        }
        public SMatrix(int n, int m)
        {
            matr = new Decimal[n, m];
        }
        public SMatrix(SMatrixSize size)
        {
            matr = new Decimal[size.N, size.M];
        }
        public SMatrix(Decimal[,] Initial)
        {
            matr = Initial;
        }

        public SMatrix(SVector vect, bool row = true)
        {
            if (row)
            {
                matr = new Decimal[1, vect.Size];
                for (int j = 0; j < vect.Size; j++)
                    matr[0, j] = vect[j];
            }
            else
            {
                matr = new Decimal[vect.Size, 1];
                for (int i = 0; i < vect.Size; i++)
                    matr[i, 0] = vect[i];
            }
        }
        public SMatrix Clone()
        {
            return new SMatrix((Decimal[,])matr.Clone());
        }
        public Decimal this[int i, int j]
        {
            get { return matr[i, j]; }
            set { matr[i, j] = value; }
        }
        public void Resize(int n, int m)
        {
            Resize(new SMatrixSize(n, m));
        }
        public void Resize(SMatrixSize size)
        {
            Decimal[,] newMatr = new Decimal[size.N, size.M];
            int n = Math.Min(size.N, N);
            int m = Math.Min(size.M, M);
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    newMatr[i, j] = matr[i, j];
            matr = newMatr;
        }
        public void Clear()
        {
            for (int i = 0; i < N; i++)
                for (int j = 0; j < M; j++)
                    matr[i, j] = 0;
        }
        public SVector getColumn(int j)
        {
            SVector v = new SVector(N);
            for (int i = 0; i < N; i++)
                v[i] = matr[i, j];
            return v;
        }
        public SVector getRow(int i)
        {
            SVector v = new SVector(M);
            for (int j = 0; j < M; j++)
                v[j] = matr[i, j];
            return v;
        }
        public void setColumn(int col, SVector value)
        {
            if (value.Size != N)
                throw new Exception("Невозможно задать столбец. Размерности не совпадают");
            for (int i = 0; i < N; i++)
                matr[i, col] = value[i];
        }
        public void setRow(int row, SVector value)
        {
            if (value.Size != M)
                throw new Exception("Невозможно задать строку. Размерности не совпадают");
            for (int j = 0; j < M; j++)
                matr[row, j] = value[j];
        }
        public void addColumn(SVector value)
        {
            if (value.Size != M)
                throw new Exception(string.Format("Нельзя добавить строку длиной {0} в матрицу размером {1}x{2}", value.Size, N, M));
            Decimal[,] newMatrix = new Decimal[N, M + 1];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                    newMatrix[i, j] = matr[i, j];
                newMatrix[i, M] = value[i];
            }
            matr = newMatrix;
        }
        public void addRow(SVector value)
        {
            if (value.Size != M)
                throw new Exception(string.Format("Нельзя добавить строку длиной {0} в матрицу размером {1}x{2}", value.Size, N, M));
            Decimal[,] newMatrix = new Decimal[N + 1, M];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < M; j++)
                {
                    newMatrix[i, j] = matr[i, j];
                    newMatrix[N, j] = value[j];
                }
            matr = newMatrix;
        }
        public void removeColumn(int col)
        {
            Decimal[,] newMatr = new Decimal[N, M - 1];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < M - 1; j++)
                    newMatr[i, j] = matr[i, j >= col ? j + 1 : j];
            matr = newMatr;
        }
        public void removeRow(int row)
        {
            Decimal[,] newMatr = new Decimal[N - 1, M];
            for (int i = 0; i < N - 1; i++)
                for (int j = 0; j < M; j++)
                    newMatr[i, j] = matr[i >= row ? i + 1 : i, j];
            matr = newMatr;
        }
        public void swapColumns(int col1, int col2)
        {
            SVector temp = getColumn(col1);
            setColumn(col1, getColumn(col2));
            setColumn(col2, temp);
        }
        public void swapRows(int row1, int row2)
        {
            SVector temp = getRow(row1);
            setRow(row1, getRow(row2));
            setRow(row2, temp);
        }
        public List<Decimal> ListOfElements
        {
            get
            {
                List<Decimal> res = new List<decimal>();
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < M; j++)
                        res.Add(matr[i, j]);
                return res;
            }
        }
        /// <summary>
        /// Удаляет из матрицы нулевые столбцы и строки
        /// </summary>
        public void Clean()
        {
            List<int> needDeleteRows = new List<int>();
            List<int> needDeleteCols = new List<int>();
            for (int i = 0; i < N; i++)
            {
                bool isNull = true;
                for (int j = 0; j < M; j++)
                {
                    if (matr[i, j] != 0)
                    {
                        isNull = false;
                        break;
                    }
                }
                if (isNull)
                    needDeleteRows.Add(i);
            }
            for (int j = 0; j < M; j++)
            {
                bool isNull = true;
                for (int i = 0; i < N; i++)
                {
                    if (matr[i, j] != 0)
                    {
                        isNull = false;
                        break;
                    }
                }
                if (isNull)
                    needDeleteCols.Add(j);
            }
            needDeleteRows.ForEach(i => this.removeRow(i));
            needDeleteCols.ForEach(j => this.removeColumn(j));
        }
        /// <summary>
        /// Транспонированная матрица.
        /// </summary>
        public SMatrix Transpose
        {
            get
            {
                SMatrix s = new SMatrix(M, N);
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < M; j++)
                        s[j, i] = matr[i, j];
                return s;
            }
        }
        /// <summary>
        /// Обратная матрица.        
        /// </summary>
        public SMatrix Inverse
        {
            get
            {
                if (N != M)
                    throw new Exception("Попытка обратить неквадратную матрицу");
                int n = N;
                Decimal[,] E = new Decimal[N, N];
                for (int i = 0; i < n; i++)
                    E[i, i] = 1;
                Decimal[,] A = (Decimal[,])matr.Clone();
                for (int i = 0; i < n; i++)
                {
                    Decimal prevA = A[i, i];
                    for (int j = 0; j < n; j++)
                    {
                        A[i, j] /= prevA;
                        E[i, j] /= prevA;
                    }
                    for (int k = i + 1; k < n; k++)
                    {
                        Decimal koef = A[k, i];
                        for (int j = 0; j < n; j++)
                        {
                            A[k, j] -= koef * A[i, j];
                            E[k, j] -= koef * E[i, j];
                        }
                    }
                }

                for (int k = n - 1; k > 0; k--)
                {
                    for (int i = k - 1; i >= 0; i--)
                    {
                        Decimal koef = A[i, k];

                        for (int j = 0; j < n; j++)
                        {
                            A[i, j] -= A[k, j] * koef;
                            E[i, j] -= E[k, j] * koef;
                        }
                    }
                }
                return new SMatrix(E);
            }
        }
        /// <summary>
        /// Определитель матрицы.
        /// </summary>
        public Decimal Determinant
        {
            get
            {
                if (N != M)
                    throw new Exception("Невозможно найти определитель неквадратной матрицы");
                Decimal det = 1;
                int n = N;
                Decimal[,] A = (Decimal[,])matr.Clone();
                for (int i = 0; i < n; i++)
                {
                    det *= A[i, i];
                    Decimal prevA = A[i, i];
                    for (int j = i; j < n; j++)
                        A[i, j] /= prevA;
                    for (int k = i + 1; k < n; k++)
                    {
                        Decimal koef = A[k, i];
                        for (int j = 0; j < n; j++)
                            A[k, j] -= koef * A[i, j];
                    }
                }
                return det;
            }
        }
        /// <summary>
        /// Возвращает матрицу, приведенную к ступенчатому виду с помощью элементарных преобразований.
        /// </summary>
        public SMatrix getStepMatrix
        {
            get
            {
                throw new Exception("НЕРАБОТАЕТ. ПЕРЕДЕЛАТЬ. 22.06.13. Методы оптимизации послезавтра, некогда как бэ.");
                SMatrix A = Clone();
                for (int i = 0; i < A.N; i++)
                {
                    int indx = A.getRow(i).getList.FindIndex(colInRow => colInRow != 0);
                    if (indx == -1)
                    {
                        A.removeRow(i);
                        i--;
                        continue;
                    }
                    if (indx != i)
                        A.swapColumns(indx, i);
                    Decimal prevA = A[i, i];
                    for (int j = i; j < A.M; j++)
                        A[i, j] /= prevA;
                    for (int k = i + 1; k < A.N; k++)
                    {
                        Decimal koef = A[k, i];
                        for (int j = 0; j < A.M; j++)
                            A[k, j] -= koef * A[i, j];
                    }
                }
                return A;
            }
        }
        /// <summary>
        /// Угловые миноры квадратной матрицы
        /// </summary>
        public SVector CornerMinors
        {
            get
            {
                if (N != M)
                    throw new Exception("Невозможно найти угловые миноры неквадратной матрицы");
                List<Decimal> minors = new List<decimal>();
                SMatrix m = this.Clone();
                for (int i = 0; i < N - 1; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        m.removeColumn(N - j);
                        m.removeRow(N - j);
                    }
                    minors.Add(m.Determinant);
                }
                minors.Reverse();
                return new SVector(minors.ToArray());
            }
        }
        /// <summary>
        /// Максимальное из чисел, полученных при сложении всех элементов каждого столбца, взятых по модулю.
        /// </summary>
        public Decimal Norm1
        {
            get
            {
                List<Decimal> sum = new List<Decimal>(new Decimal[M]);
                for (int j = 0; j < M; j++)
                    for (int i = 0; i < N; i++)
                        sum[j] += Math.Abs(matr[i, j]);
                return sum.Max();
            }
        }
        /// <summary>
        /// Квадратный корень из суммы квадратов всех элементов матрицы.
        /// </summary>
        public Decimal Norm2
        {
            get
            {
                Decimal quad = 0;
                ListOfElements.ForEach(e => quad += e * e);
                return (Decimal)Math.Sqrt((double)quad);
            }
        }
        /// <summary>
        /// Максимальное из чисел, полученных при сложении всех элементов каждой строки, взятых по модулю.
        /// </summary>
        public Decimal Norm3
        {
            get
            {
                List<Decimal> sum = new List<Decimal>(new Decimal[N]);
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < M; j++)
                        sum[i] += Math.Abs(matr[i, j]);
                return sum.Max();
            }
        }


        /// <summary>
        /// Единичная матрица размера size x size
        /// </summary>
        /// <param name="size">Размер матрицы</param>
        /// <returns></returns>
        public static SMatrix Identity(int size)
        {
            SMatrix s = new SMatrix(size);
            for (int i = 0; i < size; i++)
                s[i, i] = 1;
            return s;
        }
        /// <summary>
        /// Пфф.... как бы это не звучало, но, берем меньшую матрицу, натягиваем на бОльшую - возвращаем элементы бОльшей, которые находятся в точках пересечения натянутой матрицы с бОльшей. Как-то так...
        /// </summary>
        /// <param name="big">бОльшая матрица</param>
        /// <param name="small">меньшая матрица</param>
        /// <returns></returns>
        public static SMatrix Scale(SMatrix big, SMatrix small)
        {
            SMatrixSize bigSize = big.Size, smallSize = small.Size;
            if (bigSize.N < smallSize.N || bigSize.M < smallSize.M)
                throw new Exception("Большая матрица имеет меньший размер");
            SMatrix newMatr = new SMatrix(smallSize);
            if (bigSize.N % smallSize.N == 0 && bigSize.M % smallSize.M == 0)
            {
                //можем найти общие точки
                int dx = bigSize.M / smallSize.M, dt = bigSize.N / smallSize.N;
                for (int i = 0; i < smallSize.N; i++)
                    for (int j = 0; j < smallSize.M; j++)
                        newMatr[i, j] = big[i * dt, j * dx];
            }
            return newMatr;
        }

        public static SMatrix operator +(SMatrix A, SMatrix B)
        {
            if (A.Size != B.Size)
                throw new IndexOutOfRangeException(String.Format("Сложение невозможно. Матрицы должны иметь одинаковый размер ({0}, {1}).", A.Size.ToString(), B.Size.ToString()));
            SMatrix s = new SMatrix(A.Size);
            for (int i = 0; i < A.N; i++)
                for (int j = 0; j < A.M; j++)
                    s[i, j] = A[i, j] + B[i, j];
            return s;
        }
        public static SMatrix operator -(SMatrix A, SMatrix B)
        {
            if (A.Size != B.Size)
                throw new IndexOutOfRangeException(String.Format("Вычитание невозможно. Матрицы должны иметь одинаковый размер ({0}, {1}).", A.Size.ToString(), B.Size.ToString()));
            SMatrix s = new SMatrix(A.Size);
            for (int i = 0; i < A.N; i++)
                for (int j = 0; j < A.M; j++)
                    s[i, j] = A[i, j] - B[i, j];
            return s;
        }
        public static SMatrix operator *(SMatrix A, SMatrix B)
        {
            if (A.M != B.N)
                throw new IndexOutOfRangeException(String.Format("Умножение невозможно. Количество столбцов первой матрицы должно быть равно количеству строк второй ({0}, {1}).", A.Size.ToString(), B.Size.ToString()));
            SMatrix s = new SMatrix(A.N, B.M);
            for (int i = 0; i < A.N; i++)
                for (int j = 0; j < B.M; j++)
                    for (int k = 0; k < B.N; k++)
                        s[i, j] += A[i, k] * B[k, j];
            return s;
        }

        public static SMatrix operator *(SMatrix A, Decimal b)
        {
            SMatrix result = A.Clone();
            for (int i = 0; i < A.N; i++)
                for (int j = 0; j < A.M; j++)
                    result[i, j] *= b;
            return result;
        }

        public override string ToString()
        {
            return ToString(3);
        }
        public string ToString(int DigitsAfterPoint = 3)
        {
            string s = "";
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                    s += string.Format("{0}{1}", Math.Round(matr[i, j], DigitsAfterPoint), j + 1 == M ? "" : " ");
                //s += string.Format("{0:0.0}{1}", matr[i, j], j + 1 == M ? "" : " ");
                s += "\r\n";
            }
            return s;
        }
        public string ToHTMLTable()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<table style=\"border: 1px solid black;\">");
            sb.AppendLine("\t<tr style=\"page-break-inside:avoid; page-break-after:auto\">");
            for (int j = 0; j < M; j++)
                sb.AppendFormat("\t\t<th>{0}</th>\r\n", j + 1);
            sb.AppendLine("\t</tr>");
            for (int i = 0; i < N; i++)
            {
                sb.AppendLine("\t<tr style=\"page-break-inside:avoid; page-break-after:auto\">");
                for (int j = 0; j < M; j++)
                {
                    sb.AppendFormat("\t\t<td>{0:0.0}</td>\r\n", matr[i, j]);
                }
                sb.AppendLine("\t</tr>");
            }
            sb.AppendLine("</table>");
            return sb.ToString();
        }
        public Decimal maxABS
        {
            get
            {
                Decimal max = 0;
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < M; j++)
                        if (Math.Abs(matr[i, j]) > max)
                            max = Math.Abs(matr[i, j]);
                return max;
            }
        }
        public static SMatrix Parse(string[] source, char separator = ' ')
        {
            int n = source.Length;
            int m = source[0].Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries).Length;
            if (n <= 0 || m <= 0)
                throw new Exception(string.Format("Неверный размер матрицы ({0}x{1})", n, m));
            SMatrix result = new SMatrix(n, m);
            decimal temp;
            for (int i = 0; i < n; i++)
            {
                string[] nums = source[i].Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                if (nums.Length != m)
                    throw new Exception(string.Format("Ошибка чтения строки {0} - несовпадение с размером первой строки ({1}!={2})", i, nums.Length, m));
                for (int j = 0; j < m; j++)
                {
                    if (!decimal.TryParse(nums[j], out temp))
                        throw new Exception(string.Format("Неверный формат \"{0}\" в [{1}, {2}]", nums[j], i, j));
                    else
                        result[i, j] = temp;
                }
            }
            return result;
        }
    }
    /// <summary>
    /// Класс вектора
    /// </summary>
    #endregion
    #region Вектор
    public class SVector
    {
        Decimal[] vector;
        public SVector()
        {
        }
        public SVector Clone()
        {
            return new SVector(vector.ToArray());
        }
        public static implicit operator SVector(decimal[] arr)
        {
            return new SVector(arr);
        }
        public SVector(int size)
        {
            vector = new Decimal[size];
        }
        public SVector(params Decimal[] Initial)
        {
            vector = Initial;
        }
        public int Size
        {
            get
            {
                return vector.Length;
            }
        }
        public List<Decimal> getList
        {
            get
            {
                return vector.ToList();
            }
        }
        public Decimal this[int i]
        {
            get { return vector[i]; }
            set { vector[i] = value; }
        }
        public void Resize(int size)
        {
            Array.Resize(ref vector, size);
        }
        public void RemoveAt(int index)
        {
            List<Decimal> temp = getList;
            temp.RemoveAt(index);
            vector = temp.ToArray();
        }
        public int MinInd
        {
            get
            {
                int min = 0;
                for (int i = 0; i < Size; i++)
                    if (this[min] > this[i])
                        min = i;
                return min;
            }
        }
        public int MaxInd
        {
            get
            {
                int max = 0;
                for (int i = 0; i < Size; i++)
                    if (this[max] < this[i])
                        max = i;
                return max;
            }
        }
        public SVector getSubById(params int[] indecies)
        {
            Decimal[] r = new Decimal[indecies.Length];
            for (int i = 0; i < indecies.Length; i++)
                r[i] = vector[indecies[i]];
            return new SVector(r);
        }
        public override string ToString()
        {
            return ToString(3);
        }
        public string ToString(int DigitsAfterPoint = 3)
        {
            string answ = "";
            getList.ForEach(coord => answ += Math.Round(coord, DigitsAfterPoint).ToString() + " ");
            return answ;
        }
        public string ToHTMLTable(bool markNonZero = false, int maxCol = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<table style=\"border: 1px solid black; page-break-inside:auto\">");
            /*sb.AppendLine("\t<tr style=\"page-break-inside:avoid; page-break-after:auto\">");
            for (int j = 0; j < Size; j++)
                sb.AppendFormat("\t\t<th>{0}</th>\r\n", j);
            sb.AppendLine("\t</tr>");
            */
            sb.AppendLine("\t<tr style=\"page-break-inside:avoid; page-break-after:auto\">");
            for (int j = 0; j < Size; j++)
            {
                sb.AppendFormat("\t\t<td>{0}</td>\r\n", Math.Abs(vector[j]) > 0 && markNonZero ? string.Format("<span style='color: red; font-size: 14px;'>{0}</span>", vector[j]) : string.Format("{0}", vector[j]));
                if (maxCol > 0 && (j + 1) % maxCol == 0)
                    sb.AppendLine("</tr><tr>");
            }
            sb.AppendLine("\t</tr>");

            sb.AppendLine("</table>");
            return sb.ToString();
        }
        /// <summary>
        /// Эвклидова норма
        /// </summary>
        public Decimal Norm2
        {
            get
            {
                Decimal s = 0;
                vector.ToList().ForEach(coord => s += coord * coord);
                return (Decimal)Math.Sqrt((double)s);
            }
        }
        /// <summary>
        /// Математическое ожидание
        /// </summary>
        public Decimal m
        {
            get
            {
                Decimal M = 0;
                M = vector.ToList().Sum();
                M /= vector.Length;
                return M;
            }
        }
        public Decimal D
        {
            get
            {
                Decimal _m = this.m;
                decimal d = 0;
                vector.ToList().ForEach(coord => d += (coord - m) * (coord - m));
                return d / (vector.Length - 1);
            }
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            SVector b = (SVector)obj;
            if (b.Size != Size)
                return false;
            for (int i = 0; i < Size; i++)
                if (b[i] != vector[i])
                    return false;
            return true;
        }
        public static bool operator ==(SVector A, SVector B)
        {
            if ((object)A == null)
                return false;
            return A.Equals(B);
        }
        public static bool operator !=(SVector A, SVector B)
        {
            if ((object)A == null)
                return false;
            return !A.Equals(B);
        }
        public static SVector operator +(SVector A, SVector B)
        {
            if (A.Size != B.Size)
                throw new IndexOutOfRangeException(string.Format("Сложение векторов разной размерности невозможно [{0} и {1}]", A.Size, B.Size));
            SVector v = new SVector(A.Size);
            for (int i = 0; i < A.Size; i++)
                v[i] = A[i] + B[i];
            return v;
        }
        public static SVector operator -(SVector A, SVector B)
        {
            if (A.Size != B.Size)
                throw new IndexOutOfRangeException(string.Format("Вычитание векторов разной размерности невозможно [{0} и {1}]", A.Size, B.Size));
            SVector v = new SVector(A.Size);
            for (int i = 0; i < A.Size; i++)
                v[i] = A[i] - B[i];
            return v;
        }
        public static SVector operator *(SVector A, SVector B)
        {
            if (A.Size != B.Size)
                throw new IndexOutOfRangeException(string.Format("Поэлементное произведение векторов разной размерности невозможно [{0} и {1}]", A.Size, B.Size));
            SVector v = new SVector(A.Size);
            for (int i = 0; i < A.Size; i++)
                v[i] = A[i] * B[i];
            return v;
        }
        public static SVector operator /(SVector A, SVector B)
        {
            if (A.Size != B.Size)
                throw new IndexOutOfRangeException(string.Format("Поэлементное частное векторов разной размерности невозможно [{0} и {1}]", A.Size, B.Size));
            SVector v = new SVector(A.Size);
            for (int i = 0; i < A.Size; i++)
                v[i] = A[i] / B[i];
            return v;
        }

        public static SVector operator *(SVector A, Decimal B)
        {
            SVector v = A.Clone();
            for (int i = 0; i < A.Size; i++)
                v[i] *= B;
            return v;
        }
        public static SVector operator /(SVector A, Decimal B)
        {
            SVector v = A.Clone();
            for (int i = 0; i < A.Size; i++)
                v[i] /= B;
            return v;
        }
        public static SVector operator *(Decimal B, SVector A)
        {
            return A * B;
        }
        public static SVector operator *(SMatrix m, SVector vect)
        {
            if (m.M != vect.Size)
                throw new IndexOutOfRangeException(string.Format("Невозможно умножить матрицу с {0} столбцом(ами) на вектор из {1} элементов", m.M, vect.Size));
            SVector v = new SVector(m.N);
            for (int i = 0; i < m.N; i++)
                for (int k = 0; k < vect.Size; k++)
                    v[i] += m[i, k] * vect[k];
            return v;
        }
        /// <summary>
        /// Скалярное произведение
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Decimal Dot(SVector A, SVector B)
        {
            if (A.Size != B.Size)
                throw new IndexOutOfRangeException(string.Format("Нахождение скалярного произведения разной размерности невозможно [{0} и {1}]", A.Size, B.Size));
            Decimal dot = 0;
            for (int i = 0; i < A.Size; i++)
                dot += A[i] * B[i];
            return dot;
        }
        public void Fill(Decimal value, int startIndex, int endIndex)
        {
            if (startIndex >= Size || startIndex < 0)
                throw new IndexOutOfRangeException("Начальный индекс задан неверно");
            if (endIndex >= Size || endIndex < 0)
                throw new IndexOutOfRangeException("Конечный индекс задан неверно");
            for (int i = startIndex; i <= endIndex; i++)
                vector[i] = value;
        }
        public void Fill(Decimal value, int startIndex)
        {
            Fill(value, startIndex, Size - 1);
        }
        public void Fill(Decimal value)
        {
            Fill(value, 0, Size - 1);
        }
        public void AddBefore(params Decimal[] before)
        {
            Decimal[] temp = new Decimal[vector.Length + before.Length];
            before.CopyTo(temp, 0);
            vector.CopyTo(temp, before.Length);
            vector = temp;
        }
        public void AddAfter(params Decimal[] after)
        {
            Decimal[] temp = new Decimal[vector.Length + after.Length];
            vector.CopyTo(temp, 0);
            after.CopyTo(temp, vector.Length);
            vector = temp;
        }
        public static SVector Parse(string str, char separator = ' ')
        {
            try
            {
                //return new SVector(str.Trim().Split(' ').ToList().ConvertAll<Decimal>(s => Decimal.Parse(s)).ToArray());
                string[] lines = str.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                Decimal[] arr = lines.ToList().ConvertAll<Decimal>(s => Decimal.Parse(s)).ToArray();
                return new SVector(arr);
            }
            catch (Exception e)
            {
                throw new Exception("Невозможно преобразовать строку в вектор: " + e.Message);
            }
        }
        public static List<SVector> Generate(int n, int step)
        {
            if (n <= 0)
                throw new Exception("Некорректный размер вектора");

            List<SVector> result = new List<SVector>();
            result.Add(new SVector(Layer.conv(Layer.Convert(step, step + 1, n), step)));
            Layer layer;
            if (n == 1)
                return result;
            else
                layer = Layer.GetL2(step);

            for (int i = 2; i <= n; i++)
            {
                for (int j = 0; j < layer.elements.Count; j++)
                    result.Add(new SVector(Layer.conv(Layer.Convert(step * layer.elements[j], step + 1, n), step)));
                if (i < n)
                    layer = layer.GenerateNext(i != 2);
            }
            return result;
        }
        public static void Generate(int n, int step, string filename)
        {
            if (n <= 0)
                throw new Exception("Некорректный размер вектора");
            File.WriteAllText(filename, "");
            var start = DateTime.Now;
            List<SVector> result = new List<SVector>();
            result.Add(new SVector(Layer.conv(Layer.Convert(step, step + 1, n), step)));
            Layer layer;
            int size = (int)Math.Log10(step);
            if (n == 1)
            {
                File.WriteAllText(filename, result[0].ToString(size));
                return;
            }
            else
                layer = Layer.GetL2(step);

            for (int i = 2; i <= n; i++)
            {
                for (int j = 0; j < layer.elements.Count; j++)
                    if (filename.Length > 0 && (result.Count == 1000000 || j == layer.elements.Count - 1))
                    {
                        Console.Title = string.Format("Слой {0}/{1}. {2}% ({3}/{4})", i, n, (float)j / layer.elements.Count * 100, j, layer.elements.Count);
                        StringBuilder sb = new StringBuilder();
                        foreach (var t in result)
                            sb.AppendLine(t.ToString(size));
                        File.AppendAllText(filename, sb.ToString());
                        result.Clear();
                    }
                    else
                        result.Add(new SVector(Layer.conv(Layer.Convert(step * layer.elements[j], step + 1, n), step)));
                if (i < n)
                    layer = layer.GenerateNext(i != 2);
            }
            File.AppendAllText(filename, string.Format("Время: {0}", DateTime.Now - start));
        }

    }
    #endregion
    #region Генерация единичных векторов
    internal class Layer
    {
        internal int n;
        internal int[] keys;
        internal List<Space> spaces = new List<Space>();

        internal List<int> elements = new List<int>();
        public Layer(int N) { n = N; keys = new int[n]; }
        public Layer GenerateNext(bool copysubs = true)
        {
            Layer newlayer = new Layer(n);

            for (int i = 0; i < n; i++)
            {
                int oldcount = newlayer.elements.Count;

                var nspace = new Space(elements[spaces[keys[i]].Start], oldcount, oldcount + spaces[keys[i]].End - spaces[keys[i]].Start);
                newlayer.keys[i] = newlayer.spaces.Count;
                newlayer.spaces.Add(nspace);

                newlayer.elements.Add((i == 0 ? elements.Last() : newlayer.elements.Last()) + nspace.Value);

                for (int j = 1; j < spaces[keys[i]].End - spaces[keys[i]].Start + 1; j++)
                {
                    newlayer.elements.Add(newlayer.elements.Last() + 1);
                }

                if (copysubs)
                    for (int k = keys[i] + 1; k < spaces.Count; k++)
                    {
                        oldcount = newlayer.elements.Count;
                        var n2space = new Space(spaces[k].Value, oldcount, oldcount + spaces[k].End - spaces[k].Start);
                        newlayer.spaces.Add(n2space);

                        newlayer.elements.Add(newlayer.elements.Last() + n2space.Value);

                        for (int j = 1; j < spaces[k].End - spaces[k].Start + 1; j++)
                        {
                            newlayer.elements.Add(newlayer.elements.Last() + 1);
                        }
                    }

            }

            return newlayer;
        }
        internal static Layer GetL2(int n)
        {
            Layer l2 = new Layer(n);
            for (int i = 0; i < n; i++)
            {
                l2.keys[i] = i;
                l2.spaces.Add(new Space(1, i, n - 1));
                l2.elements.Add(i + 2);
            }
            return l2;
        }
        internal static Layer GetL1(int n)
        {
            Layer l1 = new Layer(n);
            for (int i = 0; i < n; i++)
                l1.keys[i] = 0;
            l1.spaces.Add(new Space(1, 0, 0));
            l1.elements.Add(1);
            return l1;
        }
        internal static int[] Convert(int x, int Base, int count)
        {
            int[] res = new int[count];
            int i = 0;
            while (x != 0)
            {
                if (i >= count)
                    throw new OverflowException("Перевод числа в др систему счисления: не умещается");
                res[count - ++i] = x % Base;
                x /= Base;
            }

            return res;
        }
        internal static decimal[] conv(int[] arr, int bs)
        {
            decimal[] r = new decimal[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                r[i] = (decimal)arr[i] / bs;
            return r;
        }
    }
    internal struct Space
    {
        internal int Value;
        internal int Start, End;
        internal int Length
        {
            get
            {
                return End - Start + 1;
            }
        }
        internal Space(int value, int start, int end)
        {
            Value = value;
            Start = start;
            End = end;
        }
        public override string ToString()
        {
            return string.Format("[{0}] {1}-{2}", Value, Start, End);
        }
    }
#endregion
    #region SwarthyHelper
    /// <summary>
    /// Класс, хранящих полезные методы
    /// </summary>
    public static class SwarthyHelper
    {
        #region НОД/НОК
        /// <summary>
        /// Наибольший Общий Делитель
        /// </summary>
        /// <param name="a">Число1</param>
        /// <param name="b">Число2</param>
        /// <returns>НОД</returns>
        public static int GCD(int a, int b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }
        /// <summary>
        /// Наименьшее Общее Кратное
        /// </summary>
        /// <param name="a">Число1</param>
        /// <param name="b">Число2</param>
        /// <returns>НОК</returns>
        public static int LCM(int a, int b)
        {
            return Math.Abs(a * b) / GCD(a, b);
        }
        #endregion
        #region Math.доработки
        public static Decimal Sqrt(Decimal x)
        {
            return (Decimal)Math.Sqrt((double)x);
        }
        public static Decimal Exp(Decimal x)
        {
            return (Decimal)Math.Exp((double)x);
        }
        public static Decimal Pow(Decimal x, Decimal pow)
        {
            return (Decimal)Math.Pow((double)x, (double)pow);
        }
        public static Decimal Log(Decimal a)
        {
            return (Decimal)Math.Log((double)a);
        }
        #endregion
        #region Методы оптимизации
        /// <summary>
        /// Одномерная минимизации функции f методом золотого сечения
        /// </summary>
        /// <param name="f">Функция f</param>
        /// <param name="paramsForF">Параметры, которые будут использованы во время расчетов (x+s*l)</param>
        /// <param name="left">Левая граница поиска</param>
        /// <param name="right">Правая граница поиска</param>
        /// <param name="eps">Точность</param>
        /// <returns>Минимум функции от одной переменной на отрезке [left, right]</returns>
        public static Decimal GoldDivision(function1N f, functionN1 paramsForF, Decimal left, Decimal right, Decimal eps)
        {
            Decimal l = ((Decimal)Math.Sqrt((double)5) - 1) / 2;
            Decimal alpha = left + (1 - l) * (right - left), beta = left + l * (right - left);
            int i = 0;
            do
            {
                if (f(paramsForF(alpha)) > f(paramsForF(beta)))
                {
                    left = alpha;
                    alpha = beta;
                    beta = left + l * (right - left);
                    i++;
                }
                else
                {
                    right = beta;
                    beta = alpha;
                    alpha = left + (1 - l) * (right - left);
                    i++;
                }
            } while (right - left > eps);
            return (left + right) / 2;
        }
        /// <summary>
        /// Нахождение минимума многомерной функции методом конфигураций.
        /// </summary>
        /// <param name="f">Функция f</param>
        /// <param name="x">Начальное приближение</param>
        /// <param name="eps">Точность</param>
        /// <param name="step">Шаг</param>        
        /// <param name="divMul">Множитель делителя</param>
        /// <param name="div">Делитель шага, для случая, когда не удается сразу найти точку лучшую чем х</param>
        /// <param name="a">Скорость</param>
        /// <returns>Минимум функции f</returns>
        public static SVector ConfigurationMethod(function1N f, SVector x, Decimal eps, Decimal step = 0.1m, Decimal divMul = 2, Decimal div = 1, Decimal a = 2)
        {
            SVector x3, xPrev = (x + new Decimal[] { 5, 5 }) * 10;
            while (step > eps && (x - xPrev).Norm2 > eps)
            {
                xPrev = x.Clone();
                SVector newX = firstPhase(f, x.Clone(), step, eps, div, divMul);
                x3 = newX + (newX - x) * a;
                if (f(x3) >= f(newX))
                {
                    x = newX;
                    step /= 2;
                }
                else
                    x = x3;
            }
            return x;
        }
        static SVector firstPhase(function1N f, SVector v, Decimal step, Decimal eps, Decimal div = 1, Decimal divMul = 2)
        {
            if (step < eps)
                return v;
            SVector oldV = v.Clone();
            for (int i = 0; i < v.Size; i++)
            {
                SVector temp = v.Clone();
                temp[i] += step;
                if (f(temp) < f(v))
                    v = temp;
                else
                {
                    temp[i] -= 2 * step;
                    if (f(temp) < f(v))
                        v = temp;
                }
            }
            if (oldV == v)
            {
                div *= divMul;
                return firstPhase(f, v, step / div, eps);
            }
            return v;
        }

        #endregion
        #region Численные методы
        #region 3х-точечная прогонка
        /// <summary>
        /// Метод скалярной трехточечной прогонки
        /// </summary>
        /// <param name="A">Вектор, состоящий из элементов, находящихся ниже главное диагонали (значения должны быть сдвинуты на 1 вперед)</param>
        /// <param name="B">Вектор, состоящий из элементов главное диагонали</param>
        /// <param name="C">Вектор, состоящий из элементов, находящихся выше главное диагонали (значения должны быть сдвинуты на 1 назад)</param>
        /// <param name="D">Вектор свободных членов</param>
        /// <param name="FindResult">Необходимо ли подсчитывать результат. Поумолчанию результат не считается.</param>
        /// <returns>Возвращает структуру AlphaBetaResult, хранящую вектора Alpha, Beta и, если необходимо, вектор решений.</returns>
        public static AlphaBetaResult ThreePointSweep(SVector A, SVector B, SVector C, SVector D, funcAB customLastVal, bool FindResult = false, bool customAB = false, Decimal Alpha0 = 0, Decimal Beta0 = 0, bool customLastX = false)
        {
            AlphaBetaResult result = new AlphaBetaResult();
            if (A.Size == C.Size && A.Size + 1 == B.Size)
            {
                A.AddBefore(0);
                C.AddAfter(0);
            }
            if (A.Size != B.Size || B.Size != C.Size || C.Size != D.Size)
                throw new ArgumentException(string.Format("Размерности переданных параметров не подходят. A={0}, B={1}, C={2}, D={3}", A.Size, B.Size, C.Size, D.Size));
            for (int i = 0; i < A.Size; i++)
                if (Math.Abs(A[i]) + Math.Abs(C[i]) > Math.Abs(B[i]))
                    throw new ArgumentException("Нет диагонального преобладания.");

            result.Alpha = new SVector(A.Size);
            result.Beta = new SVector(A.Size);

            result.Alpha[0] = customAB ? Alpha0 : -C[0] / B[0];
            result.Beta[0] = customAB ? Beta0 : D[0] / B[0];

            for (int i = 1; i < A.Size; i++)
            {
                result.Alpha[i] = -C[i] / (A[i] * result.Alpha[i - 1] + B[i]);
                result.Beta[i] = (D[i] - A[i] * result.Beta[i - 1]) / (A[i] * result.Alpha[i - 1] + B[i]);
            }
            if (FindResult)
            {
                SVector x = new SVector(A.Size);
                if (!customLastX)
                    x[A.Size - 1] = (D[A.Size - 1] - A[A.Size - 1] * result.Beta[A.Size - 2]) / (A[A.Size - 1] * result.Alpha[A.Size - 2] + B[A.Size - 1]);
                else
                    x[A.Size - 1] = customLastVal(result.Alpha[A.Size - 2], result.Beta[A.Size - 2]);
                for (int i = A.Size - 2; i >= 0; i--)
                    x[i] = result.Alpha[i] * x[i + 1] + result.Beta[i];
                result.Result = x;
            }
            return result;
        }
        /// <summary>
        /// Упрощенный вызов: передаешь параметры - получаешь решение.
        /// </summary>
        /// <param name="A">Вектор, состоящий из элементов, находящихся ниже главное диагонали (значения должны быть сдвинуты на 1 вперед)</param>
        /// <param name="B">Вектор, состоящий из элементов главное диагонали</param>
        /// <param name="C">Вектор, состоящий из элементов, находящихся выше главное диагонали (значения должны быть сдвинуты на 1 назад)</param>
        /// <param name="D">Вектор свободных членов</param>
        /// <returns>Возвращает структуру AlphaBetaResult, хранящую вектора Alpha, Beta и, если необходимо, вектор решений.</returns>
        public static AlphaBetaResult ThreePointSweep(SVector A, SVector B, SVector C, SVector D)
        {
            return ThreePointSweep(A, B, C, D, null, true);
        }
        #endregion
        #region СЛАУ
        #region Метод Гаусса
        public static SVector Gauss(SMatrix A, SVector b)
        {
            SVector x = new SVector(b.Size);
            for (int i = 0; i < b.Size; i++)
            {
                Decimal prevA = A[i, i];
                for (int j = i; j < b.Size; j++)
                    A[i, j] /= prevA;
                b[i] /= prevA;
                for (int k = i + 1; k < b.Size; k++)
                {
                    Decimal koef = A[k, i];
                    for (int j = 0; j < b.Size; j++)
                        A[k, j] -= koef * A[i, j];
                    b[k] -= koef * b[i];
                }
            }
            for (int i = b.Size - 1; i >= 0; i--)
            {
                decimal r = 0;
                for (int j = i + 1; j < b.Size; j++)
                    r += A[i, j] * x[j];
                x[i] = b[i] - r;
            }
            return x;
        }
        #endregion
        #endregion
        #endregion
    }
    /// <summary>
    /// Структура, хранящая результат работы метода трехточечной прогонки
    /// </summary>
    public struct AlphaBetaResult
    {
        public SVector Alpha, Beta, Result;
        public AlphaBetaResult(SVector Alpha, SVector Beta)
        {
            this.Alpha = Alpha;
            this.Beta = Beta;
            this.Result = new SVector(Alpha.Size);
        }
        public AlphaBetaResult(SVector Alpha, SVector Beta, SVector Result)
        {
            this.Alpha = Alpha;
            this.Beta = Beta;
            this.Result = Result;
        }

    }
    #endregion
}
