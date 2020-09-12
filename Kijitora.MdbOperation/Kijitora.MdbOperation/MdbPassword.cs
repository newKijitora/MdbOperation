namespace Kijitora.MdbOperation
{
    /// <summary>
    /// MDBファイルのパスワードです。
    /// </summary>
    public struct MdbPassword
    {
        /// <summary>
        /// <see cref="MdbPassword"/>構造体の新しいインスタンスを初期化します。
        /// </summary>
        public MdbPassword(string password)
        {
            PasswordString = password;
        }

        /// <summary>
        /// パスワード文字列
        /// </summary>
        public string PasswordString { get; set; }
    }
}
