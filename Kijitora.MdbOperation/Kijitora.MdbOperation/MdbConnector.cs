using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace Kijitora.MdbOperation
{
    /// <summary>
    /// MDBファイルへ接続します。
    /// </summary>
    public class MdbConnector
    {
        // 接続文字列
        private string _dataSource;

        private MdbUser _user;
        private MdbPassword _password;
        private string _connectionString;

        // データセット
        private DataSet _dataSet = new DataSet();

        /// <summary>
        /// <see cref="MdbConnector"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        public MdbConnector(string dataSource, MdbUser user, MdbPassword password)
        {
            if (string.IsNullOrEmpty(dataSource) || string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(password.PasswordString))
            {
                throw new ArgumentException("接続文字列を生成することができません。");
            }

            _dataSource = dataSource;
            _user = user;
            _password = password;

            _connectionString = new OleDbConnectionStringBuilder
            {
                { "Provider", "Microsoft.Jet.OLEDB.4.0" },
                { "Data Source", _dataSource },
                { "User ID", _user.Name },
                { "Jet OLEDB:Database Password", _password.PasswordString },
            }
            .ConnectionString;
        }

        // 読み込み処理（マッパーが複数の場合）
        public int ReadAllFields<TSource>(IEnumerable<IMdbMapper<TSource>> mappers)
        {
            if (!CheckFile(_dataSource))
            {
                throw new InvalidOperationException("読み込みしようとしているMDBファイルは、現在読み込みできない状態です。\nファイルが指定の場所に存在しているかどうか、\nまた他のアプリケーションが使用中でないかどうかを確認してください。");
            }

            int selectCount = 0;

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();

                foreach (var mapper in mappers)
                {
                    SelectTable(mapper, connection);
                    selectCount++;
                }
            }

            return selectCount;
        }

        // 読み込み処理（マッパーが単体の場合）
        public int ReadAllFields<TSource>(IMdbMapper<TSource> mapper)
        {
            if (!CheckFile(_dataSource))
            {
                throw new InvalidOperationException("読み込みしようとしているMDBファイルは、現在読み込みできない状態です。\nファイルが指定の場所に存在しているかどうか、\nまた他のアプリケーションが使用中でないかどうかを確認してください。");
            }

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                UpdateTable(mapper, connection);
            }

            return 1;
        }

        private void SelectTable<TSource>(IMdbMapper<TSource> mapper, OleDbConnection connection)
        {
            using (var adapter = new OleDbDataAdapter($"SELECT * FROM {mapper.TableName}", connection))
            using (var commandBuilder = new OleDbCommandBuilder(adapter))
            {
                adapter.Fill(_dataSet, $"{mapper.TableName}");
                mapper.ReadAllColumns(_dataSet.Tables[mapper.TableName]);
            }
        }

        // 更新処理（マッパーが複数の場合）
        public int WriteAllFields<TSource>(IEnumerable<IMdbMapper<TSource>> mappers)
        {
            if (!CheckFile(_dataSource))
            {
                throw new InvalidOperationException("更新しようとしているMDBファイルは、現在書き込みできない状態です。\nファイルが指定の場所に存在しているかどうか、\nまた他のアプリケーションが使用中でないかどうかを確認してください。");
            }

            int updateCount = 0;

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();

                foreach (var mapper in mappers)
                {
                    UpdateTable(mapper, connection);
                    updateCount++;
                }
            }

            return updateCount;
        }

        // 更新処理（マッパーが単体の場合）
        public int WriteAllFields<TSource>(IMdbMapper<TSource> mapper)
        {
            if (!CheckFile(_dataSource))
            {
                throw new InvalidOperationException("更新しようとしているMDBファイルは、現在書き込みできない状態です。\nファイルが指定の場所に存在しているかどうか、\nまた他のアプリケーションが使用中でないかどうかを確認してください。");
            }

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                UpdateTable(mapper, connection);
            }

            return 1;
        }

        /// <summary>
        /// データテーブルを取得します。
        /// </summary>
        public DataTable GetDataTable(string tableName)
        {
            using (var connection = new OleDbConnection(_connectionString))
            using (var adapter = new OleDbDataAdapter($"SELECT * FROM {tableName}", connection))
            using (var commandBuilder = new OleDbCommandBuilder(adapter))
            {
                adapter.Fill(_dataSet, $"{tableName}");
                adapter.UpdateCommand = commandBuilder.GetUpdateCommand();
                adapter.Update(_dataSet, $"{tableName}");
            }

            return _dataSet.Tables[tableName];
        }

        /// <summary>
        /// データテーブルをクリアします。
        /// </summary>
        public void TableClear(DataTable table)
        {
            using (var connection = new OleDbConnection(_connectionString))
            using (var adapter = new OleDbDataAdapter($"SELECT * FROM {table.TableName}", connection))
            using (var commandBuilder = new OleDbCommandBuilder(adapter))
            {
                connection.Open();

                adapter.DeleteCommand = commandBuilder.GetDeleteCommand();

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    table.Rows[i].Delete();
                }

                adapter.Update(table.DataSet, $"{table.TableName}");
            }
        }

        private void UpdateTable<TSource>(IMdbMapper<TSource> mapper, OleDbConnection connection)
        {
            using (var adapter = new OleDbDataAdapter($"SELECT * FROM {mapper.TableName}", connection))
            using (var commandBuilder = new OleDbCommandBuilder(adapter))
            {
                adapter.Fill(_dataSet, $"{mapper.TableName}");
                adapter.UpdateCommand = commandBuilder.GetUpdateCommand();
                mapper.WriteAllColumns(_dataSet.Tables[mapper.TableName]);
                adapter.Update(_dataSet, $"{mapper.TableName}");
            }
        }

        /// <summary>
        /// データセットをクリアします。
        /// </summary>
        public void Flush()
        {
            _dataSet = new DataSet();
        }

        // MDBファイルが使用可能かどうかチェックする
        public static bool CheckFile(string path)
        {
            var fileInfo = new FileInfo(path);

            if (fileInfo.IsReadOnly)
            {
                fileInfo.IsReadOnly = false;
            }

            FileStream stream = null;

            try
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            return true;
        }
    }
}
