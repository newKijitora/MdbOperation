namespace Kijitora.MdbOperation
{
    /// <summary>
    /// MDBファイルのユーザーです。
    /// </summary>
    public struct MdbUser
    {
        /// <summary>
        /// <see cref="MdbUser"/>構造体の新しいインスタンスを初期化します。
        /// </summary>
        public MdbUser(string userName)
        {
            Name = userName;
        }

        /// <summary>
        /// ユーザー名です。
        /// </summary>
        public string Name { get; set; }
    }
}
