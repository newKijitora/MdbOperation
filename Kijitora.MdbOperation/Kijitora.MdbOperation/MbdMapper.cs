using System;
using System.Data;

namespace Kijitora.MdbOperation
{
    /// <summary>
    /// MDBファイルのレコードの書き込みのマッピングを表します。
    /// </summary>
    public class MdbMapper<TSource> : IMdbMapper<TSource>
    {
        /// <summary>
        /// <see cref="MdbMapper"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        public MdbMapper(TSource source, string tableName, Func<DataTable, int> func)
        {
            if (source == null || string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException();
            }

            Source = source;
            TableName = tableName;
            MapStart = func;
        }

        /// <summary>
        /// <see cref="MdbMapper"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        public MdbMapper(TSource source, string name)
        {
            Source = source;
            TableName = name;
        }

        /// <summary>
        /// テーブルの名前です。
        /// </summary>
        public virtual string TableName { get; set; }

        /// <summary>
        /// レコードに書き込むソースになるオブジェクトです。
        /// </summary>
        public virtual TSource Source { get; set; }

        /// <summary>
        /// 実際に実行される内容のデリゲートです。
        /// </summary>
        protected virtual Func<DataTable, int> MapStart { get; set; }

        /// <summary>
        /// レコードへの書き込みを実行します。
        /// </summary>
        public virtual int WriteAllColumns(DataTable dataRow)
        {
            if (MapStart is null)
            {
                throw new InvalidOperationException();
            }

            return MapStart(dataRow);
        }

        /// <summary>
        /// レコードへの読み込みを実行します。
        /// </summary>
        public virtual int ReadAllColumns(DataTable dataRow)
        {
            if (MapStart is null)
            {
                throw new InvalidOperationException();
            }

            return MapStart(dataRow);
        }
    }
}
