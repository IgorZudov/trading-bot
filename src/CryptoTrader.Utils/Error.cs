using System;
using System.Runtime.Serialization;

namespace CryptoTrader.Utils
{
    /// <inheritdoc />
    [DataContract]
    public class Error : IEquatable<Error>
    {
        /// <summary>
        /// Описание ошибки
        /// </summary>
        [DataMember(IsRequired = true, Name = "ErrorDescription")]
        public string Message { get; protected set; }

        /// <summary>
        /// Числовой код ошибки
        /// </summary>
        [DataMember(IsRequired = true, Name = "ErrorCode")]
        public int Code { get; protected set; }

        /// <inheritdoc />
        public Error(string message = "Ошибка", int code = -1)
        {
            Message = message;
            Code = code;
        }

        protected Error()
        {
        }

        /// <inheritdoc />
        public bool Equals(Error other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Code == other.Code;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return ((Error) obj).Code == Code;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Code.GetHashCode();
        }
    }
}
