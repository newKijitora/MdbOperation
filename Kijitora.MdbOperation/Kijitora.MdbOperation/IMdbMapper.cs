using System.Data;

namespace Kijitora.MdbOperation
{
    /// <summary>
    /// MDBファイルのレコードの書き込みのマッピングを定義します。
    /// </summary>
    public interface IMdbMapper<TSource>
    {
        /// <summary>
        /// テーブルの名前
        /// </summary>
        string TableName { get; set; }

        /// <summary>
        /// レコードに書き込むソースになるオブジェクト
        /// </summary>
        TSource Source { get; set; }

        /// <summary>
        /// レコードへの書き込みを実行する
        /// </summary>
        int WriteAllColumns(DataTable dataRow);

        /// <summary>
        /// レコードからの読み込みを実行する
        /// </summary>
        int ReadAllColumns(DataTable dataRow);
    }
}
